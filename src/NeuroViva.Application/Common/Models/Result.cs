namespace NeuroViva.Application.Common.Models;

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T value) { IsSuccess = true; _value = value; }
    private Result(Error error) { IsSuccess = false; _error = error; }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    public Error Error => IsFailure
        ? _error!.Value
        : throw new InvalidOperationException("Cannot access error of a successful result.");

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}

public sealed class Result
{
    private readonly Error? _error;

    private Result() { IsSuccess = true; }
    private Result(Error error) { IsSuccess = false; _error = error; }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsFailure
        ? _error!.Value
        : throw new InvalidOperationException("Cannot access error of a successful result.");

    public static readonly Result Ok = new();
    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);
}
