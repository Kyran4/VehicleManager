using VehicleManager.Core.Domain;
using VehicleManager.Core.Services;

namespace VehicleManager.Core.Abstractions;

public interface IVehicleRepository
{
    /// <summary>Returns all vehicles (with manufacturer) in the requested order.</summary>
    Task<IReadOnlyList<Vehicle>> ListAsync(VehicleSortOptions sort, CancellationToken ct = default);

    Task AddAsync(Vehicle vehicle, CancellationToken ct = default);
}

public interface ICategoryRepository
{
    /// <summary>Returns all categories ordered by starting weight.</summary>
    Task<IReadOnlyList<VehicleCategory>> GetAllAsync(CancellationToken ct = default);

    Task AddAsync(VehicleCategory category, CancellationToken ct = default);
    Task UpdateAsync(VehicleCategory category, CancellationToken ct = default);
    Task DeleteAsync(VehicleCategory category, CancellationToken ct = default);

    /// <summary>
    /// Runs read-validate-write as one atomic unit so concurrent edits cannot combine into an invalid
    /// configuration.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default);
}

public interface IManufacturerRepository
{
    Task<IReadOnlyList<Manufacturer>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
