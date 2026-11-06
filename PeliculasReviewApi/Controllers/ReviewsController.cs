using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeliculasReviewApi.Dtos.Common;
using PeliculasReviewApi.Dtos.Reviews;
using PeliculasReviewApi.Models;
using PeliculasReviewApi.Services;

namespace PeliculasReviewApi.Controllers;

/// <summary>
/// Equivalente a <c>ReviewsController</c> de NestJS (<c>@Controller('reviews')</c> + prefijo global <c>api</c>).
/// Protegido con JWT; <c>PATCH</c> y <c>DELETE</c> verifican que el usuario sea el autor o <c>ADMIN</c>.
/// </summary>
[ApiController]
[Route("api/reviews")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
public class ReviewsController(IReviewsService reviewsService) : ControllerBase
{
    private const string MensajeSoloPropias = "Solo puedes editar o eliminar tus propias reseñas";
    private const string MensajePeliculaNoExiste = "No existe una pelicula con ese id";
    private const string MensajeResenaDuplicada = "Ya has publicado una reseña para esta pelicula";
    private const string MensajeTokenInvalido = "Token invalido o expirado";

    [HttpPost]
    [ProducesResponseType<ReviewResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewResponseDto>> Create(
        [FromBody] CreateReviewDto dto,
        CancellationToken cancellationToken)
    {
        if (!TryExtraerUsuarioActual(out var usuarioActual))
        {
            return Unauthorized(ErrorResponseDto.Unauthorized(MensajeTokenInvalido));
        }

        var resultado = await reviewsService.CreateAsync(dto, usuarioActual.Sub, cancellationToken);
        return resultado.Status switch
        {
            ReviewMutationStatus.UsuarioNotFound => Unauthorized(ErrorResponseDto.Unauthorized(MensajeTokenInvalido)),
            ReviewMutationStatus.PeliculaNotFound => NotFound(ErrorResponseDto.NotFound(MensajePeliculaNoExiste)),
            ReviewMutationStatus.DuplicateReview => Conflict(ErrorResponseDto.Conflict(MensajeResenaDuplicada)),
            _ => CreatedAtAction(nameof(FindOne), new { id = resultado.Review!.Id }, resultado.Review),
        };
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ReviewResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReviewResponseDto>>> FindAll(
        CancellationToken cancellationToken)
    {
        var reviews = await reviewsService.FindAllAsync(cancellationToken);
        return Ok(reviews);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<ReviewResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponseDto>> FindOne(
        int id,
        CancellationToken cancellationToken)
    {
        var review = await reviewsService.FindOneAsync(id, cancellationToken);
        return review is null
            ? NotFound(ErrorResponseDto.NotFound(MensajeNoExiste(id)))
            : Ok(review);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType<ReviewResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReviewResponseDto>> Update(
        int id,
        [FromBody] UpdateReviewDto dto,
        CancellationToken cancellationToken)
    {
        if (!TryExtraerUsuarioActual(out var usuarioActual))
        {
            return Unauthorized(ErrorResponseDto.Unauthorized(MensajeTokenInvalido));
        }

        var resultado = await reviewsService.UpdateAsync(id, dto, usuarioActual, cancellationToken);
        return resultado.Status switch
        {
            ReviewMutationStatus.ReviewNotFound => NotFound(ErrorResponseDto.NotFound(MensajeNoExiste(id))),
            ReviewMutationStatus.PeliculaNotFound => NotFound(ErrorResponseDto.NotFound(MensajePeliculaNoExiste)),
            ReviewMutationStatus.Forbidden => StatusCode(
                StatusCodes.Status403Forbidden,
                ErrorResponseDto.Forbidden(MensajeSoloPropias)),
            ReviewMutationStatus.DuplicateReview => Conflict(ErrorResponseDto.Conflict(MensajeResenaDuplicada)),
            _ => Ok(resultado.Review),
        };
    }

    [HttpDelete("{id}")]
    [ProducesResponseType<ReviewResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewResponseDto>> Remove(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryExtraerUsuarioActual(out var usuarioActual))
        {
            return Unauthorized(ErrorResponseDto.Unauthorized(MensajeTokenInvalido));
        }

        var resultado = await reviewsService.RemoveAsync(id, usuarioActual, cancellationToken);
        return resultado.Status switch
        {
            ReviewMutationStatus.ReviewNotFound => NotFound(ErrorResponseDto.NotFound(MensajeNoExiste(id))),
            ReviewMutationStatus.Forbidden => StatusCode(
                StatusCodes.Status403Forbidden,
                ErrorResponseDto.Forbidden(MensajeSoloPropias)),
            _ => Ok(resultado.Review),
        };
    }

    private bool TryExtraerUsuarioActual(out UsuarioActualJwt usuarioActual)
    {
        var subValor = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(subValor, out var sub) || sub <= 0)
        {
            usuarioActual = default;
            return false;
        }

        var rolValor = User.FindFirstValue("rol")
            ?? User.FindFirstValue(ClaimTypes.Role);

        var rol = Enum.TryParse<Rol>(rolValor, ignoreCase: false, out var rolParseado)
            ? rolParseado
            : Rol.USUARIO;

        usuarioActual = new UsuarioActualJwt(sub, rol);
        return true;
    }

    private static string MensajeNoExiste(int id) => $"No existe una reseña con id {id}";
}
