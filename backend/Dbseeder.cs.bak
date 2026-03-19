using LHubVestibular.Models;
using Microsoft.EntityFrameworkCore;

namespace LHubVestibular.Data;

/// <summary>
/// Popula o banco com dados iniciais: provas, questões reais de vestibular,
/// notícias, cronograma e locais de prova.
/// Equivale ao popular_banco.py + inserts do schema.sql
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Só popula se não há dados
        if (await db.Provas.AnyAsync()) return;

        // ── CRONOGRAMA ─────────────────────────────────────────
        db.Cronograma.AddRange(
            new Cronograma { Titulo="Inscrições",          Descricao="Período de inscrições para o vestibular",      DataInicio=new DateOnly(2026,2,1),  DataFim=new DateOnly(2026,3,15), Ordem=1 },
            new Cronograma { Titulo="Pagamento",           Descricao="Prazo final para pagamento da taxa de inscrição",DataInicio=new DateOnly(2026,2,1),  DataFim=new DateOnly(2026,3,18), Ordem=2 },
            new Cronograma { Titulo="Divulgação dos Locais",Descricao="Divulgação dos locais e salas de prova",      DataInicio=new DateOnly(2026,5,20), DataFim=new DateOnly(2026,5,20), Ordem=3 },
            new Cronograma { Titulo="Prova",               Descricao="Realização da prova do vestibular",            DataInicio=new DateOnly(2026,6,15), DataFim=new DateOnly(2026,6,15), Ordem=4 },
            new Cronograma { Titulo="Resultado",           Descricao="Divulgação do resultado final",               DataInicio=new DateOnly(2026,6,30), DataFim=new DateOnly(2026,6,30), Ordem=5 },
            new Cronograma { Titulo="Matrícula",           Descricao="Período de matrícula dos aprovados",          DataInicio=new DateOnly(2026,7,1),  DataFim=new DateOnly(2026,7,15), Ordem=6 }
        );

        // ── NOTÍCIAS E EDITAIS ─────────────────────────────────
        db.Noticias.AddRange(
            new Noticia {
                Titulo="Inscrições Abertas para o Vestibular 2026",
                Slug="inscricoes-abertas-vestibular-2026",
                Resumo="As inscrições para o processo seletivo 2026 estão abertas até 15 de março. Confira os cursos disponíveis e se inscreva!",
                Corpo="<p>O <strong>Vestibular L-Hub 2026</strong> está com inscrições abertas! O período vai de <strong>01 de fevereiro</strong> até <strong>15 de março de 2026</strong>.</p><p>Cursos disponíveis: Ciência da Computação, Engenharia Civil, Direito, Medicina, Administração e Psicologia.</p><p>Taxa de inscrição: <strong>R$ 85,00</strong>, vencimento 18/03/2026.</p>",
                Categoria="noticia", BadgeTipo="novo", Destaque=true, PublicadoEm=new DateTime(2026,2,10,8,0,0)
            },
            new Noticia {
                Titulo="Edital Completo do Processo Seletivo 2026",
                Slug="edital-completo-processo-seletivo-2026",
                Resumo="Confira todas as regras, critérios de avaliação e informações sobre o vestibular 2026.",
                Corpo="<p>A Comissão do Vestibular divulga o <strong>Edital do Processo Seletivo 2026</strong>.</p><h3>Inscrições:</h3><p>01/02/2026 a 15/03/2026, exclusivamente online.</p><h3>Prova:</h3><p>15/06/2026, das 09h às 13h, 60 questões objetivas.</p><h3>Resultado:</h3><p>Divulgação em 30/06/2026.</p>",
                Categoria="edital", BadgeTipo="edital", Destaque=true, PublicadoEm=new DateTime(2026,2,8,10,0,0)
            },
            new Noticia {
                Titulo="Prazo para Pagamento Encerra em 18/03",
                Slug="prazo-pagamento-encerra-18-marco",
                Resumo="Atenção! O boleto da taxa de inscrição vence em 18/03/2026.",
                Corpo="<p><strong>Atenção, candidatos!</strong> O prazo final para pagamento é <strong>18 de março de 2026</strong>. Acesse a Área do Candidato para obter o código do boleto.</p>",
                Categoria="comunicado", BadgeTipo="urgente", Destaque=false, PublicadoEm=new DateTime(2026,2,12,9,0,0)
            },
            new Noticia {
                Titulo="Divulgação dos Locais de Prova em Maio",
                Slug="divulgacao-locais-prova-maio",
                Resumo="Os locais e salas de prova serão divulgados a partir de 20 de maio de 2026.",
                Corpo="<p>A divulgação dos locais de prova será em <strong>20 de maio de 2026</strong>. Cada candidato poderá consultar seu local na Área do Candidato.</p>",
                Categoria="noticia", BadgeTipo="importante", Destaque=false, PublicadoEm=new DateTime(2026,2,13,11,0,0)
            }
        );

        // ── LOCAIS DE PROVA ────────────────────────────────────
        db.LocaisProva.AddRange(
            new LocalProva { Nome="L-Hub Campus Central", Endereco="Av. Paulista, 1000",  Bairro="Bela Vista",  Cidade="São Paulo", Estado="SP", Capacidade=500 },
            new LocalProva { Nome="L-Hub Campus Norte",   Endereco="Rua das Flores, 250", Bairro="Santana",     Cidade="São Paulo", Estado="SP", Capacidade=300 },
            new LocalProva { Nome="L-Hub Campus Sul",     Endereco="Av. Interlagos, 800", Bairro="Santo Amaro", Cidade="São Paulo", Estado="SP", Capacidade=300 },
            new LocalProva { Nome="Colégio Parceiro Centro",Endereco="Rua da Consolação, 150",Bairro="Consolação",Cidade="São Paulo",Estado="SP", Capacidade=200 }
        );

        // ── PROVAS ─────────────────────────────────────────────
        var provas = new List<Prova>
        {
            new() { Titulo="FUVEST 2025 — Primeira Fase", Ano=2025, Periodo="1ª Fase", Instituicao="FUVEST",   Area="geral",             Questoes=90, DuracaoHoras=5, ProvaUrl="https://www.fuvest.br/wp-content/uploads/fuvest-2025-1a-fase-prova.pdf",   GabaritoUrl="https://www.fuvest.br/wp-content/uploads/fuvest-2025-1a-fase-gabarito.pdf",   Fonte="externo", FonteNome="FUVEST",   Downloads=1240 },
            new() { Titulo="FUVEST 2024 — Primeira Fase", Ano=2024, Periodo="1ª Fase", Instituicao="FUVEST",   Area="geral",             Questoes=90, DuracaoHoras=5, ProvaUrl="https://www.fuvest.br/wp-content/uploads/fuvest-2024-1a-fase-prova.pdf",   GabaritoUrl="https://www.fuvest.br/wp-content/uploads/fuvest-2024-1a-fase-gabarito.pdf",   Fonte="externo", FonteNome="FUVEST",   Downloads=3850 },
            new() { Titulo="FUVEST 2023 — Primeira Fase", Ano=2023, Periodo="1ª Fase", Instituicao="FUVEST",   Area="geral",             Questoes=90, DuracaoHoras=5, ProvaUrl="https://www.fuvest.br/wp-content/uploads/fuvest_2023_1a_fase_prova.pdf",   GabaritoUrl="https://www.fuvest.br/wp-content/uploads/fuvest_2023_1a_fase_gabarito.pdf",   Fonte="externo", FonteNome="FUVEST",   Downloads=5100 },
            new() { Titulo="UNICAMP 2025 — Primeira Fase", Ano=2025, Periodo="1ª Fase",Instituicao="UNICAMP",  Area="geral",             Questoes=72, DuracaoHoras=4, ProvaUrl="https://www.comvest.unicamp.br/wp-content/uploads/2024/11/1fase2025_prova.pdf",GabaritoUrl="https://www.comvest.unicamp.br/wp-content/uploads/2024/11/1fase2025_gabarito.pdf",Fonte="externo",FonteNome="COMVEST",Downloads=980 },
            new() { Titulo="UNICAMP 2024 — Primeira Fase", Ano=2024, Periodo="1ª Fase",Instituicao="UNICAMP",  Area="geral",             Questoes=72, DuracaoHoras=4, ProvaUrl="https://www.comvest.unicamp.br/wp-content/uploads/2023/11/1fase_2024_prova.pdf",GabaritoUrl="https://www.comvest.unicamp.br/wp-content/uploads/2023/11/1fase_2024_gabarito.pdf",Fonte="externo",FonteNome="COMVEST",Downloads=2700 },
            new() { Titulo="ENEM 2024 — Caderno Azul (Dia 1)", Ano=2024, Periodo="1º Dia", Instituicao="ENEM/MEC", Area="humanas_linguagens",Questoes=90, DuracaoHoras=5, ProvaUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2024_PV_impresso_D1_CD1.pdf",GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2024_GB_impresso_D1.pdf",Fonte="externo",FonteNome="INEP",Downloads=8200 },
            new() { Titulo="ENEM 2024 — Caderno Azul (Dia 2)", Ano=2024, Periodo="2º Dia", Instituicao="ENEM/MEC", Area="exatas_natureza",   Questoes=90, DuracaoHoras=5, ProvaUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2024_PV_impresso_D2_CD1.pdf",GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2024_GB_impresso_D2.pdf",Fonte="externo",FonteNome="INEP",Downloads=7900 },
            new() { Titulo="ENEM 2023 — Caderno Azul (Dia 1)", Ano=2023, Periodo="1º Dia", Instituicao="ENEM/MEC", Area="humanas_linguagens",Questoes=90, DuracaoHoras=5, ProvaUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2023_PV_impresso_D1_CD1.pdf",GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2023_GB_impresso_D1.pdf",Fonte="externo",FonteNome="INEP",Downloads=12300 },
            new() { Titulo="ENEM 2023 — Caderno Azul (Dia 2)", Ano=2023, Periodo="2º Dia", Instituicao="ENEM/MEC", Area="exatas_natureza",   Questoes=90, DuracaoHoras=5, ProvaUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2023_PV_impresso_D2_CD1.pdf",GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2023_GB_impresso_D2.pdf",Fonte="externo",FonteNome="INEP",Downloads=11800 },
            new() { Titulo="ENEM 2022 — Caderno Azul (Dia 1)", Ano=2022, Periodo="1º Dia", Instituicao="ENEM/MEC", Area="humanas_linguagens",Questoes=90, DuracaoHoras=5, ProvaUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2022_PV_impresso_D1_CD1.pdf",GabaritoUrl="https://download.inep.gov.br/enem/provas_e_gabaritos/2022_GB_impresso_D1.pdf",Fonte="externo",FonteNome="INEP",Downloads=15600 },
        };
        db.Provas.AddRange(provas);
        await db.SaveChangesAsync();

        // ── QUESTÕES REAIS DE VESTIBULAR ───────────────────────
        // Equivale ao QUESTOES list do popular_banco.py
        var provaFuvest24 = await db.Provas.FirstAsync(p => p.Ano == 2024 && p.Instituicao == "FUVEST");
        var provaFuvest23 = await db.Provas.FirstAsync(p => p.Ano == 2023 && p.Instituicao == "FUVEST");
        var provaEnem24   = await db.Provas.FirstAsync(p => p.Ano == 2024 && p.Instituicao == "ENEM/MEC" && p.Area == "exatas_natureza");
        var provaEnem23   = await db.Provas.FirstAsync(p => p.Ano == 2023 && p.Instituicao == "ENEM/MEC" && p.Area == "exatas_natureza");
        var provaUnicamp23= await db.Provas.FirstAsync(p => p.Ano == 2024 && p.Instituicao == "UNICAMP");

        var questoes = new List<Questao>
        {
            // ── MATEMÁTICA ──────────────────────────────────────
            new() { ProvaId=provaFuvest24.Id, NumeroQuestao=1,  Materia="Matemática", Ano=2024, Instituicao="FUVEST", Dificuldade="Médio",
                Enunciado="Vinte times de futebol disputam a Série A do Campeonato Brasileiro, sendo seis deles paulistas. Cada time joga duas vezes contra cada um dos seus adversários. A porcentagem de jogos nos quais os dois oponentes são paulistas é:",
                AlternativaA="7,5%", AlternativaB="7,9%", AlternativaC="10,0%", AlternativaD="12,0%", AlternativaE="15,0%", RespostaCorreta="B" },

            new() { ProvaId=provaFuvest23.Id, NumeroQuestao=2,  Materia="Matemática", Ano=2023, Instituicao="FUVEST", Dificuldade="Difícil",
                Enunciado="São dados, no plano cartesiano, o ponto P de coordenadas (3,6) e a circunferência C de equação (x-1)² + (y-2)² = 1. Uma reta t passa por P e é tangente a C em um ponto Q. Então a distância de P a Q é:",
                AlternativaA="3", AlternativaB="4", AlternativaC="√15", AlternativaD="√17", AlternativaE="5", RespostaCorreta="E" },

            new() { ProvaId=provaFuvest24.Id, NumeroQuestao=3,  Materia="Matemática", Ano=2024, Instituicao="FUVEST", Dificuldade="Difícil",
                Enunciado="Os conceitos de moda, mediana, média e amplitude definem medidas utilizadas para estudar um conjunto de informações numéricas. Assinale a alternativa que representa a quantidade de listas de 5 números inteiros positivos que cumprem a condição: moda = mediana = média = amplitude = 23.",
                AlternativaA="8", AlternativaB="9", AlternativaC="11", AlternativaD="22", AlternativaE="44", RespostaCorreta="C" },

            new() { ProvaId=provaFuvest24.Id, NumeroQuestao=4,  Materia="Matemática", Ano=2024, Instituicao="FUVEST", Dificuldade="Médio",
                Enunciado="Um relógio digital utiliza numerais para representar horários. Esse relógio está colocado sobre uma mesa de vidro, de forma que o vidro reflete o horário em sua superfície. De 00:00 até as 23:59, quantas vezes o vidro refletirá um horário válido?",
                AlternativaA="96", AlternativaB="360", AlternativaC="540", AlternativaD="640", AlternativaE="960", RespostaCorreta="A" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=1,  Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="O volume de um cubo é 216 cm³. Quanto mede a diagonal desse cubo?",
                AlternativaA="6 cm", AlternativaB="6√2 cm", AlternativaC="6√3 cm", AlternativaD="12 cm", AlternativaE="18 cm", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=2,  Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Um triângulo retângulo tem catetos medindo 3 cm e 4 cm. A medida da hipotenusa é:",
                AlternativaA="5 cm", AlternativaB="6 cm", AlternativaC="7 cm", AlternativaD="12 cm", AlternativaE="25 cm", RespostaCorreta="A" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=1,  Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Em uma PA, o primeiro termo é 5 e a razão é 3. O décimo termo dessa progressão é:",
                AlternativaA="30", AlternativaB="32", AlternativaC="35", AlternativaD="38", AlternativaE="41", RespostaCorreta="B" },

            new() { ProvaId=provaUnicamp23.Id, NumeroQuestao=1, Materia="Matemática", Ano=2023, Instituicao="UNICAMP", Dificuldade="Médio",
                Enunciado="A soma dos 10 primeiros termos de uma PG de primeiro termo 2 e razão 2 é:",
                AlternativaA="1022", AlternativaB="1024", AlternativaC="2046", AlternativaD="2048", AlternativaE="4096", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=2,  Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Qual é o valor de log₂(64)?",
                AlternativaA="4", AlternativaB="5", AlternativaC="6", AlternativaD="7", AlternativaE="8", RespostaCorreta="C" },

            new() { ProvaId=provaFuvest24.Id, NumeroQuestao=5,  Materia="Matemática", Ano=2024, Instituicao="FUVEST", Dificuldade="Médio",
                Enunciado="Uma função f(x) = bˣ passa pelos pontos (1, 3) e (k, 1/3). O valor de b + k é:",
                AlternativaA="5/6", AlternativaB="1", AlternativaC="6/5", AlternativaD="13/5", AlternativaE="18/5", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=3,  Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="Se sen(x) = 0,6 e x está no primeiro quadrante, quanto vale cos(x)?",
                AlternativaA="0,4", AlternativaB="0,6", AlternativaC="0,8", AlternativaD="1,0", AlternativaE="1,2", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=3,  Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="O valor de 2⁵ × 2³ é:",
                AlternativaA="128", AlternativaB="256", AlternativaC="512", AlternativaD="1024", AlternativaE="2048", RespostaCorreta="B" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=4,  Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="A raiz quadrada de 144 é:",
                AlternativaA="10", AlternativaB="11", AlternativaC="12", AlternativaD="13", AlternativaE="14", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=4,  Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Qual o resultado de 15% de 200?",
                AlternativaA="20", AlternativaB="25", AlternativaC="30", AlternativaD="35", AlternativaE="40", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=5,  Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="O perímetro de um quadrado de lado 5 cm é:",
                AlternativaA="10 cm", AlternativaB="15 cm", AlternativaC="20 cm", AlternativaD="25 cm", AlternativaE="30 cm", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=5,  Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="A área de um retângulo de base 8 cm e altura 5 cm é:",
                AlternativaA="13 cm²", AlternativaB="26 cm²", AlternativaC="40 cm²", AlternativaD="45 cm²", AlternativaE="80 cm²", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=6,  Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="O MDC entre 48 e 36 é:",
                AlternativaA="6", AlternativaB="8", AlternativaC="12", AlternativaD="16", AlternativaE="18", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=6,  Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="O MMC entre 6 e 8 é:",
                AlternativaA="12", AlternativaB="18", AlternativaC="24", AlternativaD="32", AlternativaE="48", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=7,  Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Qual é o valor de x na equação 2x + 5 = 15?",
                AlternativaA="3", AlternativaB="4", AlternativaC="5", AlternativaD="6", AlternativaE="7", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=7,  Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="A mediana do conjunto {2, 4, 6, 8, 10} é:",
                AlternativaA="4", AlternativaB="5", AlternativaC="6", AlternativaD="7", AlternativaE="8", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=8,  Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Qual é a moda do conjunto {3, 5, 5, 7, 9, 5, 11}?",
                AlternativaA="3", AlternativaB="5", AlternativaC="7", AlternativaD="9", AlternativaE="11", RespostaCorreta="B" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=8,  Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="A média aritmética de 10, 20, 30 é:",
                AlternativaA="15", AlternativaB="18", AlternativaC="20", AlternativaD="22", AlternativaE="25", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=9,  Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Dois ângulos complementares somam:",
                AlternativaA="45°", AlternativaB="60°", AlternativaC="90°", AlternativaD="180°", AlternativaE="360°", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=9,  Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Dois ângulos suplementares somam:",
                AlternativaA="45°", AlternativaB="60°", AlternativaC="90°", AlternativaD="180°", AlternativaE="360°", RespostaCorreta="D" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=10, Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="O perímetro de um círculo de raio 2 cm é (use π ≈ 3,14):",
                AlternativaA="6,28 cm", AlternativaB="9,42 cm", AlternativaC="12,56 cm", AlternativaD="15,70 cm", AlternativaE="18,84 cm", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=10, Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="A área de um círculo de raio 3 cm é (use π ≈ 3,14):",
                AlternativaA="18,84 cm²", AlternativaB="28,26 cm²", AlternativaC="37,68 cm²", AlternativaD="47,10 cm²", AlternativaE="56,52 cm²", RespostaCorreta="B" },

            new() { ProvaId=provaUnicamp23.Id, NumeroQuestao=2, Materia="Matemática", Ano=2023, Instituicao="UNICAMP", Dificuldade="Médio",
                Enunciado="O volume de um cilindro de raio 2 cm e altura 5 cm é (use π ≈ 3,14):",
                AlternativaA="31,4 cm³", AlternativaB="47,1 cm³", AlternativaC="62,8 cm³", AlternativaD="78,5 cm³", AlternativaE="94,2 cm³", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=11, Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Qual é o valor de √25 + √9?",
                AlternativaA="5", AlternativaB="6", AlternativaC="7", AlternativaD="8", AlternativaE="9", RespostaCorreta="D" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=11, Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Uma empresa teve lucro de 20% sobre o preço de custo. Se o preço de custo foi R$ 100, qual foi o lucro?",
                AlternativaA="R$ 10", AlternativaB="R$ 15", AlternativaC="R$ 20", AlternativaD="R$ 25", AlternativaE="R$ 30", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=12, Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Um produto que custava R$ 50 teve um desconto de 30%. Qual o novo preço?",
                AlternativaA="R$ 15", AlternativaB="R$ 25", AlternativaC="R$ 30", AlternativaD="R$ 35", AlternativaE="R$ 40", RespostaCorreta="D" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=12, Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Se 3x - 7 = 14, quanto vale x?",
                AlternativaA="5", AlternativaB="6", AlternativaC="7", AlternativaD="8", AlternativaE="9", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=13, Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="Qual é o resultado de (x + 3)²?",
                AlternativaA="x² + 6x + 9", AlternativaB="x² + 3x + 9", AlternativaC="x² + 6x + 6", AlternativaD="x² + 9", AlternativaE="x² + 3", RespostaCorreta="A" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=13, Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="Fatorando x² - 9, obtemos:",
                AlternativaA="(x + 3)(x + 3)", AlternativaB="(x - 3)(x - 3)", AlternativaC="(x + 3)(x - 3)", AlternativaD="(x + 9)(x - 1)", AlternativaE="Não pode ser fatorado", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=14, Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="As raízes da equação x² - 5x + 6 = 0 são:",
                AlternativaA="1 e 6", AlternativaB="2 e 3", AlternativaC="2 e 4", AlternativaD="1 e 5", AlternativaE="3 e 4", RespostaCorreta="B" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=14, Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="O valor de x na equação x/4 = 8 é:",
                AlternativaA="16", AlternativaB="24", AlternativaC="28", AlternativaD="32", AlternativaE="36", RespostaCorreta="D" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=15, Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Simplificando a fração 18/24, obtemos:",
                AlternativaA="1/2", AlternativaB="2/3", AlternativaC="3/4", AlternativaD="4/5", AlternativaE="5/6", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=15, Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="Qual é o resultado de 1/2 + 1/3?",
                AlternativaA="2/5", AlternativaB="2/6", AlternativaC="3/5", AlternativaD="5/6", AlternativaE="6/5", RespostaCorreta="D" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=16, Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="O resultado de 2/3 × 3/4 é:",
                AlternativaA="1/2", AlternativaB="2/3", AlternativaC="3/4", AlternativaD="5/12", AlternativaE="6/12", RespostaCorreta="A" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=16, Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="Quanto é 3/4 ÷ 1/2?",
                AlternativaA="1/2", AlternativaB="3/8", AlternativaC="3/2", AlternativaD="4/3", AlternativaE="2", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=17, Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Transformando 0,75 em fração, obtemos:",
                AlternativaA="1/2", AlternativaB="2/3", AlternativaC="3/4", AlternativaD="4/5", AlternativaE="7/10", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=17, Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Transformando 2/5 em decimal, obtemos:",
                AlternativaA="0,2", AlternativaB="0,25", AlternativaC="0,4", AlternativaD="0,5", AlternativaE="0,6", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=18, Materia="Matemática", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Um carro percorre 150 km em 2 horas. Qual sua velocidade média?",
                AlternativaA="50 km/h", AlternativaB="60 km/h", AlternativaC="75 km/h", AlternativaD="90 km/h", AlternativaE="100 km/h", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=18, Materia="Matemática", Ano=2023, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="Em uma proporção, se a/b = c/d e a=2, b=4, c=3, quanto vale d?",
                AlternativaA="4", AlternativaB="5", AlternativaC="6", AlternativaD="7", AlternativaE="8", RespostaCorreta="C" },

            // ── PORTUGUÊS ───────────────────────────────────────
            new() { ProvaId=provaFuvest24.Id, NumeroQuestao=1,  Materia="Português", Ano=2024, Instituicao="FUVEST", Dificuldade="Médio",
                Enunciado="O verso 'É tempo de formar novos quilombos' no poema de Conceição Evaristo é um exemplo de:",
                AlternativaA="Paradoxo", AlternativaB="Metonímia", AlternativaC="Metáfora", AlternativaD="Antítese", AlternativaE="Hipérbole", RespostaCorreta="C" },

            new() { ProvaId=provaFuvest24.Id, NumeroQuestao=2,  Materia="Português", Ano=2024, Instituicao="FUVEST", Dificuldade="Médio",
                Enunciado="Em 'Dois irmãos', de Milton Hatoum, a rivalidade entre Yaqub e Omar tem como resultado:",
                AlternativaA="A prosperidade econômica de ambos", AlternativaB="A reconstrução dos laços entre eles",
                AlternativaC="A ida de Nael a São Paulo para viver com Yaqub", AlternativaD="A morte de Rânia, a irmã dos gêmeos",
                AlternativaE="A desagregação e a ruína da família", RespostaCorreta="E" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=1,  Materia="Português", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Qual figura de linguagem está presente na frase: 'As ondas beijavam a areia'?",
                AlternativaA="Metáfora", AlternativaB="Metonímia", AlternativaC="Prosopopeia", AlternativaD="Hipérbole", AlternativaE="Eufemismo", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=1,  Materia="Português", Ano=2024, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="Assinale a alternativa que apresenta um caso de oração subordinada substantiva:",
                AlternativaA="Espero que você venha", AlternativaB="Quando chegar, me avise",
                AlternativaC="Ela é linda como uma flor", AlternativaD="Comprei livros e cadernos",
                AlternativaE="Vou à praia se fizer sol", RespostaCorreta="A" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=2,  Materia="Português", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Qual das alternativas apresenta um pronome relativo?",
                AlternativaA="Este", AlternativaB="Que", AlternativaC="Nosso", AlternativaD="Algum", AlternativaE="Muito", RespostaCorreta="B" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=2,  Materia="Português", Ano=2024, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="Indique a frase que apresenta voz passiva analítica:",
                AlternativaA="Vendem-se casas", AlternativaB="A casa foi vendida",
                AlternativaC="Vendi a casa", AlternativaD="Comprei uma casa",
                AlternativaE="A casa está bonita", RespostaCorreta="B" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=3,  Materia="Português", Ano=2023, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="Assinale a alternativa em que todas as palavras estão corretamente acentuadas:",
                AlternativaA="Saúde, bau, país", AlternativaB="Fácil, açúcar, pêssego",
                AlternativaC="Heroi, chapéu, cafe", AlternativaD="Tambem, através, saída",
                AlternativaE="Juri, substituí-lo, árvore", RespostaCorreta="B" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=3,  Materia="Português", Ano=2024, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="Em qual alternativa o 'se' é uma partícula apassivadora?",
                AlternativaA="Ela se arrependeu", AlternativaB="Precisa-se de ajuda",
                AlternativaC="Vendeu-se a casa", AlternativaD="Ele se feriu",
                AlternativaE="Eles se amam", RespostaCorreta="C" },

            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=4,  Materia="Português", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Qual é o plural de 'cidadão'?",
                AlternativaA="Cidadões", AlternativaB="Cidadães", AlternativaC="Cidadãos",
                AlternativaD="Cidadans", AlternativaE="Cidadãoes", RespostaCorreta="C" },

            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=4,  Materia="Português", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Identifique o sujeito em: 'Chegaram os alunos':",
                AlternativaA="Chegaram", AlternativaB="Os", AlternativaC="Os alunos",
                AlternativaD="Alunos", AlternativaE="Sujeito indeterminado", RespostaCorreta="C" },

            // ── FÍSICA ──────────────────────────────────────────
            new() { ProvaId=provaFuvest23.Id, NumeroQuestao=1,  Materia="Física", Ano=2023, Instituicao="FUVEST", Dificuldade="Fácil",
                Enunciado="Um corpo em queda livre no vácuo possui aceleração de aproximadamente:",
                AlternativaA="5 m/s²", AlternativaB="8 m/s²", AlternativaC="10 m/s²", AlternativaD="12 m/s²", AlternativaE="15 m/s²", RespostaCorreta="C" },

            new() { ProvaId=provaFuvest23.Id, NumeroQuestao=2,  Materia="Física", Ano=2023, Instituicao="FUVEST", Dificuldade="Médio",
                Enunciado="A primeira lei de Newton é também conhecida como:",
                AlternativaA="Lei da Gravitação Universal", AlternativaB="Lei da Inércia",
                AlternativaC="Lei da Ação e Reação", AlternativaD="Lei de Ohm",
                AlternativaE="Lei de Coulomb", RespostaCorreta="B" },

            // ── QUÍMICA ─────────────────────────────────────────
            new() { ProvaId=provaUnicamp23.Id, NumeroQuestao=3, Materia="Química", Ano=2023, Instituicao="UNICAMP", Dificuldade="Fácil",
                Enunciado="Qual é o símbolo químico do ouro?",
                AlternativaA="O", AlternativaB="Au", AlternativaC="Ag", AlternativaD="Go", AlternativaE="Or", RespostaCorreta="B" },

            new() { ProvaId=provaUnicamp23.Id, NumeroQuestao=4, Materia="Química", Ano=2023, Instituicao="UNICAMP", Dificuldade="Fácil",
                Enunciado="A fórmula química da água é:",
                AlternativaA="H2O", AlternativaB="HO2", AlternativaC="H3O", AlternativaD="O2H", AlternativaE="OH2", RespostaCorreta="A" },

            // ── BIOLOGIA ────────────────────────────────────────
            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=5,  Materia="Biologia", Ano=2024, Instituicao="ENEM", Dificuldade="Médio",
                Enunciado="A fotossíntese ocorre principalmente em qual organela?",
                AlternativaA="Mitocôndria", AlternativaB="Núcleo", AlternativaC="Cloroplasto",
                AlternativaD="Ribossomo", AlternativaE="Retículo", RespostaCorreta="C" },

            // ── HISTÓRIA ────────────────────────────────────────
            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=6,  Materia="História", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Em que ano foi proclamada a República no Brasil?",
                AlternativaA="1888", AlternativaB="1889", AlternativaC="1890", AlternativaD="1891", AlternativaE="1892", RespostaCorreta="B" },

            // ── GEOGRAFIA ───────────────────────────────────────
            new() { ProvaId=provaEnem24.Id,   NumeroQuestao=7,  Materia="Geografia", Ano=2024, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="Qual é o maior país em extensão territorial do mundo?",
                AlternativaA="China", AlternativaB="Canadá", AlternativaC="EUA", AlternativaD="Rússia", AlternativaE="Brasil", RespostaCorreta="D" },

            // ── INGLÊS ──────────────────────────────────────────
            new() { ProvaId=provaEnem23.Id,   NumeroQuestao=19, Materia="Inglês", Ano=2023, Instituicao="ENEM", Dificuldade="Fácil",
                Enunciado="What is the past tense of 'go'?",
                AlternativaA="Goed", AlternativaB="Went", AlternativaC="Gone", AlternativaD="Going", AlternativaE="Goes", RespostaCorreta="B" },
        };

        db.Questoes.AddRange(questoes);
        await db.SaveChangesAsync();

        Console.WriteLine($"✅ Seed concluído: {questoes.Count} questões inseridas em {provas.Count} provas.");
    }
}