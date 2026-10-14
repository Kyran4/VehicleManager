namespace VehicleManager.Core.Common;

public enum ResultStatus
{
    Success,
    ValidationFailed,
    NotFound,
}

/// <summary>Outcome of a service operation. Expected failures are values, not exceptions.</summary>
public class Result
{
    protected Result(ResultStatus status, IReadOnlyList<ValidationError> errors)
    {
        Status = status;
        Errors = errors;
    }

    public ResultStatus Status { get; }
    public IReadOnlyList<ValidationError> Errors { get; }
    public bool Succeeded => Status == ResultStatus.Success;

    public static Result Ok() => new(ResultStatus.Success, []);

    public static Result Invalid(IEnumerable<ValidationError> errors) =>
        new(ResultStatus.ValidationFailed, errors.ToList());

    public static Result Invalid(string field, string message) =>
        new(ResultStatus.ValidationFailed, [new ValidationError(field, message)]);

    public static Result NotFound(string message) =>
        new(ResultStatus.NotFound, [new ValidationError(ValidationError.General, message)]);
}

public sealed class Result<T> : Result
{
    private Result(ResultStatus status, IReadOnlyList<ValidationError> errors, T? value)
        : base(status, errors) => Value = value;

    public T? Value { get; }

    public static Result<T> Ok(T value) => new(ResultStatus.Success, [], value);

    public static Result<T> From(Result failure) => new(failure.Status, failure.Errors, default);
}
