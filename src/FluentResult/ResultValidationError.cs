namespace DrifterApps.Seeds.FluentResult;

/// <summary>
///     Represents a list of validation errors.
/// </summary>
public record ResultValidationError : ResultError
{
    /// <summary>
    /// Gets the dictionary of validation errors by property names.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    /// <summary>
    ///     Represents an list of validation errors.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <param name="validationErrors">Dictionary of validations error by property names.</param>
    public ResultValidationError(string code, string description,
        IReadOnlyDictionary<string, string[]> validationErrors)
        : base(code, description) => ValidationErrors = validationErrors;

    /// <summary>
    /// Determines whether the specified <see cref="ResultValidationError"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ResultValidationError"/> to compare with the current instance.</param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="ResultValidationError"/> is equal to the current instance; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool Equals(ResultValidationError? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return base.Equals(other) && AreValidationErrorsEqual(other.ValidationErrors);
    }

    /// <summary>
    /// Computes the hash code for the current instance.
    /// </summary>
    /// <returns>
    /// A hash code for the current instance.
    /// </returns>
    public override int GetHashCode() =>
        ValidationErrors.Aggregate(base.GetHashCode(),
            (current, error) => error.Value.Aggregate(HashCode.Combine(current, error.Key), HashCode.Combine));

    private bool AreValidationErrorsEqual(IReadOnlyDictionary<string, string[]> other)
    {
        // Check if the dictionaries have the same number of keys
        if (ValidationErrors.Count != other.Count)
        {
            return false;
        }

        // Check if all keys and their corresponding values are equal
        foreach (var kvp in ValidationErrors)
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
