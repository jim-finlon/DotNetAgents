using DotLangChain.Abstractions.LLM;
using DotLangChain.Core.Memory;
using FluentAssertions;

namespace DotLangChain.Tests.Unit.Memory;

public class BufferMemoryTests
{
    [Fact]
    public async Task AddMessageAsync_AddsMessage()
    {
        // Arrange
        var memory = new BufferMemory();
        var message = ChatMessage.User("Hello");

        // Act
        await memory.AddMessageAsync(message);

        // Assert
        var messages = await memory.GetMessagesAsync();
        messages.Should().HaveCount(1);
        messages[0].Should().Be(message);
    }

    [Fact]
    public async Task GetMessagesAsync_ReturnsAllMessages()
    {
        // Arrange
        var memory = new BufferMemory();
        var messages = new[]
        {
            ChatMessage.User("Hello"),
            ChatMessage.Assistant("Hi there!"),
            ChatMessage.User("How are you?")
        };

        foreach (var msg in messages)
        {
            await memory.AddMessageAsync(msg);
        }

        // Act
        var retrieved = await memory.GetMessagesAsync();

        // Assert
        retrieved.Should().HaveCount(3);
        retrieved.Should().BeEquivalentTo(messages);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllMessages()
    {
        // Arrange
        var memory = new BufferMemory();
        await memory.AddMessageAsync(ChatMessage.User("Test"));
        await memory.AddMessageAsync(ChatMessage.Assistant("Response"));

        // Act
        await memory.ClearAsync();

        // Assert
        var messages = await memory.GetMessagesAsync();
        messages.Should().BeEmpty();
    }

    [Fact]
    public async Task AddMessageAsync_IsThreadSafe()
    {
        // Arrange
        var memory = new BufferMemory();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var index = i;
            tasks.Add(Task.Run(async () =>
            {
                await memory.AddMessageAsync(ChatMessage.User($"Message {index}"));
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var messages = await memory.GetMessagesAsync();
        messages.Should().HaveCount(100);
    }
}

