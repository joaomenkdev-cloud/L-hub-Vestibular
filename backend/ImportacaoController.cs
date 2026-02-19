using LHubVestibular.Data;
using LHubVestibular.Models;
using LHubVestibular.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LHubVestibular.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  DTOs DE ENTRADA/SAÍDA
// ═══════════════════════════════════════════════════════════════════

public record ImportarUrlRequest(
    string Url,
    string? GabaritoUrl = null);

public record ImportarListaRequest(
    List<string> Urls);

public record QuestaoManualRequest(
    int     ProvaId,
    int     NumeroQuestao,
    string  Materia,
    string  Enunciado,
    string  AlternativaA,
    string  AlternativaB,
    string  AlternativaC,
    string  AlternativaD,
    string  AlternativaE,
    string  RespostaCorreta,
    string  Dificuldade = "Médio");

public record AtualizarQuestaoRequest(
    string? Materia,
    string? Enunciado,
    string? AlternativaA,
    string? AlternativaB,
    string? AlternativaC,
    string? AlternativaD,
    string? AlternativaE,
    string? RespostaCorreta,
    string? Dificuldade);


// ═══════════════════════════════════════════════════════════════════
//  ADMIN — IMPORTAÇÃO DE PDFs
// ═══════════════════════════════════════════════════════════════════

[ApiController, Route("api/admin/importacao")]
public class ImportacaoController(
    AppDbContext db,
    PdfImportService importService,
    ILogger<ImportacaoController> logger) : ControllerBase
{
    // ── POST /api/admin/importacao/url ────────────────────────────
    /// <summary>
    /// Importa questões de um único PDF pelo link.
    /// Baixa o PDF, extrai questões, classifica por matéria e salva no banco.
    /// </summary>
    [HttpPost("url")]
    public async Task<IActionResult> ImportarUrl([FromBody] ImportarUrlRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Url))
            return BadRequest(new { success = false, message = "URL obrigatória." });

        logger.LogInformation("Requisição de importação: {Url}", req.Url);

        var resultado = await importService.ImportarAsync(req.Url, req.GabaritoUrl);

        return resultado.Success
            ? Ok(new
              {
                  success      = true,
                  mensagem     = resultado.Mensagem,
                  fonte_url    = resultado.FonteUrl,
                  questoes_salvas  = resultado.QuestoesSalvas,
                  duplicadas   = resultado.QueuesIgnoradas,
                  erros_parse  = resultado.ErrosParsing,
                  avisos       = resultado.Erros,
              })
            : StatusCode(500, new
              {
                  success  = false,
                  mensagem = resultado.Mensagem,
                  erros    = resultado.Erros,
              });
    }

    // ── POST /api/admin/importacao/lista ──────────────────────────
    /// <summary>
    /// Importa questões de múltiplas URLs de uma vez.
    /// </summary>
    [HttpPost("lista")]
    public async Task<IActionResult> ImportarLista([FromBody] ImportarListaRequest req)
    {
        if (req.Urls == null || req.Urls.Count == 0)
            return BadRequest(new { success = false, message = "Lista de URLs vazia." });

        var resultados = new List<object>();
        var totalSalvas = 0;

        foreach (var url in req.Urls)
        {
            var r = await importService.ImportarAsync(url);
            totalSalvas += r.QuestoesSalvas;
            resultados.Add(new
            {
                url          = r.FonteUrl,
                success      = r.Success,
                mensagem     = r.Mensagem,
                salvas       = r.QuestoesSalvas,
                duplicadas   = r.QueuesIgnoradas,
                erros        = r.ErrosParsing,
            });
            await Task.Delay(300); // respeitar rate limiting dos servidores
        }

        return Ok(new
        {
            success        = true,
            total_salvas   = totalSalvas,
            total_urls     = req.Urls.Count,
            resultados     = resultados,
        });
    }

    // ── POST /api/admin/importacao/todas ──────────────────────────
    /// <summary>
    /// Importa TODAS as provas do catálogo oficial (ENEM 2022-2024, FUVEST 2023-2025, UNICAMP 2024-2025).
    /// </summary>
    [HttpPost("todas")]
    public async Task<IActionResult> ImportarTodas()
    {
        logger.LogInformation("Iniciando importação de TODAS as fontes conhecidas...");
        var resultados = await importService.ImportarTodasAsync();

        var totalSalvas = resultados.Sum(r => r.QuestoesSalvas);
        var sucessos    = resultados.Count(r => r.Success);

        return Ok(new
        {
            success       = true,
            total_urls    = resultados.Count,
            total_salvas  = totalSalvas,
            sucessos      = sucessos,
            falhas        = resultados.Count - sucessos,
            detalhes      = resultados.Select(r => new
            {
                url       = r.FonteUrl,
                success   = r.Success,
                mensagem  = r.Mensagem,
                salvas    = r.QuestoesSalvas,
                duplicadas= r.QueuesIgnoradas,
            }).ToList(),
        });
    }

    // ── GET /api/admin/importacao/fontes ──────────────────────────
    /// <summary>
    /// Lista todas as fontes cadastradas no catálogo.
    /// </summary>
    [HttpGet("fontes")]
    public IActionResult ListarFontes()
    {
        return Ok(new
        {
            success = true,
            fontes  = PdfImportService.FontesConhecidas.Select(f => new
            {
                url         = f.Url,
                gabarito_url= f.GabaritoUrl,
                instituicao = f.Instituicao,
                ano         = f.Ano,
                caderno     = f.Caderno,
                periodo     = f.Periodo,
            }).ToList()
        });
    }
}


