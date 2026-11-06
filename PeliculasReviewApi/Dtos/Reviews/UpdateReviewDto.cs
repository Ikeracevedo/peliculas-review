using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PeliculasReviewApi.Dtos.Reviews;

/// <summary>
/// DTO de entrada para actualizar parcialmente una reseña (<c>PATCH /api/reviews/:id</c>).
/// Equivalente a <c>UpdateReviewDto = PartialType(CreateReviewDto)</c> en NestJS.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateReviewDto
{
    [StringLength(150, MinimumLength = 1)]
    public string? Titulo { get; set; }

    [StringLength(3000, MinimumLength = 1)]
    public string? Contenido { get; set; }

    [Range(1, 5)]
    public int? Calificacion { get; set; }

    [Range(1, int.MaxValue)]
    public int? PeliculaId { get; set; }
}
