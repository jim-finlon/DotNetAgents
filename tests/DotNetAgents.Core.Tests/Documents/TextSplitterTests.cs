using DotNetAgents.Abstractions.Documents;
using DotNetAgents.Core.Documents;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Documents;

public class CharacterTextSplitterTests
{
    [Fact]
    public async Task SplitAsync_WithShortText_ReturnsSingleChunk()
    {
        // Arrange
        var splitter = new CharacterTextSplitter(chunkSize: 1000, chunkOverlap: 0);
        var document = new Document { Content = "Short text" };

        // Act
        var chunks = await splitter.SplitAsync(document);

        // Assert
        chunks.Should().HaveCount(1);
        chunks[0].Content.Should().Be("Short text");
    }

    [Fact]
    public async Task SplitAsync_WithLongText_SplitsIntoMultipleChunks()
    {
        // Arrange
        var splitter = new CharacterTextSplitter(chunkSize: 10, chunkOverlap: 2);
        var document = new Document { Content = "This is a very long text that should be split" };

        // Act
        var chunks = await splitter.SplitAsync(document);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        chunks.All(c => c.Content.Length <= 10).Should().BeTrue();
    }

    [Fact]
    public async Task SplitAsync_WithOverlap_IncludesOverlapInNextChunk()
    {
        // Arrange
        var splitter = new CharacterTextSplitter(chunkSize: 10, chunkOverlap: 3);
        var document = new Document { Content = "12345678901234567890" };

        // Act
        var chunks = await splitter.SplitAsync(document);

        // Assert
        chunks.Should().HaveCountGreaterThan(1);
        // Verify overlap exists between chunks
        if (chunks.Count > 1)
        {
            var firstChunkEnd = chunks[0].Content.Substring(chunks[0].Content.Length - 3);
            var secondChunkStart = chunks[1].Content.Substring(0, Math.Min(3, chunks[1].Content.Length));
            firstChunkEnd.Should().Be(secondChunkStart);
        }
    }

    [Fact]
    public async Task SplitDocumentsAsync_WithMultipleDocuments_SplitsAll()
    {
        // Arrange
        var splitter = new CharacterTextSplitter(chunkSize: 10, chunkOverlap: 0);
        var documents = new[]
        {
            new Document { Content = "First document" },
            new Document { Content = "Second document" }
        };

        // Act
        var chunks = await splitter.SplitDocumentsAsync(documents);

        // Assert
        chunks.Should().HaveCountGreaterThan(2);
    }

    [Fact]
    public void Constructor_WithInvalidChunkSize_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CharacterTextSplitter(chunkSize: 0));
        Assert.Throws<ArgumentException>(() => new CharacterTextSplitter(chunkSize: -1));
    }

    [Fact]
    public void Constructor_WithOverlapGreaterThanChunkSize_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new CharacterTextSplitter(chunkSize: 10, chunkOverlap: 11));
    }
}