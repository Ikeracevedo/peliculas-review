using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeliculasReviewApi.Dtos.Auth;
using PeliculasReviewApi.Dtos.Common;
using PeliculasReviewApi.Services;

namespace PeliculasReviewApi.Controllers;

/// <summary>
/// Equivalente a <c>AuthController</c> de NestJS (<c>@Controller('auth')</c> + prefijo global <c>api</c>).
/// Contrato:
/// <list type="bullet">
/// <item><c>POST /api/auth/registro</c> → 201 + <c>{ usuario, access_token }</c> | 400 | 409</item>
/// <item><c>POST /api/auth/login</c>    → 200 + <c>{ usuario, access_token }</c> | 400 | 401</item>
/// </list>
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private const string MensajeCorreoDuplicado = "Ya existe una cuenta con ese correo";
    private const string MensajeCredencialesInvalidas = "Credenciales invalidas";

    [HttpPost("registro")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Registrar(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await authService.RegistrarAsync(dto, cancellationToken);
        if (resultado is null)
        {
            return Conflict(ErrorResponseDto.Conflict(MensajeCorreoDuplicado));
        }

        return StatusCode(StatusCodes.Status201Created, resultado);
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> IniciarSesion(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await authService.IniciarSesionAsync(dto, cancellationToken);
        if (resultado is null)
        {
            return Unauthorized(ErrorResponseDto.Unauthorized(MensajeCredencialesInvalidas));
        }

        return Ok(resultado);
    }
}
