using DotNetAgents.Voice.IntentClassification;
using DotNetAgents.Voice.Parsing;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetAgents.Voice.Tests.Parsing;

public class CommandParserTests
{
    private readonly Mock<IIntentClassifier> _classifierMock;
    private readonly Mock<ILogger<CommandParser>> _loggerMock;
    private readonly CommandParser _parser;

    public CommandParserTests()
    {
        _classifierMock = new Mock<IIntentClassifier>();
        _loggerMock = new Mock<ILogger<CommandParser>>();
        _parser = new CommandParser(_classifierMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ParseAsync_WithValidCommand_ReturnsIntent()
    {
        // Arrange
        var commandText = "create a note";
        var expectedIntent = new Intent
        {
            Domain = "notes",
            Action = "create",
            Parameters = new Dictionary<string, object> { ["content"] = "note" },
            MissingRequired = new List<string>(),
            Confidence = 0.95,
            RawText = commandText
        };

        _classifierMock
            .Setup(c => c.ClassifyAsync(commandText, It.IsAny<IntentContext?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedIntent);

        // Act
        var result = await _parser.ParseAsync(commandText);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedIntent);
    }

    [Fact]
    public async Task ParseAsync_WithEmptyCommand_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _parser.ParseAsync(string.Empty));
    }

    [Fact]
    public async Task ParseAsync_WithNullCommand_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _parser.ParseAsync(null!));
    }

    [Fact]
    public async Task ParseAsync_WhenClassifierThrows_ThrowsInvalidOperationException()
    {
        // Arrange
        var commandText = "some command";
        _classifierMock
            .Setup(c => c.ClassifyAsync(commandText, It.IsAny<IntentContext?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Classification failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _parser.ParseAsync(commandText));
    }
}
