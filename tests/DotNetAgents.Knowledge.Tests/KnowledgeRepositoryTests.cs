using DotNetAgents.Knowledge.Models;
using DotNetAgents.Knowledge.Storage;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using KnowledgeCategory = DotNetAgents.Knowledge.Models.KnowledgeCategory;
using KnowledgeSeverity = DotNetAgents.Knowledge.Models.KnowledgeSeverity;

namespace DotNetAgents.Knowledge.Tests;

public class KnowledgeRepositoryTests
{
    private readonly Mock<IKnowledgeStore> _knowledgeStoreMock;
    private readonly Mock<ILogger<KnowledgeRepository>> _loggerMock;
    private readonly KnowledgeRepository _repository;

    public KnowledgeRepositoryTests()
    {
        _knowledgeStoreMock = new Mock<IKnowledgeStore>();
        _loggerMock = new Mock<ILogger<KnowledgeRepository>>();
        _repository = new KnowledgeRepository(_knowledgeStoreMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AddKnowledgeAsync_WithValidKnowledge_ReturnsAddedKnowledge()
    {
        // Arrange
        var knowledge = new KnowledgeItem
        {
            SessionId = "session-123",
            Title = "Test knowledge",
            Description = "Test description",
            Category = KnowledgeCategory.Solution
        };

        var addedKnowledge = knowledge with { Id = Guid.NewGuid() };
        _knowledgeStoreMock.Setup(x => x.CreateAsync(knowledge, It.IsAny<CancellationToken>()))
            .ReturnsAsync(addedKnowledge);

        // Act
        var result = await _repository.AddKnowledgeAsync(knowledge);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test knowledge");
        _knowledgeStoreMock.Verify(x => x.CreateAsync(knowledge, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddKnowledgeAsync_WithNullKnowledge_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _repository.AddKnowledgeAsync(null!));
    }

    [Fact]
    public async Task AddKnowledgeAsync_WithEmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var knowledge = new KnowledgeItem
        {
            Title = string.Empty,
            Description = "Test description"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _repository.AddKnowledgeAsync(knowledge));
    }

    [Fact]
    public async Task GetKnowledgeAsync_WithValidId_ReturnsKnowledge()
    {
        // Arrange
        var knowledgeId = Guid.NewGuid();
        var knowledge = new KnowledgeItem
        {
            Id = knowledgeId,
            Title = "Test knowledge",
            Description = "Test description"
        };

        _knowledgeStoreMock.Setup(x => x.GetByIdAsync(knowledgeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(knowledge);

        // Act
        var result = await _repository.GetKnowledgeAsync(knowledgeId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(knowledgeId);
        _knowledgeStoreMock.Verify(x => x.GetByIdAsync(knowledgeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchKnowledgeAsync_WithValidSearchText_ReturnsResults()
    {
        // Arrange
        var searchText = "test";
        var results = new List<KnowledgeItem>
        {
            new KnowledgeItem { Id = Guid.NewGuid(), Title = "Test knowledge 1", Description = "Description" },
            new KnowledgeItem { Id = Guid.NewGuid(), Title = "Test knowledge 2", Description = "Description" }
        };

        _knowledgeStoreMock.Setup(x => x.SearchAsync(searchText, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(results);

        // Act
        var result = await _repository.SearchKnowledgeAsync(searchText);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        _knowledgeStoreMock.Verify(x => x.SearchAsync(searchText, null, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRelevantKnowledgeAsync_WithTechStackTags_ReturnsRelevantKnowledge()
    {
        // Arrange
        var techStackTags = new[] { "dotnet", "csharp" };
        var projectTags = new[] { "api" };
        var results = new List<KnowledgeItem>
        {
            new KnowledgeItem
            {
                Id = Guid.NewGuid(),
                Title = "Relevant knowledge",
                Description = "Description",
                TechStack = new[] { "dotnet", "csharp" },
                Tags = new[] { "api" }
            }
        };

        _knowledgeStoreMock.Setup(x => x.GetRelevantGlobalKnowledgeAsync(
                techStackTags,
                projectTags,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(results);

        // Act
        var result = await _repository.GetRelevantKnowledgeAsync(techStackTags, projectTags, 10);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
    }

    [Fact]
    public async Task FindDuplicateAsync_WithExistingContent_ReturnsDuplicate()
    {
        // Arrange
        var title = "Test knowledge";
        var description = "Test description";
        var duplicate = new KnowledgeItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description
        };

        // Calculate the actual content hash that will be used
        var contentHash = Helpers.ContentHashHelper.CalculateContentHash(title, description);

        _knowledgeStoreMock.Setup(x => x.GetByContentHashAsync(contentHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicate);

        // Act
        var result = await _repository.FindDuplicateAsync(title, description);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(title);
    }

    [Fact]
    public async Task FindDuplicateAsync_WithNonExistentContent_ReturnsNull()
    {
        // Arrange
        var title = "New knowledge";
        var description = "New description";
        var contentHash = "new-hash";

        _knowledgeStoreMock.Setup(x => x.GetByContentHashAsync(contentHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((KnowledgeItem?)null);

        // Act
        var result = await _repository.FindDuplicateAsync(title, description);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IncrementReferenceCountAsync_WithValidId_IncrementsCount()
    {
        // Arrange
        var knowledgeId = Guid.NewGuid();

        // Act
        await _repository.IncrementReferenceCountAsync(knowledgeId);

        // Assert
        _knowledgeStoreMock.Verify(x => x.IncrementReferenceCountAsync(knowledgeId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
