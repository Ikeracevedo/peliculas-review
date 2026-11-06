using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PeliculasReviewApi.Validation;

namespace PeliculasReviewApi.Dtos.Auth;

/// <summary>
/// DTO de entrada para registro público (<c>POST /api/auth/registro</c>).
/// Equivalente a <c>RegisterDto</c> en NestJS. No incluye <c>rol</c>: todo registro crea <c>USUARIO</c>.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class RegisterDto
{
    [Required]
    [StrictEmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Entre 8 y 72 caracteres / 72 bytes UTF-8 (bcrypt trunca después de 72 bytes).</summary>
    [Required]
    [StringLength(72, MinimumLength = 8)]
    [MaxUtf8ByteLength(72)]
    public string Password { get; set; } = string.Empty;
}
