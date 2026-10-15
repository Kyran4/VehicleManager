using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleManager.Core.Services;

namespace VehicleManager.Web.Pages;

public class IndexModel(VehicleService vehicleService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Sort { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Dir { get; set; }

    public IReadOnlyList<VehicleListItem> Items { get; private set; } = [];
    public VehicleSortOptions SortOptions { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        SortOptions = new VehicleSortOptions(
            ParseField(Sort),
            string.Equals(Dir, "desc", StringComparison.OrdinalIgnoreCase));

        Items = await vehicleService.GetVehiclesAsync(SortOptions, ct);
    }

    // Query-string input is untrusted: unknown or numeric values fall back to the default.
    private static VehicleSortField ParseField(string? value) =>
        Enum.TryParse<VehicleSortField>(value, ignoreCase: true, out var field) && Enum.IsDefined(field)
            ? field
            : VehicleSortField.Owner;
}
