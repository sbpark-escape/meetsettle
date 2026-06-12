// 파일 용도: 모임에 초대되거나 참여한 사람의 상태를 표현한다.
// 파일 목적: 참석 여부와 정산 대상자를 비용/송금 계산에서 일관되게 참조하게 한다.
namespace MeetSettle.Api.Models;

public sealed class Participant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MeetupId { get; set; }

    public Meetup? Meetup { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsAttending { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