// ═══════════════════════════════════════════════════════════════════
//  ADMIN — GERENCIAMENTO DO BANCO DE QUESTÕES
// ═══════════════════════════════════════════════════════════════════

[ApiController, Route("api/admin/questoes")]
public class AdminQuestoesController(AppDbContext db) : ControllerBase
{
    // ── GET /api/admin/questoes ───────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string?  materia     = null,
        [FromQuery] string?  instituicao = null,
        [FromQuery] int?     ano         = null,
        [FromQuery] string?  dificuldade = null,
        [FromQuery] int      pagina      = 1,
        [FromQuery] int      tamanho     = 50)
    {
        var query = db.Questoes
            .Include(q => q.Prova)
            .Where(q => q.Ativo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(materia))
            query = query.Where(q => q.Materia.ToLower().Contains(materia.ToLower()));
        if (!string.IsNullOrEmpty(instituicao))
            query = query.Where(q => q.Instituicao != null && q.Instituicao.ToLower().Contains(instituicao.ToLower()));
        if (ano.HasValue)
            query = query.Where(q => q.Ano == ano.Value);
        if (!string.IsNullOrEmpty(dificuldade))
            query = query.Where(q => q.Dificuldade == dificuldade);

        var total = await query.CountAsync();

        var questoes = await query
            .OrderBy(q => q.Ano)
            .ThenBy(q => q.NumeroQuestao)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(q => new
            {
                q.Id, q.NumeroQuestao, q.Materia, q.Dificuldade,
                q.Ano, q.Instituicao, q.RespostaCorreta,
                enunciado = q.Enunciado.Length > 120
                    ? (q.Enunciado.Substring(0, 120) + "...")
                    : q.Enunciado,
                prova = q.Prova == null ? null : new { q.Prova.Titulo, q.Prova.Instituicao },
            })
            .ToListAsync();

        return Ok(new { success = true, total, pagina, tamanho, data = questoes });
    }

    // ── GET /api/admin/questoes/{id} ──────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalhes(int id)
    {
        var q = await db.Questoes.Include(x => x.Prova).FirstOrDefaultAsync(x => x.Id == id);
        if (q == null) return NotFound(new { success = false, message = "Questão não encontrada." });
        return Ok(new { success = true, data = q });
    }

    // ── POST /api/admin/questoes ──────────────────────────────────
    /// <summary>
    /// Cadastra uma questão manualmente (sem precisar de PDF).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CriarManual([FromBody] QuestaoManualRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Enunciado) || string.IsNullOrWhiteSpace(req.Materia))
            return BadRequest(new { success = false, message = "Enunciado e matéria são obrigatórios." });

        // Verificar se prova existe
        var prova = await db.Provas.FindAsync(req.ProvaId);
        if (prova == null)
            return NotFound(new { success = false, message = $"Prova ID {req.ProvaId} não encontrada." });

        var q = new Questao
        {
            ProvaId        = req.ProvaId,
            NumeroQuestao  = req.NumeroQuestao,
            Materia        = req.Materia,
            Enunciado      = req.Enunciado.Trim(),
            AlternativaA   = req.AlternativaA.Trim(),
            AlternativaB   = req.AlternativaB.Trim(),
            AlternativaC   = req.AlternativaC?.Trim() ?? "",
            AlternativaD   = req.AlternativaD?.Trim() ?? "",
            AlternativaE   = req.AlternativaE?.Trim() ?? "",
            RespostaCorreta= req.RespostaCorreta.ToUpper(),
            Dificuldade    = req.Dificuldade,
            Ano            = prova.Ano,
            Instituicao    = prova.Instituicao,
        };

        db.Questoes.Add(q);
        await db.SaveChangesAsync();

        return Created("", new { success = true, message = "Questão criada.", data = new { q.Id } });
    }

    // ── PUT /api/admin/questoes/{id} ──────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarQuestaoRequest req)
    {
        var q = await db.Questoes.FindAsync(id);
        if (q == null) return NotFound(new { success = false, message = "Questão não encontrada." });

        if (req.Materia        != null) q.Materia         = req.Materia;
        if (req.Enunciado      != null) q.Enunciado        = req.Enunciado.Trim();
        if (req.AlternativaA   != null) q.AlternativaA     = req.AlternativaA.Trim();
        if (req.AlternativaB   != null) q.AlternativaB     = req.AlternativaB.Trim();
        if (req.AlternativaC   != null) q.AlternativaC     = req.AlternativaC.Trim();
        if (req.AlternativaD   != null) q.AlternativaD     = req.AlternativaD.Trim();
        if (req.AlternativaE   != null) q.AlternativaE     = req.AlternativaE.Trim();
        if (req.RespostaCorreta!= null) q.RespostaCorreta  = req.RespostaCorreta.ToUpper();
        if (req.Dificuldade    != null) q.Dificuldade      = req.Dificuldade;

        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Questão atualizada." });
    }

    // ── DELETE /api/admin/questoes/{id} ───────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var q = await db.Questoes.FindAsync(id);
        if (q == null) return NotFound(new { success = false, message = "Questão não encontrada." });

        q.Ativo = false; // soft delete
        await db.SaveChangesAsync();
        return Ok(new { success = true, message = "Questão desativada." });
    }

    // ── GET /api/admin/questoes/stats ─────────────────────────────
    [HttpGet("stats")]
    public async Task<IActionResult> Estatisticas()
    {
        var stats = await db.Questoes
            .Where(q => q.Ativo)
            .GroupBy(q => q.Materia)
            .Select(g => new { materia = g.Key, total = g.Count() })
            .OrderByDescending(g => g.total)
            .ToListAsync();

        var porFonte = await db.Questoes
            .Where(q => q.Ativo)
            .GroupBy(q => new { q.Instituicao, q.Ano })
            .Select(g => new { g.Key.Instituicao, g.Key.Ano, total = g.Count() })
            .OrderBy(g => g.Instituicao)
            .ThenBy(g => g.Ano)
            .ToListAsync();

        return Ok(new
        {
            success    = true,
            total      = stats.Sum(s => s.total),
            por_materia= stats,
            por_fonte  = porFonte,
        });
    }
}


