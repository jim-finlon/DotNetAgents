namespace DotNetAgents.IntegrationTests.Storage;

/// <summary>
/// Collection definition for PostgreSQL integration tests.
/// Ensures tests run sequentially to avoid database conflicts.
/// </summary>
[CollectionDefinition("PostgreSQL")]
public class PostgreSQLCollection : ICollectionFixture<PostgreSQLFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
