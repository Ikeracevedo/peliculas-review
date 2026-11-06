using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PeliculasReviewApi.Dtos.Peliculas;

/// <summary>
/// DTO de entrada para actualizar parcialmente una película (<c>PATCH /api/peliculas/:id</c>).
/// Equivalente a <c>UpdatePeliculaDto = PartialType(CreatePeliculaDto)</c> en NestJS.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdatePeliculaDto
{
    [StringLength(200, MinimumLength = 1)]
    public string? Nombre { get; set; }

    [Url]
    public string? Imagen { get; set; }

    [StringLength(2000)]
    public string? Sinopsis { get; set; }

    [Range(1888, 2100)]
    public int? Estreno { get; set; }
}
