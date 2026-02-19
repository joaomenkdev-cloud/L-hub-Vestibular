using LHubVestibular.Data;
using LHubVestibular.Models;
using LHubVestibular.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace LHubVestibular.Controllers;

// ── DTOs ─────────────────────────────────────────────────────
public record InscricaoRequest(
    string Nome, string Cpf, string Email, string Telefone, string Curso,
    string? Rg, string? DataNascimento, string? Sexo,
    string? NomeMae, string? NomePai, string? Cep, string? Rua,
    string? Numero, string? Complemento, string? Bairro, string? Cidade, string? Estado,
    string? Turno, string? Modalidade, bool Cotas, string? TipoCota);

public record LoginRequest(string NumeroInscricao, string Senha);
public record TrocarSenhaRequest(string NumeroInscricao, string SenhaAtual, string SenhaNova);
public record ConfirmarPagamentoRequest(string NumeroInscricao);


// ══════════════════════════════════════════════════════════════
//  INSCRIÇÃO
// ══════════════════════════════════════════════════════════════
[ApiController, Route("api/inscricao")]
public class InscricaoController(AppDbContext db, InscricaoService svc, AuthService auth) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] InscricaoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Cpf) ||
            string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Telefone) ||
            string.IsNullOrWhiteSpace(req.Curso))
            return BadRequest(new { success = false, message = "Campos obrigatórios ausentes." });

        var cpf = req.Cpf.Replace(".", "").Replace("-", "");

        if (await svc.CpfJaExisteAsync(cpf))
            return Conflict(new { success = false, message = "CPF já cadastrado." });

        var numero = InscricaoService.GerarNumeroInscricao();
        var senha  = Guid.NewGuid().ToString("N")[..8];
        var venc   = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));

        DateOnly? dataNasc = null;
        if (DateOnly.TryParse(req.DataNascimento, out var dn)) dataNasc = dn;

        var inscricao = new Inscricao
        {
            NumeroInscricao = numero,
            Nome = req.Nome, Cpf = cpf, Rg = req.Rg, DataNascimento = dataNasc,
            Sexo = req.Sexo, Email = req.Email, Telefone = req.Telefone,
            NomeMae = req.NomeMae, NomePai = req.NomePai,
            Cep = req.Cep, Rua = req.Rua, NumeroEnd = req.Numero,
            Complemento = req.Complemento, Bairro = req.Bairro,
            Cidade = req.Cidade, Estado = req.Estado,
            Curso = req.Curso, Turno = req.Turno, Modalidade = req.Modalidade,
            Cotas = req.Cotas, TipoCota = req.TipoCota
        };
        db.Inscricoes.Add(inscricao);
        await db.SaveChangesAsync();

        db.Pagamentos.Add(new Pagamento
        {
            InscricaoId = inscricao.Id, NumeroInscricao = numero,
            Status = "pendente", Vencimento = venc
        });
        db.Usuarios.Add(new Usuario
        {
            NumeroInscricao = numero, Email = req.Email, Nome = req.Nome,
            SenhaHash = AuthService.HashSenha(senha)
        });
        await db.SaveChangesAsync();

        return Created("", new
        {
            success = true, message = "Inscrição realizada!",
            numero_inscricao = numero,
            senha_temporaria = senha,
            vencimento = venc.ToString("yyyy-MM-dd")
        });
    }

    [HttpGet("{numero}")]
    public async Task<IActionResult> Buscar(string numero)
    {
        var insc = await svc.BuscarPorNumeroAsync(numero);
        if (insc == null) return NotFound(new { success = false, message = "Não encontrada." });

        // Verificação de acesso
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(token))
        {
            var usuario = await auth.ValidarTokenAsync(token);
            if (usuario != null && usuario.NumeroInscricao != numero)
                return StatusCode(403, new { success = false, message = "Acesso negado." });
        }

        var pag = await svc.BuscarPagamentoPorNumeroAsync(numero);
        return Ok(new { inscricao = insc, pagamento = pag });
    }
}


