using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PeliculasReviewApi.Json;

/// <summary>
/// Serializa <see cref="DateTime"/> igual que Prisma/NestJS (<c>Date.toISOString()</c>):
/// <c>2026-11-06T17:07:50.123Z</c>. SQLite devuelve las fechas con <c>Kind = Unspecified</c>; como
/// siempre se guardan en UTC (ver AppDbContext), se tratan como UTC. Sin esto saldrían sin la "Z"
/// y el navegador las interpretaría como hora local.
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    private const string Formato = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valor = reader.GetDateTime();
        return valor.Kind switch
        {
            DateTimeKind.Utc => valor,
            DateTimeKind.Local => valor.ToUniversalTime(),
            _ => DateTime.SpecifyKind(valor, DateTimeKind.Utc),
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Utc => value,
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
        writer.WriteStringValue(utc.ToString(Formato, CultureInfo.InvariantCulture));
    }
}
