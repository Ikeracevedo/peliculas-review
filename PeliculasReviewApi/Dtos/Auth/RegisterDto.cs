using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PeliculasReviewApi.Dtos.Auth;

/// <summary>
/// DTO de entrada para registro público (<c>POST /api/auth/registro</c>).
/// Equivalente a <c>RegisterDto</c> en NestJS. No incluye <c>rol</c>: todo registro crea <c>USUARIO</c>.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Entre 8 y 72 caracteres (bcrypt trunca después de 72 bytes).</summary>
    [Required]
    [StringLength(72, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
