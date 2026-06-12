// 파일 용도: invite_tokens 테이블의 EF Core Fluent API 매핑을 정의한다.
// 파일 목적: 추측하기 어려운 초대 토큰을 고유하게 저장하고 만료 기준을 관리한다.
using MeetSettle.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetSettle.Api.Data.Configurations;

public sealed class InviteTokenConfiguration : IEntityTypeConfiguration<InviteToken>
{
    public void Configure(EntityTypeBuilder<InviteToken> builder)
    {
        builder.ToTable("invite_tokens");
        builder.HasKey(token => token.Id);

        builder.Property(token => token.Id).HasColumnName("id");
        builder.Property(token => token.MeetupId).HasColumnName("meetup_id");
        builder.Property(token => token.Token).HasColumnName("token").HasMaxLength(96).IsRequired();
        builder.Property(token => token.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(token => token.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

        builder.HasOne(token => token.Meetup)
            .WithMany(meetup => meetup.InviteTokens)
            .HasForeignKey(token => token.MeetupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(token => token.Token)
            .IsUnique()
            .HasDatabaseName("ux_invite_tokens_token");
    }
}
