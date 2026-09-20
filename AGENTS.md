# FluentResult — Agent Instructions

## Engineering principles
@docs/contributing/engineering-principles.md

## Git workflow
@docs/contributing/git-instructions.md

## What this repository is

This repo **is** the source of `DrifterApps.Seeds.FluentResult` — a Railway-Oriented
Programming (ROP) Result pattern library for .NET. You are working *on* the library,
not merely *with* it.

| Path | Contents |
|---|---|
| `src/FluentResult/` | The library: `Result<T>`, `ResultError`, `Nothing`, `ResultAggregate`, `ResultErrorAggregate`, `EnsureOnFailure`, `ResultExtensions` |
| `src/FluentResult.FluentAssertions/` | Test assertions package: `Should().BeSuccessful()`, `.BeFailure()`, `.WithValue()`, `.WithError()` |
| `tests/FluentResult.Tests/` | xUnit v3 test suite (the only test project) |
| `docs/`, `README.md`, `llms.txt` | Published documentation, for people **using** the library — packed into the NuGet package, see "Documentation is part of the API" below |
| `docs/contributing/` | Process docs, for people **working on** this repo — never packed |

Assembly/namespace: `DrifterApps.Seeds.FluentResult`. Target: `net10.0` only.
SDK pinned in `global.json` (10.0.401, `rollForward: latestMinor`).

## Build and test

```bash
dotnet build                      # whole solution
dotnet test                       # whole suite — fast, always run it after each change
dotnet format --verify-no-changes --severity error   # what the linter workflow enforces
dotnet format --severity error                       # same check, but fixes in place
```

Test runner is **Microsoft.Testing.Platform** (`global.json` → `test.runner`), with
xUnit v3 (`xunit.v3.mtp-v2`). There are no long-running tests to exclude — run them all.

### The build is strict

`Directory.Build.props` sets `TreatWarningsAsErrors`, `CodeAnalysisTreatWarningsAsErrors`,
`AnalysisMode=All`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`, and `Nullable=enable`.
A warning **is** a build failure, so "all warnings resolved" in the git workflow is
satisfied by a clean `dotnet build` — never silence one with a blanket `NoWarn`.
Suppress a rule only at the narrowest scope with a justification, the way
`[SuppressMessage("Design", "CA1062:…")]` is used on the existing public types.

`GenerateDocumentationFile` is on: every public member needs an XML doc comment.

## Package management

Versions are centralized — `ManagePackageVersionsCentrally=true`. Add a
`<PackageReference Include="X" />` with **no** `Version` attribute to the csproj, and the
`<PackageVersion>` to `Directory.Packages.props`. Per the engineering principles, do not
add a new dependency without approval.

`FluentAssertions` is pinned to 7.x deliberately (8.x changed its license). Do not bump it
to 8 or beyond.

## Test conventions

Match the existing suite:

- Class carries `[UnitTest]` (the local `ITraitAttribute` adding trait `Category=Unit`).
- Global usings already cover `Bogus`, `DrifterApps.Seeds.FluentResult`, `FluentAssertions`
  — do not re-import them per file.
- Random data comes from a `private static readonly Faker Faker = new();`, never hard-coded
  literals, unless the literal is the thing under test.
- Test names describe behavior: `GivenEnsure_WhenValidateOnFailure_ThenPerformValidation`
  or `AddResult_WithFailureResult_UpdatesAggregate`.
- Bodies use explicit `// Arrange` / `// Act` / `// Assert` sections.
- Parameterized cases use `public static TheoryData<…>` properties with `[Theory]`.
- Assert through the FluentAssertions package under test:
  `result.Should().BeSuccessful().And.WithValue(expected);`
  `result.Should().BeFailure().And.WithError(expectedError);`
- Test files mirror the source file they cover; partial-class splits are wired with
  `<Compile Update … DependentUpon>` in the csproj — keep that mapping when adding files.

CI reports coverage with thresholds `60 80`. New public behavior needs tests; follow the
TDD cycle in the engineering principles (failing test first).

## Library design constraints

These are load-bearing decisions — see `docs/ARCHITECTURE.md` before changing any of them:

- `Result<T>` is a `readonly partial struct` implementing `IEquatable<Result<T>>`. It is
  never null and allocates nothing on the happy path. Keep it a struct.
- Source is split by concern into partials: `Result.cs` (state/equality),
  `Result.Static.cs` (conversions), `Result.Methods.cs` (composition), and the same
  pattern for `ResultAggregate` and `ResultExtensions` (`.Async.cs`). Put new members in
  the partial that matches their concern.
- Implicit conversions are part of the public contract: `T → Result<T>`,
  `ResultError → Result<T>`, `Result<T> → T`, `Result<T> → ResultError`,
  `Result<T> → Task<Result<T>>`.
- Every synchronous composition method needs its `Task<Result<T>>` counterpart in
  `ResultExtensions.Async.cs`.
- `ToErrorAggregate<TOut>()` is **internal**. Consumers reach aggregated errors through
  `ResultAggregate.Match` / `OnSuccess` / `OnFailure`, which produce the
  `ResultErrorAggregate` for them. Do not document it as a consumer-facing call.
- `Ensure` defaults to `EnsureOnFailure.ValidateOnFailure`; `IgnoreOnFailure` skips the
  guard once the aggregate has already failed.
- Programmer errors stay exceptions (`ArgumentNullException.ThrowIfNull`). `Result<T>` is
  for expected domain failures only.

## Documentation is part of the API

`Directory.Build.props` packs `README.md`, `docs/ARCHITECTURE.md`, `docs/AI-GUIDELINES.md`,
`docs/API.md`, and `docs/EXAMPLES.md` into the NuGet package. When the public surface
changes, update in the same change:

- `docs/API.md` — signatures, overloads, conversions
- `docs/EXAMPLES.md` — usage patterns
- `docs/AI-GUIDELINES.md` — rules for assistants consuming the library
- `docs/ARCHITECTURE.md` — only when a design decision changes
- `README.md` and `llms.txt` — only when the overview or doc map changes
- XML doc comments on the members themselves

Treat a doc-only correction as its own `docs:` commit, separate from behavior.

## Usage reference

Do not restate the library's usage rules here — they live in, and are kept current by,
the published docs:

- **`docs/API.md`** — every type, signature, and implicit conversion
- **`docs/EXAMPLES.md`** — domain models, repositories, service layers, ASP.NET Core,
  MediatR, async pipelines, testing
- **`docs/AI-GUIDELINES.md`** — when to apply the pattern, anti-patterns, gotchas
- **`docs/ARCHITECTURE.md`** — why the design is what it is, and what the library will not do

The short version, which the code in this repo must also obey: every operation that can
fail returns `Result<T>`; use `Nothing` for void-like results; define domain errors as
static members near their type; compose with `Match` / `OnSuccess` / `OnFailure` /
`Select` / `SelectMany` (LINQ `from … select` for multi-step pipelines) rather than
`if (result.IsSuccess)` guards; never return `null`, never read `.Value` on an unchecked
result, and never construct a `ResultError` without both `Code` and `Description`.
