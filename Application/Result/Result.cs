namespace Application.Common;

public class Result
{
    protected Result(bool isSuccess, string? message, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorType = errorType;
    }

    public bool IsSuccess { get; }
    public string? Message { get; }
    public ErrorType ErrorType { get; }

    public static Result Success(string? message = null) => new(true, message, ErrorType.None);
    public static Result Failure(string message, ErrorType errorType = ErrorType.Failure) => new(false, message, errorType);
}

public class Result<T> : Result
{
    private Result(bool isSuccess, T? value, string? message, ErrorType errorType)
        : base(isSuccess, message, errorType)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value, string? message = null) => new(true, value, message, ErrorType.None);
    public new static Result<T> Failure(string message, ErrorType errorType = ErrorType.Failure) => new(false, default, message, errorType);
}
