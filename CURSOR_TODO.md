# DotLangChain Educational Extensions - Implementation Plan

## Overview

This document outlines the implementation plan for extending DotLangChain to support the **Science Companion AI** educational platform. These extensions will be implemented as a new package: `DotLangChain.Education`.

**Related Project:** `/mnt/workspace/Obsidian_Shared/Research/Science/AI_Companion_Project/`

---

## Phase 1: Core Framework Updates

### 1.1 Upgrade to .NET 10

- [ ] Update `Directory.Build.props` to target `net10.0`
- [ ] Update all project files to `net10.0`
- [ ] Update NuGet dependencies to .NET 10 compatible versions
- [ ] Test compilation and fix any breaking changes
- [ ] Update CI/CD pipeline for .NET 10 SDK

**Files to modify:**
- `Directory.Build.props`
- `src/DotLangChain.Abstractions/DotLangChain.Abstractions.csproj`
- `src/DotLangChain.Core/DotLangChain.Core.csproj`
- `tests/DotLangChain.Tests.Unit/DotLangChain.Tests.Unit.csproj`

### 1.2 Microsoft.Extensions.AI Compatibility

- [ ] Add `Microsoft.Extensions.AI` package reference
- [ ] Implement `IChatClient` adapter for `IChatCompletionService`
- [ ] Implement `IEmbeddingGenerator<string, Embedding<float>>` adapter
- [ ] Create extension methods for DI registration with MEAI
- [ ] Add middleware support for MEAI pipeline

**New files:**
```
src/DotLangChain.Extensions.AI/
├── DotLangChain.Extensions.AI.csproj
├── ChatClientAdapter.cs
├── EmbeddingGeneratorAdapter.cs
└── ServiceCollectionExtensions.cs
```

---

## Phase 2: DotLangChain.Education Package

### 2.1 Project Setup

- [ ] Create new project `DotLangChain.Education`
- [ ] Add project references to Abstractions and Core
- [ ] Configure package metadata for NuGet
- [ ] Set up folder structure

**Structure:**
```
src/DotLangChain.Education/
├── DotLangChain.Education.csproj
├── Pedagogy/
├── Safety/
├── Assessment/
├── Memory/
└── Retrieval/
```

### 2.2 Pedagogy Components

#### 2.2.1 Socratic Dialogue Engine

- [ ] Create `ISocraticDialogueEngine` interface
- [ ] Implement `SocraticDialogueEngine` with question scaffolding
- [ ] Create `SocraticQuestionType` enum (Clarifying, Probing, Assumption, Implication, Viewpoint)
- [ ] Implement `SocraticDialogueState` for graph-based tutoring
- [ ] Create pre-built `SocraticTutorGraph` using `IGraphBuilder`

**Files:**
```
Pedagogy/
├── ISocraticDialogueEngine.cs
├── SocraticDialogueEngine.cs
├── SocraticQuestionType.cs
├── SocraticDialogueState.cs
└── Graphs/
    └── SocraticTutorGraph.cs
```

**Interface design:**
```csharp
public interface ISocraticDialogueEngine
{
    Task<SocraticQuestion> GenerateQuestionAsync(
        ConceptContext concept,
        StudentUnderstanding currentLevel,
        CancellationToken ct = default);

    Task<UnderstandingAssessment> EvaluateResponseAsync(
        string studentResponse,
        SocraticQuestion question,
        CancellationToken ct = default);

    Task<ScaffoldedHint> GenerateHintAsync(
        SocraticQuestion question,
        int hintLevel,
        CancellationToken ct = default);
}
```

#### 2.2.2 Spaced Repetition Scheduler

- [ ] Create `ISpacedRepetitionScheduler` interface
- [ ] Implement `SM2Scheduler` (SuperMemo 2 algorithm)
- [ ] Create `ReviewItem` record for tracking items
- [ ] Implement `ReviewSchedule` for batch scheduling
- [ ] Add support for custom difficulty adjustments

**Files:**
```
Pedagogy/
├── ISpacedRepetitionScheduler.cs
├── SM2Scheduler.cs
├── ReviewItem.cs
├── ReviewSchedule.cs
└── SpacedRepetitionOptions.cs
```

**Interface design:**
```csharp
public interface ISpacedRepetitionScheduler
{
    ReviewSchedule CalculateNextReview(
        ReviewItem item,
        PerformanceRating rating);

    IReadOnlyList<ReviewItem> GetDueItems(
        IEnumerable<ReviewItem> items,
        DateTimeOffset asOf);

    float CalculateRetention(
        ReviewItem item,
        DateTimeOffset asOf);
}

public enum PerformanceRating
{
    CompleteBlackout = 0,    // No recall
    IncorrectButRemembered = 1,
    IncorrectButEasy = 2,
    CorrectWithDifficulty = 3,
    CorrectWithHesitation = 4,
    Perfect = 5
}
```

