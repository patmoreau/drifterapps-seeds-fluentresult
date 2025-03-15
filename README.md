# <img alt='paper plane icons' src='./icon.png' height='10%' width='10%'> FluentResult

FluentResult is a C# library that provides a fluent API for handling results of operations, including success and
failure cases. It includes various utilities and extensions to simplify error handling and result aggregation.

Some of my inspirations comes
from [Andrew Lock's Series: Working with the result pattern](https://andrewlock.net/series/working-with-the-result-pattern/)

[![Build and Publish NuGet Package](https://github.com/patmoreau/drifterapps-seeds-fluentresult/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/patmoreau/drifterapps-seeds-fluentresult/actions/workflows/ci-cd.yml)
[![CodeQL](https://github.com/patmoreau/drifterapps-seeds-fluentresult/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/patmoreau/drifterapps-seeds-fluentresult/actions/workflows/codeql-analysis.yml)
![.Net 8 Tests results](https://gist.githubusercontent.com/patmoreau/51a2fc9fd8b7ed500ed3b6aabe0fc2d6/raw/seeds-fluent-result-tests-badge-net8.0.svg)
![.Net 9 Tests results](https://gist.githubusercontent.com/patmoreau/51a2fc9fd8b7ed500ed3b6aabe0fc2d6/raw/seeds-fluent-result-tests-badge-net9.0.svg)

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
    - [Basic Result Handling](#basic-result-handling)
    - [Aggregating Results](#aggregating-results)
    - [Asynchronous Result Handling](#asynchronous-result-handling)
    - [Matching Results]
    - [Ensuring Validation](#ensuring-validation)
- [Contributing](#contributing)
- [License](#license)

## Installation

To install FluentResult, you can use the NuGet package manager:

```sh
dotnet add package DrifterApps.Seeds.FluentResult
```

## Usage

### Basic Result Handling

You can create and handle results using the `Result` class and its extension methods.

```csharp
using DrifterApps.Seeds.FluentResult;

// Creating a success result by implicit conversion
Result<int> successResult = 42;

// Creating a success result by ToResult extension method
var successResult = 45.ToResult<int>();

// Creating a failure result by implicit conversion
Result<int> failureResult = new ResultError("Error.Code", "Error description");

// Creating a failure result by ToResult extension method
failureResult = new ResultError("Error.Code", "Error description").ToResult<int>();

// Handling success and failure
var finalResult = successResult.OnSuccess(value => (Result<string>)value.ToString())
                               .OnFailure(error => (Result<string>)error);
```

### Aggregating Results

You can aggregate multiple results using the ResultAggregate class.

```csharp
using DrifterApps.Seeds.FluentResult;

var resultAggregate = ResultAggregate.Create();
Result<Nothing> successResult = Nothing.Value;
Result<Nothing> failureResult = new ResultError("Error.Code", "Error description");

resultAggregate.AddResult(successResult);
resultAggregate.AddResult(failureResult);

bool isSuccess = resultAggregate.IsSuccess; // false
bool isFailure = resultAggregate.IsFailure; // true
```

### Asynchronous Result Handling

You can handle results asynchronously using the extension methods provided in ResultExtensions.Async.

```csharp
using DrifterApps.Seeds.FluentResult;

var resultTask = Task.FromResult((Result<int>)42);

var finalResult = await resultTask.OnSuccess(async value => await Task.FromResult((Result<string>)value.ToString()))
                                  .OnFailure(async error => await Task.FromResult((Result<string>)error));
```

### Matching Results

You can perform different actions based on success or failure:

```csharp
using DrifterApps.Seeds.FluentResult;

Result<int> result = 42;

result.Match(
    onSuccess: value => Console.WriteLine($"Success with value: {value}"),
    onFailure: error => Console.WriteLine($"Error: {error.Code} - {error.Message}")
);
```

### Ensuring Validation

You can ensure that a specific validation function returns true using the `Ensure` method. If the validation fails, it
adds a failure result to the source.

```csharp
using DrifterApps.Seeds.FluentResult;

var aggregate = ResultAggregate.Create();
aggregate.AddResult(new ResultError("Error.Code", "Initial error"));

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
