using DotNetAgents.Core.Documents;
using DotNetAgents.Core.Documents.Loaders;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Documents.Loaders;

public class PdfDocumentLoaderTests : IDisposable
{
    private readonly string _testPdfPath;
    private readonly List<string> _tempFiles = new();

    public PdfDocumentLoaderTests()
    {
        // Create a temporary PDF file for testing
        // Note: In a real scenario, you'd use a proper PDF library to create test PDFs
        // For now, we'll test error cases and basic functionality
        _testPdfPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.pdf");
    }

    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        // Act
        var loader = new PdfDocumentLoader();

        // Assert
        loader.Should().NotBeNull();
        loader.SplitByPage.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithSplitByPageFalse_SetsProperty()
    {
        // Act
        var loader = new PdfDocumentLoader(splitByPage: false);

        // Assert
        loader.SplitByPage.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_WithNullSource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new PdfDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(null!));
    }

    [Fact]
    public async Task LoadAsync_WithEmptySource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new PdfDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(string.Empty));
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync("   "));
    }

    [Fact]
    public async Task LoadAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var loader = new PdfDocumentLoader();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.pdf");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => loader.LoadAsync(nonExistentPath));
        exception.FileName.Should().Be(nonExistentPath);
    }

    [Fact]
    public async Task LoadAsync_WithNonPdfFile_ThrowsArgumentException()
    {
        // Arrange
        var loader = new PdfDocumentLoader();
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);

        try
        {
            await File.WriteAllTextAsync(tempFile, "This is not a PDF");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_WithInvalidPdfFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var loader = new PdfDocumentLoader();
        var invalidPdfPath = Path.Combine(Path.GetTempPath(), $"invalid_{Guid.NewGuid()}.pdf");
        _tempFiles.Add(invalidPdfPath);

        try
        {
            // Create a file with .pdf extension but invalid PDF content
            await File.WriteAllBytesAsync(invalidPdfPath, new byte[] { 0x00, 0x01, 0x02, 0x03 });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => loader.LoadAsync(invalidPdfPath));
        }
        finally
        {
            if (File.Exists(invalidPdfPath))
                File.Delete(invalidPdfPath);
        }
    }

    [Fact]
    public async Task LoadAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var loader = new PdfDocumentLoader();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => loader.LoadAsync(_testPdfPath, cts.Token));
    }

    public void Dispose()
    {
        // Clean up temporary files
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

        try
        {
            if (File.Exists(_testPdfPath))
                File.Delete(_testPdfPath);
        }
        catch
        {
            // Ignore cleanup errors
        }

        GC.SuppressFinalize(this);
    }
}