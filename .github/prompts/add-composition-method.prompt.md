---
mode: agent
description: Add a composition method to Result or ResultAggregate together with its async counterpart and docs
---

<!-- Generated from docs/contributing/tasks/add-composition-method.md by scripts/sync-agent-files.sh. Do not edit. -->

# Add a composition method

> **Audience:** contributors *working on* this repository. Not packaged — see `AGENTS.md` at the repository root.

Composition is the public surface of this library: `Match`, `OnSuccess`,
`OnFailure`, `Select`, `SelectMany`, `Ensure`, `AddResult`. Two rules govern every
addition and both are load-bearing — see `docs/ARCHITECTURE.md` before changing
either:

1. **Every synchronous method needs its `Task<Result<T>>` counterpart** — in the
   same file for the instance methods on `Result<T>` and `ResultAggregate`, in
   `src/FluentResult/ResultExtensions.Async.cs` for the extension methods. A
   sync-only method breaks pipelines the moment one step goes async.
2. **Programmer errors stay exceptions.** `Result<T>` is for expected domain
   failures. Guard arguments with `ArgumentNullException.ThrowIfNull`; do not
   wrap a null delegate in a failure result.

Ask the user which signature to add if it is not already stated (receiver: a
`Result<T>` or a `ResultAggregate`; whether the type changes, `T -> TOut`;
whether the delegate is sync or async).

## 1. Find the shape to copy

| Where | Holds |
|---|---|
| `src/FluentResult/Result.cs` | state and equality — rarely touched |
| `src/FluentResult/Result.Static.cs` | conversions |
| `src/FluentResult/Result.Methods.cs` | instance composition: `Match`, `OnSuccess`, `OnFailure`, each in a `Result<T>` and a `Task<Result<T>>` form |
| `src/FluentResult/ResultExtensions.cs` | extension composition: `ToResult`, `Select`, `SelectMany` |
| `src/FluentResult/ResultExtensions.Async.cs` | the `Task<Result<T>>` counterpart of everything above |
| `src/FluentResult/ResultAggregate.Methods.cs` | aggregate composition: `Ensure`, `AddResult`, terminal `Match` / `OnSuccess` / `OnFailure` |

Copy the closest existing member. Both a same-type (`Result<T> -> Result<T>`) and
a type-changing (`Result<TIn> -> Result<TOut>`) overload usually exist; add both
when the operation can change the type.

Put the member in the partial that matches its concern. Do not consolidate the
partials.

## 2. Verify async parity mechanically

The instance methods carry their `Task` form in the **same** file; the extension
methods carry theirs in `ResultExtensions.Async.cs`:

```bash
for file in src/FluentResult/Result.Methods.cs src/FluentResult/ResultAggregate.Methods.cs; do
  for name in Match OnSuccess OnFailure; do
    grep -qE "public +(async +)?Task<Result<[A-Za-z<>, ]*>> +$name\b" "$file" ||
      echo "MISSING Task form of $name in $(basename "$file")"
  done
done

for name in ToResult Select SelectMany; do
  grep -qE "public static +(async +)?Task<Result<[A-Za-z<>, ]*>> +$name\b" \
    src/FluentResult/ResultExtensions.Async.cs ||
    echo "MISSING async counterpart of $name in ResultExtensions.Async.cs"
done
```

Nothing must print. Add the name you introduced to the matching list above in the
same change, so the next person's check covers it too.

`Ensure` and `AddResult` are deliberately absent from these lists: they mutate a
`ResultAggregate` and return it, so they have no `Task<Result<T>>` form. Every
other composition method does.

This catches an omitted counterpart, not a wrong signature — still read the async
file and confirm the shape matches its sync twin.

## 3. Respect the public contract

- Implicit conversions are part of the API and must keep working:
  `T -> Result<T>`, `ResultError -> Result<T>`, `Result<T> -> T`,
  `Result<T> -> ResultError`, `Result<T> -> Task<Result<T>>`.
- `Result<T>` stays a `readonly partial struct` implementing
  `IEquatable<Result<T>>`; it is never null and allocates nothing on the happy
  path.
- `ToErrorAggregate<TOut>()` stays **internal**. Aggregated errors reach the
  caller only through the terminal `Match` / `OnSuccess` / `OnFailure`, which
  build the `ResultErrorAggregate` themselves.
- `IsSuccess` does not promise a non-null `Value` — do not add a method that
  assumes it does.

`GenerateDocumentationFile` is on and warnings are errors: every new public
member needs `<summary>`, `<param>`, `<typeparam>` and `<returns>` docs.

## 4. Test it

- Unit tests in `tests/FluentResult.Tests/`, in the file mirroring the source
  file you changed — and a test for the **async** counterpart, not only the sync
  one. Partial-class splits are wired with `<Compile Update … DependentUpon>` in
  the csproj; keep that mapping when adding a file.
- Follow the suite's conventions: `[UnitTest]` on the class, a
  `private static readonly Faker Faker = new();` for random data,
  `// Arrange` / `// Act` / `// Assert` sections, behavior-named tests.
- Assert through the package under test:
  `result.Should().BeSuccessful().And.WithValue(expected);`
  `result.Should().BeFailure().And.WithError(expectedError);`

Write the failing test first, per the engineering principles. Coverage
thresholds in CI are `60 80`.

## 5. Update the documentation — all of it

The docs are packed into the NuGet package, and this repo also ships instruction
files for five assistants. A public-surface change touches:

| File | What |
|---|---|
| `docs/API.md` | the signature, its overloads, any conversion |
| `docs/EXAMPLES.md` | a usage pattern, if the method enables a new one |
| `docs/AI-GUIDELINES.md` | the rule for assistants consuming the library |
| `docs/ARCHITECTURE.md` | only when a design decision changed |
| `README.md`, `llms.txt` | only when the overview or doc map changed |
| `.github/copilot-instructions.md` | the condensed rules |
| `.cursor/rules/fluentresult.mdc` | the condensed rules |
| `.windsurf/rules/fluentresult.md` | the condensed rules |
| `.junie/guidelines.md` | the condensed rules |

Those last four are hand-maintained copies of the same facts and they **have**
drifted before: all four carried a stale `Targets: .NET 8, 9, 10` line long after
the repo moved to `net10.0` only. After editing any of them, grep the fact across
all five to confirm they agree:

```bash
grep -rn "<the fact you changed>" .github/copilot-instructions.md .cursor .windsurf .junie docs
```

Treat a doc-only correction as its own `docs:` commit, separate from behavior.

## 6. Check before committing

```bash
dotnet build                                          # must report 0 warnings
dotnet test                                           # whole suite green
dotnet format --verify-no-changes --severity error    # what CI enforces
```

Commit structural and behavioral changes separately, conventional-commit format,
and do not push. See `docs/contributing/git-instructions.md`.
