using System.Diagnostics.CodeAnalysis;

namespace DrifterApps.Seeds.FluentResult;

/// <summary>
/// Offers a set of extension methods for converting objects into results, handling result errors, and performing transformations.
/// </summary>
[SuppressMessage("Minor Code Smell", "S4136:Method overloads should be grouped together")]
[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task")]
public static partial class ResultExtensions
{
    /// <summary>
    /// Converts an instance of <typeparamref name="T"/> to a successful result unless it is <see cref="ResultError"/> or null for a non-nullable type.
    /// </summary>
    /// <typeparam name="T">The type of the source object.</typeparam>
    /// <param name="source">The source object to convert.</param>
    /// <returns>A successful result containing the source object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the source object is null and <typeparamref name="T"/> is not nullable.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T"/> is <see cref="ResultError"/>.</exception>
    public static Result<T> ToResult<T>(this T source)
    {
        if (typeof(T) == typeof(ResultError))
        {
            throw new InvalidOperationException("ResultError is not allowed.");
        }

        var type = typeof(T);
        var isNullable = Nullable.GetUnderlyingType(type) != null;
        if (!isNullable && source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return source;
    }

    /// <summary>
    /// Creates a result of type <typeparamref name="T"/> from a <see cref="ResultError"/> unless the error is <see cref="ResultError.None"/>.
    /// </summary>
    /// <typeparam name="T">The type of the source object.</typeparam>
    /// <param name="error">The <see cref="ResultError"/> to convert.</param>
    /// <returns>A successful result containing the source object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the source object is null.</exception>
    public static Result<T> ToResult<T>(this ResultError error) => error == ResultError.None
        ? throw new ArgumentException("Invalid error", nameof(error))
        : error;

    /// <summary>
    /// Applies a mapping function to a successful result or propagates errors.
    /// </summary>
    /// <typeparam name="TFrom">The type of the input value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The target for the extension.</param>
    /// <param name="selector">The mapping/selector method.</param>
    /// <returns>A result of the selector function or a failure result.</returns>
    public static Result<TResult> Select<TFrom, TResult>(this Result<TFrom> source, Func<TFrom, TResult> selector) =>
        source.Match(onSuccess: r => selector(r), onFailure: r => (Result<TResult>)r);

    /// <summary>
    /// Performs a select-many operation, extracting intermediate results and combining them into a final result.
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
        source.Match(
            onSuccess: r =>
            {
                var result = collectionSelector(r);

                // If result is a success, we run the "result selector" to
                // get the final TResult. If it is not a success, then
                // Select() just passes the error through as a failed Result<TResult>
                return result.Select(v => resultSelector(r, v));
            },
            onFailure: r => (Result<TResult>)r); // error -> return a failed Result<TResult>
}
