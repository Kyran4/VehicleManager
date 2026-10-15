using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VehicleManager.Core.Services;
using VehicleManager.Infrastructure.Persistence;

namespace VehicleManager.Tests.Integration;

/// <summary>
/// A fresh in-memory SQLite database per test, built from the real EF model (so the seeded manufacturers
/// and categories, unique indexes and check constraints are all present). Each Create*Service call uses a
/// new DbContext, mirroring one-context-per-request in the web app.
/// </summary>
public sealed class SqliteDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly List<AppDbContext> _contexts = [];

    public SqliteDatabase()
    {
        _connection.Open();
        CreateContext().Database.EnsureCreated();
    }

    public CategoryService CreateCategoryService() => new(new CategoryRepository(CreateContext()));

    public VehicleService CreateVehicleService()
    {
        var context = CreateContext();
        return new VehicleService(
            new VehicleRepository(context),
            new CategoryRepository(context),
            new ManufacturerRepository(context),
            TimeProvider.System);
    }

    private AppDbContext CreateContext()
    {
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _contexts.Add(context);
        return context;
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
            context.Dispose();
        _connection.Dispose();
    }
}
