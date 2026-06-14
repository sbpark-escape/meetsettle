# Getting Started

This guide starts the local database, API, and web app.

## Run with Docker Compose

```bash
docker compose up --build
```

Open:

- Web: `http://localhost:3000`
- API Swagger UI: `http://localhost:5076/swagger`
- PostgreSQL: `localhost:5432`

Stop the stack:

```bash
docker compose down
```

Remove the local database volume when you want a clean database:

```bash
docker compose down -v
```

## Run Services Separately

Start PostgreSQL:

```bash
docker compose up -d db
```

Run the API:

```bash
dotnet restore
dotnet run --project apps/api/MeetSettle.Api.csproj
```

Run the web app in another terminal:

```bash
npm install
npm run web:dev
```

## Run Checks

```bash
dotnet build
dotnet test packages/settlement-core.tests/MeetSettle.SettlementCore.Tests.csproj
npm --workspace @meetsettle/web run lint
npm --workspace @meetsettle/web run build
```

The API applies EF Core migrations automatically while running in the development environment.
