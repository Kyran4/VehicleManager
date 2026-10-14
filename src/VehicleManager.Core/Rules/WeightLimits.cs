namespace VehicleManager.Core.Rules;

/// <summary>Weight constraints shared by vehicle and category validation (and the database schema).</summary>
public static class WeightLimits
{
    public const int DecimalPlaces = 2;

    /// <summary>Sanity ceiling (1,000 tonnes). The top category itself is unbounded.</summary>
    public const decimal MaxKg = 1_000_000m;

    public static bool HasValidPrecision(decimal value) => decimal.Round(value, DecimalPlaces) == value;
}
