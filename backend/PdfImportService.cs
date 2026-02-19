using System.Text;
using System.Text.RegularExpressions;
using LHubVestibular.Data;
using LHubVestibular.Models;
using Microsoft.EntityFrameworkCore;

namespace LHubVestibular.Services;

// ═══════════════════════════════════════════════════════════════════
//  MODELOS DE RESULTADO
// ═══════════════════════════════════════════════════════════════════

public class ImportResult
{
    public bool   Success          { get; set; }
    public string Mensagem         { get; set; } = "";
    public string FonteUrl         { get; set; } = "";
    public int    QuestoesSalvas   { get; set; }
    public int    QueuesIgnoradas  { get; set; }  // duplicadas
    public int    ErrosParsing     { get; set; }
    public List<QuestaoParseada> Questoes { get; set; } = new();
    public List<string> Erros            { get; set; } = new();
}

public class QuestaoParseada
{
    public int    NumeroQuestao  { get; set; }
    public string Materia        { get; set; } = "";
    public string Enunciado      { get; set; } = "";
    public string AlternativaA   { get; set; } = "";
    public string AlternativaB   { get; set; } = "";
    public string AlternativaC   { get; set; } = "";
    public string AlternativaD   { get; set; } = "";
    public string AlternativaE   { get; set; } = "";
    public string RespostaCorreta { get; set; } = "";
    public string Dificuldade    { get; set; } = "Médio";
    public bool   ParseOk        { get; set; } = true;
    public string? ObsErro       { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
//  MAPEAMENTO DE FONTES CONHECIDAS
// ═══════════════════════════════════════════════════════════════════

public class FonteProva
{
    public string  Url          { get; set; } = "";
    public string  Instituicao  { get; set; } = "";
    public int     Ano          { get; set; }
    public string  Caderno      { get; set; } = "";
    public string  Periodo      { get; set; } = "";
    public TipoProva Tipo       { get; set; }
    // Gabarito opcional (será buscado se informado)
    public string? GabaritoUrl  { get; set; }
}

public enum TipoProva { ENEM_D1, ENEM_D2, FUVEST_1F, UNICAMP_1F }

// ═══════════════════════════════════════════════════════════════════
//  SERVIÇO PRINCIPAL
// ═══════════════════════════════════════════════════════════════════

public class PdfImportService
{
    private readonly AppDbContext  _db;
    private readonly ILogger<PdfImportService> _logger;
    private readonly HttpClient    _http;

    // Mapeamento questão → matéria por intervalo (baseado nos editais oficiais)
    // ENEM Dia 1: Q1-45 = Linguagens, Q46-90 = Humanas
    // ENEM Dia 2: Q91-135 = Ciências da Natureza, Q136-180 = Matemática
    private static readonly Dictionary<TipoProva, List<(int ini, int fim, string materia)>> MapMateria = new()
    {
        [TipoProva.ENEM_D1] = new()
        {
            (1,  5,  "Inglês"),
            (6,  10, "Inglês"),
            (11, 45, "Português"),
            (46, 90, "História"),    // Ciências Humanas inclui História e Geografia
            // refinamento por contexto semântico será feito no parser
        },
        [TipoProva.ENEM_D2] = new()
        {
            (91,  135, "Ciências da Natureza"),
            (136, 180, "Matemática"),
        },
        [TipoProva.FUVEST_1F] = new()
        {
            (1, 90, "Mista"), // FUVEST mistura matérias; classificação por keywords
        },
        [TipoProva.UNICAMP_1F] = new()
        {
            (1, 72, "Mista"),
        },
    };

    // Keywords para classificação semântica de matéria
    private static readonly Dictionary<string, string[]> KeywordsMateria = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Matemática"] = new[] {
            "equação","função","geometria","trigonometria","logaritmo","probabilidade",
            "matriz","vetor","integral","derivada","progressão","estatística","polinômio",
            "inequação","circunferência","parábola","elipse","hipérbole","combinação",
            "permutação","fatorial","binômio","sequência","razão","proporção","porcentagem",
            "juros","área","volume","perímetro","triângulo","quadrado","retângulo","círculo"
        },
        ["Física"] = new[] {
            "velocidade","aceleração","força","energia","potência","trabalho","onda","frequência",
            "resistência","tensão","corrente","campo magnético","campo elétrico","temperatura",
            "pressão","volume","termodinâmica","óptica","refração","reflexão","lei de newton",
            "gravitação","movimento","colisão","impulso","momento","calor","entropia","elétron",
            "fóton","quântica","relatividade","circuito","transformador"
        },
        ["Química"] = new[] {
            "reação","mol","átomo","molécula","elemento","tabela periódica","ácido","base",
            "sal","óxido","ph","oxidação","redução","orgânica","inorgânica","polímero",
            "combustão","estequiometria","solução","concentração","titulação","eletrólise",
            "ligação covalente","ligação iônica","isômero","alqueno","alcano","álcool","aldeído",
            "cetona","éster","ácido carboxílico","amina","amida","carbono","hidrogênio"
        },
        ["Biologia"] = new[] {
            "célula","dna","rna","proteína","enzima","fotossíntese","respiração celular",
            "mitose","meiose","genética","evolução","ecologia","bioma","espécie","organismo",
            "vírus","bactéria","fungo","vegetal","animal","tecido","órgão","sistema",
            "imunidade","hormônio","neurônio","sinapse","herança","mutação","seleção natural",
            "adaptação","parasita","simbiose","cadeia alimentar","nutriente"
        },
        ["História"] = new[] {
            "revolução","guerra","império","colônia","república","democracia","ditadura",
            "feudalismo","capitalismo","socialismo","nazismo","fascismo","independência",
            "escravidão","abolição","iluminismo","renascimento","reforma","contrarreforma",
            "imperialismo","colonialismo","tratado","constituição","presidente","rei","rainha",
            "época","século","período","movimento","revolta","crise","democracia"
        },
        ["Geografia"] = new[] {
            "bioma","clima","relevo","hidrografia","urbanização","população","migração",
            "globalização","geopolítica","fronteira","território","continente","oceano",
            "latitude","longitude","mapa","cartografia","desenvolvimento","sustentável",
            "desertificação","desmatamento","aquecimento global","recursos naturais",
            "indústria","agricultura","agronegócio","exportação","importação"
        },
        ["Português"] = new[] {
            "texto","linguagem","narrador","personagem","enunciado","oração","sujeito",
            "predicado","verbo","substantivo","adjetivo","advérbio","conjunção","preposição",
            "pronome","artigo","numeral","interjeição","coesão","coerência","argumento",
            "gênero textual","dissertação","crônica","poema","conto","romance","figura de linguagem",
            "metáfora","hipérbole","ironia","metonímia","norma culta","concordância","regência"
        },
        ["Inglês"] = new[] {
            "text","read","according","passage","author","meaning","sentence","grammar",
            "tense","verb","noun","adjective","adverb","preposition","conjunction",
            "vocabulary","comprehension","translate","english","language","speaker",
            "past","present","future","perfect","simple","continuous","passive","active"
        },
    };

