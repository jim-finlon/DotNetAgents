# Grafana Dashboards for DotNetAgents

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

This guide explains how to import and use Grafana dashboards for monitoring DotNetAgents. Pre-built dashboards are available in `kubernetes/grafana/dashboards/`.

## Available Dashboards

### 1. DotNetAgents Overview
- **File:** `dotnetagents-overview.json`
- **Purpose:** High-level overview of system health
- **Metrics:**
  - Request rate
  - Error rate
  - Response times (p50, p95)
  - Active agents
  - Task queue depth

### 2. DotNetAgents - Agents
- **File:** `dotnetagents-agents.json`
- **Purpose:** Detailed agent performance monitoring
- **Metrics:**
  - Agent status breakdown
  - Tasks by agent
  - Agent task success rate
  - Agent response times

### 3. DotNetAgents - LLM Performance
- **File:** `dotnetagents-llm.json`
- **Purpose:** LLM call monitoring and cost tracking
- **Metrics:**
  - LLM call rate
  - LLM latency (p50, p95)
  - LLM error rate
  - Token usage
  - Cost per hour

## Importing Dashboards

### Method 1: Via Grafana UI

1. Open Grafana UI (typically http://localhost:3000)
2. Navigate to **Dashboards** → **Import**
3. Click **Upload JSON file**
4. Select the dashboard JSON file from `kubernetes/grafana/dashboards/`
5. Select Prometheus as the data source
6. Click **Import**

### Method 2: Via Grafana API

```bash
# Import dashboard
curl -X POST \
  http://admin:admin@localhost:3000/api/dashboards/db \
  -H 'Content-Type: application/json' \
  -d @kubernetes/grafana/dashboards/dotnetagents-overview.json
```

### Method 3: Via ConfigMap (Kubernetes)

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: grafana-dashboards
  namespace: monitoring
data:
  dotnetagents-overview.json: |
    # Paste dashboard JSON here
```

## Configuring Data Source

1. Navigate to **Configuration** → **Data Sources**
2. Click **Add data source**
3. Select **Prometheus**
4. Configure:
   - **URL:** `http://prometheus:9090`
   - **Access:** Server (default)
5. Click **Save & Test**

## Customizing Dashboards

### Adding New Panels

1. Open dashboard in Grafana
2. Click **Add panel**
3. Configure query:
   ```promql
   rate(dotnetagents_your_metric_total[5m])
   ```
4. Set visualization type and options
5. Click **Apply**

### Modifying Existing Panels

1. Click panel title → **Edit**
2. Modify PromQL query
3. Adjust visualization settings
4. Click **Apply**

## Dashboard Variables

Add variables for filtering:

1. Dashboard settings → **Variables**
2. Click **Add variable**
3. Configure:
   - **Name:** `agent_id`
   - **Type:** Query
   - **Query:** `label_values(dotnetagents_agents_total, agent_id)`
4. Use in queries: `dotnetagents_agents_total{agent_id="$agent_id"}`

## Best Practices

1. **Use Variables:** Create variables for common filters (agent_id, model, etc.)
2. **Set Refresh Intervals:** Configure appropriate refresh rates
3. **Organize Panels:** Group related metrics together
4. **Add Annotations:** Mark deployments and incidents
5. **Set Alerts:** Link panels to Prometheus alerts

## Related Documentation

- [Alerting Guide](./ALERTING.md)
- [Observability Guide](./OBSERVABILITY.md)
- [Distributed Tracing](../examples/DISTRIBUTED_TRACING.md)
