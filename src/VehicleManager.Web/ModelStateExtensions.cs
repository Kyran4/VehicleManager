using Microsoft.AspNetCore.Mvc.ModelBinding;
using VehicleManager.Core.Common;

namespace VehicleManager.Web;

public static class ModelStateExtensions
{
    /// <summary>Maps service validation errors onto form fields (general errors go to the summary).</summary>
    public static void AddServiceErrors(this ModelStateDictionary modelState, IEnumerable<ValidationError> errors, string formPrefix)
    {
        foreach (var error in errors)
        {
            var key = string.IsNullOrEmpty(error.Field) ? string.Empty : $"{formPrefix}.{error.Field}";
            modelState.AddModelError(key, error.Message);
        }
    }
}
