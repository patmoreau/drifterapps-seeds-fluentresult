namespace DrifterApps.Seeds.FluentResult;

public partial struct Result<T>
{
    /// <summary>
    ///     Executes the next function if the result is successful and returns a result of type <typeparamref name="TOut" />.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="next">The function to execute if the result is successful.</param>
    /// <returns>The result of the next function or a failure result.</returns>
    public Result<TOut> OnSuccess<TOut>(Func<T, Result<TOut>> next) => Switch(next, error => error);

    /// <summary>
    ///     Executes the next function if the result is successful and returns a result of type <typeparamref name="TOut" />.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="next">The function to execute if the result is successful.</param>
    /// <returns>The result of the next function or a failure result.</returns>
    public Task<Result<TOut>> OnSuccess<TOut>(Func<T, Task<Result<TOut>>> next) =>
        Switch(next, error => Task.FromResult((Result<TOut>) error));

    /// <summary>
    ///     Executes the next action if the result is successful and returns a result of type <see cref="Nothing"/> />.
    /// </summary>
    /// <param name="next">The function to execute if the result is successful.</param>
    /// <returns>The result of the next function or a failure result.</returns>
    public Result<Nothing> OnSuccess(Action<T> next) => Switch(next, _ => {});

    /// <summary>
    ///     Executes the next action if the result is successful and returns a result of type <see cref="Nothing" />.
    /// </summary>
    /// <param name="next">The function to execute if the result is successful.</param>
    /// <returns>The result of the next function or a failure result.</returns>
    public Task<Result<Nothing>> OnSuccess(Func<T, Task> next) => Switch(next, _ => Task.CompletedTask);

    /// <summary>
    ///     Executes the next function if the result is a failure.
    /// </summary>
    /// <param name="next">The function to execute if the result is a failure.</param>
    /// <returns>The result of the next function or the initial result.</returns>
    public Result<T> OnFailure(Func<ResultError, Result<T>> next) => Switch(arg => arg, next);

    /// <summary>
    ///     Executes the next function if the result is a failure.
    /// </summary>
    /// <param name="next">The function to execute if the result is a failure.</param>
    /// <returns>The result of the next function or the initial result.</returns>
    public Task<Result<T>> OnFailure(Func<ResultError, Task<Result<T>>> next) =>
        Switch(arg => Task.FromResult((Result<T>) arg), next);

    /// <summary>
    ///     Executes the next function if the result is a failure.
    /// </summary>
    /// <param name="next">The function to execute if the result is a failure.</param>
    /// <returns>The result of the next function or the initial result.</returns>
    public Result<Nothing> OnFailure(Action<ResultError> next) => Switch(_ => { }, next);

    /// <summary>
    ///     Executes the next function if the result is a failure.
    /// </summary>
    /// <param name="next">The function to execute if the result is a failure.</param>
    /// <returns>The result of the next function or the initial result.</returns>
    public Task<Result<Nothing>> OnFailure(Func<ResultError, Task> next) => Switch(_ => Task.CompletedTask, next);

    /// <summary>
    ///     Matches the result to the appropriate function based on success or failure and returns a result of type
    ///     <typeparamref name="TOut" />.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Result<TOut> Switch<TOut>(Func<T, Result<TOut>> onSuccess, Func<ResultError, Result<TOut>> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>
    ///     Matches the result to the appropriate function based on success or failure and returns a result of type
    ///     <typeparamref name="TOut" />.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Task<Result<TOut>> Switch<TOut>(Func<T, Task<Result<TOut>>> onSuccess,
        Func<ResultError, Task<Result<TOut>>> onFailure) => IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>
    ///     Matches the result to the appropriate function based on success or failure and returns a result of type
    ///     <See cref="Nothing" />.
    /// </summary>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Result<Nothing> Switch(Action<T> onSuccess, Action<ResultError> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(Value);
            return Nothing.Value;
        }

        onFailure(Error);
        return Error;
    }

    /// <summary>
    ///     Matches the result to the appropriate function based on success or failure and returns a result of type
    ///     <see cref="Nothing" />.
    /// </summary>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public async Task<Result<Nothing>> Switch(Func<T, Task> onSuccess, Func<ResultError, Task> onFailure)
    {
        if (IsSuccess)
        {
            await onSuccess(Value);
            return Nothing.Value;
        }

        await onFailure(Error);
        return Error;
    }
}
