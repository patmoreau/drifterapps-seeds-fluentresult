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
    public static implicit operator Result<T>(ResultError error) => new(false, error, default);

    /// <summary>
    /// Converts the current instance to a <see cref="Result{T}"/>.
    /// </summary>
    /// <returns>A successful or failed <see cref="Result{T}"/> based on the current instance.</returns>
    public Result<T> ToResult() => IsSuccess ? Value : (Result<T>) Error;

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing the value.</returns>
    public static implicit operator Task<Result<T>>(Result<T> value) =>
        Task.FromResult(value);

    /// <summary>
    /// Converts the current instance to a <see cref="Result{T}"/>.
    /// </summary>
    /// <returns>A successful or failed <see cref="Result{T}"/> based on the current instance.</returns>
    public Task<Result<T>> ToTask() => Task.FromResult(this);

    /// <summary>
    /// Implicitly converts a value of type <see cref="Result{T}"/> to a <typeparamref name="T"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>The <typeparamref name="T"/> value.</returns>
    /// <exception cref="InvalidOperationException">When the result is failure.</exception>
    public static implicit operator T(Result<T> result) => result.Value;

    /// <summary>
    /// Implicitly converts a <see cref="Result{T}"/> to a <see cref="ResultError"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns><see cref="ResultError"/>.</returns>
    /// <remarks>Will return ResultError.None when is successful result.</remarks>
    public static implicit operator ResultError(Result<T> result) => result.Error;

    /// <summary>
    /// Converts the current instance to a <see cref="Result{T}"/>.
    /// </summary>
    /// <returns>A successful or failed <see cref="Result{T}"/> based on the current instance.</returns>
    public T FromResult() => Value;
}
