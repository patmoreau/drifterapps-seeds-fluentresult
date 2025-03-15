using System.Diagnostics.CodeAnalysis;

namespace DrifterApps.Seeds.FluentResult;

[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
public partial record ResultAggregate
{
    /// <summary>
    ///     Validates a condition and adds a failure if the validation fails.
    /// </summary>
    /// <param name="validation">The validation function to execute returning a <see cref="System.Boolean" /></param>
    /// <param name="error">The error to add if the validation fails.</param>
    /// <param name="options">Indicates whether to run validation on failure.</param>
    /// <returns>The updated result aggregate.</returns>
    public ResultAggregate Ensure(Func<bool> validation, ResultError error,
        EnsureOnFailure options = EnsureOnFailure.ValidateOnFailure)
    {
        return Ensure(Func, options);

        Result<Nothing> Func()
        {
            return !validation() ? error : Nothing.Value;
        }
    }

    /// <summary>
    ///     Invokes the provided validation result and adds it to the aggregate.
    /// </summary>
    /// <param name="validation">The validation function to execute returning a <see cref="Result{Nothing}" />.</param>
    /// <param name="options">Indicates whether to run validation on failure.</param>
    /// <returns>The updated result aggregate.</returns>
    public ResultAggregate Ensure(Func<Result<Nothing>> validation,
        EnsureOnFailure options = EnsureOnFailure.ValidateOnFailure)
    {
        ArgumentNullException.ThrowIfNull(validation);

        if (IsFailure && options == EnsureOnFailure.IgnoreOnFailure)
        {
            return this;
        }

        AddResult(validation());

        return this;
    }

    /// <summary>
    ///     Executes an action for success or failure and returns a <see cref="Result{Nothing}" />.
    /// </summary>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    public Result<Nothing> Match(Action onSuccess, Action<ResultErrorAggregate> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess();
            return Nothing.Value;
        }

        var error = ToErrorAggregate<Nothing>();
        onFailure(error);
        return error.ToResult<Nothing>();
    }

    /// <summary>
    ///     Executes a function for success or failure and returns a typed result.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Result<TOut> Match<TOut>(Func<Result<TOut>> onSuccess,
        Func<ResultErrorAggregate, Result<TOut>> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(ToErrorAggregate<TOut>());

    /// <summary>
    ///     Asynchronously executes an action for success or failure and returns a <see cref="Result{Nothing}" />.
    /// </summary>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>
    ///     <see cref="Task" />
    /// </returns>
    public async Task<Result<Nothing>> Match(Func<Task> onSuccess, Func<ResultErrorAggregate, Task> onFailure)
    {
        if (IsSuccess)
        {
            await onSuccess();
            return Nothing.Value;
        }

        var error = ToErrorAggregate<Nothing>();
        await onFailure(error);
        return error;
    }

    /// <summary>
    ///     Asynchronously executes a function for success or failure and returns a typed result.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Task<Result<TOut>> Match<TOut>(Func<Task<Result<TOut>>> onSuccess,
        Func<ResultErrorAggregate, Task<Result<TOut>>> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(ToErrorAggregate<TOut>());

    /// <summary>
    ///     Runs an action if the result is successful.
    /// </summary>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Result<Nothing> OnSuccess(Action onSuccess) => Match(onSuccess, _ => { });

    /// <summary>
    ///     Runs a function if the result is successful and returns a typed result.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Result<TOut> OnSuccess<TOut>(Func<Result<TOut>> onSuccess) => Match(onSuccess, error => error);

    /// <summary>
    ///     Asynchronously runs an action if the result is successful.
    /// </summary>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Task<Result<Nothing>> OnSuccess(Func<Task> onSuccess) =>
        Match(onSuccess, error => Task.FromResult(error.ToResult<Nothing>()));

    /// <summary>
    ///     Asynchronously runs a function if the result is successful and returns a typed result.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Task<Result<TOut>> OnSuccess<TOut>(Func<Task<Result<TOut>>> onSuccess) =>
        Match(onSuccess, error => Task.FromResult((Result<TOut>) error));

    /// <summary>
    ///     Runs an action if the result is a failure.
    /// </summary>
    /// <param name="onFailure">The function to execute if the result is failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Result<Nothing> OnFailure(Action<ResultErrorAggregate> onFailure) => Match(() => { }, onFailure);

    /// <summary>
    ///     Asynchronously runs an action if the result is a failure.
    /// </summary>
    /// <param name="onFailure">The function to execute if the result is failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Task<Result<Nothing>> OnFailure(Func<ResultErrorAggregate, Task> onFailure) =>
        Match(() => Task.CompletedTask, onFailure);

    /// <summary>
    ///     Creates an aggregated error result for a given type.
    /// </summary>
    internal ResultErrorAggregate ToErrorAggregate<TOut>() =>
        new($"{typeof(TOut).Name}.Errors",
            "Errors occurred",
            Results
                .Where(r => r.IsFailure)
                .Select(r => r.Error)
                .GroupBy(error => error.Code)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Select(x => x.Description).ToArray()
                ));
}
