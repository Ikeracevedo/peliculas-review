using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PeliculasReviewApi.Data;
using PeliculasReviewApi.Dtos.Auth;
using PeliculasReviewApi.Models;

namespace PeliculasReviewApi.Services;

/// <summary>
/// Equivalente a <c>AuthService</c> en NestJS (<c>backend/src/auth/auth.service.ts</c>).
/// Hashea contraseñas con BCrypt (workFactor = 10) y emite tokens JWT firmados con HMAC-SHA256.
/// </summary>
public class AuthService(AppDbContext context, IConfiguration configuration) : IAuthService
{
    private const int SaltRounds = 10;
    private const int DefaultExpiresInSeconds = 3600;

    public async Task<AuthResponseDto?> RegistrarAsync(
        RegisterDto dto,
        CancellationToken cancellationToken = default)
    {
        var existe = await context.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.Correo == dto.Correo, cancellationToken);

        if (existe)
        {
            return null;
        }

        var passwordHasheada = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: SaltRounds);

        // Todo registro público crea un usuario con rol USUARIO (default del esquema).
        var usuario = new Usuario
        {
            Correo = dto.Correo,
            Nombre = dto.Nombre,
            Password = passwordHasheada,
            Rol = Rol.USUARIO,
        };

        context.Usuarios.Add(usuario);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await context.Usuarios.AsNoTracking().AnyAsync(u => u.Correo == dto.Correo, cancellationToken))
            {
                return null;
            }

            throw;
        }

        return FirmarToken(usuario);
    }

    public async Task<AuthResponseDto?> IniciarSesionAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var usuario = await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Correo == dto.Correo, cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        bool passwordValida;
        try
        {
            passwordValida = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.Password);
        }
        catch
        {
            passwordValida = false;
        }

        if (!passwordValida)
        {
            return null;
        }

        return FirmarToken(usuario);
    }

    private AuthResponseDto FirmarToken(Usuario usuario)
    {
        var secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("No se configuró Jwt:SecretKey en appsettings.json.");
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var expiresInSeconds = configuration.GetValue<int?>("Jwt:ExpiresInSeconds") is > 0 and var segundos
            ? segundos
            : DefaultExpiresInSeconds;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var ahora = DateTime.UtcNow;
        var iat = EpochTime.GetIntDate(ahora).ToString(CultureInfo.InvariantCulture);

        var claims = new Claim[]
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer32),
            new("correo", usuario.Correo),
            new("rol", usuario.Rol.ToString()),
            new(JwtRegisteredClaimNames.Iat, iat, ClaimValueTypes.Integer64),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: ahora,
            expires: ahora.AddSeconds(expiresInSeconds),
            signingCredentials: credentials);

        return new AuthResponseDto
        {
            Usuario = new UsuarioPublicoDto
            {
                Id = usuario.Id,
                Correo = usuario.Correo,
                Nombre = usuario.Nombre,
                Rol = usuario.Rol,
            },
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
        };
    }
}
