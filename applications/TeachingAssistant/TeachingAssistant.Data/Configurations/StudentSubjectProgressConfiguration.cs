using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="StudentSubjectProgress"/>.
/// </summary>
public class StudentSubjectProgressConfiguration : IEntityTypeConfiguration<StudentSubjectProgress>
{
    public void Configure(EntityTypeBuilder<StudentSubjectProgress> builder)
    {
        builder.HasKey(p => new { p.StudentId, p.Subject });

        builder.Property(p => p.Subject).HasConversion<string>();
        builder.Property(p => p.OverallProgress).HasColumnType("decimal(5,2)").HasDefaultValue(0);
        builder.Property(p => p.TotalTimeMinutes).HasDefaultValue(0);
        builder.Property(p => p.StreakDays).HasDefaultValue(0);
        builder.Property(p => p.LongestStreak).HasDefaultValue(0);
        builder.Property(p => p.Settings).HasColumnType("jsonb").HasDefaultValue(new Dictionary<string, object>());
    }
}