#### 2.2.3 Mastery Calculator

- [ ] Create `IMasteryCalculator` interface
- [ ] Implement `MasteryCalculator` with weighted scoring
- [ ] Create `MasteryLevel` record with thresholds
- [ ] Implement concept dependency tracking
- [ ] Add mastery decay over time

**Files:**
```
Pedagogy/
├── IMasteryCalculator.cs
├── MasteryCalculator.cs
├── MasteryLevel.cs
├── ConceptMastery.cs
└── MasteryCalculatorOptions.cs
```

**Interface design:**
```csharp
public interface IMasteryCalculator
{
    MasteryLevel CalculateMastery(
        ConceptId concept,
        IReadOnlyList<AssessmentResult> history);

    bool MeetsPrerequisites(
        ConceptId targetConcept,
        IReadOnlyDictionary<ConceptId, MasteryLevel> studentMastery);

    IReadOnlyList<ConceptId> GetReadyConcepts(
        IReadOnlyList<ConceptId> availableConcepts,
        IReadOnlyDictionary<ConceptId, MasteryLevel> studentMastery);
}
```

### 2.3 Safety Components

#### 2.3.1 Child Safety Filter

- [ ] Create `IContentFilter` interface (pre/post filtering)
- [ ] Implement `ChildSafetyFilter` for COPPA compliance
- [ ] Create blocked/sensitive pattern configurations
- [ ] Implement `ContentFilterResult` with categories
- [ ] Add configurable severity levels

**Files:**
```
Safety/
├── IContentFilter.cs
├── ChildSafetyFilter.cs
├── ContentFilterResult.cs
├── ContentCategory.cs
├── ChildSafetyFilterOptions.cs
└── Patterns/
    ├── BlockedPatterns.cs
    └── SensitivePatterns.cs
```

**Interface design:**
```csharp
public interface IContentFilter
{
    Task<ContentFilterResult> FilterInputAsync(
        string input,
        FilterContext context,
        CancellationToken ct = default);

    Task<ContentFilterResult> FilterOutputAsync(
        string output,
        FilterContext context,
        CancellationToken ct = default);
}

public record ContentFilterResult(
    bool IsAllowed,
    string? FilteredContent,
    IReadOnlyList<ContentCategory> FlaggedCategories,
    bool RequiresReview,
    string? ReviewReason);
```

#### 2.3.2 Conversation Monitor

- [ ] Create `IConversationMonitor` interface
- [ ] Implement `ConversationMonitor` for concerning content detection
- [ ] Create `MonitoringAlert` record for flagged interactions
- [ ] Implement alert severity levels and routing
- [ ] Add pattern detection for distress signals

**Files:**
```
Safety/
├── IConversationMonitor.cs
├── ConversationMonitor.cs
├── MonitoringAlert.cs
├── AlertSeverity.cs
└── ConversationMonitorOptions.cs
```

#### 2.3.3 Age-Adaptive Middleware

- [ ] Create `IAgeAdaptiveTransformer` interface
- [ ] Implement `AgeAdaptiveMiddleware` for grade-level responses
- [ ] Create grade-level vocabulary mappings
- [ ] Implement complexity scoring
- [ ] Add response length adjustments by age

**Files:**
```
Safety/
├── IAgeAdaptiveTransformer.cs
├── AgeAdaptiveMiddleware.cs
├── GradeLevel.cs
├── ComplexityScore.cs
└── AgeAdaptiveOptions.cs
```

**Interface design:**
```csharp
public interface IAgeAdaptiveTransformer
{
    Task<string> TransformPromptAsync(
        string prompt,
        GradeLevel gradeLevel,
        CancellationToken ct = default);

    Task<string> TransformResponseAsync(
        string response,
        GradeLevel gradeLevel,
        CancellationToken ct = default);

    ComplexityScore AssessComplexity(string text);
}

public enum GradeLevel
{
    K2,      // Kindergarten - 2nd grade (ages 5-8)
    G3_5,    // 3rd - 5th grade (ages 8-11)
    G6_8,    // 6th - 8th grade (ages 11-14)
    G9_10,   // 9th - 10th grade (ages 14-16)
    G11_12   // 11th - 12th grade (ages 16-18)
}
```

### 2.4 Assessment Components

#### 2.4.1 Assessment Generator

- [ ] Create `IAssessmentGenerator` interface
- [ ] Implement `AssessmentGenerator` using LLM
- [ ] Create `Assessment` record with questions
- [ ] Implement question type factories (multiple choice, short answer, etc.)
- [ ] Add difficulty calibration

**Files:**
```
Assessment/
├── IAssessmentGenerator.cs
├── AssessmentGenerator.cs
├── Assessment.cs
├── AssessmentQuestion.cs
├── QuestionType.cs
└── AssessmentGeneratorOptions.cs
```

