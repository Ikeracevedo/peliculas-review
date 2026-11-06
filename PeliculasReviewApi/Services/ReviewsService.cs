using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PeliculasReviewApi.Data;
using PeliculasReviewApi.Dtos.Peliculas;
using PeliculasReviewApi.Dtos.Reviews;
using PeliculasReviewApi.Models;

namespace PeliculasReviewApi.Services;

/// <summary>
/// Equivalente a <c>ReviewsService</c> en NestJS (<c>backend/src/reviews/reviews.service.ts</c>).
/// </summary>
public class ReviewsService(AppDbContext context) : IReviewsService
{
    public async Task<ReviewMutationResult> CreateAsync(
        CreateReviewDto dto,
        int autorId,
        CancellationToken cancellationToken = default)
    {
        if (!await context.Usuarios.AsNoTracking().AnyAsync(u => u.Id == autorId, cancellationToken))
        {
            return new ReviewMutationResult(ReviewMutationStatus.UsuarioNotFound);
        }

        if (!await context.Peliculas.AsNoTracking().AnyAsync(p => p.Id == dto.PeliculaId, cancellationToken))
        {
            return new ReviewMutationResult(ReviewMutationStatus.PeliculaNotFound);
        }

        var yaExiste = await context.Reviews
            .AsNoTracking()
            .AnyAsync(r => r.PeliculaId == dto.PeliculaId && r.AutorId == autorId, cancellationToken);
        if (yaExiste)
        {
            return new ReviewMutationResult(ReviewMutationStatus.DuplicateReview);
        }

        var review = new Review
        {
            Titulo = dto.Titulo,
            Contenido = dto.Contenido,
            Calificacion = dto.Calificacion,
            PeliculaId = dto.PeliculaId,
            AutorId = autorId,
        };

        context.Reviews.Add(review);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await context.Reviews.AsNoTracking().AnyAsync(
                    r => r.PeliculaId == dto.PeliculaId && r.AutorId == autorId,
                    cancellationToken))
            {
                return new ReviewMutationResult(ReviewMutationStatus.DuplicateReview);
            }

            if (!await context.Peliculas.AsNoTracking().AnyAsync(p => p.Id == dto.PeliculaId, cancellationToken))
            {
                return new ReviewMutationResult(ReviewMutationStatus.PeliculaNotFound);
            }

