using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeliculasReviewApi.Dtos.Common;
using PeliculasReviewApi.Dtos.Usuarios;
using PeliculasReviewApi.Models;
using PeliculasReviewApi.Services;

namespace PeliculasReviewApi.Controllers;

/// <summary>
/// Equivalente a <c>UsuariosController</c> de NestJS (<c>@Controller('usuarios')</c> + <c>@UseGuards(RolesGuard) @Roles('ADMIN')</c>).
/// Exige JWT válido con rol <c>ADMIN</c> en todas sus rutas.
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = nameof(Rol.ADMIN))]
[Produces("application/json")]
[ProducesResponseType<ErrorResponseDto>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ErrorResponseDto>(StatusCodes.Status403Forbidden)]
public class UsuariosController(IUsuariosService usuariosService) : ControllerBase
{
    private const string MensajeCorreoDuplicado = "Ya existe una cuenta con ese correo";

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UsuarioResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UsuarioResponseDto>>> FindAll(
        CancellationToken cancellationToken)
    {
        var usuarios = await usuariosService.FindAllAsync(cancellationToken);
        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<UsuarioResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponseDto>> FindOne(
        int id,
        CancellationToken cancellationToken)
    {
        var usuario = await usuariosService.FindOneAsync(id, cancellationToken);
        return usuario is null
            ? NotFound(ErrorResponseDto.NotFound(MensajeNoExiste(id)))
            : Ok(usuario);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType<UsuarioResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponseDto>> Update(
        int id,
        [FromBody] UpdateUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await usuariosService.UpdateAsync(id, dto, cancellationToken);
        return resultado.Status switch
        {
            UpdateUsuarioStatus.NotFound => NotFound(ErrorResponseDto.NotFound(MensajeNoExiste(id))),
            UpdateUsuarioStatus.EmailConflict => Conflict(ErrorResponseDto.Conflict(MensajeCorreoDuplicado)),
            _ => Ok(resultado.Usuario),
        };
    }

    [HttpDelete("{id}")]
    [ProducesResponseType<UsuarioResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorResponseDto>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponseDto>> Remove(
        int id,
        CancellationToken cancellationToken)
    {
        var usuario = await usuariosService.RemoveAsync(id, cancellationToken);
        return usuario is null
            ? NotFound(ErrorResponseDto.NotFound(MensajeNoExiste(id)))
            : Ok(usuario);
    }

    private static string MensajeNoExiste(int id) => $"No existe un usuario con id {id}";
}
