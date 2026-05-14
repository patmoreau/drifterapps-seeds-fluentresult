using DrifterApps.Seeds.FluentResult;

namespace FluentResult.Examples;

// Domain model with embedded error definitions and factory validation.
public sealed class User
{
    public Guid Id { get; }
    public string Email { get; private set; }
    public string Name { get; private set; }

    private User(Guid id, string email, string name)
    {
        Id = id;
        Email = email;
        Name = name;
    }

    public static Result<User> Create(string email, string name)
    {
        var aggregate = ResultAggregate.Create()
            .Ensure(() => !string.IsNullOrWhiteSpace(email), Errors.InvalidEmail)
            .Ensure(() => email.Contains('@'),                Errors.InvalidEmail)
            .Ensure(() => !string.IsNullOrWhiteSpace(name),  Errors.NameRequired);

        return aggregate.OnSuccess(() =>
            new User(Guid.NewGuid(), email.ToLowerInvariant(), name.Trim()).ToResult());
    }

    public Result<Nothing> ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains('@'))
            return Errors.InvalidEmail;

        Email = newEmail.ToLowerInvariant();
        return Nothing.Value;
    }

    public Result<Nothing> Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Errors.NameRequired;

        Name = newName.Trim();
        return Nothing.Value;
    }

    internal static class Errors
    {
        internal static readonly ResultError InvalidEmail =
            new("User.InvalidEmail", "The email address is not valid.");

        internal static readonly ResultError NameRequired =
            new("User.NameRequired", "Name is required.");

        internal static ResultError NotFound(Guid id) =>
            new("User.NotFound", $"User '{id}' was not found.");

        internal static ResultError EmailTaken(string email) =>
            new("User.EmailTaken", $"Email '{email}' is already in use.");
    }
}
