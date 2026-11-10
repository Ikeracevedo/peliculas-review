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
/// Los endpoints de lectura (<c>GET</c>) son públicos; los de escritura (<c>POST</c>, <c>PATCH</c>, <c>PUT</c>, <c>DELETE</c>)
/// exigen JWT (<c>[Authorize]</c>) y verifican que el usuario sea el autor o <c>ADMIN</c>.
/// </summary>
[ApiController]
[Route("api/reviews")]
[Produces("application/json")]
public class ReviewsController(IReviewsService reviewsService) : ControllerBase
{
    private const string MensajeSoloPropias = "Solo puedes editar o eliminar tus propias reseñas";
    private const string MensajePeliculaNoExiste = "No existe una pelicula con ese id";
    private const string MensajeResenaDuplicada = "Ya has publicado una reseña para esta pelicula";
    private const string MensajeTokenInvalido = "Token invalido o expirado";

    [HttpPost]
    [Authorize]
    [ProducesResponseType<ReviewResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
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

    /// <remarks>
    /// Soporta búsqueda (<c>nombre</c>, <c>search</c>, <c>titulo</c>, <c>peliculaId</c>, <c>autorId</c>)
    /// y paginación opcional (<c>page</c>, <c>limit</c> / <c>pageSize</c>).
    /// El cuerpo SIEMPRE es un array plano para mantener el contrato con el frontend de Vue;
    /// los metadatos de paginación viajan en headers <c>X-Total-Count</c>, <c>X-Page</c>, <c>X-Limit</c>, <c>X-Total-Pages</c>.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ReviewResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ReviewResponseDto>>> FindAll(
        [FromQuery] ReviewsQueryDto query,
        CancellationToken cancellationToken)
    {
        var resultado = await reviewsService.FindAllAsync(query, cancellationToken);

        Response.Headers["X-Total-Count"] = resultado.Total.ToString();
        if (resultado.Page is { } page && resultado.Limit is { } limit)
        {
            Response.Headers["X-Page"] = page.ToString();
            Response.Headers["X-Limit"] = limit.ToString();
            Response.Headers["X-Total-Pages"] = resultado.TotalPages?.ToString() ?? "0";
        }

        return Ok(resultado.Items);
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
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType<ReviewResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
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
    [Authorize]
    [ProducesResponseType<ReviewResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
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
