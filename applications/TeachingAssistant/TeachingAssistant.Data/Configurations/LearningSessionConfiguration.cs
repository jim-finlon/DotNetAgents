using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="LearningSession"/>.
/// </summary>
public class LearningSessionConfiguration : IEntityTypeConfiguration<LearningSession>
{
    public void Configure(EntityTypeBuilder<LearningSession> builder)
    {
        builder.HasKey(ls => ls.Id);
        builder.Property(ls => ls.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ls => ls.Subject).HasConversion<string>();
        builder.Property(ls => ls.SessionType).HasMaxLength(50);
        builder.Property(ls => ls.StartedAt).HasDefaultValueSql("NOW()");
        builder.Property(ls => ls.ContentUnitsCovered).HasColumnType("uuid[]");
        builder.Property(ls => ls.Summary).HasColumnType("jsonb");
        builder.Property(ls => ls.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(ls => new { ls.StudentId, ls.StartedAt })
            .IsDescending(true, true);
    }
}
