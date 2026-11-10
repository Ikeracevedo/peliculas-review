using System.ComponentModel.DataAnnotations;

namespace PeliculasReviewApi.Models;

/// <summary>
/// Película sobre la que se escriben las reseñas. Equivale al modelo <c>Pelicula</c> de Prisma.
/// </summary>
public class Pelicula
{
    public int Id { get; set; }

    /// <summary>1–200 caracteres (CreatePeliculaDto en NestJS).</summary>
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>URL del póster.</summary>
    [Required]
    [Url]
    public string Imagen { get; set; } = string.Empty;

    /// <summary>Sinopsis breve opcional, máximo 2000 caracteres.</summary>
    [StringLength(2000)]
    public string? Sinopsis { get; set; }

    /// <summary>Año de estreno opcional. 1888 = primera película que se conserva.</summary>
    [Range(1888, 2100)]
    public int? Estreno { get; set; }

    // Ojo: en Prisma este campo se llama "creadaEn" (femenino), a diferencia de Usuario/Review.
    public DateTime CreadaEn { get; set; } = DateTime.UtcNow;

    /// <summary>Equivalente a <c>@updatedAt</c>; se refrescará en AppDbContext.SaveChanges.</summary>
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
