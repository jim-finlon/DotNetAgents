using DotNetAgents.Security.Secrets;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Security.Tests.Secrets;

public class EnvironmentSecretsProviderTests
{
    [Fact]
    public async Task GetSecretAsync_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var provider = new EnvironmentSecretsProvider();
        var testKey = "TEST_SECRET_KEY_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(testKey, "test-value");

        try
        {
            // Act
            var result = await provider.GetSecretAsync(testKey);

            // Assert
            result.Should().Be("test-value");
        }
        finally
        {
            Environment.SetEnvironmentVariable(testKey, null);
        }
    }

    [Fact]
    public async Task GetSecretAsync_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        var provider = new EnvironmentSecretsProvider();
        var nonExistentKey = "NON_EXISTENT_KEY_" + Guid.NewGuid().ToString("N");

        // Act
        var result = await provider.GetSecretAsync(nonExistentKey);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SecretExistsAsync_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        var provider = new EnvironmentSecretsProvider();
        var testKey = "TEST_SECRET_EXISTS_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(testKey, "test-value");

        try
        {
            // Act
            var result = await provider.SecretExistsAsync(testKey);

            // Assert
            result.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable(testKey, null);
        }
    }

    [Fact]
    public async Task GetSecretsAsync_WithMultipleKeys_ReturnsDictionary()
    {
        // Arrange
        var provider = new EnvironmentSecretsProvider();
        var key1 = "TEST_KEY_1_" + Guid.NewGuid().ToString("N");
        var key2 = "TEST_KEY_2_" + Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(key1, "value1");
        Environment.SetEnvironmentVariable(key2, "value2");

        try
        {
            // Act
            var result = await provider.GetSecretsAsync(new[] { key1, key2 });

            // Assert
            result.Should().HaveCount(2);
            result[key1].Should().Be("value1");
            result[key2].Should().Be("value2");
        }
        finally
        {
            Environment.SetEnvironmentVariable(key1, null);
            Environment.SetEnvironmentVariable(key2, null);
        }
    }
}