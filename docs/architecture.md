# Architecture

MeetSettle is a small monorepo with three main parts.

## apps/web

`apps/web` is the Next.js App Router frontend. It provides the mobile-first meetup creation, invite, detail, and settlement views. It talks to the API through `NEXT_PUBLIC_API_BASE_URL`.

## apps/api

`apps/api` is the ASP.NET Core Web API. It follows a CQRS-lite pattern:

- GET endpoints use Dapper and response DTOs.
- POST/PATCH endpoints use EF Core entities and `AppDbContext`.
- Entity mapping is defined in `Data/Configurations`.

## packages/settlement-core

`packages/settlement-core` is the reusable settlement algorithm. It has no dependency on ASP.NET Core, PostgreSQL, or the web app.

## Invite Token Flow

1. A meetup owner creates an invite token through `POST /api/meetups/{id}/invite-token`.
2. The API stores a random URL-safe token with an expiration timestamp.
3. Guests open `/invites/{token}` in the web app.
4. The web app can resolve token metadata through `GET /api/invites/{token}`.

## Settlement Calculation Flow

1. Participants and expenses are stored by EF Core commands.
2. Expense shares define who splits each expense.
3. The API passes participants and expenses into `SettlementCalculator`.
4. The calculator returns balances and minimal transfers.
5. Command paths rebuild persisted transfer rows so completion status can be tracked.

## Database Overview

- `meetups`
- `participants`
- `expenses`
- `expense_shares`
- `settlement_transfers`
- `invite_tokens`
