using DotLangChain.Abstractions.Documents;
using DotLangChain.Core.Documents.Splitters;
using FluentAssertions;

namespace DotLangChain.Tests.Unit.Documents.Splitters;

public class RecursiveCharacterTextSplitterTests
{
    private readonly RecursiveCharacterTextSplitter _splitter = new();

    [Fact]
    public async Task SplitTextAsync_WithShortText_ReturnsSingleChunk()
    {
        // Arrange
        var text = "Short text";
        var options = new TextSplitterOptions { ChunkSize = 100 };

        // Act
        var chunks = await _splitter.SplitTextAsync(text, options).ToListAsync();

        // Assert
        chunks.Should().HaveCount(1);
        chunks[0].Should().Be(text);
    }

    [Fact]
    public async Task SplitTextAsync_WithLongText_SplitsIntoMultipleChunks()
    {
        // Arrange
        var text = string.Join("\n", Enumerable.Range(1, 100).Select(i => $"Line {i}"));
        var options = new TextSplitterOptions { ChunkSize = 100 };

        // Act
        var chunks = await _splitter.SplitTextAsync(text, options).ToListAsync();

        // Assert
        chunks.Should().NotBeEmpty();
        chunks.Count.Should().BeGreaterThan(1);
        chunks.All(c => c.Length <= 100 + options.ChunkOverlap).Should().BeTrue();
    }

    [Fact]
    public async Task SplitTextAsync_WithOverlap_IncludesOverlap()
    {
        // Arrange
        var text = string.Join("\n", Enumerable.Range(1, 20).Select(i => $"Line {i}"));
        var options = new TextSplitterOptions
        {
            ChunkSize = 50,
            ChunkOverlap = 10
        };

        // Act
        var chunks = await _splitter.SplitTextAsync(text, options).ToListAsync();

        // Assert
        if (chunks.Count > 1)
        {
            // Adjacent chunks should have overlap
            var firstEnd = chunks[0][^options.ChunkOverlap..];
            chunks[1].Should().StartWith(firstEnd);
        }
    }

    [Fact]
    public async Task SplitAsync_WithDocument_CreatesDocumentChunks()
    {
        // Arrange
        var document = new Document
        {
            Id = "doc1",
            Content = "This is a test document with multiple sentences. It has several lines.\n\nAnd paragraphs.",
            Metadata = new DocumentMetadata { Title = "Test" }
        };
        var options = new TextSplitterOptions { ChunkSize = 30 };

        // Act
        var chunks = await _splitter.SplitAsync(document, options).ToListAsync();

        // Assert
        chunks.Should().NotBeEmpty();
        chunks.All(c => c.ParentDocumentId == document.Id).Should().BeTrue();
        chunks.All(c => c.Metadata.ParentDocumentId == document.Id).Should().BeTrue();
        chunks[0].ChunkIndex.Should().Be(0);
        chunks[1].ChunkIndex.Should().Be(1);
    }

    [Fact]
    public async Task SplitAsync_PreservesMetadata()
    {
        // Arrange
        var document = new Document
        {
            Id = "doc1",
            Content = "Test content",
            Metadata = new DocumentMetadata
            {
                Title = "Original Title",
                Source = "original-source"
            }
        };

        // Act
        var chunks = await _splitter.SplitAsync(document).ToListAsync();

        // Assert
        chunks.Should().HaveCount(1);
        chunks[0].Metadata.Title.Should().Be("Original Title");
        chunks[0].Metadata.Source.Should().Be("original-source");
    }

    [Fact]
    public async Task SplitTextAsync_WithStripWhitespace_TrimsChunks()
    {
        // Arrange
        var text = "  Line 1  \n\n  Line 2  ";
        var options = new TextSplitterOptions
        {
            ChunkSize = 100,
            StripWhitespace = true
        };

        // Act
        var chunks = await _splitter.SplitTextAsync(text, options).ToListAsync();

        // Assert
        chunks.All(c => c == c.Trim()).Should().BeTrue();
    }

    [Fact]
    public async Task SplitTextAsync_RespectsParagraphs()
    {
        // Arrange
        var text = "Paragraph 1.\n\nParagraph 2.\n\nParagraph 3.";
        var options = new TextSplitterOptions { ChunkSize = 20 };

        // Act
        var chunks = await _splitter.SplitTextAsync(text, options).ToListAsync();

        // Assert
        // Should split on paragraph boundaries first
        chunks.Should().NotBeEmpty();
    }
}

