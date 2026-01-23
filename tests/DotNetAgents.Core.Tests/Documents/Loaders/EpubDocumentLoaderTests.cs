using DotNetAgents.Core.Documents;
using DotNetAgents.Core.Documents.Loaders;
using FluentAssertions;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace DotNetAgents.Core.Tests.Documents.Loaders;

public class EpubDocumentLoaderTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        // Act
        var loader = new EpubDocumentLoader();

        // Assert
        loader.Should().NotBeNull();
        loader.SplitByChapter.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithSplitByChapterFalse_SetsProperty()
    {
        // Act
        var loader = new EpubDocumentLoader(splitByChapter: false);

        // Assert
        loader.SplitByChapter.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_WithNullSource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new EpubDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(null!));
    }

    [Fact]
    public async Task LoadAsync_WithEmptySource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new EpubDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(string.Empty));
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync("   "));
    }

    [Fact]
    public async Task LoadAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var loader = new EpubDocumentLoader();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.epub");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => loader.LoadAsync(nonExistentPath));
    }

    [Fact]
    public async Task LoadAsync_WithNonEpubFile_ThrowsArgumentException()
    {
        // Arrange
        var loader = new EpubDocumentLoader();
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);

        try
        {
            await File.WriteAllTextAsync(tempFile, "This is not an EPUB file");

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
    public async Task LoadAsync_WithInvalidEpubFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var loader = new EpubDocumentLoader();
        var invalidEpubPath = Path.Combine(Path.GetTempPath(), $"invalid_{Guid.NewGuid()}.epub");
        _tempFiles.Add(invalidEpubPath);

        try
        {
            // Create a file with .epub extension but invalid EPUB content
            await File.WriteAllBytesAsync(invalidEpubPath, new byte[] { 0x00, 0x01, 0x02, 0x03 });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => loader.LoadAsync(invalidEpubPath));
        }
        finally
        {
            if (File.Exists(invalidEpubPath))
                File.Delete(invalidEpubPath);
        }
    }

    [Fact]
    public async Task LoadAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var loader = new EpubDocumentLoader();
        var tempFile = CreateMinimalValidEpub();
        _tempFiles.Add(tempFile);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => loader.LoadAsync(tempFile, cts.Token));
    }

    private string CreateMinimalValidEpub()
    {
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".epub");
        
        // Create a minimal valid EPUB structure
        using (var archive = ZipFile.Open(tempFile, ZipArchiveMode.Create))
        {
            // META-INF/container.xml
            var containerEntry = archive.CreateEntry("META-INF/container.xml");
            using (var writer = new StreamWriter(containerEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("<?xml version=\"1.0\"?>");
                writer.WriteLine("<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">");
                writer.WriteLine("  <rootfiles>");
                writer.WriteLine("    <rootfile full-path=\"OEBPS/content.opf\" media-type=\"application/oebps-package+xml\"/>");
                writer.WriteLine("  </rootfiles>");
                writer.WriteLine("</container>");
            }

            // OEBPS/content.opf
            var opfEntry = archive.CreateEntry("OEBPS/content.opf");
            using (var writer = new StreamWriter(opfEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("<?xml version=\"1.0\"?>");
                writer.WriteLine("<package version=\"2.0\" xmlns=\"http://www.idpf.org/2007/opf\" unique-identifier=\"BookId\">");
                writer.WriteLine("  <metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\">");
                writer.WriteLine("    <dc:title>Test Book</dc:title>");
                writer.WriteLine("    <dc:identifier id=\"BookId\">test-123</dc:identifier>");
                writer.WriteLine("  </metadata>");
                writer.WriteLine("  <manifest>");
                writer.WriteLine("    <item id=\"chapter1\" href=\"chapter1.xhtml\" media-type=\"application/xhtml+xml\"/>");
                writer.WriteLine("  </manifest>");
                writer.WriteLine("  <spine toc=\"ncx\">");
                writer.WriteLine("    <itemref idref=\"chapter1\"/>");
                writer.WriteLine("  </spine>");
                writer.WriteLine("</package>");
            }

            // OEBPS/chapter1.xhtml
            var chapterEntry = archive.CreateEntry("OEBPS/chapter1.xhtml");
            using (var writer = new StreamWriter(chapterEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
                writer.WriteLine("  <head><title>Chapter 1</title></head>");
                writer.WriteLine("  <body><p>This is chapter 1 content.</p></body>");
                writer.WriteLine("</html>");
            }

            // OEBPS/toc.ncx
            var ncxEntry = archive.CreateEntry("OEBPS/toc.ncx");
            using (var writer = new StreamWriter(ncxEntry.Open(), Encoding.UTF8))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\">");
                writer.WriteLine("  <head><meta name=\"dtb:uid\" content=\"test-123\"/></head>");
                writer.WriteLine("  <docTitle><text>Test Book</text></docTitle>");
                writer.WriteLine("  <navMap><navPoint id=\"nav1\" playOrder=\"1\"><navLabel><text>Chapter 1</text></navLabel><content src=\"chapter1.xhtml\"/></navPoint></navMap>");
                writer.WriteLine("</ncx>");
            }
        }

        return tempFile;
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