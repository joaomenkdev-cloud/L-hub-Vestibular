using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LHubVestibular.Models;

// ── USUÁRIOS ──────────────────────────────────────────────────
public class Usuario
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)]  public string NumeroInscricao { get; set; } = "";
    [Required, MaxLength(150)] public string Email { get; set; } = "";
    [Required, MaxLength(255)] public string Nome { get; set; } = "";
    [Required, MaxLength(255)] public string SenhaHash { get; set; } = "";
    public bool PrimeiroAcesso { get; set; } = true;
    public DateTime? UltimoAcesso { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public bool Ativo { get; set; } = true;
}

// ── INSCRIÇÕES ────────────────────────────────────────────────
public class Inscricao
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)]  public string NumeroInscricao { get; set; } = "";
    public int? UsuarioId { get; set; }
    [Required, MaxLength(255)] public string Nome { get; set; } = "";
    [Required, MaxLength(14)]  public string Cpf { get; set; } = "";
    [MaxLength(20)]  public string? Rg { get; set; }
    public DateOnly? DataNascimento { get; set; }
    [MaxLength(20)]  public string? Sexo { get; set; }
    [Required, MaxLength(150)] public string Email { get; set; } = "";
    [Required, MaxLength(20)]  public string Telefone { get; set; } = "";
    [MaxLength(255)] public string? NomeMae { get; set; }
    [MaxLength(255)] public string? NomePai { get; set; }
    [MaxLength(10)]  public string? Cep { get; set; }
    [MaxLength(255)] public string? Rua { get; set; }
    [MaxLength(10)]  public string? NumeroEnd { get; set; }
    [MaxLength(100)] public string? Complemento { get; set; }
    [MaxLength(100)] public string? Bairro { get; set; }
    [MaxLength(100)] public string? Cidade { get; set; }
    [MaxLength(2)]   public string? Estado { get; set; }
    [Required, MaxLength(100)] public string Curso { get; set; } = "";
    [MaxLength(50)]  public string? Turno { get; set; }
    [MaxLength(50)]  public string? Modalidade { get; set; }
    public bool Cotas { get; set; } = false;
    [MaxLength(100)] public string? TipoCota { get; set; }
    [MaxLength(50)]  public string Status { get; set; } = "aguardando_pagamento";
    [Column(TypeName = "decimal(10,2)")] public decimal Valor { get; set; } = 85.00m;
    public DateTime DataInscricao { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UsuarioId))] public Usuario? Usuario { get; set; }
}

// ── PAGAMENTOS ────────────────────────────────────────────────
public class Pagamento
{
    [Key] public int Id { get; set; }
    public int InscricaoId { get; set; }
    [Required, MaxLength(50)] public string NumeroInscricao { get; set; } = "";
    [MaxLength(50)] public string Status { get; set; } = "pendente";
    public DateOnly Vencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal Valor { get; set; } = 85.00m;
    [MaxLength(100)] public string CodigoBarras { get; set; } = "34191.79001 01043.510047 91020.150008 1 89370000008500";
    [MaxLength(100)] public string LinhaDigitavel { get; set; } = "34191790010104351004791020150008189370000008500";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(InscricaoId))] public Inscricao? Inscricao { get; set; }
}

// ── TOKENS ────────────────────────────────────────────────────
public class Token
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(255)] public string TokenValue { get; set; } = "";
    public int UsuarioId { get; set; }
    [Required, MaxLength(50)] public string NumeroInscricao { get; set; } = "";
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiraEm { get; set; }
    public bool Ativo { get; set; } = true;

    [ForeignKey(nameof(UsuarioId))] public Usuario? Usuario { get; set; }
}

// ── CRONOGRAMA ────────────────────────────────────────────────
public class Cronograma
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(255)] public string Titulo { get; set; } = "";
    public string? Descricao { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public int Ordem { get; set; } = 0;
    public bool Ativo { get; set; } = true;
}

// ── NOTÍCIAS ──────────────────────────────────────────────────
public class Noticia
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(255)] public string Titulo { get; set; } = "";
    [MaxLength(255)] public string? Slug { get; set; }
    [Required] public string Corpo { get; set; } = "";
    public string? Resumo { get; set; }
    [MaxLength(50)]  public string Categoria { get; set; } = "noticia";
    [MaxLength(50)]  public string BadgeTipo { get; set; } = "novo";
    public bool Publicado { get; set; } = true;
    public bool Destaque { get; set; } = false;
    [MaxLength(255)] public string? ArquivoUrl { get; set; }
    [MaxLength(100)] public string? ArquivoNome { get; set; }
    [MaxLength(100)] public string Autor { get; set; } = "Comissão do Vestibular";
    public int Views { get; set; } = 0;
    public DateTime PublicadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}

