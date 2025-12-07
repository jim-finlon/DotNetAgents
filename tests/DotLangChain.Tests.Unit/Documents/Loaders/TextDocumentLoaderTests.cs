using DotLangChain.Abstractions.Documents;
using DotLangChain.Core.Documents.Loaders;
using DotLangChain.Core.Exceptions;
using FluentAssertions;

namespace DotLangChain.Tests.Unit.Documents.Loaders;

public class TextDocumentLoaderTests
{
    private readonly TextDocumentLoader _loader = new();

    [Fact]
    public void SupportedExtensions_ContainsExpectedFormats()
    {
        // Act
        var extensions = _loader.SupportedExtensions;

        // Assert
        extensions.Should().Contain(".txt");
        extensions.Should().Contain(".csv");
        extensions.Should().Contain(".json");
        extensions.Should().Contain(".xml");
        extensions.Should().Contain(".log");
    }

    [Fact]
    public async Task LoadAsync_WithValidStream_ReturnsDocument()
    {
        // Arrange
        var content = "This is test content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var fileName = "test.txt";

        // Act
        var document = await _loader.LoadAsync(stream, fileName);

        // Assert
        document.Should().NotBeNull();
        document.Content.Should().Be(content);
        document.Id.Should().NotBeNullOrEmpty();
        document.Metadata.Source.Should().Be(fileName);
        document.SourceUri.Should().Be(fileName);
    }

    [Fact]
    public async Task LoadAsync_WithMetadata_PreservesMetadata()
    {
        // Arrange
        var content = "Test content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var metadata = new DocumentMetadata
        {
            Title = "Test Document",
            Source = "test-source"
        };

        // Act
        var document = await _loader.LoadAsync(stream, "test.txt", metadata);

        // Assert
        document.Metadata.Title.Should().Be("Test Document");
        document.Metadata.Source.Should().Be("test-source");
    }

    [Fact]
    public async Task LoadAsync_WithFilePath_LoadsFromFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = "File content";
        await File.WriteAllTextAsync(tempFile, content);

        try
        {
            // Act
            var document = await _loader.LoadAsync(tempFile);

            // Assert
            document.Content.Should().Be(content);
            document.FilePath.Should().Be(tempFile);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_WithNonExistentFile_ThrowsDocumentException()
    {
        // Arrange
        var nonExistentPath = "/nonexistent/file.txt";

        // Act
        var act = async () => await _loader.LoadAsync(nonExistentPath);

        // Assert
        await act.Should().ThrowAsync<DocumentException>()
            .Where(e => e.FilePath == nonExistentPath);
    }

    [Fact]
    public async Task LoadAsync_WithFileUri_LoadsFromFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = "URI content";
        await File.WriteAllTextAsync(tempFile, content);
        var uri = new Uri($"file://{tempFile}");

        try
        {
            // Act
            var document = await _loader.LoadAsync(uri);

            // Assert
            document.Content.Should().Be(content);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_WithUnsupportedUriScheme_ThrowsDocumentException()
    {
        // Arrange
        var uri = new Uri("ftp://example.com/file.txt");

        // Act
        var act = async () => await _loader.LoadAsync(uri);

        // Assert
        await act.Should().ThrowAsync<DocumentException>();
    }

    [Theory]
    [InlineData(".txt", "text/plain")]
    [InlineData(".csv", "text/csv")]
    [InlineData(".json", "application/json")]
    [InlineData(".xml", "application/xml")]
    public async Task LoadAsync_SetsCorrectContentType(string extension, string expectedContentType)
    {
        // Arrange
        var fileName = $"test{extension}";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("content"));

        // Act
        var document = await _loader.LoadAsync(stream, fileName);

        // Assert
        document.Metadata.ContentType.Should().Be(expectedContentType);
    }
}

