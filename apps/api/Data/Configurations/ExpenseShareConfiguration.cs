// 파일 용도: expense_shares 테이블의 EF Core Fluent API 매핑을 정의한다.
// 파일 목적: 한 비용 안에서 참가자별 부담 가중치를 중복 없이 저장한다.
using MeetSettle.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetSettle.Api.Data.Configurations;

public sealed class ExpenseShareConfiguration : IEntityTypeConfiguration<ExpenseShare>
{
    public void Configure(EntityTypeBuilder<ExpenseShare> builder)
    {
        builder.ToTable("expense_shares", table =>
        {
            table.HasCheckConstraint("ck_expense_shares_weight_positive", "weight > 0");
        });

        builder.HasKey(share => share.Id);

        builder.Property(share => share.Id).HasColumnName("id");
        builder.Property(share => share.ExpenseId).HasColumnName("expense_id");
        builder.Property(share => share.ParticipantId).HasColumnName("participant_id");
        builder.Property(share => share.Weight).HasColumnName("weight").HasPrecision(8, 2).HasDefaultValue(1m);

        builder.HasOne(share => share.Expense)
            .WithMany(expense => expense.Shares)
            .HasForeignKey(share => share.ExpenseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(share => share.Participant)
            .WithMany()
            .HasForeignKey(share => share.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(share => new { share.ExpenseId, share.ParticipantId })
            .IsUnique()
            .HasDatabaseName("ux_expense_shares_expense_id_participant_id");
    }
}