// ══════════════════════════════════════════════════════════════
//  AUTENTICAÇÃO
// ══════════════════════════════════════════════════════════════
[ApiController, Route("api/auth")]
public class AuthController(AppDbContext db, AuthService auth) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NumeroInscricao) || string.IsNullOrWhiteSpace(req.Senha))
            return BadRequest(new { success = false, message = "Campos obrigatórios." });

        var num = req.NumeroInscricao.Trim().ToUpper();
        var u   = await db.Usuarios.FirstOrDefaultAsync(x => x.NumeroInscricao == num);

        if (u == null || u.SenhaHash != AuthService.HashSenha(req.Senha))
            return Unauthorized(new { success = false, message = "Credenciais inválidas." });

        var token = await auth.CriarTokenAsync(u.Id, num);
        u.UltimoAcesso = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var insc = await db.Inscricoes.FirstOrDefaultAsync(i => i.NumeroInscricao == num);
        var pag  = await db.Pagamentos.FirstOrDefaultAsync(p => p.NumeroInscricao == num);

        return Ok(new
        {
            success = true,
            token = token.TokenValue,
            usuario = new { u.NumeroInscricao, u.Nome, u.Email, primeiro_acesso = u.PrimeiroAcesso },
            inscricao = insc == null ? null : new { insc.Curso, insc.Status, pagamento = pag }
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var tokenVal = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(tokenVal))
            await auth.RevogarTokenAsync(tokenVal);
        return Ok(new { success = true });
    }

    [HttpPost("trocar-senha")]
    public async Task<IActionResult> TrocarSenha([FromBody] TrocarSenhaRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.SenhaNova) || req.SenhaNova.Length < 6)
            return BadRequest(new { success = false, message = "Mínimo 6 caracteres." });

        var u = await db.Usuarios.FirstOrDefaultAsync(x => x.NumeroInscricao == req.NumeroInscricao.ToUpper());
        if (u == null) return NotFound(new { success = false, message = "Usuário não encontrado." });
        if (u.SenhaHash != AuthService.HashSenha(req.SenhaAtual))
            return Unauthorized(new { success = false, message = "Senha atual incorreta." });

        u.SenhaHash = AuthService.HashSenha(req.SenhaNova);
        u.PrimeiroAcesso = false;
        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Senha alterada com sucesso." });
    }

    [HttpGet("perfil")]
    public async Task<IActionResult> Perfil()
    {
        var tokenVal = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var u = await auth.ValidarTokenAsync(tokenVal);
        if (u == null) return Unauthorized(new { success = false, message = "Não autenticado." });

        var insc = await db.Inscricoes.FirstOrDefaultAsync(i => i.NumeroInscricao == u.NumeroInscricao);
        return Ok(new
        {
            success = true,
            usuario = new { u.NumeroInscricao, u.Nome, u.Email },
            inscricao = insc
        });
    }
}


// ══════════════════════════════════════════════════════════════
//  PAGAMENTO
// ══════════════════════════════════════════════════════════════
[ApiController, Route("api/pagamento")]
public class PagamentoController(AppDbContext db) : ControllerBase
{
    [HttpPost("confirmar")]
    public async Task<IActionResult> Confirmar([FromBody] ConfirmarPagamentoRequest req)
    {
        var insc = await db.Inscricoes.FirstOrDefaultAsync(i => i.NumeroInscricao == req.NumeroInscricao);
        if (insc == null) return NotFound(new { success = false, message = "Inscrição não encontrada." });

        var pag = await db.Pagamentos.FirstOrDefaultAsync(p => p.NumeroInscricao == req.NumeroInscricao);
        if (pag != null) { pag.Status = "confirmado"; pag.DataPagamento = DateTime.UtcNow; }

        insc.Status = "inscrito";
        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Pagamento confirmado." });
    }

    [HttpGet("/api/boleto/{numero}")]
    public async Task<IActionResult> Boleto(string numero)
    {
        var pag = await db.Pagamentos.FirstOrDefaultAsync(p => p.NumeroInscricao == numero);
        if (pag == null) return NotFound(new { success = false, message = "Não encontrado." });
        return Ok(new
        {
            numero_inscricao = numero, pag.Valor,
            vencimento = pag.Vencimento.ToString("yyyy-MM-dd"),
            pag.CodigoBarras, pag.LinhaDigitavel, pag.Status
        });
    }
}


