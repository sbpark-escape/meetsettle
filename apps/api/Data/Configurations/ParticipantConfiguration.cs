// 파일 용도: participants 테이블의 EF Core Fluent API 매핑을 정의한다.
// 파일 목적: 모임별 참가자 목록과 참석 상태를 안정적으로 저장한다.
using MeetSettle.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetSettle.Api.Data.Configurations;

public sealed class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("participants");
        builder.HasKey(participant => participant.Id);

        builder.Property(participant => participant.Id).HasColumnName("id");
        builder.Property(participant => participant.MeetupId).HasColumnName("meetup_id");
        builder.Property(participant => participant.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(participant => participant.IsAttending).HasColumnName("is_attending").HasDefaultValue(true);
        builder.Property(participant => participant.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(participant => participant.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

        builder.HasOne(participant => participant.Meetup)
            .WithMany(meetup => meetup.Participants)
            .HasForeignKey(participant => participant.MeetupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(participant => new { participant.MeetupId, participant.Name })
            .HasDatabaseName("ix_participants_meetup_id_name");
    }
}
