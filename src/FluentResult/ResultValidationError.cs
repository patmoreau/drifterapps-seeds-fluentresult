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
}
