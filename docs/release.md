# Release Process

This document keeps the MeetSettle release flow reproducible for maintainers.

## Release Pre-Checklist

Before creating a tag or GitHub Release:

1. Confirm `main` is up to date with `origin/main`.
2. Confirm the working tree is clean.
3. Confirm local validation passes.
4. Confirm GitHub Actions is green on `main`.
5. Review `CHANGELOG.md`.
6. Confirm no real secrets are committed.
7. Confirm open issues and roadmap reflect real planned work.

## Local Validation

```bash
dotnet restore
dotnet build
dotnet test packages/settlement-core.tests/MeetSettle.SettlementCore.Tests.csproj
npm install
npm --workspace @meetsettle/web run lint
npm --workspace @meetsettle/web run build
npm audit
```

## v0.1.0 Tag Creation

Create the initial OSS release tag from a verified `main` branch:

```bash
git checkout main
git fetch origin
git merge --ff-only origin/main
git tag v0.1.0
git push origin v0.1.0
```

If `v0.1.0` already exists, do not recreate it. Review the existing tag and release notes instead.

## GitHub Release Creation

With GitHub CLI:

```bash
gh release create v0.1.0 --title "v0.1.0 - Initial OSS release" --notes-file CHANGELOG.md
```

If GitHub CLI is unavailable, create the release from the GitHub web UI:

1. Open `https://github.com/sbpark-escape/meetsettle/releases/new`.
2. Select tag `v0.1.0`.
3. Use title `v0.1.0 - Initial OSS release`.
4. Use the `CHANGELOG.md` v0.1.0 section as release notes.
5. Publish after confirming CI is green.

## Release Notes Method

Release notes should be factual and avoid adoption claims. For early releases, include:

- Reusable settlement engine changes.
- API and demo app changes.
- Documentation and maintainer workflow changes.
- Known limitations and planned follow-up work.

## v0.1.0 Scope

`v0.1.0` is the first public OSS foundation release. It is focused on settlement logic, API shape, demo app structure, tests, CI, Docker Compose, and maintainer documentation.
