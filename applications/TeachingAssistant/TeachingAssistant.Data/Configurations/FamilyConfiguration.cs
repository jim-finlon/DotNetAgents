using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="Family"/>.
/// </summary>
public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(f => f.Name).IsRequired().HasMaxLength(255);
        builder.Property(f => f.SubscriptionTier).HasConversion<string>();
        builder.Property(f => f.Settings).HasColumnType("jsonb").HasDefaultValue(new Dictionary<string, object>());
        builder.Property(f => f.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(f => f.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasQueryFilter(f => f.DeletedAt == null);

        builder.HasMany(f => f.Guardians)
            .WithOne(g => g.Family)
            .HasForeignKey(g => g.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Students)
            .WithOne(s => s.Family)
            .HasForeignKey(s => s.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
