# DotNetAgents Educational Extensions - Implementation Plan

## Overview

This document outlines the implementation plan for extending DotNetAgents to support the **Science Companion AI** educational platform. These extensions will be implemented as a new package: `DotNetAgents.Education`.

**Related Project:** `/mnt/workspace/Obsidian_Shared/Research/Science/AI_Companion_Project/`

**Note:** This plan extends the existing DotNetAgents library (not DotLangChain). DotNetAgents is the production-ready .NET 10 library for building AI agents, chains, and workflows.

---

## Phase 1: Core Framework Updates

### 1.1 Upgrade to .NET 10

- [ ] Update `Directory.Build.props` to target `net10.0`
- [ ] Update all project files to `net10.0`
- [ ] Update NuGet dependencies to .NET 10 compatible versions
- [ ] Test compilation and fix any breaking changes
- [ ] Update CI/CD pipeline for .NET 10 SDK

**Files to modify:**
- `Directory.Build.props` (already targeting .NET 10 ✅)
- All project files (already targeting .NET 10 ✅)
- Update CI/CD pipeline for .NET 10 SDK (if needed)

**Status:** ✅ DotNetAgents is already on .NET 10 - this phase is complete!

### 1.2 Microsoft.Extensions.AI Compatibility

- [ ] Add `Microsoft.Extensions.AI` package reference
- [ ] Implement `IChatClient` adapter for `IChatCompletionService`
- [ ] Implement `IEmbeddingGenerator<string, Embedding<float>>` adapter
- [ ] Create extension methods for DI registration with MEAI
- [ ] Add middleware support for MEAI pipeline

**New files:**
```
src/DotNetAgents.Extensions.AI/
├── DotNetAgents.Extensions.AI.csproj
├── ChatClientAdapter.cs
├── EmbeddingGeneratorAdapter.cs
└── ServiceCollectionExtensions.cs
```

**Note:** DotNetAgents already has Microsoft Agent Framework compatibility layer (`DotNetAgents.AgentFramework`). This extension would add Microsoft.Extensions.AI compatibility.

---

## Phase 2: DotNetAgents.Education Package

### 2.1 Project Setup

- [ ] Create new project `DotNetAgents.Education`
- [ ] Add project references to `DotNetAgents.Core` and `DotNetAgents.Workflow`
- [ ] Configure package metadata for NuGet
- [ ] Set up folder structure

**Structure:**
```
src/DotNetAgents.Education/
├── DotNetAgents.Education.csproj
├── Pedagogy/
├── Safety/
├── Assessment/
├── Memory/
└── Retrieval/
```

**Dependencies:**
- `DotNetAgents.Core` - For core interfaces and abstractions
- `DotNetAgents.Workflow` - For graph-based tutoring workflows
- `DotNetAgents.Configuration` - For configuration management

### 2.2 Pedagogy Components

#### 2.2.1 Socratic Dialogue Engine

- [ ] Create `ISocraticDialogueEngine` interface
- [ ] Implement `SocraticDialogueEngine` with question scaffolding
- [ ] Create `SocraticQuestionType` enum (Clarifying, Probing, Assumption, Implication, Viewpoint)
- [ ] Implement `SocraticDialogueState` for graph-based tutoring
- [ ] Create pre-built `SocraticTutorGraph` using `StateGraph<TState>` (DotNetAgents workflow engine)

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
- [ ] Integrate with DotNetAgents `ISanitizer` for content filtering
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

- [ ] Create `IStudentProfileMemory` interface extending DotNetAgents `IMemory`
- [ ] Implement `StudentProfileMemory` with learning preferences
- [ ] Create `StudentProfile` record
- [ ] Add preference learning from interactions
- [ ] Integrate with DotNetAgents `IMemoryStore` for persistence

**Files:**
```
Memory/
├── IStudentProfileMemory.cs
├── StudentProfileMemory.cs
├── StudentProfile.cs
└── LearningPreferences.cs
```

**Integration:** Use DotNetAgents `IMemory` and `IMemoryStore` interfaces from `DotNetAgents.Core.Memory`

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

- [ ] Create `ICurriculumAwareRetriever` interface extending DotNetAgents `IVectorStore`
- [ ] Implement `CurriculumAwareRetriever` with grade filtering
- [ ] Add prerequisite-aware retrieval
- [ ] Implement curriculum metadata filtering
- [ ] Integrate with DotNetAgents `RetrievalChain` for RAG workflows

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
public interface ICurriculumAwareRetriever : IVectorStore
{
    Task<IReadOnlyList<Document>> RetrieveAsync(
        string query,
        GradeLevel gradeLevel,
        SubjectArea subject,
        VectorSearchOptions options,
        CancellationToken ct = default);

