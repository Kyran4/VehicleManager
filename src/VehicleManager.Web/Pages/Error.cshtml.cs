using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VehicleManager.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public int StatusCode { get; private set; } = 500;
    public string? RequestId { get; private set; }

    public string Heading => StatusCode == 404 ? "Page not found" : "Something went wrong";

    public string Message => StatusCode == 404
        ? "We couldn't find what you were looking for. It may have been removed."
        : "An unexpected error occurred. Please try again; if the problem continues, quote the reference below.";

    public void OnGet(int? statusCode)
    {
        StatusCode = statusCode ?? 500;
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}
