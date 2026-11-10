using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PeliculasReviewApi.Dtos.Reviews;

/// <summary>
/// DTO de entrada para crear una reseña (<c>POST /api/reviews</c>).
/// Equivalente a <c>CreateReviewDto</c> en NestJS.
/// <c>AutorId</c> no va en el DTO de entrada; se obtiene del claim <c>sub</c> del JWT.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class CreateReviewDto
{
    [Required]
    [StringLength(150, MinimumLength = 1)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(3000, MinimumLength = 1)]
    public string Contenido { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public int Calificacion { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int PeliculaId { get; set; }
}
