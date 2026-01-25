# TeachingAssistant Content Pipeline

Python-based content extraction and processing pipeline for curriculum content.

## Overview

This pipeline extracts curriculum content from markdown files located at `S:\Obsidian_Shared\Research\Science`, enriches it with LLM-generated metadata, and prepares it for embedding and storage in PostgreSQL with pgvector.

## Structure

```
TeachingAssistant.ContentPipeline/
├── extraction/
│   ├── parser.py          # Markdown parsing logic
│   └── __init__.py
├── enrichment/
│   ├── enricher.py        # LLM enrichment (learning objectives, prerequisites, etc.)
│   └── __init__.py
├── embeddings/
│   ├── generator.py       # Embedding generation using BGE-large
│   └── __init__.py
├── requirements.txt       # Python dependencies
└── README.md
```

## Dependencies

- Python 3.11+
- Anthropic SDK (for Claude API)
- HuggingFace Transformers (for local embeddings)
- PostgreSQL adapter (psycopg2 or asyncpg)
- Markdown parsing libraries

## Usage

```bash
# Install dependencies
pip install -r requirements.txt

# Run content extraction
python -m extraction.parser --input "S:\Obsidian_Shared\Research\Science" --output extracted_content.json

# Run enrichment
python -m enrichment.enricher --input extracted_content.json --output enriched_content.json

# Generate embeddings
python -m embeddings.generator --input enriched_content.json --db-connection "postgresql://..."
```
