# Teaching Assistant Infrastructure

## Docker Compose Setup

### Prerequisites
- Docker and Docker Compose
- NVIDIA GPU with CUDA support (for LLM containers)
- NVIDIA Container Toolkit

### Quick Start

```bash
# Start all services
docker-compose up -d

# Check service health
docker-compose ps

# View logs
docker-compose logs -f vllm
docker-compose logs -f ollama
docker-compose logs -f postgres
```

### Services

- **PostgreSQL**: Database with pgvector extension
- **vLLM**: Local LLM inference server (Mistral-7B)
- **Ollama**: Alternative local LLM server

### Health Monitoring

Model router health monitoring is handled by `ModelHealthMonitor` service in the API.
