namespace VehicleManager.Core.Domain;

/// <summary>A vehicle manufacturer. Stored in a lookup table so the list can change without code changes.</summary>
public class Manufacturer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
