namespace DrifterApps.Seeds.FluentResult;

/// <summary>
///     Provides asynchronous extension methods for converting tasks into results and handling them through matching and
///     transformation.
/// </summary>
public static partial class ResultExtensions
{
    /// <summary>
    ///     Converts a task of type <typeparamref name="T" /> into a successful result unless <typeparamref name="T" /> is
    ///     <see cref="ResultError" />.
    /// </summary>
    /// <typeparam name="T">The type of the source object.</typeparam>
    /// <param name="source">The source object to convert.</param>
    /// <returns>A successful result containing the source object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="T" /> is <see cref="ResultError" />.</exception>
    public static async Task<Result<T>> ToResult<T>(this Task<T> source)
    {
        if (typeof(T) == typeof(ResultError))
        {
            throw new InvalidOperationException("ResultError is not allowed.");
        }

        var result = await source;
        return result;
    }

    /// <summary>
    ///     Converts a task that returns a <see cref="ResultError" /> into a result of type <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">The type of the source object.</typeparam>
    /// <param name="source">The source object to convert.</param>
    /// <returns>A successful result containing the source object.</returns>
    public static async Task<Result<T>> ToResult<T>(this Task<ResultError> source)
    {
        var result = await source;
        return result;
    }

    /// <summary>
    ///     Asynchronously matches a result to execute actions or functions on success or failure, returning a result.
    /// </summary>
    /// <typeparam name="T">The type of the input result value.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<T>> Match<T>(this Task<Result<T>> resultTask,
        Action<T> onSuccess, Action<ResultError> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>
    ///     Asynchronously matches a result to execute actions or functions on success or failure, returning a result.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the result returned by the onFailure or onFailure function.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<TOut>> Match<TIn, TOut>(this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> onSuccess, Func<ResultError, Result<TOut>> onFailure)
    {
        var result = await resultTask;
        return result.Match(onSuccess, onFailure);
    }

    /// <summary>
    ///     Asynchronously matches a result to execute actions or functions on success or failure, returning a result.
    /// </summary>
    /// <typeparam name="T">The type of the input result value.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<T>> Match<T>(this Task<Result<T>> resultTask,
        Func<T, Task> onSuccess, Func<ResultError, Task> onFailure)
    {
        var result = await resultTask;
        return await result.Match(onSuccess, onFailure);
    }

    /// <summary>
    ///     Asynchronously matches a result to execute actions or functions on success or failure, returning a result.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <typeparam name="TOut">The type of the result returned by the onFailure or onFailure function.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="onSuccess">The function to execute if the result is successful.</param>
    /// <param name="onFailure">The function to execute if the result is a failure.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<TOut>> Match<TIn, TOut>(this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> onSuccess, Func<ResultError, Task<Result<TOut>>> onFailure)
    {
        var result = await resultTask;
        return await result.Match(onSuccess, onFailure);
    }

    /// <summary>
    ///     Executes the next function if the result is successful, passing the result value to the next function and returning
    ///     a result of type TOut.
    /// </summary>
    /// <typeparam name="T">The type of the input result value.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="next">The function to execute if the result is successful.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<T>> OnSuccess<T>(this Task<Result<T>> resultTask, Action<T> next)
    {
        var result = await resultTask;
        return result.OnSuccess(next);
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
        Func<TIn, Result<TOut>> next)
    {
        var result = await resultTask;
        return result.OnSuccess(next);
    }

    /// <summary>
    ///     Executes the next function if the result is successful, passing the result value to the next function and returning
    ///     a result of type TOut.
    /// </summary>
    /// <typeparam name="T">The type of the input result value.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="next">The function to execute if the result is successful.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<T>> OnSuccess<T>(this Task<Result<T>> resultTask, Func<T, Task> next)
    {
        var result = await resultTask;
        return await result.OnSuccess(next);
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
        return await result.OnSuccess(next);
    }

    /// <summary>
    ///     Executes the next function if the result is a failure, passing the error to the next function.
    /// </summary>
    /// <typeparam name="TIn">The type of the input result value.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="next">The function to execute if the result is a failure.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<TIn>> OnFailure<TIn>(this Task<Result<TIn>> resultTask, Action<ResultError> next)
    {
        var result = await resultTask;
        return result.OnFailure(next);
    }

    /// <summary>
    ///     Executes the next function if the result is a failure, passing the error to the next function.
    /// </summary>
    /// <typeparam name="T">The type of the input result value.</typeparam>
    /// <param name="resultTask">The task that returns a result of type TIn.</param>
    /// <param name="next">The function to execute if the result is a failure.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task<Result<T>> OnFailure<T>(this Task<Result<T>> resultTask, Func<ResultError, Task> next)
    {
        var result = await resultTask;
        return await result.OnFailure(next);
    }

    /// <summary>
    ///     Selects a value asynchronously from a successful result or propagates errors.
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
        return await result.Match(async r => await selector(r),
            e => Task.FromResult((Result<TResult>) e));
    }

    /// <summary>
    ///     Performs a select-many operation asynchronously, combining intermediate results into a final result.
    /// </summary>
    /// <typeparam name="TSource">The type of the input value.</typeparam>
    /// <typeparam name="TMiddle">The type of the intermediate result.</typeparam>
    /// <typeparam name="TResult">The type of the final result.</typeparam>
    /// <param name="source">The target for the extension.</param>
    /// <param name="collectionSelector">How to map to the <see cref="Result{TMiddle}" /> type.</param>
    /// <param name="resultSelector">How to map a <typeparamref name="TMiddle" /> to a <typeparamref name="TResult" />.</param>
    /// <returns>A result of the result selector function or a failure result.</returns>
    public static async Task<Result<TResult>> SelectMany<TSource, TMiddle, TResult>(this Task<Result<TSource>> source,
        Func<TSource, Task<Result<TMiddle>>> collectionSelector, Func<TSource, TMiddle, TResult> resultSelector)
    {
        var result = await source;
        return await result.Match(
            async r =>
            {
                var collectionResult = await collectionSelector(r);

                // If result is a success, we run the "result selector" to
                // get the final TResult. If it is not a success, then
                // Select() just passes the error through as a failed Result<TResult>
                return collectionResult.Select(v => resultSelector(r, v));
            },
            e => Task.FromResult((Result<TResult>) e));
    }
}
