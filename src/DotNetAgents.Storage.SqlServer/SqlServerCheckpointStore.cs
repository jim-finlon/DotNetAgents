using DotNetAgents.Workflow.Checkpoints;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace DotNetAgents.Storage.SqlServer;

/// <summary>
/// SQL Server implementation of <see cref="ICheckpointStore{TState}"/> for persistent checkpoint storage.
/// </summary>
/// <typeparam name="TState">The type of the workflow state.</typeparam>
public class SqlServerCheckpointStore<TState> : ICheckpointStore<TState> where TState : class
{
    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly IStateSerializer<TState> _serializer;
    private readonly ILogger<SqlServerCheckpointStore<TState>>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerCheckpointStore{TState}"/> class.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="tableName">The table name for storing checkpoints. Default: "WorkflowCheckpoints".</param>
    /// <param name="serializer">The state serializer to use.</param>
    /// <param name="logger">Optional logger for tracking operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public SqlServerCheckpointStore(
        string connectionString,
        string tableName = "WorkflowCheckpoints",
        IStateSerializer<TState>? serializer = null,
        ILogger<SqlServerCheckpointStore<TState>>? logger = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        _serializer = serializer ?? new JsonStateSerializer<TState>();
        _logger = logger;

        // Ensure table exists
        EnsureTableExistsAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async Task<string> SaveAsync(
        string runId,
        TState state,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        ArgumentNullException.ThrowIfNull(state);

        try
        {
            var serializedState = await _serializer.SerializeAsync(state, cancellationToken).ConfigureAwait(false);
            var checkpointId = Guid.NewGuid().ToString("N");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var command = new SqlCommand(
                $@"INSERT INTO [{_tableName}] (CheckpointId, RunId, State, CreatedAt, ExpiresAt)
                   VALUES (@CheckpointId, @RunId, @State, @CreatedAt, @ExpiresAt)",
                connection);

            command.Parameters.AddWithValue("@CheckpointId", checkpointId);
            command.Parameters.AddWithValue("@RunId", runId);
            command.Parameters.AddWithValue("@State", serializedState);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@ExpiresAt", DateTime.UtcNow.AddDays(30)); // Default 30-day expiration

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _logger?.LogInformation(
                "Checkpoint saved. RunId: {RunId}, CheckpointId: {CheckpointId}",
                runId,
                checkpointId);

            return checkpointId;
        }
        catch (SqlException ex)
        {
            _logger?.LogError(ex, "Failed to save checkpoint. RunId: {RunId}", runId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Checkpoint<TState>?> GetAsync(
        string checkpointId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(checkpointId);

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var command = new SqlCommand(
                $@"SELECT CheckpointId, RunId, State, CreatedAt, ExpiresAt
                   FROM [{_tableName}]
                   WHERE CheckpointId = @CheckpointId AND (ExpiresAt IS NULL OR ExpiresAt > @Now)",
                connection);

            command.Parameters.AddWithValue("@CheckpointId", checkpointId);
            command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

            using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            var runId = reader.GetString(reader.GetOrdinal("RunId"));
            var serializedState = reader.GetString(reader.GetOrdinal("State"));
            var createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
            var expiresAtOrdinal = reader.GetOrdinal("ExpiresAt");
            var expiresAt = reader.IsDBNull(expiresAtOrdinal) ? (DateTime?)null : reader.GetDateTime(expiresAtOrdinal);

            var state = await _serializer.DeserializeAsync(serializedState, cancellationToken).ConfigureAwait(false);

            return new Checkpoint<TState>
            {
                CheckpointId = checkpointId,
                RunId = runId,
                State = state,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt
            };
        }
        catch (SqlException ex)
        {
            _logger?.LogError(ex, "Failed to get checkpoint. CheckpointId: {CheckpointId}", checkpointId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Checkpoint<TState>?> GetLatestAsync(
        string runId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var command = new SqlCommand(
                $@"SELECT TOP 1 CheckpointId, RunId, State, CreatedAt, ExpiresAt
                   FROM [{_tableName}]
                   WHERE RunId = @RunId AND (ExpiresAt IS NULL OR ExpiresAt > @Now)
                   ORDER BY CreatedAt DESC",
                connection);

            command.Parameters.AddWithValue("@RunId", runId);
            command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

            using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            var checkpointId = reader.GetString(reader.GetOrdinal("CheckpointId"));
            var serializedState = reader.GetString(reader.GetOrdinal("State"));
            var createdAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"));
            var expiresAtOrdinal = reader.GetOrdinal("ExpiresAt");
            var expiresAt = reader.IsDBNull(expiresAtOrdinal) ? (DateTime?)null : reader.GetDateTime(expiresAtOrdinal);

            var state = await _serializer.DeserializeAsync(serializedState, cancellationToken).ConfigureAwait(false);

            return new Checkpoint<TState>
            {
                CheckpointId = checkpointId,
                RunId = runId,
                State = state,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt
            };
        }
        catch (SqlException ex)
        {
            _logger?.LogError(ex, "Failed to get latest checkpoint. RunId: {RunId}", runId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(
        string checkpointId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(checkpointId);

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var command = new SqlCommand(
                $@"DELETE FROM [{_tableName}] WHERE CheckpointId = @CheckpointId",
                connection);

            command.Parameters.AddWithValue("@CheckpointId", checkpointId);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _logger?.LogInformation(
                "Checkpoint deleted. CheckpointId: {CheckpointId}, RowsAffected: {RowsAffected}",
                checkpointId,
                rowsAffected);

            return rowsAffected > 0;
        }
        catch (SqlException ex)
        {
            _logger?.LogError(ex, "Failed to delete checkpoint. CheckpointId: {CheckpointId}", checkpointId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            var command = new SqlCommand(
                $@"DELETE FROM [{_tableName}] WHERE ExpiresAt IS NOT NULL AND ExpiresAt < @Now",
                connection);

            command.Parameters.AddWithValue("@Now", DateTime.UtcNow);

            var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            _logger?.LogInformation("Cleaned up {Count} expired checkpoints", rowsAffected);

            return rowsAffected;
        }
        catch (SqlException ex)
        {
            _logger?.LogError(ex, "Failed to cleanup expired checkpoints");
            throw;
        }
    }

    private async Task EnsureTableExistsAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            var command = new SqlCommand(
                $@"IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{_tableName}]') AND type in (N'U'))
                   BEGIN
                       CREATE TABLE [{_tableName}] (
                           CheckpointId NVARCHAR(50) PRIMARY KEY,
                           RunId NVARCHAR(100) NOT NULL,
                           State NVARCHAR(MAX) NOT NULL,
                           CreatedAt DATETIME2 NOT NULL,
                           ExpiresAt DATETIME2 NULL,
                           INDEX IX_RunId (RunId),
                           INDEX IX_ExpiresAt (ExpiresAt)
                       )
                   END",
                connection);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

            _logger?.LogInformation("Ensured checkpoint table exists: {TableName}", _tableName);
        }
        catch (SqlException ex)
        {
            _logger?.LogError(ex, "Failed to ensure checkpoint table exists");
            throw;
        }
    }
}