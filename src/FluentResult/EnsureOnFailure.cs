namespace DrifterApps.Seeds.FluentResult;

/// <summary>
/// Specifies the behavior of the Ensure validation when a failure occurs.
/// </summary>
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
