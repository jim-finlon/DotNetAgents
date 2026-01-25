using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="AuditLogEntry"/>.
/// </summary>
public class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.ActorType).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(100);
        builder.Property(a => a.ResourceType).IsRequired().HasMaxLength(50);
        builder.Property(a => a.OldValues).HasColumnType("jsonb");
        builder.Property(a => a.NewValues).HasColumnType("jsonb");
        builder.Property(a => a.UserAgent).HasColumnType("text");
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(a => new { a.ActorId, a.CreatedAt })
            .IsDescending(true, true);

        builder.HasIndex(a => new { a.ResourceType, a.ResourceId });
    }
}
