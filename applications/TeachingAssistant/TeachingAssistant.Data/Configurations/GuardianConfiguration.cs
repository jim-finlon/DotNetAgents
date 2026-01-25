using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="Guardian"/>.
/// </summary>
public class GuardianConfiguration : IEntityTypeConfiguration<Guardian>
{
    public void Configure(EntityTypeBuilder<Guardian> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(g => g.AuthProviderId).IsRequired().HasMaxLength(255);
        builder.HasIndex(g => g.AuthProviderId).IsUnique();
        builder.Property(g => g.Email).IsRequired().HasMaxLength(255);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(255);
        builder.Property(g => g.Role).HasConversion<string>();
        builder.Property(g => g.Permissions).HasColumnType("jsonb");
        builder.Property(g => g.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(g => g.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(g => g.FamilyId);
    }
}
