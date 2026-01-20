namespace DotNetAgents.Core.Chains;

/// <summary>
/// Chain that executes multiple runnables sequentially, passing output of one as input to the next.
/// </summary>
/// <typeparam name="TInput">The input type of the first runnable.</typeparam>
/// <typeparam name="TOutput">The output type of the last runnable.</typeparam>
public class SequentialChain<TInput, TOutput> : IRunnable<TInput, TOutput>
{
    private readonly IReadOnlyList<IRunnable<object, object>> _runnables;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequentialChain{TInput, TOutput}"/> class.
    /// </summary>
    /// <param name="runnables">The runnables to execute in sequence.</param>
    /// <exception cref="ArgumentNullException">Thrown when runnables is null or empty.</exception>
    public SequentialChain(IReadOnlyList<IRunnable<object, object>> runnables)
    {
        if (runnables == null || runnables.Count == 0)
            throw new ArgumentException("Runnables cannot be null or empty.", nameof(runnables));

        _runnables = runnables;
    }

    /// <inheritdoc/>
    public async Task<TOutput> InvokeAsync(
        TInput input,
        RunnableOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        object current = input!;

        foreach (var runnable in _runnables)
        {
            current = await runnable.InvokeAsync(current, options, cancellationToken).ConfigureAwait(false);
        }

        return (TOutput)current;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<TOutput> StreamAsync(
        TInput input,
        RunnableOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // For sequential chains, streaming is complex - we'll just invoke and yield
        var result = await InvokeAsync(input, options, cancellationToken).ConfigureAwait(false);
        yield return result;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TOutput>> BatchAsync(
        IEnumerable<TInput> inputs,
        RunnableOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (inputs == null)
            throw new ArgumentNullException(nameof(inputs));

        var results = new List<TOutput>();
        foreach (var input in inputs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await InvokeAsync(input, options, cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }

        return results;
    }
}