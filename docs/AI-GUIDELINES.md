# AI Guidelines — DrifterApps.Seeds.FluentResult

Guidelines for AI coding assistants (GitHub Copilot, Claude, Cursor, Windsurf, JetBrains AI) when working in a codebase that uses `DrifterApps.Seeds.FluentResult`.

---

## Core Principle

Every method that can fail **must** return `Result<T>`. Never throw exceptions for expected domain failures. Never return `null`.

```csharp
// Wrong
User FindUser(Guid id) => throw new NotFoundException("user not found");

// Wrong
User? FindUser(Guid id) => null;

// Correct
Result<User> FindUser(Guid id) => UserErrors.NotFound(id);
```

---

## Recognizing When to Apply This Pattern

Apply `Result<T>` whenever:
- The operation can fail for business/domain reasons (not programmer errors)
- The caller needs to handle both success and failure
- You would otherwise throw a checked or custom exception
- You would otherwise return `null` to indicate absence

Do **not** wrap `ArgumentNullException`, `ArgumentOutOfRangeException`, or other programmer-error exceptions in a `Result<T>`. Those represent bugs and should remain as exceptions.

---

## Generating Result-Returning Code

### Method signatures

```csharp
// Synchronous — returns value
Result<User> GetUser(Guid id) { ... }

// Synchronous — no meaningful return value
Result<Nothing> DeleteUser(Guid id) { ... }

// Asynchronous
Task<Result<User>> GetUserAsync(Guid id) { ... }
Task<Result<Nothing>> DeleteUserAsync(Guid id) { ... }
```

### Creating results

```csharp
// Success — implicit conversion preferred
Result<User> success = user;
Result<Nothing> unit = Nothing.Value;

// Failure — implicit conversion from ResultError
Result<User> failure = UserErrors.NotFound(id);

// From Task<T>
Task<Result<User>> fromTask = dbQuery.ToResult();
```

### Defining errors

Always define errors as static members near the domain type. Never scatter `new ResultError(...)` inline throughout business logic.

```csharp
internal static class UserErrors
{
    internal static ResultError NotFound(Guid id) =>
        new("User.NotFound", $"User '{id}' was not found.");

    internal static readonly ResultError EmailAlreadyExists =
        new("User.EmailAlreadyExists", "A user with this email already exists.");

    internal static readonly ResultError InvalidEmail =
        new("User.InvalidEmail", "The email address is invalid.");
}
```

Error code format: `"Domain.Reason"` — PascalCase, dot-separated.

---

## Composing Results — Preferred Patterns

### Single-step pipeline

```csharp
return GetUser(id).OnSuccess(user => UpdateEmail(user, newEmail));
```

### Multi-step pipeline — use LINQ query syntax

```csharp
return from user    in GetUser(id)
       from updated in UpdateEmail(user, newEmail)
       select updated;
```

### Side-effect logging

```csharp
return GetUser(id)
    .OnFailure(err => logger.LogWarning("User lookup failed: {Code}", err.Code))
    .OnSuccess(user => ProcessUser(user));
```

### Terminal branching

```csharp
return result.Match(
    onSuccess: user => Ok(user),
    onFailure: err  => Problem(err.Description)
);
```

---

## Anti-Patterns to Avoid

### Checking `.IsSuccess` manually

```csharp
// Wrong — use composition methods instead
if (result.IsSuccess)
    Process(result.Value);

// Correct
result.OnSuccess(Process);
```

### Accessing `.Value` without checking `.IsSuccess`

```csharp
// Wrong — throws InvalidOperationException if failed
var value = result.Value;

// Correct
result.OnSuccess(value => Use(value));
```

### Nesting results

```csharp
// Wrong
Result<Result<User>> nested = GetUser(id).OnSuccess(u => GetResult(u));

// Correct — use OnSuccess with a function returning Result<T>
Result<User> flat = GetUser(id).OnSuccess(u => Transform(u));
```

### Returning null from Result-returning methods

```csharp
// Wrong
Result<User>? FindUser(Guid id) => null;

// Correct
Result<User> FindUser(Guid id) => UserErrors.NotFound(id);
```

### Using ResultError.None as a failure

```csharp
// Wrong — None is reserved for "no error"
Result<User> f = ResultError.None;

// Correct
Result<User> f = new ResultError("User.NotFound", "User was not found.");
```

### Swallowing failures silently

```csharp
// Wrong
result.OnSuccess(Process);
return Nothing.Value; // ignores failure

// Correct — propagate the result
return result.OnSuccess(Process);
```

---

## Batch Validation Pattern

Use `ResultAggregate` when you need to collect all validation errors before returning. Never short-circuit on the first failure in validation scenarios.

