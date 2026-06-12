// 파일 용도: 정산 엔진이 주고받는 입력/출력 모델을 정의한다.
// 파일 목적: API와 테스트가 같은 계약으로 금액 분배 결과를 검증하도록 돕는다.
namespace MeetSettle.SettlementCore;

public sealed record SettlementParticipant(
    string Id,
    string Name);

public sealed record SettlementExpense(
    string Id,
    string Title,
    string PayerId,
    decimal Amount,
    IReadOnlyCollection<string> SharedByParticipantIds,
    IReadOnlyDictionary<string, decimal>? Weights = null,
    IReadOnlyCollection<string>? ExcludedParticipantIds = null);

public sealed record SettlementOptions(
    string Currency = "KRW",
    int MinorUnitDigits = 0);

public sealed record ParticipantBalance(
    string ParticipantId,
    string ParticipantName,
    decimal PaidAmount,
    decimal OwedAmount,
    decimal NetAmount);

public sealed record SettlementTransfer(
    string FromParticipantId,
    string FromParticipantName,
    string ToParticipantId,
    string ToParticipantName,
    decimal Amount);

public sealed record SettlementResult(
    string Currency,
    IReadOnlyList<ParticipantBalance> Balances,
    IReadOnlyList<SettlementTransfer> Transfers);
