// 파일 용도: 정산 엔진이 계산한 송금 항목과 완료 여부를 저장한다.
// 파일 목적: 조회는 계산 결과를 보여주고 Command 경로는 송금 완료 상태를 추적하게 한다.
namespace MeetSettle.Api.Models;

public sealed class SettlementTransfer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MeetupId { get; set; }

    public Meetup? Meetup { get; set; }

    public Guid FromParticipantId { get; set; }

    public Participant? FromParticipant { get; set; }

    public Guid ToParticipantId { get; set; }

    public Participant? ToParticipant { get; set; }

    public decimal Amount { get; set; }

    public bool IsCompleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