// ══════════════════════════════════════════════════════════════
//  NOTÍCIAS E EDITAIS
// ══════════════════════════════════════════════════════════════
[ApiController, Route("api/noticias")]
public class NoticiasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(string? categoria, int pagina = 1, int limite = 10)
    {
        var query = db.Noticias.Where(n => n.Publicado);
        if (!string.IsNullOrEmpty(categoria)) query = query.Where(n => n.Categoria == categoria);

        var total = await query.CountAsync();
        var dados = await query.OrderByDescending(n => n.PublicadoEm)
                               .Skip((pagina - 1) * limite).Take(limite)
                               .ToListAsync();
        return Ok(new { success = true, total, pagina, data = dados });
    }

    [HttpGet("destaques")]
    public async Task<IActionResult> Destaques() =>
        Ok(new { success = true, data = await db.Noticias
            .Where(n => n.Publicado && n.Destaque)
            .OrderByDescending(n => n.PublicadoEm).Take(3).ToListAsync() });

    [HttpGet("recentes")]
    public async Task<IActionResult> Recentes(int limite = 5) =>
        Ok(new { success = true, data = await db.Noticias
            .Where(n => n.Publicado)
            .OrderByDescending(n => n.PublicadoEm)
            .Take(limite)
            .Select(n => new { n.Id, n.Titulo, n.Resumo, n.Categoria, n.BadgeTipo, n.PublicadoEm })
            .ToListAsync() });

    [HttpGet("{slugOuId}")]
    public async Task<IActionResult> Get(string slugOuId)
    {
        Noticia? noticia;
        if (int.TryParse(slugOuId, out var id))
            noticia = await db.Noticias.FirstOrDefaultAsync(n => n.Id == id && n.Publicado);
        else
            noticia = await db.Noticias.FirstOrDefaultAsync(n => n.Slug == slugOuId && n.Publicado);

        if (noticia == null) return NotFound(new { success = false, message = "Não encontrada." });
        noticia.Views++;
        await db.SaveChangesAsync();
        return Ok(new { success = true, data = noticia });
    }
}


// ══════════════════════════════════════════════════════════════
//  PROVAS
// ══════════════════════════════════════════════════════════════
[ApiController, Route("api/provas")]
public class ProvasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(int? ano, string? area, string? instituicao)
    {
        var q = db.Provas.Where(p => p.Ativo);
        if (ano != null)          q = q.Where(p => p.Ano == ano);
        if (!string.IsNullOrEmpty(area)) q = q.Where(p => p.Area == area);
        if (!string.IsNullOrEmpty(instituicao)) q = q.Where(p => p.Instituicao == instituicao);
        var dados = await q.OrderByDescending(p => p.Ano).ThenBy(p => p.Id).ToListAsync();
        return Ok(new { success = true, data = dados, total = dados.Count });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var prova = await db.Provas.FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
        if (prova == null) return NotFound(new { success = false, message = "Não encontrada." });
        return Ok(new { success = true, data = prova });
    }

    [HttpPost("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var prova = await db.Provas.FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
        if (prova == null) return NotFound(new { success = false, message = "Não encontrada." });
        prova.Downloads++;
        await db.SaveChangesAsync();
        return Ok(new { success = true, prova_url = prova.ProvaUrl, gabarito_url = prova.GabaritoUrl, titulo = prova.Titulo });
    }
}


// ══════════════════════════════════════════════════════════════
//  QUESTÕES / SIMULADO
// ══════════════════════════════════════════════════════════════
[ApiController, Route("api/questoes")]
public class QuestoesController(AppDbContext db, AuthService auth) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar(string? materia, string? dificuldade, int? ano, int limit = 20)
    {
        var q = db.Questoes.Where(x => x.Ativo);
        if (!string.IsNullOrEmpty(materia))    q = q.Where(x => x.Materia == materia);
        if (!string.IsNullOrEmpty(dificuldade)) q = q.Where(x => x.Dificuldade == dificuldade);
        if (ano != null) q = q.Where(x => x.Ano == ano);
        var dados = await q.OrderBy(_ => EF.Functions.Random()).Take(limit).ToListAsync();
        return Ok(new { success = true, data = dados, total = dados.Count });
    }

    [HttpGet("materias")]
    public async Task<IActionResult> Materias()
    {
        var lista = await db.Questoes.Where(q => q.Ativo)
            .GroupBy(q => q.Materia)
            .Select(g => new { materia = g.Key, total = g.Count() })
            .ToListAsync();
        return Ok(new { success = true, data = lista });
    }
}


