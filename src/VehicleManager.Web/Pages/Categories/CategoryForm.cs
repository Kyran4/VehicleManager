using System.ComponentModel.DataAnnotations;

namespace VehicleManager.Web.Pages.Categories;

/// <summary>Form fields shared by the create and edit pages. Validation happens in the service layer.</summary>
public sealed class CategoryForm
{
    [Display(Name = "Name")]
    public string? Name { get; set; }

    [Display(Name = "Icon")]
    public string? IconKey { get; set; }

    [Display(Name = "Starts at (kg, inclusive)")]
    public decimal? MinWeightKg { get; set; }
}
