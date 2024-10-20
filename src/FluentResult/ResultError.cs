namespace DrifterApps.Seeds.FluentResult;

/// <summary>
///     Represents an error with a code and description.
/// </summary>
/// <example>
///     How to implement your domain errors:
///     <code>
///     public static class DomainErrors
///     {
///         public static ResultError InvalidEmail = new("Domain.InvalidEmail", "The email is invalid.");
///         public static ResultError NotFound(Guid id) = new("Domain.NotFound", $"Domain Id '{id}' was not found.");
///     }
///     </code>
/// </example>
public record ResultError
{
    /// <summary>
    ///     Represents an error with a code and description.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <example>
    ///     How to implement your errors in a domain:
    ///     <code>
    ///     public static class DomainErrors
    ///     {
    ///         public static ResultError InvalidEmail = new("Domain.InvalidEmail", "The email is invalid.");
    ///         public static ResultError NotFound(Guid id) = new("Domain.NotFound", $"Domain Id '{id}' was not found.");
    ///     }
    ///     </code>
    /// </example>
    public ResultError(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("The error code cannot be empty when the description is not empty.",
                nameof(code));
        }

        if (!string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("The error description cannot be empty when the code is not empty.",
                nameof(description));
        }

        Code = code;
        Description = description;
    }

    public static readonly ResultError None = new(string.Empty, string.Empty);

    /// <summary>The error code.</summary>
    public string Code { get; }

    /// <summary>The error description.</summary>
    public string Description { get; }

    /// <summary>
    /// Deconstructs the <see cref="ResultError"/> into its code and description.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    public void Deconstruct(out string code, out string description)
    {
        code = Code;
        description = Description;
    }
}
