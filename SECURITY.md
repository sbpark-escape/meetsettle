# Security Policy

MeetSettle is an early-stage example for meetup expense tracking. Do not use it for sensitive payment flows without your own security checks.

## Supported Versions

Security reports are accepted for the current `main` branch.

## Reporting a Vulnerability

Open a private security advisory if the repository is hosted on GitHub, or contact the project owner directly once a private contact method is available.

## Sensitive Data Rules

- Do not commit real secrets.
- Do not hardcode production database URLs.
- Do not log invite tokens, JWTs, passwords, or API keys.
- Treat invite tokens as bearer credentials.

## Invite Token Notes

Invite tokens should be long, random, expiring values. The MVP uses random URL-safe tokens and stores expiration timestamps. Production deployments should add rate limiting and monitoring.
