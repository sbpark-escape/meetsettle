# Contributing

Thanks for considering a contribution to MeetSettle.

## Local Setup

```bash
docker compose up -d db
dotnet restore
npm install
```

Run the API:

```bash
dotnet run --project apps/api/MeetSettle.Api.csproj
```

Run the web app:

```bash
npm run web:dev
```

## Checks

```bash
dotnet build
dotnet test packages/settlement-core.tests/MeetSettle.SettlementCore.Tests.csproj
npm --workspace @meetsettle/web run lint
npm --workspace @meetsettle/web run build
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

Keep pull requests focused. Include tests for settlement algorithm changes, API behavior changes, and bug fixes.
