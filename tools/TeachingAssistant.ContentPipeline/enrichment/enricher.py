"""
LLM enrichment pipeline for curriculum content.

Enriches content units with learning objectives, prerequisites, misconceptions, and other pedagogical metadata.
"""
import json
import re
import anthropic
from typing import Dict, List
from dataclasses import dataclass
from pathlib import Path


@dataclass
class EnrichedContent:
    """Enriched content metadata."""
    learning_objectives: List[str]
    prerequisites: List[str]
    misconceptions: List[Dict[str, str]]
    discussion_questions: List[str]
    vocabulary: Dict[str, List[str]]  # tier -> words
    real_world_applications: List[str]


ENRICHMENT_PROMPT = """
Given this educational content, extract and generate the following:

### Input Content:
{content_text}

### Subject: {subject}
### Grade Band: {grade_band}

### Required Extractions:

1. **Learning Objectives** (3-5 measurable outcomes starting with action verbs):
   - Format: "Students will be able to [verb] [content]"

2. **Prerequisites** (concepts student must understand first):
   - List specific concepts, not general subjects

3. **Common Misconceptions** (2-4 mistakes students typically make):
   - Include the misconception and the correct understanding

4. **Socratic Discussion Questions** (3-5 questions to guide discovery):
   - Questions that lead students to insights, not direct answers

5. **Vocabulary Classification**:
   - Tier 1 (everyday words): [list]
   - Tier 2 (academic vocabulary): [list]
   - Tier 3 (domain-specific): [list]

6. **Real-World Connections** (2-3 practical applications):
   - Relevant to student's daily life

### Output Format: JSON
{{
  "learning_objectives": ["..."],
  "prerequisites": ["..."],
  "misconceptions": [{{"misconception": "...", "correction": "..."}}],
  "discussion_questions": ["..."],
  "vocabulary": {{"tier1": [], "tier2": [], "tier3": []}},
  "real_world_applications": ["..."]
}}
"""


class ContentEnricher:
    """Enriches content units with pedagogical metadata using LLM."""

    def __init__(self, api_key: str):
        """Initialize enricher with Anthropic API key."""
        self.client = anthropic.Anthropic(api_key=api_key)

    def enrich_content(self, content_unit: Dict) -> EnrichedContent:
        """Enrich a content unit with pedagogical metadata."""
        prompt = ENRICHMENT_PROMPT.format(
            content_text=content_unit['full_text'][:4000],  # Limit context
            subject=content_unit['subject'],
            grade_band=content_unit['grade_band']
        )

        try:
            response = self.client.messages.create(
                model="claude-sonnet-4-20250514",
                max_tokens=2000,
                messages=[{"role": "user", "content": prompt}]
            )

            # Parse JSON from response
            response_text = response.content[0].text
            # Find JSON block
            json_match = re.search(r'\{[\s\S]*\}', response_text)
            if json_match:
                data = json.loads(json_match.group())
                return EnrichedContent(**data)
            else:
                raise ValueError("Could not parse JSON from response")
        except Exception as e:
            print(f"Error enriching content {content_unit.get('id', 'unknown')}: {e}")
            # Return empty enrichment on error
            return EnrichedContent(
                learning_objectives=[],
                prerequisites=[],
                misconceptions=[],
                discussion_questions=[],
                vocabulary={"tier1": [], "tier2": [], "tier3": []},
                real_world_applications=[]
            )

    def enrich_batch(self, content_units: List[Dict], output_path: str):
        """Enrich multiple content units."""
        enriched = []

        for i, unit in enumerate(content_units):
            print(f"Enriching {i+1}/{len(content_units)}: {unit['title']}")
            try:
                enrichment = self.enrich_content(unit)
                unit['enrichment'] = {
                    'learning_objectives': enrichment.learning_objectives,
                    'prerequisites': enrichment.prerequisites,
                    'misconceptions': enrichment.misconceptions,
                    'discussion_questions': enrichment.discussion_questions,
                    'vocabulary': enrichment.vocabulary,
                    'real_world_applications': enrichment.real_world_applications
                }
                enriched.append(unit)
            except Exception as e:
                print(f"  Error: {e}")
                enriched.append(unit)  # Keep unenriched

        output_file = Path(output_path)
        output_file.parent.mkdir(parents=True, exist_ok=True)
        output_file.write_text(
            json.dumps(enriched, indent=2, ensure_ascii=False),
            encoding='utf-8'
        )
        print(f"Enriched {len(enriched)} units to {output_path}")


if __name__ == '__main__':
    import sys
    import os

    api_key = os.getenv('ANTHROPIC_API_KEY')
    if not api_key:
        print("Error: ANTHROPIC_API_KEY environment variable not set")
        sys.exit(1)

    input_path = sys.argv[1] if len(sys.argv) > 1 else './extracted_content.json'
    output_path = sys.argv[2] if len(sys.argv) > 2 else './enriched_content.json'

    with open(input_path, 'r', encoding='utf-8') as f:
        content_units = json.load(f)

    enricher = ContentEnricher(api_key)
    enricher.enrich_batch(content_units, output_path)