```csharp
var aggregate = ResultAggregate.Create();

aggregate
    .Ensure(() => !string.IsNullOrEmpty(email),  UserErrors.InvalidEmail)
    .Ensure(() => password.Length >= 8,           UserErrors.PasswordTooShort)
    .Ensure(() => age >= 18,                      UserErrors.UnderAge,
            EnsureOnFailure.IgnoreOnFailure); // optional check

return aggregate.OnSuccess(() => CreateUser(email, password, age));
```

Use `EnsureOnFailure.IgnoreOnFailure` only for optional/conditional checks — never for mandatory validations that must always run.

---

## Async Patterns

All composition methods have `Task<Result<T>>` overloads. Chain them without `await` mid-pipeline.

```csharp
// Correct — chain without intermediate awaits
return await GetUserAsync(id)
    .OnSuccess(user => ValidateAsync(user))
    .OnSuccess(user => SaveAsync(user))
    .OnFailure(err  => logger.LogAsync(err));

// Wrong — breaks the pipeline
var userResult = await GetUserAsync(id);
if (userResult.IsSuccess)
{
    var validated = await ValidateAsync(userResult.Value);
    ...
}
```

Converting existing async code:

```csharp
// Task<T> → Task<Result<T>>
Task<Result<User>> result = dbContext.Users.FindAsync(id).ToResult();

// Task<ResultError> → Task<Result<T>>
Task<Result<User>> result = GetErrorAsync().ToResult<User>();
```

---

## Performance Considerations

- `Result<T>` is a `readonly struct` — passed by value, no heap allocation for the wrapper itself (value may still allocate depending on `T`)
- `Nothing` is a zero-byte struct — use it freely for void-like results
- `ResultError` is a `record` (heap-allocated) — pre-allocate shared errors as `static readonly` fields
- Avoid `ResultAggregate` for single-validation paths — use direct `Result<T>` instead
- In hot paths, prefer `OnSuccess` chains over LINQ query syntax (same semantics, marginally lower overhead)

```csharp
// For shared errors — allocate once
internal static readonly ResultError InvalidEmail =
    new("User.InvalidEmail", "The email address is invalid.");

// For parameterized errors — allocate on use (acceptable)
internal static ResultError NotFound(Guid id) =>
    new("User.NotFound", $"User '{id}' was not found.");
```

---

## Integration Gotchas

### ASP.NET Core — mapping to HTTP responses

Map in the controller or a result-mapping helper; do not map inside the domain.

```csharp
return result.Match(
    onSuccess: value => Ok(value),
    onFailure: err   => err.Code switch
    {
        "User.NotFound"     => NotFound(err.Description),
        "User.InvalidEmail" => BadRequest(err.Description),
        _                   => Problem(err.Description)
    }
);
```

### MediatR / CQRS — handler return types

```csharp
public class GetUserHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    public Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken ct) =>
        _repo.FindAsync(request.Id)
             .OnSuccess(user => _mapper.Map<UserDto>(user).ToResult());
}
```

### EF Core — async repository pattern

```csharp
public async Task<Result<User>> FindByIdAsync(Guid id)
{
    var user = await _context.Users.FindAsync(id);
    return user is null
        ? UserErrors.NotFound(id)
        : user;
}
```

### FluentValidation integration

Convert `ValidationResult` to `ResultAggregate` at the boundary:

```csharp
var validation = await _validator.ValidateAsync(command);
if (!validation.IsValid)
{
    var aggregate = ResultAggregate.Create();
    foreach (var failure in validation.Errors)
        aggregate.AddResult(new ResultError(failure.ErrorCode, failure.ErrorMessage));
    return aggregate.OnSuccess<User>(() => throw new UnreachableException());
}
```

### Implicit `T → Result<T>` conversion

The implicit operator means you can return `T` directly from a `Result<T>`-returning method. This is intentional — do not add explicit `.ToResult()` calls when the implicit conversion suffices.

```csharp
Result<int> Compute() => 42;          // OK — implicit conversion
Result<int> Compute() => 42.ToResult(); // also OK, but redundant
```

---

## Testing

Use the `DrifterApps.Seeds.FluentResult.FluentAssertions` package for readable assertions.

```csharp
// Assert success with specific value
result.Should().BeSuccessful().And.WithValue(expectedUser);

// Assert failure with specific error
result.Should().BeFailure().And.WithError(UserErrors.NotFound(id));

// Assert success only (value not checked)
result.Should().BeSuccessful();

// Assert failure only (error not checked)
result.Should().BeFailure();
```

Do not assert via `.IsSuccess`/`.Value` directly in tests — the custom assertions produce better failure messages.

---

## Code Style

- Group error definitions together in a dedicated `*Errors` static class per domain type
- Keep pipeline chains vertically aligned — one method call per line
- Prefer `from … select` LINQ syntax for pipelines with 3+ steps
- For 1–2 steps, prefer explicit `OnSuccess` chains (clearer intent)
- Never mix `.Result` (sync blocking) with async code — await the pipeline end
