using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeliculasReviewApi.Dtos.Common;
using PeliculasReviewApi.Dtos.Peliculas;
using PeliculasReviewApi.Services;

namespace PeliculasReviewApi.Controllers;

/// <summary>
/// Equivalente a <c>PeliculasController</c> de NestJS (<c>@Controller('peliculas')</c> + prefijo global <c>api</c>).
/// Contrato:
/// <list type="bullet">
/// <item><c>POST   /api/peliculas</c>      → 201 + película creada | 400 | 401 (protegido con <c>[Authorize]</c>)</item>
/// <item><c>GET    /api/peliculas</c>      → 200 + array de películas (creadaEn desc) | 400 (lectura pública)</item>
/// <item><c>GET    /api/peliculas/{id}</c> → 200 + película | 400 | 404 (lectura pública)</item>
/// <item><c>PATCH  /api/peliculas/{id}</c> → 200 + película actualizada | 400 | 401 | 404 (protegido con <c>[Authorize]</c>)</item>
/// <item><c>PUT    /api/peliculas/{id}</c> → 200 + película actualizada | 400 | 401 | 404 (protegido con <c>[Authorize]</c>)</item>
/// <item><c>DELETE /api/peliculas/{id}</c> → 200 + película eliminada | 400 | 401 | 404 (protegido con <c>[Authorize]</c>)</item>
/// </list>
/// </summary>
/// <remarks>
/// El id se declara sin restricción <c>:int</c> a propósito: así un id no numérico produce 400
/// (como <c>ParseIntPipe</c>) en lugar de 404.
/// Los endpoints de lectura (<c>GET</c>) son públicos y los de escritura (<c>POST</c>, <c>PATCH</c>, <c>PUT</c>, <c>DELETE</c>)
/// llevan <c>[Authorize]</c>.
/// </remarks>
[ApiController]
[Route("api/peliculas")]
[Produces("application/json")]
public class PeliculasController(IPeliculasService peliculasService) : ControllerBase
{
    private const string MensajeNoExiste = "No existe una pelicula con ese id";

    [HttpPost]
    [Authorize]
    [ProducesResponseType<PeliculaResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PeliculaResponseDto>> Create(
        [FromBody] CreatePeliculaDto dto,
        CancellationToken cancellationToken)
    {
        var pelicula = await peliculasService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(FindOne), new { id = pelicula.Id }, pelicula);
    }

    /// <remarks>
    /// Sin query params devuelve exactamente lo mismo que NestJS. Opcionales: <c>nombre</c> / <c>search</c> (búsqueda),
    /// <c>page</c>, <c>limit</c> / <c>pageSize</c>. El cuerpo SIEMPRE es un array; los metadatos de
    /// paginación van en headers <c>X-Total-Count</c>, <c>X-Page</c>, <c>X-Limit</c>, <c>X-Total-Pages</c>.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PeliculaResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PeliculaResponseDto>>> FindAll(
        [FromQuery] PeliculasQueryDto query,
        CancellationToken cancellationToken)
    {
        var resultado = await peliculasService.FindAllAsync(query, cancellationToken);

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
    [ProducesResponseType<PeliculaResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PeliculaResponseDto>> FindOne(int id, CancellationToken cancellationToken)
    {
        var pelicula = await peliculasService.FindOneAsync(id, cancellationToken);
        return pelicula is null ? NotFound(ErrorResponseDto.NotFound(MensajeNoExiste)) : Ok(pelicula);
    }

    [HttpPatch("{id}")]
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType<PeliculaResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PeliculaResponseDto>> Update(
        int id,
        [FromBody] UpdatePeliculaDto dto,
        CancellationToken cancellationToken)
    {
        var pelicula = await peliculasService.UpdateAsync(id, dto, cancellationToken);
        return pelicula is null ? NotFound(ErrorResponseDto.NotFound(MensajeNoExiste)) : Ok(pelicula);
    }

    /// <remarks>NestJS devuelve 200 con el registro eliminado (no 204); se replica.</remarks>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType<PeliculaResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PeliculaResponseDto>> Remove(int id, CancellationToken cancellationToken)
    {
        var pelicula = await peliculasService.RemoveAsync(id, cancellationToken);
        return pelicula is null ? NotFound(ErrorResponseDto.NotFound(MensajeNoExiste)) : Ok(pelicula);
    }
}
