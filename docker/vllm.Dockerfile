FROM vllm/vllm-openai:latest

# Configure vLLM for Teaching Assistant
ENV MODEL_NAME=mistralai/Mistral-7B-Instruct-v0.2
ENV MAX_MODEL_LEN=4096
ENV GPU_MEMORY_UTILIZATION=0.9

EXPOSE 8000

HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
  CMD curl -f http://localhost:8000/health || exit 1
