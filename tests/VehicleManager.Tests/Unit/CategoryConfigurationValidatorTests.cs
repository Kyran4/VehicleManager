using VehicleManager.Core.Domain;
using VehicleManager.Core.Rules;

namespace VehicleManager.Tests.Unit;

[Trait("Category", "Unit")]
public class CategoryConfigurationValidatorTests
{
    private static readonly string MinField = nameof(VehicleCategory.MinWeightKg);

    [Fact]
    public void Default_configuration_is_valid()
    {
        Assert.Empty(CategoryConfigurationValidator.Validate(TestData.DefaultCategories()));
    }

    [Fact]
    public void Empty_configuration_is_rejected()
    {
        Assert.NotEmpty(CategoryConfigurationValidator.Validate([]));
    }

    [Fact]
    public void Gap_below_the_lowest_category_is_rejected()
    {
        // Nothing covers 0..600 kg.
        var categories = new[] { TestData.Category(1, "Medium", 600m), TestData.Category(2, "Heavy", 2500m) };

        var errors = CategoryConfigurationValidator.Validate(categories);

        Assert.Contains(errors, e => e.Field == MinField && e.Message.Contains("start at 0"));
    }

    [Fact]
    public void Two_categories_starting_at_the_same_weight_would_overlap_and_are_rejected()
    {
        var categories = TestData.DefaultCategories();
        categories.Add(TestData.Category(4, "Also medium", 500m));

        var errors = CategoryConfigurationValidator.Validate(categories);

        Assert.Contains(errors, e => e.Field == MinField && e.Message.Contains("overlap"));
    }

    [Fact]
    public void Duplicate_names_are_rejected_case_insensitively()
    {
        var categories = TestData.DefaultCategories();
        categories.Add(TestData.Category(4, "  heavy ", 3000m));

        Assert.Contains(CategoryConfigurationValidator.Validate(categories),
            e => e.Field == nameof(VehicleCategory.Name));
    }

    [Theory]
    [InlineData(null, "car", "0")]
    [InlineData("", "car", "0")]
    [InlineData("   ", "car", "0")]
    [InlineData("Light", null, "0")]
    [InlineData("Light", "", "0")]
    [InlineData("Light", "not-an-icon", "0")]
    [InlineData("Light", "car", "-1")]
    [InlineData("Light", "car", "10.005")]       // three decimal places
    [InlineData("Light", "car", "99999999")]     // above the sanity ceiling
    public void Invalid_definitions_are_rejected(string? name, string? icon, string min)
    {
        Assert.NotEmpty(CategoryConfigurationValidator.ValidateDefinition(name, icon, TestData.Kg(min)));
    }

    [Fact]
    public void Missing_starting_weight_is_rejected()
    {
        Assert.Contains(CategoryConfigurationValidator.ValidateDefinition("Light", "car", null),
            e => e.Field == MinField);
    }

    [Fact]
    public void A_valid_definition_has_no_errors()
    {
        Assert.Empty(CategoryConfigurationValidator.ValidateDefinition("Light", "feather", 0m));
    }
}
