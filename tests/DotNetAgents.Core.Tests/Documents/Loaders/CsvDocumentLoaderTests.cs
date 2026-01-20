using DotNetAgents.Core.Documents;
using DotNetAgents.Core.Documents.Loaders;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Documents.Loaders;

public class CsvDocumentLoaderTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        // Act
        var loader = new CsvDocumentLoader();

        // Assert
        loader.Should().NotBeNull();
        loader.Delimiter.Should().Be(',');
        loader.HasHeaders.Should().BeTrue();
        loader.IncludeHeaders.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithCustomParameters_SetsProperties()
    {
        // Act
        var loader = new CsvDocumentLoader(delimiter: ';', hasHeaders: false, includeHeaders: false);

        // Assert
        loader.Delimiter.Should().Be(';');
        loader.HasHeaders.Should().BeFalse();
        loader.IncludeHeaders.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_WithFilepath_LoadsFileContent()
    {
        // Arrange
        var loader = new CsvDocumentLoader();
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".csv");
        _tempFiles.Add(tempFile);
        var csvContent = "Name,Age,City\nJohn,30,New York\nJane,25,Boston";
        await File.WriteAllTextAsync(tempFile, csvContent);

        // Act
        var documents = await loader.LoadAsync(tempFile);

        // Assert
        documents.Should().HaveCountGreaterThan(0);
        documents[0].Metadata.Should().ContainKey("source");
        documents[0].Metadata.Should().ContainKey("filename");
        documents[0].Metadata.Should().ContainKey("type");
        documents[0].Metadata["type"].Should().Be("csv");
    }

    [Fact]
    public async Task LoadAsync_WithRawContent_TreatsAsContent()
    {
        // Arrange
        var loader = new CsvDocumentLoader();
        var rawContent = "Name,Age\nJohn,30\nJane,25";

        // Act
        var documents = await loader.LoadAsync(rawContent);

        // Assert
        documents.Should().HaveCountGreaterThan(0);
        documents[0].Metadata["type"].Should().Be("csv");
    }

    [Fact]
    public async Task LoadAsync_WithHeaders_CreatesHeaderDocument()
    {
        // Arrange
        var loader = new CsvDocumentLoader(hasHeaders: true, includeHeaders: true);
        var csvContent = "Name,Age\nJohn,30\nJane,25";

        // Act
        var documents = await loader.LoadAsync(csvContent);

        // Assert
        documents.Should().HaveCountGreaterThan(1);
        documents[0].Metadata["row_type"].Should().Be("header");
    }

    [Fact]
    public async Task LoadAsync_WithHeadersAndColumnMapping_IncludesColumnMetadata()
    {
        // Arrange
        var loader = new CsvDocumentLoader(hasHeaders: true);
        var csvContent = "Name,Age\nJohn,30";

        // Act
        var documents = await loader.LoadAsync(csvContent);

        // Assert
        var dataDoc = documents.FirstOrDefault(d => d.Metadata.ContainsKey("row_type") && d.Metadata["row_type"].ToString() == "data");
        dataDoc.Should().NotBeNull();
        dataDoc!.Metadata.Should().ContainKey("column_Name");
        dataDoc.Metadata.Should().ContainKey("column_Age");
    }

    [Fact]
    public async Task LoadAsync_WithQuotedFields_ParsesCorrectly()
    {
        // Arrange
        var loader = new CsvDocumentLoader();
        var csvContent = "\"Name\",\"Age\"\n\"John, Jr.\",\"30\"";

        // Act
        var documents = await loader.LoadAsync(csvContent);

        // Assert
        documents.Should().HaveCountGreaterThan(0);
        documents.Any(d => d.Content.Contains("John, Jr.")).Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_WithNullSource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new CsvDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(null!));
    }

    [Fact]
    public async Task LoadAsync_WithEmptySource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new CsvDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(string.Empty));
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync("   "));
    }

    [Fact]
    public async Task LoadAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var loader = new CsvDocumentLoader();
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".csv");
        _tempFiles.Add(tempFile);
        await File.WriteAllTextAsync(tempFile, "Name,Age\nJohn,30");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => loader.LoadAsync(tempFile, cts.Token));
    }

    [Fact]
    public async Task LoadAsync_WithCustomDelimiter_ParsesCorrectly()
    {
        // Arrange
        var loader = new CsvDocumentLoader(delimiter: ';');
        var csvContent = "Name;Age\nJohn;30";

        // Act
        var documents = await loader.LoadAsync(csvContent);

        // Assert
        documents.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task LoadAsync_WithEmptyCsv_ReturnsEmptyDocument()
    {
        // Arrange
        var loader = new CsvDocumentLoader();
        var csvContent = "";

        // Act
        var documents = await loader.LoadAsync(csvContent);

        // Assert
        documents.Should().HaveCount(1);
        documents[0].Content.Should().BeEmpty();
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