// ═══════════════════════════════════════════════════════════════════
//  QUESTÕES PÚBLICAS — Endpoint para o Simulado
// ═══════════════════════════════════════════════════════════════════

[ApiController, Route("api/questoes")]
public class QuestoesPublicasController(AppDbContext db) : ControllerBase
{
    // ── GET /api/questoes?materia=Matemática&limit=45 ─────────────
    /// <summary>
    /// Retorna questões aleatórias por matéria para o simulado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? materia = null,
        [FromQuery] int     limit   = 45)
    {
        var query = db.Questoes
            .Where(q => q.Ativo && !string.IsNullOrEmpty(q.AlternativaA))
            .AsQueryable();

        if (!string.IsNullOrEmpty(materia))
            query = query.Where(q => q.Materia.ToLower() == materia.ToLower());

        // Selecionar colunas relevantes (sem ID interno para o front)
        var questoes = await query
            .Select(q => new
            {
                q.Id,
                q.NumeroQuestao,
                q.Materia,
                q.Dificuldade,
                q.Enunciado,
                q.AlternativaA,
                q.AlternativaB,
                q.AlternativaC,
                q.AlternativaD,
                q.AlternativaE,
                q.RespostaCorreta,
                q.Ano,
                q.Instituicao,
                Caderno = q.Prova != null ? q.Prova.Periodo : "",
            })
            .ToListAsync();

        // Embaralhar em memória (SQLite não suporta ORDER BY RANDOM() eficientemente)
        var rng       = new Random();
        var embaral   = questoes.OrderBy(_ => rng.Next()).Take(limit).ToList();

        return Ok(new
        {
            success = true,
            materia = materia,
            total   = embaral.Count,
            data    = embaral,
        });
    }

    // ── GET /api/questoes/materias ────────────────────────────────
    [HttpGet("materias")]
    public async Task<IActionResult> ListarMaterias()
    {
        var materias = await db.Questoes
            .Where(q => q.Ativo)
            .GroupBy(q => q.Materia)
            .Select(g => new { materia = g.Key, total = g.Count() })
            .OrderBy(m => m.materia)
            .ToListAsync();

        return Ok(new { success = true, data = materias });
    }
}
