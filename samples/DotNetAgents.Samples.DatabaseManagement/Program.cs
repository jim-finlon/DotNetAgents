using DotNetAgents.Database.Analysis;
using DotNetAgents.Database.AI;
using DotNetAgents.Database.Validation;
using DotNetAgents.Database.Orchestration;
using DotNetAgents.Database.Security;
using DotNetAgents.Security.Secrets;
using DotNetAgents.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetAgents.Samples.DatabaseManagement;

/// <summary>
/// Sample demonstrating database management capabilities in DotNetAgents.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("DotNetAgents - Database Management Example");
        Console.WriteLine("==========================================\n");

        // Setup services
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Add database services
        services.AddDatabaseSchemaAnalyzers();
        services.AddSchemaAnalyzerFactory();
        services.AddDatabaseValidation();
        services.AddDatabaseOrchestration();
        services.AddDatabaseSecurity();

        // Add secrets provider (required for database security)
        services.AddSingleton<ISecretsProvider, EnvironmentSecretsProvider>();

        // Add OpenAI for AI-powered features (optional)
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddOpenAI(apiKey, "gpt-3.5-turbo");
            services.AddDatabaseAI();
        }

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            // Demo 1: Schema Analysis
            await DemonstrateSchemaAnalysis(serviceProvider);

            // Demo 2: AI Query Optimization (if OpenAI is configured)
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                await DemonstrateAIQueryOptimization(serviceProvider);
            }

            // Demo 3: Database Validation
            await DemonstrateDatabaseValidation(serviceProvider);

            // Demo 4: Operation Orchestration
            await DemonstrateOperationOrchestration(serviceProvider);

            Console.WriteLine("\n✅ All demonstrations completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    static async Task DemonstrateSchemaAnalysis(IServiceProvider services)
    {
        Console.WriteLine("\n📊 Demo 1: Database Schema Analysis");
        Console.WriteLine("-----------------------------------");

        var factory = services.GetRequiredService<SchemaAnalyzerFactory>();
        
        // Get connection string from environment or .env file
        // For development, use Anubis server: 192.168.4.25
        // Connection string should be loaded from .env file or environment variables
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("⚠️  POSTGRES_CONNECTION_STRING not found in environment.");
            Console.WriteLine("   Please set it in your .env file or environment variables.");
            Console.WriteLine("   See .env.example for the format.");
            return;
        }

        var analyzer = await factory.GetAnalyzerAsync(connectionString);
        if (analyzer == null)
        {
            Console.WriteLine("⚠️  No compatible analyzer found for connection string");
            return;
        }

        Console.WriteLine($"Using analyzer: {analyzer.ProviderType}");

        try
        {
            var schema = await analyzer.AnalyzeAsync(connectionString);
            var stats = schema.GetStatistics();

            Console.WriteLine($"Database: {schema.Name}");
            Console.WriteLine($"Tables: {stats.TableCount}");
            Console.WriteLine($"Views: {stats.ViewCount}");
            Console.WriteLine($"Stored Procedures: {stats.StoredProcedureCount}");
            Console.WriteLine($"Functions: {stats.FunctionCount}");
            Console.WriteLine($"Total Objects: {stats.TotalObjectCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Schema analysis failed: {ex.Message}");
            Console.WriteLine("   (This is expected if the database is not accessible)");
        }
    }

    static async Task DemonstrateAIQueryOptimization(IServiceProvider services)
    {
        Console.WriteLine("\n🤖 Demo 2: AI Query Optimization");
        Console.WriteLine("--------------------------------");

        var optimizer = services.GetService<AIQueryOptimizer>();
        if (optimizer == null)
        {
            Console.WriteLine("⚠️  AI Query Optimizer not available (OpenAI not configured)");
            return;
        }

        var query = "SELECT * FROM users WHERE age > 25 ORDER BY name";
        Console.WriteLine($"Original Query: {query}");

        try
        {
            var result = await optimizer.OptimizeAsync(query);
            Console.WriteLine($"Optimized Query: {result.OptimizedQuery ?? result.OriginalQuery}");
            Console.WriteLine($"Confidence Score: {result.ConfidenceScore}%");
            Console.WriteLine($"Suggestions: {result.Suggestions.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Query optimization failed: {ex.Message}");
        }
    }

    static async Task DemonstrateDatabaseValidation(IServiceProvider services)
    {
        Console.WriteLine("\n✅ Demo 3: Database Validation");
        Console.WriteLine("-------------------------------");

        var validator = services.GetRequiredService<IDatabaseValidator>();
        
        // Get connection string from environment or .env file
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("⚠️  POSTGRES_CONNECTION_STRING not found in environment.");
            return;
        }

        try
        {
            var result = await validator.ValidateAsync(connectionString);
            Console.WriteLine($"Validation Result: {(result.IsValid ? "✅ Passed" : "❌ Failed")}");
            Console.WriteLine($"Checks Performed: {result.Checks.Count}");
            Console.WriteLine($"Passed: {result.PassedChecks}, Failed: {result.FailedChecks}");

            foreach (var check in result.Checks)
            {
                var status = check.Passed ? "✅" : "❌";
                Console.WriteLine($"  {status} {check.Name}: {check.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Validation failed: {ex.Message}");
        }
    }

    static async Task DemonstrateOperationOrchestration(IServiceProvider services)
    {
        Console.WriteLine("\n🎯 Demo 4: Operation Orchestration");
        Console.WriteLine("-----------------------------------");

        var orchestrator = services.GetRequiredService<IDatabaseOperationOrchestrator>();
        
        // Get connection string from environment or .env file
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("⚠️  POSTGRES_CONNECTION_STRING not found in environment.");
            return;
        }

        var operation = new DatabaseOperation
        {
            Type = "schema_analysis",
            TargetConnectionString = connectionString,
            Parameters = new Dictionary<string, object>
            {
                ["include_system_objects"] = false
            }
        };

        try
        {
            var result = await orchestrator.ExecuteAsync(operation, "demo-operation-1");
            Console.WriteLine($"Operation Result: {(result.Success ? "✅ Success" : "❌ Failed")}");
            Console.WriteLine($"Operation ID: {result.OperationId}");
            if (result.Errors.Count > 0)
            {
                Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Operation failed: {ex.Message}");
        }
    }
}
