// 파일 용도: meetups 테이블의 EF Core Fluent API 매핑을 정의한다.
// 파일 목적: 공개 API의 모임 데이터 제약조건을 DB 레벨에서도 일관되게 보장한다.
using MeetSettle.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeetSettle.Api.Data.Configurations;

public sealed class MeetupConfiguration : IEntityTypeConfiguration<Meetup>
{
    public void Configure(EntityTypeBuilder<Meetup> builder)
    {
        builder.ToTable("meetups");
        builder.HasKey(meetup => meetup.Id);

        builder.Property(meetup => meetup.Id).HasColumnName("id");
        builder.Property(meetup => meetup.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(meetup => meetup.Date).HasColumnName("date").IsRequired();
        builder.Property(meetup => meetup.Location).HasColumnName("location").HasMaxLength(160);
        builder.Property(meetup => meetup.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(meetup => meetup.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
        builder.Property(meetup => meetup.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()");

        builder.HasIndex(meetup => meetup.Date).HasDatabaseName("ix_meetups_date");
    }
}
