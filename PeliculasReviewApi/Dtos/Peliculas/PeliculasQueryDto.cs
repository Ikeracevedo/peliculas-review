using System.ComponentModel.DataAnnotations;

namespace PeliculasReviewApi.Dtos.Peliculas;

/// <summary>
/// Parámetros OPCIONALES de <c>GET /api/peliculas</c> (búsqueda por nombre y paginación).
/// NestJS no tenía estos parámetros: si no se envía ninguno, la respuesta es idéntica a la de
/// NestJS (array completo ordenado por <c>creadaEn</c> descendente), así el frontend de Vue no cambia.
/// </summary>
public class PeliculasQueryDto
{
    /// <summary>Texto a buscar dentro del nombre (contiene, sin distinguir mayúsculas ASCII).</summary>
    [StringLength(200)]
    public string? Nombre { get; set; }

    /// <summary>Alias opcional de <see cref="Nombre"/> para búsqueda por nombre.</summary>
    [StringLength(200)]
    public string? Search { get; set; }

    /// <summary>Página (base 1). Si se envía <c>page</c>, <c>limit</c> o <c>pageSize</c>, se pagina.</summary>
    [Range(1, int.MaxValue)]
    public int? Page { get; set; }

    /// <summary>Elementos por página (1–100).</summary>
    [Range(1, PeliculasQueryDefaults.MaxLimit)]
    public int? Limit { get; set; }

    /// <summary>Alias de <see cref="Limit"/>. Si llegan ambos, prevalece <c>limit</c>.</summary>
    [Range(1, PeliculasQueryDefaults.MaxLimit)]
    public int? PageSize { get; set; }

    /// <summary>Indica si el cliente pidió paginación explícitamente.</summary>
    public bool EsPaginada => Page.HasValue || Limit.HasValue || PageSize.HasValue;
}

public static class PeliculasQueryDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultLimit = 10;
    public const int MaxLimit = 100;
}
