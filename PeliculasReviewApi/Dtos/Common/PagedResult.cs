namespace PeliculasReviewApi.Dtos.Common;

/// <summary>
/// Resultado interno de un listado. El controlador solo serializa <see cref="Items"/> (array plano,
/// como NestJS); los metadatos de paginación viajan en headers <c>X-Total-Count</c>, <c>X-Page</c>,
/// <c>X-Limit</c> y <c>X-Total-Pages</c> para no romper el contrato del cuerpo.
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int? Page, int? Limit)
{
    public int? TotalPages => Limit is > 0 ? (int)Math.Ceiling(Total / (double)Limit.Value) : null;
}
