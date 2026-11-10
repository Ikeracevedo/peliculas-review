using System.ComponentModel.DataAnnotations;

namespace PeliculasReviewApi.Models;

/// <summary>
/// Usuario que se registra y publica reseñas. Equivale al modelo <c>Usuario</c> de Prisma.
/// </summary>
public class Usuario
{
    public int Id { get; set; }

    /// <summary>Correo único (el índice único se configura en AppDbContext).</summary>
    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    /// <summary>Máximo 100 caracteres (RegisterDto / UpdateUsuarioDto en NestJS).</summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Contraseña hasheada con bcrypt. Nunca se expone en las respuestas.</summary>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>Todo registro público crea USUARIO. ADMIN solo se asigna manualmente.</summary>
    [EnumDataType(typeof(Rol))]
    public Rol Rol { get; set; } = Rol.USUARIO;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    /// <summary>Equivalente a <c>@updatedAt</c>; se refrescará en AppDbContext.SaveChanges.</summary>
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
