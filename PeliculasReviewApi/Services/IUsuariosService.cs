using PeliculasReviewApi.Dtos.Usuarios;

namespace PeliculasReviewApi.Services;

public enum UpdateUsuarioStatus
{
    Ok,
    NotFound,
    EmailConflict,
}

public readonly record struct UpdateUsuarioResult(
    UpdateUsuarioStatus Status,
    UsuarioResponseDto? Usuario = null);

public interface IUsuariosService
{
    Task<IReadOnlyList<UsuarioResponseDto>> FindAllAsync(CancellationToken cancellationToken = default);
    Task<UsuarioResponseDto?> FindOneAsync(int id, CancellationToken cancellationToken = default);
    Task<UpdateUsuarioResult> UpdateAsync(int id, UpdateUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioResponseDto?> RemoveAsync(int id, CancellationToken cancellationToken = default);
}
