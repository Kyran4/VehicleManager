using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleManager.Core.Services;

namespace VehicleManager.Web.Pages.Categories;

public class CreateModel(CategoryService categoryService) : PageModel
{
    [BindProperty]
    public CategoryForm Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            var result = await categoryService.CreateAsync(
                new CategoryInput(Input.Name, Input.IconKey, Input.MinWeightKg), ct);

            if (result.Succeeded)
            {
                TempData["Message"] = $"Category '{Input.Name!.Trim()}' created.";
                return RedirectToPage("Index");
            }

            ModelState.AddServiceErrors(result.Errors, nameof(Input));
        }

        return Page();
    }
}
