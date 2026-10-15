using VehicleManager.Core.Common;
using VehicleManager.Core.Services;

namespace VehicleManager.Core.Rules;

public static class VehicleValidator
{
    public const int MaxOwnerNameLength = 100;
    public const int EarliestYear = 1886; // first practical motor vehicle

    /// <summary>Pure validation. Checking that the manufacturer exists needs the database, so the service does that.</summary>
    public static IReadOnlyList<ValidationError> Validate(VehicleInput input, int currentYear)
    {
        var errors = new List<ValidationError>();

        var owner = input.OwnerName?.Trim();
        if (string.IsNullOrEmpty(owner))
            errors.Add(new(nameof(VehicleInput.OwnerName), "Owner's name is required."));
        else if (owner.Length > MaxOwnerNameLength)
            errors.Add(new(nameof(VehicleInput.OwnerName), $"Owner's name must be {MaxOwnerNameLength} characters or fewer."));

        if (input.ManufacturerId is null or <= 0)
            errors.Add(new(nameof(VehicleInput.ManufacturerId), "Manufacturer is required."));

        var latestYear = currentYear + 1; // allow next model year
        if (input.YearOfManufacture is null)
            errors.Add(new(nameof(VehicleInput.YearOfManufacture), "Year of manufacture is required."));
        else if (input.YearOfManufacture < EarliestYear || input.YearOfManufacture > latestYear)
            errors.Add(new(nameof(VehicleInput.YearOfManufacture),
                $"Year of manufacture must be between {EarliestYear} and {latestYear}."));

        if (input.WeightKg is null)
        {
            errors.Add(new(nameof(VehicleInput.WeightKg), "Weight is required."));
        }
        else
        {
            var weight = input.WeightKg.Value;
            if (weight <= 0)
                errors.Add(new(nameof(VehicleInput.WeightKg), "Weight must be greater than 0 kg."));
            else if (!WeightLimits.HasValidPrecision(weight))
                errors.Add(new(nameof(VehicleInput.WeightKg), "Weight can have at most two decimal places."));
            else if (weight > WeightLimits.MaxKg)
                errors.Add(new(nameof(VehicleInput.WeightKg), $"Weight must not exceed {WeightLimits.MaxKg:N2} kg."));
        }

        return errors;
    }
}
