// 파일 용도: settlement_transfers 테이블의 EF Core Fluent API 매핑을 정의한다.
// 파일 목적: 계산된 송금 항목과 완료 상태를 모임별로 안정적으로 추적한다.
using MeetSettle.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetSettle.Api.Data.Configurations;

public sealed class SettlementTransferConfiguration : IEntityTypeConfiguration<SettlementTransfer>
{
    public void Configure(EntityTypeBuilder<SettlementTransfer> builder)
    {
        builder.ToTable("settlement_transfers", table =>
        {
            table.HasCheckConstraint("ck_settlement_transfers_amount_positive", "amount > 0");
        });

        builder.HasKey(transfer => transfer.Id);

        builder.Property(transfer => transfer.Id).HasColumnName("id");
        builder.Property(transfer => transfer.MeetupId).HasColumnName("meetup_id");
        builder.Property(transfer => transfer.FromParticipantId).HasColumnName("from_participant_id");
        builder.Property(transfer => transfer.ToParticipantId).HasColumnName("to_participant_id");
        builder.Property(transfer => transfer.Amount).HasColumnName("amount").HasPrecision(12, 2).IsRequired();
        builder.Property(transfer => transfer.IsCompleted).HasColumnName("is_completed").HasDefaultValue(false);
        builder.Property(transfer => transfer.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(transfer => transfer.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

        builder.HasOne(transfer => transfer.Meetup)
            .WithMany(meetup => meetup.SettlementTransfers)
            .HasForeignKey(transfer => transfer.MeetupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(transfer => transfer.FromParticipant)
            .WithMany()
            .HasForeignKey(transfer => transfer.FromParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transfer => transfer.ToParticipant)
            .WithMany()
            .HasForeignKey(transfer => transfer.ToParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(transfer => transfer.MeetupId)
            .HasDatabaseName("ix_settlement_transfers_meetup_id");
    }
}
