# DotNetAgents.Education

Enterprise-grade educational extensions for DotNetAgents, providing AI-powered tutoring, assessment, and learning management capabilities.

## Overview

DotNetAgents.Education extends the DotNetAgents framework with specialized components for educational applications, including:

- **Pedagogy Components**: Socratic dialogue, spaced repetition, mastery tracking
- **Safety & Compliance**: COPPA, FERPA, GDPR compliance, content filtering, conversation monitoring
- **Assessment**: Question generation, response evaluation, misconception detection
- **Memory & Retrieval**: Student profiles, mastery tracking, curriculum-aware content retrieval
- **Multi-Tenancy**: Tenant isolation, tenant-specific configuration
- **Caching**: Education-specific content caching

## Features

### Pedagogy

- **Socratic Dialogue Engine**: Generates thought-provoking questions, evaluates responses, provides scaffolded hints
- **Spaced Repetition (SM2)**: SuperMemo 2 algorithm for optimal review scheduling
- **Mastery Calculator**: Weighted scoring with prerequisite checking

### Safety

- **Child Safety Filter**: COPPA-compliant content filtering
- **Conversation Monitor**: Detects distress signals, bullying, abuse indicators
- **Age-Adaptive Transformer**: Adjusts content complexity for grade levels

### Assessment

- **Assessment Generator**: Creates multiple question types (multiple choice, short answer, essay, etc.)
- **Response Evaluator**: Scores responses, detects misconceptions, provides feedback

### Memory & Retrieval

- **Student Profile Memory**: Manages student profiles and preferences
- **Mastery State Memory**: Tracks concept mastery over time
- **Learning Session Memory**: Manages learning sessions with resume capability
- **Curriculum-Aware Retriever**: Filters content based on student mastery and prerequisites

### Compliance

- **FERPA Compliance**: Access control, logging, parent consent management
- **GDPR Compliance**: Data export, deletion, anonymization, consent tracking
- **RBAC**: Role-based access control for education roles
- **Audit Logging**: Comprehensive audit trail for educational events

### Infrastructure

- **Multi-Tenancy**: Tenant context, isolation, management
- **Caching**: Education-specific content caching with TTL support

## Getting Started

### Installation

```bash
dotnet add package DotNetAgents.Education
```

### Basic Setup

```csharp
using DotNetAgents.Education;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Add DotNetAgents Core first
services.AddDotNetAgents(config => 
{
    config.WithDefaultLLMProvider("OpenAI");
});

// Add Education extensions
services.AddDotNetAgentsEducation(options =>
{
    // Configure education-specific options
});
```

### Example: Socratic Dialogue

```csharp
using DotNetAgents.Education.Pedagogy;
using DotNetAgents.Education.Models;

var socraticEngine = serviceProvider.GetRequiredService<ISocraticDialogueEngine>();

var concept = new ConceptContext(
    new ConceptId("photosynthesis", SubjectArea.Science, GradeLevel.G6_8),
    "Science",
    GradeLevel.G6_8
);

var understanding = new StudentUnderstanding(
    new MasteryLevel(new ConceptId("photosynthesis", SubjectArea.Science, GradeLevel.G6_8), 0.6, DateTimeOffset.UtcNow),
    0.7,
    Array.Empty<string>()
);

// Generate a Socratic question
var question = await socraticEngine.GenerateQuestionAsync(
    concept,
    understanding,
    cancellationToken: cancellationToken
);

Console.WriteLine($"Question: {question.Text}");
```

### Example: Assessment Generation

```csharp
using DotNetAgents.Education.Assessment;

var assessmentGenerator = serviceProvider.GetRequiredService<IAssessmentGenerator>();

var spec = new AssessmentSpecification
{
    ConceptId = new ConceptId("fractions", SubjectArea.Mathematics, GradeLevel.G3_5),
    QuestionCount = 5,
    QuestionTypes = new[] { QuestionType.MultipleChoice, QuestionType.ShortAnswer },
    DifficultyDistribution = (30, 50, 20), // Easy, Medium, Hard percentages
    GradeLevel = GradeLevel.G3_5
};

var assessment = await assessmentGenerator.GenerateAsync(
    new ConceptId("fractions", SubjectArea.Mathematics, GradeLevel.G3_5),
    spec,
    cancellationToken
);

foreach (var question in assessment.Questions)
{
    Console.WriteLine($"Q: {question.QuestionText}");
    Console.WriteLine($"Correct Answer: {string.Join(", ", question.CorrectAnswers)}");
}
```

### Example: Student Profile Management

```csharp
using DotNetAgents.Education.Memory;

var profileMemory = serviceProvider.GetRequiredService<StudentProfileMemory>();

// Create a student profile
var profile = new StudentProfile
{
    StudentId = "student-123",
    GradeLevel = GradeLevel.G6_8,
    LearningStyle = "visual",
    Interests = new[] { "science", "mathematics" },
    PreferredLanguage = "en"
};

await profileMemory.SaveProfileAsync(profile, cancellationToken);

// Retrieve profile
var retrieved = await profileMemory.GetProfileAsync("student-123", cancellationToken);
```

### Example: Content Filtering

```csharp
using DotNetAgents.Education.Safety;

var contentFilter = serviceProvider.GetRequiredService<IContentFilter>();

var context = new FilterContext(
    "student-123",
    "conversation-456",
    isInput: true,
    metadata: new Dictionary<string, object> { ["grade_level"] = GradeLevel.G6_8 }
);

var result = await contentFilter.FilterInputAsync(
    "I want to learn about photosynthesis",
    context,
    cancellationToken
);

if (result.IsAllowed)
{
    Console.WriteLine($"Filtered content: {result.FilteredContent}");
}
```

## Architecture

### Core Components

- **Pedagogy**: `ISocraticDialogueEngine`, `ISpacedRepetitionScheduler`, `IMasteryCalculator`
- **Safety**: `IContentFilter`, `IConversationMonitor`, `IAgeAdaptiveTransformer`
- **Assessment**: `IAssessmentGenerator`, `IResponseEvaluator`
- **Memory**: `StudentProfileMemory`, `MasteryStateMemory`, `LearningSessionMemory`
- **Retrieval**: `ICurriculumAwareRetriever`, `IPrerequisiteChecker`
- **Compliance**: `IFerpaComplianceService`, `IGdprComplianceService`, `IEducationAuthorizationService`
- **Infrastructure**: `ITenantContext`, `ITenantManager`, `EducationContentCache`

### Integration with DotNetAgents

DotNetAgents.Education integrates seamlessly with DotNetAgents Core:

- Uses `ILLMModel` for question generation and evaluation
- Extends `IMemory` and `IMemoryStore` for student data
- Uses `IVectorStore` for curriculum-aware retrieval
- Integrates with `ICache` for content caching
- Extends `IAuditLogger` for compliance logging

## Requirements

- .NET 10.0 or later
- DotNetAgents.Core v1.0+
- DotNetAgents.Workflow v1.0+
- DotNetAgents.Configuration v1.0+
- DotNetAgents.Security v1.0+

## Documentation

- [Requirements](docs/education/REQUIREMENTS.md)
- [Technical Specification](docs/education/TECHNICAL_SPECIFICATION.md)
- [Implementation Plan](docs/education/IMPLEMENTATION_PLAN.md)

## License

MIT License - see LICENSE file for details.
