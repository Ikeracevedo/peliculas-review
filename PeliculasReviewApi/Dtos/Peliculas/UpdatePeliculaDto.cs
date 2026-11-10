using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PeliculasReviewApi.Validation;

namespace PeliculasReviewApi.Dtos.Peliculas;

/// <summary>
/// DTO de entrada para actualizar parcialmente una película (<c>PATCH /api/peliculas/:id</c>).
/// Equivalente a <c>UpdatePeliculaDto = PartialType(CreatePeliculaDto)</c> en NestJS.
/// </summary>
/// <remarks>
/// Distingue "propiedad ausente" (no se toca) de "propiedad enviada con null": igual que Prisma en
/// NestJS, <c>{"sinopsis": null}</c> o <c>{"estreno": null}</c> limpian el campo. Para los campos
/// obligatorios (<c>nombre</c>, <c>imagen</c>) un null o cadena en blanco explícitos devuelven 400.
/// </remarks>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdatePeliculaDto
{
    private string? _nombre;
    private string? _imagen;
    private string? _sinopsis;
    private int? _estreno;

    [RequiredWhenPresent(nameof(NombreEnviado))]
    [StringLength(200, MinimumLength = 1)]
    public string? Nombre
    {
        get => _nombre;
        set { _nombre = value; NombreEnviado = true; }
    }

    [HttpUrl]
    public string? Imagen
    {
        get => _imagen;
        set { _imagen = value; ImagenEnviada = true; }
    }

    [StringLength(2000)]
    public string? Sinopsis
    {
        get => _sinopsis;
        set { _sinopsis = value; SinopsisEnviada = true; }
    }

    [Range(1888, 2100)]
    public int? Estreno
    {
        get => _estreno;
        set { _estreno = value; EstrenoEnviado = true; }
    }

    [JsonIgnore] public bool NombreEnviado { get; private set; }
    [JsonIgnore] public bool ImagenEnviada { get; private set; }
    [JsonIgnore] public bool SinopsisEnviada { get; private set; }
    [JsonIgnore] public bool EstrenoEnviado { get; private set; }
}
