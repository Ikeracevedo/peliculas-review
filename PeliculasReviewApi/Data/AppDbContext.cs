using Microsoft.EntityFrameworkCore;
using PeliculasReviewApi.Models;

namespace PeliculasReviewApi.Data;

/// <summary>
/// Contexto principal de Entity Framework Core (SQLite).
/// Replica el esquema definido en <c>backend/prisma/schema.prisma</c>.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Pelicula> Peliculas => Set<Pelicula>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.Correo).IsUnique();

            entity.Property(u => u.Rol)
                .HasConversion<string>()
                .HasDefaultValue(Rol.USUARIO);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            // Equivalente a @@unique([peliculaId, autorId]), @@index([peliculaId]), @@index([autorId]) en Prisma.
            entity.HasIndex(r => new { r.PeliculaId, r.AutorId }).IsUnique();
            entity.HasIndex(r => r.PeliculaId);
            entity.HasIndex(r => r.AutorId);

            entity.HasOne(r => r.Pelicula)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.PeliculaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Autor)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.AutorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
        => SaveChanges(acceptAllChangesOnSuccess: true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ActualizarFechasAuditoria();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ActualizarFechasAuditoria();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// Replica el comportamiento de <c>@default(now())</c> y <c>@updatedAt</c> de Prisma.
    /// </summary>
    private void ActualizarFechasAuditoria()
    {
        var ahora = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                switch (entry.Entity)
                {
                    case Usuario usuario:
                        usuario.CreadoEn = ahora;
                        usuario.ActualizadoEn = ahora;
                        break;
                    case Pelicula pelicula:
                        pelicula.CreadaEn = ahora;
                        pelicula.ActualizadoEn = ahora;
                        break;
                    case Review review:
                        review.CreadoEn = ahora;
                        review.ActualizadoEn = ahora;
                        break;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                switch (entry.Entity)
                {
                    case Usuario usuario:
                        usuario.ActualizadoEn = ahora;
                        break;
                    case Pelicula pelicula:
                        pelicula.ActualizadoEn = ahora;
                        break;
                    case Review review:
                        review.ActualizadoEn = ahora;
                        break;
                }
            }
        }
    }
}
