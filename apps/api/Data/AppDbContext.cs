// 파일 용도: EF Core Command 경로에서 사용하는 데이터베이스 컨텍스트를 정의한다.
// 파일 목적: 상태 변경 로직과 Fluent API 매핑을 한 진입점으로 묶어 도메인 무결성을 유지한다.
using MeetSettle.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MeetSettle.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Meetup> Meetups => Set<Meetup>();

    public DbSet<Participant> Participants => Set<Participant>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<ExpenseShare> ExpenseShares => Set<ExpenseShare>();

    public DbSet<SettlementTransfer> SettlementTransfers => Set<SettlementTransfer>();

    public DbSet<InviteToken> InviteTokens => Set<InviteToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
