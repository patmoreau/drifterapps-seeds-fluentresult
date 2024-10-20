using System.Diagnostics.CodeAnalysis;

namespace DrifterApps.Seeds.FluentResult;

/// <summary>
///     Provides extension methods for handling results.
/// </summary>
[SuppressMessage("Minor Code Smell", "S4136:Method overloads should be grouped together")]
[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
public static partial class ResultExtensions
{
    /// <summary>
    ///     Executes the next function if the result is successful and returns a result of type <typeparamref name="TOut" />.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="result">The initial result.</param>
    /// <param name="next">The function to execute if the result is successful.</param>
    /// <returns>The result of the next function or a failure result.</returns>
    public static Result<TOut> OnSuccess<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> next) =>
        result.IsSuccess ? next(result.Value) : Result<TOut>.Failure(result.Error);

    /// <summary>
    ///     Executes the next function if the result is a failure.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <param name="result">The initial result.</param>
    /// <param name="next">The function to execute if the result is a failure.</param>
    /// <returns>The result of the next function or the initial result.</returns>
    public static Result<TIn> OnFailure<TIn>(this Result<TIn> result, Func<ResultError, Result<TIn>> next) =>
        result.IsFailure ? next(result.Error) : result;

    /// <summary>
    ///     Matches the result to the appropriate function based on success or failure and returns a result of type
    ///     <typeparamref name="TOut" />.
    /// </summary>
    /// <typeparam name="TIn">The type of the input value.</typeparam>
    /// <typeparam name="TOut">The type of the result.</typeparam>
    /// <param name="result">The initial result.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>The result of the appropriate function.</returns>
    public static Result<TOut> Switch<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> onSuccess,
        Func<ResultError, Result<TOut>> onFailure) =>
        result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);

    /// <summary>
    /// Projects each element of a sequence into a new form.
    /// </summary>
    /// <typeparam name="TFrom">The type of the input value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The target for the extension.</param>
    /// <param name="selector">The mapping/selector method.</param>
    /// <returns>A result of the selector function or a failure result.</returns>
    public static Result<TResult> Select<TFrom, TResult>(this Result<TFrom> source, Func<TFrom, TResult> selector) =>
        source.Switch(onSuccess: r => selector(r), onFailure: Result<TResult>.Failure);

    /// <summary>
    /// Projects each element of a sequence to a <see cref="Result{TMiddle}"/> and flattens the resulting sequences into one sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the input value.</typeparam>
    /// <typeparam name="TMiddle">The type of the intermediate result.</typeparam>
    /// <typeparam name="TResult">The type of the final result.</typeparam>
    /// <param name="source">The target for the extension.</param>
    /// <param name="collectionSelector">How to map to the <see cref="Result{TMiddle}"/> type.</param>
    /// <param name="resultSelector">How to map a <typeparamref name="TMiddle"/> to a <typeparamref name="TResult"/>.</param>
    /// <returns>A result of the result selector function or a failure result.</returns>
    public static Result<TResult> SelectMany<TSource, TMiddle, TResult>(this Result<TSource> source,
        Func<TSource, Result<TMiddle>> collectionSelector, Func<TSource, TMiddle, TResult> resultSelector) =>
        source.Switch(
            onSuccess: r =>
            {
                var result = collectionSelector(r);

                // If result is a success, we run the "result selector" to
                // get the final TResult. If it is not a success, then
                // Select() just passes the error through as a failed Result<TResult>
                return result.Select(v => resultSelector(r, v));
            },
            onFailure: Result<TResult>.Failure); // error -> return a failed Result<TResult>

    /// <summary>
    /// Ensures that the specified validation function returns true. If the validation fails,
    /// it adds a failure result to the source.
    /// </summary>
    /// <param name="source">The initial result aggregate.</param>
    /// <param name="validation">The validation function to execute returning a <see cref="System.Boolean"/></param>
    /// <param name="error">The error to add if the validation fails.</param>
    /// <param name="options">Indicates whether to run validation on failure.</param>
    /// <returns>The updated result aggregate.</returns>
    public static ResultAggregate Ensure(this ResultAggregate source, Func<bool> validation,
        ResultError error, EnsureOnFailure options = EnsureOnFailure.ValidateOnFailure)
    {
        return source.Ensure(Func, options);

        Result<Nothing> Func() => !validation() ? Result<Nothing>.Failure(error) : Result<Nothing>.Success();
    }

    /// <summary>
    /// Ensures that the specified validation function returns true. If the validation fails,
    /// it adds a failure result to the source.
    /// </summary>
    /// <param name="source">The initial result aggregate.</param>
    /// <param name="validation">The validation function to execute returning a <see cref="Result{Nothing}"/>.</param>
    /// <param name="options">Indicates whether to run validation on failure.</param>
    /// <returns>The updated result aggregate.</returns>
    public static ResultAggregate Ensure(this ResultAggregate source, Func<Result<Nothing>> validation,
        EnsureOnFailure options = EnsureOnFailure.ValidateOnFailure)
    {
        if (source.IsFailure && options == EnsureOnFailure.IgnoreOnFailure)
        {
            return source;
        }

        source.AddResult(validation());

        return source;
    }
}
