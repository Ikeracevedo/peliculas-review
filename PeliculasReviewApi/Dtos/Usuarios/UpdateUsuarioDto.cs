using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PeliculasReviewApi.Models;
using PeliculasReviewApi.Validation;

namespace PeliculasReviewApi.Dtos.Usuarios;

/// <summary>
/// DTO de entrada para que un ADMIN actualice parcialmente un usuario (<c>PATCH /api/usuarios/:id</c>).
/// Equivalente a <c>UpdateUsuarioDto</c> en NestJS. No incluye <c>password</c> por diseño.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateUsuarioDto
{
    private string? _nombre;
    private string? _correo;
    private Rol? _rol;

    [RequiredWhenPresent(nameof(NombreEnviado))]
    [StringLength(100, MinimumLength = 1)]
    public string? Nombre
    {
        get => _nombre;
        set { _nombre = value; NombreEnviado = true; }
    }

    [RequiredWhenPresent(nameof(CorreoEnviado))]
    [StrictEmailAddress]
    public string? Correo
    {
        get => _correo;
        set { _correo = value; CorreoEnviado = true; }
    }

    [RequiredWhenPresent(nameof(RolEnviado), ErrorMessage = "rol debe ser USUARIO o ADMIN")]
    [EnumDataType(typeof(Rol), ErrorMessage = "rol debe ser USUARIO o ADMIN")]
    public Rol? Rol
    {
        get => _rol;
        set { _rol = value; RolEnviado = true; }
    }

    [JsonIgnore] public bool NombreEnviado { get; private set; }
    [JsonIgnore] public bool CorreoEnviado { get; private set; }
    [JsonIgnore] public bool RolEnviado { get; private set; }
}
