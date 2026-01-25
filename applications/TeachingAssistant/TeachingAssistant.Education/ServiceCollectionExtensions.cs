using DotNetAgents.Education.Assessment;
using DotNetAgents.Education.Compliance;
using DotNetAgents.Education.Memory;
using DotNetAgents.Education.Pedagogy;
using DotNetAgents.Education.Retrieval;
using DotNetAgents.Education.Safety;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Education;

/// <summary>
/// Extension methods for registering DotNetAgents.Education services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds DotNetAgents.Education services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional action to configure education services.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddDotNetAgentsEducation(
        this IServiceCollection services,
        Action<EducationServiceOptions>? configure = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        var options = new EducationServiceOptions();
        configure?.Invoke(options);

        // Register pedagogy components
        services.AddScoped<ISocraticDialogueEngine, SocraticDialogueEngine>();
        services.AddScoped<ISpacedRepetitionScheduler, SM2Scheduler>();
        services.AddScoped<IMasteryCalculator, MasteryCalculator>();

        // Register safety components
        services.AddScoped<IContentFilter, ChildSafetyFilter>();
        services.AddScoped<IConversationMonitor, ConversationMonitor>();
        services.AddScoped<IAgeAdaptiveTransformer, AgeAdaptiveTransformer>();

        // Register assessment components
        services.AddScoped<IAssessmentGenerator, AssessmentGenerator>();
        services.AddScoped<IResponseEvaluator, ResponseEvaluator>();

        // Register memory components
        services.AddScoped<StudentProfileMemory>();
        services.AddScoped<MasteryStateMemory>();
        services.AddScoped<LearningSessionMemory>();

        // Register retrieval components
        services.AddScoped<ICurriculumAwareRetriever, CurriculumAwareRetriever>();
        services.AddScoped<IPrerequisiteChecker, PrerequisiteChecker>();

        // Register compliance components
        services.AddScoped<IEducationAuthorizationService, EducationAuthorizationService>();
        services.AddScoped<IFerpaComplianceService, FerpaComplianceService>();
        services.AddScoped<IGdprComplianceService, GdprComplianceService>();
        services.AddScoped<IEducationAuditLogger, EducationAuditLogger>();

        // Register infrastructure components
        services.AddSingleton<Infrastructure.ITenantContextProvider, Infrastructure.AsyncLocalTenantContextProvider>();
        services.AddScoped<Infrastructure.ITenantManager, Infrastructure.TenantManager>();
        services.AddScoped<Infrastructure.EducationContentCache>();

        return services;
    }

    /// <summary>
    /// Adds content pipeline supervisor services.
    /// </summary>
    public static IServiceCollection AddContentPipelineSupervisor(this IServiceCollection services)
    {
        // Register content pipeline supervisor
        services.AddScoped<ContentPipeline.ContentPipelineSupervisor>();

        return services;
    }
}

/// <summary>
/// Options for configuring DotNetAgents.Education services.
/// </summary>
public class EducationServiceOptions
{
    /// <summary>
    /// Gets or sets the prerequisite graph for concepts.
    /// </summary>
    public Dictionary<Models.ConceptId, IReadOnlyList<Models.ConceptId>>? PrerequisiteGraph { get; set; }

    /// <summary>
    /// Gets or sets whether to enable strict prerequisite checking.
    /// </summary>
    public bool StrictPrerequisites { get; set; } = true;
}
