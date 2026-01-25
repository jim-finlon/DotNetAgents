using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="LearningObjective"/>.
/// </summary>
public class LearningObjectiveConfiguration : IEntityTypeConfiguration<LearningObjective>
{
    public void Configure(EntityTypeBuilder<LearningObjective> builder)
    {
        builder.HasKey(lo => lo.Id);
        builder.Property(lo => lo.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(lo => lo.Description).IsRequired().HasColumnType("text");
        builder.Property(lo => lo.BloomLevel).HasConversion<string>();
        builder.Property(lo => lo.MeasurableCriteria).HasColumnType("text");
        builder.Property(lo => lo.SequenceOrder).HasDefaultValue(0);
    }
}
