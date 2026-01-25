using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;
using Pgvector;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="ContentEmbedding"/>.
/// </summary>
public class ContentEmbeddingConfiguration : IEntityTypeConfiguration<ContentEmbedding>
{
    public void Configure(EntityTypeBuilder<ContentEmbedding> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.ChunkText).IsRequired().HasColumnType("text");
        builder.Property(e => e.Embedding).HasColumnType("vector(1536)");
        builder.Property(e => e.Metadata).HasColumnType("jsonb").HasDefaultValue(new Dictionary<string, object>());
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

        // Unique constraint on content_unit_id and chunk_index
        builder.HasIndex(e => new { e.ContentUnitId, e.ChunkIndex })
            .IsUnique();

        // HNSW index for vector similarity search
        builder.HasIndex(e => e.Embedding)
            .HasMethod("hnsw")
            .HasOperators("vector_cosine_ops");
    }
}
