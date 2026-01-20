using DotNetAgents.Observability.Metrics;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Observability.Tests.Metrics;

public class ModelPricingTests
{
    [Fact]
    public void GetPricing_WithKnownModel_ReturnsPricing()
    {
        // Act
        var pricing = ModelPricing.GetPricing("gpt-4");

        // Assert
        pricing.Should().NotBeNull();
        pricing!.Value.InputPricePer1K.Should().Be(0.03m);
        pricing.Value.OutputPricePer1K.Should().Be(0.06m);
    }

    [Fact]
    public void GetPricing_WithUnknownModel_ReturnsNull()
    {
        // Act
        var pricing = ModelPricing.GetPricing("unknown-model");

        // Assert
        pricing.Should().BeNull();
    }

    [Fact]
    public void CalculateCost_WithValidInputs_ReturnsCost()
    {
        // Act
        var cost = ModelPricing.CalculateCost("gpt-4", 1000, 500);

        // Assert
        cost.Should().NotBeNull();
        cost.Should().BeApproximately(0.06m, 0.001m); // 0.03 + 0.03
    }

    [Fact]
    public void CalculateCost_WithZeroTokens_ReturnsZero()
    {
        // Act
        var cost = ModelPricing.CalculateCost("gpt-4", 0, 0);

        // Assert
        cost.Should().Be(0m);
    }

    [Fact]
    public void RegisterPricing_WithNewModel_RegistersSuccessfully()
    {
        // Arrange
        var modelName = "custom-model";

        // Act
        ModelPricing.RegisterPricing(modelName, 0.01m, 0.02m);
        var pricing = ModelPricing.GetPricing(modelName);

        // Assert
        pricing.Should().NotBeNull();
        pricing!.Value.InputPricePer1K.Should().Be(0.01m);
        pricing.Value.OutputPricePer1K.Should().Be(0.02m);
    }

    [Fact]
    public void RegisterPricing_WithNullModel_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ModelPricing.RegisterPricing(null!, 0.01m, 0.02m));
        Assert.Throws<ArgumentException>(() => ModelPricing.RegisterPricing(string.Empty, 0.01m, 0.02m));
        Assert.Throws<ArgumentException>(() => ModelPricing.RegisterPricing("   ", 0.01m, 0.02m));
    }
}