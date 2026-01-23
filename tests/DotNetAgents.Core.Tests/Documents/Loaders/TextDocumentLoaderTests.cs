using DotNetAgents.Core.Documents;
using DotNetAgents.Core.Documents.Loaders;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Documents.Loaders;

public class TextDocumentLoaderTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    [Fact]
    public async Task LoadAsync_WithFilepath_LoadsFileContent()
    {
        // Arrange
        var loader = new TextDocumentLoader();
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);
        var expectedContent = "This is test content\nWith multiple lines";
        await File.WriteAllTextAsync(tempFile, expectedContent);

        // Act
        var documents = await loader.LoadAsync(tempFile);

        // Assert
        documents.Should().HaveCount(1);
        documents[0].Content.Should().Be(expectedContent);
        documents[0].Metadata.Should().ContainKey("source");
        documents[0].Metadata.Should().ContainKey("filename");
        documents[0].Metadata.Should().ContainKey("type");
        documents[0].Metadata["type"].Should().Be("text");
    }

    [Fact]
    public async Task LoadAsync_WithRawContent_TreatsAsContent()
    {
        // Arrange
        var loader = new TextDocumentLoader();
        var rawContent = "This is raw text content";

        // Act
        var documents = await loader.LoadAsync(rawContent);

        // Assert
        documents.Should().HaveCount(1);
        documents[0].Content.Should().Be(rawContent);
        documents[0].Metadata["source"].Should().Be("inline");
        documents[0].Metadata["type"].Should().Be("text");
    }

    [Fact]
    public async Task LoadAsync_WithNullSource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new TextDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(null!));
    }

    [Fact]
    public async Task LoadAsync_WithEmptySource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new TextDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(string.Empty));
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync("   "));
    }

    [Fact]
    public async Task LoadAsync_WithNonExistentFile_TreatsAsRawContent()
    {
        // Arrange
        var loader = new TextDocumentLoader();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.txt");
        var content = "This is treated as raw content";

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
        var loader = new TextDocumentLoader();
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);
        await File.WriteAllTextAsync(tempFile, "test");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => loader.LoadAsync(tempFile, cts.Token));
    }

    [Fact]
    public async Task LoadAsync_WithLargeFile_LoadsSuccessfully()
    {
        // Arrange
        var loader = new TextDocumentLoader();
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);
        var largeContent = new string('a', 10000);
        await File.WriteAllTextAsync(tempFile, largeContent);

        // Act
        var documents = await loader.LoadAsync(tempFile);

        // Assert
        documents.Should().HaveCount(1);
        documents[0].Content.Should().Be(largeContent);
        documents[0].Content.Length.Should().Be(10000);
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

        GC.SuppressFinalize(this);
    }
}