using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VehicleManager.Core.Abstractions;
using VehicleManager.Core.Domain;
using VehicleManager.Core.Services;

namespace VehicleManager.Infrastructure.Persistence;

public sealed class VehicleRepository(AppDbContext db) : IVehicleRepository
{
    public async Task<IReadOnlyList<Vehicle>> ListAsync(VehicleSortOptions sort, CancellationToken ct = default)
    {
        Expression<Func<Vehicle, object>> key = sort.Field switch
        {
            VehicleSortField.Owner => v => v.OwnerName,
            VehicleSortField.Manufacturer => v => v.Manufacturer.Name,
            VehicleSortField.Year => v => v.YearOfManufacture,
            VehicleSortField.Weight => v => v.WeightKg,
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort.Field, "Unknown sort field."),
        };

        var query = db.Vehicles.AsNoTracking().Include(v => v.Manufacturer);
        var ordered = sort.Descending ? query.OrderByDescending(key) : query.OrderBy(key);

        // Id as a tie-breaker keeps the order stable between requests.
        return await ordered.ThenBy(v => v.Id).ToListAsync(ct);
    }

    public async Task AddAsync(Vehicle vehicle, CancellationToken ct = default)
    {
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync(ct);
    }
}
