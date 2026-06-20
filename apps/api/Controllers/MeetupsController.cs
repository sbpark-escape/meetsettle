// 파일 용도: 모임 생성, 참가자/비용 입력, 정산 조회 API를 제공한다.
// 파일 목적: Command는 EF Core, Query는 Dapper로 분리한 CQRS lite 예제를 보여준다.
using System.Security.Cryptography;
using Dapper;
using MeetSettle.Api.Data;
using MeetSettle.Api.Features.Meetups;
using MeetSettle.Api.Infrastructure;
using MeetSettle.Api.Models;
using MeetSettle.SettlementCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace MeetSettle.Api.Controllers;

[ApiController]
[Route("api/meetups")]
public sealed class MeetupsController(
    AppDbContext dbContext,
    IDbConnectionFactory connectionFactory,
    SettlementCalculator settlementCalculator,
    SettlementProjectionService settlementProjectionService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MeetupSummaryResponse>> CreateMeetupAsync(
        CreateMeetupRequest request,
        CancellationToken cancellationToken)
    {
        // 공개 API의 첫 진입점이므로 필수값을 DB 저장 전에 검증하고 일관된 ProblemDetails로 실패시킨다.
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ValidationProblem("Meetup name is required.");
        }

        var now = DateTimeOffset.UtcNow;
        var meetup = new Meetup
        {
            Name = request.Name.Trim(),
            Date = request.Date,
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            Currency = NormalizeCurrency(request.Currency),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Meetups.Add(meetup);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Created(
            $"/api/meetups/{meetup.Id}",
            new MeetupSummaryResponse(
                meetup.Id,
                meetup.Name,
                meetup.Date,
                meetup.Location,
                meetup.Currency,
                meetup.CreatedAt));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MeetupDetailResponse>> GetMeetupAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        const string meetupSql = """
            SELECT
                id AS "Id",
                name AS "Name",
                date AS "Date",
                location AS "Location",
                currency AS "Currency",
                created_at AS "CreatedAt"
            FROM meetups
            WHERE id = @Id
            """;

        const string participantsSql = """
            SELECT
                id AS "Id",
                name AS "Name",
                is_attending AS "IsAttending"
            FROM participants
            WHERE meetup_id = @Id
            ORDER BY name ASC, id ASC
            """;

        const string expensesSql = """
            SELECT
                id AS "Id",
                title AS "Title",
                payer_participant_id AS "PayerParticipantId",
                amount AS "Amount"
            FROM expenses
            WHERE meetup_id = @Id
            ORDER BY created_at ASC, id ASC
            """;

        const string sharesSql = """
            SELECT
                expense_id AS "ExpenseId",
                participant_id AS "ParticipantId",
                weight AS "Weight"
            FROM expense_shares
            WHERE expense_id IN (
                SELECT id
                FROM expenses
                WHERE meetup_id = @Id
            )
            ORDER BY expense_id ASC, participant_id ASC
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var meetup = await connection.QuerySingleOrDefaultAsync<MeetupSummaryRow>(
            new CommandDefinition(meetupSql, new { Id = id }, cancellationToken: cancellationToken));

        if (meetup is null)
        {
            return NotFound();
        }

        var participants = (await connection.QueryAsync<ParticipantResponse>(
            new CommandDefinition(participantsSql, new { Id = id }, cancellationToken: cancellationToken))).ToList();

        var expenseRows = (await connection.QueryAsync<ExpenseRow>(
            new CommandDefinition(expensesSql, new { Id = id }, cancellationToken: cancellationToken))).ToList();

        var shareRows = (await connection.QueryAsync<ExpenseShareRow>(
            new CommandDefinition(sharesSql, new { Id = id }, cancellationToken: cancellationToken))).ToList();

        var sharesByExpenseId = shareRows
            .GroupBy(share => share.ExpenseId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<ExpenseShareResponse>)group
                    .Select(share => new ExpenseShareResponse(share.ParticipantId, share.Weight))
                    .ToList());

        var expenses = expenseRows
            .Select(expense => new ExpenseResponse(
                expense.Id,
                expense.Title,
                expense.PayerParticipantId,
                expense.Amount,
                sharesByExpenseId.GetValueOrDefault(expense.Id, [])))
            .ToList();

        return new MeetupDetailResponse(
            meetup.Id,
            meetup.Name,
            meetup.Date,
            meetup.Location,
            meetup.Currency,
            participants,
            expenses);
    }

    [HttpPost("{id:guid}/participants")]
    public async Task<ActionResult<ParticipantResponse>> AddParticipantAsync(
        Guid id,
        CreateParticipantRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ValidationProblem("Participant name is required.");
        }

        var meetupExists = await dbContext.Meetups.AnyAsync(meetup => meetup.Id == id, cancellationToken);
        if (!meetupExists)
        {
            return NotFound();
        }

        var now = DateTimeOffset.UtcNow;
        var participant = new Participant
        {
            MeetupId = id,
            Name = request.Name.Trim(),
            IsAttending = request.IsAttending,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Participants.Add(participant);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Created(
            $"/api/meetups/{id}/participants/{participant.Id}",
            new ParticipantResponse(participant.Id, participant.Name, participant.IsAttending));
    }

    [HttpPatch("{id:guid}/participants/{participantId:guid}")]
    public async Task<ActionResult<ParticipantResponse>> UpdateParticipantAsync(
        Guid id,
        Guid participantId,
        UpdateParticipantRequest request,
        CancellationToken cancellationToken)
    {
        var participant = await dbContext.Participants
            .FirstOrDefaultAsync(item => item.Id == participantId && item.MeetupId == id, cancellationToken);

        if (participant is null)
        {
            return NotFound();
        }

        participant.IsAttending = request.IsAttending;
        participant.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return new ParticipantResponse(participant.Id, participant.Name, participant.IsAttending);
    }

    [HttpPost("{id:guid}/expenses")]
    public async Task<ActionResult<ExpenseResponse>> AddExpenseAsync(
        Guid id,
        CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return ValidationProblem("Expense title is required.");
        }

        if (request.Amount < 0)
        {
            return ValidationProblem("Expense amount cannot be negative.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var participants = await dbContext.Participants
            .Where(participant => participant.MeetupId == id)
            .ToListAsync(cancellationToken);

        if (participants.Count == 0)
        {
            return ValidationProblem("At least one participant is required before adding an expense.");
        }

        if (participants.All(participant => participant.Id != request.PayerParticipantId))
        {
            return ValidationProblem("Payer participant must belong to this meetup.");
        }

        var sharedByParticipantIds = request.SharedByParticipantIds?.Count > 0
            ? request.SharedByParticipantIds.Distinct().ToList()
            : participants.Where(participant => participant.IsAttending).Select(participant => participant.Id).ToList();

        if (sharedByParticipantIds.Count == 0)
        {
            return ValidationProblem("Expense must be shared by at least one participant.");
        }

        var participantIds = participants.Select(participant => participant.Id).ToHashSet();
        if (sharedByParticipantIds.Any(participantId => !participantIds.Contains(participantId)))
        {
            return ValidationProblem("Every shared participant must belong to this meetup.");
        }

        var now = DateTimeOffset.UtcNow;
        var expense = new Expense
        {
            MeetupId = id,
            PayerParticipantId = request.PayerParticipantId,
            Title = request.Title.Trim(),
            Amount = request.Amount,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var participantId in sharedByParticipantIds)
        {
            expense.Shares.Add(new ExpenseShare
            {
                ParticipantId = participantId,
                Weight = 1m
            });
        }

        dbContext.Expenses.Add(expense);
        await dbContext.SaveChangesAsync(cancellationToken);

        await settlementProjectionService.RebuildTransfersAsync(id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Created(
            $"/api/meetups/{id}/expenses/{expense.Id}",
            new ExpenseResponse(
                expense.Id,
                expense.Title,
                expense.PayerParticipantId,
                expense.Amount,
                expense.Shares.Select(share => new ExpenseShareResponse(share.ParticipantId, share.Weight)).ToList()));
    }

    [HttpGet("{id:guid}/settlement")]
    public async Task<ActionResult<SettlementResponse>> GetSettlementAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        const string meetupSql = """
            SELECT id AS "Id", currency AS "Currency"
            FROM meetups
            WHERE id = @Id
            """;

        const string participantsSql = """
            SELECT
                id AS "Id",
                name AS "Name"
            FROM participants
            WHERE meetup_id = @Id
            ORDER BY name ASC, id ASC
            """;

        const string expensesSql = """
            SELECT
                id AS "Id",
                title AS "Title",
                payer_participant_id AS "PayerParticipantId",
                amount AS "Amount"
            FROM expenses
            WHERE meetup_id = @Id
            ORDER BY created_at ASC, id ASC
            """;

        const string sharesSql = """
            SELECT
                expense_id AS "ExpenseId",
                participant_id AS "ParticipantId",
                weight AS "Weight"
            FROM expense_shares
            WHERE expense_id IN (
                SELECT id
                FROM expenses
                WHERE meetup_id = @Id
            )
            ORDER BY expense_id ASC, participant_id ASC
            """;

        const string transferSql = """
            SELECT
                id AS "Id",
                from_participant_id AS "FromParticipantId",
                to_participant_id AS "ToParticipantId",
                amount AS "Amount",
                is_completed AS "IsCompleted"
            FROM settlement_transfers
            WHERE meetup_id = @Id
            ORDER BY created_at ASC, id ASC
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var meetup = await connection.QuerySingleOrDefaultAsync<SettlementMeetupRow>(
            new CommandDefinition(meetupSql, new { Id = id }, cancellationToken: cancellationToken));

        if (meetup is null)
        {
            return NotFound();
        }

        var participants = (await connection.QueryAsync<SettlementParticipantRow>(
            new CommandDefinition(participantsSql, new { Id = id }, cancellationToken: cancellationToken))).ToList();

        var expenses = (await connection.QueryAsync<ExpenseRow>(
            new CommandDefinition(expensesSql, new { Id = id }, cancellationToken: cancellationToken))).ToList();

        var shares = (await connection.QueryAsync<ExpenseShareRow>(
            new CommandDefinition(sharesSql, new { Id = id }, cancellationToken: cancellationToken))).ToList();

        var persistedTransfers = (await connection.QueryAsync<PersistedTransferRow>(
            new CommandDefinition(transferSql, new { Id = id }, cancellationToken: cancellationToken))).ToList();

        var result = settlementCalculator.Calculate(
            participants.Select(participant => new SettlementParticipant(participant.Id.ToString(), participant.Name)).ToList(),
            expenses.Select(expense => new SettlementExpense(
                expense.Id.ToString(),
                expense.Title,
                expense.PayerParticipantId.ToString(),
                expense.Amount,
                shares
                    .Where(share => share.ExpenseId == expense.Id)
                    .Select(share => share.ParticipantId.ToString())
                    .ToList(),
                shares
                    .Where(share => share.ExpenseId == expense.Id)
                    .ToDictionary(share => share.ParticipantId.ToString(), share => share.Weight))).ToList(),
            new SettlementOptions(meetup.Currency, meetup.Currency == "KRW" ? 0 : 2));

        var persistedByKey = persistedTransfers.ToDictionary(
            transfer => MakeTransferKey(transfer.FromParticipantId, transfer.ToParticipantId, transfer.Amount),
            transfer => transfer);

        return new SettlementResponse(
            id,
            result.Currency,
            result.Balances.Select(balance => new BalanceResponse(
                Guid.Parse(balance.ParticipantId),
                balance.ParticipantName,
                balance.PaidAmount,
                balance.OwedAmount,
                balance.NetAmount)).ToList(),
            result.Transfers.Select(transfer =>
            {
                var fromParticipantId = Guid.Parse(transfer.FromParticipantId);
                var toParticipantId = Guid.Parse(transfer.ToParticipantId);
                var key = MakeTransferKey(fromParticipantId, toParticipantId, transfer.Amount);
                persistedByKey.TryGetValue(key, out var persistedTransfer);

                return new TransferResponse(
                    persistedTransfer?.Id ?? Guid.Empty,
                    fromParticipantId,
                    transfer.FromParticipantName,
                    toParticipantId,
                    transfer.ToParticipantName,
                    transfer.Amount,
                    persistedTransfer?.IsCompleted ?? false);
            }).ToList());
    }

    [HttpPatch("{id:guid}/transfers/{transferId:guid}")]
    public async Task<ActionResult<TransferResponse>> UpdateTransferAsync(
        Guid id,
        Guid transferId,
        UpdateTransferRequest request,
        CancellationToken cancellationToken)
    {
        var transfer = await dbContext.SettlementTransfers
            .Include(item => item.FromParticipant)
            .Include(item => item.ToParticipant)
            .FirstOrDefaultAsync(item => item.Id == transferId && item.MeetupId == id, cancellationToken);

        if (transfer is null)
        {
            return NotFound();
        }

        transfer.IsCompleted = request.IsCompleted;
        transfer.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new TransferResponse(
            transfer.Id,
            transfer.FromParticipantId,
            transfer.FromParticipant?.Name ?? string.Empty,
            transfer.ToParticipantId,
            transfer.ToParticipant?.Name ?? string.Empty,
            transfer.Amount,
            transfer.IsCompleted);
    }

    [HttpPost("{id:guid}/invite-token")]
    public async Task<ActionResult<CreateInviteTokenResponse>> CreateInviteTokenAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var meetupExists = await dbContext.Meetups.AnyAsync(meetup => meetup.Id == id, cancellationToken);
        if (!meetupExists)
        {
            return NotFound();
        }

        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var inviteToken = new InviteToken
        {
            MeetupId = id,
            Token = token,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(14),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.InviteTokens.Add(inviteToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Created(
            $"/api/invites/{token}",
            new CreateInviteTokenResponse(token, $"/invites/{token}", inviteToken.ExpiresAt));
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency) ? "KRW" : currency.Trim().ToUpperInvariant();
    }

    private static string MakeTransferKey(Guid fromParticipantId, Guid toParticipantId, decimal amount)
    {
        return $"{fromParticipantId:N}:{toParticipantId:N}:{amount:0.##}";
    }

    private sealed class MeetupSummaryRow
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public string? Location { get; set; }

        public string Currency { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
    }

    private sealed class SettlementMeetupRow
    {
        public Guid Id { get; set; }

        public string Currency { get; set; } = string.Empty;
    }

    private sealed class SettlementParticipantRow
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    private sealed class ExpenseRow
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public Guid PayerParticipantId { get; set; }

        public decimal Amount { get; set; }
    }

    private sealed class ExpenseShareRow
    {
        public Guid ExpenseId { get; set; }

        public Guid ParticipantId { get; set; }

        public decimal Weight { get; set; }
    }

    private sealed class PersistedTransferRow
    {
        public Guid Id { get; set; }

        public Guid FromParticipantId { get; set; }

        public Guid ToParticipantId { get; set; }

        public decimal Amount { get; set; }

        public bool IsCompleted { get; set; }
    }
}
