using Microsoft.EntityFrameworkCore;
using PeliculasReviewApi.Models;

namespace PeliculasReviewApi.Data;

/// <summary>
/// Aplica migraciones pendientes y siembra datos iniciales equivalentes a <c>backend/src/prisma/seed.ts</c>
/// cuando la base de datos está vacía.
/// </summary>
public static class DbSeeder
{
    private const string PasswordDePrueba = "Password123!";

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        var huboCambios = false;

        if (!await context.Usuarios.AnyAsync(cancellationToken))
        {
            var passwordHasheada = BCrypt.Net.BCrypt.HashPassword(PasswordDePrueba, workFactor: 10);

            context.Usuarios.AddRange(
                new Usuario
                {
                    Correo = "ana@ejemplo.com",
                    Nombre = "Ana",
                    Password = passwordHasheada,
                    Rol = Rol.USUARIO
                },
                new Usuario
                {
                    Correo = "admin@ejemplo.com",
                    Nombre = "Admin",
                    Password = passwordHasheada,
                    Rol = Rol.ADMIN
                });

            huboCambios = true;
        }

        if (!await context.Peliculas.AnyAsync(cancellationToken))
        {
            context.Peliculas.AddRange(ObtenerPeliculasSemilla());
            huboCambios = true;
        }

