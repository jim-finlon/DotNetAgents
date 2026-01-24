using DotNetAgents.Abstractions.Documents;
using DotNetAgents.Core.Documents;
using DotNetAgents.Documents.Loaders;
using Xunit;

namespace DotNetAgents.Core.Tests.Documents.Loaders;

public class RecursiveTextSplitterTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var splitter = new RecursiveTextSplitter(chunkSize: 100, chunkOverlap: 20);

        // Assert
        Assert.NotNull(splitter);
    }

    [Fact]
    public void Constructor_WithInvalidChunkSize_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RecursiveTextSplitter(chunkSize: 0));
        Assert.Throws<ArgumentException>(() => new RecursiveTextSplitter(chunkSize: -1));
    }

    [Fact]
    public void Constructor_WithInvalidOverlap_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RecursiveTextSplitter(chunkSize: 100, chunkOverlap: -1));
        Assert.Throws<ArgumentException>(() => new RecursiveTextSplitter(chunkSize: 100, chunkOverlap: 100));
    }

    [Fact]
    public async Task SplitTextAsync_WithShortText_ReturnsSingleChunk()
    {
        // Arrange
        var splitter = new RecursiveTextSplitter(chunkSize: 1000);
        var text = "Short text";

        // Act
        var chunks = await splitter.SplitTextAsync(text);

        // Assert
        Assert.Single(chunks);
        Assert.Equal(text, chunks[0]);
    }

    [Fact]
    public async Task SplitTextAsync_WithLongText_SplitsIntoMultipleChunks()
    {
        // Arrange
        var splitter = new RecursiveTextSplitter(chunkSize: 50, chunkOverlap: 10);
        var text = new string('a', 200); // 200 characters

        // Act
        var chunks = await splitter.SplitTextAsync(text);

        // Assert
        Assert.True(chunks.Count > 1);
    }

    [Fact]
    public async Task SplitTextAsync_WithParagraphSeparators_SplitsAtParagraphs()
    {
        // Arrange
        var splitter = new RecursiveTextSplitter(chunkSize: 100);
        var text = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.";

        // Act
        var chunks = await splitter.SplitTextAsync(text);

        // Assert
        Assert.True(chunks.Count >= 2);
    }

    [Fact]
    public async Task SplitDocumentsAsync_WithMultipleDocuments_SplitsAll()
    {
        // Arrange
        var splitter = new RecursiveTextSplitter(chunkSize: 50);
        var documents = new[]
        {
            new Document { Content = new string('a', 100) },
            new Document { Content = new string('b', 100) }
        };

        // Act
        var chunks = await splitter.SplitDocumentsAsync(documents);

        // Assert
        Assert.True(chunks.Count > 2);
    }

    [Fact]
    public async Task SplitTextAsync_WithCustomSeparators_UsesCustomSeparators()
    {
        // Arrange
        var separators = new[] { "|", "---", "" };
        var splitter = new RecursiveTextSplitter(
            chunkSize: 50,
            chunkOverlap: 10,
            separators: separators);
        var text = "Part1|Part2|Part3";

        // Act
        var chunks = await splitter.SplitTextAsync(text);

        // Assert
        Assert.True(chunks.Count >= 2);
    }

    [Fact]
    public async Task SplitTextAsync_WithEmptyText_ReturnsEmptyList()
    {
        // Arrange
        var splitter = new RecursiveTextSplitter();

        // Act
        var chunks = await splitter.SplitTextAsync("");

        // Assert
        Assert.Empty(chunks);
    }

    [Fact]
    public async Task SplitDocumentsAsync_PreservesMetadata()
    {
        // Arrange
        var splitter = new RecursiveTextSplitter(chunkSize: 50);
        var document = new Document
        {
            Content = new string('a', 100),
            Metadata = new Dictionary<string, object> { ["source"] = "test" }
        };

        // Act
        var chunks = await splitter.SplitDocumentsAsync(new[] { document });

        // Assert
        Assert.All(chunks, chunk => Assert.Equal("test", chunk.Metadata["source"]));
    }
}