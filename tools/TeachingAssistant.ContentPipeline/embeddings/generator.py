"""
Embedding generation pipeline for curriculum content.

Generates embeddings using BGE-large model and stores them in PostgreSQL with pgvector.
"""
import asyncio
import httpx
import asyncpg
from typing import List, Dict
from dataclasses import dataclass
from pathlib import Path
import json


@dataclass
class ContentChunk:
    """Represents a chunk of content for embedding."""
    content_unit_id: str
    chunk_index: int
    text: str
    metadata: Dict


class SemanticChunker:
    """Chunks content for embedding while preserving semantic boundaries."""

    def __init__(
        self,
        max_chunk_tokens: int = 512,
        overlap_tokens: int = 64,
        min_chunk_tokens: int = 100
    ):
        self.max_chunk = max_chunk_tokens
        self.overlap = overlap_tokens
        self.min_chunk = min_chunk_tokens

    def chunk_content(self, content_unit: Dict) -> List[ContentChunk]:
        """Split content into overlapping semantic chunks."""
        text = content_unit['full_text']
        chunks = []

        # First, split by major sections
        sections = self._split_by_headers(text)

        chunk_idx = 0
        for section in sections:
            section_chunks = self._chunk_section(section, content_unit, chunk_idx)
            chunks.extend(section_chunks)
            chunk_idx += len(section_chunks)

        return chunks

    def _split_by_headers(self, text: str) -> List[str]:
        """Split text by markdown headers."""
        sections = re.split(r'\n(?=##\s)', text)
        return [s.strip() for s in sections if s.strip()]

    def _chunk_section(
        self,
        section: str,
        content_unit: Dict,
        start_idx: int
    ) -> List[ContentChunk]:
        """Chunk a single section with overlap."""
        # Simple word-based chunking (in production, use tiktoken)
        words = section.split()
        chunks = []

        if len(words) <= self.max_chunk:
            # Section fits in one chunk
            return [ContentChunk(
                content_unit_id=content_unit['id'],
                chunk_index=start_idx,
                text=section,
                metadata={
                    'subject': content_unit['subject'],
                    'grade_band': content_unit['grade_band'],
                    'topic_path': '/'.join(content_unit.get('topic_path', [])),
                    'title': content_unit['title']
                }
            )]

        # Split into overlapping chunks
        i = 0
        chunk_idx = start_idx
        while i < len(words):
            end = min(i + self.max_chunk, len(words))
            chunk_words = words[i:end]

            if len(chunk_words) >= self.min_chunk:
                chunks.append(ContentChunk(
                    content_unit_id=content_unit['id'],
                    chunk_index=chunk_idx,
                    text=' '.join(chunk_words),
                    metadata={
                        'subject': content_unit['subject'],
                        'grade_band': content_unit['grade_band'],
                        'topic_path': '/'.join(content_unit.get('topic_path', [])),
                        'title': content_unit['title']
                    }
                ))
                chunk_idx += 1

            i += self.max_chunk - self.overlap

        return chunks


class EmbeddingGenerator:
    """Generate embeddings using local text-embeddings-inference server."""

    def __init__(self, server_url: str = "http://localhost:8003"):
        """Initialize generator with embedding server URL."""
        self.server_url = server_url

    async def embed_texts(self, texts: List[str]) -> List[List[float]]:
        """Generate embeddings for a batch of texts."""
        async with httpx.AsyncClient(timeout=60.0) as client:
            response = await client.post(
                f"{self.server_url}/embed",
                json={"inputs": texts}
            )
            response.raise_for_status()
            return response.json()

    async def embed_chunks_to_db(
        self,
        chunks: List[ContentChunk],
        db_connection_string: str,
        batch_size: int = 32
    ):
        """Embed chunks and store in PostgreSQL with pgvector."""
        conn = await asyncpg.connect(db_connection_string)

        try:
            for i in range(0, len(chunks), batch_size):
                batch = chunks[i:i + batch_size]
                texts = [c.text for c in batch]

                embeddings = await self.embed_texts(texts)

                # Insert into database
                for chunk, embedding in zip(batch, embeddings):
                    await conn.execute("""
                        INSERT INTO content_embeddings
                        (content_unit_id, chunk_index, chunk_text, embedding, metadata)
                        VALUES ($1, $2, $3, $4::vector, $5::jsonb)
                        ON CONFLICT (content_unit_id, chunk_index)
                        DO UPDATE SET embedding = EXCLUDED.embedding
                    """,
                        chunk.content_unit_id,
                        chunk.chunk_index,
                        chunk.text,
                        embedding,
                        json.dumps(chunk.metadata)
                    )

                print(f"Embedded batch {i//batch_size + 1}/{(len(chunks) + batch_size - 1)//batch_size}")
        finally:
            await conn.close()


if __name__ == '__main__':
    import sys
    import os
    import re

    input_path = sys.argv[1] if len(sys.argv) > 1 else './enriched_content.json'
    db_connection = sys.argv[2] if len(sys.argv) > 2 else os.getenv('DATABASE_URL', 'postgresql://localhost/teachingassistant')
    embedding_server = sys.argv[3] if len(sys.argv) > 3 else 'http://localhost:8003'

    with open(input_path, 'r', encoding='utf-8') as f:
        content_units = json.load(f)

    chunker = SemanticChunker()
    generator = EmbeddingGenerator(embedding_server)

    all_chunks = []
    for unit in content_units:
        chunks = chunker.chunk_content(unit)
        all_chunks.extend(chunks)

    print(f"Generated {len(all_chunks)} chunks from {len(content_units)} content units")
    asyncio.run(generator.embed_chunks_to_db(all_chunks, db_connection))
