# Examples — DrifterApps.Seeds.FluentResult

Real-world usage patterns. Full sample files live in [examples/](../examples/).

---

## Domain Model with Errors

Define errors as static members next to the domain type.

```csharp
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
```

---

## Repository Pattern

```csharp
public interface IUserRepository
{
    Task<Result<User>> FindByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<User>> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<Result<Nothing>> SaveAsync(User user, CancellationToken ct = default);
}

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<Result<User>> FindByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await _db.Users.FindAsync([id], ct);
        return user is null
            ? User.Errors.NotFound(id)
            : user;
    }

    public async Task<Result<User>> FindByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);
        return user is null
            ? User.Errors.NotFound(Guid.Empty)
            : user;
    }

    public async Task<Result<Nothing>> SaveAsync(User user, CancellationToken ct)
    {
        _db.Users.Update(user);
        await _db.SaveChangesAsync(ct);
        return Nothing.Value;
    }
}
```

---

## Service Layer — Composing Multi-Step Operations

### Using LINQ query syntax (preferred for 3+ steps)

```csharp
public sealed class UserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo) => _repo = repo;

    public Task<Result<User>> ChangeEmailAsync(
        Guid userId, string newEmail, CancellationToken ct)
    {
        return from user    in _repo.FindByIdAsync(userId, ct)
               from _       in EnsureEmailNotTaken(newEmail, ct)
               from updated in user.ChangeEmail(newEmail).ToTask()
               from __      in _repo.SaveAsync(updated, ct)
               select updated;
    }

    private async Task<Result<Nothing>> EnsureEmailNotTaken(
        string email, CancellationToken ct)
    {
        var existing = await _repo.FindByEmailAsync(email, ct);
        return existing.IsSuccess
            ? User.Errors.EmailTaken(email)
            : Nothing.Value;
    }
}
```

### Using OnSuccess chains (clearer for 1–2 steps)

```csharp
public Task<Result<User>> GetUserAsync(Guid id, CancellationToken ct) =>
    _repo.FindByIdAsync(id, ct)
         .OnFailure(err => _logger.LogWarning("User {Id} not found: {Code}", id, err.Code));
```

---

## Batch Validation on Command Handlers

```csharp
public sealed record RegisterUserCommand(string Email, string Name, int Age);

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _repo;

    public RegisterUserHandler(IUserRepository repo) => _repo = repo;

    public Task<Result<User>> HandleAsync(RegisterUserCommand cmd, CancellationToken ct)
    {
        var aggregate = ResultAggregate.Create()
            .Ensure(() => !string.IsNullOrWhiteSpace(cmd.Email),   RegistrationErrors.EmailRequired)
            .Ensure(() => cmd.Email.Contains('@'),                  RegistrationErrors.EmailInvalid)
            .Ensure(() => !string.IsNullOrWhiteSpace(cmd.Name),    RegistrationErrors.NameRequired)
            .Ensure(() => cmd.Age >= 18,                           RegistrationErrors.UnderAge,
                    EnsureOnFailure.IgnoreOnFailure); // conditional check

        return aggregate.OnSuccess(() => CreateAndSaveAsync(cmd, ct));
    }

    private Task<Result<User>> CreateAndSaveAsync(RegisterUserCommand cmd, CancellationToken ct) =>
        from user in User.Create(cmd.Email, cmd.Name)
        from _    in _repo.SaveAsync(user, ct)
        select user;

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
```

---

## ASP.NET Core Controller — Mapping to HTTP Responses

```csharp
[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service) => _service = service;

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
    {
        var result = await _service.GetUserAsync(id, ct);

        return result.Match(
            onSuccess: user => Ok(user),
            onFailure: err  => MapError(err)
        );
    }

    [HttpPut("{id:guid}/email")]
    public async Task<IActionResult> ChangeEmail(
        Guid id, [FromBody] ChangeEmailRequest request, CancellationToken ct)
    {
        var result = await _service.ChangeEmailAsync(id, request.Email, ct);

        return result.Match(
            onSuccess: user => Ok(user),
            onFailure: err  => MapError(err)
        );
    }

    private IActionResult MapError(ResultError err) => err.Code switch
    {
        "User.NotFound"     => NotFound(new { err.Code, err.Description }),
        "User.EmailTaken"   => Conflict(new { err.Code, err.Description }),
        "User.InvalidEmail" => BadRequest(new { err.Code, err.Description }),
        _                   => Problem(err.Description)
    };
}
```

---

## Minimal API Endpoint Mapping

