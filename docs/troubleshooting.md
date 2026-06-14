# Troubleshooting

## Docker Compose Cannot Start PostgreSQL

Another local PostgreSQL process may already be using port `5432`.

Options:

- Stop the other process.
- Change the `db` port mapping in `docker-compose.yml`.
- Run only the API/web parts against an existing PostgreSQL instance and update `ConnectionStrings__DefaultConnection`.

## API Cannot Connect To The Database

Check that PostgreSQL is running:

```bash
docker compose ps
```

Check the connection string used by the API:

```bash
echo $ConnectionStrings__DefaultConnection
```

On Windows PowerShell:

```powershell
echo $env:ConnectionStrings__DefaultConnection
```

## Web Requests Fail In The Browser

Confirm that:

- The API is reachable at `http://localhost:5076/swagger`.
- `NEXT_PUBLIC_API_BASE_URL` points to the API URL.
- `ALLOWED_ORIGINS` includes the web origin, usually `http://localhost:3000`.

After changing `NEXT_PUBLIC_API_BASE_URL`, restart the web dev server or rebuild the Docker image.

## Database State Looks Wrong

For a clean local database:

```bash
docker compose down -v
docker compose up --build
```

This removes the Docker volume used by PostgreSQL.

## Frontend Build Fails

Install dependencies from the repository root:

```bash
npm install
npm --workspace @meetsettle/web run lint
npm --workspace @meetsettle/web run build
```

## Settlement Results Look Unexpected

Check:

- The payer is included as a participant.
- `sharedByParticipantIds` contains the people who share the expense.
- Empty `sharedByParticipantIds` means the expense is shared across attending participants in the API flow.
- KRW rounds to whole won.

See [settlement-algorithm.md](settlement-algorithm.md) for the calculation rules.
