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
    - [Matching Results](#matching-results)
    - [Ensuring Validation](#ensuring-validation)
- [AI Coding Assistants](#ai-coding-assistants)
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

## AI Coding Assistants

This repository ships instruction files for the most common AI coding assistants. If you are using this library in your own project, copy the file for your IDE into your repository so your AI assistant understands the Result pattern and generates correct code without prompting.

| IDE / Tool | Instruction file | Where to copy it in your project |
| --- | --- | --- |
| **Claude Code** | [CLAUDE.md](https://github.com/patmoreau/drifterapps-seeds-fluentresult/blob/main/CLAUDE.md) | `CLAUDE.md` at the project root |
| **GitHub Copilot** | [copilot-instructions.md](https://github.com/patmoreau/drifterapps-seeds-fluentresult/blob/main/.github/copilot-instructions.md) | `.github/copilot-instructions.md` |
| **Cursor** | [fluentresult.mdc](https://github.com/patmoreau/drifterapps-seeds-fluentresult/blob/main/.cursor/rules/fluentresult.mdc) | `.cursor/rules/fluentresult.mdc` |
| **Windsurf** | [fluentresult.md](https://github.com/patmoreau/drifterapps-seeds-fluentresult/blob/main/.windsurf/rules/fluentresult.md) | `.windsurf/rules/fluentresult.md` |
| **JetBrains AI (Junie)** | [guidelines.md](https://github.com/patmoreau/drifterapps-seeds-fluentresult/blob/main/.junie/guidelines.md) | `.junie/guidelines.md` |

> **Tip:** you only need the file for the IDE you are using.

### Quick copy via curl

```sh
# Claude Code
curl -sLo CLAUDE.md https://raw.githubusercontent.com/patmoreau/drifterapps-seeds-fluentresult/main/CLAUDE.md

# GitHub Copilot
mkdir -p .github && curl -sLo .github/copilot-instructions.md https://raw.githubusercontent.com/patmoreau/drifterapps-seeds-fluentresult/main/.github/copilot-instructions.md

# Cursor
mkdir -p .cursor/rules && curl -sLo .cursor/rules/fluentresult.mdc https://raw.githubusercontent.com/patmoreau/drifterapps-seeds-fluentresult/main/.cursor/rules/fluentresult.mdc

# Windsurf
mkdir -p .windsurf/rules && curl -sLo .windsurf/rules/fluentresult.md https://raw.githubusercontent.com/patmoreau/drifterapps-seeds-fluentresult/main/.windsurf/rules/fluentresult.md

# JetBrains AI (Junie)
mkdir -p .junie && curl -sLo .junie/guidelines.md https://raw.githubusercontent.com/patmoreau/drifterapps-seeds-fluentresult/main/.junie/guidelines.md
```

## Contributing

Contributions are welcome! Please read the contributing guidelines for more information.

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details.
