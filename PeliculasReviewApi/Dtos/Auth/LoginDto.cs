using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PeliculasReviewApi.Validation;

namespace PeliculasReviewApi.Dtos.Auth;

/// <summary>
/// DTO de entrada para inicio de sesión (<c>POST /api/auth/login</c>).
/// Equivalente a <c>LoginDto</c> en NestJS.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class LoginDto
{
    [Required]
    [StrictEmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
