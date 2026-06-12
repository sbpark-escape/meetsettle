// 파일 용도: 비용 항목을 어떤 참가자들이 어떤 가중치로 나누는지 표현한다.
// 파일 목적: 기본 N분의 1뿐 아니라 향후 가중치 정산 확장을 가능하게 한다.
namespace MeetSettle.Api.Models;

public sealed class ExpenseShare
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ExpenseId { get; set; }

    public Expense? Expense { get; set; }

    public Guid ParticipantId { get; set; }

    public Participant? Participant { get; set; }

    public decimal Weight { get; set; } = 1m;
}
