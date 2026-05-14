# Architecture — DrifterApps.Seeds.FluentResult

Design decisions and trade-offs for the Railway-Oriented Programming implementation.

---

## Philosophy

FluentResult implements **Railway-Oriented Programming (ROP)**: operations return a `Result<T>` that is either a success or a failure, and the pipeline automatically routes around failures without manual null checks or exception handlers.

The goal is to make the failure path as natural to express as the happy path, while keeping the type system honest: callers cannot ignore the possibility of failure.

---

## Core Design Decisions

### `Result<T>` as a `readonly struct`

`Result<T>` is a value type, not a reference type. This eliminates heap allocation for the result wrapper itself. The trade-off is that `Result<T>` is copied on assignment, which is acceptable because results are typically short-lived and not stored in large collections.

Consequences:
- Default `Result<T>` (zero-initialized) is a failure with `ResultError.None`. Code must never rely on this; always construct via implicit conversion or extension methods.
- Cannot be `null` — one source of nullability bugs eliminated.

### `ResultError` as a `record`

`ResultError` is a reference type with value-based equality. Errors can be pre-allocated as `static readonly` fields and compared by value without custom equality plumbing.

The `None` sentinel (both strings empty) represents "no error" and is the default `Error` on a success result. The constructor enforces that you cannot supply one field without the other.

### `Nothing` as a zero-byte struct

Replacing `void` in `Result<void>` with `Result<Nothing>` allows the library to be generic without special-casing void. `Nothing` carries no data and all instances are equal.

`Nothing.Task` is a pre-completed `Task<Nothing>` — avoids allocating a new task for every async void-result operation.

### Partial classes for `Result<T>` and `ResultAggregate`

The core type is split across files by concern:

| File | Contains |
|---|---|
| `Result.cs` | Properties, equality, `ToResult`, `ToTask`, `FromResult` |
| `Result.Methods.cs` | `Match`, `OnSuccess`, `OnFailure` |
| `Result.Static.cs` | Implicit conversion operators |

This keeps each file focused and allows the static operators (which require the full type to be visible) to be isolated from instance methods.

### Extension methods vs. instance methods

- **Instance methods** (`Match`, `OnSuccess`, `OnFailure`) are on `Result<T>` directly — they are the primary composition API and must be discoverable via IntelliSense.
- **Extension methods** (`Select`, `SelectMany`, `ToResult`) live in `ResultExtensions` — they extend `T`, `ResultError`, and `Task<Result<T>>`. Keeping them separate avoids polluting the main struct with combinators that are rarely needed.
- **Async extensions** (`Task<Result<T>>` overloads) are in `ResultExtensions.Async` — separating sync and async surfaces keeps each file small and lets consumers reference only what they need.

### LINQ integration via `Select` / `SelectMany`

Implementing `Select` and `SelectMany` on `Result<T>` enables the C# query expression syntax:

```csharp
from order    in GetOrder(orderId)
from customer in GetCustomer(order.CustomerId)
select new OrderDto(order, customer)
```

This is syntactic sugar over `SelectMany` calls. The compiler desugars it to:

```csharp
GetOrder(orderId).SelectMany(
    order    => GetCustomer(order.CustomerId),
    (order, customer) => new OrderDto(order, customer))
```

The result is a pipeline where any failure short-circuits the chain. Prefer LINQ syntax for 3+ steps and explicit `OnSuccess` for 1–2 steps.

---

## ResultAggregate Design

`ResultAggregate` exists to solve the **collect-all-errors** problem, distinct from the **short-circuit-on-first-failure** problem.

It is a `record` (not a struct) because it is mutable (via `AddResult`) and may be passed across method boundaries for incremental population.

### `Ensure` vs `AddResult`

`AddResult` takes a pre-computed `Result<Nothing>` — the validation has already run. `Ensure` takes a delegate so evaluation is deferred and can be skipped conditionally via `EnsureOnFailure.IgnoreOnFailure`.

Use `Ensure` in fluent validation chains. Use `AddResult` when the result comes from an existing method.

### `ToErrorAggregate<T>()`

The type parameter `T` names the aggregate — `ToErrorAggregate<User>()` produces code `"User.Errors"`. This mirrors how ASP.NET Core's `ValidationProblemDetails` groups errors: the outer code identifies the failing entity; the inner dictionary maps field codes to messages.

---

## Async Architecture

Every synchronous method has an async counterpart. The strategy:

1. `Result<T>` instance methods (`OnSuccess`, `OnFailure`, `Match`) return `Task<Result<T>>` when given async delegates.
2. `Task<Result<T>>` extension methods (`ResultExtensions.Async`) wrap the sync instance methods, awaiting the task and then delegating.
3. `implicit operator Task<Result<T>>(Result<T>)` allows a sync result to be returned where an async result is expected, enabling mixed sync/async pipelines without `.ToTask()` noise.

This means a pipeline can mix sync and async steps:

```csharp
await syncResult               // Result<T>
    .OnSuccess(syncStep)        // Result<T> → still sync
    .OnSuccess(asyncStep)       // Result<T> → Task<Result<T>>
    .OnFailure(asyncLog);       // Task<Result<T>> extension method
```

---

## Error Code Convention

Error codes follow `"Domain.Reason"` PascalCase to enable prefix-based routing in controllers and middleware. This allows:

```csharp
err.Code.StartsWith("User.") => NotFound / Conflict / ...
err.Code.StartsWith("Order.") => ...
```

Aggregate error codes follow `"TypeName.Errors"` — produced automatically by `ToErrorAggregate<T>()`.

---

## Testing Architecture

`FluentResult.FluentAssertions` is a separate NuGet package so production code does not take a test-framework dependency. It implements `ReferenceTypeAssertions<Result<T>, ResultAssertions<T>>` following the FluentAssertions custom assertions pattern.

The assertion chain:
```
result.Should()  →  ResultAssertions<T>
    .BeSuccessful()  →  AndConstraint<ResultAssertions<T>>
    .And.WithValue(x)
```

`And` returns the same `ResultAssertions<T>` instance, so `WithValue` and `WithError` can be chained after `BeSuccessful`/`BeFailure` in a single expression.

---

## What This Library Does Not Do

- **No exception wrapping.** `Result<T>` is for expected domain failures. Programmer errors (null arguments, out-of-range indices) remain exceptions.
- **No async void.** All async paths return `Task<Result<T>>` or `Task<Nothing>`.
- **No built-in HTTP mapping.** Mapping error codes to HTTP status codes is application-layer responsibility.
- **No serialization support.** `Result<T>` is not designed to cross process boundaries. Serialize the unwrapped value or a DTO derived from the error.
- **No multi-targeting.** The library targets .NET 10 only. Older runtimes are not supported.
