using LHubVestibular.Data;
using LHubVestibular.Models;
using Microsoft.EntityFrameworkCore;

namespace LHubVestibular.Services;

public class InscricaoService(AppDbContext db)
{
    public static string GerarNumeroInscricao()
    {
        var hex = Convert.ToHexString(Guid.NewGuid().ToByteArray()[..3]).ToUpper();
        return $"LH{DateTime.UtcNow:yyMMdd}{hex}";
    }

    public async Task<bool> CpfJaExisteAsync(string cpf) =>
        await db.Inscricoes.AnyAsync(i => i.Cpf == cpf);

    public async Task<Inscricao?> BuscarPorNumeroAsync(string numero) =>
        await db.Inscricoes.FirstOrDefaultAsync(i => i.NumeroInscricao == numero);

    public async Task<Pagamento?> BuscarPagamentoPorNumeroAsync(string numero) =>
        await db.Pagamentos.FirstOrDefaultAsync(p => p.NumeroInscricao == numero);

    public static string CalcStatus(DateOnly inicio, DateOnly fim)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        if (hoje > fim)    return "concluido";
        if (hoje >= inicio) return "em_andamento";
        return "futuro";
    }
}