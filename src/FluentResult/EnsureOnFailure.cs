namespace DrifterApps.Seeds.FluentResult;

/// <summary>
/// Specifies the behavior of the Ensure validation when a failure occurs.
/// </summary>
/// <remarks>
/// This enumeration defines how to handle failures when applying Ensure validation.
/// </remarks>
public enum EnsureOnFailure
{
    /// <summary>
    /// Ignore the validation on failure.
    /// </summary>
    IgnoreOnFailure,

    /// <summary>
    /// Perform the validation on failure.
    /// </summary>
    ValidateOnFailure
}
