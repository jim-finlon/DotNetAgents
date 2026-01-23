using DotNetAgents.Configuration;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Configuration.Tests;

public class AgentConfigurationTests
{
    [Fact]
    public void Validate_WithValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var config = new AgentConfiguration
        {
            DefaultLLMProvider = "OpenAI",
            DefaultLLMModel = "gpt-4",
            LLMProviders = new Dictionary<string, LLMProviderConfiguration>
            {
                ["OpenAI"] = new LLMProviderConfiguration { ProviderType = "OpenAI" }
            }
        };

        // Act & Assert
        config.Invoking(c => c.Validate()).Should().NotThrow();
    }

    [Fact]
    public void Validate_WithMissingDefaultProvider_ThrowsException()
    {
        // Arrange
        var config = new AgentConfiguration();

        // Act & Assert
        config.Invoking(c => c.Validate())
            .Should()
            .Throw<Core.Exceptions.AgentException>()
            .WithMessage("*DefaultLLMProvider*");
    }

    [Fact]
    public void Validate_WithProviderNotConfigured_ThrowsException()
    {
        // Arrange
        var config = new AgentConfiguration
        {
            DefaultLLMProvider = "OpenAI",
            DefaultLLMModel = "gpt-4"
        };

        // Act & Assert
        config.Invoking(c => c.Validate())
            .Should()
            .Throw<Core.Exceptions.AgentException>()
            .WithMessage("*not configured*");
    }
}