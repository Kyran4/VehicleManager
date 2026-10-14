namespace VehicleManager.Core.Domain;

/// <summary>
/// A vehicle. Its category is deliberately NOT stored: it is calculated from <see cref="WeightKg"/>
/// against the current category configuration, so it can never become stale.
/// </summary>
public class Vehicle
{
    public int Id { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int ManufacturerId { get; set; }
    public Manufacturer Manufacturer { get; set; } = null!;
    public int YearOfManufacture { get; set; }
    public decimal WeightKg { get; set; }
}
