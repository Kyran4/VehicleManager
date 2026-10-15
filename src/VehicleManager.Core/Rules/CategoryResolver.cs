using VehicleManager.Core.Domain;

namespace VehicleManager.Core.Rules;

public static class CategoryResolver
{
    /// <summary>
    /// Returns the category with the greatest lower bound that is &lt;= the weight. With lower bounds
    /// inclusive, a weight exactly on a boundary belongs to the category that starts there.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// No category covers the weight - only possible if the stored configuration violates its invariants.
    /// </exception>
    public static VehicleCategory Resolve(IEnumerable<VehicleCategory> categories, decimal weightKg)
    {
        VehicleCategory? best = null;
        foreach (var category in categories)
        {
            if (category.MinWeightKg <= weightKg && (best is null || category.MinWeightKg > best.MinWeightKg))
            {
                best = category;
            }
        }

        return best ?? throw new InvalidOperationException(
            $"No category covers a weight of {weightKg} kg. The category configuration is invalid.");
    }
}
