using DotNetAgents.Configuration;
using DotNetAgents.Core.Models;
using DotNetAgents.Education.Assessment;
using DotNetAgents.Education.Memory;
using DotNetAgents.Education.Models;
using DotNetAgents.Education.Pedagogy;
using DotNetAgents.Education.Safety;
using DotNetAgents.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Samples.Education;

/// <summary>
/// Sample application demonstrating DotNetAgents.Education features.
/// This sample shows how to use pedagogy components, safety filters, assessment tools,
/// and memory management for educational AI applications.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents.Education Sample Application");
        Console.WriteLine("==========================================\n");

        // Build service collection
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());

        // Add DotNetAgents Core
        services.AddDotNetAgents(config =>
        {
            config.WithDefaultLLMProvider("OpenAI");
        });

        // Add OpenAI provider
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Warning: OPENAI_API_KEY environment variable not set. Some features may not work.");
            Console.WriteLine("Please set OPENAI_API_KEY to your OpenAI API key.\n");
        }
        else
        {
            services.AddOpenAI(options =>
            {
                options.ApiKey = apiKey;
                options.Model = "gpt-4o-mini"; // Use a cheaper model for demos
            });
        }

        // Add Education extensions
        services.AddDotNetAgentsEducation();

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // Demo 1: Socratic Dialogue
            await DemonstrateSocraticDialogue(serviceProvider, logger);

            // Demo 2: Spaced Repetition
            await DemonstrateSpacedRepetition(serviceProvider, logger);

            // Demo 3: Mastery Calculation
            await DemonstrateMasteryCalculation(serviceProvider, logger);

            // Demo 4: Content Filtering
            await DemonstrateContentFiltering(serviceProvider, logger);

            // Demo 5: Assessment Generation
            await DemonstrateAssessmentGeneration(serviceProvider, logger);

            // Demo 6: Student Profile Management
            await DemonstrateStudentProfile(serviceProvider, logger);

            Console.WriteLine("\n✅ All demonstrations completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running sample application");
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    static async Task DemonstrateSocraticDialogue(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n📚 Demo 1: Socratic Dialogue Engine");
        Console.WriteLine("------------------------------------");

        var llm = services.GetService<ILLMModel<ChatMessage[], ChatMessage>>();
        if (llm == null)
        {
            Console.WriteLine("⚠️  LLM provider not available (requires API key)");
            Console.WriteLine("   SocraticDialogueEngine requires an LLM to generate questions.");
            return;
        }

        var engine = new SocraticDialogueEngine(llm, null, logger);
        var concept = new ConceptContext(
            new ConceptId("photosynthesis", SubjectArea.Science, GradeLevel.G6_8),
            "Science",
            GradeLevel.G6_8
        );

        var understanding = new StudentUnderstanding(
            MasteryLevel.Proficient,
            0.7,
            Array.Empty<string>()
        );

        try
        {
            var question = await engine.GenerateQuestionAsync(concept, understanding);
            Console.WriteLine($"Question: {question.Text}");
            Console.WriteLine($"Type: {question.Type}");
            Console.WriteLine($"Difficulty: {question.Difficulty:P0}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not generate Socratic question");
            Console.WriteLine($"⚠️  Error: {ex.Message}");
        }
    }

    static async Task DemonstrateSpacedRepetition(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n🔄 Demo 2: Spaced Repetition (SM2 Algorithm)");
        Console.WriteLine("--------------------------------------------");

        var scheduler = services.GetRequiredService<ISpacedRepetitionScheduler>();

        var item = new ReviewItem(
            "item-1",
            new ConceptId("fractions", SubjectArea.Mathematics, GradeLevel.G3_5),
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow,
            2.5,
            0
        );

        var schedule = scheduler.CalculateNextReview(item, PerformanceRating.CorrectWithHesitation);
        Console.WriteLine($"Next review: {schedule.Item.NextReview:yyyy-MM-dd}");
        Console.WriteLine($"Interval: {schedule.IntervalDays} days");
        Console.WriteLine($"Ease Factor: {schedule.Item.EaseFactor:F2}");
        Console.WriteLine($"Repetition Count: {schedule.Item.RepetitionCount}");

        var retention = scheduler.CalculateRetention(schedule.Item, DateTimeOffset.UtcNow);
        Console.WriteLine($"Estimated Retention: {retention:P0}");
    }

    static async Task DemonstrateMasteryCalculation(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n📊 Demo 3: Mastery Calculator");
        Console.WriteLine("-----------------------------");

        var calculator = services.GetRequiredService<IMasteryCalculator>();

        var concept = new ConceptId("addition", SubjectArea.Mathematics, GradeLevel.K2);
        var history = new List<AssessmentResult>
        {
            new() { Score = 70, Timestamp = DateTimeOffset.UtcNow.AddDays(-7), AssessmentId = "assess-1" },
            new() { Score = 80, Timestamp = DateTimeOffset.UtcNow.AddDays(-3), AssessmentId = "assess-2" },
            new() { Score = 85, Timestamp = DateTimeOffset.UtcNow.AddDays(-1), AssessmentId = "assess-3" }
        };

        var mastery = calculator.CalculateMastery(concept, history);
        Console.WriteLine($"Concept: {concept.Value}");
        Console.WriteLine($"Mastery Level: {mastery}");
        Console.WriteLine($"Assessment History: {history.Count} assessments");

        var studentMastery = new Dictionary<ConceptId, MasteryLevel>
        {
            [concept] = mastery
        };

        var targetConcept = new ConceptId("subtraction", SubjectArea.Mathematics, GradeLevel.K2);
        var meetsPrereqs = calculator.MeetsPrerequisites(targetConcept, studentMastery);
        Console.WriteLine($"Ready for '{targetConcept.Value}': {meetsPrereqs}");
    }

    static async Task DemonstrateContentFiltering(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n🛡️  Demo 4: Content Filtering");
        Console.WriteLine("------------------------------");

        var filter = services.GetRequiredService<IContentFilter>();
        var context = new FilterContext(
            "student-123",
            "conversation-456",
            isInput: true
        );

        var testInput = "I want to learn about photosynthesis";
        var result = await filter.FilterInputAsync(testInput, context);

        Console.WriteLine($"Input: {testInput}");
        Console.WriteLine($"Allowed: {result.IsAllowed}");
        Console.WriteLine($"Filtered: {result.FilteredContent}");
        Console.WriteLine($"Flagged Categories: {string.Join(", ", result.FlaggedCategories)}");
        Console.WriteLine($"Requires Review: {result.RequiresReview}");
    }

    static async Task DemonstrateAssessmentGeneration(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n📝 Demo 5: Assessment Generation");
        Console.WriteLine("--------------------------------");

        var llm = services.GetService<ILLMModel<ChatMessage[], ChatMessage>>();
        if (llm == null)
        {
            Console.WriteLine("⚠️  LLM provider not available (requires API key)");
            Console.WriteLine("   AssessmentGenerator requires an LLM to generate questions.");
            return;
        }

        var generator = new AssessmentGenerator(llm, logger);
        var concept = new ConceptId("fractions", SubjectArea.Mathematics, GradeLevel.G3_5);
        var spec = new AssessmentSpecification
        {
            ConceptId = concept,
            QuestionCount = 3,
            QuestionTypes = new[] { QuestionType.MultipleChoice, QuestionType.ShortAnswer },
            GradeLevel = GradeLevel.G3_5
        };

        try
        {
            var assessment = await generator.GenerateAsync(concept, spec);

            Console.WriteLine($"Assessment: {assessment.Title}");
            Console.WriteLine($"Questions: {assessment.Questions.Count}");
            foreach (var question in assessment.Questions)
            {
                Console.WriteLine($"  - {question.QuestionText} ({question.Type})");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not generate assessment");
            Console.WriteLine($"⚠️  Error: {ex.Message}");
        }
    }

    static async Task DemonstrateStudentProfile(IServiceProvider services, ILogger logger)
    {
        Console.WriteLine("\n👤 Demo 6: Student Profile Management");
        Console.WriteLine("--------------------------------------");

        var profileMemory = services.GetRequiredService<StudentProfileMemory>();

        var profile = new StudentProfile
        {
            StudentId = "demo-student-1",
            Name = "Demo Student",
            GradeLevel = GradeLevel.G6_8,
            Age = 12,
            LearningPreferences = new[] { "Visual", "Kinesthetic" },
            AcademicGoals = new[] { "Improve math skills", "Learn science concepts" },
            Strengths = new[] { "Problem solving" },
            Weaknesses = new[] { "Algebra" }
        };

        await profileMemory.SaveProfileAsync(profile);
        Console.WriteLine($"Created profile for: {profile.Name}");

        var retrieved = await profileMemory.GetProfileAsync("demo-student-1");
        if (retrieved != null)
        {
            Console.WriteLine($"Retrieved: {retrieved.Name}");
            Console.WriteLine($"Grade Level: {retrieved.GradeLevel}");
            Console.WriteLine($"Learning Preferences: {string.Join(", ", retrieved.LearningPreferences)}");
        }
    }
}

/// <summary>
/// Extension method to check if a service is registered.
/// </summary>
static class ServiceProviderExtensions
{
    public static bool HasService<T>(this IServiceProvider services) where T : class
    {
        try
        {
            return services.GetService<T>() != null;
        }
        catch
        {
            return false;
        }
    }
}
