# Settlement Algorithm

The settlement engine lives in `packages/settlement-core`.

## Inputs

The calculator accepts:

- `participants`: people who can pay, owe, or receive money.
- `expenses`: shared costs with a payer, amount, title, and sharing rules.
- `currency`: `KRW` by default.
- `MinorUnitDigits`: `0` for KRW whole-won settlement, `2` for currencies such as USD.

Each expense includes:

- `PayerId`: the participant who paid.
- `Amount`: the paid amount.
- `SharedByParticipantIds`: participants who share the expense. If empty, all participants share it.
- `ExcludedParticipantIds`: optional participants to exclude from that expense.
- `Weights`: optional per-participant weights. Missing weights default to `1`.

## Outputs

The calculator returns:

- `Balances`: paid amount, owed amount, and net amount for each participant.
- `Transfers`: recommended payments from debtors to creditors.

## Rounding Policy

KRW settlement uses whole-won amounts by default.

When an amount does not divide evenly:

1. Calculate each exact share.
2. Round each share down to the minor unit.
3. Distribute the remaining minor units in deterministic participant order.

Example: `100 KRW` split by Alex, Bora, and Chris becomes `34`, `33`, and `33`.

This keeps totals reconciled while making test results stable.

## Uneven Multi-Payer Example

Alex pays `10,001 KRW` for Alex, Bora, and Chris. Bora also pays `5,000 KRW` for Bora and Chris.

The first expense becomes shares of `3,334`, `3,334`, and `3,333`. The second expense becomes shares of `2,500` and `2,500`.

After netting both expenses:

- Bora owes Alex `834 KRW`.
- Chris owes Alex `5,833 KRW`.

## Transfer Calculation

After all expenses are processed:

1. Participants with negative net amounts become debtors.
2. Participants with positive net amounts become creditors.
3. The calculator walks both lists and creates transfers for the smaller remaining amount.
4. Fully settled participants are removed from the running list.

This produces a practical transfer list based on net balances.

## Validation

The calculator rejects:

- Empty participant lists.
- Duplicate participant IDs.
- Missing participant IDs.
- Expenses with missing IDs.
- Payers that are not participants.
- Negative amounts.
- Share participants that are not participants.
- Non-positive weights.
- Expense rules that leave no participants to share the cost.

## Simple Example

Alex pays `60,000 KRW` for Alex, Bora, and Chris.

Each person owes `20,000 KRW`.

Alex already paid, so the transfers are:

- Bora to Alex: `20,000 KRW`
- Chris to Alex: `20,000 KRW`
