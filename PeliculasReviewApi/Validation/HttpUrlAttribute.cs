using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Text;
using PeliculasReviewApi.Dtos.Peliculas;
using PeliculasReviewApi.Dtos.Usuarios;

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
/// Valida que un correo electrónico tenga formato estándar con dominio y TLD válidos sin espacios en
/// blanco (equivalente a <c>@IsEmail()</c> de <c>class-validator</c>), corrigiendo la permisividad de
/// <see cref="EmailAddressAttribute"/> en .NET (que acepta <c>"a@b"</c> o correos con espacios).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class StrictEmailAddressAttribute : ValidationAttribute
{
    public StrictEmailAddressAttribute()
        : base("correo must be an email")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var memberNames = validationContext.MemberName is { Length: > 0 } member
            ? new[] { member }
            : null;

        if (value is null)
        {
            if (validationContext.ObjectInstance is UpdateUsuarioDto { CorreoEnviado: true })
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
            }

            return ValidationResult.Success;
        }

        if (value is string texto && EsCorreoValido(texto))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
    }

    public static bool EsCorreoValido(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor) || valor.Length > 254 || valor.Any(char.IsWhiteSpace))
        {
            return false;
        }

        var arrobaIdx = valor.IndexOf('@');
        if (arrobaIdx <= 0 || arrobaIdx != valor.LastIndexOf('@') || arrobaIdx == valor.Length - 1)
        {
            return false;
        }

        var local = valor[..arrobaIdx];
        var dominio = valor[(arrobaIdx + 1)..];

        if (local.Length > 64
            || local.StartsWith('.')
            || local.EndsWith('.')
            || local.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        if (dominio.StartsWith('.')
            || dominio.EndsWith('.')
            || dominio.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        var etiquetas = dominio.Split('.');
        if (etiquetas.Length < 2)
        {
            return false;
        }

        foreach (var etiqueta in etiquetas)
        {
            if (etiqueta.Length is 0 or > 63
                || etiqueta.StartsWith('-')
                || etiqueta.EndsWith('-')
                || !etiqueta.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'))
            {
                return false;
            }
        }

        var tld = etiquetas[^1];
        if (tld.Length < 2 || !tld.All(char.IsAsciiLetter))
        {
            return false;
        }

        return MailAddress.TryCreate(valor, out var mail)
            && string.Equals(mail.Address, valor, StringComparison.Ordinal);
    }
}

/// <summary>
/// Verifica que una cadena no supere el límite de bytes UTF-8 de BCrypt (72 bytes).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class MaxUtf8ByteLengthAttribute(int maxBytes) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string texto)
        {
            return ValidationResult.Success;
        }

        if (Encoding.UTF8.GetByteCount(texto) <= maxBytes)
        {
            return ValidationResult.Success;
        }

        var memberNames = validationContext.MemberName is { Length: > 0 } member
            ? new[] { member }
            : null;

        return new ValidationResult(
            $"The field {validationContext.DisplayName} must not exceed {maxBytes} UTF-8 bytes.",
            memberNames);
    }
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

        if (value is not null and not string)
        {
            return ValidationResult.Success;
        }

        var memberNames = validationContext.MemberName is { Length: > 0 } member
            ? new[] { member }
            : null;

        var mensaje = !string.IsNullOrWhiteSpace(ErrorMessage)
            ? ErrorMessage
            : $"The {validationContext.DisplayName} field is required.";

        return new ValidationResult(mensaje, memberNames);
    }
}