    // ─────────────────────────────────────────────────────────────
    //  CATÁLOGO DE FONTES CONHECIDAS
    // ─────────────────────────────────────────────────────────────
    public static readonly List<FonteProva> FontesConhecidas = new()
    {
        // ENEM 2024
        new() { Url="https://download.inep.gov.br/enem/provas_e_gabaritos/2024_PV_impresso_D1_CD1.pdf",
                GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2024_GB_impresso_D1.pdf",
                Instituicao="ENEM", Ano=2024, Caderno="Dia 1 – CD1", Periodo="1º Dia", Tipo=TipoProva.ENEM_D1 },
        new() { Url="https://download.inep.gov.br/enem/provas_e_gabaritos/2024_PV_impresso_D2_CD1.pdf",
                GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2024_GB_impresso_D2.pdf",
                Instituicao="ENEM", Ano=2024, Caderno="Dia 2 – CD1", Periodo="2º Dia", Tipo=TipoProva.ENEM_D2 },
        // ENEM 2023
        new() { Url="https://download.inep.gov.br/enem/provas_e_gabaritos/2023_PV_impresso_D1_CD1.pdf",
                GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2023_GB_impresso_D1.pdf",
                Instituicao="ENEM", Ano=2023, Caderno="Dia 1 – CD1", Periodo="1º Dia", Tipo=TipoProva.ENEM_D1 },
        new() { Url="https://download.inep.gov.br/enem/provas_e_gabaritos/2023_PV_impresso_D2_CD5.pdf",
                GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2023_GB_impresso_D2.pdf",
                Instituicao="ENEM", Ano=2023, Caderno="Dia 2 – CD5", Periodo="2º Dia", Tipo=TipoProva.ENEM_D2 },
        // ENEM 2022
        new() { Url="https://download.inep.gov.br/enem/provas_e_gabaritos/2022_PV_impresso_D1_CD1.pdf",
                GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2022_GB_impresso_D1.pdf",
                Instituicao="ENEM", Ano=2022, Caderno="Dia 1 – CD1", Periodo="1º Dia", Tipo=TipoProva.ENEM_D1 },
        new() { Url="https://download.inep.gov.br/enem/provas_e_gabaritos/2022_PV_impresso_D2_CD1.pdf",
                GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2022_GB_impresso_D2.pdf",
                Instituicao="ENEM", Ano=2022, Caderno="Dia 2 – CD1", Periodo="2º Dia", Tipo=TipoProva.ENEM_D2 },
        // FUVEST
        new() { Url="https://www.fuvest.br/wp-content/uploads/fuvest2025_primeira_fase_prova_V1.pdf",
                Instituicao="FUVEST", Ano=2025, Caderno="1ª Fase – V1", Periodo="1ª Fase", Tipo=TipoProva.FUVEST_1F },
        new() { Url="https://www.fuvest.br/wp-content/uploads/fuvest2024_primeira_fase_prova_V.pdf",
                Instituicao="FUVEST", Ano=2024, Caderno="1ª Fase", Periodo="1ª Fase", Tipo=TipoProva.FUVEST_1F },
        new() { Url="https://www.fuvest.br/wp-content/uploads/fuvest2023_primeira_fase_prova_V.pdf",
                Instituicao="FUVEST", Ano=2023, Caderno="1ª Fase", Periodo="1ª Fase", Tipo=TipoProva.FUVEST_1F },
        // UNICAMP
        new() { Url="https://www.curso-objetivo.br/vestibular/resolucao-comentada/unicamp/2025_1fase/unicamp2025_1fase_prova_QZ.pdf",
                Instituicao="UNICAMP", Ano=2025, Caderno="1ª Fase – QZ", Periodo="1ª Fase", Tipo=TipoProva.UNICAMP_1F },
        new() { Url="https://www.curso-objetivo.br/vestibular/resolucao-comentada/unicamp/2024_1fase/unicamp2024_1fase_prova_QY.pdf",
                Instituicao="UNICAMP", Ano=2024, Caderno="1ª Fase – QY", Periodo="1ª Fase", Tipo=TipoProva.UNICAMP_1F },
    };

