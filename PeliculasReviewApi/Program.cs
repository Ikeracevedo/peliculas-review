using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IReviewsService, ReviewsService>();

// Configuración JWT leyendo SecretKey, Issuer y Audience desde appsettings.json.
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecretKey = jwtSection["SecretKey"]
    ?? throw new InvalidOperationException("No se configuró Jwt:SecretKey en appsettings.json.");
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "rol",
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                if (context.Response.HasStarted)
                {
                    return;
                }

                // En NestJS AuthGuard:
                // - Sin "Bearer <token>" -> "No se envio un token de acceso"
                // - Con "Bearer <token>" inválido o expirado -> "Token invalido o expirado"
                var mensaje = TieneTokenBearer(context.Request)
                    ? "Token invalido o expirado"
                    : "No se envio un token de acceso";

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsJsonAsync(
                    ErrorResponseDto.Unauthorized(mensaje),
                    context.HttpContext.RequestAborted);
            },
            OnForbidden = async context =>
            {
                if (context.Response.HasStarted)
                {
                    return;
                }

                // En NestJS RolesGuard: ForbiddenException('No tienes permisos para esta accion')
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsJsonAsync(
                    ErrorResponseDto.Forbidden("No tienes permisos para esta accion"),
                    context.HttpContext.RequestAborted);
            },
        };
    });

builder.Services.AddAuthorization();

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
                .SelectMany(kvp => kvp.Value!.Errors.Select(error => (kvp.Key, Error: error)))
                .Select(item =>
                {
                    var raw = string.IsNullOrWhiteSpace(item.Error.ErrorMessage)
                        ? item.Error.Exception?.Message ?? "Invalid value"
                        : item.Error.ErrorMessage;

                    if (item.Key is "$.rol" or "rol"
                        || raw.Contains("PeliculasReviewApi.Models.Rol", StringComparison.Ordinal))
                    {
                        return "rol debe ser USUARIO o ADMIN";
                    }

                    return raw;
                })
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

// Configuración de Swagger / OpenAPI con soporte de autenticación JWT Bearer (botón "Authorize").
var bearerSecurityScheme = new OpenApiSecurityScheme
{
    Name = "Authorization",
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Ingresa el token JWT obtenido en /api/auth/login o /api/auth/registro (sin el prefijo 'Bearer ').",
};

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Películas Review API";
        document.Info.Version = "v1";
        document.Info.Description = "API REST para reseñas de películas.";
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = bearerSecurityScheme;
        return Task.CompletedTask;
    });
    options.AddOperationTransformer((operation, context, _) =>
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any())
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = [],
            });
        }
        return Task.CompletedTask;
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(dbContext);
}

// Configure the HTTP request pipeline.
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Películas Review API v1");
    options.DocumentTitle = "Películas Review API - Swagger UI";
});
app.MapOpenApi();

if (!app.Environment.IsDevelopment())
{
    // En desarrollo el front llama por HTTP (http://localhost:5000/api); una redirección 307 a HTTPS
    // rompería las peticiones preflight de CORS.
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyFrontend);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static bool TieneTokenBearer(HttpRequest request)
{
    var authHeader = request.Headers.Authorization.ToString();
    if (string.IsNullOrWhiteSpace(authHeader))
    {
        return false;
    }

    var partes = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    return partes.Length == 2
        && partes[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(partes[1]);
}
