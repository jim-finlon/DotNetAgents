# Teaching Assistant Helm Chart

This Helm chart deploys the TeachingAssistant application on Kubernetes.

## Prerequisites

- Kubernetes 1.24+
- Helm 3.x
- NVIDIA GPU nodes (for LLM workloads)
- kubectl configured

## Installation

### 1. Add Bitnami Helm repository (for PostgreSQL)

```bash
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update
```

### 2. Create namespace

```bash
kubectl create namespace teaching-assistant
```

### 3. Create secrets

Create a `secrets.yaml` file with your actual secrets:

```yaml
jwtSecretKey: "your-secret-key-at-least-32-characters"
postgresPassword: "your-postgres-password"
openaiApiKey: "your-openai-api-key"
anthropicApiKey: "your-anthropic-api-key"
```

### 4. Install the chart

```bash
helm install teaching-assistant ./kubernetes/helm/teaching-assistant \
  --namespace teaching-assistant \
  --set secrets.jwtSecretKey="your-secret-key" \
  --set secrets.postgresPassword="your-postgres-password" \
  --set secrets.openaiApiKey="your-openai-api-key" \
  --set secrets.anthropicApiKey="your-anthropic-api-key" \
  --set postgresql.auth.postgresPassword="your-postgres-password"
```

### 5. Verify installation

```bash
kubectl get pods -n teaching-assistant
kubectl get services -n teaching-assistant
```

## Configuration

See `values.yaml` for all configurable parameters.

### Key Parameters

- `api.replicaCount`: Number of API replicas (default: 2)
- `vllm.enabled`: Enable vLLM service (default: true)
- `ollama.enabled`: Enable Ollama service (default: true)
- `postgresql.enabled`: Enable PostgreSQL (default: true)
- `monitoring.prometheus.enabled`: Enable Prometheus (default: true)
- `monitoring.grafana.enabled`: Enable Grafana (default: true)
- `monitoring.loki.enabled`: Enable Loki (default: true)

## Upgrading

```bash
helm upgrade teaching-assistant ./kubernetes/helm/teaching-assistant \
  --namespace teaching-assistant \
  --reuse-values
```

## Uninstalling

```bash
helm uninstall teaching-assistant --namespace teaching-assistant
```

## Accessing Services

- API: `http://api.teachingassistant.local`
- Student UI: `http://student.teachingassistant.local`
- Parent UI: `http://parent.teachingassistant.local`
- Admin UI: `http://admin.teachingassistant.local`
- Grafana: Port-forward to `grafana:3000`
- Prometheus: Port-forward to `prometheus:9090`

## Troubleshooting

### Check pod logs

```bash
kubectl logs -n teaching-assistant <pod-name>
```

### Check service status

```bash
kubectl describe service -n teaching-assistant <service-name>
```

### Check persistent volumes

```bash
kubectl get pvc -n teaching-assistant
```
