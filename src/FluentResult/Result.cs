using System.Diagnostics.CodeAnalysis;

namespace DrifterApps.Seeds.FluentResult;

/// <summary>
///     Represents the result of an operation with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
public sealed partial record Result<T>
{
    private readonly T? _value;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Result{T}" /> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error associated with the operation.</param>
    /// <param name="value">The value associated with the operation.</param>
    private Result(bool isSuccess, ResultError error, T? value)
    {
        IsSuccess = isSuccess;
        Error = error;
        _value = value;
    }

    /// <summary>
    ///     Gets a value indicating whether the operation was successful.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; }

    /// <summary>
    ///     Gets a value indicating whether the operation was a failure.
    /// </summary>
    [MemberNotNullWhen(false, nameof(_value))]
    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsFailure => !IsSuccess;

    /// <summary>
    ///     Gets the value associated with the operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown the result is failure.</exception>
    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("Cannot access the value of a failed result.");
            }

            return _value;
        }
    }

    /// <summary>
    ///     Gets the error associated with the operation.
    /// </summary>
    public ResultError Error { get; }
}
