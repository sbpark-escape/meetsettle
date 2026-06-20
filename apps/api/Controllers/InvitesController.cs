// 파일 용도: 초대 토큰으로 모임 정보를 조회하는 API를 제공한다.
// 파일 목적: 로그인 없는 MVP 접근 흐름에서 토큰 검증과 조회 책임을 분리한다.
using Dapper;
using MeetSettle.Api.Features.Invites;
using MeetSettle.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace MeetSettle.Api.Controllers;

[ApiController]
[Route("api/invites")]
public sealed class InvitesController(IDbConnectionFactory connectionFactory) : ControllerBase
{
    [HttpGet("{token}")]
    public async Task<ActionResult<InviteMeetupResponse>> GetInviteAsync(
        string token,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                m.id AS "MeetupId",
                m.name AS "MeetupName",
                m.date AS "Date",
                m.location AS "Location",
                m.currency AS "Currency",
                i.expires_at AS "ExpiresAt"
            FROM invite_tokens i
            INNER JOIN meetups m ON m.id = i.meetup_id
            WHERE i.token = @Token
              AND i.expires_at > now()
            """;

        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var invite = await connection.QuerySingleOrDefaultAsync<InviteMeetupResponse>(
            new CommandDefinition(sql, new { Token = token }, cancellationToken: cancellationToken));

        return invite is null ? NotFound() : invite;
    }
}
