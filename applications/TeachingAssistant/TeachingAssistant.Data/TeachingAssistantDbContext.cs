using Microsoft.EntityFrameworkCore;
using TeachingAssistant.Data.Configurations;
using TeachingAssistant.Data.Entities;

namespace TeachingAssistant.Data;

/// <summary>
/// Entity Framework Core database context for TeachingAssistant application.
/// </summary>
public class TeachingAssistantDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TeachingAssistantDbContext"/> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public TeachingAssistantDbContext(DbContextOptions<TeachingAssistantDbContext> options)
        : base(options)
    {
    }

    // Core entities
    /// <summary>
    /// Gets or sets the families.
    /// </summary>
    public DbSet<Family> Families => Set<Family>();

    /// <summary>
    /// Gets or sets the guardians.
    /// </summary>
    public DbSet<Guardian> Guardians => Set<Guardian>();

    /// <summary>
    /// Gets or sets the students.
    /// </summary>
    public DbSet<Student> Students => Set<Student>();

    // Content entities
    /// <summary>
    /// Gets or sets the content units.
    /// </summary>
    public DbSet<ContentUnit> ContentUnits => Set<ContentUnit>();

    /// <summary>
    /// Gets or sets the learning objectives.
    /// </summary>
    public DbSet<LearningObjective> LearningObjectives => Set<LearningObjective>();

    /// <summary>
    /// Gets or sets the prerequisites.
    /// </summary>
    public DbSet<Prerequisite> Prerequisites => Set<Prerequisite>();

    /// <summary>
    /// Gets or sets the content embeddings.
    /// </summary>
    public DbSet<ContentEmbedding> ContentEmbeddings => Set<ContentEmbedding>();

    // Assessment entities
    /// <summary>
    /// Gets or sets the assessments.
    /// </summary>
    public DbSet<Assessment> Assessments => Set<Assessment>();

    /// <summary>
    /// Gets or sets the assessment items.
    /// </summary>
    public DbSet<AssessmentItem> AssessmentItems => Set<AssessmentItem>();

    // Progress entities
    /// <summary>
    /// Gets or sets the student subject progress records.
    /// </summary>
    public DbSet<StudentSubjectProgress> StudentSubjectProgress => Set<StudentSubjectProgress>();

    /// <summary>
    /// Gets or sets the content mastery records.
    /// </summary>
    public DbSet<ContentMastery> ContentMastery => Set<ContentMastery>();

    /// <summary>
    /// Gets or sets the learning sessions.
    /// </summary>
    public DbSet<LearningSession> LearningSessions => Set<LearningSession>();

    /// <summary>
    /// Gets or sets the student responses.
    /// </summary>
    public DbSet<StudentResponse> StudentResponses => Set<StudentResponse>();

    // Conversation entities
    /// <summary>
    /// Gets or sets the conversation threads.
    /// </summary>
    public DbSet<ConversationThread> ConversationThreads => Set<ConversationThread>();

    /// <summary>
    /// Gets or sets the conversation messages.
    /// </summary>
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();

    // Audit entities
    /// <summary>
    /// Gets or sets the audit log entries.
    /// </summary>
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enable PostgreSQL extensions
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("pgcrypto");
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.HasPostgresExtension("pg_trgm"); // For trigram text search

        // Apply configurations
        modelBuilder.ApplyConfiguration(new FamilyConfiguration());
        modelBuilder.ApplyConfiguration(new GuardianConfiguration());
        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new ContentUnitConfiguration());
        modelBuilder.ApplyConfiguration(new LearningObjectiveConfiguration());
        modelBuilder.ApplyConfiguration(new PrerequisiteConfiguration());
        modelBuilder.ApplyConfiguration(new ContentEmbeddingConfiguration());
        modelBuilder.ApplyConfiguration(new AssessmentConfiguration());
        modelBuilder.ApplyConfiguration(new AssessmentItemConfiguration());
        modelBuilder.ApplyConfiguration(new StudentSubjectProgressConfiguration());
        modelBuilder.ApplyConfiguration(new ContentMasteryConfiguration());
        modelBuilder.ApplyConfiguration(new LearningSessionConfiguration());
        modelBuilder.ApplyConfiguration(new StudentResponseConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationThreadConfiguration());
        modelBuilder.ApplyConfiguration(new ConversationMessageConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogEntryConfiguration());

        // Configure PostgreSQL enums (these will be created in migrations)
        // Note: EF Core will automatically create enum types in PostgreSQL when using HasConversion<string>()
        // For production, you may want to create these explicitly via raw SQL in migrations
    }
}
