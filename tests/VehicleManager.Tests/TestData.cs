using VehicleManager.Core.Domain;

namespace VehicleManager.Tests;

internal static class TestData
{
    public static VehicleCategory Category(int id, string name, decimal minKg, string icon = "car") =>
        new() { Id = id, Name = name, IconKey = icon, MinWeightKg = minKg };

    /// <summary>The initial configuration from the brief: Light [0,500), Medium [500,2500), Heavy [2500,..).</summary>
    public static List<VehicleCategory> DefaultCategories() =>
    [
        Category(1, "Light", 0m, "feather"),
        Category(2, "Medium", 500m),
        Category(3, "Heavy", 2500m, "truck"),
    ];

    public static decimal Kg(string value) => decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
}
