using DotLangChain.Core.Exceptions;
using FluentAssertions;

namespace DotLangChain.Tests.Unit.Common.Exceptions;

public class DocumentExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsProperties()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new DocumentException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.ErrorCode.Should().Be("DLC001");
        exception.FilePath.Should().BeNull();
        exception.FileExtension.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithFilePath_SetsFilePathAndExtension()
    {
        // Arrange
        var message = "Load failed";
        var filePath = "/path/to/document.pdf";

        // Act
        var exception = new DocumentException(message, filePath);

        // Assert
        exception.FilePath.Should().Be(filePath);
        exception.FileExtension.Should().Be(".pdf");
    }

    [Fact]
    public void UnsupportedFormat_CreatesCorrectException()
    {
        // Arrange
        var extension = ".xyz";

        // Act
        var exception = DocumentException.UnsupportedFormat(extension);

        // Assert
        exception.Message.Should().Contain(extension);
        exception.ErrorCode.Should().Be("DLC001-001");
        exception.Context.Should().NotBeNull();
        exception.Context!["extension"].Should().Be(extension);
    }

    [Fact]
    public void LoadFailed_CreatesExceptionWithInnerException()
    {
        // Arrange
        var filePath = "/path/to/file.txt";
        var innerException = new IOException("File access denied");

        // Act
        var exception = DocumentException.LoadFailed(filePath, innerException);

        // Assert
        exception.Message.Should().Contain(filePath);
        exception.FilePath.Should().Be(filePath);
        exception.InnerException.Should().Be(innerException);
        exception.ErrorCode.Should().Be("DLC001-002");
    }

    [Fact]
    public void InvalidMetadata_CreatesExceptionWithContext()
    {
        // Arrange
        var reason = "Metadata field is required";

        // Act
        var exception = DocumentException.InvalidMetadata(reason);

        // Assert
        exception.Message.Should().Contain(reason);
        exception.ErrorCode.Should().Be("DLC001-003");
        exception.Context.Should().NotBeNull();
        exception.Context!["reason"].Should().Be(reason);
    }
}

