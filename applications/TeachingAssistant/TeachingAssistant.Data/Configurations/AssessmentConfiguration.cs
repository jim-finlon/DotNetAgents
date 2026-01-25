using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="Assessment"/>.
/// </summary>
public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.Type).HasConversion<string>();
        builder.Property(a => a.Subject).HasConversion<string>();
        builder.Property(a => a.GradeBand).HasConversion<string>();
        builder.Property(a => a.Title).IsRequired().HasMaxLength(255);
        builder.Property(a => a.Description).HasColumnType("text");
        builder.Property(a => a.ContentUnitIds).HasColumnType("uuid[]");
        builder.Property(a => a.PassingScore).HasColumnType("decimal(5,2)");
        builder.Property(a => a.IsAdaptive).HasDefaultValue(false);
        builder.Property(a => a.IsActive).HasDefaultValue(true);
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("NOW()");
    }
}
