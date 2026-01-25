using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="ConversationThread"/>.
/// </summary>
public class ConversationThreadConfiguration : IEntityTypeConfiguration<ConversationThread>
{
    public void Configure(EntityTypeBuilder<ConversationThread> builder)
    {
        builder.HasKey(ct => ct.Id);
        builder.Property(ct => ct.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ct => ct.Subject).HasConversion<string>();
        builder.Property(ct => ct.Summary).HasColumnType("text");
        builder.Property(ct => ct.FlagReason).HasColumnType("text");
        builder.Property(ct => ct.StartedAt).HasDefaultValueSql("NOW()");
        builder.Property(ct => ct.MessageCount).HasDefaultValue(0);
        builder.Property(ct => ct.FlaggedForReview).HasDefaultValue(false);
    }
}
