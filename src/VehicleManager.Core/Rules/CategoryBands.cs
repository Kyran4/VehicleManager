using VehicleManager.Core.Domain;

namespace VehicleManager.Core.Rules;

/// <summary>A category together with its derived range: [FromKg, ToKgExclusive). Null upper bound = unbounded.</summary>
public sealed record CategoryBand(VehicleCategory Category, decimal FromKg, decimal? ToKgExclusive)
{
    public bool Contains(decimal weightKg) =>
        weightKg >= FromKg && (ToKgExclusive is null || weightKg < ToKgExclusive);
}

public static class CategoryBands
{
    /// <summary>Derives each category's upper bound from the next category's lower bound.</summary>
    public static IReadOnlyList<CategoryBand> Build(IEnumerable<VehicleCategory> categories)
    {
        var ordered = categories.OrderBy(c => c.MinWeightKg).ToList();
        return ordered
            .Select((c, i) => new CategoryBand(
                c,
                c.MinWeightKg,
                i + 1 < ordered.Count ? ordered[i + 1].MinWeightKg : null))
            .ToList();
    }
}
