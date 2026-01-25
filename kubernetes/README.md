# Kubernetes Deployment

## Prerequisites
- Kubernetes cluster (1.24+)
- kubectl configured
- Helm 3.x
- NVIDIA GPU nodes (for LLM workloads)

## Deployment Steps

1. Create namespace:
```bash
kubectl create namespace teaching-assistant
```

2. Install PostgreSQL with pgvector:
```bash
helm install postgres bitnami/postgresql \
  --namespace teaching-assistant \
  --set postgresqlExtensions=vector
```

3. Deploy LLM services (vLLM/Ollama) with GPU support

4. Deploy API and frontend services

## Helm Charts

Helm charts for all services will be created in `kubernetes/helm/` directory.
