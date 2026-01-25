using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="ContentMastery"/>.
/// </summary>
public class ContentMasteryConfiguration : IEntityTypeConfiguration<ContentMastery>
{
    public void Configure(EntityTypeBuilder<ContentMastery> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(m => m.MasteryLevel).HasConversion<string>().HasDefaultValue(MasteryLevel.NotStarted);
        builder.Property(m => m.MasteryScore).HasColumnType("decimal(5,2)");
        builder.Property(m => m.EaseFactor).HasColumnType("decimal(4,2)").HasDefaultValue(2.5m);
        builder.Property(m => m.IntervalDays).HasDefaultValue(1);
        builder.Property(m => m.Metadata).HasColumnType("jsonb").HasDefaultValue(new Dictionary<string, object>());

        builder.HasIndex(m => new { m.StudentId, m.ContentUnitId })
            .IsUnique();

        builder.HasIndex(m => m.StudentId);
        builder.HasIndex(m => m.NextReviewAt)
            .HasFilter("[mastery_level] != 'NotStarted'");
    }
}
