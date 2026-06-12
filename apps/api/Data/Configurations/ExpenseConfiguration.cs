// 파일 용도: expenses 테이블의 EF Core Fluent API 매핑을 정의한다.
// 파일 목적: 결제자, 금액, 비용명 제약을 DB 레벨에서 명확하게 보장한다.
using MeetSettle.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetSettle.Api.Data.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses", table =>
        {
            table.HasCheckConstraint("ck_expenses_amount_non_negative", "amount >= 0");
        });

        builder.HasKey(expense => expense.Id);

        builder.Property(expense => expense.Id).HasColumnName("id");
        builder.Property(expense => expense.MeetupId).HasColumnName("meetup_id");
        builder.Property(expense => expense.PayerParticipantId).HasColumnName("payer_participant_id");
        builder.Property(expense => expense.Title).HasColumnName("title").HasMaxLength(120).IsRequired();
        builder.Property(expense => expense.Amount).HasColumnName("amount").HasPrecision(12, 2).IsRequired();
        builder.Property(expense => expense.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(expense => expense.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

        builder.HasOne(expense => expense.Meetup)
            .WithMany(meetup => meetup.Expenses)
            .HasForeignKey(expense => expense.MeetupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(expense => expense.PayerParticipant)
            .WithMany()
            .HasForeignKey(expense => expense.PayerParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(expense => expense.MeetupId).HasDatabaseName("ix_expenses_meetup_id");
    }
}
