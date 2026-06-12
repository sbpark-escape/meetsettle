// 파일 용도: 공유 가능한 초대 링크의 토큰과 만료 정보를 저장한다.
// 파일 목적: MVP에서 로그인 없이도 모임 접근을 제한된 링크로 처리하게 한다.
namespace MeetSettle.Api.Models;

public sealed class InviteToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MeetupId { get; set; }

    public Meetup? Meetup { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
