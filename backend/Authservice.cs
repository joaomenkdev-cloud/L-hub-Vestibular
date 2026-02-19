using System.Security.Cryptography;
using System.Text;
using LHubVestibular.Data;
using LHubVestibular.Models;
using Microsoft.EntityFrameworkCore;

namespace LHubVestibular.Services;

public class AuthService(AppDbContext db)
{
    public static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToHexString(bytes).ToLower();
    }

    public static string GerarToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(30))
               .Replace("+", "-").Replace("/", "_").Replace("=", "");

    public async Task<Usuario?> ValidarTokenAsync(string token)
    {
        var t = await db.Tokens
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.TokenValue == token && x.Ativo);
        return t?.Usuario?.Ativo == true ? t.Usuario : null;
    }

    public async Task<Token> CriarTokenAsync(int usuarioId, string numeroInscricao)
    {
        var token = new Token
        {
            TokenValue = GerarToken(),
            UsuarioId = usuarioId,
            NumeroInscricao = numeroInscricao,
            ExpiraEm = DateTime.UtcNow.AddDays(7)
        };
        db.Tokens.Add(token);
        await db.SaveChangesAsync();
        return token;
    }

    public async Task RevogarTokenAsync(string tokenValue)
    {
        var token = await db.Tokens.FirstOrDefaultAsync(t => t.TokenValue == tokenValue);
        if (token != null) { token.Ativo = false; await db.SaveChangesAsync(); }
    }
}