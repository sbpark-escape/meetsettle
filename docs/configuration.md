# Configuration

Local defaults live in `.env.example`. Docker Compose also provides fallback values so the stack can start without copying the file first.

## Environment Variables

| Name | Used by | Default | Notes |
| --- | --- | --- | --- |
| `POSTGRES_DB` | Docker Compose | `meetsettle` | Local database name. |
| `POSTGRES_USER` | Docker Compose | `meetsettle` | Local database role. |
| `POSTGRES_PASSWORD` | Docker Compose | `meetsettle_dev_password` | Local database password. |
| `ConnectionStrings__DefaultConnection` | API | See `.env.example` | PostgreSQL connection string. |
| `NEXT_PUBLIC_API_BASE_URL` | Web | `http://localhost:5076` | API base URL used by browser requests. |
| `ALLOWED_ORIGINS` | API | `http://localhost:3000` | Comma-separated CORS origin list. |
| `APPLY_MIGRATIONS` | API | Development only | Set to `true` to apply EF Core migrations at startup. |
| `ENABLE_SWAGGER` | API | Development only | Set to `true` to expose Swagger UI outside local development. |

## Local Database

When using Docker Compose, PostgreSQL is exposed on `localhost:5432`.

The API uses EF Core migrations. In development, migrations run automatically during API startup. To run them manually:

```bash
dotnet ef database update --project apps/api/MeetSettle.Api.csproj --startup-project apps/api/MeetSettle.Api.csproj
```

## Web API URL

The web app reads `NEXT_PUBLIC_API_BASE_URL` at build time. When using Docker Compose, this is set to `http://localhost:5076`.

If the API runs on another port, update `NEXT_PUBLIC_API_BASE_URL` before starting or rebuilding the web app.

## Secrets

Do not commit real database URLs, passwords, invite tokens, JWT keys, or third-party API keys.
