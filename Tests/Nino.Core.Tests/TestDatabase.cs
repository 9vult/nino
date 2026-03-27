// SPDX-License-Identifier: MPL-2.0

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Nino.Core.Tests;

public class TestDatabase : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public NinoDbContext Context { get; }
    public ReadOnlyNinoDbContext ReadOnlyContext { get; }

    private TestDatabase(
        SqliteConnection connection,
        NinoDbContext context,
        ReadOnlyNinoDbContext readOnlyContext
    )
    {
        _connection = connection;
        Context = context;
        ReadOnlyContext = readOnlyContext;
    }

    public static async Task<TestDatabase> CreateAsync()
    {
        // Named in-memory DB: keeps the connection alive and isolated per instance
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<NinoDbContext>().UseSqlite(connection).Options;

        var context = new NinoDbContext(options);
        var readOnlyContext = new ReadOnlyNinoDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return new TestDatabase(connection, context, readOnlyContext);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
