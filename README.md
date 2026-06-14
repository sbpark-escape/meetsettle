# MeetSettle

[![CI](https://github.com/sbpark-escape/meetsettle/actions/workflows/ci.yml/badge.svg)](https://github.com/sbpark-escape/meetsettle/actions/workflows/ci.yml)

MeetSettle is a small meetup expense-splitting project with a C# settlement engine, an ASP.NET Core API, and a Next.js example web app.

## What this project does

- Creates meetup records with a date, location, and currency.
- Tracks participants and attendance.
- Records shared expenses and the participant who paid each one.
- Calculates balances and suggested transfers.
- Supports invite-token based meetup access for the example flow.
- Runs locally with PostgreSQL through Docker Compose.

## Who this may be useful for

- Developers building a small meetup or group-expense flow.
- People who want a readable C# settlement calculation example.
- Teams comparing an ASP.NET Core API with a Next.js frontend in a simple monorepo.

## Requirements

- .NET SDK 8.
- Node.js 22 and npm.
- Docker with Compose for the local PostgreSQL setup.

The repository includes `global.json` for .NET SDK selection. The web workspace is under `apps/web`.

## Getting started

Run the full local stack:

```bash
git clone https://github.com/sbpark-escape/meetsettle.git
cd meetsettle
docker compose up --build
```

Then open:

- Web: `http://localhost:3000`
- API Swagger UI: `http://localhost:5076/swagger`
- PostgreSQL: `localhost:5432`

To run the API and web app separately:

```bash
docker compose up -d db
dotnet restore
dotnet run --project apps/api/MeetSettle.Api.csproj
```

In another terminal:

```bash
npm install
npm run web:dev
```

Useful checks:

```bash
dotnet build
dotnet test packages/settlement-core.tests/MeetSettle.SettlementCore.Tests.csproj
npm --workspace @meetsettle/web run lint
npm --workspace @meetsettle/web run build
```

## Configuration

Local defaults are shown in `.env.example`.

| Name | Used by | Notes |
| --- | --- | --- |
| `POSTGRES_DB` | Docker Compose | Local database name. |
| `POSTGRES_USER` | Docker Compose | Local database role. |
| `POSTGRES_PASSWORD` | Docker Compose | Local database password. |
| `ConnectionStrings__DefaultConnection` | API | PostgreSQL connection string. |
| `NEXT_PUBLIC_API_BASE_URL` | Web | Browser-visible API base URL. |
| `ALLOWED_ORIGINS` | API | Comma-separated CORS origins. |
| `APPLY_MIGRATIONS` | API | Set to `true` to apply EF Core migrations at startup. |
| `ENABLE_SWAGGER` | API | Set to `true` to expose Swagger UI outside local development. |

See [docs/configuration.md](docs/configuration.md) for more detail.

## Example usage

Check the API process:

```bash
curl http://localhost:5076/health
```

The health endpoint returns `200 OK` with `{"status":"ok"}` and does not query PostgreSQL.

Create a meetup:

```bash
curl --json '{"name":"Friday dinner","date":"2026-06-20","location":"Seoul","currency":"KRW"}' \
  http://localhost:5076/api/meetups
```

Add participants:

```bash
curl --json '{"name":"Alex","isAttending":true}' \
  http://localhost:5076/api/meetups/{meetupId}/participants

curl --json '{"name":"Bora","isAttending":true}' \
  http://localhost:5076/api/meetups/{meetupId}/participants
```

Add an expense:

```bash
curl --json '{"title":"Dinner","payerParticipantId":"{alexId}","amount":60000,"sharedByParticipantIds":["{alexId}","{boraId}"]}' \
  http://localhost:5076/api/meetups/{meetupId}/expenses
```

Read the settlement:

```bash
curl http://localhost:5076/api/meetups/{meetupId}/settlement
```

For settlement rules, see [docs/settlement-algorithm.md](docs/settlement-algorithm.md).

## Notes / limitations

- This is an early version and is still example-focused.
- The API and web app do not include full authentication.
- There is no payment provider integration.
- Invite tokens should be treated like bearer credentials.
- Automated tests currently focus on `settlement-core`.

Troubleshooting notes are in [docs/troubleshooting.md](docs/troubleshooting.md).

Deployment notes are in [docs/deployment.md](docs/deployment.md).

## Contributing

Small, focused changes are easiest to discuss. Please include tests for settlement calculation changes and describe setup or configuration changes in the related docs.

See [CONTRIBUTING.md](CONTRIBUTING.md) for local setup and pull request notes.

## License

MIT. See [LICENSE](LICENSE).
