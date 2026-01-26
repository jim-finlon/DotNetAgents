# Capacity Planning Guide for DotNetAgents

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

This guide helps you plan capacity for DotNetAgents deployments based on expected load and performance requirements.

## Resource Requirements

### Per Agent Instance

- **CPU:** 500m (0.5 cores) minimum, 2 cores recommended
- **Memory:** 512Mi minimum, 2Gi recommended
- **Storage:** 10Gi for logs and temporary data

### Per API Instance

- **CPU:** 1 core minimum, 4 cores recommended
- **Memory:** 1Gi minimum, 4Gi recommended
- **Storage:** 20Gi for logs

### Database (PostgreSQL)

- **CPU:** 2 cores minimum, 8 cores recommended
- **Memory:** 4Gi minimum, 16Gi recommended
- **Storage:** 100Gi minimum, 500Gi recommended (depends on retention)

## Scaling Calculations

### Worker Pool Sizing

```
Required Workers = (Tasks per second × Average task duration) / Max concurrent tasks per worker
```

**Example:**
- 100 tasks/second
- Average duration: 2 seconds
- Max concurrent: 3 tasks/worker
- Required: (100 × 2) / 3 = 67 workers

### API Instance Sizing

```
Required Instances = (Requests per second × Average response time) / Target response time
```

**Example:**
- 1000 requests/second
- Average response: 200ms
- Target: 100ms
- Required: (1000 × 0.2) / 0.1 = 2000 instances (or optimize response time)

## Performance Targets

### Response Times

- **API Requests:** p95 < 500ms
- **LLM Calls:** p95 < 30s
- **Task Assignment:** p95 < 100ms
- **Database Queries:** p95 < 50ms

### Throughput

- **API Requests:** 1000+ requests/second per instance
- **Tasks:** 100+ tasks/second per worker
- **LLM Calls:** 10+ calls/second per instance

## Monitoring Capacity

### Key Metrics

- **CPU Usage:** Should stay < 80%
- **Memory Usage:** Should stay < 90%
- **Queue Depth:** Should stay < 100 pending tasks
- **Response Times:** Monitor p50, p95, p99

### Scaling Triggers

- **Scale Up:** CPU > 70% for 5 minutes
- **Scale Down:** CPU < 30% for 15 minutes
- **Queue Depth:** Scale up if > 50 pending tasks

## Related Documentation

- [Disaster Recovery](./DISASTER_RECOVERY.md)
- [Runbook](./RUNBOOK.md)
- [Performance Benchmarks](../PERFORMANCE_BENCHMARKS.md)
