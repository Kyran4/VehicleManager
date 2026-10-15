using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleManager.Core.Common;
using VehicleManager.Core.Services;

namespace VehicleManager.Web.Pages.Categories;

public class EditModel(CategoryService categoryService) : PageModel
{
    [BindProperty]
    public CategoryForm Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var band = await categoryService.GetBandAsync(id, ct);
        if (band is null)
            return NotFound();

        Input = new CategoryForm
        {
            Name = band.Category.Name,
            IconKey = band.Category.IconKey,
            MinWeightKg = band.Category.MinWeightKg,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            var result = await categoryService.UpdateAsync(
                id, new CategoryInput(Input.Name, Input.IconKey, Input.MinWeightKg), ct);

            if (result.Succeeded)
            {
                TempData["Message"] = $"Category '{Input.Name!.Trim()}' updated.";
                return RedirectToPage("Index");
            }

            if (result.Status == ResultStatus.NotFound)
                return NotFound();

            ModelState.AddServiceErrors(result.Errors, nameof(Input));
        }

        return Page();
    }
}
