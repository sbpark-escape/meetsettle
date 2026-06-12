# Deployment

MeetSettle is designed so the demo app can be deployed on free or low-cost services. These notes are guidance, not a production guarantee.

## Web: Vercel

Suggested setup:

- Project root: repository root.
- Framework preset: Next.js.
- Build command: `npm --workspace @meetsettle/web run build`
- Output: managed by Next.js.
- Environment variable: `NEXT_PUBLIC_API_BASE_URL`

## API: Render

Suggested setup:

- Runtime: Docker.
- Dockerfile: `apps/api/Dockerfile`
- Health check path: add a lightweight health endpoint before production use.
- Environment variable: `ConnectionStrings__DefaultConnection`
- Environment variable: `ALLOWED_ORIGINS`

## Database: Neon PostgreSQL

Suggested setup:

- Create a PostgreSQL database.
- Copy the pooled connection string into `ConnectionStrings__DefaultConnection`.
- Run EF Core migrations before the first API deployment.

```bash
dotnet ef database update --project apps/api/MeetSettle.Api.csproj --startup-project apps/api/MeetSettle.Api.csproj
```

## Required Environment Variables

- `ConnectionStrings__DefaultConnection`
- `ALLOWED_ORIGINS`
- `NEXT_PUBLIC_API_BASE_URL`
- `POSTGRES_DB`, `POSTGRES_USER`, and `POSTGRES_PASSWORD` for Docker Compose only.

## Free-Tier Limitations

- Render free services may sleep after inactivity.
- Neon free databases may have connection and storage limits.
- Vercel frontend deployments need a reachable API URL.

## Known Limitations

- No production authentication yet.
- No payment-provider integration.
- No SMS or push notification integration yet.
- Invite tokens are bearer credentials and should be protected.
- Rate limiting should be added before public production use.
