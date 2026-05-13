# DrifterApps.Seeds.FluentResult — JetBrains AI Guidelines

Railway-Oriented Programming (ROP) Result pattern for .NET.

**Package:** `DrifterApps.Seeds.FluentResult`
**Namespace:** `using DrifterApps.Seeds.FluentResult;`
**Targets:** .NET 8, 9, 10

---

## Core Types

| Type | Purpose |
|---|---|
| `Result<T>` | Discriminated union: success with `T` or failure with `ResultError` |
| `ResultError` | Structured error with `Code` and `Description` strings |
| `Nothing` | Unit/void type — use when there's no meaningful return value |
| `ResultAggregate` | Collects `Result<Nothing>` for batch validation |
| `ResultErrorAggregate` | Errors grouped by code; produced from `ResultAggregate` |
| `EnsureOnFailure` | `ValidateOnFailure` (default) or `IgnoreOnFailure` |

---

## Creating Results

```csharp
// Success — implicit from value, or extension method
Result<int> r1 = 42;
Result<int> r2 = value.ToResult();
Result<Nothing> r3 = Nothing.Value;   // void/unit result

// Failure — implicit from error, or extension method
Result<int> f1 = new ResultError("Domain.NotFound", "Entity was not found");
Result<int> f2 = someError.ToResult<int>();
```

**Domain error pattern — static errors near the domain type:**

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

## Composition API

### Match — terminal branch on success/failure

```csharp
// Side-effect
result.Match(
    onSuccess: value => Console.WriteLine($"Got {value}"),
    onFailure: error  => Console.WriteLine(error.Code)
);

// Returns new Result<TOut>
Result<string> output = result.Match<string>(
    onSuccess: value => value.ToString().ToResult(),
    onFailure: error  => error.ToResult<string>()
);
```

### OnSuccess — continue on the happy path

```csharp
result.OnSuccess(value => logger.Log(value));                          // side-effect
Result<string> s = intResult.OnSuccess(i => i.ToString().ToResult()); // transform
Result<User>   u = idResult.OnSuccess(id => repo.FindById(id));       // flat-map
```

### OnFailure — observe the error; result is unchanged

```csharp
result.OnFailure(error => logger.LogError(error.Code, error.Description));
```

### Select — pure value transform (cannot fail)

```csharp
Result<string>  s = intResult.Select(i => i.ToString());
Result<decimal> d = intResult.Select(i => (decimal)i);
```

### SelectMany — multi-step pipeline (LINQ query syntax)

```csharp
var result = from order    in GetOrder(orderId)
             from customer in GetCustomer(order.CustomerId)
             select new OrderDto(order, customer);

// Equivalent method syntax
var result = orderResult.SelectMany(
    order    => GetCustomer(order.CustomerId),
    (order, customer) => new OrderDto(order, customer)
);
```

---

## Async Patterns

All methods have `Task<Result<T>>` overloads for seamless async pipelines:

```csharp
var result = await GetUserAsync(userId)
    .OnSuccess(user => ValidateAsync(user))
    .OnSuccess(user => SaveAsync(user))
    .OnFailure(err  => LogAsync(err));

// Task<T> → Result<T>
Result<int> r = await Task.FromResult(42).ToResult();

// Result<T> → Task<Result<T>> (implicit)
Task<Result<int>> t = (Result<int>)42;
```

---

## Aggregation — batch validation

```csharp
var agg = ResultAggregate.Create();

agg.AddResult(ValidateEmail(email));
agg.AddResult(ValidatePassword(password));

agg
    .Ensure(() => email.Contains('@'),  new ResultError("Email.Invalid",    "Invalid format"))
    .Ensure(() => password.Length >= 8, new ResultError("Password.TooShort", "Min 8 chars"))
    .Ensure(() => age >= 18,            new ResultError("Age.TooYoung",     "Must be 18+"),
            EnsureOnFailure.IgnoreOnFailure);   // skip check if already failed

if (agg.IsFailure)
{
    ResultErrorAggregate errors = agg.ToErrorAggregate<User>();
    // errors.Code        = "User.Errors"
    // errors.Description = "Errors occurred"
    // errors.Errors      = IReadOnlyDictionary<string, string[]>
    return errors.ToResult<User>();
}
```

---

## Testing

Add `DrifterApps.Seeds.FluentResult.FluentAssertions` for custom assertions:

```csharp
result.Should().BeSuccessful();
result.Should().BeSuccessful().And.WithValue(expectedValue);

result.Should().BeFailure();
result.Should().BeFailure().And.WithError(expectedError);
```

---

## Rules

**DO:**
- Return `Result<T>` from every method that can fail
- Use `Nothing` as `T` for void-like operations
- Define domain errors as `static readonly` or factory methods near the domain type
- Chain with `OnSuccess` / `Select` rather than `if (result.IsSuccess)` guards
- Use LINQ query syntax for multi-step pipelines

**DO NOT:**
- Throw exceptions for expected domain failures — return a failure `Result<T>`
- Access `result.Value` without first checking `result.IsSuccess` (throws `InvalidOperationException`)
- Return `null` from domain methods — use `Result<T>` or `Nothing`
- Create `ResultError` without both `Code` and `Description`
- Apply `EnsureOnFailure.IgnoreOnFailure` to mandatory validation guards
