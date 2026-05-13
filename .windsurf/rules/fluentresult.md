---
trigger: glob
glob: "**/*.cs"
---

# DrifterApps.Seeds.FluentResult — Windsurf Rules

Railway-Oriented Programming (ROP) Result pattern for .NET.

**Package:** `DrifterApps.Seeds.FluentResult`
**Namespace:** `using DrifterApps.Seeds.FluentResult;`
**Targets:** .NET 8, 9, 10

## Core Types

| Type | Purpose |
|---|---|
| `Result<T>` | Success with value `T` **or** failure with `ResultError` |
| `ResultError` | Structured error: `Code` + `Description` strings |
| `Nothing` | Unit/void type — use when there's no meaningful return value |
| `ResultAggregate` | Collects `Result<Nothing>` for batch validation |
| `ResultErrorAggregate` | Errors grouped by code; produced from `ResultAggregate` |
| `EnsureOnFailure` | `ValidateOnFailure` (default) or `IgnoreOnFailure` |

## Creating Results

```csharp
// Success
Result<int> r1 = 42;
Result<int> r2 = value.ToResult();
Result<Nothing> r3 = Nothing.Value;   // void/unit

// Failure
Result<int> f1 = new ResultError("Domain.NotFound", "Entity was not found");
Result<int> f2 = someError.ToResult<int>();
```

**Domain error pattern:**

```csharp
internal static class UserErrors
{
    internal static ResultError NotFound(Guid id) =>
        new("User.NotFound", $"User '{id}' was not found.");

    internal static readonly ResultError InvalidEmail =
        new("User.InvalidEmail", "The email address is invalid.");
}
```

## Composition API

### Match — terminal branch

```csharp
result.Match(
    onSuccess: value => DoSomething(value),
    onFailure: error  => HandleError(error)
);

Result<TOut> output = result.Match<TOut>(
    onSuccess: value => Convert(value).ToResult(),
    onFailure: error  => error.ToResult<TOut>()
);
```

### OnSuccess — continue on the happy path

```csharp
result.OnSuccess(value => SideEffect(value));                          // pass-through
Result<TOut> r = result.OnSuccess(v => Convert(v).ToResult());        // transform
Result<User> u = idResult.OnSuccess(id => repository.FindById(id));   // flat-map
```

### OnFailure — observe error (result unchanged)

```csharp
result.OnFailure(error => logger.LogError(error.Code, error.Description));
```

### Select — pure value transform

```csharp
Result<string>  s = intResult.Select(i => i.ToString());
Result<decimal> d = intResult.Select(i => (decimal)i);
```

### SelectMany — multi-step pipelines (LINQ query syntax preferred)

```csharp
var result = from order    in GetOrder(orderId)
             from customer in GetCustomer(order.CustomerId)
             select new OrderDto(order, customer);
```

## Async Patterns

```csharp
var result = await GetUserAsync(userId)
    .OnSuccess(user => ValidateAsync(user))
    .OnSuccess(user => SaveAsync(user))
    .OnFailure(err  => LogAsync(err));

Result<int> r    = await Task.FromResult(42).ToResult();
Task<Result<int>> t = (Result<int>)42;   // implicit conversion
```

## Aggregation — batch validation

```csharp
var agg = ResultAggregate.Create();
agg.AddResult(ValidateEmail(email));
agg.AddResult(ValidatePassword(password));
agg
    .Ensure(() => email.Contains('@'),  new ResultError("Email.Invalid",    "Invalid format"))
    .Ensure(() => password.Length >= 8, new ResultError("Password.TooShort", "Min 8 chars"))
    .Ensure(() => age >= 18,            new ResultError("Age.TooYoung",     "Must be 18+"),
            EnsureOnFailure.IgnoreOnFailure);

if (agg.IsFailure)
{
    ResultErrorAggregate errors = agg.ToErrorAggregate<User>();
    return errors.ToResult<User>();
}
```

## Testing

```csharp
// Package: DrifterApps.Seeds.FluentResult.FluentAssertions
result.Should().BeSuccessful();
result.Should().BeSuccessful().And.WithValue(expectedValue);
result.Should().BeFailure();
result.Should().BeFailure().And.WithError(expectedError);
```

## Mandatory Rules

**DO:**
- Return `Result<T>` from every method that can fail
- Use `Nothing` as `T` for void-like operations
- Define errors as `static readonly` or static factory methods near the domain type
- Chain with `OnSuccess` / `Select` instead of `if (result.IsSuccess)` guards
- Use LINQ query syntax for multi-step pipelines

**DO NOT:**
- Throw exceptions for expected domain failures — return a failure `Result<T>`
- Read `result.Value` without first checking `result.IsSuccess` (throws)
- Return `null` from any domain method
- Create `ResultError` without both `Code` and `Description`
- Use `IgnoreOnFailure` for mandatory validation guards
