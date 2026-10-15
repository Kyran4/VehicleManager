using VehicleManager.Core.Rules;

namespace VehicleManager.Tests.Unit;

[Trait("Category", "Unit")]
public class CategoryResolverTests
{
    [Theory]
    [InlineData("0.01", "Light")]
    [InlineData("499.99", "Light")]
    [InlineData("500", "Medium")]      // boundary: belongs to the category that STARTS here
    [InlineData("500.00", "Medium")]
    [InlineData("2499.99", "Medium")]
    [InlineData("2500", "Heavy")]      // boundary
    [InlineData("1000000", "Heavy")]
    public void Resolves_default_categories_including_boundaries(string weight, string expected)
    {
        var category = CategoryResolver.Resolve(TestData.DefaultCategories(), TestData.Kg(weight));

        Assert.Equal(expected, category.Name);
    }

    [Theory]
    [InlineData("1999.99", "Medium")]
    [InlineData("2000", "Heavy")]
    [InlineData("2200", "Heavy")]      // the brief's worked example: was Medium, now Heavy
    public void Resolves_using_the_current_configuration_after_a_boundary_moves(string weight, string expected)
    {
        var categories = TestData.DefaultCategories();
        categories[2].MinWeightKg = 2000m; // Medium now ends at 2000, Heavy begins at 2000

        Assert.Equal(expected, CategoryResolver.Resolve(categories, TestData.Kg(weight)).Name);
    }

    [Fact]
    public void Order_of_the_input_does_not_matter()
    {
        var shuffled = TestData.DefaultCategories();
        shuffled.Reverse();

        Assert.Equal("Medium", CategoryResolver.Resolve(shuffled, 1000m).Name);
    }

    [Fact]
    public void Throws_when_the_configuration_does_not_cover_the_weight()
    {
        var invalid = new[] { TestData.Category(1, "Medium", 500m) };

        Assert.Throws<InvalidOperationException>(() => CategoryResolver.Resolve(invalid, 100m));
    }
}
