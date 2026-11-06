using System.ComponentModel.DataAnnotations;
using PeliculasReviewApi.Dtos.Peliculas;

namespace PeliculasReviewApi.Validation;

/// <summary>
/// Valida que un campo sea una URL absoluta <c>http://</c> o <c>https://</c> con host no vacío y sin
/// espacios en blanco (equivalente a <c>@IsUrl()</c> de <c>class-validator</c>).
/// A diferencia de <see cref="UrlAttribute"/> + <see cref="IValidatableObject"/>, se ejecuta como
/// validación de propiedad para no omitirse cuando otro campo del mismo DTO también falla.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class HttpUrlAttribute : ValidationAttribute
{
    public HttpUrlAttribute()
        : base("imagen must be a URL address")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var memberNames = validationContext.MemberName is { Length: > 0 } member
            ? new[] { member }
            : null;

        if (value is null)
        {
            if (validationContext.ObjectInstance is UpdatePeliculaDto { ImagenEnviada: true })
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
            }

            return ValidationResult.Success;
        }

        if (value is string texto && EsHttpUrlValida(texto))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
    }

    public static bool EsHttpUrlValida(string? valor) =>
        !string.IsNullOrWhiteSpace(valor)
        && !valor.Any(char.IsWhiteSpace)
        && Uri.TryCreate(valor, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
        && !string.IsNullOrWhiteSpace(uri.Host);
}

/// <summary>
/// Para campos opcionales de un PATCH (<c>PartialType</c>) que no admiten <c>null</c> ni cadenas en
/// blanco cuando el cliente envía explícitamente la propiedad en el JSON.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredWhenPresentAttribute(string flagPropertyName) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var flagProp = validationContext.ObjectType.GetProperty(flagPropertyName);
        var fueEnviado = flagProp?.GetValue(validationContext.ObjectInstance) is true;
        if (!fueEnviado)
        {
            return ValidationResult.Success;
        }

        if (value is string texto && !string.IsNullOrWhiteSpace(texto))
        {
            return ValidationResult.Success;
        }

        var memberNames = validationContext.MemberName is { Length: > 0 } member
            ? new[] { member }
            : null;

        return new ValidationResult(
            $"The {validationContext.DisplayName} field is required.",
            memberNames);
    }
}
