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
    /// Converts the source object to a <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the source object.</typeparam>
    /// <param name="source">The source object to convert.</param>
    /// <returns>A successful result containing the source object.</returns>
    public static Result<T> ToResult<T>(this T source) => Result<T>.Success(source);

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
}
