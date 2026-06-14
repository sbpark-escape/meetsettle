// 파일 용도: 모임 비용을 참가자별 부담액과 최소 송금 목록으로 계산한다.
// 파일 목적: 서비스와 무관하게 재사용할 수 있는 정산 규칙을 제공한다.
namespace MeetSettle.SettlementCore;

public sealed class SettlementCalculator
{
    public SettlementResult Calculate(
        IReadOnlyCollection<SettlementParticipant> participants,
        IReadOnlyCollection<SettlementExpense> expenses,
        SettlementOptions? options = null)
    {
        options ??= new SettlementOptions();
        ValidateParticipants(participants);

        var participantMap = participants.ToDictionary(participant => participant.Id);
        var paidAmounts = participants.ToDictionary(participant => participant.Id, _ => 0m);
        var owedAmounts = participants.ToDictionary(participant => participant.Id, _ => 0m);

        foreach (var expense in expenses)
        {
            ValidateExpense(expense, participantMap);

            if (expense.Amount == 0)
            {
                continue;
            }

            paidAmounts[expense.PayerId] += Round(expense.Amount, options);

            foreach (var share in SplitExpense(expense, participantMap, options))
            {
                owedAmounts[share.ParticipantId] += share.Amount;
            }
        }

        var balances = participants
            .OrderBy(participant => participant.Name)
            .ThenBy(participant => participant.Id)
            .Select(participant =>
            {
                var paidAmount = Round(paidAmounts[participant.Id], options);
                var owedAmount = Round(owedAmounts[participant.Id], options);

                return new ParticipantBalance(
                    participant.Id,
                    participant.Name,
                    paidAmount,
                    owedAmount,
                    Round(paidAmount - owedAmount, options));
            })
            .ToList();

        return new SettlementResult(
            options.Currency,
            balances,
            BuildTransfers(balances, options));
    }

