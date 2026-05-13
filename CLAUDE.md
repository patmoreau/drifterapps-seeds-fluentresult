# FluentResult — Claude Code Instructions

This repository contains **DrifterApps.Seeds.FluentResult**, a Railway-Oriented Programming (ROP) Result pattern library for .NET. All operations that can fail must return `Result<T>` instead of throwing exceptions or returning null.

**Package:** `DrifterApps.Seeds.FluentResult`
**Namespace:** `using DrifterApps.Seeds.FluentResult;`
**Targets:** .NET 10

## Core Types

| Type | Purpose |
|---|---|
| `Result<T>` | Discriminated union: success carrying `T` or failure carrying `ResultError` |
| `ResultError` | Structured error with `Code` and `Description` strings |
| `Nothing` | Unit/void type — use when the operation has no meaningful return value |
| `ResultAggregate` | Collects multiple `Result<Nothing>` for batch validation |
| `ResultErrorAggregate` | Groups multiple errors by code; produced from `ResultAggregate` |
| `EnsureOnFailure` | Enum controlling whether a guard runs when the aggregate already failed |

## Creating Results

```csharp
// Success — implicit conversion or extension method
Result<int> r1 = 42;
Result<int> r2 = value.ToResult();
Result<Nothing> r3 = Nothing.Value;   // void/unit result

// Failure — implicit conversion or extension method
Result<int> f1 = new ResultError("Domain.NotFound", "Entity was not found");
Result<int> f2 = someError.ToResult<int>();
```

**Domain error pattern** — define errors as static members close to the domain type:

```csharp
internal static class UserErrors
{
    internal static ResultError NotFound(Guid id) =>
        new("User.NotFound", $"User '{id}' was not found.");

    internal static readonly ResultError InvalidEmail =
        new("User.InvalidEmail", "The email address is invalid.");
}
```

## Composing Results

### Match — terminal branch on outcome

```csharp
// Side-effect only
result.Match(
    onSuccess: value => Console.WriteLine($"Got {value}"),
    onFailure: error  => Console.WriteLine(error.Code)
);

// Returns a new Result<TOut>
Result<string> output = result.Match<string>(
    onSuccess: value => value.ToString().ToResult(),
    onFailure: error  => error.ToResult<string>()
);
```

### OnSuccess — continue the happy path

```csharp
// Side-effect, original result returned
result.OnSuccess(value => logger.Log(value));

// Transform value → new Result<TOut>
Result<string> output = intResult.OnSuccess(i => i.ToString().ToResult());

// Flat-map: value → Task/Result from another operation
Result<User> user = idResult.OnSuccess(id => repository.FindById(id));
```

### OnFailure — observe the error without altering the result

```csharp
result.OnFailure(error => logger.LogError(error.Code, error.Description));
// Returns the original result unchanged
```

### Select — pure value transformation (cannot fail)

```csharp
Result<string>  s = intResult.Select(i => i.ToString());
Result<decimal> d = intResult.Select(i => (decimal)i);
```

### SelectMany — monadic bind / LINQ query syntax

```csharp
// Method syntax
var result = orderResult.SelectMany(
    order    => GetCustomer(order.CustomerId),
    (order, customer) => new OrderDto(order, customer)
);

// LINQ query syntax (preferred for multi-step pipelines)
var result = from order    in GetOrder(orderId)
             from customer in GetCustomer(order.CustomerId)
             select new OrderDto(order, customer);
```

## Async Patterns

All composition methods have overloads on `Task<Result<T>>`:

```csharp
var result = await GetUserAsync(userId)
    .OnSuccess(user => ValidateAsync(user))
    .OnSuccess(user => SaveAsync(user))
    .OnFailure(err  => logger.LogErrorAsync(err));

// Convert Task<T> → Result<T>
Result<int> r = await Task.FromResult(42).ToResult();

// Implicit: Result<T> → Task<Result<T>>
Task<Result<int>> task = (Result<int>)42;
```

## Aggregation — batch validation

```csharp
var aggregate = ResultAggregate.Create();

aggregate.AddResult(ValidateEmail(email));
aggregate.AddResult(ValidatePassword(password));

// Inline guards
aggregate
    .Ensure(() => email.Contains('@'),  new ResultError("Email.Invalid",    "Invalid format"))
    .Ensure(() => password.Length >= 8, new ResultError("Password.TooShort", "Min 8 chars"))
    .Ensure(() => age >= 18,            new ResultError("Age.TooYoung",     "Must be 18+"),
            EnsureOnFailure.IgnoreOnFailure);  // skip if aggregate already failed

if (aggregate.IsFailure)
{
    ResultErrorAggregate errors = aggregate.ToErrorAggregate<User>();
    // errors.Code        = "User.Errors"
    // errors.Description = "Errors occurred"
    // errors.Errors      = IReadOnlyDictionary<string, string[]>
    return errors.ToResult<User>();
}
```

## Testing with FluentAssertions

Add package `DrifterApps.Seeds.FluentResult.FluentAssertions`:

```csharp
result.Should().BeSuccessful();
result.Should().BeSuccessful().And.WithValue(expectedValue);

result.Should().BeFailure();
result.Should().BeFailure().And.WithError(expectedError);
```

## Rules

**DO**
- Return `Result<T>` from every method that can fail
- Use `Nothing` as `T` for void-like operations
- Define domain errors in a static class or static members near the domain type
- Chain with `OnSuccess` / `Select` rather than manual `if (result.IsSuccess)` guards
- Use LINQ `from … select` syntax for multi-step pipelines

**DO NOT**
- Throw exceptions for expected domain failures — return a failure `Result<T>`
- Access `result.Value` without checking `result.IsSuccess` first (throws `InvalidOperationException`)
- Return `null` — use `Result<T>` or `Nothing`
- Construct `ResultError` without both `Code` and `Description`
- Use `EnsureOnFailure.IgnoreOnFailure` for mandatory validations
