# Contributing

Thanks for considering a contribution to MeetSettle.

## Local Setup

```bash
docker compose up db
dotnet restore
dotnet test packages/settlement-core.tests/MeetSettle.SettlementCore.Tests.csproj
cd apps/web
npm install
npm run build
```

## Branches

Use short topic branches:

```bash
git checkout -b feat/invite-flow
git checkout -b fix/rounding-edge-case
```

## Issues

Please include:

- What you expected.
- What happened.
- Reproduction steps.
- Logs or screenshots when useful.

## Pull Requests

Keep PRs focused. Include tests for settlement algorithm changes, API behavior changes, and bug fixes.

## Test Commands

```bash
dotnet test packages/settlement-core.tests/MeetSettle.SettlementCore.Tests.csproj
cd apps/web
npm run build
```
