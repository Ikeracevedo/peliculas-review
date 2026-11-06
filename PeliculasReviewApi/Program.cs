using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using PeliculasReviewApi.Data;
using PeliculasReviewApi.Dtos.Common;
using PeliculasReviewApi.Json;
using PeliculasReviewApi.Services;
using PeliculasReviewApi.Validation;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyFrontend = "FrontendVue";

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPeliculasService, PeliculasService>();

builder.Services.AddControllers(options =>
    {
        // Los nombres de campo en los errores de validación usan el nombre JSON (camelCase),
        // tanto en las claves de ModelState como en el texto de los mensajes.
        options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
        options.ModelMetadataDetailsProviders.Add(new CamelCaseDisplayMetadataProvider());
    })
    .AddJsonOptions(options =>
    {
        var json = options.JsonSerializerOptions;

        // camelCase estricto en respuestas Y peticiones: "Nombre" en el body no se mapea a "nombre"
        // y, por JsonUnmappedMemberHandling.Disallow, devuelve 400 (como forbidNonWhitelisted).
        json.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        json.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        json.PropertyNameCaseInsensitive = false;
        json.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;

        // Como class-validator @IsInt(): "2014" (string) no es un número válido.
        json.NumberHandling = JsonNumberHandling.Strict;

        json.Converters.Add(new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false));
        json.Converters.Add(new UtcDateTimeConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Errores 400 con la misma forma que ValidationPipe de NestJS:
        // { "message": ["..."], "error": "Bad Request", "statusCode": 400 }
        options.InvalidModelStateResponseFactory = context =>
        {
            var entradasConError = context.ModelState
                .Where(kvp => kvp.Value is { Errors.Count: > 0 })
                .ToList();

            // Si falló el parseo JSON de un campo concreto ($ o $.campo), se omite el error
            // genérico "The <param> field is required." que añade el binder del parámetro [FromBody].
            var hayErrorJson = entradasConError.Any(kvp =>
                kvp.Key == "$"
                || kvp.Key.StartsWith("$.", StringComparison.Ordinal)
                || kvp.Value!.Errors.Any(e => e.Exception is JsonException));

            if (hayErrorJson && entradasConError.Count > 1)
            {
                entradasConError = entradasConError
                    .Where(kvp =>
                        kvp.Key == "$"
                        || kvp.Key.StartsWith("$.", StringComparison.Ordinal)
                        || kvp.Value!.Errors.Any(e => e.Exception is JsonException))
                    .ToList();
            }

            var mensajes = entradasConError
                .SelectMany(kvp => kvp.Value!.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? error.Exception?.Message ?? "Invalid value"
                    : error.ErrorMessage)
                .Distinct()
                .ToList();

            return new BadRequestObjectResult(ErrorResponseDto.BadRequest(mensajes))
            {
                ContentTypes = { "application/json" },
            };
        };
    });

// CORS para el cliente Vue (equivalente a app.enableCors({ origin: [...], credentials: true }) de NestJS).
var origenesPermitidos = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    is { Length: > 0 } origenes
        ? origenes
        : ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyFrontend, policy => policy
        .WithOrigins(origenesPermitidos)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials() // el frontend usa axios con withCredentials: true
        .WithExposedHeaders("Location", "X-Total-Count", "X-Page", "X-Limit", "X-Total-Pages"));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    // En desarrollo el front llama por HTTP (http://localhost:3000/api); una redirección 307 a HTTPS
    // rompería las peticiones preflight de CORS.
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyFrontend);

app.UseAuthorization();

app.MapControllers();

app.Run();
