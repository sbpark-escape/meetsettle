// 파일 용도: 특정 참가자가 결제한 비용 항목을 표현한다.
// 파일 목적: 비용 부담 대상 목록과 함께 정산 엔진의 입력 데이터가 되도록 한다.
namespace MeetSettle.Api.Models;

public sealed class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MeetupId { get; set; }

    public Meetup? Meetup { get; set; }

    public Guid PayerParticipantId { get; set; }

    public Participant? PayerParticipant { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<ExpenseShare> Shares { get; } = [];
}
