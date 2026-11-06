using System.Text.Json.Serialization;
using PeliculasReviewApi.Dtos.Peliculas;

namespace PeliculasReviewApi.Dtos.Reviews;

/// <summary>
/// DTO de salida para las reseñas (<c>/api/reviews</c>).
/// En <c>GET /api/reviews</c> y <c>GET /api/reviews/:id</c>, NestJS incluye <c>pelicula</c> completa
/// y <c>autor</c> proyectado a <c>{ id, nombre }</c>. En <c>POST</c>/<c>PATCH</c>/<c>DELETE</c>
/// devuelve solo los campos escalares, por lo que <c>Pelicula</c> y <c>Autor</c> se omiten si son <c>null</c>.
/// </summary>
public class ReviewResponseDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public int Calificacion { get; set; }
    public DateTime CreadoEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
    public int PeliculaId { get; set; }
    public int AutorId { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PeliculaResponseDto? Pelicula { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ReviewAutorDto? Autor { get; set; }
}

/// <summary>
/// Proyección del autor incluida en las reseñas (<c>autor: { select: { id: true, nombre: true } }</c>).
/// </summary>
public class ReviewAutorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
