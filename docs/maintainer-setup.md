# Maintainer Setup

This document keeps GitHub project management setup reproducible.

## Repository About

Recommended GitHub repository metadata:

- Description: `Open-source meetup attendance and settlement toolkit`
- Website: leave blank until a public demo URL exists.
- Topics: `open-source`, `settlement`, `expense-sharing`, `meetup`, `split-bill`, `nextjs`, `dotnet`, `postgresql`, `pwa`, `typescript`

## Labels

The repository can start with GitHub default labels:

- `good first issue`
- `help wanted`
- `bug`
- `enhancement`
- `documentation`

Optional labels for later maintenance:

- `security`
- `roadmap`
- `maintenance`

With GitHub CLI:

```bash
gh label create "good first issue" --color 7057ff --description "Good for newcomers"
gh label create "help wanted" --color 008672 --description "Maintainer welcomes help"
gh label create bug --color d73a4a --description "Something is not working"
gh label create enhancement --color a2eeef --description "New feature or improvement"
gh label create documentation --color 0075ca --description "Documentation work"
gh label create security --color b60205 --description "Security-related work"
gh label create roadmap --color fbca04 --description "Roadmap planning"
gh label create maintenance --color cfd3d7 --description "Maintenance and cleanup"
```

## Milestones

Recommended milestones:

```bash
gh milestone create "v0.1.0" --description "Initial OSS release"
gh milestone create "v0.2.0" --description "Invite and transfer tracking improvements"
gh milestone create "v0.3.0" --description "PWA install guide and calendar export"
```

## Initial Issues

Recommended first issues:

1. Improve settlement rounding policy documentation.
2. Add invite token expiration policy.
3. Add API integration tests.
4. Add demo deployment guide.
5. Add PWA install guide.
6. Add localization plan for Korean and English.
7. Add README screenshots.
8. Improve API error response examples.

Create only issues that represent real planned work.

If GitHub CLI is available, these commands create the recommended initial issues:

```bash
gh issue create --title "Improve settlement rounding policy documentation" --label documentation --label "good first issue" --body "Document additional KRW rounding examples and edge cases so contributors can understand how deterministic remainder handling works. Roadmap connection: settlement-core documentation quality."
gh issue create --title "Add invite token expiration policy" --label enhancement --body "Define and implement a clear expiration policy for invite tokens, including default duration, rotation behavior, and API error handling. Roadmap connection: invite-token hardening."
gh issue create --title "Add API integration tests" --label enhancement --body "Add integration tests for meetup creation, participant attendance, expense recording, and settlement retrieval. Roadmap connection: API reliability before demo deployment."
gh issue create --title "Add demo deployment guide" --label documentation --body "Document how to deploy the API and web demo with safe placeholder configuration and no committed secrets. Roadmap connection: public demo preparation."
gh issue create --title "Add PWA install guide" --label documentation --label "good first issue" --body "Add a short guide explaining expected PWA install behavior for the demo app and planned offline-friendly scope. Roadmap connection: mobile-first meetup usage."
gh issue create --title "Add Korean and English localization plan" --label enhancement --body "Plan the Korean and English localization approach for the demo app, including routing, copy ownership, and fallback behavior. Roadmap connection: contributor-friendly internationalization."
gh issue create --title "Add README screenshots" --label documentation --label "good first issue" --body "Add screenshots after the first public demo deployment so the README shows real product state instead of placeholder imagery. Roadmap connection: demo release readiness."
gh issue create --title "Improve API error response examples" --label documentation --label "good first issue" --body "Add concrete ProblemDetails-style error response examples to the API docs for validation errors, missing resources, and invalid invite tokens. Roadmap connection: API documentation quality."
```
