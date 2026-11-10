using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PeliculasReviewApi.Validation;

namespace PeliculasReviewApi.Dtos.Peliculas;

/// <summary>
/// DTO de entrada para crear una película (<c>POST /api/peliculas</c>).
/// Equivalente a <c>CreatePeliculaDto</c> en NestJS.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class CreatePeliculaDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [HttpUrl]
    public string Imagen { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Sinopsis { get; set; }

    /// <summary>Año de estreno opcional (1888 = primera película que se conserva).</summary>
    [Range(1888, 2100)]
    public int? Estreno { get; set; }
}
