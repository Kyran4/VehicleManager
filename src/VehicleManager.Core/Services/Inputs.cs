using VehicleManager.Core.Domain;

namespace VehicleManager.Core.Services;

/// <summary>Raw, unvalidated vehicle input (nullable so "missing" can be reported properly).</summary>
public sealed record VehicleInput(string? OwnerName, int? ManufacturerId, int? YearOfManufacture, decimal? WeightKg);

public sealed record CategoryInput(string? Name, string? IconKey, decimal? MinWeightKg);

/// <summary>A vehicle row for display, with its category resolved from the current configuration.</summary>
public sealed record VehicleListItem(
    int Id,
    string OwnerName,
    string Manufacturer,
    int YearOfManufacture,
    decimal WeightKg,
    VehicleCategory Category);