    // ─────────────────────────────────────────────────────────────
    //  CONSTRUTOR
    // ─────────────────────────────────────────────────────────────
    public PdfImportService(AppDbContext db, ILogger<PdfImportService> logger, IHttpClientFactory httpClientFactory)
    {
        _db     = db;
        _logger = logger;
        _http   = httpClientFactory.CreateClient("PdfImport");
    }

    // ═══════════════════════════════════════════════════════════════
    //  MÉTODO PRINCIPAL: ImportarAsync
    //  Recebe uma URL de PDF, baixa, extrai texto, parseia questões
    //  e salva no banco de dados.
    // ═══════════════════════════════════════════════════════════════
    public async Task<ImportResult> ImportarAsync(string url, string? gabaritoUrl = null)
    {
        var result = new ImportResult { FonteUrl = url };

        // 1. Identificar fonte no catálogo
        var fonte = FontesConhecidas.FirstOrDefault(f =>
            string.Equals(f.Url, url, StringComparison.OrdinalIgnoreCase));

        if (fonte == null)
        {
            // Tentar inferir pelos padrões da URL
            fonte = InferirFonte(url);
        }

        _logger.LogInformation("Iniciando importação: {Url} [{Inst} {Ano}]",
            url, fonte?.Instituicao ?? "?", fonte?.Ano ?? 0);

        // 2. Baixar PDF
        byte[] pdfBytes;
        try
        {
            pdfBytes = await BaixarPdfAsync(url);
            _logger.LogInformation("PDF baixado: {Size} bytes", pdfBytes.Length);
        }
        catch (Exception ex)
        {
            result.Success  = false;
            result.Mensagem = $"Erro ao baixar PDF: {ex.Message}";
            result.Erros.Add(result.Mensagem);
            return result;
        }

        // 3. Extrair texto do PDF usando UglyToad.PdfPig
        string textoCompleto;
        try
        {
            textoCompleto = ExtrairTextoPdf(pdfBytes);
            _logger.LogInformation("Texto extraído: {Chars} caracteres", textoCompleto.Length);
        }
        catch (Exception ex)
        {
            result.Success  = false;
            result.Mensagem = $"Erro ao extrair texto do PDF: {ex.Message}";
            result.Erros.Add(result.Mensagem);
            return result;
        }

        // 4. Baixar gabarito (se fornecido)
        Dictionary<int, string> gabarito = new();
        var gabUrl = gabaritoUrl ?? fonte?.GabaritoUrl;
        if (!string.IsNullOrEmpty(gabUrl))
        {
            try
            {
                var gabBytes = await BaixarPdfAsync(gabUrl);
                var gabTexto = ExtrairTextoPdf(gabBytes);
                gabarito = ParsearGabarito(gabTexto, fonte?.Tipo ?? TipoProva.ENEM_D1);
                _logger.LogInformation("Gabarito carregado: {N} respostas", gabarito.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Não foi possível carregar gabarito: {Msg}", ex.Message);
                result.Erros.Add($"Aviso: gabarito não carregado — {ex.Message}");
            }
        }

        // 5. Parsear questões do texto
        var questoesParseadas = ParsearQuestoes(textoCompleto, fonte);
        _logger.LogInformation("Questões parseadas: {N}", questoesParseadas.Count);

        // 6. Aplicar gabarito às questões
        if (gabarito.Count > 0)
        {
            foreach (var q in questoesParseadas)
            {
                if (gabarito.TryGetValue(q.NumeroQuestao, out var resp))
                    q.RespostaCorreta = resp;
            }
        }

        // 7. Classificar matéria (se não definida pelo intervalo)
        foreach (var q in questoesParseadas)
        {
            if (string.IsNullOrEmpty(q.Materia) || q.Materia == "Mista")
                q.Materia = ClassificarMateria(q.Enunciado + " " + q.AlternativaA + " " + q.AlternativaB);
        }

        result.Questoes = questoesParseadas;

        // 8. Salvar no banco de dados
        try
        {
            var (salvas, ignoradas, erros) = await SalvarNoBancoAsync(questoesParseadas, fonte, url);
            result.QuestoesSalvas  = salvas;
            result.QueuesIgnoradas = ignoradas;
            result.ErrosParsing    = erros;
            result.Success         = true;
            result.Mensagem = $"Importação concluída: {salvas} salvas, {ignoradas} duplicadas, {erros} com erro de parse.";
        }
        catch (Exception ex)
        {
            result.Success  = false;
            result.Mensagem = $"Erro ao salvar no banco: {ex.Message}";
            result.Erros.Add(ex.ToString());
        }

        return result;
    }

    // ═══════════════════════════════════════════════════════════════
    //  BAIXAR PDF
    // ═══════════════════════════════════════════════════════════════
    private async Task<byte[]> BaixarPdfAsync(string url)
    {
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    //  EXTRAIR TEXTO DO PDF (UglyToad.PdfPig)
    // ═══════════════════════════════════════════════════════════════
    private static string ExtrairTextoPdf(byte[] pdfBytes)
    {
        var sb = new StringBuilder();
        using var doc = UglyToad.PdfPig.PdfDocument.Open(pdfBytes);
        foreach (var page in doc.GetPages())
        {
            // Ordenar palavras por posição (top→bottom, left→right)
            var words = page.GetWords()
                .OrderByDescending(w => w.BoundingBox.Top)
                .ThenBy(w => w.BoundingBox.Left)
                .ToList();

            double lastTop = double.MaxValue;
            foreach (var word in words)
            {
                // Nova linha se diferença vertical > 4 pontos
                if (Math.Abs(word.BoundingBox.Top - lastTop) > 4)
                    sb.AppendLine();
                else
                    sb.Append(' ');

                sb.Append(word.Text);
                lastTop = word.BoundingBox.Top;
            }
            sb.AppendLine("\n--- PÁGINA ---");
        }
        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════
    //  PARSEAR GABARITO
    //  Padrão INEP: tabela com colunas QUESTÃO / GABARITO
    // ═══════════════════════════════════════════════════════════════
    private static Dictionary<int, string> ParsearGabarito(string texto, TipoProva tipo)
    {
        var result = new Dictionary<int, string>();

        // Padrão: "136 A" ou "136A" ou "136. A"
        var pattern = new Regex(@"\b(\d{1,3})\s*[.:\-]?\s*([ABCDE])\b",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        foreach (Match m in pattern.Matches(texto))
        {
            if (int.TryParse(m.Groups[1].Value, out int num))
                result[num] = m.Groups[2].Value.ToUpper();
        }

        return result;
    }

    // ═══════════════════════════════════════════════════════════════
    //  PARSEAR QUESTÕES (Algoritmo principal)
    // ═══════════════════════════════════════════════════════════════
    private static List<QuestaoParseada> ParsearQuestoes(string texto, FonteProva? fonte)
    {
        var questoes = new List<QuestaoParseada>();
        var tipo = fonte?.Tipo ?? TipoProva.FUVEST_1F;

        // ── Estratégia 1: Regex de questão numerada ──────────────
        // Padrão: "QUESTÃO 01", "Questão 1", "01.", "1)"
        // com alternativas "A)" "a)" "(A)" "(a)"

        var rQuestao = new Regex(
            @"(?:QUEST[ÃA]O\s+|Q\.\s*)(\d{1,3})\s*\n([\s\S]+?)(?=(?:QUEST[ÃA]O\s+\d|Q\.\s*\d|\n\s*\d{1,3}\s*[.)\s]|$))",
            RegexOptions.IgnoreCase | RegexOptions.Multiline);

        // Se não houver matches com padrão QUESTÃO XX, tentar padrão numérico simples
        var matches = rQuestao.Matches(texto);

        if (matches.Count < 3)
        {
            // Padrão alternativo: número seguido de enunciado e alternativas A-E
            rQuestao = new Regex(
                @"^\s*(\d{1,3})\s*[.)]\s*([\s\S]+?)(?=^\s*\d{1,3}\s*[.)]|\z)",
                RegexOptions.Multiline);
            matches = rQuestao.Matches(texto);
        }

        foreach (Match m in matches)
        {
            if (!int.TryParse(m.Groups[1].Value.Trim(), out int num)) continue;
            if (num < 1 || num > 200) continue;

            var corpo = m.Groups[2].Value;
            var q     = ParsearCorpoQuestao(corpo, num, tipo, fonte);
            if (q != null) questoes.Add(q);
        }

        // ── Se poucos resultados, tentar separação por alternativas ──
        if (questoes.Count < 5)
            questoes = ParsearPorAlternativas(texto, tipo, fonte);

        // ── Enriquecer com matéria por intervalo ──────────────────
        if (fonte != null && MapMateria.TryGetValue(tipo, out var intervalos))
        {
            foreach (var q in questoes)
            {
                var intervalo = intervalos.FirstOrDefault(i =>
                    q.NumeroQuestao >= i.ini && q.NumeroQuestao <= i.fim);
                if (intervalo != default && intervalo.materia != "Mista")
                    q.Materia = intervalo.materia;
            }
        }

        return questoes
            .OrderBy(q => q.NumeroQuestao)
            .GroupBy(q => q.NumeroQuestao)
            .Select(g => g.First())  // deduplicar
            .ToList();
    }

    // ─────────────────────────────────────────────────────────────
    //  Parsear corpo de uma questão (enunciado + alternativas)
    // ─────────────────────────────────────────────────────────────
    private static QuestaoParseada? ParsearCorpoQuestao(string corpo, int num, TipoProva tipo, FonteProva? fonte)
    {
        // Regex para alternativas: "(A)", "A)", "A." no início de linha
        var rAlt = new Regex(
            @"(?:^|\n)\s*[(\[]?\s*([AaBbCcDdEe])\s*[)\].:-]\s*(.+?)(?=(?:\n\s*[(\[]?\s*[AaBbCcDdEe]\s*[)\].:-])|$)",
            RegexOptions.Singleline | RegexOptions.Multiline);

        var altMatches = rAlt.Matches(corpo);
        if (altMatches.Count < 2) return null; // sem alternativas = não é questão

        var primeiraAlt = altMatches[0].Index;
        var enunciado   = corpo[..primeiraAlt].Trim();
        enunciado = LimparTexto(enunciado);

        if (enunciado.Length < 10) return null;

        var alts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match am in altMatches)
        {
            var letra = am.Groups[1].Value.ToUpper();
            var texto = LimparTexto(am.Groups[2].Value.Trim());
            if (!alts.ContainsKey(letra) && texto.Length > 0)
                alts[letra] = texto;
        }

        if (alts.Count < 2) return null;

        return new QuestaoParseada
        {
            NumeroQuestao  = num,
            Enunciado      = enunciado,
            AlternativaA   = alts.GetValueOrDefault("A", ""),
            AlternativaB   = alts.GetValueOrDefault("B", ""),
            AlternativaC   = alts.GetValueOrDefault("C", ""),
            AlternativaD   = alts.GetValueOrDefault("D", ""),
            AlternativaE   = alts.GetValueOrDefault("E", ""),
            Dificuldade    = InferirDificuldade(num, tipo),
            ParseOk        = true,
        };
    }

    // ─────────────────────────────────────────────────────────────
    //  Parser alternativo: varre o texto buscando padrões A) B) C)
    // ─────────────────────────────────────────────────────────────
    private static List<QuestaoParseada> ParsearPorAlternativas(string texto, TipoProva tipo, FonteProva? fonte)
    {
        var questoes = new List<QuestaoParseada>();
        var linhas   = texto.Split('\n');

        QuestaoParseada? atual = null;
        string altAtual = "";
        int    numQ     = 0;

        var rNumQ = new Regex(@"^\s*(\d{1,3})\s*[.)]\s+\S");
        var rAlt  = new Regex(@"^\s*([AaBbCcDdEe])\s*[).\-]\s+(.+)");

        foreach (var linha in linhas)
        {
            var mNum = rNumQ.Match(linha);
            if (mNum.Success && int.TryParse(mNum.Groups[1].Value, out int n) && n >= 1 && n <= 200)
            {
                // Salvar questão anterior
                if (atual != null && atual.Enunciado.Length > 15)
                    questoes.Add(atual);

                numQ  = n;
                atual = new QuestaoParseada
                {
                    NumeroQuestao = numQ,
                    Enunciado     = LimparTexto(linha[mNum.Length..]),
                    Dificuldade   = InferirDificuldade(numQ, tipo),
                };
                altAtual = "";
                continue;
            }

            if (atual != null)
            {
                var mAlt = rAlt.Match(linha);
                if (mAlt.Success)
                {
                    altAtual = mAlt.Groups[1].Value.ToUpper();
                    var val  = LimparTexto(mAlt.Groups[2].Value.Trim());
                    switch (altAtual)
                    {
                        case "A": atual.AlternativaA = val; break;
                        case "B": atual.AlternativaB = val; break;
                        case "C": atual.AlternativaC = val; break;
                        case "D": atual.AlternativaD = val; break;
                        case "E": atual.AlternativaE = val; break;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(altAtual) && !string.IsNullOrWhiteSpace(linha.Trim()))
                {
                    // Continuação da alternativa anterior
                    switch (altAtual)
                    {
                        case "A": atual.AlternativaA += " " + linha.Trim(); break;
                        case "B": atual.AlternativaB += " " + linha.Trim(); break;
                        case "C": atual.AlternativaC += " " + linha.Trim(); break;
                        case "D": atual.AlternativaD += " " + linha.Trim(); break;
                        case "E": atual.AlternativaE += " " + linha.Trim(); break;
                        default:  atual.Enunciado    += " " + linha.Trim(); break;
                    }
                }
                else if (string.IsNullOrEmpty(altAtual) && !string.IsNullOrWhiteSpace(linha.Trim()))
                {
                    atual.Enunciado += "\n" + linha.Trim();
                }
            }
        }
        if (atual != null && atual.Enunciado.Length > 15)
            questoes.Add(atual);

        return questoes;
    }

    // ═══════════════════════════════════════════════════════════════
    //  CLASSIFICAR MATÉRIA POR KEYWORDS
    // ═══════════════════════════════════════════════════════════════
    public static string ClassificarMateria(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "Geral";

        var textoLower = texto.ToLower();
        var scores     = new Dictionary<string, int>();

        foreach (var (materia, keywords) in KeywordsMateria)
        {
            int score = keywords.Count(kw => textoLower.Contains(kw.ToLower()));
            if (score > 0) scores[materia] = score;
        }

        if (scores.Count == 0) return "Geral";

        return scores.OrderByDescending(kv => kv.Value).First().Key;
    }

    // ═══════════════════════════════════════════════════════════════
    //  INFERIR DIFICULDADE
    // ═══════════════════════════════════════════════════════════════
    private static string InferirDificuldade(int numQ, TipoProva tipo)
    {
        return tipo switch
        {
            TipoProva.ENEM_D1 or TipoProva.ENEM_D2 =>
                numQ % 3 == 0 ? "Difícil" : numQ % 3 == 1 ? "Fácil" : "Médio",
            TipoProva.FUVEST_1F  => numQ <= 20 ? "Fácil" : numQ <= 55 ? "Médio" : "Difícil",
            TipoProva.UNICAMP_1F => numQ <= 18 ? "Fácil" : numQ <= 48 ? "Médio" : "Difícil",
            _ => "Médio"
        };
    }

    // ═══════════════════════════════════════════════════════════════
    //  SALVAR NO BANCO DE DADOS
    // ═══════════════════════════════════════════════════════════════
    private async Task<(int salvas, int ignoradas, int erros)> SalvarNoBancoAsync(
        List<QuestaoParseada> questoesParseadas, FonteProva? fonte, string url)
    {
        int salvas = 0, ignoradas = 0, erros = 0;

        if (fonte == null) return (0, 0, questoesParseadas.Count);

        // Criar ou recuperar entrada de Prova
        var prova = await _db.Provas.FirstOrDefaultAsync(p =>
            p.Instituicao == fonte.Instituicao &&
            p.Ano         == fonte.Ano         &&
            p.Periodo     == fonte.Periodo);

        if (prova == null)
        {
            prova = new Prova
            {
                Titulo       = $"{fonte.Instituicao} {fonte.Ano} — {fonte.Periodo}",
                Ano          = fonte.Ano,
                Periodo      = fonte.Periodo,
                Instituicao  = fonte.Instituicao,
                ProvaUrl     = url,
                GabaritoUrl  = fonte.GabaritoUrl,
                Questoes     = questoesParseadas.Count,
                DuracaoHoras = fonte.Tipo is TipoProva.ENEM_D1 or TipoProva.ENEM_D2 ? 5 : 4,
                Fonte        = fonte.Instituicao,
                FonteNome    = fonte.Instituicao switch {
                    "ENEM"    => "INEP/MEC (inep.gov.br)",
                    "FUVEST"  => "FUVEST (fuvest.br)",
                    "UNICAMP" => "COMVEST (comvest.unicamp.br)",
                    _         => fonte.Instituicao
                }
            };
            _db.Provas.Add(prova);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Prova criada: ID={Id}", prova.Id);
        }

        // Questões já existentes nesta prova (evitar duplicatas)
        var numerosExistentes = (await _db.Questoes
            .Where(q => q.ProvaId == prova.Id)
            .Select(q => q.NumeroQuestao)
            .ToListAsync()).ToHashSet();

        var novas = new List<Questao>();

        foreach (var qp in questoesParseadas)
        {
            if (!qp.ParseOk || qp.Enunciado.Length < 10)
            {
                erros++;
                continue;
            }

            if (numerosExistentes.Contains(qp.NumeroQuestao))
            {
                ignoradas++;
                continue;
            }

            if (string.IsNullOrEmpty(qp.AlternativaA) || string.IsNullOrEmpty(qp.AlternativaB))
            {
                erros++;
                _logger.LogWarning("Q{N}: sem alternativas suficientes", qp.NumeroQuestao);
                continue;
            }

            novas.Add(new Questao
            {
                ProvaId        = prova.Id,
                NumeroQuestao  = qp.NumeroQuestao,
                Materia        = string.IsNullOrEmpty(qp.Materia) ? "Geral" : qp.Materia,
                Enunciado      = qp.Enunciado.Trim(),
                AlternativaA   = qp.AlternativaA.Trim(),
                AlternativaB   = qp.AlternativaB.Trim(),
                AlternativaC   = qp.AlternativaC.Trim(),
                AlternativaD   = qp.AlternativaD.Trim(),
                AlternativaE   = qp.AlternativaE.Trim(),
                RespostaCorreta= string.IsNullOrEmpty(qp.RespostaCorreta) ? "A" : qp.RespostaCorreta,
                Dificuldade    = qp.Dificuldade,
                Ano            = fonte.Ano,
                Instituicao    = fonte.Instituicao,
            });
        }

        if (novas.Count > 0)
        {
            _db.Questoes.AddRange(novas);
            await _db.SaveChangesAsync();
            salvas = novas.Count;

            // Atualizar contagem de questões da prova
            prova.Questoes = await _db.Questoes.CountAsync(q => q.ProvaId == prova.Id);
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("Salvas: {S}  Ignoradas: {I}  Erros: {E}", salvas, ignoradas, erros);
        return (salvas, ignoradas, erros);
    }

    // ═══════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════
    private static string LimparTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        // Remover múltiplos espaços e quebras de linha redundantes
        texto = Regex.Replace(texto, @"\r\n|\r", "\n");
        texto = Regex.Replace(texto, @"\n{3,}", "\n\n");
        texto = Regex.Replace(texto, @"[ \t]{2,}", " ");
        texto = texto.Trim();
        // Remover cabeçalhos de página comuns
        texto = Regex.Replace(texto, @"(ENEM|FUVEST|UNICAMP)\s*\d{4}.*?\n", "", RegexOptions.IgnoreCase);
        return texto;
    }

    private static FonteProva InferirFonte(string url)
    {
        var ano  = 2024;
        var inst = "Desconhecido";
        var tipo = TipoProva.ENEM_D1;

        var mAno = Regex.Match(url, @"(20\d{2})");
        if (mAno.Success) int.TryParse(mAno.Groups[1].Value, out ano);

        if (url.Contains("inep.gov.br"))    { inst = "ENEM";    tipo = url.Contains("D2") ? TipoProva.ENEM_D2 : TipoProva.ENEM_D1; }
        if (url.Contains("fuvest.br"))      { inst = "FUVEST";  tipo = TipoProva.FUVEST_1F; }
        if (url.Contains("unicamp"))        { inst = "UNICAMP"; tipo = TipoProva.UNICAMP_1F; }
        if (url.Contains("objetivo.br"))    { inst = "UNICAMP"; tipo = TipoProva.UNICAMP_1F; }

        return new FonteProva { Url = url, Instituicao = inst, Ano = ano, Tipo = tipo, Caderno = "", Periodo = "1ª Fase" };
    }

    // ═══════════════════════════════════════════════════════════════
    //  IMPORTAR TODAS AS FONTES CONHECIDAS DE UMA VEZ
    // ═══════════════════════════════════════════════════════════════
    public async Task<List<ImportResult>> ImportarTodasAsync(IProgress<string>? progresso = null)
    {
        var resultados = new List<ImportResult>();
        foreach (var fonte in FontesConhecidas)
        {
            progresso?.Report($"Importando {fonte.Instituicao} {fonte.Ano} {fonte.Caderno}...");
            var r = await ImportarAsync(fonte.Url, fonte.GabaritoUrl);
            resultados.Add(r);
            _logger.LogInformation("{Inst} {Ano}: {Msg}", fonte.Instituicao, fonte.Ano, r.Mensagem);
            await Task.Delay(500); // evitar rate limiting
        }
        return resultados;
    }
}