            throw;
        }

        return new ReviewMutationResult(ReviewMutationStatus.Ok, ADtoEscalar(review));
    }

    public async Task<IReadOnlyList<ReviewResponseDto>> FindAllAsync(
        CancellationToken cancellationToken = default) =>
        await context.Reviews
            .AsNoTracking()
            .OrderByDescending(r => r.CreadoEn)
            .ThenByDescending(r => r.Id)
            .Select(ProyectarConRelaciones())
            .ToListAsync(cancellationToken);

    public Task<ReviewResponseDto?> FindOneAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        context.Reviews
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(ProyectarConRelaciones())
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<ReviewMutationResult> UpdateAsync(
        int id,
        UpdateReviewDto dto,
        UsuarioActualJwt usuarioActual,
        CancellationToken cancellationToken = default)
    {
        var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (review is null)
        {
            return new ReviewMutationResult(ReviewMutationStatus.ReviewNotFound);
        }

        if (!PuedeModificar(review.AutorId, usuarioActual))
        {
            return new ReviewMutationResult(ReviewMutationStatus.Forbidden);
        }

        if (dto.PeliculaIdEnviado && dto.PeliculaId.HasValue && dto.PeliculaId.Value != review.PeliculaId)
        {
            var nuevaPeliculaId = dto.PeliculaId.Value;
            if (!await context.Peliculas.AsNoTracking().AnyAsync(p => p.Id == nuevaPeliculaId, cancellationToken))
            {
                return new ReviewMutationResult(ReviewMutationStatus.PeliculaNotFound);
            }

            var duplicada = await context.Reviews
                .AsNoTracking()
                .AnyAsync(
                    r => r.Id != id && r.PeliculaId == nuevaPeliculaId && r.AutorId == review.AutorId,
                    cancellationToken);
            if (duplicada)
            {
                return new ReviewMutationResult(ReviewMutationStatus.DuplicateReview);
            }
        }

        if (dto.TituloEnviado && dto.Titulo is not null) review.Titulo = dto.Titulo;
        if (dto.ContenidoEnviado && dto.Contenido is not null) review.Contenido = dto.Contenido;
        if (dto.CalificacionEnviada && dto.Calificacion.HasValue) review.Calificacion = dto.Calificacion.Value;
        if (dto.PeliculaIdEnviado && dto.PeliculaId.HasValue) review.PeliculaId = dto.PeliculaId.Value;

        context.Entry(review).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.Reviews.AnyAsync(r => r.Id == id, cancellationToken))
            {
                return new ReviewMutationResult(ReviewMutationStatus.ReviewNotFound);
            }

            throw;
        }
        catch (DbUpdateException)
        {
            if (dto.PeliculaIdEnviado
                && dto.PeliculaId.HasValue
                && await context.Reviews.AsNoTracking().AnyAsync(
                    r => r.Id != id && r.PeliculaId == dto.PeliculaId.Value && r.AutorId == review.AutorId,
                    cancellationToken))
            {
                return new ReviewMutationResult(ReviewMutationStatus.DuplicateReview);
            }

            throw;
        }

        return new ReviewMutationResult(ReviewMutationStatus.Ok, ADtoEscalar(review));
    }

    public async Task<ReviewMutationResult> RemoveAsync(
        int id,
        UsuarioActualJwt usuarioActual,
        CancellationToken cancellationToken = default)
    {
        var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (review is null)
        {
            return new ReviewMutationResult(ReviewMutationStatus.ReviewNotFound);
        }

        if (!PuedeModificar(review.AutorId, usuarioActual))
        {
            return new ReviewMutationResult(ReviewMutationStatus.Forbidden);
        }

        var eliminada = ADtoEscalar(review);

        context.Reviews.Remove(review);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.Reviews.AnyAsync(r => r.Id == id, cancellationToken))
            {
                return new ReviewMutationResult(ReviewMutationStatus.ReviewNotFound);
            }

            throw;
        }

        return new ReviewMutationResult(ReviewMutationStatus.Ok, eliminada);
    }

    private static bool PuedeModificar(int autorId, UsuarioActualJwt usuarioActual) =>
        autorId == usuarioActual.Sub || usuarioActual.EsAdmin;

    private static Expression<Func<Review, ReviewResponseDto>> ProyectarConRelaciones() =>
        r => new ReviewResponseDto
        {
            Id = r.Id,
            Titulo = r.Titulo,
            Contenido = r.Contenido,
            Calificacion = r.Calificacion,
            CreadoEn = r.CreadoEn,
            ActualizadoEn = r.ActualizadoEn,
            PeliculaId = r.PeliculaId,
            AutorId = r.AutorId,
            Pelicula = new PeliculaResponseDto
            {
                Id = r.Pelicula.Id,
                Nombre = r.Pelicula.Nombre,
                Imagen = r.Pelicula.Imagen,
                Sinopsis = r.Pelicula.Sinopsis,
                Estreno = r.Pelicula.Estreno,
                CreadaEn = r.Pelicula.CreadaEn,
                ActualizadoEn = r.Pelicula.ActualizadoEn,
            },
            Autor = new ReviewAutorDto
            {
                Id = r.Autor.Id,
                Nombre = r.Autor.Nombre,
            },
        };

    private static ReviewResponseDto ADtoEscalar(Review r) => new()
    {
        Id = r.Id,
        Titulo = r.Titulo,
        Contenido = r.Contenido,
        Calificacion = r.Calificacion,
        CreadoEn = r.CreadoEn,
        ActualizadoEn = r.ActualizadoEn,
        PeliculaId = r.PeliculaId,
        AutorId = r.AutorId,
    };
}
