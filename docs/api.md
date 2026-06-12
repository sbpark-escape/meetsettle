# API

The API is served by `apps/api`.

## Create Meetup

`POST /api/meetups`

```json
{
  "name": "Friday dinner",
  "date": "2026-06-11",
  "location": "Seoul",
  "currency": "KRW"
}
```

## Get Meetup

`GET /api/meetups/{id}`

Returns meetup details, participants, expenses, and expense shares.

## Add Participant

`POST /api/meetups/{id}/participants`

```json
{
  "name": "Bora",
  "isAttending": true
}
```

## Update Participant

`PATCH /api/meetups/{id}/participants/{participantId}`

```json
{
  "isAttending": true
}
```

## Add Expense

`POST /api/meetups/{id}/expenses`

```json
{
  "title": "Dinner",
  "payerParticipantId": "00000000-0000-0000-0000-000000000000",
  "amount": 60000,
  "sharedByParticipantIds": [
    "00000000-0000-0000-0000-000000000000"
  ]
}
```

## Get Settlement

`GET /api/meetups/{id}/settlement`

Returns participant balances and transfer recommendations.

## Mark Transfer Complete

`PATCH /api/meetups/{id}/transfers/{transferId}`

```json
{
  "isCompleted": true
}
```

## Create Invite Token

`POST /api/meetups/{id}/invite-token`

## Resolve Invite

`GET /api/invites/{token}`
