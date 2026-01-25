using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="Prerequisite"/>.
/// </summary>
public class PrerequisiteConfiguration : IEntityTypeConfiguration<Prerequisite>
{
    public void Configure(EntityTypeBuilder<Prerequisite> builder)
    {
        builder.HasKey(p => new { p.ContentUnitId, p.PrerequisiteUnitId });

        builder.Property(p => p.Strength).HasDefaultValue(2);
        builder.ToTable(t => t.HasCheckConstraint("CK_Prerequisite_Strength", "strength BETWEEN 1 AND 3"));
    }
}
