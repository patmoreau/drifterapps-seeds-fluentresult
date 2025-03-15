namespace DrifterApps.Seeds.FluentResult;

public partial struct Result<T>
{
    /// <summary>
    ///     Executes onSuccess if this is a successful result, or onFailure if it is failed.
    /// </summary>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <param name="onFailure">The action to execute if the result is a failure.</param>
    /// <returns>The current result.</returns>
    public Result<T> Match(Action<T> onSuccess, Action<ResultError> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(Value);
        }
        else
        {
            onFailure(Error);
        }

        return this;
    }

    /// <summary>
    ///     Executes onSuccess if this is a successful result, or onFailure if it is failed, returning a new typed result.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Result<TOut> Match<TOut>(Func<T, Result<TOut>> onSuccess, Func<ResultError, Result<TOut>> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>
    ///     Asynchronously executes the corresponding function based on success or failure.
    /// </summary>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <param name="onFailure">The action to execute if the result is a failure.</param>
    /// <returns>This current result.</returns>
    public async Task<Result<T>> Match(Func<T, Task> onSuccess, Func<ResultError, Task> onFailure)
    {
        if (IsSuccess)
        {
            await onSuccess(Value);
        }
        else
        {
            await onFailure(Error);
        }

        return this;
    }

    /// <summary>
    ///     Asynchronously executes the corresponding function based on success or failure, returning a new typed result.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Task<Result<TOut>> Match<TOut>(Func<T, Task<Result<TOut>>> onSuccess,
        Func<ResultError, Task<Result<TOut>>> onFailure) => IsSuccess ? onSuccess(Value) : onFailure(Error);

    /// <summary>
    ///     Calls onSuccess if the result is success, returning the original result.
    /// </summary>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    public Result<T> OnSuccess(Action<T> onSuccess) => Match(onSuccess, _ => { });

    /// <summary>
    ///     Calls onSuccess if the result is success, returning a new typed result or a failure.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <returns>The result of the onSuccess function or a failure result.</returns>
    public Result<TOut> OnSuccess<TOut>(Func<T, Result<TOut>> onSuccess) => Match(onSuccess, error => error);

    /// <summary>
    ///     Asynchronously calls onSuccess if the result is success, returning the original result.
    /// </summary>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    public Task<Result<T>> OnSuccess(Func<T, Task> onSuccess) => Match(onSuccess, _ => Task.CompletedTask);

    /// <summary>
    ///     Asynchronously calls onSuccess if the result is success, returning a new typed result or a failure.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <returns>The result of the onSuccess function or a failure result.</returns>
    public Task<Result<TOut>> OnSuccess<TOut>(Func<T, Task<Result<TOut>>> onSuccess) =>
        Match(onSuccess, error => Task.FromResult((Result<TOut>) error));

    /// <summary>
    ///     Calls onFailure if the result is failure, returning the original result.
    /// </summary>
    /// <param name="onFailure">The function to execute if the result is successful.</param>
    public Result<T> OnFailure(Action<ResultError> onFailure) => Match(_ => { }, onFailure);

    /// <summary>
    ///     Asynchronously calls onFailure if the result is failure, returning the original result.
    /// </summary>
    /// <param name="onFailure">The function to execute if the result is successful.</param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    public Task<Result<T>> OnFailure(Func<ResultError, Task> onFailure) => Match(_ => Task.CompletedTask, onFailure);
}
