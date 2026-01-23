using DotNetAgents.Core.Memory;
using DotNetAgents.Core.Memory.Implementations;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Core.Tests.Memory;

public class InMemoryMemoryTests
{
    [Fact]
    public async Task AddMessageAsync_WithValidMessage_AddsMessage()
    {
        // Arrange
        var memory = new InMemoryMemory();
        var message = new MemoryMessage
        {
            Content = "Hello, World!",
            Role = "user"
        };

        // Act
        await memory.AddMessageAsync(message);

        // Assert
        var messages = await memory.GetMessagesAsync(10);
        messages.Should().HaveCount(1);
        messages[0].Content.Should().Be("Hello, World!");
        messages[0].Role.Should().Be("user");
    }

    [Fact]
    public async Task GetMessagesAsync_WithMultipleMessages_ReturnsMostRecent()
    {
        // Arrange
        var memory = new InMemoryMemory();
        await memory.AddMessageAsync(new MemoryMessage { Content = "First", Role = "user" });
        await memory.AddMessageAsync(new MemoryMessage { Content = "Second", Role = "assistant" });
        await memory.AddMessageAsync(new MemoryMessage { Content = "Third", Role = "user" });

        // Act
        var messages = await memory.GetMessagesAsync(2);

        // Assert
        messages.Should().HaveCount(2);
        messages[0].Content.Should().Be("Second");
        messages[1].Content.Should().Be("Third");
    }

    [Fact]
    public async Task ClearAsync_RemovesAllMessages()
    {
        // Arrange
        var memory = new InMemoryMemory();
        await memory.AddMessageAsync(new MemoryMessage { Content = "Test", Role = "user" });

        // Act
        await memory.ClearAsync();

        // Assert
        var messages = await memory.GetMessagesAsync(10);
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task AddMessageAsync_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var memory = new InMemoryMemory();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => memory.AddMessageAsync(null!));
    }
}