# Disaster Recovery Procedures for DotNetAgents

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

This document outlines disaster recovery procedures for DotNetAgents deployments. It covers recovery procedures for various failure scenarios including database failures, message bus outages, agent failures, and complete system failures.

## Table of Contents

1. [Recovery Scenarios](#recovery-scenarios)
2. [Database Failures](#database-failures)
3. [Message Bus Outages](#message-bus-outages)
4. [Agent Failures](#agent-failures)
5. [Complete System Failure](#complete-system-failure)
6. [Recovery Testing](#recovery-testing)
7. [Backup Procedures](#backup-procedures)

## Recovery Scenarios

### Severity Levels

- **P0 (Critical):** Complete system outage, no service available
- **P1 (High):** Major functionality degraded, significant impact
- **P2 (Medium):** Partial functionality affected
- **P3 (Low):** Minor issues, workarounds available

## Database Failures

### PostgreSQL Primary Failure

**Severity:** P0

**Symptoms:**
- Database connection errors
- Agent registry unavailable
- Task queue inaccessible
- Workflow state lost

**Recovery Steps:**

1. **Verify Failure:**
   ```bash
   kubectl get pods -n dotnetagents | grep postgres
   kubectl logs -n dotnetagents postgres-0
   ```

2. **Check Replication Status:**
   ```bash
   kubectl exec -it postgres-0 -n dotnetagents -- psql -U postgres -c "SELECT * FROM pg_stat_replication;"
   ```

3. **Failover to Standby (if available):**
   ```bash
   # Promote standby to primary
   kubectl exec -it postgres-1 -n dotnetagents -- pg_ctl promote
   
   # Update service endpoint
   kubectl patch service postgres -n dotnetagents -p '{"spec":{"selector":{"role":"primary"}}}'
   ```

4. **Restore from Backup (if no standby):**
   ```bash
   # Stop application
   kubectl scale deployment dotnetagents-api --replicas=0 -n dotnetagents
   
   # Restore database
   kubectl exec -it postgres-0 -n dotnetagents -- pg_restore -U postgres -d dotnetagents /backups/latest.dump
   
   # Restart application
   kubectl scale deployment dotnetagents-api --replicas=3 -n dotnetagents
   ```

5. **Verify Recovery:**
   ```bash
   # Check database connectivity
   kubectl exec -it dotnetagents-api-0 -n dotnetagents -- curl http://localhost/health
   
   # Verify agent registry
   kubectl logs -n dotnetagents -l app=dotnetagents-api | grep "AgentRegistry"
   ```

**RTO (Recovery Time Objective):** 15 minutes  
**RPO (Recovery Point Objective):** 5 minutes (with replication)

### Database Connection Pool Exhaustion

**Severity:** P1

**Symptoms:**
- High connection pool usage
- Connection timeout errors
- Slow response times

**Recovery Steps:**

1. **Identify Leaking Connections:**
   ```sql
   SELECT pid, usename, application_name, state, query_start, query
   FROM pg_stat_activity
   WHERE datname = 'dotnetagents'
   ORDER BY query_start;
   ```

2. **Kill Long-Running Queries:**
   ```sql
   SELECT pg_terminate_backend(pid)
   FROM pg_stat_activity
   WHERE state = 'idle in transaction'
     AND query_start < NOW() - INTERVAL '5 minutes';
   ```

3. **Increase Connection Pool Size:**
   ```yaml
   # Update deployment
   env:
     - name: DATABASE_MAX_POOL_SIZE
       value: "100"  # Increase from default
   ```

4. **Restart Application:**
   ```bash
   kubectl rollout restart deployment dotnetagents-api -n dotnetagents
   ```

**RTO:** 5 minutes

## Message Bus Outages

### Kafka Broker Failure

**Severity:** P0

**Symptoms:**
- Agent messages not delivered
- Task assignments failing
- Message queue errors

**Recovery Steps:**

1. **Check Kafka Cluster Status:**
   ```bash
   kubectl get pods -n dotnetagents | grep kafka
   kubectl exec -it kafka-0 -n dotnetagents -- kafka-broker-api-versions --bootstrap-server localhost:9092
   ```

2. **Restart Failed Brokers:**
   ```bash
   kubectl delete pod kafka-0 -n dotnetagents
   # Wait for pod to restart
   kubectl wait --for=condition=ready pod -l app=kafka -n dotnetagents --timeout=5m
   ```

3. **Verify Topic Replication:**
   ```bash
   kubectl exec -it kafka-0 -n dotnetagents -- kafka-topics --bootstrap-server localhost:9092 --describe --topic agent-messages
   ```

4. **Switch to Backup Message Bus (if configured):**
   ```yaml
   # Update configuration to use RabbitMQ fallback
   env:
     - name: MESSAGE_BUS_TYPE
       value: "rabbitmq"
   ```

5. **Restart Affected Services:**
   ```bash
   kubectl rollout restart deployment dotnetagents-api -n dotnetagents
   kubectl rollout restart deployment dotnetagents-supervisor -n dotnetagents
   ```

**RTO:** 10 minutes

### RabbitMQ Failure

**Severity:** P0

**Recovery Steps:**

1. **Check RabbitMQ Status:**
   ```bash
   kubectl get pods -n dotnetagents | grep rabbitmq
   kubectl exec -it rabbitmq-0 -n dotnetagents -- rabbitmqctl status
   ```

2. **Restart RabbitMQ:**
   ```bash
   kubectl delete pod rabbitmq-0 -n dotnetagents
   kubectl wait --for=condition=ready pod -l app=rabbitmq -n dotnetagents --timeout=5m
   ```

3. **Verify Queues:**
   ```bash
   kubectl exec -it rabbitmq-0 -n dotnetagents -- rabbitmqctl list_queues
   ```

**RTO:** 10 minutes

## Agent Failures

### Agent Registry Unavailable

**Severity:** P1

**Symptoms:**
- Cannot register new agents
- Worker pool cannot find agents
- Task assignment failing

**Recovery Steps:**

1. **Check Agent Registry Service:**
   ```bash
   kubectl get pods -n dotnetagents | grep agent-registry
   kubectl logs -n dotnetagents -l app=agent-registry --tail=100
   ```

2. **Restart Agent Registry:**
   ```bash
   kubectl rollout restart deployment agent-registry -n dotnetagents
   kubectl wait --for=condition=ready pod -l app=agent-registry -n dotnetagents --timeout=5m
   ```

3. **Re-register Agents:**
   ```bash
   # Agents should auto-register on startup
   # Verify registration
   kubectl exec -it dotnetagents-api-0 -n dotnetagents -- curl http://localhost/agents
   ```

**RTO:** 5 minutes

### Worker Pool Failure

**Severity:** P1

**Recovery Steps:**

1. **Check Worker Pool Status:**
   ```bash
   kubectl get pods -n dotnetagents | grep worker
   kubectl logs -n dotnetagents -l app=worker --tail=100
   ```

2. **Scale Down and Up:**
   ```bash
   kubectl scale deployment worker-pool --replicas=0 -n dotnetagents
   sleep 10
   kubectl scale deployment worker-pool --replicas=3 -n dotnetagents
   ```

3. **Verify Worker Registration:**
   ```bash
   kubectl exec -it supervisor-0 -n dotnetagents -- curl http://localhost/workers
   ```

**RTO:** 5 minutes

## Complete System Failure

### Full Cluster Failure

**Severity:** P0

**Recovery Steps:**

1. **Assess Damage:**
   ```bash
   kubectl get nodes
   kubectl get pods --all-namespaces
   ```

2. **Restore from Backup:**
   ```bash
   # Restore database
   kubectl apply -f kubernetes/manifests/postgres-deployment.yaml
   kubectl exec -it postgres-0 -n dotnetagents -- pg_restore -U postgres -d dotnetagents /backups/latest.dump
   
   # Restore configuration
   kubectl apply -f kubernetes/manifests/
   
   # Restore secrets
   kubectl apply -f kubernetes/manifests/secrets.yaml
   ```

3. **Verify Services:**
   ```bash
   kubectl get pods -n dotnetagents
   kubectl wait --for=condition=ready pod --all -n dotnetagents --timeout=10m
   ```

4. **Health Checks:**
   ```bash
   curl https://api.dotnetagents.example.com/health
   curl https://api.dotnetagents.example.com/agents
   ```

**RTO:** 30 minutes  
**RPO:** 1 hour (based on backup frequency)

## Recovery Testing

### Test Schedule

- **Weekly:** Database failover test
- **Monthly:** Complete system recovery test
- **Quarterly:** Disaster recovery drill

### Test Procedures

1. **Database Failover Test:**
   ```bash
   # Simulate primary failure
   kubectl delete pod postgres-0 -n dotnetagents
   
   # Verify automatic failover
   # Check application continues operating
   ```

2. **Message Bus Failover Test:**
   ```bash
   # Stop primary message bus
   kubectl scale deployment kafka --replicas=0 -n dotnetagents
   
   # Verify fallback to secondary
   # Check message delivery continues
   ```

3. **Complete Recovery Test:**
   ```bash
   # Stop all services
   kubectl scale deployment --all --replicas=0 -n dotnetagents
   
   # Restore from backup
   # Verify full recovery
   ```

## Backup Procedures

### Database Backups

**Frequency:** Every 6 hours  
**Retention:** 7 days daily, 4 weeks weekly, 12 months monthly

```bash
# Automated backup script
#!/bin/bash
BACKUP_DIR="/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
kubectl exec -it postgres-0 -n dotnetagents -- pg_dump -U postgres dotnetagents > "$BACKUP_DIR/dotnetagents_$TIMESTAMP.dump"
```

### Configuration Backups

**Frequency:** On every change  
**Storage:** Git repository

```bash
# Backup Kubernetes manifests
kubectl get all -n dotnetagents -o yaml > backups/k8s-manifests-$(date +%Y%m%d).yaml
```

### State Backups

**Frequency:** Every hour  
**Storage:** Object storage (S3, Azure Blob)

```bash
# Backup workflow checkpoints
kubectl exec -it api-0 -n dotnetagents -- tar czf /tmp/checkpoints.tar.gz /data/checkpoints
kubectl cp dotnetagents/api-0:/tmp/checkpoints.tar.gz s3://backups/checkpoints-$(date +%Y%m%d_%H%M%S).tar.gz
```

## Runbook Quick Reference

### Emergency Contacts

- **On-Call Engineer:** [Contact Info]
- **Database Team:** [Contact Info]
- **Infrastructure Team:** [Contact Info]

### Quick Recovery Commands

```bash
# Restart all services
kubectl rollout restart deployment --all -n dotnetagents

# Scale up for capacity
kubectl scale deployment dotnetagents-api --replicas=5 -n dotnetagents

# Check system health
kubectl get pods -n dotnetagents
kubectl top pods -n dotnetagents
```

## Related Documentation

- [Alerting Guide](../guides/ALERTING.md)
- [Capacity Planning](./CAPACITY_PLANNING.md)
- [Runbook](./RUNBOOK.md)
