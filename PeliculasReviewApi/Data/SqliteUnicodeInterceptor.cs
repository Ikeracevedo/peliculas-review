using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PeliculasReviewApi.Data;

/// <summary>
/// Sobrescribe la función escalar <c>lower()</c> de SQLite con <see cref="string.ToLowerInvariant"/>
/// de .NET al abrir cada conexión. El <c>lower()</c> nativo de SQLite solo convierte caracteres
/// ASCII (<c>A-Z</c>), lo que impedía buscar títulos con tildes o eñes (<c>Élite</c>, <c>Árbol</c>, etc.)
/// sin importar las mayúsculas/minúsculas.
/// </summary>
public sealed class SqliteUnicodeInterceptor : DbConnectionInterceptor
{
    public static readonly SqliteUnicodeInterceptor Instance = new();

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        RegistrarFuncionesUnicode(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        RegistrarFuncionesUnicode(connection);
        return base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static void RegistrarFuncionesUnicode(DbConnection connection)
    {
        if (connection is SqliteConnection sqliteConnection)
        {
            sqliteConnection.CreateFunction(
                "lower",
                (string? valor) => valor?.ToLowerInvariant(),
                isDeterministic: true);
        }
    }
}
