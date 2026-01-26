# DotNetAgents Operations Runbook

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Quick Reference

### Emergency Commands

```bash
# Restart all services
kubectl rollout restart deployment --all -n dotnetagents

# Scale up for capacity
kubectl scale deployment dotnetagents-api --replicas=5 -n dotnetagents

# Check system health
kubectl get pods -n dotnetagents
kubectl top pods -n dotnetagents

# View logs
kubectl logs -f -l app=dotnetagents-api -n dotnetagents

# Check database
kubectl exec -it postgres-0 -n dotnetagents -- psql -U postgres -d dotnetagents
```

## Common Issues

### High Error Rate

**Symptoms:** Error rate > 0.1 errors/second

**Actions:**
1. Check error logs: `kubectl logs -l app=dotnetagents-api -n dotnetagents --tail=100 | grep ERROR`
2. Check Prometheus alerts: `http://prometheus:9090/alerts`
3. Review recent deployments: `kubectl rollout history deployment dotnetagents-api -n dotnetagents`
4. Scale up if needed: `kubectl scale deployment dotnetagents-api --replicas=5 -n dotnetagents`

### Slow Response Times

**Symptoms:** p95 latency > 5 seconds

**Actions:**
1. Check CPU/Memory: `kubectl top pods -n dotnetagents`
2. Check database performance: `kubectl exec -it postgres-0 -n dotnetagents -- psql -U postgres -c "SELECT * FROM pg_stat_activity;"`
3. Check queue depth: `kubectl exec -it api-0 -n dotnetagents -- curl http://localhost/metrics | grep task_queue`
4. Scale workers: `kubectl scale deployment worker-pool --replicas=5 -n dotnetagents`

### No Available Agents

**Symptoms:** All agents busy or unavailable

**Actions:**
1. Check agent status: `kubectl exec -it api-0 -n dotnetagents -- curl http://localhost/agents`
2. Restart agents: `kubectl rollout restart deployment worker-pool -n dotnetagents`
3. Register new agents if needed
4. Check for stuck tasks: `kubectl exec -it supervisor-0 -n dotnetagents -- curl http://localhost/tasks/pending`

### High Task Queue Depth

**Symptoms:** > 100 pending tasks

**Actions:**
1. Scale up workers: `kubectl scale deployment worker-pool --replicas=10 -n dotnetagents`
2. Check worker health: `kubectl get pods -l app=worker -n dotnetagents`
3. Review task processing rate
4. Consider increasing worker concurrency limits

### Database Connection Issues

**Symptoms:** Connection pool exhausted

**Actions:**
1. Check active connections: `kubectl exec -it postgres-0 -n dotnetagents -- psql -U postgres -c "SELECT count(*) FROM pg_stat_activity;"`
2. Kill idle connections: See disaster recovery guide
3. Increase pool size in configuration
4. Restart application: `kubectl rollout restart deployment dotnetagents-api -n dotnetagents`

## Monitoring

### Key Metrics to Watch

- **Request Rate:** `rate(dotnetagents_requests_total[5m])`
- **Error Rate:** `rate(dotnetagents_errors_total[5m])`
- **Response Time:** `histogram_quantile(0.95, rate(dotnetagents_operation_duration_seconds_bucket[5m]))`
- **Queue Depth:** `dotnetagents_task_queue_pending_count`
- **Agent Availability:** `dotnetagents_agents_available_count`
- **LLM Cost:** `rate(dotnetagents_cost_total_usd[1h])`

### Dashboards

- **Overview:** http://grafana:3000/d/dotnetagents-overview
- **Agents:** http://grafana:3000/d/dotnetagents-agents
- **LLM Performance:** http://grafana:3000/d/dotnetagents-llm

## Escalation

### Severity Levels

- **P0 (Critical):** Complete outage - escalate immediately
- **P1 (High):** Major degradation - escalate within 15 minutes
- **P2 (Medium):** Partial impact - escalate within 1 hour
- **P3 (Low):** Minor issues - handle during business hours

### On-Call Rotation

- **Primary:** [Contact Info]
- **Secondary:** [Contact Info]
- **Escalation:** [Contact Info]

## Related Documentation

- [Disaster Recovery](./DISASTER_RECOVERY.md)
- [Alerting Guide](../guides/ALERTING.md)
- [Capacity Planning](./CAPACITY_PLANNING.md)
