using LHubVestibular.Models;
using Microsoft.EntityFrameworkCore;

namespace LHubVestibular.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario>           Usuarios           { get; set; }
    public DbSet<Inscricao>         Inscricoes         { get; set; }
    public DbSet<Pagamento>         Pagamentos         { get; set; }
    public DbSet<Token>             Tokens             { get; set; }
    public DbSet<Cronograma>        Cronograma         { get; set; }
    public DbSet<Noticia>           Noticias           { get; set; }
    public DbSet<Prova>             Provas             { get; set; }
    public DbSet<Questao>           Questoes           { get; set; }
    public DbSet<SimuladoRealizado> SimuladosRealizados { get; set; }
    public DbSet<SimuladoResposta>  SimuladoRespostas  { get; set; }
    public DbSet<LocalProva>        LocaisProva        { get; set; }
    public DbSet<CandidatoLocal>    CandidatosLocais   { get; set; }
    public DbSet<Resultado>         Resultados         { get; set; }
    public DbSet<ComunicadoCandidato> Comunicados      { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Índices únicos
        mb.Entity<Usuario>().HasIndex(u => u.NumeroInscricao).IsUnique();
        mb.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<Inscricao>().HasIndex(i => i.NumeroInscricao).IsUnique();
        mb.Entity<Inscricao>().HasIndex(i => i.Cpf).IsUnique();
        mb.Entity<Token>().HasIndex(t => t.TokenValue).IsUnique();
        mb.Entity<Noticia>().HasIndex(n => n.Slug).IsUnique();

        // Índices compostos
        mb.Entity<Questao>().HasIndex(q => q.Materia);
        mb.Entity<Questao>().HasIndex(q => q.Dificuldade);
        mb.Entity<Questao>().HasIndex(q => q.Ano);
        mb.Entity<SimuladoRealizado>().HasIndex(s => s.UsuarioId);
        mb.Entity<SimuladoRealizado>().HasIndex(s => s.Materia);

        base.OnModelCreating(mb);
    }
}
