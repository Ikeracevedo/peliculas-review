using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PeliculasReviewApi.Validation;

namespace PeliculasReviewApi.Dtos.Reviews;

/// <summary>
/// DTO de entrada para actualizar parcialmente una reseña (<c>PATCH /api/reviews/:id</c>).
/// Equivalente a <c>UpdateReviewDto = PartialType(CreateReviewDto)</c> en NestJS.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateReviewDto
{
    private string? _titulo;
    private string? _contenido;
    private int? _calificacion;
    private int? _peliculaId;

    [RequiredWhenPresent(nameof(TituloEnviado))]
    [StringLength(150, MinimumLength = 1)]
    public string? Titulo
    {
        get => _titulo;
        set { _titulo = value; TituloEnviado = true; }
    }

    [RequiredWhenPresent(nameof(ContenidoEnviado))]
    [StringLength(3000, MinimumLength = 1)]
    public string? Contenido
    {
        get => _contenido;
        set { _contenido = value; ContenidoEnviado = true; }
    }

    [RequiredWhenPresent(nameof(CalificacionEnviada))]
    [Range(1, 5)]
    public int? Calificacion
    {
        get => _calificacion;
        set { _calificacion = value; CalificacionEnviada = true; }
    }

    [RequiredWhenPresent(nameof(PeliculaIdEnviado))]
    [Range(1, int.MaxValue)]
    public int? PeliculaId
    {
        get => _peliculaId;
        set { _peliculaId = value; PeliculaIdEnviado = true; }
    }

    [JsonIgnore] public bool TituloEnviado { get; private set; }
    [JsonIgnore] public bool ContenidoEnviado { get; private set; }
    [JsonIgnore] public bool CalificacionEnviada { get; private set; }
    [JsonIgnore] public bool PeliculaIdEnviado { get; private set; }
}