#### 2.4.2 Response Evaluator

- [ ] Create `IResponseEvaluator` interface
- [ ] Implement `ResponseEvaluator` for grading responses
- [ ] Create `EvaluationResult` with feedback
- [ ] Implement partial credit scoring
- [ ] Add misconception detection

**Files:**
```
Assessment/
├── IResponseEvaluator.cs
├── ResponseEvaluator.cs
├── EvaluationResult.cs
├── PartialCreditRule.cs
└── MisconceptionDetector.cs
```

#### 2.4.3 Assessment Chain

- [ ] Create `AssessmentChain` using `IGraphBuilder`
- [ ] Implement adaptive difficulty adjustment
- [ ] Create assessment state management
- [ ] Add early termination rules (mastery reached / struggling)

**Files:**
```
Assessment/
├── AssessmentChain.cs
├── AssessmentState.cs
└── AdaptiveDifficultyEngine.cs
```

### 2.5 Memory Components

#### 2.5.1 Student Profile Memory

- [ ] Create `IStudentProfileMemory` interface extending `IConversationMemory`
- [ ] Implement `StudentProfileMemory` with learning preferences
- [ ] Create `StudentProfile` record
- [ ] Add preference learning from interactions

**Files:**
```
Memory/
├── IStudentProfileMemory.cs
├── StudentProfileMemory.cs
├── StudentProfile.cs
└── LearningPreferences.cs
```

#### 2.5.2 Mastery State Memory

- [ ] Create `IMasteryStateMemory` interface
- [ ] Implement `MasteryStateMemory` for tracking concept mastery
- [ ] Create persistence adapter for database storage
- [ ] Add mastery history tracking

**Files:**
```
Memory/
├── IMasteryStateMemory.cs
├── MasteryStateMemory.cs
├── MasterySnapshot.cs
└── MasteryHistory.cs
```

#### 2.5.3 Learning Session Memory

- [ ] Create `ILearningSessionMemory` interface
- [ ] Implement `LearningSessionMemory` for session continuity
- [ ] Create session state serialization
- [ ] Add session resume capabilities

**Files:**
```
Memory/
├── ILearningSessionMemory.cs
├── LearningSessionMemory.cs
├── SessionState.cs
└── SessionResumeOptions.cs
```

### 2.6 Retrieval Components

#### 2.6.1 Curriculum-Aware Retriever

- [ ] Create `ICurriculumAwareRetriever` interface
- [ ] Implement `CurriculumAwareRetriever` with grade filtering
- [ ] Add prerequisite-aware retrieval
- [ ] Implement curriculum metadata filtering

**Files:**
```
Retrieval/
├── ICurriculumAwareRetriever.cs
├── CurriculumAwareRetriever.cs
├── CurriculumMetadata.cs
└── GradeLevelFilter.cs
```

**Interface design:**
```csharp
public interface ICurriculumAwareRetriever
{
    Task<IReadOnlyList<CurriculumDocument>> RetrieveAsync(
        string query,
        GradeLevel gradeLevel,
        SubjectArea subject,
        RetrievalOptions options,
        CancellationToken ct = default);

    Task<IReadOnlyList<CurriculumDocument>> RetrieveWithPrerequisitesAsync(
        ConceptId targetConcept,
        IReadOnlyDictionary<ConceptId, MasteryLevel> studentMastery,
        CancellationToken ct = default);
}
```

#### 2.6.2 Prerequisite Checker

- [ ] Create `IPrerequisiteChecker` interface
- [ ] Implement `PrerequisiteChecker` for concept dependencies
- [ ] Create concept graph traversal
- [ ] Add gap identification

**Files:**
```
Retrieval/
├── IPrerequisiteChecker.cs
├── PrerequisiteChecker.cs
├── ConceptGraph.cs
└── PrerequisiteGap.cs
```

---

## Phase 3: Provider Implementations

### 3.1 pgvector Provider

- [ ] Create `DotLangChain.VectorStores.Pgvector` project
- [ ] Implement `PgvectorVectorStore` : `IVectorStore`
- [ ] Add connection pooling with Npgsql
- [ ] Implement HNSW index support
- [ ] Add batch upsert optimization

**Files:**
```
src/DotLangChain.VectorStores.Pgvector/
├── DotLangChain.VectorStores.Pgvector.csproj
├── PgvectorVectorStore.cs
├── PgvectorOptions.cs
└── ServiceCollectionExtensions.cs
```

### 3.2 vLLM Provider

- [ ] Create `DotLangChain.LLM.Vllm` project
- [ ] Implement `VllmChatCompletionService` : `IChatCompletionService`
- [ ] Add OpenAI-compatible API support
- [ ] Implement streaming responses
- [ ] Add health check support