// ══════════════════════════════════════════════════════════════
//  CRONOGRAMA
// ══════════════════════════════════════════════════════════════
[ApiController, Route("api/cronograma")]
public class CronogramaController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var itens = await db.Cronograma.Where(c => c.Ativo).OrderBy(c => c.Ordem).ToListAsync();
        var resultado = itens.Select(c => new
        {
            c.Id, c.Titulo, c.Descricao,
            data_inicio = c.DataInicio.ToString("yyyy-MM-dd"),
            data_fim    = c.DataFim.ToString("yyyy-MM-dd"),
            c.Ordem,
            status = InscricaoService.CalcStatus(c.DataInicio, c.DataFim)
        });
        return Ok(resultado);
    }
}


// ══════════════════════════════════════════════════════════════
//  DASHBOARD DO CANDIDATO
// ══════════════════════════════════════════════════════════════
[ApiController, Route("api/candidato")]
public class CandidatoController(AppDbContext db, AuthService auth) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var tokenVal = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var u = await auth.ValidarTokenAsync(tokenVal);
        if (u == null) return Unauthorized(new { success = false, message = "Não autenticado." });

        var insc   = await db.Inscricoes.FirstOrDefaultAsync(i => i.NumeroInscricao == u.NumeroInscricao);
        var pag    = await db.Pagamentos.FirstOrDefaultAsync(p => p.NumeroInscricao == u.NumeroInscricao);
        var dias   = Math.Max(0, (new DateTime(2026, 6, 15) - DateTime.UtcNow).Days);
        var comuns = await db.Noticias.Where(n => n.Publicado)
                        .OrderByDescending(n => n.PublicadoEm).Take(3)
                        .Select(n => new { n.Id, n.Titulo, n.Resumo, n.Categoria, n.BadgeTipo, n.PublicadoEm })
                        .ToListAsync();

        return Ok(new
        {
            success = true,
            usuario = new { u.NumeroInscricao, u.Nome, u.Email },
            inscricao = insc, pagamento = pag,
            dias_para_prova = dias, comunicados = comuns
        });
    }

    [HttpGet("comprovante/{numero}")]
    public async Task<IActionResult> Comprovante(string numero)
    {
        var insc = await db.Inscricoes.FirstOrDefaultAsync(i => i.NumeroInscricao == numero);
        if (insc == null) return NotFound(new { success = false, message = "Não encontrada." });
        return Ok(new { success = true, numero_inscricao = numero, insc.Nome, insc.Curso, data_inscricao = insc.DataInscricao });
    }
}


// ══════════════════════════════════════════════════════════════
//  LOCAL DE PROVA / RESULTADO
// ══════════════════════════════════════════════════════════════
[ApiController, Route("api")]
public class LocalResultadoController(AppDbContext db) : ControllerBase
{
    [HttpGet("local-prova/{numero}")]
    public async Task<IActionResult> LocalProva(string numero)
    {
        var insc = await db.Inscricoes.FirstOrDefaultAsync(i => i.NumeroInscricao == numero);
        if (insc == null) return NotFound(new { success = false, message = "Não encontrada." });
        if (insc.Status is not ("inscrito" or "concluido"))
            return StatusCode(403, new { success = false, message = "Pagamento pendente." });
        if (DateTime.UtcNow < new DateTime(2026, 5, 20))
            return StatusCode(403, new { success = false, message = "Locais divulgados em 20/05/2026." });

        return Ok(new
        {
            success = true, local = "L-Hub Campus Central",
            endereco = "Av. Paulista, 1000", cidade = "São Paulo – SP",
            sala = "Bloco A – Sala 101", data = "2026-06-15",
            horario = "09:00", portao_abre = "08:00", portao_fecha = "08:45"
        });
    }

