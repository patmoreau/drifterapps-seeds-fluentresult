using DrifterApps.Seeds.FluentResult;

namespace FluentResult.Examples;

// Illustrates OnSuccess chaining, LINQ query syntax, and failure propagation.
public sealed class UserService
{
    private readonly IUserRepository _repo;
    private readonly ILogger _logger;

    public UserService(IUserRepository repo, ILogger logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // Single-step: OnSuccess chain with side-effect logging.
    public Task<Result<User>> GetUserAsync(Guid id, CancellationToken ct) =>
        _repo.FindByIdAsync(id, ct)
             .OnFailure(err => _logger.LogWarning(
                 "User {Id} not found: [{Code}] {Description}", id, err.Code, err.Description));

    // Multi-step: LINQ query syntax for clear sequential dependency.
    public Task<Result<User>> ChangeEmailAsync(Guid userId, string newEmail, CancellationToken ct) =>
        from user  in _repo.FindByIdAsync(userId, ct)
        from _     in EnsureEmailAvailableAsync(newEmail, ct)
        from __    in user.ChangeEmail(newEmail).ToTask()
        from saved in _repo.SaveAsync(user, ct)
        select user;

    // Batch validation before performing work.
    public Task<Result<User>> RegisterAsync(
        string email, string name, int age, CancellationToken ct)
    {
        var aggregate = ResultAggregate.Create()
            .Ensure(() => !string.IsNullOrWhiteSpace(email), RegistrationErrors.EmailRequired)
            .Ensure(() => email.Contains('@'),                RegistrationErrors.EmailInvalid)
            .Ensure(() => !string.IsNullOrWhiteSpace(name),  RegistrationErrors.NameRequired)
            .Ensure(() => age >= 18,                         RegistrationErrors.UnderAge,
                    EnsureOnFailure.IgnoreOnFailure); // optional check — skipped if prior errors

        return aggregate.OnSuccess(() =>
            from user in User.Create(email, name)
            from _    in _repo.SaveAsync(user, ct)
            select user);
    }

    private async Task<Result<Nothing>> EnsureEmailAvailableAsync(string email, CancellationToken ct)
    {
        var existing = await _repo.FindByEmailAsync(email, ct);
        return existing.IsSuccess
            ? User.Errors.EmailTaken(email)
            : Nothing.Value;
    }

    private static class RegistrationErrors
    {
        internal static readonly ResultError EmailRequired =
            new("Registration.EmailRequired", "Email is required.");

        internal static readonly ResultError EmailInvalid =
            new("Registration.EmailInvalid", "Email format is invalid.");

        internal static readonly ResultError NameRequired =
            new("Registration.NameRequired", "Name is required.");

        internal static readonly ResultError UnderAge =
            new("Registration.UnderAge", "Must be at least 18 years old.");
    }
}

// Minimal interfaces used by the examples — replace with your actual persistence layer.
public interface IUserRepository
{
    Task<Result<User>> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<User>> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<Result<Nothing>> SaveAsync(User user, CancellationToken ct = default);
}

public interface ILogger
{
    void LogWarning(string message, params object[] args);
}
