# Alerting Guide for DotNetAgents

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

This guide explains how to configure and use Prometheus alerting rules for DotNetAgents. The alerting rules monitor critical metrics and notify you when thresholds are exceeded.

## Alerting Rules

Alerting rules are defined in `kubernetes/monitoring/prometheus-alerts.yml`. These rules monitor:

- Error rates
- Response times
- Queue depths
- Agent availability
- LLM call performance
- Cost tracking
- System resources
- Workflow execution
- Message bus performance
- Database connections
- Circuit breaker states

## Setting Up Alerts

### 1. Configure Prometheus

Add the alerting rules to your Prometheus configuration:

```yaml
# prometheus-config.yml
global:
  scrape_interval: 15s
  evaluation_interval: 30s

rule_files:
  - /etc/prometheus/alerts/dotnetagents-alerts.yml

alerting:
  alertmanagers:
    - static_configs:
        - targets:
            - alertmanager:9093
```

### 2. Deploy Alertmanager

```yaml
# alertmanager-deployment.yml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: alertmanager
spec:
  replicas: 1
  template:
    spec:
      containers:
      - name: alertmanager
        image: prom/alertmanager:latest
        ports:
        - containerPort: 9093
```

### 3. Configure Alert Notifications

Create `alertmanager-config.yml`:

```yaml
global:
  resolve_timeout: 5m

route:
  group_by: ['alertname', 'severity']
  group_wait: 10s
  group_interval: 10s
  repeat_interval: 12h
  receiver: 'default'
  routes:
    - match:
        severity: critical
      receiver: 'critical'
    - match:
        severity: warning
      receiver: 'warning'

receivers:
  - name: 'default'
    webhook_configs:
      - url: 'http://webhook:8080/alerts'
  
  - name: 'critical'
    email_configs:
      - to: 'ops-team@example.com'
        from: 'alertmanager@example.com'
        smarthost: 'smtp.example.com:587'
        auth_username: 'alertmanager'
        auth_password: 'password'
    slack_configs:
      - api_url: 'https://hooks.slack.com/services/YOUR/WEBHOOK/URL'
        channel: '#alerts-critical'
        title: 'Critical Alert'
  
  - name: 'warning'
    slack_configs:
      - api_url: 'https://hooks.slack.com/services/YOUR/WEBHOOK/URL'
        channel: '#alerts-warning'
        title: 'Warning Alert'
```

## Alert Descriptions

### Critical Alerts

#### HighErrorRate
- **Threshold:** > 0.1 errors/second
- **Duration:** 5 minutes
- **Action:** Investigate error logs immediately

#### NoAvailableAgents
- **Threshold:** 0 available agents
- **Duration:** 2 minutes
- **Action:** Check agent registration and health

#### HighLLMFailureRate
- **Threshold:** > 5% failure rate
- **Duration:** 5 minutes
- **Action:** Check LLM provider status and API keys

#### DatabaseConnectionPoolExhausted
- **Threshold:** > 90% connections in use
- **Duration:** 5 minutes
- **Action:** Increase connection pool size or investigate connection leaks

### Warning Alerts

#### SlowResponseTime
- **Threshold:** 95th percentile > 5 seconds
- **Duration:** 10 minutes
- **Action:** Investigate performance bottlenecks

#### HighTaskQueueDepth
- **Threshold:** > 100 pending tasks
- **Duration:** 5 minutes
- **Action:** Scale up workers or investigate processing delays

#### HighCostRate
- **Threshold:** > $10/second
- **Duration:** 5 minutes
- **Action:** Review cost usage and optimize LLM calls

#### HighMemoryUsage
- **Threshold:** > 90% of limit
- **Duration:** 5 minutes
- **Action:** Increase memory limits or investigate memory leaks

## Customizing Alerts

### Modify Thresholds

Edit `kubernetes/monitoring/prometheus-alerts.yml`:

```yaml
- alert: HighErrorRate
  expr: |
    rate(dotnetagents_errors_total[5m]) > 0.05  # Changed from 0.1
  for: 5m
```

### Add Custom Alerts

```yaml
- alert: CustomAlert
  expr: |
    your_custom_metric > threshold
  for: 5m
  labels:
    severity: warning
    component: custom
  annotations:
    summary: "Custom alert triggered"
    description: "Description of the alert"
```

### Add Alert Labels

```yaml
- alert: HighErrorRate
  expr: |
    rate(dotnetagents_errors_total[5m]) > 0.1
  labels:
    severity: critical
    component: error_rate
    environment: production  # Custom label
    team: platform          # Custom label
```

## Testing Alerts

### 1. Test Alert Rule Syntax

```bash
promtool check rules kubernetes/monitoring/prometheus-alerts.yml
```

### 2. Test Alert Evaluation

```bash
promtool test rules kubernetes/monitoring/prometheus-alerts.yml
```

### 3. Manually Trigger Alert

Use Prometheus query to simulate alert condition:

```promql
# Force high error rate
rate(dotnetagents_errors_total[5m]) > 0.1
```

## Alert Response Procedures

### 1. Receive Alert

- Check alert severity and component
- Review alert description and annotations
- Check related metrics in Grafana

### 2. Investigate

- Review logs for the affected component
- Check system resources (CPU, memory, disk)
- Review recent deployments or changes

### 3. Resolve

- Fix the underlying issue
- Verify alert resolves automatically
- Document the incident

## Integration with Notification Channels

### Slack Integration

```yaml
receivers:
  - name: 'slack'
    slack_configs:
      - api_url: 'https://hooks.slack.com/services/YOUR/WEBHOOK/URL'
        channel: '#dotnetagents-alerts'
        title: '{{ .GroupLabels.alertname }}'
        text: '{{ .CommonAnnotations.description }}'
        send_resolved: true
```

### PagerDuty Integration

```yaml
receivers:
  - name: 'pagerduty'
    pagerduty_configs:
      - service_key: 'YOUR_PAGERDUTY_SERVICE_KEY'
        description: '{{ .CommonAnnotations.description }}'
```

### Email Integration

```yaml
receivers:
  - name: 'email'
    email_configs:
      - to: 'ops-team@example.com'
        from: 'alertmanager@example.com'
        smarthost: 'smtp.example.com:587'
        auth_username: 'alertmanager'
        auth_password: 'password'
        headers:
          Subject: 'DotNetAgents Alert: {{ .GroupLabels.alertname }}'
```

## Best Practices

1. **Set Appropriate Thresholds**: Base thresholds on baseline metrics and SLA requirements
2. **Use Alert Grouping**: Group related alerts to avoid alert fatigue
3. **Set Reasonable Durations**: Avoid alerting on transient spikes
4. **Document Runbooks**: Create runbooks for each alert type
5. **Regular Review**: Review and adjust alerts based on false positives
6. **Test Alerts**: Regularly test alert delivery channels

## Related Documentation

- [Observability Guide](./OBSERVABILITY.md)
- [Distributed Tracing](../examples/DISTRIBUTED_TRACING.md)
- [Grafana Dashboards](./GRAFANA_DASHBOARDS.md)
