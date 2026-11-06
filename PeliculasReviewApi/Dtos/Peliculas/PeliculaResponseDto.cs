namespace PeliculasReviewApi.Dtos.Peliculas;

/// <summary>
/// DTO de salida que representa una película tal como la devuelve Prisma en NestJS.
/// </summary>
public class PeliculaResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty;
    public string? Sinopsis { get; set; }
    public int? Estreno { get; set; }

    // En Prisma este campo se llama "creadaEn" (femenino).
    public DateTime CreadaEn { get; set; }
    public DateTime ActualizadoEn { get; set; }
}