    Task<IReadOnlyList<Document>> RetrieveWithPrerequisitesAsync(
        ConceptId targetConcept,
        IReadOnlyDictionary<ConceptId, MasteryLevel> studentMastery,
        CancellationToken ct = default);
}
```

**Integration:** Extends DotNetAgents `IVectorStore` from `DotNetAgents.Core.Retrieval`

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

- [ ] Create `DotNetAgents.VectorStores.Pgvector` project
- [ ] Implement `PgvectorVectorStore` : `IVectorStore` (from DotNetAgents.Core)
- [ ] Add connection pooling with Npgsql
- [ ] Implement HNSW index support
- [ ] Add batch upsert optimization
- [ ] Follow DotNetAgents vector store patterns (similar to PineconeVectorStore)

**Files:**
```
src/DotNetAgents.VectorStores.Pgvector/
├── DotNetAgents.VectorStores.Pgvector.csproj
├── PgvectorVectorStore.cs
├── PgvectorOptions.cs
└── ServiceCollectionExtensions.cs
```

**Reference:** See `DotNetAgents.VectorStores.Pinecone` for implementation patterns

### 3.2 vLLM Provider

- [ ] ✅ **Already Implemented!** See `DotNetAgents.Providers.vLLM`
- [ ] Review existing implementation for any enhancements needed
- [ ] Ensure streaming support is complete
- [ ] Add health check support if missing

**Status:** ✅ vLLM provider already exists in DotNetAgents

### 3.3 Ollama Provider

- [ ] ✅ **Already Implemented!** See `DotNetAgents.Providers.Ollama`
- [ ] Review existing implementation for embedding support
- [ ] Add embedding service if missing
- [ ] Ensure streaming support is complete

**Status:** ✅ Ollama provider already exists in DotNetAgents

### 3.4 Anthropic Claude Provider

- [ ] ✅ **Already Implemented!** See `DotNetAgents.Providers.Anthropic`
- [ ] Review existing implementation for tool use support
- [ ] Ensure streaming support is complete
- [ ] Verify rate limiting and retry logic (DotNetAgents has `RetryPolicy` and `CircuitBreaker`)

**Status:** ✅ Anthropic provider already exists in DotNetAgents

---

## Phase 4: Pre-built Educational Graphs

### 4.1 Socratic Tutor Graph

- [ ] Create `SocraticTutorGraph` using DotNetAgents `StateGraph<TState>`
- [ ] Implement nodes: assess, question, evaluate, hint, celebrate
- [ ] Add conditional edges for mastery routing
- [ ] Create state management for multi-turn conversations
- [ ] Use DotNetAgents checkpointing for session persistence

**Reference:** See `DotNetAgents.Workflow` for graph building patterns

### 4.2 Assessment Graph

- [ ] Create `AdaptiveAssessmentGraph` using DotNetAgents `StateGraph<TState>`
- [ ] Implement adaptive difficulty adjustment
- [ ] Add early termination conditions
- [ ] Create comprehensive result reporting
- [ ] Integrate with DotNetAgents checkpointing for resume capability

### 4.3 Lesson Graph

- [ ] Create `LessonDeliveryGraph` using DotNetAgents `StateGraph<TState>`
- [ ] Implement concept introduction flow
- [ ] Add practice problem integration
- [ ] Create mastery check gates
- [ ] Use DotNetAgents workflow engine for stateful execution

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
<!-- DotNetAgents.Education.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.AI" Version="9.0.0" />
  <PackageReference Include="Microsoft.Extensions.AI.Abstractions" Version="9.0.0" />
</ItemGroup>
<ItemGroup>
  <ProjectReference Include="..\DotNetAgents.Core\DotNetAgents.Core.csproj" />
  <ProjectReference Include="..\DotNetAgents.Workflow\DotNetAgents.Workflow.csproj" />
  <ProjectReference Include="..\DotNetAgents.Configuration\DotNetAgents.Configuration.csproj" />
</ItemGroup>

<!-- DotNetAgents.VectorStores.Pgvector.csproj -->
<ItemGroup>
  <PackageReference Include="Npgsql" Version="9.0.0" />
  <PackageReference Include="Pgvector" Version="0.3.0" />
</ItemGroup>
<ItemGroup>
  <ProjectReference Include="..\DotNetAgents.Core\DotNetAgents.Core.csproj" />
</ItemGroup>

<!-- Note: vLLM, Ollama, and Anthropic providers already exist in DotNetAgents -->
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
