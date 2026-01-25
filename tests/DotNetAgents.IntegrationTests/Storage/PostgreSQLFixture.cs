using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Npgsql;
using Xunit;

namespace DotNetAgents.IntegrationTests.Storage;

/// <summary>
/// Fixture for PostgreSQL integration tests.
/// Provides database connection and cleanup.
/// </summary>
public class PostgreSQLFixture : IAsyncLifetime
{
    private readonly string _connectionString;
    private readonly ILoggerFactory _loggerFactory;

    public PostgreSQLFixture()
    {
        // Get connection string from environment variable or use default for local testing
        _connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") 
            ?? "Host=localhost;Port=5432;Database=dotnetagents_test;Username=postgres;Password=postgres";
        
        _loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
    }

    public string ConnectionString => _connectionString;
    public ILoggerFactory LoggerFactory => _loggerFactory;

    public async Task InitializeAsync()
    {
        // Verify connection is available
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Cannot connect to PostgreSQL database. Please ensure PostgreSQL is running and connection string is correct. " +
                $"Connection string: {_connectionString}. Error: {ex.Message}", ex);
        }
    }

    public async Task DisposeAsync()
    {
        // Clean up test data
        await CleanupTestDataAsync();
        _loggerFactory.Dispose();
    }

    private async Task CleanupTestDataAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Clean up tables used by tests
            var cleanupCommands = new[]
            {
                "TRUNCATE TABLE agent_registry CASCADE",
                "TRUNCATE TABLE task_queue CASCADE",
                "TRUNCATE TABLE task_store CASCADE"
            };

            foreach (var command in cleanupCommands)
            {
                try
                {
                    await using var cmd = new NpgsqlCommand(command, connection);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (PostgresException ex) when (ex.SqlState == "42P01") // Table does not exist
                {
                    // Table doesn't exist yet, which is fine
                }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
