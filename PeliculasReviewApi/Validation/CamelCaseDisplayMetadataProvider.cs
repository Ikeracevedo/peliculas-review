using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace PeliculasReviewApi.Validation;

/// <summary>
/// Usa el nombre camelCase de cada propiedad como nombre visible en los mensajes de validación
/// ("The field estreno must be between...") para que coincidan con los campos del JSON / query string,
/// como hace class-validator en NestJS. Respeta un <c>[Display(Name = ...)]</c> explícito.
/// </summary>
public sealed class CamelCaseDisplayMetadataProvider : IDisplayMetadataProvider
{
    public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
    {
        if (context.Key.MetadataKind != ModelMetadataKind.Property
            || context.Key.Name is not { Length: > 0 } nombre
            || context.DisplayMetadata.DisplayName is not null)
        {
            return;
        }

        var nombreCamel = JsonNamingPolicy.CamelCase.ConvertName(nombre);
        context.DisplayMetadata.DisplayName = () => nombreCamel;
    }
}
