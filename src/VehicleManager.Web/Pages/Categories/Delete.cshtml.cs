using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleManager.Core.Common;
using VehicleManager.Core.Rules;
using VehicleManager.Core.Services;

namespace VehicleManager.Web.Pages.Categories;

public class DeleteModel(CategoryService categoryService) : PageModel
{
    public CategoryBand? Band { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        Band = await categoryService.GetBandAsync(id, ct);
        return Band is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var band = await categoryService.GetBandAsync(id, ct);
        if (band is null)
            return NotFound();

        var result = await categoryService.DeleteAsync(id, ct);
        if (result.Status == ResultStatus.NotFound)
            return NotFound();

        if (result.Succeeded)
        {
            TempData["Message"] = $"Category '{band.Category.Name}' deleted.";
            return RedirectToPage("Index");
        }

        // This page has no form fields, so every error is shown in the summary.
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Message);

        Band = band;
        return Page();
    }
}