**Files:**
```
src/DotLangChain.LLM.Vllm/
├── DotLangChain.LLM.Vllm.csproj
├── VllmChatCompletionService.cs
├── VllmOptions.cs
└── ServiceCollectionExtensions.cs
```

### 3.3 Ollama Provider

- [ ] Create `DotLangChain.LLM.Ollama` project
- [ ] Implement `OllamaChatCompletionService` : `IChatCompletionService`
- [ ] Implement `OllamaEmbeddingService` : `IEmbeddingService`
- [ ] Add model management support
- [ ] Implement streaming responses

**Files:**
```
src/DotLangChain.LLM.Ollama/
├── DotLangChain.LLM.Ollama.csproj
├── OllamaChatCompletionService.cs
├── OllamaEmbeddingService.cs
├── OllamaOptions.cs
└── ServiceCollectionExtensions.cs
```

### 3.4 Anthropic Claude Provider

- [ ] Create `DotLangChain.LLM.Anthropic` project
- [ ] Implement `AnthropicChatCompletionService` : `IChatCompletionService`
- [ ] Add tool use support
- [ ] Implement streaming responses
- [ ] Add rate limiting and retry logic

**Files:**
```
src/DotLangChain.LLM.Anthropic/
├── DotLangChain.LLM.Anthropic.csproj
├── AnthropicChatCompletionService.cs
├── AnthropicOptions.cs
└── ServiceCollectionExtensions.cs
```

---

## Phase 4: Pre-built Educational Graphs

### 4.1 Socratic Tutor Graph

- [ ] Create `SocraticTutorGraph` using `IGraphBuilder`
- [ ] Implement nodes: assess, question, evaluate, hint, celebrate
- [ ] Add conditional edges for mastery routing
- [ ] Create state management for multi-turn conversations

### 4.2 Assessment Graph

- [ ] Create `AdaptiveAssessmentGraph`
- [ ] Implement adaptive difficulty adjustment
- [ ] Add early termination conditions
- [ ] Create comprehensive result reporting

### 4.3 Lesson Graph

- [ ] Create `LessonDeliveryGraph`
- [ ] Implement concept introduction flow
- [ ] Add practice problem integration
- [ ] Create mastery check gates

---

## Phase 5: Testing

### 5.1 Unit Tests

- [ ] Tests for SM2Scheduler
- [ ] Tests for MasteryCalculator
- [ ] Tests for ChildSafetyFilter
- [ ] Tests for AgeAdaptiveMiddleware
- [ ] Tests for all graph executions

### 5.2 Integration Tests

- [ ] pgvector integration tests with Testcontainers
- [ ] vLLM integration tests
- [ ] Ollama integration tests
- [ ] End-to-end educational workflow tests

### 5.3 Educational Quality Tests

- [ ] Socratic question quality evaluation
- [ ] Age-appropriateness validation
- [ ] Content safety validation
- [ ] Pedagogical effectiveness metrics

---

## Phase 6: Documentation

- [ ] Update main README.md with Education package
- [ ] Create `docs/EDUCATION_GUIDE.md`
- [ ] Add API documentation for all new interfaces
- [ ] Create example applications
- [ ] Add migration guide from Semantic Kernel

---

## Dependencies to Add

```xml
<!-- DotLangChain.Education.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.AI" Version="9.0.0" />
  <PackageReference Include="Microsoft.Extensions.AI.Abstractions" Version="9.0.0" />
</ItemGroup>

<!-- DotLangChain.VectorStores.Pgvector.csproj -->
<ItemGroup>
  <PackageReference Include="Npgsql" Version="9.0.0" />
  <PackageReference Include="Pgvector" Version="0.3.0" />
</ItemGroup>

<!-- DotLangChain.LLM.Vllm.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.Http" Version="9.0.0" />
</ItemGroup>

<!-- DotLangChain.LLM.Ollama.csproj -->
<ItemGroup>
  <PackageReference Include="OllamaSharp" Version="4.0.0" />
</ItemGroup>

<!-- DotLangChain.LLM.Anthropic.csproj -->
<ItemGroup>
  <PackageReference Include="Anthropic.SDK" Version="3.0.0" />
</ItemGroup>
```

---

## Estimated Effort

| Phase | Estimated Effort |
|-------|------------------|
| Phase 1: Framework Updates | Small |
| Phase 2: Education Package | Large |
| Phase 3: Provider Implementations | Medium |
| Phase 4: Pre-built Graphs | Medium |
| Phase 5: Testing | Medium |
| Phase 6: Documentation | Small |

---

## Notes

- All interfaces should follow the existing DotLangChain patterns
- Use `CancellationToken` on all async methods
- Follow the existing exception hierarchy
- Add OpenTelemetry traces for all operations
- Ensure thread-safety for all stateful components
