# FluentResult

FluentResult is a C# library that provides a fluent API for handling results of operations, including success and failure cases. It includes various utilities and extensions to simplify error handling and result aggregation.

Some of my inspirations comes from [Andrew Lock's Series: Working with the result pattern](https://andrewlock.net/series/working-with-the-result-pattern/)

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
  - [Basic Result Handling](#basic-result-handling)
  - [Aggregating Results](#aggregating-results)
  - [Asynchronous Result Handling](#asynchronous-result-handling)
  - [Ensuring Validation](#ensuring-validation)
- [Contributing](#contributing)
- [License](#license)

## Installation

To install FluentResult, you can use the NuGet package manager:

```sh
dotnet add package FluentResult
```

## Usage

### Basic Result Handling

You can create and handle results using the `Result` class and its extension methods.

```csharp
using DrifterApps.Seeds.FluentResult;

// Creating a success result
var successResult = Result<int>.Success(42);

// Creating a failure result
var failureResult = Result<int>.Failure(new ResultError("Error.Code", "Error description"));

// Handling success and failure
var finalResult = successResult.OnSuccess(value => Result<string>.Success(value.ToString()))
                               .OnFailure(error => Result<string>.Failure(error));
```

### Aggregating Results

You can aggregate multiple results using the ResultAggregate class.

```csharp
using DrifterApps.Seeds.FluentResult;

var resultAggregate = ResultAggregate.Create();
var successResult = Result<Nothing>.Success();
var failureResult = Result<Nothing>.Failure(new ResultError("Error.Code", "Error description"));

resultAggregate.AddResult(successResult);
resultAggregate.AddResult(failureResult);

bool isSuccess = resultAggregate.IsSuccess; // false
bool isFailure = resultAggregate.IsFailure; // true
```

### Asynchronous Result Handling

You can handle results asynchronously using the extension methods provided in ResultExtensions.Async.

```csharp
using DrifterApps.Seeds.FluentResult;

var resultTask = Task.FromResult(Result<int>.Success(42));

var finalResult = await resultTask.OnSuccess(async value => await Task.FromResult(Result<string>.Success(value.ToString())))
                                  .OnFailure(async error => await Task.FromResult(Result<string>.Failure(error)));
```

### Ensuring Validation

You can ensure that a specific validation function returns true using the `Ensure` method. If the validation fails, it adds a failure result to the source.

```csharp
using DrifterApps.Seeds.FluentResult;

var aggregate = ResultAggregate.Create();
aggregate.AddResult(Result<Nothing>.Failure(new ResultError("Error.Code", "Initial error")));

// Ensure validation with IgnoreOnFailure option
var result = aggregate.Ensure(() => true, new ResultError("Validation.Error", "Validation failed"), EnsureOnFailure.IgnoreOnFailure);

// Ensure validation with default ValidateOnFailure option
var resultWithValidation = aggregate.Ensure(() => false, new ResultError("Validation.Error", "Validation failed"));

bool isSuccess = result.IsSuccess; // true
bool isFailure = result.IsFailure; // false

bool isValidationSuccess = resultWithValidation.IsSuccess; // false
bool isValidationFailure = resultWithValidation.IsFailure; // true
```

## Contributing

Contributions are welcome! Please read the contributing guidelines for more information.

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details.
