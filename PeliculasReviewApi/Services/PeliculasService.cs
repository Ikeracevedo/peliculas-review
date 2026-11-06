using Microsoft.EntityFrameworkCore;
using PeliculasReviewApi.Data;
using PeliculasReviewApi.Dtos.Common;
using PeliculasReviewApi.Dtos.Peliculas;
using PeliculasReviewApi.Models;

namespace PeliculasReviewApi.Services;

public class PeliculasService(AppDbContext context) : IPeliculasService
{
    public async Task<PagedResult<PeliculaResponseDto>> FindAllAsync(
        PeliculasQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query ??= new PeliculasQueryDto();

        IQueryable<Pelicula> peliculas = context.Peliculas.AsNoTracking();

        var termino = !string.IsNullOrWhiteSpace(query.Nombre)
            ? query.Nombre.Trim()
            : query.Search?.Trim();
        if (!string.IsNullOrEmpty(termino))
        {
            var terminoMinusculas = termino.ToLowerInvariant();
            peliculas = peliculas.Where(p => p.Nombre.ToLower().Contains(terminoMinusculas));
        }

        // Mismo orden que NestJS (creadaEn desc). El desempate por id hace estable la paginación
        // cuando varias películas comparten fecha (p. ej. las del seeder).
        var ordenadas = peliculas
            .OrderByDescending(p => p.CreadaEn)
            .ThenByDescending(p => p.Id);

        if (!query.EsPaginada)
        {
            var todas = await ordenadas.Select(ProyectarDto()).ToListAsync(cancellationToken);
            return new PagedResult<PeliculaResponseDto>(todas, todas.Count, null, null);
        }

        var page = query.Page ?? PeliculasQueryDefaults.DefaultPage;
        var limit = query.Limit ?? query.PageSize ?? PeliculasQueryDefaults.DefaultLimit;

        var total = await peliculas.CountAsync(cancellationToken);
        var items = await ordenadas
            .Skip((int)Math.Min((long)(page - 1) * limit, int.MaxValue))
            .Take(limit)
            .Select(ProyectarDto())
            .ToListAsync(cancellationToken);

        return new PagedResult<PeliculaResponseDto>(items, total, page, limit);
    }

    public Task<PeliculaResponseDto?> FindOneAsync(int id, CancellationToken cancellationToken = default) =>
        context.Peliculas
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ProyectarDto())
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<PeliculaResponseDto> CreateAsync(CreatePeliculaDto dto, CancellationToken cancellationToken = default)
    {
        var pelicula = new Pelicula
        {
            Nombre = dto.Nombre,
            Imagen = dto.Imagen,
            Sinopsis = dto.Sinopsis,
            Estreno = dto.Estreno,
        };

        context.Peliculas.Add(pelicula);
        await context.SaveChangesAsync(cancellationToken);

        return ADto(pelicula);
    }

    public async Task<PeliculaResponseDto?> UpdateAsync(
        int id,
        UpdatePeliculaDto dto,
        CancellationToken cancellationToken = default)
    {
        var pelicula = await context.Peliculas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (pelicula is null)
        {
            return null;
        }

        // Solo se tocan las propiedades enviadas (semántica PATCH / PartialType).
        if (dto.NombreEnviado && dto.Nombre is not null) pelicula.Nombre = dto.Nombre;
        if (dto.ImagenEnviada && dto.Imagen is not null) pelicula.Imagen = dto.Imagen;
        if (dto.SinopsisEnviada) pelicula.Sinopsis = dto.Sinopsis;
        if (dto.EstrenoEnviado) pelicula.Estreno = dto.Estreno;

        // Prisma refresca @updatedAt en cada update aunque no cambie ningún campo.
        context.Entry(pelicula).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.Peliculas.AnyAsync(p => p.Id == id, cancellationToken))
            {
                return null;
            }

            throw;
        }

        return ADto(pelicula);
    }

    public async Task<PeliculaResponseDto?> RemoveAsync(int id, CancellationToken cancellationToken = default)
    {
        var pelicula = await context.Peliculas.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (pelicula is null)
        {
            return null;
        }

        var eliminada = ADto(pelicula);

        // Las reviews asociadas se borran en cascada (FK ON DELETE CASCADE), igual que en Prisma.
        context.Peliculas.Remove(pelicula);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.Peliculas.AnyAsync(p => p.Id == id, cancellationToken))
            {
                return null;
            }

            throw;
        }

        return eliminada;
    }

    private static System.Linq.Expressions.Expression<Func<Pelicula, PeliculaResponseDto>> ProyectarDto() =>
        p => new PeliculaResponseDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Imagen = p.Imagen,
            Sinopsis = p.Sinopsis,
            Estreno = p.Estreno,
            CreadaEn = p.CreadaEn,
            ActualizadoEn = p.ActualizadoEn,
        };

    private static PeliculaResponseDto ADto(Pelicula p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Imagen = p.Imagen,
        Sinopsis = p.Sinopsis,
        Estreno = p.Estreno,
        CreadaEn = p.CreadaEn,
        ActualizadoEn = p.ActualizadoEn,
    };
}
