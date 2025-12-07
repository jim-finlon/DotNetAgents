using DotLangChain.Abstractions.Agents.Tools;
using DotLangChain.Abstractions.LLM;
using DotLangChain.Core.Exceptions;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace DotLangChain.Core.Agents.Tools;

/// <summary>
/// Registry for managing tool implementations.
/// </summary>
public sealed class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ToolInfo> _tools = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<ToolRegistry>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolRegistry"/> class.
    /// </summary>
    public ToolRegistry(ILogger<ToolRegistry>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Register<T>() where T : class
    {
        var type = typeof(T);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var toolAttr = method.GetCustomAttribute<ToolAttribute>();
            if (toolAttr == null)
                continue;

            var toolName = toolAttr.Name ?? method.Name;
            RegisterMethod(toolName, method, toolAttr.Description);
        }

        _logger?.LogDebug("Registered tool type: {Type}", type.Name);
    }

    /// <inheritdoc/>
    public void Register(object toolInstance)
    {
        ArgumentNullException.ThrowIfNull(toolInstance);

        var type = toolInstance.GetType();
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var toolAttr = method.GetCustomAttribute<ToolAttribute>();
            if (toolAttr == null)
                continue;

            var toolName = toolAttr.Name ?? method.Name;
            RegisterMethod(toolName, method, toolAttr.Description, toolInstance);
        }

        _logger?.LogDebug("Registered tool instance: {Type}", type.Name);
    }

    /// <inheritdoc/>
    public void Register(string name, Delegate handler, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(handler);

        var method = handler.Method;
        RegisterMethod(name, method, description, handler.Target);

        _logger?.LogDebug("Registered tool delegate: {Name}", name);
    }

    /// <inheritdoc/>
    public IToolExecutor BuildExecutor()
    {
        return new ToolExecutor(_tools, _logger);
    }

    private void RegisterMethod(string toolName, MethodInfo method, string? description, object? instance = null)
    {
        if (_tools.ContainsKey(toolName))
        {
            _logger?.LogWarning("Tool '{ToolName}' already registered, overwriting", toolName);
        }

        var parameters = method.GetParameters();
        var properties = new Dictionary<string, JsonElement>();

        foreach (var param in parameters)
        {
            var paramAttr = param.GetCustomAttribute<ToolParameterAttribute>();
            var paramName = param.Name ?? "unknown";
            var isRequired = paramAttr?.Required ?? !param.HasDefaultValue;

            var paramSchema = new
            {
                type = GetJsonTypeName(param.ParameterType),
                description = paramAttr?.Description,
                @default = param.HasDefaultValue ? param.DefaultValue : null
            };

            properties[paramName] = JsonSerializer.SerializeToElement(paramSchema);
        }

        var schema = new
        {
            type = "object",
            properties,
            required = properties.Keys.Where((k, i) => 
            {
                var param = parameters[i];
                var paramAttr = param.GetCustomAttribute<ToolParameterAttribute>();
                return paramAttr?.Required ?? !param.HasDefaultValue;
            }).ToArray()
        };

        var toolInfo = new ToolInfo
        {
            Name = toolName,
            Description = description,
            Method = method,
            Instance = instance,
            ParametersSchema = JsonSerializer.SerializeToElement(schema)
        };

        _tools[toolName] = toolInfo;
    }

    private static string GetJsonTypeName(Type type)
    {
        // Handle nullable types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        // Handle task types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return type.Name switch
        {
            nameof(String) => "string",
            nameof(Int32) => "integer",
            nameof(Int64) => "integer",
            nameof(Double) => "number",
            nameof(Single) => "number",
            nameof(Boolean) => "boolean",
            _ => "object"
        };
    }

    private sealed class ToolInfo
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required MethodInfo Method { get; init; }
        public object? Instance { get; init; }
        public required JsonElement ParametersSchema { get; init; }
    }

    private sealed class ToolExecutor : IToolExecutor
    {
        private readonly Dictionary<string, ToolInfo> _tools;
        private readonly ILogger? _logger;

        public ToolExecutor(Dictionary<string, ToolInfo> tools, ILogger? logger)
        {
            _tools = tools;
            _logger = logger;
        }

        public IReadOnlyList<ToolDefinition> GetToolDefinitions()
        {
            return _tools.Values.Select(tool => new ToolDefinition
            {
                Name = tool.Name,
                Description = tool.Description,
                ParametersSchema = tool.ParametersSchema
            }).ToList();
        }

        public async Task<ToolResult> ExecuteAsync(
            ToolCall toolCall,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(toolCall);

            if (!_tools.TryGetValue(toolCall.Name, out var tool))
            {
                throw ToolException.ToolNotFound(toolCall.Name);
            }

            try
            {
                var argsDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(toolCall.Arguments);
                if (argsDict == null)
                {
                    throw ToolException.InvalidParameters(toolCall.Name, "Arguments JSON is null");
                }

                var parameters = tool.Method.GetParameters();
                var args = new object?[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];
                    var paramName = param.Name ?? throw new InvalidOperationException($"Parameter {i} has no name");

                    if (argsDict.TryGetValue(paramName, out var argValue))
                    {
                        args[i] = JsonSerializer.Deserialize(argValue, param.ParameterType);
                    }
                    else if (param.HasDefaultValue)
                    {
                        args[i] = param.DefaultValue;
                    }
                    else
                    {
                        throw ToolException.InvalidParameters(toolCall.Name, $"Missing required parameter: {paramName}");
                    }
                }

                object? result;
                if (tool.Instance != null)
                {
                    result = tool.Method.Invoke(tool.Instance, args);
                }
                else
                {
                    throw new InvalidOperationException($"Tool '{toolCall.Name}' requires an instance but none was provided");
                }

                // Handle async methods
                if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                    if (task.GetType().IsGenericType)
                    {
                        var resultProperty = task.GetType().GetProperty("Result");
                        result = resultProperty?.GetValue(task);
                    }
                    else
                    {
                        result = null;
                    }
                }

                var content = result != null
                    ? JsonSerializer.Serialize(result)
                    : "{}";

                return new ToolResult
                {
                    ToolCallId = toolCall.Id,
                    Content = content,
                    IsError = false
                };
            }
            catch (Exception ex) when (ex is not ToolException)
            {
                _logger?.LogError(ex, "Error executing tool {ToolName}", toolCall.Name);
                return new ToolResult
                {
                    ToolCallId = toolCall.Id,
                    Content = JsonSerializer.Serialize(new { error = ex.Message }),
                    IsError = true
                };
            }
        }

        public async Task<IReadOnlyList<ToolResult>> ExecuteAllAsync(
            IReadOnlyList<ToolCall> toolCalls,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(toolCalls);

            var tasks = toolCalls.Select(tc => ExecuteAsync(tc, cancellationToken));
            return (await Task.WhenAll(tasks)).ToList();
        }
    }
}

