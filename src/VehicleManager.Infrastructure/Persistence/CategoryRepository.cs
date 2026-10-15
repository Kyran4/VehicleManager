using System.Data;
using Microsoft.EntityFrameworkCore;
using VehicleManager.Core.Abstractions;
using VehicleManager.Core.Domain;

namespace VehicleManager.Infrastructure.Persistence;

public sealed class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<IReadOnlyList<VehicleCategory>> GetAllAsync(CancellationToken ct = default) =>
        await db.VehicleCategories.AsNoTracking().OrderBy(c => c.MinWeightKg).ToListAsync(ct);

    public async Task AddAsync(VehicleCategory category, CancellationToken ct = default)
    {
        db.VehicleCategories.Add(category);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(VehicleCategory category, CancellationToken ct = default)
    {
        db.VehicleCategories.Update(category);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(VehicleCategory category, CancellationToken ct = default)
    {
        db.VehicleCategories.Remove(category);
        await db.SaveChangesAsync(ct);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
    {
        // Serializable: two admins editing at once cannot each validate against stale data and then both commit.
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var result = await action();
        await transaction.CommitAsync(ct);
        return result;
    }
}
