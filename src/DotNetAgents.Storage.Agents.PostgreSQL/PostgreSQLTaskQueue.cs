using DotNetAgents.Agents.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;
using TaskStatus = DotNetAgents.Agents.Tasks.TaskStatus;

namespace DotNetAgents.Storage.Agents.PostgreSQL;

/// <summary>
/// PostgreSQL implementation of <see cref="ITaskQueue"/>.
/// Suitable for distributed deployments requiring persistent task queues.
/// </summary>
public class PostgreSQLTaskQueue : ITaskQueue
{
    private readonly string _connectionString;
    private readonly ILogger<PostgreSQLTaskQueue>? _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSQLTaskQueue"/> class.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    /// <param name="logger">Optional logger instance.</param>
    public PostgreSQLTaskQueue(
        string connectionString,
        ILogger<PostgreSQLTaskQueue>? logger = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task EnqueueAsync(
        WorkerTask task,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureSchemaExistsAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            INSERT INTO task_queue (
                task_id, task_type, input_data, required_capability, preferred_agent_id,
                priority, timeout_ms, metadata, created_at, status
            )
            VALUES (
                @task_id, @task_type, @input_data, @required_capability, @preferred_agent_id,
                @priority, @timeout_ms, @metadata, @created_at, @status
            )";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("task_id", task.TaskId);
        command.Parameters.AddWithValue("task_type", task.TaskType);
        command.Parameters.AddWithValue("input_data", JsonSerializer.Serialize(task.Input, _jsonOptions));
        command.Parameters.AddWithValue("required_capability", (object?)task.RequiredCapability ?? DBNull.Value);
        command.Parameters.AddWithValue("preferred_agent_id", (object?)task.PreferredAgentId ?? DBNull.Value);
        command.Parameters.AddWithValue("priority", task.Priority);
        command.Parameters.AddWithValue("timeout_ms", (object?)task.Timeout?.TotalMilliseconds ?? DBNull.Value);
        command.Parameters.AddWithValue("metadata", JsonSerializer.Serialize(task.Metadata, _jsonOptions));
        command.Parameters.AddWithValue("created_at", task.CreatedAt);
        command.Parameters.AddWithValue("status", (int)TaskStatus.Pending);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        _logger?.LogDebug("Enqueued task {TaskId} to PostgreSQL queue", task.TaskId);
    }

    /// <inheritdoc />
    public async Task<WorkerTask?> DequeueAsync(
        string? agentId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureSchemaExistsAsync(cancellationToken).ConfigureAwait(false);

        string sql;
        if (!string.IsNullOrEmpty(agentId))
        {
            sql = @"
                UPDATE task_queue
                SET status = @assigned_status, assigned_at = NOW()
                WHERE task_id = (
                    SELECT task_id
                    FROM task_queue
                    WHERE status = @pending_status
                      AND (preferred_agent_id IS NULL OR preferred_agent_id = @agent_id)
                    ORDER BY priority DESC, created_at ASC
                    LIMIT 1
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING task_id, task_type, input_data, required_capability, preferred_agent_id,
                          priority, timeout_ms, metadata, created_at";
        }
        else
        {
            sql = @"
                UPDATE task_queue
                SET status = @assigned_status, assigned_at = NOW()
                WHERE task_id = (
                    SELECT task_id
                    FROM task_queue
                    WHERE status = @pending_status
                    ORDER BY priority DESC, created_at ASC
                    LIMIT 1
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING task_id, task_type, input_data, required_capability, preferred_agent_id,
                          priority, timeout_ms, metadata, created_at";
        }

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("assigned_status", (int)TaskStatus.Assigned);
        command.Parameters.AddWithValue("pending_status", (int)TaskStatus.Pending);
        if (!string.IsNullOrEmpty(agentId))
        {
            command.Parameters.AddWithValue("agent_id", agentId);
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return MapToWorkerTask(reader);
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<int> GetPendingCountAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureSchemaExistsAsync(cancellationToken).ConfigureAwait(false);

        const string sql = "SELECT COUNT(*) FROM task_queue WHERE status = @pending_status";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("pending_status", (int)TaskStatus.Pending);

        var count = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(count);
    }

    /// <inheritdoc />
    public async Task<WorkerTask?> PeekAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureSchemaExistsAsync(cancellationToken).ConfigureAwait(false);

        const string sql = @"
            SELECT task_id, task_type, input_data, required_capability, preferred_agent_id,
                   priority, timeout_ms, metadata, created_at
            FROM task_queue
            WHERE status = @pending_status
            ORDER BY priority DESC, created_at ASC
            LIMIT 1";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("pending_status", (int)TaskStatus.Pending);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

        if (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            return MapToWorkerTask(reader);
        }

        return null;
    }

    private WorkerTask MapToWorkerTask(NpgsqlDataReader reader)
    {
        var inputJson = reader.GetString(reader.GetOrdinal("input_data"));
        var input = JsonSerializer.Deserialize<object>(inputJson, _jsonOptions) ?? new object();

        var metadataJson = reader.GetString(reader.GetOrdinal("metadata"));
        var metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson, _jsonOptions)
            ?? new Dictionary<string, object>();

        TimeSpan? timeoutMs = null;
        if (!reader.IsDBNull(reader.GetOrdinal("timeout_ms")))
        {
            timeoutMs = TimeSpan.FromMilliseconds(reader.GetInt64(reader.GetOrdinal("timeout_ms")));
        }

        return new WorkerTask
        {
            TaskId = reader.GetString(reader.GetOrdinal("task_id")),
            TaskType = reader.GetString(reader.GetOrdinal("task_type")),
            Input = input,
            RequiredCapability = reader.IsDBNull(reader.GetOrdinal("required_capability"))
                ? null
                : reader.GetString(reader.GetOrdinal("required_capability")),
            PreferredAgentId = reader.IsDBNull(reader.GetOrdinal("preferred_agent_id"))
                ? null
                : reader.GetString(reader.GetOrdinal("preferred_agent_id")),
            Priority = reader.GetInt32(reader.GetOrdinal("priority")),
            Timeout = timeoutMs,
            Metadata = metadata,
            CreatedAt = new DateTimeOffset(reader.GetDateTime(reader.GetOrdinal("created_at")))
        };
    }

    private async Task EnsureSchemaExistsAsync(CancellationToken cancellationToken)
    {
        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS task_queue (
                task_id VARCHAR(255) PRIMARY KEY,
                task_type VARCHAR(255) NOT NULL,
                input_data JSONB NOT NULL,
                required_capability VARCHAR(255),
                preferred_agent_id VARCHAR(255),
                priority INTEGER NOT NULL DEFAULT 0,
                timeout_ms BIGINT,
                metadata JSONB NOT NULL DEFAULT '{}'::JSONB,
                status INTEGER NOT NULL DEFAULT 0,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                assigned_at TIMESTAMPTZ
            );

            CREATE INDEX IF NOT EXISTS idx_task_queue_status ON task_queue(status);
            CREATE INDEX IF NOT EXISTS idx_task_queue_priority ON task_queue(priority DESC, created_at ASC);
            CREATE INDEX IF NOT EXISTS idx_task_queue_type ON task_queue(task_type);
            CREATE INDEX IF NOT EXISTS idx_task_queue_preferred_agent ON task_queue(preferred_agent_id)
                WHERE preferred_agent_id IS NOT NULL;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = new NpgsqlCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
