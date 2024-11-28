using System.Diagnostics.CodeAnalysis;

namespace DrifterApps.Seeds.FluentResult;

/// <summary>
///     Represents the result of an operation with a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
public readonly partial struct Result<T> : IEquatable<Result<T>>
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
    public T Value =>
        IsFailure ? throw new InvalidOperationException("Cannot access the value of a failed result.") : _value;

    /// <summary>
    ///     Gets the error associated with the operation.
    /// </summary>
    public ResultError Error { get; }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Result<T> other && Equals(other);

    /// <summary>
    /// Determines whether the specified Result{T} is equal to the current Result{T}.
    /// </summary>
    /// <param name="other">The Result{T} to compare with the current Result{T}.</param>
    /// <returns>true if the specified ResultResult{T} is equal to the current Result{T}; otherwise, false.</returns>
    public bool Equals(Result<T> other) =>
        EqualityComparer<T?>.Default.Equals(_value, other._value) &&
        IsSuccess == other.IsSuccess &&
        Error.Equals(other.Error);

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => IsSuccess.GetHashCode() ^ Error.GetHashCode() ^ (_value?.GetHashCode() ?? 0);

    /// <summary>
    /// Determines whether two specified instances of Result{T} are equal.
    /// </summary>
    /// <param name="left">The first Result{T} to compare.</param>
    /// <param name="right">The second Result{T} to compare.</param>
    /// <returns>true if the two specified instances are equal; otherwise, false.</returns>
    public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);

    /// <summary>
    /// Determines whether two specified instances of Result{T} are not equal.
    /// </summary>
    /// <param name="left">The first Result{T} to compare.</param>
    /// <param name="right">The second Result{T} to compare.</param>
    /// <returns>true if the two specified instances are not equal; otherwise, false.</returns>
    public static bool operator !=(Result<T> left, Result<T> right) => !(left == right);
}
