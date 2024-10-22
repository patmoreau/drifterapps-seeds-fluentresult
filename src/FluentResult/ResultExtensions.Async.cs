namespace DrifterApps.Seeds.FluentResult;

/// <summary>
///     Provides extension methods for handling results asynchronously.
/// </summary>
public static partial class ResultExtensions
{
    /// <summary>
    /// Converts the source object to a <see cref="Result{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the source object.</typeparam>
    /// <param name="source">The source object to convert.</param>
    /// <returns>A successful result containing the source object.</returns>
    public static async Task<Result<T>> ToResult<T>(this Task<T> source)
    {
        var result = await source;
        return Result<T>.Success(result);
    }

    /// <summary>
    ///     Executes the next function if the result is successful, passing the result value to the next function and returning
    ///     a result of type TOut.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the result returned by the next function.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="next">The function to execute if the result is successful.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<TOut>> OnSuccess<TIn, TOut>(this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> next)
    {
        var result = await resultTask;
        return result.IsSuccess ? await next(result.Value) : Result<TOut>.Failure(result.Error);
    }

    /// <summary>
    ///     Executes the next function if the result is a failure, passing the error to the next function.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="next">The function to execute if the result is a failure.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<TIn>> OnFailure<TIn>(this Task<Result<TIn>> resultTask,
        Func<ResultError, Task<Result<TIn>>> next)
    {
        var result = await resultTask;
        return result.IsFailure ? await next(result.Error) : result;
    }

    ///     Matches the result to either the onSuccess or onFailure function, passing the result value to the onSuccess
    ///     function and returning a result of type TOut.
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the result returned by the onSuccess or onFailure function.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<TOut>> Switch<TIn, TOut>(this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> onSuccess,
        Func<ResultError, Task<Result<TOut>>> onFailure)
    {
        var result = await resultTask;
        return result.IsSuccess ? await onSuccess(result.Value) : await onFailure(result.Error);
    }

    /// <summary>
    /// Projects each element of a sequence into a new form.
    /// </summary>
    /// <typeparam name="TFrom">The type of the input value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The target for the extension.</param>
    /// <param name="selector">The mapping/selector method.</param>
    /// <returns>A result of the selector function or a failure result.</returns>
    public static async Task<Result<TResult>> Select<TFrom, TResult>(this Task<Result<TFrom>> source,
        Func<TFrom, Task<TResult>> selector)
    {
        var result = await source;
        return await result.Switch(onSuccess: async r => await selector(r),
            onFailure: e => Task.FromResult(Result<TResult>.Failure(e)));
    }

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
    public static async Task<Result<TResult>> SelectMany<TSource, TMiddle, TResult>(this Task<Result<TSource>> source,
        Func<TSource, Task<Result<TMiddle>>> collectionSelector, Func<TSource, TMiddle, TResult> resultSelector)
    {
        var result = await source;
        return await result.Switch(
            onSuccess: async r =>
            {
                var collectionResult = await collectionSelector(r);

                // If result is a success, we run the "result selector" to
                // get the final TResult. If it is not a success, then
                // Select() just passes the error through as a failed Result<TResult>
                return collectionResult.Select(v => resultSelector(r, v));
            },
            onFailure: e => Task.FromResult(Result<TResult>.Failure(e)));
        // error -> return a failed Result<TResult>
    }
}
