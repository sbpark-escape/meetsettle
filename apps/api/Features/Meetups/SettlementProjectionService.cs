// 파일 용도: Command 이후 정산 송금 목록을 재계산해 저장한다.
// 파일 목적: GET 요청은 부수효과 없이 읽기만 하고, 상태 갱신은 EF Core Command 경로에서 끝내도록 한다.
using MeetSettle.Api.Data;
using MeetSettle.Api.Models;
using MeetSettle.SettlementCore;
using Microsoft.EntityFrameworkCore;
using ApiSettlementTransfer = MeetSettle.Api.Models.SettlementTransfer;

namespace MeetSettle.Api.Features.Meetups;

public sealed class SettlementProjectionService(
    AppDbContext dbContext,
    SettlementCalculator settlementCalculator)
{
    public async Task RebuildTransfersAsync(Guid meetupId, CancellationToken cancellationToken)
    {
        var meetup = await dbContext.Meetups
            .Include(item => item.Participants)
            .Include(item => item.Expenses)
            .ThenInclude(expense => expense.Shares)
            .FirstOrDefaultAsync(item => item.Id == meetupId, cancellationToken);

        if (meetup is null)
        {
            return;
        }

        var participants = meetup.Participants
            .OrderBy(participant => participant.Name)
            .Select(participant => new SettlementParticipant(participant.Id.ToString(), participant.Name))
            .ToList();

        if (participants.Count == 0)
        {
            return;
        }

        var expenses = meetup.Expenses
            .OrderBy(expense => expense.CreatedAt)
            .Select(expense => new SettlementExpense(
                expense.Id.ToString(),
                expense.Title,
                expense.PayerParticipantId.ToString(),
                expense.Amount,
                expense.Shares.Select(share => share.ParticipantId.ToString()).ToList(),
                expense.Shares.ToDictionary(share => share.ParticipantId.ToString(), share => share.Weight)))
            .ToList();

        var result = settlementCalculator.Calculate(
            participants,
            expenses,
            new SettlementOptions(meetup.Currency, meetup.Currency == "KRW" ? 0 : 2));

        var existingTransfers = await dbContext.SettlementTransfers
            .Where(transfer => transfer.MeetupId == meetupId)
            .ToListAsync(cancellationToken);

        var completedTransferKeys = existingTransfers
            .Where(transfer => transfer.IsCompleted)
            .Select(MakeTransferKey)
            .ToHashSet(StringComparer.Ordinal);

        dbContext.SettlementTransfers.RemoveRange(existingTransfers);

        var now = DateTimeOffset.UtcNow;
        foreach (var transfer in result.Transfers)
        {
            var transferEntity = new ApiSettlementTransfer
            {
                MeetupId = meetupId,
                FromParticipantId = Guid.Parse(transfer.FromParticipantId),
                ToParticipantId = Guid.Parse(transfer.ToParticipantId),
                Amount = transfer.Amount,
                IsCompleted = completedTransferKeys.Contains(MakeTransferKey(
                    Guid.Parse(transfer.FromParticipantId),
                    Guid.Parse(transfer.ToParticipantId),
                    transfer.Amount)),
                CreatedAt = now,
                UpdatedAt = now
            };

            dbContext.SettlementTransfers.Add(transferEntity);
        }
    }

    private static string MakeTransferKey(ApiSettlementTransfer transfer)
    {
        return MakeTransferKey(transfer.FromParticipantId, transfer.ToParticipantId, transfer.Amount);
    }

    private static string MakeTransferKey(Guid fromParticipantId, Guid toParticipantId, decimal amount)
    {
        return $"{fromParticipantId:N}:{toParticipantId:N}:{amount:0.##}";
    }
}
