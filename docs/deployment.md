# Deployment

This project can be deployed as three parts:

- PostgreSQL database.
- ASP.NET Core API.
- Next.js web app.

One practical setup is Neon for PostgreSQL, Render for the API container, and Vercel for the web app.

## Database

Create a PostgreSQL database and copy its connection string.

For Neon, the dashboard provides a connection string from the Connect button. A direct connection string is simplest for the API and EF Core migrations. If you use a pooled connection string, keep Neon's pooling limitations in mind.

## API on Render

The API has a Dockerfile at `apps/api/Dockerfile`. When creating a Render web service:

- Language: Docker.
- Branch: `main`.
- Dockerfile path: `apps/api/Dockerfile`.
- Docker context: repository root.

Set these environment variables:

| Name | Value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `ALLOWED_ORIGINS` | Vercel web URL |
| `APPLY_MIGRATIONS` | `true` for initial setup |
| `ENABLE_SWAGGER` | `true` if you want `/swagger` available |

After the first successful deploy, you can set `APPLY_MIGRATIONS` to `false` and run migrations intentionally when schema changes are added.

## Web on Vercel

Create a Vercel project from the same GitHub repository.

Use these settings:

- Framework preset: Next.js.
- Root directory: `apps/web`.
- Build command: `npm run build`.
- Install command: `npm install`.

Set this environment variable:

| Name | Value |
| --- | --- |
| `NEXT_PUBLIC_API_BASE_URL` | Render API URL |

Redeploy the web app after changing `NEXT_PUBLIC_API_BASE_URL`.

## Checks After Deploy

Open:

- Web: Vercel URL.
- API Swagger: `https://<render-api-url>/swagger`, if Swagger is enabled.

Create a meetup from the web UI, add participants and an expense, then open the settlement page.
