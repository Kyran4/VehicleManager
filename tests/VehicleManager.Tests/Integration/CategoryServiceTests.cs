using System.Globalization;
using VehicleManager.Core.Common;
using VehicleManager.Core.Domain;
using VehicleManager.Core.Services;

namespace VehicleManager.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class CategoryServiceTests : IDisposable
{
    // Ids of the seeded categories and manufacturers (see the EF configurations).
    private const int LightId = 1;
    private const int MediumId = 2;
    private const int HeavyId = 3;
    private const int MazdaId = 1;

    // Ranges are rendered as "Name from-to" (to = "max" for the unbounded top category) to keep assertions readable.
    private const string DefaultRanges = "Light 0-500; Medium 500-2500; Heavy 2500-max";

    private readonly SqliteDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private async Task<string> RangesAsync()
    {
        var bands = await _db.CreateCategoryService().GetBandsAsync();
        return string.Join("; ", bands.Select(b =>
            $"{b.Category.Name} {Format(b.FromKg)}-{(b.ToKgExclusive is { } to ? Format(to) : "max")}"));

        static string Format(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private async Task<IReadOnlyList<VehicleListItem>> ListVehiclesAsync() =>
        await _db.CreateVehicleService().GetVehiclesAsync(new VehicleSortOptions());

    private async Task<int> AddVehicleAsync(string owner, decimal weightKg)
    {
        var result = await _db.CreateVehicleService().AddVehicleAsync(new VehicleInput(owner, MazdaId, 2020, weightKg));
        Assert.True(result.Succeeded);
        return result.Value;
    }

    [Fact]
    public async Task Seed_data_contains_the_initial_categories()
    {
        Assert.Equal(DefaultRanges, await RangesAsync());
    }

    [Fact]
    public async Task Creating_a_category_inside_an_existing_range_splits_it_without_gaps_or_overlaps()
    {
        var result = await _db.CreateCategoryService().CreateAsync(new CategoryInput("Compact", "van", 1000m));

        Assert.True(result.Succeeded);
        Assert.Equal("Light 0-500; Medium 500-1000; Compact 1000-2500; Heavy 2500-max", await RangesAsync());
    }

    [Fact]
    public async Task Creating_a_category_that_starts_where_another_starts_is_rejected()
    {
        var result = await _db.CreateCategoryService().CreateAsync(new CategoryInput("Clash", "van", 500m));

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, e => e.Message.Contains("overlap"));
        Assert.Equal(DefaultRanges, await RangesAsync()); // nothing was persisted
    }

    [Fact]
    public async Task Creating_an_invalid_category_reports_every_problem_and_persists_nothing()
    {
        var result = await _db.CreateCategoryService().CreateAsync(new CategoryInput("", "", null));

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, e => e.Field == nameof(VehicleCategory.Name));
        Assert.Contains(result.Errors, e => e.Field == nameof(VehicleCategory.IconKey));
        Assert.Contains(result.Errors, e => e.Field == nameof(VehicleCategory.MinWeightKg));
        Assert.Equal(DefaultRanges, await RangesAsync());
    }

    [Fact]
    public async Task Moving_a_boundary_recategorises_existing_vehicles_immediately()
    {
        // The brief's example: a 2200 kg vehicle is Medium...
        await AddVehicleAsync("Jane Turei", 2200m);
        Assert.Equal("Medium", (await ListVehiclesAsync()).Single().Category.Name);

        // ...until Medium is changed to end at 2000 kg (Heavy begins at 2000 kg).
        var update = await _db.CreateCategoryService().UpdateAsync(HeavyId, new CategoryInput("Heavy", "truck", 2000m));

        Assert.True(update.Succeeded);
        Assert.Equal("Heavy", (await ListVehiclesAsync()).Single().Category.Name);
    }

    [Theory]
    [InlineData("499.99", "Light")]
    [InlineData("500.00", "Medium")]
    [InlineData("2499.99", "Medium")]
    [InlineData("2500.00", "Heavy")]
    public async Task Vehicles_on_a_boundary_belong_to_exactly_one_category(string weight, string expected)
    {
        var id = await AddVehicleAsync("Boundary Owner", TestData.Kg(weight));

        Assert.Equal(expected, (await ListVehiclesAsync()).Single(v => v.Id == id).Category.Name);
    }

    [Fact]
    public async Task Renaming_and_changing_the_icon_keeps_the_ranges()
    {
        var result = await _db.CreateCategoryService().UpdateAsync(MediumId, new CategoryInput("Mid-size", "bus", 500m));

        Assert.True(result.Succeeded);
        Assert.Equal("Light 0-500; Mid-size 500-2500; Heavy 2500-max", await RangesAsync());
    }

    [Fact]
    public async Task Moving_the_lowest_category_away_from_zero_is_rejected()
    {
        var result = await _db.CreateCategoryService().UpdateAsync(LightId, new CategoryInput("Light", "feather", 100m));

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Equal(DefaultRanges, await RangesAsync());
    }

    [Fact]
    public async Task Moving_a_category_onto_another_categorys_start_is_rejected()
    {
        var result = await _db.CreateCategoryService().UpdateAsync(HeavyId, new CategoryInput("Heavy", "truck", 500m));

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, e => e.Message.Contains("overlap"));
        Assert.Equal(DefaultRanges, await RangesAsync());
    }

    [Fact]
    public async Task Updating_a_missing_category_returns_not_found()
    {
        var result = await _db.CreateCategoryService().UpdateAsync(999, new CategoryInput("X", "car", 10m));

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Deleting_a_middle_category_lets_the_one_below_absorb_its_range()
    {
        var result = await _db.CreateCategoryService().DeleteAsync(MediumId);

        Assert.True(result.Succeeded);
        Assert.Equal("Light 0-2500; Heavy 2500-max", await RangesAsync());
    }

    [Fact]
    public async Task Deleting_the_top_category_lets_the_one_below_become_unbounded()
    {
        var result = await _db.CreateCategoryService().DeleteAsync(HeavyId);

        Assert.True(result.Succeeded);
        Assert.Equal("Light 0-500; Medium 500-max", await RangesAsync());
    }

    [Fact]
    public async Task Deleting_the_lowest_category_extends_the_next_one_down_to_zero()
    {
        var result = await _db.CreateCategoryService().DeleteAsync(LightId);

        Assert.True(result.Succeeded);
        Assert.Equal("Medium 0-2500; Heavy 2500-max", await RangesAsync());
    }

    [Fact]
    public async Task Deleting_the_last_remaining_category_is_rejected()
    {
        Assert.True((await _db.CreateCategoryService().DeleteAsync(LightId)).Succeeded);
        Assert.True((await _db.CreateCategoryService().DeleteAsync(MediumId)).Succeeded);

        var result = await _db.CreateCategoryService().DeleteAsync(HeavyId);

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Equal("Heavy 0-max", await RangesAsync());
    }

    [Fact]
    public async Task Every_vehicle_still_resolves_after_a_category_is_deleted()
    {
        await AddVehicleAsync("A", 100m);
        await AddVehicleAsync("B", 3000m);

        await _db.CreateCategoryService().DeleteAsync(LightId);

        var list = await ListVehiclesAsync();
        Assert.Equal("Medium", list.Single(v => v.OwnerName == "A").Category.Name);
        Assert.Equal("Heavy", list.Single(v => v.OwnerName == "B").Category.Name);
    }

    [Fact]
    public async Task Deleting_a_missing_category_returns_not_found()
    {
        var result = await _db.CreateCategoryService().DeleteAsync(999);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }
}
