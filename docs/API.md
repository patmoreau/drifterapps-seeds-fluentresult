# API Reference — DrifterApps.Seeds.FluentResult

**Namespace:** `DrifterApps.Seeds.FluentResult`
**Package:** `DrifterApps.Seeds.FluentResult`
**Target:** .NET 10

---

## Result\<T\>

```csharp
public readonly struct Result<T>
```

Discriminated union: either a success carrying a value of type `T`, or a failure carrying a `ResultError`. Cannot be both.

### Properties

| Property | Type | Description |
|---|---|---|
| `IsSuccess` | `bool` | `true` if the result is a success |
| `IsFailure` | `bool` | `true` if the result is a failure |
| `Value` | `T` | The success value. Throws `InvalidOperationException` when `IsFailure`. |
| `Error` | `ResultError` | The failure error. Returns `ResultError.None` when `IsSuccess`. |

### Implicit conversions

```csharp
// T → Result<T>  (creates success)
public static implicit operator Result<T>(T value)

// ResultError → Result<T>  (creates failure)
public static implicit operator Result<T>(ResultError error)

// Result<T> → Task<Result<T>>  (wraps in completed task)
public static implicit operator Task<Result<T>>(Result<T> value)

// Result<T> → T  (unwraps value; throws if failure)
public static implicit operator T(Result<T> result)

// Result<T> → ResultError  (extracts error; returns ResultError.None if success)
public static implicit operator ResultError(Result<T> result)
```

### Match

Branches on the outcome. Use when you need both branches to produce a value.

```csharp
// Side-effect — returns original result
Result<T> Match(Action<T> onSuccess, Action<ResultError> onFailure)

// Transform — returns new Result<TOut>
Result<TOut> Match<TOut>(
    Func<T, Result<TOut>> onSuccess,
    Func<ResultError, Result<TOut>> onFailure)

// Async side-effect
Task<Result<T>> Match(
    Func<T, Task> onSuccess,
    Func<ResultError, Task> onFailure)

// Async transform
Task<Result<TOut>> Match<TOut>(
    Func<T, Task<Result<TOut>>> onSuccess,
    Func<ResultError, Task<Result<TOut>>> onFailure)
```

### OnSuccess

Continues the happy path. The failure branch is passed through unchanged.

```csharp
// Side-effect — returns original result
Result<T> OnSuccess(Action<T> onSuccess)

// Transform to new result
Result<TOut> OnSuccess<TOut>(Func<T, Result<TOut>> onSuccess)

// Async side-effect
Task<Result<T>> OnSuccess(Func<T, Task> onSuccess)

// Async transform
Task<Result<TOut>> OnSuccess<TOut>(Func<T, Task<Result<TOut>>> onSuccess)
```

### OnFailure

Observes the error without altering the result. Use for logging or metrics.

```csharp
// Side-effect — returns original result unchanged
Result<T> OnFailure(Action<ResultError> onFailure)

// Async side-effect
Task<Result<T>> OnFailure(Func<ResultError, Task> onFailure)
```

### Conversion methods

```csharp
// Identity — returns itself (idempotent)
Result<T> ToResult()

// Wraps in a completed Task
Task<Result<T>> ToTask()

// Extracts the value (same as implicit T conversion)
T FromResult()
```

### Equality

`Result<T>` implements value equality. Two results are equal when both have the same `IsSuccess` state and equal `Value` (on success) or equal `Error` (on failure).

```csharp
bool Equals(Result<T> other)
override int GetHashCode()
public static bool operator ==(Result<T> left, Result<T> right)
public static bool operator !=(Result<T> left, Result<T> right)
```

---

## ResultError

```csharp
public record ResultError(string Code, string Description)
```

Immutable structured error. Both `Code` and `Description` must be provided together; providing one without the other throws `ArgumentException`.

### Static members

| Member | Description |
|---|---|
| `static readonly ResultError None` | Represents absence of error. Both `Code` and `Description` are empty. |

### Properties

| Property | Type | Description |
|---|---|---|
| `Code` | `string` | Machine-readable error code, e.g. `"User.NotFound"` |
| `Description` | `string` | Human-readable error message |

### Methods

```csharp
// Deconstruction support
void Deconstruct(out string code, out string description)
```

### Error code convention

Format: `"Domain.Reason"` — PascalCase, dot-separated.

```csharp
"User.NotFound"
"Order.AlreadyShipped"
"Payment.InsufficientFunds"
```

---

## Nothing

