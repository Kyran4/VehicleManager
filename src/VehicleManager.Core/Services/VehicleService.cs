using VehicleManager.Core.Abstractions;
using VehicleManager.Core.Common;
using VehicleManager.Core.Domain;
using VehicleManager.Core.Rules;

namespace VehicleManager.Core.Services;

public sealed class VehicleService(
    IVehicleRepository vehicles,
    ICategoryRepository categories,
    IManufacturerRepository manufacturers,
    TimeProvider clock)
{
    public async Task<IReadOnlyList<VehicleListItem>> GetVehiclesAsync(VehicleSortOptions sort, CancellationToken ct = default)
    {
        var rows = await vehicles.ListAsync(sort, ct);
        var allCategories = await categories.GetAllAsync(ct);

        // Category is calculated on every read, so definition changes apply to existing vehicles immediately.
        return rows
            .Select(v => new VehicleListItem(
                v.Id,
                v.OwnerName,
                v.Manufacturer.Name,
                v.YearOfManufacture,
                v.WeightKg,
                CategoryResolver.Resolve(allCategories, v.WeightKg)))
            .ToList();
    }

    public Task<IReadOnlyList<Manufacturer>> GetManufacturersAsync(CancellationToken ct = default) =>
        manufacturers.GetAllAsync(ct);

    public async Task<Result<int>> AddVehicleAsync(VehicleInput input, CancellationToken ct = default)
    {
        var errors = VehicleValidator.Validate(input, clock.GetLocalNow().Year).ToList();

        if (input.ManufacturerId is > 0
            && errors.All(e => e.Field != nameof(VehicleInput.ManufacturerId))
            && !await manufacturers.ExistsAsync(input.ManufacturerId.Value, ct))
        {
            errors.Add(new(nameof(VehicleInput.ManufacturerId), "The selected manufacturer does not exist."));
        }

        if (errors.Count > 0)
            return Result<int>.From(Result.Invalid(errors));

        var vehicle = new Vehicle
        {
            OwnerName = input.OwnerName!.Trim(),
            ManufacturerId = input.ManufacturerId!.Value,
            YearOfManufacture = input.YearOfManufacture!.Value,
            WeightKg = input.WeightKg!.Value,
        };

        await vehicles.AddAsync(vehicle, ct);
        return Result<int>.Ok(vehicle.Id);
    }
}
