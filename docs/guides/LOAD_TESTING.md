# Load Testing Guide for DotNetAgents

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

This guide explains how to run load tests for DotNetAgents to validate performance and scalability.

## Running Load Tests

### Run All Tests

```bash
dotnet run --project tests/DotNetAgents.LoadTests
```

### Run Specific Test

```bash
# Agent registry tests
dotnet run --project tests/DotNetAgents.LoadTests -- registry

# Task queue tests
dotnet run --project tests/DotNetAgents.LoadTests -- taskqueue

# Worker pool tests
dotnet run --project tests/DotNetAgents.LoadTests -- workerpool
```

## Test Scenarios

### Agent Registry Load Test

- **Scenario:** Register agents at high rate
- **Load:** 100 registrations/second
- **Duration:** 1 minute
- **Metrics:**
  - Registration latency (p50, p95, p99)
  - Success rate
  - Throughput

### Task Queue Load Test

- **Scenarios:**
  - Enqueue: 200 tasks/second
  - Dequeue: 100 tasks/second
- **Duration:** 2 minutes each
- **Metrics:**
  - Enqueue/dequeue latency
  - Queue depth
  - Throughput

### Worker Pool Load Test

- **Scenarios:**
  - Get available worker: 50 requests/second
  - Get statistics: 10 requests/second
- **Duration:** 1 minute each
- **Metrics:**
  - Response latency
  - Success rate
  - Worker selection time

## Interpreting Results

### Key Metrics

- **RPS (Requests Per Second):** Actual throughput achieved
- **Latency (p50, p95, p99):** Response time percentiles
- **Success Rate:** Percentage of successful requests
- **Data Transfer:** Bytes sent/received

### Performance Targets

- **Agent Registry:** p95 < 100ms at 100 RPS
- **Task Queue:** p95 < 50ms at 200 RPS
- **Worker Pool:** p95 < 50ms at 50 RPS

## Customizing Tests

### Modify Load Patterns

Edit test files to change:
- Injection rate
- Test duration
- Ramp-up patterns

Example:
```csharp
.WithLoadSimulations(
    Simulation.RampPerSec(rate: 10, during: TimeSpan.FromSeconds(30)), // Ramp up
    Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(2)), // Sustained load
    Simulation.RampPerSec(rate: 0, during: TimeSpan.FromSeconds(30)) // Ramp down
)
```

## CI Integration

Add to CI pipeline:

```yaml
- name: Run Load Tests
  run: |
    dotnet run --project tests/DotNetAgents.LoadTests -- --report-format json --report-folder ./load-test-results
```

## Related Documentation

- [Performance Benchmarks](../PERFORMANCE_BENCHMARKS.md)
- [Capacity Planning](../operations/CAPACITY_PLANNING.md)
- [Disaster Recovery](../operations/DISASTER_RECOVERY.md)
