using DotNetAgents.Core.Tools;
using FluentAssertions;
using Moq;
using System.Text.Json;
using Xunit;

namespace DotNetAgents.Core.Tests.Tools;

public class ToolRegistryTests
{
    [Fact]
    public void Register_WithValidTool_RegistersSuccessfully()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tool = CreateMockTool("test-tool", "Test tool");

        // Act
        registry.Register(tool.Object);

        // Assert
        registry.IsRegistered("test-tool").Should().BeTrue();
        registry.GetTool("test-tool").Should().Be(tool.Object);
    }

    [Fact]
    public void Register_WithNullTool_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
    }

    [Fact]
    public void Register_WithDuplicateName_ThrowsArgumentException()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tool1 = CreateMockTool("test-tool", "Test tool 1");
        var tool2 = CreateMockTool("test-tool", "Test tool 2");

        registry.Register(tool1.Object);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => registry.Register(tool2.Object));
    }

    [Fact]
    public void GetTool_WithRegisteredTool_ReturnsTool()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tool = CreateMockTool("test-tool", "Test tool");
        registry.Register(tool.Object);

        // Act
        var result = registry.GetTool("test-tool");

        // Assert
        result.Should().Be(tool.Object);
    }

    [Fact]
    public void GetTool_WithUnregisteredTool_ReturnsNull()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.GetTool("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetTool_WithCaseInsensitiveName_ReturnsTool()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tool = CreateMockTool("test-tool", "Test tool");
        registry.Register(tool.Object);

        // Act
        var result = registry.GetTool("TEST-TOOL");

        // Assert
        result.Should().Be(tool.Object);
    }

    [Fact]
    public void GetAllTools_WithMultipleTools_ReturnsAll()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tool1 = CreateMockTool("tool1", "Tool 1");
        var tool2 = CreateMockTool("tool2", "Tool 2");
        var tool3 = CreateMockTool("tool3", "Tool 3");

        registry.Register(tool1.Object);
        registry.Register(tool2.Object);
        registry.Register(tool3.Object);

        // Act
        var result = registry.GetAllTools();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(tool1.Object);
        result.Should().Contain(tool2.Object);
        result.Should().Contain(tool3.Object);
    }

    [Fact]
    public void Unregister_WithRegisteredTool_ReturnsTrue()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tool = CreateMockTool("test-tool", "Test tool");
        registry.Register(tool.Object);

        // Act
        var result = registry.Unregister("test-tool");

        // Assert
        result.Should().BeTrue();
        registry.IsRegistered("test-tool").Should().BeFalse();
    }

    [Fact]
    public void Unregister_WithUnregisteredTool_ReturnsFalse()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.Unregister("non-existent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRegistered_WithRegisteredTool_ReturnsTrue()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tool = CreateMockTool("test-tool", "Test tool");
        registry.Register(tool.Object);

        // Act
        var result = registry.IsRegistered("test-tool");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRegistered_WithUnregisteredTool_ReturnsFalse()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.IsRegistered("non-existent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetTool_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => registry.GetTool(null!));
        Assert.Throws<ArgumentException>(() => registry.GetTool(string.Empty));
        Assert.Throws<ArgumentException>(() => registry.GetTool("   "));
    }

    private static Mock<ITool> CreateMockTool(string name, string description)
    {
        var mock = new Mock<ITool>();
        mock.Setup(t => t.Name).Returns(name);
        mock.Setup(t => t.Description).Returns(description);
        mock.Setup(t => t.InputSchema).Returns(JsonDocument.Parse("{}").RootElement);
        return mock;
    }
}