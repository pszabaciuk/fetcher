namespace Fetcher.DTOs;

public sealed class Result<T>
{
    public bool IsSuccess { get; set; }

    public T Value { get; set; } = default!;

    public string Error { get; set; } = default!;

    public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };
    public static Result<T> Failure(string error) => new Result<T> { IsSuccess = false, Error = error };
}

public sealed class Empty
{

}