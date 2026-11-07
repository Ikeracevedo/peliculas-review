using PeliculasReviewApi.Dtos.Common;
using PeliculasReviewApi.Dtos.Reviews;
using PeliculasReviewApi.Models;

namespace PeliculasReviewApi.Services;

public readonly record struct UsuarioActualJwt(int Sub, Rol Rol)
{
    public bool EsAdmin => Rol == Rol.ADMIN;
}

public enum ReviewMutationStatus
{
    Ok,
    ReviewNotFound,
    PeliculaNotFound,
    UsuarioNotFound,
    Forbidden,
    DuplicateReview,
}

public readonly record struct ReviewMutationResult(
    ReviewMutationStatus Status,
    ReviewResponseDto? Review = null);

public interface IReviewsService
{
    Task<ReviewMutationResult> CreateAsync(
        CreateReviewDto dto,
        int autorId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ReviewResponseDto>> FindAllAsync(
        ReviewsQueryDto? query = null,
        CancellationToken cancellationToken = default);

    Task<ReviewResponseDto?> FindOneAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ReviewMutationResult> UpdateAsync(
        int id,
        UpdateReviewDto dto,
        UsuarioActualJwt usuarioActual,
        CancellationToken cancellationToken = default);

    Task<ReviewMutationResult> RemoveAsync(
        int id,
        UsuarioActualJwt usuarioActual,
        CancellationToken cancellationToken = default);
}
