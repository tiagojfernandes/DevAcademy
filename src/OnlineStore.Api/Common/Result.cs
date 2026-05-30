namespace OnlineStore.Api.Common;

// Tiny "outcome" wrapper for service methods.
// Lets endpoints pick the HTTP status code without throwing exceptions.
public enum ResultStatus
{
    Ok,
    NotFound,
    Conflict,
    Invalid
}

public record Result<T>(T? Value, ResultStatus Status, string? Error)
{
    public static Result<T> Ok(T value)             => new(value,   ResultStatus.Ok,       null);
    public static Result<T> NotFound(string error)  => new(default, ResultStatus.NotFound, error);
    public static Result<T> Conflict(string error)  => new(default, ResultStatus.Conflict, error);
    public static Result<T> Invalid(string error)   => new(default, ResultStatus.Invalid,  error);
}
