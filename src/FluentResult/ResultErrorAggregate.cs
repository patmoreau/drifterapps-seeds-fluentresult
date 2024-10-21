namespace DrifterApps.Seeds.FluentResult;

/// <summary>
///     Represents a list of errors.
/// </summary>
public record ResultErrorAggregate : ResultError
{
    /// <summary>
    /// Gets the dictionary of errors by error codes.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    ///     Represents an list of errors.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="errors">Dictionary of error by codes.</param>
    public ResultErrorAggregate(string code, string description,
        IReadOnlyDictionary<string, string[]> errors)
        : base(code, description) => Errors = errors;

    /// <summary>
    /// Determines whether the specified <see cref="ResultErrorAggregate"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ResultErrorAggregate"/> to compare with the current instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="ResultErrorAggregate"/> is equal to the current instance; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool Equals(ResultErrorAggregate? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && AreErrorAggregateEqual(other.Errors);
    }

    /// <summary>
    /// Computes the hash code for the current instance.
    /// </summary>
    /// <returns>
    /// A hash code for the current instance.
    /// </returns>
    public override int GetHashCode() =>
        Errors.Aggregate(base.GetHashCode(),
            (current, error) => error.Value.Aggregate(HashCode.Combine(current, error.Key), HashCode.Combine));

    private bool AreErrorAggregateEqual(IReadOnlyDictionary<string, string[]> other)
    {
        // Check if the dictionaries have the same number of keys
        if (Errors.Count != other.Count)
        {
            return false;
        }

        // Check if all keys and their corresponding values are equal
        foreach (var kvp in Errors)
        {
            if (!other.TryGetValue(kvp.Key, out var dict2Value))
            {
                return false;
            }

            // Compare the values (string arrays) using SequenceEqual
            if (!kvp.Value.SequenceEqual(dict2Value))
            {
                return false;
            }
        }

        return true;
    }
}
