using DotNetAgents.Core.Chains;
using FluentAssertions;
using System.Runtime.CompilerServices;
using Xunit;

namespace DotNetAgents.Core.Tests.Chains;

public class RunnableTests
{
    [Fact]
    public async Task InvokeAsync_WithValidFunction_ExecutesFunction()
    {
        // Arrange
        var runnable = new Runnable<string, string>(
            async (input, ct) => await Task.FromResult($"Hello {input}").ConfigureAwait(false));

        // Act
        var result = await runnable.InvokeAsync("World").ConfigureAwait(false);

        // Assert
        result.Should().Be("Hello World");
    }

    [Fact]
    public async Task InvokeAsync_WithCancellation_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var runnable = new Runnable<string, string>(
            async (input, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return await Task.FromResult("result").ConfigureAwait(false);
            });

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => runnable.InvokeAsync("test", cancellationToken: cts.Token)).ConfigureAwait(false);
    }

    [Fact]
    public async Task StreamAsync_WithValidFunction_ReturnsSingleItem()
    {
        // Arrange
        var runnable = new Runnable<string, string>(
            async (input, ct) => await Task.FromResult($"Hello {input}").ConfigureAwait(false));

        // Act
        var results = new List<string>();
        await foreach (var item in runnable.StreamAsync("World"))
        {
            results.Add(item);
        }

        // Assert
        results.Should().HaveCount(1);
        results[0].Should().Be("Hello World");
    }

    [Fact]
    public async Task BatchAsync_WithMultipleInputs_ProcessesAll()
    {
        // Arrange
        var runnable = new Runnable<int, int>(
            async (input, ct) => await Task.FromResult(input * 2).ConfigureAwait(false));

        var inputs = new[] { 1, 2, 3, 4, 5 };

        // Act
        var results = await runnable.BatchAsync(inputs).ConfigureAwait(false);

        // Assert
        results.Should().HaveCount(5);
        results.Should().BeEquivalentTo(new[] { 2, 4, 6, 8, 10 });
    }

    [Fact]
    public async Task BatchAsync_WithNullInputs_ThrowsArgumentNullException()
    {
        // Arrange
        var runnable = new Runnable<int, int>(
            async (input, ct) => await Task.FromResult(input).ConfigureAwait(false));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => runnable.BatchAsync(null!)).ConfigureAwait(false);
    }

    [Fact]
    public async Task BatchAsync_WithCancellation_RespectsCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var runnable = new Runnable<int, int>(
            async (input, ct) =>
            {
                if (input == 3)
                {
                    cts.Cancel();
                }

                ct.ThrowIfCancellationRequested();
                return await Task.FromResult(input).ConfigureAwait(false);
            });

        var inputs = new[] { 1, 2, 3, 4, 5 };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => runnable.BatchAsync(inputs, cancellationToken: cts.Token)).ConfigureAwait(false);
    }
}

public class RunnableExtensionsTests
{
    [Fact]
    public async Task Pipe_WithTwoRunnables_ComposesCorrectly()
    {
        // Arrange
        var first = new Runnable<int, int>(async (input, ct) => await Task.FromResult(input * 2).ConfigureAwait(false));
        var second = new Runnable<int, string>(async (input, ct) => await Task.FromResult($"Result: {input}").ConfigureAwait(false));

        // Act
        var piped = first.Pipe<int, int, string>(second);
        var result = await piped.InvokeAsync(5).ConfigureAwait(false);

        // Assert
        result.Should().Be("Result: 10");
    }

    [Fact]
    public async Task Pipe_WithNullFirstRunnable_ThrowsArgumentNullException()
    {
        // Arrange
        var second = new Runnable<int, string>(async (input, ct) => await Task.FromResult("test").ConfigureAwait(false));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IRunnable<int, int>?)null)!.Pipe<int, int, string>(second));
    }

    [Fact]
    public async Task Pipe_WithNullSecondRunnable_ThrowsArgumentNullException()
    {
        // Arrange
        var first = new Runnable<int, int>(async (input, ct) => await Task.FromResult(input).ConfigureAwait(false));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => first.Pipe<int, int, string>(null!));
    }

    [Fact]
    public async Task Map_WithMapperFunction_AppliesTransformation()
    {
        // Arrange
        var runnable = new Runnable<int, int>(async (input, ct) => await Task.FromResult(input * 2).ConfigureAwait(false));
        var mapped = runnable.Map(x => x + 1);

        // Act
        var result = await mapped.InvokeAsync(5).ConfigureAwait(false);

        // Assert
        result.Should().Be(11); // (5 * 2) + 1
    }

    [Fact]
    public async Task Map_WithNullRunnable_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IRunnable<int, int>?)null)!.Map(x => x));
    }

    [Fact]
    public async Task Map_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        var runnable = new Runnable<int, int>(async (input, ct) => await Task.FromResult(input).ConfigureAwait(false));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => runnable.Map<int, int>(null!));
    }

    [Fact]
    public async Task Pipe_WithStreaming_ComposesStreams()
    {
        // Arrange
        var first = new Runnable<int, int>(async (input, ct) => await Task.FromResult(input * 2).ConfigureAwait(false));
        var second = new Runnable<int, string>(async (input, ct) => await Task.FromResult($"Result: {input}").ConfigureAwait(false));

        // Act
        var piped = first.Pipe<int, int, string>(second);
        var results = new List<string>();
        await foreach (var item in piped.StreamAsync(5))
        {
            results.Add(item);
        }

        // Assert
        results.Should().HaveCount(1);
        results[0].Should().Be("Result: 10");
    }
}