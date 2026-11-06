using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PeliculasReviewApi.Models;

namespace PeliculasReviewApi.Dtos.Usuarios;

/// <summary>
/// DTO de entrada para que un ADMIN actualice parcialmente un usuario (<c>PATCH /api/usuarios/:id</c>).
/// Equivalente a <c>UpdateUsuarioDto</c> en NestJS. No incluye <c>password</c> por diseño.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateUsuarioDto
{
    [StringLength(100, MinimumLength = 1)]
    public string? Nombre { get; set; }

    [EmailAddress]
    public string? Correo { get; set; }

    [EnumDataType(typeof(Rol), ErrorMessage = "rol debe ser USUARIO o ADMIN")]
    public Rol? Rol { get; set; }
}