```csharp
public readonly struct Nothing : IEquatable<Nothing>, IComparable<Nothing>, IComparable
```

Unit/void type. Use as `T` in `Result<Nothing>` when an operation has no meaningful return value.

### Static members

| Member | Type | Description |
|---|---|---|
| `Value` | `ref readonly Nothing` | The only value of this type |
| `Task` | `Task<Nothing>` | Pre-completed task returning `Nothing.Value` |

### Comparison

All `Nothing` instances are equal and compare as equal (`CompareTo` always returns 0).

```csharp
override string ToString() // Returns "()"
```

---

## ResultAggregate

```csharp
public partial record ResultAggregate
```

Accumulates multiple `Result<Nothing>` results for batch validation. Use when all validations must run before determining overall success.

### Factory

```csharp
static ResultAggregate Create()
```

### Properties

| Property | Type | Description |
|---|---|---|
| `Results` | `IReadOnlyCollection<Result<Nothing>>` | All collected results |
| `IsSuccess` | `bool` | `true` only if all results are successful |
| `IsFailure` | `bool` | `true` if any result is a failure |

### AddResult

```csharp
void AddResult(Result<Nothing> result)
```

Appends a result to the collection.

### Ensure

Adds a guard that evaluates lazily.

```csharp
// Predicate-based guard
ResultAggregate Ensure(
    Func<bool> validation,
    ResultError error,
    EnsureOnFailure options = EnsureOnFailure.ValidateOnFailure)

// Result-returning guard
ResultAggregate Ensure(
    Func<Result<Nothing>> validation,
    EnsureOnFailure options = EnsureOnFailure.ValidateOnFailure)
```

When `options` is `EnsureOnFailure.IgnoreOnFailure`, the guard is skipped if the aggregate already has failures.

### Match

```csharp
Result<Nothing> Match(
    Action onSuccess,
    Action<ResultErrorAggregate> onFailure)

Result<TOut> Match<TOut>(
    Func<Result<TOut>> onSuccess,
    Func<ResultErrorAggregate, Result<TOut>> onFailure)

Task<Result<Nothing>> Match(
    Func<Task> onSuccess,
    Func<ResultErrorAggregate, Task> onFailure)

Task<Result<TOut>> Match<TOut>(
    Func<Task<Result<TOut>>> onSuccess,
    Func<ResultErrorAggregate, Task<Result<TOut>>> onFailure)
```

### OnSuccess

```csharp
Result<Nothing> OnSuccess(Action onSuccess)
Result<TOut> OnSuccess<TOut>(Func<Result<TOut>> onSuccess)
Task<Result<Nothing>> OnSuccess(Func<Task> onSuccess)
Task<Result<TOut>> OnSuccess<TOut>(Func<Task<Result<TOut>>> onSuccess)
```

### OnFailure

```csharp
Result<Nothing> OnFailure(Action<ResultErrorAggregate> onFailure)
Task<Result<Nothing>> OnFailure(Func<ResultErrorAggregate, Task> onFailure)
```

---

## ResultErrorAggregate

```csharp
public record ResultErrorAggregate(
    string Code,
    string Description,
    IReadOnlyDictionary<string, string[]> Errors) : ResultError(Code, Description)
```

Extends `ResultError` with a dictionary grouping all collected error descriptions by their code. Produced by `ResultAggregate` when the aggregate has failures.

### Properties

| Property | Type | Description |
|---|---|---|
| `Code` | `string` | Aggregate error code, e.g. `"User.Errors"` |
| `Description` | `string` | Aggregate description, e.g. `"Errors occurred"` |
| `Errors` | `IReadOnlyDictionary<string, string[]>` | Each key is an error code; each value is an array of descriptions |

The `Code` follows the pattern `"{TypeName}.Errors"` where `TypeName` is the type argument passed to `ToErrorAggregate<T>()`.

---

## EnsureOnFailure

```csharp
public enum EnsureOnFailure
```

Controls whether a guard inside `ResultAggregate.Ensure` runs when the aggregate already has failures.

| Value | Behavior |
|---|---|
| `ValidateOnFailure` | Always run the guard (default) |
| `IgnoreOnFailure` | Skip the guard if the aggregate already has failures |

---

## ResultExtensions

Static class providing extension methods for `Result<T>` and `Task<Result<T>>`.

### ToResult (value → result)

```csharp
// Convert value to success Result<T>
// Throws ArgumentNullException if value is null for a non-nullable type
// Throws InvalidOperationException if T is ResultError
Result<T> ToResult<T>(this T source)

// Async variant
Task<Result<T>> ToResult<T>(this Task<T> source)
```

