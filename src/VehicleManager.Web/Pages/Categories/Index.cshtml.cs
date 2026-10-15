using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleManager.Core.Rules;
using VehicleManager.Core.Services;

namespace VehicleManager.Web.Pages.Categories;

public class IndexModel(CategoryService categoryService) : PageModel
{
    public IReadOnlyList<CategoryBand> Bands { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct) => Bands = await categoryService.GetBandsAsync(ct);
}
