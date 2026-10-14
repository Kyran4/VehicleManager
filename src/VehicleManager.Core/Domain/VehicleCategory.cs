namespace VehicleManager.Core.Domain;

/// <summary>
/// A weight category. Only the inclusive lower bound is stored; the upper bound is the next
/// category's lower bound (exclusive), and the highest category is unbounded. Because ranges are
/// derived rather than stored, gaps and overlaps cannot be represented.
/// </summary>
public class VehicleCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Key into <see cref="CategoryIcons"/>.</summary>
    public string IconKey { get; set; } = string.Empty;

    /// <summary>Inclusive lower bound in kilograms (max two decimal places).</summary>
    public decimal MinWeightKg { get; set; }
}