    private static void ValidateParticipants(IReadOnlyCollection<SettlementParticipant> participants)
    {
        if (participants.Count == 0)
        {
            throw new ArgumentException("At least one participant is required.", nameof(participants));
        }

        if (participants.Any(participant => string.IsNullOrWhiteSpace(participant.Id)))
        {
            throw new ArgumentException("Participant id is required.", nameof(participants));
        }

        var duplicateId = participants
            .GroupBy(participant => participant.Id)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateId is not null)
        {
            throw new ArgumentException($"Duplicate participant id: {duplicateId.Key}", nameof(participants));
        }
    }

    private static void ValidateExpense(
        SettlementExpense expense,
        IReadOnlyDictionary<string, SettlementParticipant> participantMap)
    {
        if (string.IsNullOrWhiteSpace(expense.Id))
        {
            throw new ArgumentException("Expense id is required.", nameof(expense));
        }

        if (!participantMap.ContainsKey(expense.PayerId))
        {
            throw new ArgumentException($"Expense payer does not exist: {expense.PayerId}", nameof(expense));
        }

        if (expense.Amount < 0)
        {
            throw new ArgumentException("Expense amount cannot be negative.", nameof(expense));
        }

        foreach (var participantId in expense.SharedByParticipantIds)
        {
            if (!participantMap.ContainsKey(participantId))
            {
                throw new ArgumentException($"Shared participant does not exist: {participantId}", nameof(expense));
            }
        }
    }

    private static IReadOnlyList<ExpenseShareAmount> SplitExpense(
        SettlementExpense expense,
        IReadOnlyDictionary<string, SettlementParticipant> participantMap,
        SettlementOptions options)
    {
        var excludedIds = expense.ExcludedParticipantIds?.ToHashSet(StringComparer.Ordinal) ?? [];
        var sharedParticipantIds = expense.SharedByParticipantIds.Count == 0
            ? participantMap.Keys
            : expense.SharedByParticipantIds;

        var participantIds = sharedParticipantIds
            .Where(participantId => !excludedIds.Contains(participantId))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(participantId => participantMap[participantId].Name)
            .ThenBy(participantId => participantId)
            .ToList();

        if (participantIds.Count == 0)
        {
            throw new ArgumentException("Expense must have at least one participant sharing it.", nameof(expense));
        }

        var weightedShares = participantIds
            .Select(participantId => new
            {
                ParticipantId = participantId,
                Weight = ResolveWeight(expense, participantId)
            })
            .ToList();

        var totalWeight = weightedShares.Sum(share => share.Weight);
        if (totalWeight <= 0)
        {
            throw new ArgumentException("Total share weight must be greater than zero.", nameof(expense));
        }

        var amount = Round(expense.Amount, options);
        var factor = (decimal)Math.Pow(10, options.MinorUnitDigits);
        var allocated = weightedShares
            .Select(share =>
            {
                var exactAmount = amount * share.Weight / totalWeight;
                var roundedDown = Math.Floor(exactAmount * factor) / factor;

                return new ExpenseShareAmount(share.ParticipantId, roundedDown);
            })
            .ToList();

        var allocatedTotal = allocated.Sum(share => share.Amount);
        var unitAmount = 1m / factor;
        var remainderUnits = (int)Math.Round((amount - allocatedTotal) * factor, 0, MidpointRounding.AwayFromZero);

        for (var index = 0; index < remainderUnits; index++)
        {
            var targetIndex = index % allocated.Count;
            allocated[targetIndex] = allocated[targetIndex] with
            {
                Amount = allocated[targetIndex].Amount + unitAmount
            };
        }

        return allocated;
    }

    private static decimal ResolveWeight(SettlementExpense expense, string participantId)
    {
        if (expense.Weights is null || !expense.Weights.TryGetValue(participantId, out var weight))
        {
            return 1m;
        }

        if (weight <= 0)
        {
            throw new ArgumentException("Participant weight must be greater than zero.", nameof(expense));
        }

        return weight;
    }

    private static IReadOnlyList<SettlementTransfer> BuildTransfers(
        IReadOnlyCollection<ParticipantBalance> balances,
        SettlementOptions options)
    {
        var debtors = balances
            .Where(balance => balance.NetAmount < 0)
            .OrderBy(balance => balance.ParticipantName)
            .Select(balance => new RunningBalance(balance.ParticipantId, balance.ParticipantName, -balance.NetAmount))
            .ToList();

        var creditors = balances
            .Where(balance => balance.NetAmount > 0)
            .OrderBy(balance => balance.ParticipantName)
            .Select(balance => new RunningBalance(balance.ParticipantId, balance.ParticipantName, balance.NetAmount))
            .ToList();

        var transfers = new List<SettlementTransfer>();
        var debtorIndex = 0;
        var creditorIndex = 0;

        while (debtorIndex < debtors.Count && creditorIndex < creditors.Count)
        {
            var debtor = debtors[debtorIndex];
            var creditor = creditors[creditorIndex];
            var transferAmount = Round(Math.Min(debtor.Amount, creditor.Amount), options);

            if (transferAmount > 0)
            {
                transfers.Add(new SettlementTransfer(
                    debtor.ParticipantId,
                    debtor.ParticipantName,
                    creditor.ParticipantId,
                    creditor.ParticipantName,
                    transferAmount));
            }

            debtors[debtorIndex] = debtor with { Amount = Round(debtor.Amount - transferAmount, options) };
            creditors[creditorIndex] = creditor with { Amount = Round(creditor.Amount - transferAmount, options) };

            if (debtors[debtorIndex].Amount == 0)
            {
                debtorIndex++;
            }

            if (creditors[creditorIndex].Amount == 0)
            {
                creditorIndex++;
            }
        }

        return transfers;
    }

    private static decimal Round(decimal amount, SettlementOptions options)
    {
        return Math.Round(amount, options.MinorUnitDigits, MidpointRounding.AwayFromZero);
    }

    private sealed record ExpenseShareAmount(string ParticipantId, decimal Amount);

    private sealed record RunningBalance(string ParticipantId, string ParticipantName, decimal Amount);
}
