using DotNetAgents.Abstractions.Models;
using DotNetAgents.Voice.IntentClassification;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetAgents.Voice.Tests.IntentClassification;

public class LLMIntentClassifierTests
{
    private readonly Mock<ILLMModel<ChatMessage[], ChatMessage>> _llmModelMock;
    private readonly Mock<ILogger<LLMIntentClassifier>> _loggerMock;
    private readonly LLMIntentClassifier _classifier;

    public LLMIntentClassifierTests()
    {
        _llmModelMock = new Mock<ILLMModel<ChatMessage[], ChatMessage>>();
        _loggerMock = new Mock<ILogger<LLMIntentClassifier>>();
        _classifier = new LLMIntentClassifier(_llmModelMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ClassifyAsync_WithValidCommand_ReturnsIntent()
    {
        // Arrange
        var commandText = "create a note about the meeting";
        var response = new ChatMessage
        {
            Role = "assistant",
            Content = """{"intent": "notes.create", "confidence": 0.95, "parameters": {"content": "meeting"}}"""
        };

        _llmModelMock
            .Setup(m => m.GenerateAsync(It.IsAny<ChatMessage[]>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _classifier.ClassifyAsync(commandText);

        // Assert
        result.Should().NotBeNull();
        result.Domain.Should().Be("notes");
        result.Action.Should().Be("create");
        result.Confidence.Should().Be(0.95);
        result.Parameters.Should().ContainKey("content");
        result.Parameters["content"].Should().BeEquivalentTo("meeting");
        result.RawText.Should().Be(commandText);
    }

    [Fact]
    public async Task ClassifyAsync_WithEmptyCommand_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _classifier.ClassifyAsync(string.Empty));
    }

    [Fact]
    public async Task ClassifyAsync_WithNullCommand_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _classifier.ClassifyAsync(null!));
    }

    [Fact]
    public async Task ClassifyAsync_WithInvalidJson_ReturnsFallbackIntent()
    {
        // Arrange
        var commandText = "some command";
        var response = new ChatMessage
        {
            Role = "assistant",
            Content = "invalid json"
        };

        _llmModelMock
            .Setup(m => m.GenerateAsync(It.IsAny<ChatMessage[]>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _classifier.ClassifyAsync(commandText);

        // Assert
        result.Should().NotBeNull();
        result.Domain.Should().Be("unknown");
        result.Action.Should().Be("unknown");
        result.Confidence.Should().Be(0.0);
    }

    [Fact]
    public async Task ClassifyAsync_WithMissingRequiredParameters_SetsMissingRequired()
    {
        // Arrange
        var commandText = "create a task";
        var response = new ChatMessage
        {
            Role = "assistant",
            Content = """{"intent": "tasks.create_personal", "confidence": 0.9, "parameters": {}}"""
        };

        _llmModelMock
            .Setup(m => m.GenerateAsync(It.IsAny<ChatMessage[]>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _classifier.ClassifyAsync(commandText);

        // Assert
        result.Should().NotBeNull();
        result.MissingRequired.Should().Contain("title");
        result.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task ClassifyAsync_WithSubTypeIntent_ParsesCorrectly()
    {
        // Arrange
        var commandText = "create a personal task";
        var response = new ChatMessage
        {
            Role = "assistant",
            Content = """{"intent": "tasks.create_personal", "confidence": 0.92, "parameters": {"title": "task"}}"""
        };

        _llmModelMock
            .Setup(m => m.GenerateAsync(It.IsAny<ChatMessage[]>(), It.IsAny<LLMOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _classifier.ClassifyAsync(commandText);

        // Assert
        result.Should().NotBeNull();
        result.Domain.Should().Be("tasks");
        result.Action.Should().Be("create");
        result.SubType.Should().Be("personal");
        result.FullName.Should().Be("tasks.create.personal");
    }
}
