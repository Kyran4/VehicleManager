using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleManager.Core.Services;

namespace VehicleManager.Web.Pages.Vehicles;

public class CreateModel(VehicleService vehicleService) : PageModel
{
    [BindProperty]
    public VehicleForm Input { get; set; } = new();

    public IReadOnlyList<SelectListItem> Manufacturers { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct) => await LoadManufacturersAsync(ct);

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        // ModelState is already invalid here if binding failed (e.g. "abc" typed into a number field).
        if (ModelState.IsValid)
        {
            var result = await vehicleService.AddVehicleAsync(
                new VehicleInput(Input.OwnerName, Input.ManufacturerId, Input.YearOfManufacture, Input.WeightKg), ct);

            if (result.Succeeded)
            {
                TempData["Message"] = "Vehicle added.";
                return RedirectToPage("/Index");
            }

            ModelState.AddServiceErrors(result.Errors, nameof(Input));
        }

        await LoadManufacturersAsync(ct);
        return Page();
    }

    private async Task LoadManufacturersAsync(CancellationToken ct) =>
        Manufacturers = (await vehicleService.GetManufacturersAsync(ct))
            .Select(m => new SelectListItem(m.Name, m.Id.ToString()))
            .ToList();

    public sealed class VehicleForm
    {
        [Display(Name = "Owner's name")]
        public string? OwnerName { get; set; }

        [Display(Name = "Manufacturer")]
        public int? ManufacturerId { get; set; }

        [Display(Name = "Year of manufacture")]
        public int? YearOfManufacture { get; set; }

        [Display(Name = "Weight (kg)")]
        public decimal? WeightKg { get; set; }
    }
}
