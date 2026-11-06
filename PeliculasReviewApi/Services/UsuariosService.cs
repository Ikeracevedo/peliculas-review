using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PeliculasReviewApi.Data;
using PeliculasReviewApi.Dtos.Usuarios;
using PeliculasReviewApi.Models;

namespace PeliculasReviewApi.Services;

/// <summary>
/// Equivalente a <c>UsuariosService</c> en NestJS (<c>backend/src/usuarios/usuarios.service.ts</c>).
/// Todas las respuestas usan <see cref="UsuarioResponseDto"/> (<c>SELECT_PUBLICO</c>, sin <c>Password</c>).
/// </summary>
public class UsuariosService(AppDbContext context) : IUsuariosService
{
    public async Task<IReadOnlyList<UsuarioResponseDto>> FindAllAsync(
        CancellationToken cancellationToken = default) =>
        await context.Usuarios
            .AsNoTracking()
            .OrderByDescending(u => u.CreadoEn)
            .ThenByDescending(u => u.Id)
            .Select(ProyectarDto())
            .ToListAsync(cancellationToken);

    public Task<UsuarioResponseDto?> FindOneAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        context.Usuarios
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(ProyectarDto())
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<UpdateUsuarioResult> UpdateAsync(
        int id,
        UpdateUsuarioDto dto,
        CancellationToken cancellationToken = default)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (usuario is null)
        {
            return new UpdateUsuarioResult(UpdateUsuarioStatus.NotFound);
        }

        if (dto.CorreoEnviado && dto.Correo is not null && dto.Correo != usuario.Correo)
        {
            var correoOcupado = await context.Usuarios
                .AsNoTracking()
                .AnyAsync(u => u.Id != id && u.Correo == dto.Correo, cancellationToken);
            if (correoOcupado)
            {
                return new UpdateUsuarioResult(UpdateUsuarioStatus.EmailConflict);
            }
        }

        if (dto.NombreEnviado && dto.Nombre is not null) usuario.Nombre = dto.Nombre;
        if (dto.CorreoEnviado && dto.Correo is not null) usuario.Correo = dto.Correo;
        if (dto.RolEnviado && dto.Rol.HasValue) usuario.Rol = dto.Rol.Value;

        context.Entry(usuario).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.Usuarios.AnyAsync(u => u.Id == id, cancellationToken))
            {
                return new UpdateUsuarioResult(UpdateUsuarioStatus.NotFound);
            }

            throw;
        }
        catch (DbUpdateException)
        {
            if (dto.CorreoEnviado
                && dto.Correo is not null
                && await context.Usuarios.AsNoTracking().AnyAsync(u => u.Id != id && u.Correo == dto.Correo, cancellationToken))
            {
                return new UpdateUsuarioResult(UpdateUsuarioStatus.EmailConflict);
            }

            throw;
        }

        return new UpdateUsuarioResult(UpdateUsuarioStatus.Ok, ADto(usuario));
    }

    public async Task<UsuarioResponseDto?> RemoveAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (usuario is null)
        {
            return null;
        }

        var eliminado = ADto(usuario);

        context.Usuarios.Remove(usuario);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.Usuarios.AnyAsync(u => u.Id == id, cancellationToken))
            {
                return null;
            }

            throw;
        }

        return eliminado;
    }

    private static Expression<Func<Usuario, UsuarioResponseDto>> ProyectarDto() =>
        u => new UsuarioResponseDto
        {
            Id = u.Id,
            Correo = u.Correo,
            Nombre = u.Nombre,
            Rol = u.Rol,
            CreadoEn = u.CreadoEn,
            ActualizadoEn = u.ActualizadoEn,
        };

    private static UsuarioResponseDto ADto(Usuario u) => new()
    {
        Id = u.Id,
        Correo = u.Correo,
        Nombre = u.Nombre,
        Rol = u.Rol,
        CreadoEn = u.CreadoEn,
        ActualizadoEn = u.ActualizadoEn,
    };
}
