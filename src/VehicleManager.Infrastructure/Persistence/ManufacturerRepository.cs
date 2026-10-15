using Microsoft.EntityFrameworkCore;
using VehicleManager.Core.Abstractions;
using VehicleManager.Core.Domain;

namespace VehicleManager.Infrastructure.Persistence;

public sealed class ManufacturerRepository(AppDbContext db) : IManufacturerRepository
{
    public async Task<IReadOnlyList<Manufacturer>> GetAllAsync(CancellationToken ct = default) =>
        await db.Manufacturers.AsNoTracking().OrderBy(m => m.Name).ToListAsync(ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        db.Manufacturers.AnyAsync(m => m.Id == id, ct);
}
