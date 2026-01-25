using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="AssessmentItem"/>.
/// </summary>
public class AssessmentItemConfiguration : IEntityTypeConfiguration<AssessmentItem>
{
    public void Configure(EntityTypeBuilder<AssessmentItem> builder)
    {
        builder.HasKey(ai => ai.Id);
        builder.Property(ai => ai.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ai => ai.QuestionType).HasConversion<string>();
        builder.ToTable(t => t.HasCheckConstraint("CK_AssessmentItem_Difficulty", "difficulty BETWEEN 1 AND 5"));
        builder.Property(ai => ai.BloomLevel).HasConversion<string>();
        builder.Property(ai => ai.Points).HasColumnType("decimal(5,2)").HasDefaultValue(1.0m);
        builder.Property(ai => ai.QuestionData).HasColumnType("jsonb").IsRequired();
        builder.Property(ai => ai.FeedbackData).HasColumnType("jsonb").IsRequired();
        builder.Property(ai => ai.IrtDifficulty).HasColumnType("decimal(5,3)");
        builder.Property(ai => ai.IrtDiscrimination).HasColumnType("decimal(5,3)");
        builder.Property(ai => ai.AvgTimeSeconds).HasColumnType("decimal(8,2)");
        builder.Property(ai => ai.IsActive).HasDefaultValue(true);
        builder.Property(ai => ai.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(ai => ai.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(ai => ai.ContentUnitId)
            .HasFilter("[is_active] = TRUE");

        builder.HasIndex(ai => new { ai.Difficulty, ai.BloomLevel })
            .HasFilter("[is_active] = TRUE");
    }
}