        if (huboCambios)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static IEnumerable<Pelicula> ObtenerPeliculasSemilla() =>
    [
        new()
        {
            Nombre = "Interstellar",
            Estreno = 2014,
            Sinopsis = "Un grupo de exploradores viaja a traves de un agujero de gusano en busca de un nuevo hogar para la humanidad.",
            Imagen = "https://www.themoviedb.org/t/p/w600_and_h900_face/cQuuBjG78j4b2r1dFpAjnjbUplv.jpg"
        },
        new()
        {
            Nombre = "Inception",
            Estreno = 2010,
            Sinopsis = "Un ladron experto en robar secretos del subconsciente recibe la tarea de plantar una idea en la mente de alguien.",
            Imagen = "https://image.tmdb.org/t/p/w500/oYuLEt3zVCKq57qu2F8dT7NIa6f.jpg"
        },
        new()
        {
            Nombre = "The Matrix",
            Estreno = 1999,
            Sinopsis = "Un programador descubre que la realidad es una simulacion controlada por maquinas y se une a la resistencia.",
            Imagen = "https://image.tmdb.org/t/p/w500/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg"
        },
        new()
        {
            Nombre = "Pulp Fiction",
            Estreno = 1994,
            Sinopsis = "Historias interconectadas de gangsters, un boxeador y una pareja de ladrones en el Los Angeles criminal.",
            Imagen = "https://image.tmdb.org/t/p/w500/d5iIlFn5s0ImszYzBPb8JPIfbXD.jpg"
        },
        new()
        {
            Nombre = "The Godfather",
            Estreno = 1972,
            Sinopsis = "El patriarca de una familia mafiosa transfiere el control de su imperio a su hijo menor.",
            Imagen = "https://www.themoviedb.org/t/p/w600_and_h900_face/5HlLUsmsv60cZVTzVns9ICZD6zU.jpg"
        },
        new()
        {
            Nombre = "Forrest Gump",
            Estreno = 1994,
            Sinopsis = "Un hombre de Alabama con bajas capacidades intelectuales vive eventos historicos clave del siglo XX.",
            Imagen = "https://image.tmdb.org/t/p/w500/arw2vcBveWOVZr6pxd9XTd1TdQa.jpg"
        },
        new()
        {
            Nombre = "The Dark Knight",
            Estreno = 2008,
            Sinopsis = "Batman enfrenta al Joker, un criminal caotico que amenaza con hundir Gotham en la anarquia.",
            Imagen = "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg"
        },
        new()
        {
            Nombre = "Fight Club",
            Estreno = 1999,
            Sinopsis = "Un insomne y un vendedor de jabon crean un club de lucha clandestino que deriva en algo mucho mas oscuro.",
            Imagen = "https://image.tmdb.org/t/p/w500/pB8BM7pdSp6B6Ih7QZ4DrQ3PmJK.jpg"
        },
        new()
        {
            Nombre = "Parasite",
            Estreno = 2019,
            Sinopsis = "Una familia pobre se infiltra en la vida de una adinerada familia de Seoul con consecuencias impredecibles.",
            Imagen = "https://image.tmdb.org/t/p/w500/7IiTTgloJzvGI1TAYymCfbfl3vT.jpg"
        },
        new()
        {
            Nombre = "Whiplash",
            Estreno = 2014,
            Sinopsis = "Un joven baterista ambicioso es llevado al limite por un instructor de musica obsesivo y despiadado.",
            Imagen = "https://image.tmdb.org/t/p/w500/7fn624j5lj3xTme2SgiLCeuedmO.jpg"
        },
        new()
        {
            Nombre = "Coco",
            Estreno = 2017,
            Sinopsis = "Un nino viaja al mundo de los muertos para descubrir los secretos de su familia y su pasion por la musica.",
            Imagen = "https://image.tmdb.org/t/p/w500/gGEsBPAijhVUFoiNpgZXqRVWJt2.jpg"
        },
        new()
        {
            Nombre = "Spirited Away",
            Estreno = 2001,
            Sinopsis = "Una nina queda atrapada en un mundo espiritista y debe trabajar en un bano magico para rescatar a sus padres.",
            Imagen = "https://image.tmdb.org/t/p/w500/39wmItIWsg5sZMyRUHLkWBcuVCM.jpg"
        },
        new()
        {
            Nombre = "The Shawshank Redemption",
            Estreno = 1994,
            Sinopsis = "Un banquero inocente encarcelado forge una amistad especial mientras busca recuperar su libertad.",
            Imagen = "https://image.tmdb.org/t/p/w500/q6y0Go1tsGEsmtFryDOJo3dEmqu.jpg"
        },
        new()
        {
            Nombre = "Gladiator",
            Estreno = 2000,
            Sinopsis = "Un general romano traicionado se convierte en gladiador y busca venganza contra el corrupto emperador.",
            Imagen = "https://image.tmdb.org/t/p/w500/ty8TGRuvJLPUmAR1H1nRIsgwvim.jpg"
        },
        new()
        {
            Nombre = "Titanic",
            Estreno = 1997,
            Sinopsis = "Una historia de amor entre dos jovenes de clases distintas a bordo del transatlantico Titanic.",
            Imagen = "https://image.tmdb.org/t/p/w500/9xjZS2rlVxm8SFx8kPC3aIGCOYQ.jpg"
        },
        new()
        {
            Nombre = "Avatar",
            Estreno = 2009,
            Sinopsis = "Un marine paraplejico viaja a Pandora, un exuberante planeta extraterrestre en conflicto con los humanos.",
            Imagen = "https://image.tmdb.org/t/p/w500/jRXYjXNq0Cs2TcJjLkki24MLp7u.jpg"
        },
        new()
        {
            Nombre = "Joker",
            Estreno = 2019,
            Sinopsis = "Arthur Fleck, un comico fracasado en Gotham, va perdiendo la cordura hasta convertirse en el Joker.",
            Imagen = "https://image.tmdb.org/t/p/w500/udDclJoHjfjb8Ekgsd4FDteOkCU.jpg"
        },
        new()
        {
            Nombre = "La La Land",
            Estreno = 2016,
            Sinopsis = "Una actriz aspirante y un musico de jazz se enamoran en Los Angeles mientras persiguen sus suenos.",
            Imagen = "https://image.tmdb.org/t/p/w500/uDO8zWDhfWwoFdKS4fzkUJt0Rf0.jpg"
        },
        new()
        {
            Nombre = "Get Out",
            Estreno = 2017,
            Sinopsis = "Un hombre afroamericano descubre oscuros secretos cuando visita a la familia de su novia blanca.",
            Imagen = "https://image.tmdb.org/t/p/w500/tFXcEccSQMf3lfhfXKSU9iRBpa3.jpg"
        },
        new()
        {
            Nombre = "Everything Everywhere All at Once",
            Estreno = 2022,
            Sinopsis = "Una lavandera descubre que puede acceder a vidas de universos paralelos y debe salvar el multiverso.",
            Imagen = "https://image.tmdb.org/t/p/w500/w3LxiVYdWWRvEVdn5RYq6jIqkb1.jpg"
        }
    ];
}
