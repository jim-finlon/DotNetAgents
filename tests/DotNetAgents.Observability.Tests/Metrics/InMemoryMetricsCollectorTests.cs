using DotNetAgents.Observability.Metrics;
using FluentAssertions;
using Xunit;

namespace DotNetAgents.Observability.Tests.Metrics;

public class InMemoryMetricsCollectorTests
{
    [Fact]
    public void RecordLatency_WithValidInput_RecordsSuccessfully()
    {
        // Arrange
        var collector = new InMemoryMetricsCollector();

        // Act
        collector.RecordLatency("test-operation", TimeSpan.FromMilliseconds(100));

        // Assert
        var metrics = collector.GetAllMetrics();
        metrics.Should().ContainKey("latency:test-operation");
    }

    [Fact]
    public void IncrementCounter_WithValidInput_IncrementsCounter()
    {
        // Arrange
        var collector = new InMemoryMetricsCollector();

        // Act
        collector.IncrementCounter("test-counter", 5);

        // Assert
        var metrics = collector.GetAllMetrics();
        metrics.Should().ContainKey("counter:test-counter");
        metrics["counter:test-counter"].Should().HaveCount(1);
        metrics["counter:test-counter"][0].Value.Should().Be(5);
    }

    [Fact]
    public void RecordGauge_WithValidInput_RecordsGauge()
    {
        // Arrange
        var collector = new InMemoryMetricsCollector();

        // Act
        collector.RecordGauge("test-gauge", 42.5);

        // Assert
        var metrics = collector.GetAllMetrics();
        metrics.Should().ContainKey("gauge:test-gauge");
        metrics["gauge:test-gauge"][0].Value.Should().Be(42.5);
    }

    [Fact]
    public void RecordHistogram_WithValidInput_RecordsHistogram()
    {
        // Arrange
        var collector = new InMemoryMetricsCollector();

        // Act
        collector.RecordHistogram("test-histogram", 10.0);

        // Assert
        var metrics = collector.GetAllMetrics();
        metrics.Should().ContainKey("histogram:test-histogram");
    }

    [Fact]
    public void Clear_RemovesAllMetrics()
    {
        // Arrange
        var collector = new InMemoryMetricsCollector();
        collector.RecordLatency("test", TimeSpan.FromMilliseconds(100));

        // Act
        collector.Clear();

        // Assert
        var metrics = collector.GetAllMetrics();
        metrics.Should().BeEmpty();
    }

    [Fact]
    public void RecordLatency_WithNullOperationName_ThrowsException()
    {
        // Arrange
        var collector = new InMemoryMetricsCollector();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => collector.RecordLatency(null!, TimeSpan.FromMilliseconds(100)));
    }
}