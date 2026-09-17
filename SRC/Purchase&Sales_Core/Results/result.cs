using System.Diagnostics.CodeAnalysis;

public class Result<T>
{
    public T? Value { get; }
    public ErrorResponse? Error { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(ErrorResponse error)
    {
        Error = error;
        IsSuccess = false;
    }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(ErrorType type, string message) => new(new ErrorResponse(message, type));

    public static implicit operator Result<T>(T value) => Ok(value);
}

public record ErrorResponse(string Error, ErrorType ErrorType);

public enum ErrorType
{
    NotFound,
    Invalid,
    Unauthorized,
    Forbidden,
    Conflict,
    InternalServerError
}