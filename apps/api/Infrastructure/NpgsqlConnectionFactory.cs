// 파일 용도: Dapper Query 경로에서 사용할 PostgreSQL 연결 생성을 담당한다.
// 파일 목적: 조회 코드가 DbContext에 의존하지 않고 명시적인 SQL로 데이터를 읽도록 분리한다.
using Npgsql;

namespace MeetSettle.Api.Infrastructure;

public sealed class NpgsqlConnectionFactory(string connectionString)
{
    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}
