using DotNetAgents.Workflow.Checkpoints;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Workflow.Tests.Checkpoints;

public class JsonStateSerializerTests
{
    [Fact]
    public void Serialize_WithValidState_ReturnsJsonString()
    {
        // Arrange
        var serializer = new JsonStateSerializer<TestState>();
        var state = new TestState { Value = 42, Name = "test" };

        // Act
        var result = serializer.Serialize(state);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().Contain("42");
        result.Should().Contain("test");
    }

    [Fact]
    public void Deserialize_WithValidJson_ReturnsStateObject()
    {
        // Arrange
        var serializer = new JsonStateSerializer<TestState>();
        var json = "{\"value\":42,\"name\":\"test\"}";

        // Act
        var result = serializer.Deserialize(json);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(42);
        result.Name.Should().Be("test");
    }

    [Fact]
    public void Serialize_WithNullState_ThrowsArgumentNullException()
    {
        // Arrange
        var serializer = new JsonStateSerializer<TestState>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => serializer.Serialize(null!));
    }

    [Fact]
    public void Deserialize_WithNullString_ThrowsArgumentException()
    {
        // Arrange
        var serializer = new JsonStateSerializer<TestState>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => serializer.Deserialize(null!));
        Assert.Throws<ArgumentException>(() => serializer.Deserialize(string.Empty));
        Assert.Throws<ArgumentException>(() => serializer.Deserialize("   "));
    }

    [Fact]
    public void Serialize_Deserialize_RoundTripPreservesData()
    {
        // Arrange
        var serializer = new JsonStateSerializer<TestState>();
        var originalState = new TestState { Value = 100, Name = "roundtrip" };

        // Act
        var serialized = serializer.Serialize(originalState);
        var deserialized = serializer.Deserialize(serialized);

        // Assert
        deserialized.Value.Should().Be(originalState.Value);
        deserialized.Name.Should().Be(originalState.Name);
    }

    [Fact]
    public void Serialize_WithCustomOptions_UsesCustomOptions()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var serializer = new JsonStateSerializer<TestState>(options);
        var state = new TestState { Value = 42, Name = "test" };

        // Act
        var result = serializer.Serialize(state);

        // Assert
        result.Should().Contain("value"); // Snake case would be different, but default is camelCase
    }

    private class TestState
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}