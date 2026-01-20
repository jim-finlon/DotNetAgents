using DotNetAgents.Core.Documents;
using DotNetAgents.Core.Documents.Loaders;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Documents.Loaders;

public class MarkdownDocumentLoaderTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    [Fact]
    public async Task LoadAsync_WithFilepath_LoadsFileContent()
    {
        // Arrange
        var loader = new MarkdownDocumentLoader();
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".md");
        _tempFiles.Add(tempFile);
        var expectedContent = "# Test Markdown\n\nThis is **bold** text.";
        await File.WriteAllTextAsync(tempFile, expectedContent);

        // Act
        var documents = await loader.LoadAsync(tempFile);

        // Assert
        documents.Should().HaveCount(1);
        documents[0].Content.Should().Be(expectedContent);
        documents[0].Metadata.Should().ContainKey("source");
        documents[0].Metadata.Should().ContainKey("filename");
        documents[0].Metadata.Should().ContainKey("type");
        documents[0].Metadata["type"].Should().Be("markdown");
    }

    [Fact]
    public async Task LoadAsync_WithRawContent_TreatsAsContent()
    {
        // Arrange
        var loader = new MarkdownDocumentLoader();
        var rawContent = "# Raw Markdown\n\nThis is raw content.";

        // Act
        var documents = await loader.LoadAsync(rawContent);

        // Assert
        documents.Should().HaveCount(1);
        documents[0].Content.Should().Be(rawContent);
        documents[0].Metadata["source"].Should().Be("inline");
        documents[0].Metadata["type"].Should().Be("markdown");
    }

    [Fact]
    public async Task LoadAsync_WithNullSource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new MarkdownDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(null!));
    }

    [Fact]
    public async Task LoadAsync_WithEmptySource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new MarkdownDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(string.Empty));
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync("   "));
    }

    [Fact]
    public async Task LoadAsync_WithNonExistentFile_TreatsAsRawContent()
    {
        // Arrange
        var loader = new MarkdownDocumentLoader();
        var content = "# This is treated as raw markdown content";

        // Act
        var documents = await loader.LoadAsync(content);

        // Assert
        documents.Should().HaveCount(1);
        documents[0].Content.Should().Be(content);
    }

    [Fact]
    public async Task LoadAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var loader = new MarkdownDocumentLoader();
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".md");
        _tempFiles.Add(tempFile);
        await File.WriteAllTextAsync(tempFile, "# Test");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => loader.LoadAsync(tempFile, cts.Token));
    }

    [Fact]
    public async Task LoadAsync_WithComplexMarkdown_PreservesFormatting()
    {
        // Arrange
        var loader = new MarkdownDocumentLoader();
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".md");
        _tempFiles.Add(tempFile);
        var markdownContent = @"# Title

## Subtitle

- List item 1
- List item 2

```csharp
var code = ""example"";
```

**Bold** and *italic* text.";
        await File.WriteAllTextAsync(tempFile, markdownContent);

        // Act
        var documents = await loader.LoadAsync(tempFile);

        // Assert
        documents.Should().HaveCount(1);
        documents[0].Content.Should().Be(markdownContent);
        documents[0].Content.Should().Contain("Title");
        documents[0].Content.Should().Contain("```csharp");
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}