# MeetSettle

[![CI](https://github.com/sbpark-escape/meetsettle/actions/workflows/ci.yml/badge.svg)](https://github.com/sbpark-escape/meetsettle/actions/workflows/ci.yml)

MeetSettle is an open-source toolkit for building meetup attendance and settlement flows. It includes a reusable settlement engine, invite-token based meetup pages, a C# Web API, and a Next.js demo app.

## Why This Project Exists

Small teams and independent developers often need lightweight group-planning features: invite people, collect attendance, record shared costs, and show who should send money to whom. MeetSettle packages that flow as a practical OSS template instead of a one-off product feature.

## Who This Is For

- Developers adding attendance and payment-splitting flows to a small product.
- Indie builders who want a working reference app rather than a blank starter.
- Maintainers who need a simple, testable settlement algorithm for group expenses.
- Contributors who want a focused early-stage OSS project with clear next steps.

## Project Status

MeetSettle is an early-stage OSS project. The reusable `settlement-core` package is the core value of the repository; the ASP.NET Core API and Next.js web app are reference/demo implementations that show how the engine can be used in a real meetup flow.

The current v0.1.0 focus is a reliable settlement foundation, clear API structure, local development setup, tests, and contributor-friendly documentation. Production payment handling, full authentication, push notifications, and hosted demo deployment are planned or intentionally out of scope for this initial release.

## Maintainer Focus

- Keep the settlement engine small, deterministic, and easy to reuse.
- Keep API and web examples practical without pretending they are production SaaS.
- Add issues, releases, and documentation only when they represent real planned maintenance work.
- Grow the project through reviewable changes, tests, and clear roadmap items.

## Implemented Features

- Reusable C# settlement engine with unit tests.
- KRW whole-won rounding policy with deterministic remainder handling.
- Participant, expense, share, balance, and transfer models.
- ASP.NET Core Web API with PostgreSQL, EF Core commands, and Dapper queries.
- Next.js App Router frontend with mobile-first meetup and settlement screens.
- Invite-token based guest access for MVP flows.
- Docker Compose setup for local PostgreSQL, API, and web app.
- GitHub Actions CI for frontend build and backend tests.

## Planned Features

- Hosted public demo deployment.
- Invite token expiration and rotation policy.
- API integration tests around meetup, expense, and settlement flows.
- PWA installation guide and offline-friendly demo notes.
- Korean and English localization plan.
- Calendar export and notification integrations.

## Screenshots

Screenshots will be added after the first public demo deployment. The current focus is the v0.1.0 OSS foundation: settlement engine, API structure, demo app structure, tests, and documentation.

## Tech Stack

- Frontend: Next.js App Router, TypeScript, Tailwind CSS.
- Backend: C# ASP.NET Core Web API, Entity Framework Core, Dapper.
- Database: PostgreSQL.
- Tooling: Docker Compose, GitHub Actions.

## Architecture Overview

```text
apps/
  web/                   Next.js frontend
  api/                   ASP.NET Core Web API
packages/
  settlement-core/       Reusable settlement algorithm
  settlement-core.tests/ Unit tests
docs/
  architecture.md
  api.md
  settlement-algorithm.md
  deployment.md
  roadmap.md
```

The API follows a CQRS-lite style: GET endpoints use Dapper query DTOs, and create/update operations use EF Core entities and Fluent API mappings.

## Quick Start

```bash
git clone https://github.com/sbpark-escape/meetsettle.git
cd meetsettle
docker compose up --build
```

Then open:

- Web: `http://localhost:3000`
- API Swagger: `http://localhost:5076/swagger`
- PostgreSQL: `localhost:5432`

## Local Development

Run the database:

```bash
docker compose up db
```

Run the API:

```bash
dotnet restore
dotnet ef database update --project apps/api/MeetSettle.Api.csproj --startup-project apps/api/MeetSettle.Api.csproj
dotnet run --project apps/api/MeetSettle.Api.csproj
```

Run the web app:

```bash
npm install
npm --workspace @meetsettle/web run dev
```

## Docker Compose Setup

`docker-compose.yml` starts PostgreSQL, the API, and the web app. It uses development-only defaults from `.env.example`; replace them in real deployments.

```bash
docker compose up --build
docker compose down
```

## Environment Variables

Copy `.env.example` into your local environment and adjust values as needed.

- `POSTGRES_DB`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `ConnectionStrings__DefaultConnection`
- `NEXT_PUBLIC_API_BASE_URL`
- `ALLOWED_ORIGINS`

Do not commit real production secrets.

## API Overview

- `POST /api/meetups`
- `GET /api/meetups/{id}`
- `POST /api/meetups/{id}/participants`
- `PATCH /api/meetups/{id}/participants/{participantId}`
- `POST /api/meetups/{id}/expenses`
- `GET /api/meetups/{id}/settlement`
- `PATCH /api/meetups/{id}/transfers/{transferId}`
- `POST /api/meetups/{id}/invite-token`
- `GET /api/invites/{token}`

See [docs/api.md](docs/api.md) for request and response notes.

## Settlement Algorithm

The settlement engine calculates:

- How much each participant paid.
- How much each participant owes.
- Net balance per participant.
- A minimal list of transfers from debtors to creditors.

KRW uses whole-won rounding by default. If an amount does not divide evenly, the remaining won is distributed deterministically by participant order so totals always reconcile.

See [docs/settlement-algorithm.md](docs/settlement-algorithm.md) for details.

## Example Settlement

If Alex pays `60,000 KRW` for Alex, Bora, and Chris:

- Bora sends Alex `20,000 KRW`.
- Chris sends Alex `20,000 KRW`.

If multiple people paid, MeetSettle nets all balances first and then creates the fewest practical transfers.

## Testing

```bash
dotnet build
dotnet test packages/settlement-core.tests/MeetSettle.SettlementCore.Tests.csproj
npm --workspace @meetsettle/web run lint
npm --workspace @meetsettle/web run build
npm audit
```

## CI Status

GitHub Actions runs on pull requests and pushes to `main`.

- Backend: restore, build, settlement-core tests.
- Frontend: npm install, lint/type check, production build.

## Release Process

The initial release target is `v0.1.0`. See [docs/release.md](docs/release.md) for the release checklist, tag command, GitHub Release command, and fallback steps when GitHub CLI is unavailable.

## Roadmap

See [docs/roadmap.md](docs/roadmap.md).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md), [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md), and the issue templates in `.github/ISSUE_TEMPLATE`.

## Security

See [SECURITY.md](SECURITY.md). Do not commit real secrets, production database URLs, JWT keys, or third-party API keys.

## License

MIT. See [LICENSE](LICENSE).
