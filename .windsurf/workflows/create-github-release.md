---
description: Cut a GitHub release with a changelog and watch the ci-cd run that publishes the package to NuGet
---

<!-- Generated from docs/contributing/tasks/create-github-release.md by scripts/sync-agent-files.sh. Do not edit. -->

# Create a GitHub release

> **Audience:** contributors *working on* this repository. Not packaged — see `AGENTS.md` at the repository root.

Creating a release is what publishes to nuget.org: the `ci-cd` workflow runs its
`publish` job on `release: created`, packs, and pushes to
`https://api.nuget.org/v3/index.json`. Pushing a package version is **permanent**
— nuget.org does not allow re-uploading a version, only unlisting it. Confirm
with the user before creating the release, and never create one from a red build.

## 1. Preconditions

```bash
git switch main && git pull --ff-only
git status --short          # must be empty
gh run list --limit 3       # ci-cd, codeql-analysis and linter green on HEAD
```

Stop and report if the tree is dirty, HEAD is not on `main`, or any workflow on
HEAD is failing or still running.

## 2. Determine the version

Versioning is GitVersion in ContinuousDelivery mode (`GitVersion.yml`) with
`commit-message-incrementing: Enabled`. **The version only advances when a
commit message since the last bump contains a `+semver:` trailer** — `+semver: fix`
or `+semver: patch`, `+semver: feature` or `+semver: minor`, `+semver: breaking`
or `+semver: major`. Without one the computed version stays where it is and the
release would republish an existing version number.

Read the version CI actually computed for HEAD rather than guessing:

```bash
RUN_ID=$(gh run list --workflow=ci-cd.yml --branch=main --limit=1 --json databaseId --jq '.[0].databaseId')
JOB_ID=$(gh run view "$RUN_ID" --json jobs --jq '.jobs[] | select(.name == "set-version") | .databaseId')
gh run view "$RUN_ID" --log --job="$JOB_ID" | grep -E '"(majorMinorPatch|semVer)"'
```

Compare that to what is already released and tagged:

```bash
gh release list --limit 5
git tag --sort=-v:refname | head -5
```

Tags are `vX.Y.Z` and are created automatically by the `tagging` job on pushes to
`main`, so the tag for this version usually exists already and may run ahead of
the last release. If the computed version is one that was already published to
NuGet, stop: the user needs a `+semver:` commit first.

## 3. Build the changelog

Take the range from the previously *released* tag (not merely the previous tag)
to the tag being released:

```bash
PREVIOUS=$(gh release list --limit 1 --json tagName --jq '.[0].tagName')
TARGET=v<version from step 2>
git log --no-merges --pretty='%s (%h)' "$PREVIOUS..$TARGET"
```

Group the subjects by conventional-commit type under these headings, dropping
the empty ones, and drop the `type(scope):` prefix from each line:

```markdown
### Features        <- feat
### Fixes           <- fix
### Changes         <- refactor, perf
### Documentation   <- docs
### Maintenance     <- chore, ci, build, test
```

Note any breaking change (`!` in the type, or a `+semver: breaking` trailer) in a
short paragraph at the top. End with the compare link:

```markdown
**Full changelog**: https://github.com/patmoreau/drifterapps-seeds-fluentresult/compare/<PREVIOUS>...<TARGET>
```

Show the user the assembled notes and the target tag, and get an explicit yes
before the next step.

## 4. Create the release

The tag normally already exists. Create the release against it:

```bash
gh release create "$TARGET" --title "$TARGET" --notes-file <notes-file>
```

If the tag does not exist yet, add `--target main` so GitHub creates it from the
current `main` commit. Use `--latest` only when this is the newest version — it
is the default for the highest semver, so pass `--latest=false` when cutting a
patch for an older line.

## 5. Watch the publish

Creating the release triggers a fresh `ci-cd` run whose `publish` job pushes the
package. Watch that run, not the earlier push-triggered one:

```bash
RELEASE_RUN=$(gh run list --workflow=ci-cd.yml --event=release --limit=1 --json databaseId --jq '.[0].databaseId')
gh run watch "$RELEASE_RUN" --exit-status --interval 15
gh run view "$RELEASE_RUN" --json jobs --jq '.jobs[] | "\(.name): \(.conclusion)"'
```

`publish` must report `success`. If it fails, read its log before retrying —
a failure *after* `dotnet nuget push` succeeded means the version is already on
nuget.org and must not be pushed again (the push uses `--skip-duplicate`, so a
re-run is safe but will not replace anything).

## 6. Verify on nuget.org

Indexing takes a few minutes. Both packages ship from this repo:

```bash
for pkg in drifterapps.seeds.fluentresult drifterapps.seeds.fluentresult.fluentassertions; do
  echo "$pkg: $(curl -s "https://api.nuget.org/v3-flatcontainer/$pkg/index.json" | jq -r '.versions[-1]')"
done
```

Report the released version, both package versions now on nuget.org, and the
release URL. If indexing has not caught up yet, say so rather than reporting the
old version as the result.

## Never

- Create a release when CI on HEAD is red or still running.
- Publish a version that is already on nuget.org.
- Hand-edit the version in `Directory.Build.props` or a csproj — GitVersion owns it.
