using DotLangChain.Abstractions.LLM;
using DotLangChain.Core.Memory;
using FluentAssertions;

namespace DotLangChain.Tests.Unit.Memory;

public class WindowMemoryTests
{
    [Fact]
    public void Constructor_WithZeroMaxMessages_ThrowsException()
    {
        // Act
        var act = () => new WindowMemory(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task AddMessageAsync_WithinLimit_KeepsAllMessages()
    {
        // Arrange
        var memory = new WindowMemory(maxMessages: 5);
        var messages = Enumerable.Range(1, 3)
            .Select(i => ChatMessage.User($"Message {i}"))
            .ToArray();

        // Act
        foreach (var msg in messages)
        {
            await memory.AddMessageAsync(msg);
        }

        // Assert
        var retrieved = await memory.GetMessagesAsync();
        retrieved.Should().HaveCount(3);
        retrieved.Should().BeEquivalentTo(messages);
    }

    [Fact]
    public async Task AddMessageAsync_ExceedsLimit_RemovesOldest()
    {
        // Arrange
        var memory = new WindowMemory(maxMessages: 3);
        var messages = Enumerable.Range(1, 5)
            .Select(i => ChatMessage.User($"Message {i}"))
            .ToArray();

        // Act
        foreach (var msg in messages)
        {
            await memory.AddMessageAsync(msg);
        }

        // Assert
        var retrieved = await memory.GetMessagesAsync();
        retrieved.Should().HaveCount(3);
        retrieved[0].Content.Should().Contain("Message 3");
        retrieved[1].Content.Should().Contain("Message 4");
        retrieved[2].Content.Should().Contain("Message 5");
    }

    [Fact]
    public async Task ClearAsync_RemovesAllMessages()
    {
        // Arrange
        var memory = new WindowMemory(maxMessages: 10);
        await memory.AddMessageAsync(ChatMessage.User("Test"));
        await memory.AddMessageAsync(ChatMessage.Assistant("Response"));

        // Act
        await memory.ClearAsync();

        // Assert
        var messages = await memory.GetMessagesAsync();
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task AddMessageAsync_MaintainsFIFOOrder()
    {
        // Arrange
        var memory = new WindowMemory(maxMessages: 3);
        var messages = Enumerable.Range(1, 5)
            .Select(i => ChatMessage.User($"Message {i}"))
            .ToArray();

        // Act
        foreach (var msg in messages)
        {
            await memory.AddMessageAsync(msg);
        }

        // Assert
        var retrieved = await memory.GetMessagesAsync();
        retrieved.Should().HaveCount(3);
        // Should keep last 3 messages in order
        retrieved[0].Content.Should().Be("Message 3");
        retrieved[1].Content.Should().Be("Message 4");
        retrieved[2].Content.Should().Be("Message 5");
    }
}