    [HttpGet("resultado/{numero}")]
    public async Task<IActionResult> Resultado(string numero)
    {
        var res = await db.Resultados.FirstOrDefaultAsync(r => r.NumeroInscricao == numero);
        if (res != null) return Ok(new { success = true, data = res });
        return Ok(new { success = true, data = new { numero_inscricao = numero, status = "aguardando", mensagem = "Resultado em 30/06/2026." } });
    }
}


// ══════════════════════════════════════════════════════════════
//  HEALTH CHECK
// ══════════════════════════════════════════════════════════════
[ApiController, Route("api/health")]
public class HealthController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var inscricoes    = await db.Inscricoes.CountAsync();
        var tokensAtivos  = await db.Tokens.CountAsync(t => t.Ativo);
        var totalQuestoes = await db.Questoes.CountAsync(q => q.Ativo);
        return Ok(new
        {
            status = "online", version = "3.0.0",
            timestamp = DateTime.UtcNow.ToString("O"),
            inscricoes, tokens_ativos = tokensAtivos, questoes = totalQuestoes
        });
    }
}

// ══════════════════════════════════════════════════════════════
//  IA — EXPLICAÇÃO DE QUESTÕES (Groq Cloud)
// ══════════════════════════════════════════════════════════════
public record ExplicacaoRequest(string Prompt);

[ApiController, Route("api/ia")]
public class IaController : ControllerBase
{
    private static readonly HttpClient _http = new();

    // Chave lida da variável de ambiente GROQ_API_KEY
    // Configure antes de rodar: export GROQ_API_KEY="sua_chave_aqui"  (Linux/Mac)
    //                           set GROQ_API_KEY=sua_chave_aqui        (Windows CMD)
    private static readonly string GroqApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "";
    private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

    // Modelos disponíveis na Groq (fallback automático)
    private static readonly string[] GroqModelos = new[]
    {
        "llama-3.3-70b-versatile",   // Melhor qualidade
        "llama-3.1-8b-instant",       // Mais rápido
        "gemma2-9b-it"                // Fallback
    };

    [HttpPost("explicacao")]
    public async Task<IActionResult> Explicacao([FromBody] ExplicacaoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Prompt))
            return BadRequest(new { success = false, message = "Prompt vazio." });

        foreach (var modelo in GroqModelos)
        {
            var body = JsonSerializer.Serialize(new
            {
                model = modelo,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = "Você é um professor expert em vestibulares brasileiros (ENEM, FUVEST, UNICAMP). Explique questões de forma clara, didática e objetiva em português."
                    },
                    new
                    {
                        role = "user",
                        content = req.Prompt
                    }
                },
                max_tokens = 800,
                temperature = 0.7
            });

            HttpResponseMessage response;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, GroqUrl);
                request.Headers.Add("Authorization", $"Bearer {GroqApiKey}");
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                response = await _http.SendAsync(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERRO CONEXÃO GROQ [{modelo}]: {ex.Message} ===");
                continue;
            }

            var resultado = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"=== GROQ [{modelo}] STATUS: {(int)response.StatusCode} ===");

            if ((int)response.StatusCode == 429)
            {
                Console.WriteLine($"=== [{modelo}] rate limit, tentando próximo... ===");
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"=== GROQ ERRO: {resultado} ===");
                return StatusCode(500, new { success = false, message = $"Erro HTTP {(int)response.StatusCode} no modelo {modelo}" });
            }

            try
            {
                // Resposta no formato OpenAI-compatible:
                // { choices: [{ message: { content: "..." } }] }
                using var doc = JsonDocument.Parse(resultado);
                var texto = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                Console.WriteLine($"=== SUCESSO GROQ [{modelo}] ===");
                return Ok(new { success = true, texto });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERRO PARSE [{modelo}]: {ex.Message} ===");
                return StatusCode(500, new { success = false, message = "Erro ao parsear resposta: " + ex.Message });
            }
        }

        return StatusCode(429, new
        {
            success = false,
            message = "Limite de requisições atingido. Aguarde alguns segundos e tente novamente."
        });
    }
}


[ApiController, Route("")]
public class IndexController : ControllerBase
{
    [HttpGet] public IActionResult Get() =>
        Ok(new { message = "L-Hub Vestibular API", version = "3.0.0" });
}