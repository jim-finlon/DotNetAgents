using DotLangChain.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DotLangChain.Core.Security;

/// <summary>
/// Secure handling of API keys and secrets.
/// </summary>
public interface ISecretProvider
{
    /// <summary>
    /// Gets a secret as a string.
    /// </summary>
    Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a secret and deserializes it as type T.
    /// </summary>
    Task<T?> GetSecretAsync<T>(string key, CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration-based secret provider with validation.
/// </summary>
public sealed class ConfigurationSecretProvider : ISecretProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationSecretProvider>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationSecretProvider"/> class.
    /// </summary>
    public ConfigurationSecretProvider(
        IConfiguration configuration,
        ILogger<ConfigurationSecretProvider>? logger = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<string?> GetSecretAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = _configuration[key];

        if (string.IsNullOrEmpty(value))
        {
            _logger?.LogWarning("Secret not found: {Key}", key);
            return Task.FromResult<string?>(null);
        }

        _logger?.LogDebug("Secret retrieved: {Key}", key);
        return Task.FromResult<string?>(value);
    }

    /// <inheritdoc/>
    public Task<T?> GetSecretAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var section = _configuration.GetSection(key);

        if (!section.Exists())
        {
            _logger?.LogWarning("Secret section not found: {Key}", key);
            return Task.FromResult<T?>(default);
        }

        var value = section.Get<T>();
        return Task.FromResult(value);
    }
}

