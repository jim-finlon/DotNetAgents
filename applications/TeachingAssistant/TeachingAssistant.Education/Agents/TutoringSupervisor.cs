using DotNetAgents.Education.RAG;
using DotNetAgents.Education.Models;
using TeachingAssistant.AI;
using TeachingAssistant.Data.Entities;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education.Agents;

/// <summary>
/// Supervisor agent that routes tutoring queries to specialized subject tutors.
/// </summary>
public class TutoringSupervisor
{
    private readonly Dictionary<Subject, SubjectTutorAgent> _tutorAgents;
    private readonly CurriculumRagService _ragService;
    private readonly IModelRouter _modelRouter;
    private readonly ILogger<TutoringSupervisor> _logger;

    public TutoringSupervisor(
        Dictionary<Subject, SubjectTutorAgent> tutorAgents,
        CurriculumRagService ragService,
        IModelRouter modelRouter,
        ILogger<TutoringSupervisor> logger)
    {
        _tutorAgents = tutorAgents ?? throw new ArgumentNullException(nameof(tutorAgents));
        _ragService = ragService ?? throw new ArgumentNullException(nameof(ragService));
        _modelRouter = modelRouter ?? throw new ArgumentNullException(nameof(modelRouter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Routes a student query to the appropriate subject tutor.
    /// </summary>
    public async Task<TutorResponse> RouteQueryAsync(
        string studentId,
        string query,
        Subject? subject = null,
        GradeBand? gradeBand = null,
        CancellationToken cancellationToken = default)
    {
        // If subject not specified, try to infer from query using RAG
        if (!subject.HasValue)
        {
            subject = await InferSubjectFromQueryAsync(query, cancellationToken);
        }

        if (!subject.HasValue)
        {
            _logger.LogWarning("Could not determine subject for query: {Query}", query);
            return new TutorResponse(
                studentId,
                "I need to know which subject you're asking about. Please specify Biology, Chemistry, Physics, etc.",
                null,
                null,
                null,
                DateTimeOffset.UtcNow);
        }

        // Route to appropriate subject tutor
        if (_tutorAgents.TryGetValue(subject.Value, out var tutor))
        {
            return await tutor.HandleQueryAsync(studentId, query, gradeBand, cancellationToken);
        }

        _logger.LogWarning("No tutor agent found for subject {Subject}", subject);
        return new TutorResponse(
            studentId,
            $"I'm sorry, I don't have a tutor available for {subject} right now.",
            null,
            null,
            null,
            DateTimeOffset.UtcNow);
    }

    private async Task<Subject?> InferSubjectFromQueryAsync(string query, CancellationToken cancellationToken)
    {
        // Use RAG to search across all subjects and determine most relevant
        // For now, simplified - would use semantic search
        var subjects = Enum.GetValues<Subject>();
        foreach (var subject in subjects)
        {
            // Quick keyword check
            if (query.Contains(subject.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return subject;
            }
        }

        return null;
    }
}
