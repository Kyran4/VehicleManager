using VehicleManager.Core.Rules;
using VehicleManager.Core.Services;

namespace VehicleManager.Tests.Unit;

[Trait("Category", "Unit")]
public class VehicleValidatorTests
{
    private const int CurrentYear = 2026;

    private static VehicleInput Valid() => new("John Smith", 1, 2019, 1850.75m);

    private static IReadOnlyList<string> FieldsWithErrors(VehicleInput input) =>
        VehicleValidator.Validate(input, CurrentYear).Select(e => e.Field).ToList();

    [Fact]
    public void Valid_vehicle_has_no_errors()
    {
        Assert.Empty(VehicleValidator.Validate(Valid(), CurrentYear));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Owner_name_is_required(string? owner)
    {
        Assert.Contains(nameof(VehicleInput.OwnerName), FieldsWithErrors(Valid() with { OwnerName = owner }));
    }

    [Fact]
    public void Owner_name_has_a_maximum_length()
    {
        var tooLong = new string('x', VehicleValidator.MaxOwnerNameLength + 1);

        Assert.Contains(nameof(VehicleInput.OwnerName), FieldsWithErrors(Valid() with { OwnerName = tooLong }));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-3)]
    public void Manufacturer_is_required(int? manufacturerId)
    {
        Assert.Contains(nameof(VehicleInput.ManufacturerId), FieldsWithErrors(Valid() with { ManufacturerId = manufacturerId }));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1885)]   // before the first motor vehicle
    [InlineData(2028)]   // beyond next model year
    public void Year_must_be_present_and_sensible(int? year)
    {
        Assert.Contains(nameof(VehicleInput.YearOfManufacture), FieldsWithErrors(Valid() with { YearOfManufacture = year }));
    }

    [Theory]
    [InlineData(1886)]
    [InlineData(2026)]
    [InlineData(2027)]   // next model year is allowed
    public void Sensible_years_are_accepted(int year)
    {
        Assert.DoesNotContain(nameof(VehicleInput.YearOfManufacture), FieldsWithErrors(Valid() with { YearOfManufacture = year }));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("1850.755")]    // three decimal places
    [InlineData("1000000.01")]  // above the sanity ceiling
    public void Weight_must_be_present_positive_two_decimals_and_within_limit(string? weight)
    {
        var input = Valid() with { WeightKg = weight is null ? null : TestData.Kg(weight) };

        Assert.Contains(nameof(VehicleInput.WeightKg), FieldsWithErrors(input));
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("500")]
    [InlineData("1850.75")]
    [InlineData("1850.7")]
    public void Valid_weights_are_accepted(string weight)
    {
        Assert.DoesNotContain(nameof(VehicleInput.WeightKg), FieldsWithErrors(Valid() with { WeightKg = TestData.Kg(weight) }));
    }
}
