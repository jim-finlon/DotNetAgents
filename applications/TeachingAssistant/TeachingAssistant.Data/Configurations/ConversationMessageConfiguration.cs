using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="ConversationMessage"/>.
/// </summary>
public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.HasKey(cm => cm.Id);
        builder.Property(cm => cm.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(cm => cm.Role).IsRequired().HasMaxLength(20);
        builder.Property(cm => cm.Content).IsRequired().HasColumnType("text");
        builder.Property(cm => cm.ContentReferences).HasColumnType("uuid[]");
        builder.Property(cm => cm.PedagogicalIntent).HasMaxLength(50);
        builder.Property(cm => cm.SentimentScore).HasColumnType("decimal(3,2)");
        builder.Property(cm => cm.CreatedAt).HasDefaultValueSql("NOW()");

        builder.ToTable(t => t.HasCheckConstraint("CK_ConversationMessage_Role", "role IN ('student', 'tutor', 'system')"));

        builder.HasIndex(cm => new { cm.ThreadId, cm.CreatedAt });
    }
}
