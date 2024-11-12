namespace DrifterApps.Seeds.FluentResult;

public partial record Result<T>
{
    /// <summary>
    ///     Creates a new instance of <see cref="Result{T}" /> representing a successful operation.
    /// </summary>
    /// <returns>A new instance of <see cref="Result{T}" /> representing a successful operation.</returns>
    public static Result<T> Success() => new(true, ResultError.None, default);

    /// <summary>
    ///     Creates a new instance of <see cref="Result{T}" /> representing a successful operation.
    /// </summary>
    /// <param name="value">The value associated with the successful operation.</param>
    /// <returns>A new instance of <see cref="Result{T}" /> representing a successful operation.</returns>
    /// <exception cref="ArgumentNullException">value cannot be null</exception>
    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<T>(true, ResultError.None, value);
    }

    /// <summary>
    ///     Creates a new instance of <see cref="Result{T}" /> representing a failed operation.
    /// </summary>
    /// <param name="error">The error associated with the failed operation.</param>
    /// <returns>A new instance of <see cref="Result{T}" /> representing a failed operation.</returns>
    public static Result<T> Failure(ResultError error) =>
     error == ResultError.None
            ? throw new ArgumentException("Invalid error", nameof(error))
            : new Result<T>(false, error, default);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(ResultError error) => Failure(error);

    public Result<T> ToResult() => IsSuccess ? Success(Value) : Failure(Error);
}
