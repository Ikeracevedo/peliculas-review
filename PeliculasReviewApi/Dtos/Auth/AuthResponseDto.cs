using System.Text.Json.Serialization;
using PeliculasReviewApi.Models;

namespace PeliculasReviewApi.Dtos.Auth;

/// <summary>
/// DTO de salida para <c>POST /api/auth/registro</c> y <c>POST /api/auth/login</c>.
/// Replica exactamente la respuesta de <c>AuthService.firmarToken</c> en NestJS.
/// </summary>
public class AuthResponseDto
{
    public UsuarioPublicoDto Usuario { get; set; } = null!;

    /// <summary>
    /// En NestJS esta propiedad se serializa en snake_case (<c>access_token</c>).
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}

/// <summary>
/// Proyección pública del usuario incluida dentro de <c>AuthResponseDto</c>
/// (<c>{ id, correo, nombre, rol }</c>).
/// </summary>
public class UsuarioPublicoDto
{
    public int Id { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public Rol Rol { get; set; }
}
