"""
Markdown parser for curriculum content extraction.

Parses markdown files from the Science curriculum directory and extracts structured content units.
"""
import re
import yaml
import json
import hashlib
from pathlib import Path
from dataclasses import dataclass, asdict
from typing import List, Optional, Dict
from datetime import datetime


@dataclass
class ContentUnit:
    """Represents a structured content unit."""
    id: str
    type: str  # concept, lesson, assessment, lab, vocabulary, review
    subject: str  # biology, chemistry, physics, etc.
    grade_band: str  # K-2, 3-5, 6-8, 9-10, 11-12
    topic_path: List[str]
    title: str
    summary: str
    full_text: str
    source_file: str
    prerequisites: List[str] = None
    learning_objectives: List[str] = None
    vocabulary_tier: int = None
    ngss_standards: List[str] = None
    estimated_duration_minutes: int = None
    created_at: str = None
    updated_at: str = None

    def __post_init__(self):
        if self.prerequisites is None:
            self.prerequisites = []
        if self.learning_objectives is None:
            self.learning_objectives = []
        if self.ngss_standards is None:
            self.ngss_standards = []
        if self.created_at is None:
            self.created_at = datetime.utcnow().isoformat()
        if self.updated_at is None:
            self.updated_at = datetime.utcnow().isoformat()


class CurriculumParser:
    """Parses curriculum markdown files into structured content units."""

    GRADE_BAND_MAP = {
        'K-2': 'K2',
        '3-5': 'G3_5',
        '6-8': 'G6_8',
        '9-10': 'G9_10',
        '11-12': 'G11_12',
        'elementary': 'G3_5',
        'middle_school': 'G6_8',
        'high_school': 'G9_10'
    }

    SUBJECT_MAP = {
        'biology': 'Biology',
        'chemistry': 'Chemistry',
        'physics': 'Physics',
        'earth_science': 'EarthScience',
        'astronomy': 'Astronomy',
        'environmental_science': 'EnvironmentalScience',
        'mathematics': 'Mathematics'
    }

    def __init__(self, base_path: str):
        """Initialize parser with base path to curriculum directory."""
        self.base_path = Path(base_path)

    def _generate_id(self, file_path: Path, suffix: str = '') -> str:
        """Generate unique ID from file path."""
        source = str(file_path) + suffix
        return hashlib.sha256(source.encode()).hexdigest()[:16]

    def _extract_topic_path(self, file_path: Path) -> List[str]:
        """Extract topic hierarchy from file path."""
        rel_path = file_path.relative_to(self.base_path)
        # Skip subject, curriculum folders, grade folders
        parts = [p for p in rel_path.parts if p not in
                 ['curriculum', 'knowledge_base', 'elementary', 'middle_school', 'high_school']]
        return [p.lower().replace(' ', '_') for p in parts[1:-1]]

    def _determine_subject(self, file_path: Path) -> str:
        """Determine subject from file path."""
        rel_path = file_path.relative_to(self.base_path)
        subject_part = rel_path.parts[0].lower().replace(' ', '_')
        return self.SUBJECT_MAP.get(subject_part, 'Biology')

    def _determine_grade_band(self, file_path: Path, content: str) -> str:
        """Determine grade band from file path or content."""
        rel_path = file_path.relative_to(self.base_path)
        
        # Check path for grade indicators
        for part in rel_path.parts:
            if part in self.GRADE_BAND_MAP:
                return self.GRADE_BAND_MAP[part]
        
        # Check content for grade indicators
        grade_match = re.search(r'\[([K\d]+-\d+)\]', content)
        if grade_match:
            grade_str = grade_match.group(1)
            return self.GRADE_BAND_MAP.get(grade_str, 'G6_8')
        
        return 'G6_8'  # Default

    def parse_knowledge_base_article(self, file_path: Path) -> ContentUnit:
        """Extract content from knowledge base format."""
        content = file_path.read_text(encoding='utf-8')

        # Extract YAML frontmatter if present
        frontmatter = {}
        if content.startswith('---'):
            parts = content.split('---', 2)
            if len(parts) >= 3:
                try:
                    frontmatter = yaml.safe_load(parts[1]) or {}
                except yaml.YAMLError:
                    frontmatter = {}
                content = parts[2]

        # Extract title from first heading
        title_match = re.search(r'^#\s+(.+)$', content, re.MULTILINE)
        title = title_match.group(1) if title_match else file_path.stem

        # Extract summary (first paragraph after title)
        paragraphs = re.split(r'\n\n+', content)
        summary = ''
        for p in paragraphs[1:]:
            if not p.startswith('#') and len(p) > 50:
                summary = p[:500].strip()
                break

        subject = self._determine_subject(file_path)
        grade_band = self._determine_grade_band(file_path, content)
        topic_path = self._extract_topic_path(file_path)

        return ContentUnit(
            id=self._generate_id(file_path),
            type='concept',
            subject=subject,
            grade_band=grade_band,
            topic_path=topic_path,
            title=title,
            summary=summary,
            full_text=content,
            source_file=str(file_path),
            vocabulary_tier=frontmatter.get('vocabulary_tier'),
            ngss_standards=frontmatter.get('ngss_standards', [])
        )

    def parse_lesson_plan(self, file_path: Path) -> List[ContentUnit]:
        """Extract lessons from lesson plan files."""
        content = file_path.read_text(encoding='utf-8')
        units = []

        # Split by major sections (## headers)
        sections = re.split(r'^##\s+', content, flags=re.MULTILINE)

        subject = self._determine_subject(file_path)
        grade_band = self._determine_grade_band(file_path, content)
        topic_path = self._extract_topic_path(file_path)

        for section in sections[1:]:  # Skip content before first ##
            lines = section.split('\n')
            title = lines[0].strip()

            if not title:
                continue

            section_content = '\n'.join(lines[1:])
            summary = section_content[:500] if len(section_content) > 500 else section_content

            units.append(ContentUnit(
                id=self._generate_id(file_path, title),
                type='lesson',
                subject=subject,
                grade_band=grade_band,
                topic_path=topic_path,
                title=title,
                summary=summary,
                full_text=section_content,
                source_file=str(file_path)
            ))

        return units

    def process_all_content(self, output_path: str):
        """Process all curriculum content and write to JSON."""
        all_units = []

        base = Path(self.base_path)

        # Process knowledge base articles
        for kb_file in base.glob('*/knowledge_base/**/*.md'):
            try:
                unit = self.parse_knowledge_base_article(kb_file)
                all_units.append(asdict(unit))
            except Exception as e:
                print(f"Error processing {kb_file}: {e}")

        # Process lesson plans
        for lesson_file in base.glob('*/curriculum/**/*.md'):
            if 'FRAMEWORK' not in lesson_file.name.upper():
                try:
                    units = self.parse_lesson_plan(lesson_file)
                    all_units.extend([asdict(u) for u in units])
                except Exception as e:
                    print(f"Error processing {lesson_file}: {e}")

        # Write output
        output_file = Path(output_path)
        output_file.parent.mkdir(parents=True, exist_ok=True)
        output_file.write_text(
            json.dumps(all_units, indent=2, ensure_ascii=False),
            encoding='utf-8'
        )
        print(f"Extracted {len(all_units)} content units to {output_path}")


if __name__ == '__main__':
    import sys
    base_path = sys.argv[1] if len(sys.argv) > 1 else r'S:\Obsidian_Shared\Research\Science'
    output_path = sys.argv[2] if len(sys.argv) > 2 else './extracted_content.json'
    
    parser = CurriculumParser(base_path)
    parser.process_all_content(output_path)
