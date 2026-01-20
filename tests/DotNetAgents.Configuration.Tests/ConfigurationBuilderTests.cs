using DotNetAgents.Configuration;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Configuration.Tests;

public class ConfigurationBuilderTests
{
    [Fact]
    public void WithDefaultLLMProvider_SetsProvider()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        // Act
        var config = builder
            .WithDefaultLLMProvider("OpenAI")
            .WithDefaultLLMModel("gpt-4")
            .BuildWithoutValidation();

        // Assert
        config.DefaultLLMProvider.Should().Be("OpenAI");
        config.DefaultLLMModel.Should().Be("gpt-4");
    }

    [Fact]
    public void AddLLMProvider_AddsProviderConfiguration()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        // Act
        var config = builder
            .AddLLMProvider("OpenAI", p =>
            {
                p.ProviderType = "OpenAI";
                p.ApiKeySecretName = "OpenAI:ApiKey";
            })
            .BuildWithoutValidation();

        // Assert
        config.LLMProviders.Should().ContainKey("OpenAI");
        config.LLMProviders["OpenAI"].ProviderType.Should().Be("OpenAI");
        config.LLMProviders["OpenAI"].ApiKeySecretName.Should().Be("OpenAI:ApiKey");
    }

    [Fact]
    public void AddVectorStore_AddsStoreConfiguration()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        // Act
        var config = builder
            .AddVectorStore("Pinecone", s =>
            {
                s.StoreType = "Pinecone";
                s.ConnectionString = "https://api.pinecone.io";
            })
            .BuildWithoutValidation();

        // Assert
        config.VectorStores.Should().ContainKey("Pinecone");
        config.VectorStores["Pinecone"].StoreType.Should().Be("Pinecone");
        config.VectorStores["Pinecone"].ConnectionString.Should().Be("https://api.pinecone.io");
    }

    [Fact]
    public void WithExecutionOptions_ConfiguresOptions()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        // Act
        var config = builder
            .WithExecutionOptions(o =>
            {
                o.DefaultTimeout = TimeSpan.FromMinutes(10);
                o.MaxRetries = 5;
                o.EnableLogging = false;
            })
            .BuildWithoutValidation();

        // Assert
        config.ExecutionOptions.DefaultTimeout.Should().Be(TimeSpan.FromMinutes(10));
        config.ExecutionOptions.MaxRetries.Should().Be(5);
        config.ExecutionOptions.EnableLogging.Should().BeFalse();
    }

    [Fact]
    public void Build_WithValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        // Act
        var config = builder
            .WithDefaultLLMProvider("OpenAI")
            .WithDefaultLLMModel("gpt-4")
            .AddLLMProvider("OpenAI", p => p.ProviderType = "OpenAI")
            .Build();

        // Assert
        config.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithInvalidConfiguration_ThrowsException()
    {
        // Arrange
        var builder = new ConfigurationBuilder();

        // Act & Assert
        Assert.Throws<Core.Exceptions.AgentException>(() => builder.Build());
    }
}