// ── PROVAS ────────────────────────────────────────────────────
public class Prova
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(255)] public string Titulo { get; set; } = "";
    public int Ano { get; set; }
    [Required, MaxLength(50)] public string Periodo { get; set; } = "";
    [MaxLength(100)] public string Instituicao { get; set; } = "L-Hub";
    [MaxLength(100)] public string Area { get; set; } = "geral";
    public int Questoes { get; set; } = 60;
    public int DuracaoHoras { get; set; } = 4;
    [MaxLength(255)] public string? ProvaUrl { get; set; }
    [MaxLength(255)] public string? GabaritoUrl { get; set; }
    [MaxLength(50)]  public string? Fonte { get; set; }
    [MaxLength(150)] public string? FonteNome { get; set; }
    public int Downloads { get; set; } = 0;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<Questao> QuestoesNav { get; set; } = new List<Questao>();
}

// ── QUESTÕES ──────────────────────────────────────────────────
public class Questao
{
    [Key] public int Id { get; set; }
    public int ProvaId { get; set; }
    public int NumeroQuestao { get; set; }
    [Required, MaxLength(50)]  public string Materia { get; set; } = "";
    [Required] public string Enunciado { get; set; } = "";
    public string? AlternativaA { get; set; }
    public string? AlternativaB { get; set; }
    public string? AlternativaC { get; set; }
    public string? AlternativaD { get; set; }
    public string? AlternativaE { get; set; }
    [Required, MaxLength(1)] public string RespostaCorreta { get; set; } = "A";
    [MaxLength(20)] public string Dificuldade { get; set; } = "Médio";
    public int Ano { get; set; }
    [MaxLength(100)] public string? Instituicao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ProvaId))] public Prova? Prova { get; set; }
}

// ── SIMULADOS REALIZADOS ──────────────────────────────────────
public class SimuladoRealizado
{
    [Key] public int Id { get; set; }
    public int UsuarioId { get; set; }
    [Required, MaxLength(50)] public string Materia { get; set; } = "";
    public int TotalQuestoes { get; set; }
    public int Acertos { get; set; } = 0;
    [Column(TypeName = "decimal(5,2)")] public decimal? Nota { get; set; }
    public int? TempoGastoMin { get; set; }
    public bool Finalizado { get; set; } = false;
    public DateTime IniciadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? FinalizadoEm { get; set; }

    [ForeignKey(nameof(UsuarioId))] public Usuario? Usuario { get; set; }
    public ICollection<SimuladoResposta> Respostas { get; set; } = new List<SimuladoResposta>();
}

// ── RESPOSTAS DO SIMULADO ─────────────────────────────────────
public class SimuladoResposta
{
    [Key] public int Id { get; set; }
    public int SimuladoId { get; set; }
    public int QuestaoId { get; set; }
    [MaxLength(1)] public string? RespostaUsuario { get; set; }
    public bool Correta { get; set; } = false;
    public int? TempoRespostaSeg { get; set; }

    [ForeignKey(nameof(SimuladoId))] public SimuladoRealizado? Simulado { get; set; }
    [ForeignKey(nameof(QuestaoId))]  public Questao? Questao { get; set; }
}

// ── LOCAIS DE PROVA ───────────────────────────────────────────
public class LocalProva
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(255)] public string Nome { get; set; } = "";
    [MaxLength(255)] public string? Endereco { get; set; }
    [MaxLength(100)] public string? Bairro { get; set; }
    [MaxLength(100)] public string? Cidade { get; set; }
    [MaxLength(2)]   public string? Estado { get; set; }
    [MaxLength(10)]  public string? Cep { get; set; }
    public int? Capacidade { get; set; }
    public bool Ativo { get; set; } = true;
}

// ── CANDIDATO LOCAL ───────────────────────────────────────────
public class CandidatoLocal
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string NumeroInscricao { get; set; } = "";
    public int? LocalId { get; set; }
    [MaxLength(50)] public string? Sala { get; set; }
    public int? NumeroMesa { get; set; }
    public bool Liberado { get; set; } = false;
    public DateTime? LiberadoEm { get; set; }

    [ForeignKey(nameof(LocalId))] public LocalProva? Local { get; set; }
}

// ── RESULTADOS ────────────────────────────────────────────────
public class Resultado
{
    [Key] public int Id { get; set; }
    [Required, MaxLength(50)] public string NumeroInscricao { get; set; } = "";
    [Column(TypeName = "decimal(5,2)")] public decimal? NotaTotal { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? NotaLp { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? NotaCh { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? NotaCn { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? NotaMt { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal? NotaRedacao { get; set; }
    public int? Classificacao { get; set; }
    [MaxLength(50)] public string Status { get; set; } = "aguardando";
    public bool Publicado { get; set; } = false;
    public DateTime? PublicadoEm { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

// ── COMUNICADOS AO CANDIDATO ──────────────────────────────────
public class ComunicadoCandidato
{
    [Key] public int Id { get; set; }
    [MaxLength(50)] public string? NumeroInscricao { get; set; }
    [Required, MaxLength(255)] public string Titulo { get; set; } = "";
    [Required] public string Corpo { get; set; } = "";
    public bool Lido { get; set; } = false;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}