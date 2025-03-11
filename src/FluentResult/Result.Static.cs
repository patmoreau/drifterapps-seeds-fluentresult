namespace DrifterApps.Seeds.FluentResult;

public partial struct Result<T>
{
    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing the value.</returns>
    public static implicit operator Result<T>(T value) => new(true, ResultError.None, value);

    /// <summary>
    /// Implicitly converts a <see cref="ResultError"/> to a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failed <see cref="Result{T}"/> containing the error.</returns>
    public static implicit operator Result<T>(ResultError error) =>  new(false, error, default);

    /// <summary>
    /// Converts the current instance to a <see cref="Result{T}"/>.
    /// </summary>
    /// <returns>A successful or failed <see cref="Result{T}"/> based on the current instance.</returns>
    public Result<T> ToResult() => IsSuccess ? Value : (Result<T>)Error;
}
