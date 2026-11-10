using PeliculasReviewApi.Dtos.Common;
using PeliculasReviewApi.Dtos.Peliculas;

namespace PeliculasReviewApi.Services;

/// <summary>
/// Lógica CRUD de películas. Equivalente a <c>PeliculasService</c> de NestJS, más búsqueda por
/// nombre y paginación opcionales. Los métodos que reciben un id devuelven <c>null</c> cuando la
/// película no existe; el controlador lo traduce a 404.
/// </summary>
public interface IPeliculasService
{
    /// <summary>Lista ordenada por <c>creadaEn</c> descendente, filtrada/paginada si se pide.</summary>
    Task<PagedResult<PeliculaResponseDto>> FindAllAsync(PeliculasQueryDto query, CancellationToken cancellationToken = default);

    Task<PeliculaResponseDto?> FindOneAsync(int id, CancellationToken cancellationToken = default);

    Task<PeliculaResponseDto> CreateAsync(CreatePeliculaDto dto, CancellationToken cancellationToken = default);

    Task<PeliculaResponseDto?> UpdateAsync(int id, UpdatePeliculaDto dto, CancellationToken cancellationToken = default);

    /// <summary>Elimina y devuelve la película eliminada (como <c>prisma.pelicula.delete</c>).</summary>
    Task<PeliculaResponseDto?> RemoveAsync(int id, CancellationToken cancellationToken = default);
}
