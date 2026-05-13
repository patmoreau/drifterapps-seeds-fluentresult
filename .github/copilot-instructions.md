# GitHub Copilot Instructions — DrifterApps.Seeds.FluentResult

This repository implements the **Railway-Oriented Programming (ROP) Result pattern** for .NET.

**Package:** `DrifterApps.Seeds.FluentResult`
**Namespace:** `using DrifterApps.Seeds.FluentResult;`
**Targets:** .NET 8, 9, 10

---

## Core Types

| Type | Purpose |
|---|---|
| `Result<T>` | Discriminated union: success with value `T`, or failure with `ResultError` |
| `ResultError` | Structured error: `Code` (string) + `Description` (string) |
| `Nothing` | Unit/void type for operations with no meaningful return value |
| `ResultAggregate` | Collects multiple `Result<Nothing>` for batch validation |
| `ResultErrorAggregate` | Groups collected errors by code |
| `EnsureOnFailure` | `ValidateOnFailure` (default) or `IgnoreOnFailure` |

---

## Creating Results

```csharp
// Success
Result<int> r1 = 42;                       // implicit from value
Result<int> r2 = value.ToResult();         // extension method
Result<Nothing> r3 = Nothing.Value;        // unit result

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

---

## Composing Results

### Match — branch on outcome (terminal)

```csharp
// Side-effect
result.Match(
    onSuccess: value => Console.WriteLine($"Got {value}"),
    onFailure: error  => Console.WriteLine(error.Code)
);

// Produce a new Result<TOut>
Result<string> output = result.Match<string>(
    onSuccess: value => value.ToString().ToResult(),
    onFailure: error  => error.ToResult<string>()
);
```

### OnSuccess — continue on the happy path

```csharp
result.OnSuccess(value => logger.Log(value));                          // side-effect
Result<string> s = intResult.OnSuccess(i => i.ToString().ToResult()); // transform
Result<User>   u = idResult.OnSuccess(id => repository.FindById(id)); // flat-map
```

### OnFailure — observe the error, result unchanged

```csharp
result.OnFailure(error => logger.LogError(error.Code, error.Description));
```

### Select — pure value transform (cannot fail)

```csharp
Result<string>  s = intResult.Select(i => i.ToString());
Result<decimal> d = intResult.Select(i => (decimal)i);
```

### SelectMany — multi-step pipeline

```csharp
// LINQ query syntax (preferred)
var result = from order    in GetOrder(orderId)
             from customer in GetCustomer(order.CustomerId)
             select new OrderDto(order, customer);

// Method syntax
var result = orderResult.SelectMany(
    order    => GetCustomer(order.CustomerId),
    (order, customer) => new OrderDto(order, customer)
);
```

---

## Async Patterns

All composition methods have `Task<Result<T>>` overloads:

```csharp
var result = await GetUserAsync(userId)
    .OnSuccess(user => ValidateAsync(user))
    .OnSuccess(user => SaveAsync(user))
    .OnFailure(err  => logger.LogErrorAsync(err));

// Task<T> → Result<T>
Result<int> r = await Task.FromResult(42).ToResult();

// Result<T> → Task<Result<T>> (implicit)
Task<Result<int>> task = (Result<int>)42;
```

---

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
            EnsureOnFailure.IgnoreOnFailure);

if (aggregate.IsFailure)
{
    ResultErrorAggregate errors = aggregate.ToErrorAggregate<User>();
    return errors.ToResult<User>();
}
```

---

## Testing

```csharp
// Uses DrifterApps.Seeds.FluentResult.FluentAssertions
result.Should().BeSuccessful();
result.Should().BeSuccessful().And.WithValue(expectedValue);
result.Should().BeFailure();
result.Should().BeFailure().And.WithError(expectedError);
```

---

## Rules

**Always:**
- Return `Result<T>` from methods that can fail
- Use `Nothing` for void-like operations
- Define domain errors as static readonly members or factory methods
- Chain with `OnSuccess`/`Select` instead of manual `if (result.IsSuccess)` blocks

**Never:**
- Throw exceptions for expected domain failures
- Access `result.Value` without first checking `result.IsSuccess` (throws `InvalidOperationException`)
- Return `null` — use `Result<T>` or `Nothing`
- Construct `ResultError` without both `Code` and `Description`
