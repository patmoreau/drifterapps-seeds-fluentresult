using System.Diagnostics.CodeAnalysis;

namespace DrifterApps.Seeds.FluentResult;

[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
public partial record ResultAggregate
{
    /// <summary>
    /// Ensures that the specified validation function returns true. If the validation fails,
    /// it adds a failure result to the source.
    /// </summary>
    /// <param name="validation">The validation function to execute returning a <see cref="System.Boolean"/></param>
    /// <param name="error">The error to add if the validation fails.</param>
    /// <param name="options">Indicates whether to run validation on failure.</param>
    /// <returns>The updated result aggregate.</returns>
    public ResultAggregate Ensure(Func<bool> validation, ResultError error,
        EnsureOnFailure options = EnsureOnFailure.ValidateOnFailure)
    {
        return Ensure(Func, options);

        Result<Nothing> Func() => !validation() ? Result<Nothing>.Failure(error) : Result<Nothing>.Success();
    }

    /// <summary>
    /// Ensures that the specified validation function returns true. If the validation fails,
    /// it adds a failure result to the source.
    /// </summary>
    /// <param name="validation">The validation function to execute returning a <see cref="Result{Nothing}"/>.</param>
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
    ///     Matches the result to the appropriate function based on success or failure and returns a result of type
    ///     <typeparamref name="TOut" />.
    /// </summary>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public Result<TOut> Switch<TOut>(Func<Result<TOut>> onSuccess,
        Func<ResultErrorAggregate, Result<TOut>> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(ToErrorAggregate<TOut>());

    private ResultErrorAggregate ToErrorAggregate<TOut>() =>
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