### ToResult (error → result)

```csharp
// Convert error to failure Result<T>
// Throws ArgumentException if error is ResultError.None
Result<T> ToResult<T>(this ResultError error)

// Async variant
Task<Result<T>> ToResult<T>(this Task<ResultError> source)
```

### Select

Pure value transformation that cannot fail. Maps the success value; failures pass through.

```csharp
Result<TResult> Select<TFrom, TResult>(
    this Result<TFrom> source,
    Func<TFrom, TResult> selector)

Task<Result<TResult>> Select<TFrom, TResult>(
    this Task<Result<TFrom>> source,
    Func<TFrom, Task<TResult>> selector)
```

### SelectMany

Monadic bind. Enables LINQ query syntax for multi-step pipelines.

```csharp
Result<TResult> SelectMany<TSource, TMiddle, TResult>(
    this Result<TSource> source,
    Func<TSource, Result<TMiddle>> collectionSelector,
    Func<TSource, TMiddle, TResult> resultSelector)

Task<Result<TResult>> SelectMany<TSource, TMiddle, TResult>(
    this Task<Result<TSource>> source,
    Func<TSource, Task<Result<TMiddle>>> collectionSelector,
    Func<TSource, TMiddle, TResult> resultSelector)
```

### Async extension methods on Task\<Result\<T\>\>

All `Result<T>` methods (`Match`, `OnSuccess`, `OnFailure`) are available as extension methods on `Task<Result<T>>`:

```csharp
Task<Result<T>> Match<T>(this Task<Result<T>> resultTask,
    Action<T> onSuccess, Action<ResultError> onFailure)

Task<Result<TOut>> Match<TIn, TOut>(this Task<Result<TIn>> resultTask,
    Func<TIn, Result<TOut>> onSuccess,
    Func<ResultError, Result<TOut>> onFailure)

Task<Result<T>> Match<T>(this Task<Result<T>> resultTask,
    Func<T, Task> onSuccess, Func<ResultError, Task> onFailure)

Task<Result<TOut>> Match<TIn, TOut>(this Task<Result<TIn>> resultTask,
    Func<TIn, Task<Result<TOut>>> onSuccess,
    Func<ResultError, Task<Result<TOut>>> onFailure)

Task<Result<T>> OnSuccess<T>(this Task<Result<T>> resultTask, Action<T> next)

Task<Result<TOut>> OnSuccess<TIn, TOut>(this Task<Result<TIn>> resultTask,
    Func<TIn, Result<TOut>> next)

Task<Result<T>> OnSuccess<T>(this Task<Result<T>> resultTask, Func<T, Task> next)

Task<Result<TOut>> OnSuccess<TIn, TOut>(this Task<Result<TIn>> resultTask,
    Func<TIn, Task<Result<TOut>>> next)

Task<Result<TIn>> OnFailure<TIn>(this Task<Result<TIn>> resultTask,
    Action<ResultError> next)

Task<Result<T>> OnFailure<T>(this Task<Result<T>> resultTask,
    Func<ResultError, Task> next)
```

---

## FluentAssertions Integration

**Package:** `DrifterApps.Seeds.FluentResult.FluentAssertions`

```csharp
// Entry point — call .Should() on any Result<T>
ResultAssertions<TValue> Should<TValue>(this Result<TValue> instance)
```

### Assertion methods

```csharp
// Assert the result is successful
AndConstraint<ResultAssertions<TValue>> BeSuccessful(
    string because = "", params object[] becauseArgs)

// Assert the result is a failure
AndConstraint<ResultAssertions<TValue>> BeFailure(
    string because = "", params object[] becauseArgs)

// Assert the success value equals expectedValue (chain after BeSuccessful)
AndConstraint<ResultAssertions<TValue>> WithValue(
    TValue expectedValue,
    string because = "", params object[] becauseArgs)

// Assert the failure error equals resultError (chain after BeFailure)
AndConstraint<ResultAssertions<TValue>> WithError(
    ResultError resultError,
    string because = "", params object[] becauseArgs)
```

### Usage pattern

```csharp
// Check success + value
result.Should().BeSuccessful().And.WithValue(42);

// Check failure + specific error
result.Should().BeFailure().And.WithError(UserErrors.NotFound(id));

// Check success without checking value
result.Should().BeSuccessful();

// Check failure without checking which error
result.Should().BeFailure();
```
