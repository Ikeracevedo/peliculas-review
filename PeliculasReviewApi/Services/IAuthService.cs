using PeliculasReviewApi.Dtos.Auth;

namespace PeliculasReviewApi.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegistrarAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto?> IniciarSesionAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
