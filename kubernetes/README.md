# Kubernetes Deployment

## Prerequisites
- Kubernetes cluster (1.24+)
- kubectl configured
- Helm 3.x
- NVIDIA GPU nodes (for LLM workloads)
- Ingress controller (nginx recommended)
- cert-manager (optional, for TLS certificates)

## Quick Start with Helm

The easiest way to deploy is using the Helm chart:

```bash
# Add Bitnami repository
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update

# Create namespace
kubectl create namespace teaching-assistant

# Install with Helm
helm install teaching-assistant ./helm/teaching-assistant \
  --namespace teaching-assistant \
  --set secrets.jwtSecretKey="your-secret-key" \
  --set secrets.postgresPassword="your-postgres-password"
```

See `helm/teaching-assistant/README.md` for detailed Helm chart documentation.

## Manual Deployment with Manifests

### 1. Create Namespace

```bash
kubectl apply -f manifests/namespace.yaml
```

### 2. Create Secrets

**IMPORTANT**: Never commit actual secrets to version control!

Copy the example secrets file and fill in actual values:

```bash
cp manifests/secrets.yaml.example manifests/secrets.yaml
# Edit secrets.yaml with actual values
kubectl apply -f manifests/secrets.yaml
```

Or create secrets directly:

```bash
kubectl create secret generic teaching-assistant-secrets \
  --namespace teaching-assistant \
  --from-literal=CONNECTIONSTRINGS__DEFAULTCONNECTION="Host=postgres-service;Port=5432;..." \
  --from-literal=JWT_SETTINGS__SECRETKEY="your-secret-key"
```

### 3. Deploy ConfigMap

```bash
kubectl apply -f manifests/configmap.yaml
```

### 4. Deploy PostgreSQL

Option A: Use Bitnami Helm chart (recommended):
```bash
helm install postgres bitnami/postgresql \
  --namespace teaching-assistant \
  --set auth.postgresPassword=your-password \
  --set auth.database=teaching_assistant \
  --set image.repository=pgvector/pgvector \
  --set image.tag=pg16
```

Option B: Use provided manifest:
```bash
kubectl apply -f manifests/postgres-deployment.yaml
```

### 5. Deploy LLM Services

```bash
# Deploy vLLM
kubectl apply -f manifests/vllm-deployment.yaml

# Deploy Ollama
kubectl apply -f manifests/ollama-deployment.yaml
```

**Note**: Ensure your cluster has GPU nodes and NVIDIA device plugin installed.

### 6. Deploy Application Services

```bash
# Deploy API
kubectl apply -f manifests/api-deployment.yaml

# Deploy Student UI
kubectl apply -f manifests/student-ui-deployment.yaml

# Deploy Parent UI
kubectl apply -f manifests/parent-ui-deployment.yaml

# Deploy Admin UI
kubectl apply -f manifests/admin-ui-deployment.yaml
```

### 7. Deploy Ingress

```bash
kubectl apply -f manifests/ingress.yaml
```

### 8. Deploy Monitoring (Optional)

```bash
# Deploy Prometheus
kubectl apply -f monitoring/prometheus-deployment.yaml

# Deploy Grafana
kubectl apply -f monitoring/grafana-deployment.yaml

# Deploy Loki and Promtail
kubectl apply -f monitoring/loki-deployment.yaml
```

## Verify Deployment

```bash
# Check all pods
kubectl get pods -n teaching-assistant

# Check services
kubectl get services -n teaching-assistant

# Check ingress
kubectl get ingress -n teaching-assistant

# View logs
kubectl logs -n teaching-assistant -l app=teaching-assistant-api
```

## Accessing Services

### Port Forwarding (for local access)

```bash
# API
kubectl port-forward -n teaching-assistant svc/teaching-assistant-api 5000:80

# Student UI
kubectl port-forward -n teaching-assistant svc/teaching-assistant-student-ui 5001:80

# Parent UI
kubectl port-forward -n teaching-assistant svc/teaching-assistant-parent-ui 5002:80

# Admin UI
kubectl port-forward -n teaching-assistant svc/teaching-assistant-admin-ui 5003:80

# Grafana
kubectl port-forward -n teaching-assistant svc/grafana 3000:3000

# Prometheus
kubectl port-forward -n teaching-assistant svc/prometheus 9090:9090
```

### Via Ingress

If ingress is configured:
- API: `http://api.teachingassistant.local`
- Student UI: `http://student.teachingassistant.local`
- Parent UI: `http://parent.teachingassistant.local`
- Admin UI: `http://admin.teachingassistant.local`

## Troubleshooting

### Pods not starting

```bash
# Check pod status
kubectl describe pod <pod-name> -n teaching-assistant

# Check logs
kubectl logs <pod-name> -n teaching-assistant

# Check events
kubectl get events -n teaching-assistant --sort-by='.lastTimestamp'
```

### GPU not available

```bash
# Check nodes with GPU
kubectl get nodes -l accelerator=nvidia-tesla-k80

# Check device plugin
kubectl get pods -n kube-system | grep nvidia-device-plugin
```

### Database connection issues

```bash
# Check PostgreSQL pod
kubectl logs -n teaching-assistant -l app=postgres

# Test connection from API pod
kubectl exec -it -n teaching-assistant <api-pod-name> -- \
  psql -h postgres-service -U postgres -d teaching_assistant
```

### Monitoring not working

```bash
# Check Prometheus targets
kubectl port-forward -n teaching-assistant svc/prometheus 9090:9090
# Then visit http://localhost:9090/targets

# Check Loki
kubectl logs -n teaching-assistant -l app=loki

# Check Promtail
kubectl logs -n teaching-assistant -l app=promtail
```

## Scaling

### Scale API replicas

```bash
kubectl scale deployment teaching-assistant-api -n teaching-assistant --replicas=4
```

### Scale UI replicas

```bash
kubectl scale deployment teaching-assistant-student-ui -n teaching-assistant --replicas=3
kubectl scale deployment teaching-assistant-parent-ui -n teaching-assistant --replicas=3
```

## Updating

### Update application images

```bash
# Set new image
kubectl set image deployment/teaching-assistant-api \
  api=teaching-assistant-api:v1.1.0 \
  -n teaching-assistant

# Or use Helm
helm upgrade teaching-assistant ./helm/teaching-assistant \
  --namespace teaching-assistant \
  --set api.image.tag=v1.1.0
```

## Cleanup

### Remove all resources

```bash
# Using Helm
helm uninstall teaching-assistant --namespace teaching-assistant

# Or manually
kubectl delete namespace teaching-assistant
```

## Directory Structure

```
kubernetes/
├── manifests/          # Kubernetes manifest files
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secrets.yaml.example
│   ├── api-deployment.yaml
│   ├── student-ui-deployment.yaml
│   ├── parent-ui-deployment.yaml
│   ├── admin-ui-deployment.yaml
│   ├── vllm-deployment.yaml
│   ├── ollama-deployment.yaml
│   ├── postgres-deployment.yaml
│   └── ingress.yaml
├── monitoring/        # Monitoring stack manifests
│   ├── prometheus-deployment.yaml
│   ├── grafana-deployment.yaml
│   └── loki-deployment.yaml
├── helm/              # Helm charts
│   └── teaching-assistant/
│       ├── Chart.yaml
│       ├── values.yaml
│       ├── templates/
│       └── README.md
└── README.md          # This file
```

## Additional Resources

- [Helm Chart Documentation](helm/teaching-assistant/README.md)
- [Monitoring Setup Guide](../docs/guides/MONITORING.md)
- [Database Setup Guide](../infrastructure/DEV_DATABASE.md)