```csharp
app.MapGet("/users/{id:guid}", async (Guid id, UserService svc, CancellationToken ct) =>
{
    var result = await svc.GetUserAsync(id, ct);
    return result.Match(
        onSuccess: Results.Ok,
        onFailure: err => err.Code switch
        {
            "User.NotFound" => Results.NotFound(),
            _               => Results.Problem(err.Description)
        }
    );
});
```

---

## MediatR Command Handler

```csharp
public sealed record CreateOrderCommand(Guid CustomerId, List<OrderLine> Lines)
    : IRequest<Result<Order>>;

public sealed class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, Result<Order>>
{
    private readonly ICustomerRepository _customers;
    private readonly IOrderRepository _orders;

    public CreateOrderHandler(ICustomerRepository customers, IOrderRepository orders)
    {
        _customers = customers;
        _orders = orders;
    }

    public Task<Result<Order>> Handle(
        CreateOrderCommand request, CancellationToken ct) =>
        from customer in _customers.FindByIdAsync(request.CustomerId, ct)
        from order    in Order.Create(customer, request.Lines)
        from _        in _orders.SaveAsync(order, ct)
        select order;
}
```

---

## Async Pipeline with Logging

```csharp
public async Task<Result<Invoice>> GenerateInvoiceAsync(
    Guid orderId, CancellationToken ct)
{
    return await _orders.FindByIdAsync(orderId, ct)
        .OnFailure(err => _logger.LogWarning(
            "Order {Id} not found for invoicing: {Code}", orderId, err.Code))
        .OnSuccess(order => ValidateOrderForInvoicing(order))
        .OnSuccess(order => _invoiceGenerator.GenerateAsync(order, ct))
        .OnSuccess(invoice => _invoices.SaveAsync(invoice, ct)
            .OnSuccess(_ => invoice));
}
```

---

## Pure Transformations with Select

```csharp
// Map success value without possibility of failure
Result<string> emailDisplay = _repo.FindByIdAsync(id, ct)
    .Select(user => user.Email);

// Chain Select in a pipeline
Result<UserDto> dto = from user in _repo.FindByIdAsync(id, ct)
                      select new UserDto(user.Id, user.Email, user.Name);
```

---

## Testing Examples

```csharp
public class UserServiceTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly UserService _sut;

    public UserServiceTests() => _sut = new UserService(_repo);

    [Fact]
    public async Task ChangeEmail_WhenUserExists_ReturnsUpdatedUser()
    {
        var userId = Guid.NewGuid();
        var user = User.Create("old@example.com", "Alice").Value;
        _repo.FindByIdAsync(userId, default).Returns(user.ToResult());
        _repo.FindByEmailAsync("new@example.com", default)
             .Returns(User.Errors.NotFound(Guid.Empty).ToResult<User>());
        _repo.SaveAsync(Arg.Any<User>(), default).Returns(Nothing.Value);

        var result = await _sut.ChangeEmailAsync(userId, "new@example.com", default);

        result.Should().BeSuccessful().And.WithValue(
            result.Value with { });
        result.Value.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task ChangeEmail_WhenUserNotFound_ReturnsNotFoundError()
    {
        var id = Guid.NewGuid();
        _repo.FindByIdAsync(id, default)
             .Returns(User.Errors.NotFound(id).ToResult<User>());

        var result = await _sut.ChangeEmailAsync(id, "new@example.com", default);

        result.Should().BeFailure().And.WithError(User.Errors.NotFound(id));
    }

    [Fact]
    public async Task ChangeEmail_WhenEmailTaken_ReturnsEmailTakenError()
    {
        var userId = Guid.NewGuid();
        var existingUser = User.Create("taken@example.com", "Bob").Value;
        var currentUser = User.Create("current@example.com", "Alice").Value;

        _repo.FindByIdAsync(userId, default).Returns(currentUser.ToResult());
        _repo.FindByEmailAsync("taken@example.com", default).Returns(existingUser.ToResult());

        var result = await _sut.ChangeEmailAsync(userId, "taken@example.com", default);

        result.Should().BeFailure().And.WithError(User.Errors.EmailTaken("taken@example.com"));
    }
}
```

---

## Error Aggregation in Tests

```csharp
[Fact]
public void Create_WithMultipleInvalidFields_ReturnsAllErrors()
{
    var result = User.Create(email: "", name: "");

    result.Should().BeFailure();

    var aggregate = (ResultErrorAggregate)result.Error;
    aggregate.Errors.Should().ContainKey("User.InvalidEmail");
    aggregate.Errors.Should().ContainKey("User.NameRequired");
}
```

---

## Pattern Matching with Deconstruct

```csharp
Result<User> result = _repo.FindByIdAsync(id, ct);

// Deconstruct the error for pattern matching
if (result.IsFailure)
{
    var (code, description) = result.Error;
    _logger.LogError("[{Code}] {Description}", code, description);
}
```
