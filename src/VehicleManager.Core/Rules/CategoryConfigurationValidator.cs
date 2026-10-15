using VehicleManager.Core.Common;
using VehicleManager.Core.Domain;

namespace VehicleManager.Core.Rules;

/// <summary>
/// Validates a complete set of categories. A configuration is valid when every weight resolves to
/// exactly one category: the lowest category starts at 0 kg (no gap below) and no two categories
/// share a starting weight (no overlap). Everything in between follows from the model.
/// </summary>
public static class CategoryConfigurationValidator
{
    public const int MaxNameLength = 50;

    public static IReadOnlyList<ValidationError> ValidateDefinition(string? name, string? iconKey, decimal? minWeightKg)
    {
        var errors = new List<ValidationError>();

        var trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            errors.Add(new(nameof(VehicleCategory.Name), "Category name is required."));
        else if (trimmed.Length > MaxNameLength)
            errors.Add(new(nameof(VehicleCategory.Name), $"Category name must be {MaxNameLength} characters or fewer."));

        if (string.IsNullOrEmpty(iconKey))
            errors.Add(new(nameof(VehicleCategory.IconKey), "Category icon is required."));
        else if (!CategoryIcons.IsKnown(iconKey))
            errors.Add(new(nameof(VehicleCategory.IconKey), "Select an icon from the list."));

        if (minWeightKg is null)
            errors.Add(new(nameof(VehicleCategory.MinWeightKg), "Starting weight is required."));
        else if (minWeightKg < 0)
            errors.Add(new(nameof(VehicleCategory.MinWeightKg), "Starting weight cannot be negative."));
        else if (!WeightLimits.HasValidPrecision(minWeightKg.Value))
            errors.Add(new(nameof(VehicleCategory.MinWeightKg), "Starting weight can have at most two decimal places."));
        else if (minWeightKg > WeightLimits.MaxKg)
            errors.Add(new(nameof(VehicleCategory.MinWeightKg), $"Starting weight must not exceed {WeightLimits.MaxKg:N2} kg."));

        return errors;
    }

    public static IReadOnlyList<ValidationError> Validate(IReadOnlyCollection<VehicleCategory> categories)
    {
        if (categories.Count == 0)
        {
            return [new(ValidationError.General,
                "At least one category is required so that every vehicle weight maps to a category.")];
        }

        var errors = new List<ValidationError>();

        foreach (var category in categories)
            errors.AddRange(ValidateDefinition(category.Name, category.IconKey, category.MinWeightKg));

        foreach (var duplicate in categories
                     .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                     .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Where(g => g.Count() > 1))
        {
            errors.Add(new(nameof(VehicleCategory.Name), $"A category named '{duplicate.Key}' already exists."));
        }

        foreach (var duplicate in categories.GroupBy(c => c.MinWeightKg).Where(g => g.Count() > 1))
        {
            errors.Add(new(nameof(VehicleCategory.MinWeightKg),
                $"More than one category would start at {duplicate.Key:N2} kg, so their ranges would overlap."));
        }

        var lowest = categories.Min(c => c.MinWeightKg);
        if (lowest > 0)
        {
            errors.Add(new(nameof(VehicleCategory.MinWeightKg),
                $"The lowest category must start at 0 kg, otherwise weights below {lowest:N2} kg would have no category."));
        }

        return errors;
    }
}
