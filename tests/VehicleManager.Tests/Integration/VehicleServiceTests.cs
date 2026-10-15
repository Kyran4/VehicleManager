using VehicleManager.Core.Common;
using VehicleManager.Core.Services;

namespace VehicleManager.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class VehicleServiceTests : IDisposable
{
    // Seeded manufacturer ids: 1 Mazda, 2 Mercedes, 3 Honda, 4 Ferrari, 5 Toyota.
    private readonly SqliteDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private async Task SeedVehiclesAsync()
    {
        foreach (var input in new[]
                 {
                     new VehicleInput("Alice", 1, 2019, 1850.75m),   // Mazda
                     new VehicleInput("Bob", 5, 2001, 3000m),        // Toyota
                     new VehicleInput("Charlie", 3, 2010, 450m),     // Honda
                 })
        {
            Assert.True((await _db.CreateVehicleService().AddVehicleAsync(input)).Succeeded);
        }
    }

    [Fact]
    public async Task Added_vehicle_is_persisted_and_listed_with_its_calculated_category()
    {
        var result = await _db.CreateVehicleService().AddVehicleAsync(new VehicleInput("  John Smith ", 1, 2019, 1850.75m));

        var item = Assert.Single(await _db.CreateVehicleService().GetVehiclesAsync(new VehicleSortOptions()));
        Assert.True(result.Succeeded);
        Assert.Equal("John Smith", item.OwnerName);   // trimmed
        Assert.Equal("Mazda", item.Manufacturer);
        Assert.Equal(2019, item.YearOfManufacture);
        Assert.Equal(1850.75m, item.WeightKg);
        Assert.Equal("Medium", item.Category.Name);
    }

    [Fact]
    public async Task Invalid_vehicle_is_rejected_with_field_errors_and_nothing_is_saved()
    {
        var result = await _db.CreateVehicleService().AddVehicleAsync(new VehicleInput("", null, 1700, -5m));

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Equal(4, result.Errors.Select(e => e.Field).Distinct().Count());
        Assert.Empty(await _db.CreateVehicleService().GetVehiclesAsync(new VehicleSortOptions()));
    }

    [Fact]
    public async Task Unknown_manufacturer_is_rejected()
    {
        var result = await _db.CreateVehicleService().AddVehicleAsync(new VehicleInput("Alice", 999, 2019, 1000m));

        Assert.Equal(ResultStatus.ValidationFailed, result.Status);
        Assert.Contains(result.Errors, e => e.Field == nameof(VehicleInput.ManufacturerId));
    }

    [Fact]
    public async Task Manufacturers_are_seeded()
    {
        var names = (await _db.CreateVehicleService().GetManufacturersAsync()).Select(m => m.Name);

        Assert.Equal(new[] { "Ferrari", "Honda", "Mazda", "Mercedes", "Toyota" }, names);
    }

    [Theory]
    [InlineData(VehicleSortField.Owner, false, "Alice,Bob,Charlie")]
    [InlineData(VehicleSortField.Owner, true, "Charlie,Bob,Alice")]
    [InlineData(VehicleSortField.Manufacturer, false, "Charlie,Alice,Bob")]   // Honda, Mazda, Toyota
    [InlineData(VehicleSortField.Manufacturer, true, "Bob,Alice,Charlie")]
    [InlineData(VehicleSortField.Year, false, "Bob,Charlie,Alice")]           // 2001, 2010, 2019
    [InlineData(VehicleSortField.Year, true, "Alice,Charlie,Bob")]
    [InlineData(VehicleSortField.Weight, false, "Charlie,Alice,Bob")]         // 450, 1850.75, 3000
    [InlineData(VehicleSortField.Weight, true, "Bob,Alice,Charlie")]
    public async Task Vehicles_can_be_sorted_by_each_field_in_both_directions(VehicleSortField field, bool descending, string expected)
    {
        await SeedVehiclesAsync();

        var list = await _db.CreateVehicleService().GetVehiclesAsync(new VehicleSortOptions(field, descending));

        Assert.Equal(expected, string.Join(",", list.Select(v => v.OwnerName)));
    }
}
