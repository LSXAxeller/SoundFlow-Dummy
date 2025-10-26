namespace SoundFlow.Structs;

/// <summary>
/// Represents the outcome of an operation, which can be either a success or a failure.
/// </summary>
public readonly struct Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IError? Error { get; }

    private Result(bool isSuccess, IError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Ok() => new(true, null);
    public static Result Fail(IError error) => new(false, error);
    
    public static implicit operator Result(Error error) => Fail(error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value on success.
/// </summary>
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    
    public T? Value { get; }
    
    public IError? Error { get; }

    private Result(bool isSuccess, T? value, IError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(IError error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Ok(value);
    public static implicit operator Result<T>(Error error) => Fail(error);
    public static implicit operator Result(Result<T> result) => result.IsSuccess ? Result.Ok() : Result.Fail(result.Error!);
}