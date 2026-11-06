using System.ComponentModel.DataAnnotations;

namespace PeliculasReviewApi.Models;

/// <summary>
/// Reseña de una película escrita por un usuario. Equivale al modelo <c>Review</c> de Prisma.
/// Un usuario solo puede reseñar una vez cada película (índice único configurado en AppDbContext).
/// </summary>
public class Review
{
    public int Id { get; set; }

    /// <summary>1–150 caracteres (CreateReviewDto en NestJS).</summary>
    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>1–3000 caracteres (CreateReviewDto en NestJS).</summary>
    [Required]
    [StringLength(3000, MinimumLength = 1)]
    public string Contenido { get; set; } = string.Empty;

    /// <summary>Calificación de 1 a 5.</summary>
    [Range(1, 5)]
    public int Calificacion { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    /// <summary>Equivalente a <c>@updatedAt</c>; se refrescará en AppDbContext.SaveChanges.</summary>
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

    // Relación con Pelicula (onDelete: Cascade, configurado en AppDbContext).
    public int PeliculaId { get; set; }
    public Pelicula Pelicula { get; set; } = null!;

    // Relación con Usuario autor (onDelete: Cascade, configurado en AppDbContext).
    public int AutorId { get; set; }
    public Usuario Autor { get; set; } = null!;
}
