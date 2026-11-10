using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PeliculasReviewApi.Dtos.Peliculas;

namespace PeliculasReviewApi.Dtos.Reviews;

/// <summary>
/// Parámetros OPCIONALES de <c>GET /api/reviews</c> (búsqueda y paginación).
/// Si no se envía ninguno, devuelve todas las reseñas ordenadas por <c>creadoEn</c> descendente,
/// manteniendo compatibilidad total con el cliente de Vue.
/// </summary>
public class ReviewsQueryDto
{
    /// <summary>
    /// Texto a buscar en el título de la reseña, contenido, nombre de la película o nombre del autor.
    /// </summary>
    [FromQuery(Name = "nombre")]
    [StringLength(200)]
    public string? Nombre { get; set; }

    /// <summary>Alias opcional de <see cref="Nombre"/> para búsqueda general.</summary>
    [FromQuery(Name = "search")]
    [StringLength(200)]
    public string? Search { get; set; }

    /// <summary>Filtro opcional por título de la reseña.</summary>
    [FromQuery(Name = "titulo")]
    [StringLength(200)]
    public string? Titulo { get; set; }

    /// <summary>Filtro opcional por ID de película.</summary>
    [FromQuery(Name = "peliculaId")]
    [Range(1, int.MaxValue)]
    public int? PeliculaId { get; set; }

    /// <summary>Filtro opcional por ID de autor.</summary>
    [FromQuery(Name = "autorId")]
    [Range(1, int.MaxValue)]
    public int? AutorId { get; set; }

    /// <summary>Página (base 1). Si se envía <c>page</c>, <c>limit</c> o <c>pageSize</c>, se pagina.</summary>
    [FromQuery(Name = "page")]
    [Range(1, int.MaxValue)]
    public int? Page { get; set; }

    /// <summary>Elementos por página (1–100).</summary>
    [FromQuery(Name = "limit")]
    [Range(1, PeliculasQueryDefaults.MaxLimit)]
    public int? Limit { get; set; }

    /// <summary>Alias de <see cref="Limit"/>. Si llegan ambos, prevalece <c>limit</c>.</summary>
    [FromQuery(Name = "pageSize")]
    [Range(1, PeliculasQueryDefaults.MaxLimit)]
    public int? PageSize { get; set; }

    /// <summary>Indica si el cliente pidió paginación explícitamente.</summary>
    [BindNever]
    [JsonIgnore]
    public bool EsPaginada => Page.HasValue || Limit.HasValue || PageSize.HasValue;
}
