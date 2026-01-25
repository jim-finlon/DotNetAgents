using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="StudentResponse"/>.
/// </summary>
public class StudentResponseConfiguration : IEntityTypeConfiguration<StudentResponse>
{
    public void Configure(EntityTypeBuilder<StudentResponse> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.Response).HasColumnType("jsonb").IsRequired();
        builder.Property(r => r.PartialCredit).HasColumnType("decimal(5,2)");
        builder.Property(r => r.HintsUsed).HasDefaultValue(0);
        builder.Property(r => r.FeedbackShown).HasDefaultValue(false);
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(r => new { r.StudentId, r.CreatedAt })
            .IsDescending(true, true);
    }
}
