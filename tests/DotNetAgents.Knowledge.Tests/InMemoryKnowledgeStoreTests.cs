using DotNetAgents.Knowledge.Models;
using DotNetAgents.Knowledge.Storage;
using FluentAssertions;
using KnowledgeCategory = DotNetAgents.Knowledge.Models.KnowledgeCategory;
using KnowledgeSeverity = DotNetAgents.Knowledge.Models.KnowledgeSeverity;

namespace DotNetAgents.Knowledge.Tests;

public class InMemoryKnowledgeStoreTests
{
    private readonly InMemoryKnowledgeStore _store;

    public InMemoryKnowledgeStoreTests()
    {
        _store = new InMemoryKnowledgeStore();
    }

    [Fact]
    public async Task CreateAsync_WithValidKnowledge_CreatesKnowledge()
    {
        // Arrange
        var knowledge = new KnowledgeItem
        {
            SessionId = "session-123",
            Title = "Test knowledge",
            Description = "Test description",
            Category = KnowledgeCategory.Solution
        };

        // Act
        var result = await _store.CreateAsync(knowledge);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test knowledge");
        result.ContentHash.Should().NotBeNullOrWhiteSpace();
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_WithDefaultId_GeneratesNewId()
    {
        // Arrange
        var knowledge = new KnowledgeItem
        {
            Id = Guid.Empty,
            Title = "Test knowledge",
            Description = "Test description"
        };

        // Act
        var result = await _store.CreateAsync(knowledge);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_WithExistingContentHash_IndexesByHash()
    {
        // Arrange
        var knowledge1 = new KnowledgeItem
        {
            Title = "Test knowledge",
            Description = "Test description"
        };
        var created1 = await _store.CreateAsync(knowledge1);

        // Act
        var found = await _store.GetByContentHashAsync(created1.ContentHash!);

        // Assert
        found.Should().NotBeNull();
        found!.Id.Should().Be(created1.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsKnowledge()
    {
        // Arrange
        var knowledge = new KnowledgeItem
        {
            Title = "Test knowledge",
            Description = "Test description"
        };
        var created = await _store.CreateAsync(knowledge);

        // Act
        var result = await _store.GetByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Title.Should().Be("Test knowledge");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Act
        var result = await _store.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidKnowledge_UpdatesKnowledge()
    {
        // Arrange
        var knowledge = new KnowledgeItem
        {
            Title = "Original title",
            Description = "Original description",
            Category = KnowledgeCategory.Solution
        };
        var created = await _store.CreateAsync(knowledge);

        var updated = created with
        {
            Title = "Updated title",
            Description = "Updated description"
        };

        // Act
        var result = await _store.UpdateAsync(updated);

        // Assert
        result.Title.Should().Be("Updated title");
        result.UpdatedAt.Should().BeAfter(created.UpdatedAt);

        var retrieved = await _store.GetByIdAsync(created.Id);
        retrieved!.Title.Should().Be("Updated title");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_DeletesKnowledge()
    {
        // Arrange
        var knowledge = new KnowledgeItem
        {
            Title = "Test knowledge",
            Description = "Test description"
        };
        var created = await _store.CreateAsync(knowledge);

        // Act
        await _store.DeleteAsync(created.Id);

        // Assert
        var result = await _store.GetByIdAsync(created.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySessionIdAsync_WithValidSessionId_ReturnsKnowledge()
    {
        // Arrange
        var sessionId = "session-123";
        var knowledge1 = new KnowledgeItem { SessionId = sessionId, Title = "Knowledge 1", Description = "Desc 1" };
        var knowledge2 = new KnowledgeItem { SessionId = sessionId, Title = "Knowledge 2", Description = "Desc 2" };
        var knowledge3 = new KnowledgeItem { SessionId = "session-456", Title = "Knowledge 3", Description = "Desc 3" };

        await _store.CreateAsync(knowledge1);
        await _store.CreateAsync(knowledge2);
        await _store.CreateAsync(knowledge3);

        // Act
        var result = await _store.GetBySessionIdAsync(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Should().OnlyContain(k => k.SessionId == sessionId);
    }

    [Fact]
    public async Task GetGlobalKnowledgeAsync_ReturnsGlobalKnowledge()
    {
        // Arrange
        var global1 = new KnowledgeItem { SessionId = null, Title = "Global 1", Description = "Desc 1" };
        var global2 = new KnowledgeItem { SessionId = null, Title = "Global 2", Description = "Desc 2" };
        var session1 = new KnowledgeItem { SessionId = "session-123", Title = "Session 1", Description = "Desc 1" };

        await _store.CreateAsync(global1);
        await _store.CreateAsync(global2);
        await _store.CreateAsync(session1);

        // Act
        var result = await _store.GetGlobalKnowledgeAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Should().OnlyContain(k => k.SessionId == null);
    }

    [Fact]
    public async Task SearchAsync_WithMatchingText_ReturnsResults()
    {
        // Arrange
        var knowledge1 = new KnowledgeItem { Title = "Database connection", Description = "Connection pooling issue" };
        var knowledge2 = new KnowledgeItem { Title = "API endpoint", Description = "REST API implementation" };
        var knowledge3 = new KnowledgeItem { Title = "Database query", Description = "SQL optimization" };

        await _store.CreateAsync(knowledge1);
        await _store.CreateAsync(knowledge2);
        await _store.CreateAsync(knowledge3);

        // Act
        var result = await _store.SearchAsync("database");

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Should().OnlyContain(k => k.Title.Contains("Database", StringComparison.OrdinalIgnoreCase) ||
                                         k.Description.Contains("database", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task QueryAsync_WithCategoryFilter_ReturnsFilteredResults()
    {
        // Arrange
        var knowledge1 = new KnowledgeItem { Title = "Error 1", Description = "Desc", Category = KnowledgeCategory.Error };
        var knowledge2 = new KnowledgeItem { Title = "Solution 1", Description = "Desc", Category = KnowledgeCategory.Solution };
        var knowledge3 = new KnowledgeItem { Title = "Error 2", Description = "Desc", Category = KnowledgeCategory.Error };

        await _store.CreateAsync(knowledge1);
        await _store.CreateAsync(knowledge2);
        await _store.CreateAsync(knowledge3);

        var query = new KnowledgeQuery
        {
            Category = KnowledgeCategory.Error,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _store.QueryAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Items.Count.Should().Be(2);
        result.Items.Should().OnlyContain(k => k.Category == KnowledgeCategory.Error);
    }

    [Fact]
    public async Task IncrementReferenceCountAsync_WithValidId_IncrementsCount()
    {
        // Arrange
        var knowledge = new KnowledgeItem
        {
            Title = "Test knowledge",
            Description = "Test description",
            ReferenceCount = 5
        };
        var created = await _store.CreateAsync(knowledge);

        // Act
        await _store.IncrementReferenceCountAsync(created.Id);

        // Assert
        var updated = await _store.GetByIdAsync(created.Id);
        updated!.ReferenceCount.Should().Be(6);
        updated.LastReferencedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRelevantGlobalKnowledgeAsync_WithTechStackTags_ReturnsRelevantKnowledge()
    {
        // Arrange
        var knowledge1 = new KnowledgeItem
        {
            SessionId = null,
            Title = "DotNet knowledge",
            Description = "Description",
            TechStack = new[] { "dotnet", "csharp" },
            ReferenceCount = 10
        };
        var knowledge2 = new KnowledgeItem
        {
            SessionId = null,
            Title = "Python knowledge",
            Description = "Description",
            TechStack = new[] { "python" },
            ReferenceCount = 5
        };

        await _store.CreateAsync(knowledge1);
        await _store.CreateAsync(knowledge2);

        var techStackTags = new[] { "dotnet", "csharp" };

        // Act
        var result = await _store.GetRelevantGlobalKnowledgeAsync(techStackTags, null, 10);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThan(0);
        result.First().Title.Should().Be("DotNet knowledge");
    }
}
