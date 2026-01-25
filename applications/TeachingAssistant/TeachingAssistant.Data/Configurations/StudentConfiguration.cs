using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data.Configurations;

/// <summary>
/// Entity Framework configuration for <see cref="Student"/>.
/// </summary>
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Birthdate).HasColumnType("date");
        builder.ToTable(t => t.HasCheckConstraint("CK_Student_GradeLevel", "grade_level >= 0 AND grade_level <= 12"));
        builder.Property(s => s.AvatarId).HasMaxLength(50);
        builder.Property(s => s.Preferences).HasColumnType("jsonb");
        builder.Property(s => s.AccessibilitySettings).HasColumnType("jsonb");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasQueryFilter(s => s.DeletedAt == null);

        builder.HasIndex(s => s.FamilyId)
            .HasFilter("[deleted_at] IS NULL");

        builder.HasMany(s => s.SubjectProgress)
            .WithOne(p => p.Student)
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.ContentMasteryRecords)
            .WithOne(m => m.Student)
            .HasForeignKey(m => m.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.LearningSessions)
            .WithOne(ls => ls.Student)
            .HasForeignKey(ls => ls.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
