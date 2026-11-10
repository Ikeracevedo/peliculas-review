using PeliculasReviewApi.Models;

namespace PeliculasReviewApi.Dtos.Usuarios;

/// <summary>
/// DTO de salida para los endpoints de administración de usuarios (<c>/api/usuarios</c>).
/// Equivalente a la proyección <c>SELECT_PUBLICO</c> en <c>UsuariosService</c> de NestJS.
/// Nunca expone <c>Password</c>.
/// </summary>
public class UsuarioResponseDto
{
    public int Id { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public Rol Rol { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
}
