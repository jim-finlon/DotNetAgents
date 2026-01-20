using DotNetAgents.Core.Caching;
using DotNetAgents.Core.Chains;
using DotNetAgents.Core.Exceptions;
using DotNetAgents.Core.Memory;
using DotNetAgents.Core.Models;
using DotNetAgents.Core.Prompts;
using FluentAssertions;
using Moq;
using Xunit;

namespace DotNetAgents.Core.Tests.Chains;

public class ChainBuilderTests
{
    [Fact]
    public void Create_ReturnsNewBuilder()
    {
        // Act
        var builder = ChainBuilder<string, string>.Create();

        // Assert
        builder.Should().NotBeNull();
    }

    [Fact]
    public void WithLLM_SetsLLMModel()
    {
        // Arrange
        var builder = ChainBuilder<string, string>.Create();
        var mockLLM = new Mock<ILLMModel<string, string>>();

        // Act
        var result = builder.WithLLM(mockLLM.Object);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithPromptTemplate_WithTemplateObject_SetsTemplate()
    {
        // Arrange
        var builder = ChainBuilder<string, string>.Create();
        var template = new PromptTemplate("Hello {name}!");

        // Act
        var result = builder.WithPromptTemplate(template);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithPromptTemplate_WithTemplateString_SetsTemplate()
    {
        // Arrange
        var builder = ChainBuilder<string, string>.Create();

        // Act
        var result = builder.WithPromptTemplate("Hello {name}!");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithMemory_SetsMemory()
    {
        // Arrange
        var builder = ChainBuilder<string, string>.Create();
        var mockMemory = new Mock<IMemory>();

        // Act
        var result = builder.WithMemory(mockMemory.Object);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithCaching_SetsCache()
    {
        // Arrange
        var builder = ChainBuilder<string, string>.Create();
        var cache = new InMemoryCache();
        var responseCache = new LLMResponseCache<string, string>(cache);
        var mockLLM = new Mock<ILLMModel<string, string>>();

        // Act
        var result = builder.WithLLM(mockLLM.Object).WithCaching(responseCache);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithRetryPolicy_SetsRetryPolicy()
    {
        // Arrange
        var builder = ChainBuilder<string, string>.Create();

        // Act
        var result = builder.WithRetryPolicy(3, TimeSpan.FromSeconds(1));

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void Build_WithoutLLM_ThrowsException()
    {
        // Arrange
        var builder = ChainBuilder<string, string>.Create();

        // Act & Assert
        var exception = Assert.Throws<AgentException>(() => builder.Build());
        exception.Category.Should().Be(ErrorCategory.ConfigurationError);
    }

    [Fact]
    public void Build_WithLLM_BuildsChain()
    {
        // Arrange
        var builder = ChainBuilder<string, string>.Create();
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.Setup(m => m.GenerateAsync(It.IsAny<string>(), It.IsAny<LLMOptions?>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("test output");

        // Act
        var chain = builder.WithLLM(mockLLM.Object).Build();

        // Assert
        chain.Should().NotBeNull();
    }

    [Fact]
    public async Task Build_WithRetryPolicy_WrapsWithRetry()
    {
        // Arrange
        var builder = ChainBuilder<string, string>.Create();
        var mockLLM = new Mock<ILLMModel<string, string>>();
        mockLLM.SetupSequence(m => m.GenerateAsync(It.IsAny<string>(), It.IsAny<LLMOptions?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("First attempt fails"))
                .ReturnsAsync("success");

        // Act
        var chain = builder
            .WithLLM(mockLLM.Object)
            .WithRetryPolicy(1, TimeSpan.FromMilliseconds(10))
            .Build();

        var result = await chain.InvokeAsync("test");

        // Assert
        result.Should().Be("success");
        mockLLM.Verify(m => m.GenerateAsync(It.IsAny<string>(), It.IsAny<LLMOptions?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}