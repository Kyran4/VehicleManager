namespace VehicleManager.Core.Domain;

public sealed record CategoryIcon(string Key, string Glyph, string Label);

/// <summary>
/// Fixed catalogue of selectable icons. The database stores only the key, so glyphs can be
/// restyled (or swapped for image assets) without a data migration.
/// </summary>
public static class CategoryIcons
{
    public static IReadOnlyList<CategoryIcon> All { get; } =
    [
        new("feather", "🪶", "Feather"),
        new("bicycle", "🚲", "Bicycle"),
        new("scooter", "🛵", "Scooter"),
        new("motorcycle", "🏍️", "Motorcycle"),
        new("car", "🚗", "Car"),
        new("van", "🚐", "Van"),
        new("truck", "🚚", "Truck"),
        new("bus", "🚌", "Bus"),
        new("tractor", "🚜", "Tractor"),
    ];

    public static CategoryIcon? Find(string? key) =>
        All.FirstOrDefault(i => string.Equals(i.Key, key, StringComparison.Ordinal));

    public static bool IsKnown(string? key) => Find(key) is not null;
}
