using VehicleManager.Core.Rules;

namespace VehicleManager.Tests.Unit;

[Trait("Category", "Unit")]
public class CategoryBandsTests
{
    [Fact]
    public void Upper_bound_is_the_next_categories_lower_bound_and_top_is_unbounded()
    {
        var bands = CategoryBands.Build(TestData.DefaultCategories());

        Assert.Equal(
            new (decimal, decimal?)[] { (0m, 500m), (500m, 2500m), (2500m, null) },
            bands.Select(b => (b.FromKg, b.ToKgExclusive)));
    }

    [Fact]
    public void Bands_are_ordered_by_weight_regardless_of_input_order()
    {
        var input = TestData.DefaultCategories();
        input.Reverse();

        Assert.Equal(
            new[] { "Light", "Medium", "Heavy" },
            CategoryBands.Build(input).Select(b => b.Category.Name));
    }

    [Fact]
    public void Every_weight_falls_in_exactly_one_band()
    {
        var categories = new[]
        {
            TestData.Category(1, "A", 0m),
            TestData.Category(2, "B", 100.5m),
            TestData.Category(3, "C", 500m),
            TestData.Category(4, "D", 2500m),
        };
        var bands = CategoryBands.Build(categories);

        // Every representable weight (0.01 kg steps) up to beyond the last boundary.
        for (var weight = 0.01m; weight <= 3000m; weight += 0.01m)
        {
            Assert.Equal(1, bands.Count(b => b.Contains(weight)));
        }
    }
}
