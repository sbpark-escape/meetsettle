// 파일 용도: 모임의 기본 정보와 하위 도메인 컬렉션을 표현한다.
// 파일 목적: 참가자, 비용, 초대 토큰, 송금 상태의 aggregate root 역할을 제공한다.
namespace MeetSettle.Api.Models;

public sealed class Meetup
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    public string? Location { get; set; }

    public string Currency { get; set; } = "KRW";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<Participant> Participants { get; } = [];

    public List<Expense> Expenses { get; } = [];

    public List<SettlementTransfer> SettlementTransfers { get; } = [];

    public List<InviteToken> InviteTokens { get; } = [];
}
