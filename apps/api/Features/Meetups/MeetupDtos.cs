// 파일 용도: 모임 API의 요청/응답 DTO를 정의한다.
// 파일 목적: 외부 API 계약과 EF Core 엔티티를 분리해 공개 인터페이스를 안정적으로 유지한다.
namespace MeetSettle.Api.Features.Meetups;

public sealed record CreateMeetupRequest(
    string Name,
    DateOnly Date,
    string? Location,
    string? Currency);

public sealed record CreateParticipantRequest(
    string Name,
    bool IsAttending = true);

public sealed record UpdateParticipantRequest(
    bool IsAttending);

public sealed record CreateExpenseRequest(
    string Title,
    Guid PayerParticipantId,
    decimal Amount,
    IReadOnlyCollection<Guid>? SharedByParticipantIds);

public sealed record UpdateTransferRequest(
    bool IsCompleted);

public sealed record MeetupSummaryResponse(
    Guid Id,
    string Name,
    DateOnly Date,
    string? Location,
    string Currency,
    DateTimeOffset CreatedAt);

public sealed record ParticipantResponse(
    Guid Id,
    string Name,
    bool IsAttending);

public sealed record ExpenseShareResponse(
    Guid ParticipantId,
    decimal Weight);

public sealed record ExpenseResponse(
    Guid Id,
    string Title,
    Guid PayerParticipantId,
    decimal Amount,
    IReadOnlyCollection<ExpenseShareResponse> Shares);

public sealed record MeetupDetailResponse(
    Guid Id,
    string Name,
    DateOnly Date,
    string? Location,
    string Currency,
    IReadOnlyCollection<ParticipantResponse> Participants,
    IReadOnlyCollection<ExpenseResponse> Expenses);

public sealed record BalanceResponse(
    Guid ParticipantId,
    string ParticipantName,
    decimal PaidAmount,
    decimal OwedAmount,
    decimal NetAmount);

public sealed record TransferResponse(
    Guid Id,
    Guid FromParticipantId,
    string FromParticipantName,
    Guid ToParticipantId,
    string ToParticipantName,
    decimal Amount,
    bool IsCompleted);

public sealed record SettlementResponse(
    Guid MeetupId,
    string Currency,
    IReadOnlyCollection<BalanceResponse> Balances,
    IReadOnlyCollection<TransferResponse> Transfers);

public sealed record CreateInviteTokenResponse(
    string Token,
    string InvitePath,
    DateTimeOffset ExpiresAt);
