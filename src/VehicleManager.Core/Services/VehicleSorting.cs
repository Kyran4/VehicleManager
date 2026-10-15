namespace VehicleManager.Core.Services;

public enum VehicleSortField
{
    Owner,
    Manufacturer,
    Year,
    Weight,
}

public sealed record VehicleSortOptions(VehicleSortField Field = VehicleSortField.Owner, bool Descending = false);
