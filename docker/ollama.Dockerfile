FROM ollama/ollama:latest

# Pre-pull models for Teaching Assistant
# Models will be pulled on first use if not pre-pulled
ENV OLLAMA_HOST=0.0.0.0

EXPOSE 11434

HEALTHCHECK --interval=30s --timeout=10s --retries=3 \
  CMD curl -f http://localhost:11434/api/tags || exit 1
