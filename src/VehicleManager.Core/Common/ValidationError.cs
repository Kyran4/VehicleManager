namespace VehicleManager.Core.Common;

/// <summary>A validation problem, optionally tied to an input field (empty field = general error).</summary>
public sealed record ValidationError(string Field, string Message)
{
    public const string General = "";
}
