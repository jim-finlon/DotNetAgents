using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="ContentUnit"/>.
/// </summary>
public class ContentUnitConfiguration : IEntityTypeConfiguration<ContentUnit>
{
    public void Configure(EntityTypeBuilder<ContentUnit> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Type).HasConversion<string>();
        builder.Property(c => c.Subject).HasConversion<string>();
        builder.Property(c => c.GradeBand).HasConversion<string>();
        builder.Property(c => c.TopicPath).HasColumnType("text[]");
        builder.Property(c => c.Title).IsRequired().HasMaxLength(255);
        builder.Property(c => c.Summary).HasColumnType("text");
        builder.Property(c => c.Content).HasColumnType("jsonb").IsRequired();
        builder.Property(c => c.NgssStandards).HasColumnType("text[]");
        builder.ToTable(t => t.HasCheckConstraint("CK_ContentUnit_VocabularyTier", "vocabulary_tier BETWEEN 1 AND 3"));
        builder.Property(c => c.SourceFile).HasMaxLength(500);
        builder.Property(c => c.Version).HasMaxLength(20).HasDefaultValue("1.0.0");
        builder.Property(c => c.IsActive).HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("NOW()");

        // Full-text search index
        builder.HasIndex(c => new { c.Title, c.Summary })
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        // Topic path GIN index (created via raw SQL in migration)
        // CREATE INDEX idx_content_topic ON content_units USING GIN (topic_path);

        // Subject and grade band index
        builder.HasIndex(c => new { c.Subject, c.GradeBand })
            .HasFilter("[is_active] = TRUE");

        builder.HasMany(c => c.LearningObjectives)
            .WithOne(lo => lo.ContentUnit)
            .HasForeignKey(lo => lo.ContentUnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Prerequisites)
            .WithOne(p => p.ContentUnit)
            .HasForeignKey(p => p.ContentUnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.RequiredBy)
            .WithOne(p => p.PrerequisiteUnit)
            .HasForeignKey(p => p.PrerequisiteUnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Embeddings)
            .WithOne(e => e.ContentUnit)
            .HasForeignKey(e => e.ContentUnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
