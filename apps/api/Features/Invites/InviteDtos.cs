// 파일 용도: 초대 링크 조회 API의 응답 DTO를 정의한다.
// 파일 목적: 토큰으로 접근한 사용자가 필요한 모임 정보만 받도록 API 계약을 분리한다.
namespace MeetSettle.Api.Features.Invites;

public sealed record InviteMeetupResponse(
    Guid MeetupId,
    string MeetupName,
    DateOnly Date,
    string? Location,
    string Currency,
    DateTimeOffset ExpiresAt);
