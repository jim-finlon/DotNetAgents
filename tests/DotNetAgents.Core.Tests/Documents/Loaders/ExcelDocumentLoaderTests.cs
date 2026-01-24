using DotNetAgents.Abstractions.Documents;
using DotNetAgents.Documents.Loaders;
using FluentAssertions;
using OfficeOpenXml;
using Xunit;

namespace DotNetAgents.Core.Tests.Documents.Loaders;

public class ExcelDocumentLoaderTests : IDisposable
{
    private readonly List<string> _tempFiles = new();

    static ExcelDocumentLoaderTests()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        // Act
        var loader = new ExcelDocumentLoader();

        // Assert
        loader.Should().NotBeNull();
        loader.SplitByWorksheet.Should().BeTrue();
        loader.SplitByRow.Should().BeFalse();
        loader.HasHeaders.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithCustomParameters_SetsProperties()
    {
        // Act
        var loader = new ExcelDocumentLoader(splitByWorksheet: false, splitByRow: true, hasHeaders: false);

        // Assert
        loader.SplitByWorksheet.Should().BeFalse();
        loader.SplitByRow.Should().BeTrue();
        loader.HasHeaders.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_WithNullSource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new ExcelDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(null!));
    }

    [Fact]
    public async Task LoadAsync_WithEmptySource_ThrowsArgumentException()
    {
        // Arrange
        var loader = new ExcelDocumentLoader();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync(string.Empty));
        await Assert.ThrowsAsync<ArgumentException>(() => loader.LoadAsync("   "));
    }

    [Fact]
    public async Task LoadAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var loader = new ExcelDocumentLoader();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.xlsx");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => loader.LoadAsync(nonExistentPath));
    }

    [Fact]
    public async Task LoadAsync_WithNonExcelFile_ThrowsArgumentException()
    {
        // Arrange
        var loader = new ExcelDocumentLoader();
        var tempFile = Path.GetTempFileName();
        _tempFiles.Add(tempFile);

        try
        {
            await File.WriteAllTextAsync(tempFile, "This is not an Excel file");

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
    public async Task LoadAsync_WithValidExcelFile_LoadsSuccessfully()
    {
        // Arrange
        var loader = new ExcelDocumentLoader();
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".xlsx");
        _tempFiles.Add(tempFile);

        CreateTestExcelFile(tempFile, "Sheet1", new[] { "Name", "Age" }, new[] { new[] { "John", "30" } });

        // Act
        var documents = await loader.LoadAsync(tempFile);

        // Assert
        documents.Should().HaveCountGreaterThan(0);
        documents[0].Metadata.Should().ContainKey("source");
        documents[0].Metadata.Should().ContainKey("filename");
        documents[0].Metadata.Should().ContainKey("type");
        documents[0].Metadata["type"].Should().Be("excel");
    }

    [Fact]
    public async Task LoadAsync_WithCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var loader = new ExcelDocumentLoader();
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".xlsx");
        _tempFiles.Add(tempFile);
        CreateTestExcelFile(tempFile, "Sheet1", new[] { "Name" }, new[] { new[] { "John" } });
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => loader.LoadAsync(tempFile, cts.Token));
    }

    [Fact]
    public async Task LoadAsync_WithSplitByRow_CreatesOneDocumentPerRow()
    {
        // Arrange
        var loader = new ExcelDocumentLoader(splitByWorksheet: true, splitByRow: true);
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".xlsx");
        _tempFiles.Add(tempFile);
        CreateTestExcelFile(tempFile, "Sheet1", new[] { "Name", "Age" }, new[] { new[] { "John", "30" }, new[] { "Jane", "25" } });

        // Act
        var documents = await loader.LoadAsync(tempFile);

        // Assert
        documents.Should().HaveCountGreaterThan(1);
        documents.All(d => d.Metadata.ContainsKey("row_number")).Should().BeTrue();
    }

    [Fact]
    public async Task LoadAsync_WithMultipleWorksheets_SplitsByWorksheet()
    {
        // Arrange
        var loader = new ExcelDocumentLoader(splitByWorksheet: true, splitByRow: false);
        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ".xlsx");
        _tempFiles.Add(tempFile);

        using (var package = new ExcelPackage(new FileInfo(tempFile)))
        {
            var sheet1 = package.Workbook.Worksheets.Add("Sheet1");
            sheet1.Cells[1, 1].Value = "Name";
            sheet1.Cells[2, 1].Value = "John";

            var sheet2 = package.Workbook.Worksheets.Add("Sheet2");
            sheet2.Cells[1, 1].Value = "City";
            sheet2.Cells[2, 1].Value = "New York";

            package.Save();
        }

        // Act
        var documents = await loader.LoadAsync(tempFile);

        // Assert
        documents.Should().HaveCount(2);
        documents[0].Metadata["worksheet"].Should().Be("Sheet1");
        documents[1].Metadata["worksheet"].Should().Be("Sheet2");
    }

    private static void CreateTestExcelFile(string filePath, string sheetName, string[] headers, string[][] rows)
    {
        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        // Add headers
        for (int col = 0; col < headers.Length; col++)
        {
            worksheet.Cells[1, col + 1].Value = headers[col];
        }

        // Add rows
        for (int row = 0; row < rows.Length; row++)
        {
            for (int col = 0; col < rows[row].Length; col++)
            {
                worksheet.Cells[row + 2, col + 1].Value = rows[row][col];
            }
        }

        package.Save();
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