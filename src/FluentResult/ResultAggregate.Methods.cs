namespace DrifterApps.Seeds.FluentResult;

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
    /// Converts the <see cref="ResultAggregate"/> to a <see cref="Result{TSource}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the result source.</typeparam>
    /// <returns>A <see cref="ResultErrorAggregate"/> containing aggregated errors.</returns>
    public Result<TSource> ToFailure<TSource>() =>
        Result<TSource>.Failure(
            new ResultErrorAggregate(
                $"{typeof(TSource).Name}.ValidationErrors",
                "Validation errors occurred",
                Results
                    .Where(r => r.IsFailure)
                    .Select(r => r.Error)
                    .GroupBy(error => error.Code)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Select(x => x.Description).ToArray()
                    )));
}
