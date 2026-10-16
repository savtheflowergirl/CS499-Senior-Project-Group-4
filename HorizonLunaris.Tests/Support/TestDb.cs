using HorizonLunaris.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HorizonLunaris.Tests.Support;

/// <summary>
/// A throwaway in-memory SQLite database with the real schema and seed users.
/// Use one per test: using var db = new TestDb();
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public HorizonLunarisDbContext Context { get; }

    public TestDb()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open(); // the in-memory database lives as long as this connection

        var options = new DbContextOptionsBuilder<HorizonLunarisDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new HorizonLunarisDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
