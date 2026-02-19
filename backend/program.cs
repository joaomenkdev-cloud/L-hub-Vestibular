using LHubVestibular.Data;
using LHubVestibular.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Banco de dados SQLite ──────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=lhub.db"));

// ── CORS ───────────────────────────────────────────────────────
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ── HttpClient para download de PDFs (PdfImportService) ───────
builder.Services.AddHttpClient("PdfImport", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
    client.DefaultRequestHeaders.Add("User-Agent",
        "Mozilla/5.0 (compatible; LHubVestibular/1.0; +https://lhub.com.br)");
    client.DefaultRequestHeaders.Add("Accept", "application/pdf,*/*");
});

// ── Serviços ───────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<InscricaoService>();
builder.Services.AddScoped<PdfImportService>();   // importação de PDFs

builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "L-Hub Vestibular API", Version = "v1",
        Description = "API para gestão do processo seletivo e simulados." });
});

var app = builder.Build();

// ── Criar/migrar BD e popular dados iniciais ──────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    await DbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "L-Hub API v1"));
}

app.UseCors();

// ── Servir arquivos estáticos (HTML, CSS, JS) ─────────────────
// Aponta para a pasta pai onde ficam as pastas html/, css/, js/
var siteFolder = Path.Combine(Directory.GetCurrentDirectory(), "..");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.GetFullPath(siteFolder)),
    RequestPath = ""
});

// Redirecionar / para /html/index.html
app.MapGet("/", () => Results.Redirect("/html/index.html"));

app.MapControllers();

Console.WriteLine("\n╔══════════════════════════════════════════════╗");
Console.WriteLine("║       🎓  L-Hub API  v3.1.0                 ║");
Console.WriteLine("╠══════════════════════════════════════════════╣");
Console.WriteLine("║  URL:    http://localhost:5000               ║");
Console.WriteLine("║  BD:     SQLite (lhub.db)                    ║");
Console.WriteLine("║  Docs:   http://localhost:5000/swagger       ║");
Console.WriteLine("╠══════════════════════════════════════════════╣");
Console.WriteLine("║  IMPORTAR PROVAS:                            ║");
Console.WriteLine("║  POST /api/admin/importacao/todas            ║");
Console.WriteLine("║  POST /api/admin/importacao/url              ║");
Console.WriteLine("╚══════════════════════════════════════════════╝\n");

app.Run("http://0.0.0.0:5000");
