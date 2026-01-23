# DotNetAgents.Education - Technical Specification

**Version:** 1.0.0  
**Date:** January 2025  
**Status:** Ready for Review  
**Target Framework:** .NET 10 (LTS)

---

## 1. Introduction

### 1.1 Purpose

This document provides the technical specification for the DotNetAgents.Education package, detailing the architecture, design patterns, data structures, algorithms, and integration points. This specification serves as the technical blueprint for implementation.

### 1.2 Scope

This specification covers:
- Architecture and design patterns
- Data structures and models
- Algorithms and implementations
- Integration with DotNetAgents Core
- Security and compliance mechanisms
- Performance optimizations
- Testing strategies

### 1.3 Target Audience

- Software engineers implementing the package
- Architects reviewing the design
- QA engineers writing tests
- DevOps engineers deploying the system

---

## 2. Architecture Overview

### 2.1 Layered Architecture

```
┌─────────────────────────────────────────────────────────┐
│         Application Layer (User Code)                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │ Educational  │  │ Assessment   │  │ Analytics    │ │
│  │ Workflows    │  │ Workflows    │  │ Dashboards   │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│         Education Domain Layer                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │ Pedagogy     │  │ Safety       │  │ Assessment   │ │
│  │ Components   │  │ Components   │  │ Components   │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │ Memory       │  │ Retrieval    │  │ Compliance   │ │
│  │ Components   │  │ Components   │  │ Components   │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│         DotNetAgents Core Layer                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │ ILLMModel    │  │ IVectorStore │  │ IMemory      │ │
│  │ StateGraph   │  │ ITool        │  │ ISanitizer   │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│         Infrastructure Layer                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │ Database     │  │ Cache        │  │ Observability │ │
│  │ (SQL/Postgres)│  │ (Redis/Mem)  │  │ (OTel/Logs)  │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### 2.2 Package Structure

```
src/DotNetAgents.Education/
├── DotNetAgents.Education.csproj
├── Pedagogy/
│   ├── ISocraticDialogueEngine.cs
│   ├── SocraticDialogueEngine.cs
│   ├── ISpacedRepetitionScheduler.cs
│   ├── SM2Scheduler.cs
│   ├── IMasteryCalculator.cs
│   ├── MasteryCalculator.cs
│   └── Models/
│       ├── SocraticQuestion.cs
│       ├── StudentUnderstanding.cs
│       └── MasteryLevel.cs
├── Safety/
│   ├── IContentFilter.cs
│   ├── ChildSafetyFilter.cs
│   ├── IConversationMonitor.cs
│   ├── ConversationMonitor.cs
│   ├── IAgeAdaptiveTransformer.cs
│   ├── AgeAdaptiveTransformer.cs
│   └── Models/
│       ├── ContentFilterResult.cs
│       ├── MonitoringAlert.cs
│       └── GradeLevel.cs
├── Assessment/
│   ├── IAssessmentGenerator.cs
│   ├── AssessmentGenerator.cs
│   ├── IResponseEvaluator.cs
│   ├── ResponseEvaluator.cs
│   └── Models/
│       ├── Assessment.cs
│       ├── AssessmentQuestion.cs
│       └── EvaluationResult.cs
├── Memory/
│   ├── IStudentProfileMemory.cs
│   ├── StudentProfileMemory.cs
│   ├── IMasteryStateMemory.cs
│   ├── MasteryStateMemory.cs
│   ├── ILearningSessionMemory.cs
│   ├── LearningSessionMemory.cs
│   └── Models/
│       ├── StudentProfile.cs
│       ├── MasterySnapshot.cs
│       └── SessionState.cs
├── Retrieval/
│   ├── ICurriculumAwareRetriever.cs
│   ├── CurriculumAwareRetriever.cs
│   ├── IPrerequisiteChecker.cs
│   ├── PrerequisiteChecker.cs
│   └── Models/
│       ├── ConceptId.cs
│       ├── ConceptGraph.cs
│       └── PrerequisiteGap.cs
├── Integration/
│   ├── ILmsConnector.cs
│   ├── CanvasConnector.cs
│   ├── MoodleConnector.cs
│   ├── ISisConnector.cs
│   ├── PowerSchoolConnector.cs
│   └── InfiniteCampusConnector.cs
├── Compliance/
│   ├── IFerpaComplianceService.cs
│   ├── FerpaComplianceService.cs
│   ├── IGdprComplianceService.cs
│   ├── GdprComplianceService.cs
│   ├── IEducationAuthorizationService.cs
│   ├── EducationAuthorizationService.cs
│   └── Models/
│       ├── AccessLog.cs
│       ├── ConsentRecord.cs
│       └── Permission.cs
├── Infrastructure/
│   ├── ITenantContext.cs
│   ├── TenantContext.cs
│   ├── IEducationEventPublisher.cs
│   ├── EducationEventPublisher.cs
│   ├── ILearningAnalytics.cs
│   ├── LearningAnalytics.cs
│   └── Models/
│       ├── EducationEvent.cs
│       ├── StudentProgressReport.cs
│       └── ClassInsights.cs
├── Workflows/
│   ├── SocraticTutorGraph.cs
│   ├── AdaptiveAssessmentGraph.cs
│   ├── LessonDeliveryGraph.cs
│   └── States/
│       ├── SocraticDialogueState.cs
│       ├── AssessmentState.cs
│       └── LessonState.cs
└── Extensions/
    ├── ServiceCollectionExtensions.cs
    └── ConfigurationExtensions.cs
```

---

## 3. Core Components

### 3.1 Pedagogy Components

#### 3.1.1 Socratic Dialogue Engine

**Architecture:**
- Uses DotNetAgents `ILLMModel<TInput, TOutput>` for question generation
- Implements scaffolding algorithm for progressive hints
- Maintains conversation context via DotNetAgents `IMemory`

**Algorithm:**
```csharp
public class SocraticDialogueEngine : ISocraticDialogueEngine
{
    private readonly ILLMModel<ChatMessage[], ChatMessage> _llmModel;
    private readonly IMemory _memory;
    private readonly ILogger<SocraticDialogueEngine> _logger;

    public async Task<SocraticQuestion> GenerateQuestionAsync(
        ConceptContext concept,
        StudentUnderstanding currentLevel,
        string? language = null,
        CancellationToken ct = default)
    {
        // 1. Build prompt with concept and student level
        var prompt = BuildSocraticPrompt(concept, currentLevel, language);
        
        // 2. Get conversation history from memory
        var history = await _memory.GetMessagesAsync(10, ct);
        
        // 3. Generate question using LLM
        var messages = BuildChatMessages(prompt, history);
        var response = await _llmModel.GenerateAsync(messages, null, ct);
        
        // 4. Parse and return question
        return ParseQuestion(response.Content);
    }
}
```

**Question Types:**
```csharp
public enum SocraticQuestionType
{
    Clarifying,    // "What do you mean by...?"
    Probing,       // "Can you explain why...?"
    Assumption,    // "What assumptions are you making?"
    Implication,   // "What would happen if...?"
    Viewpoint      // "How might someone else see this?"
}
```

**Scaffolding Algorithm:**
- Level 1: General direction hint
- Level 2: More specific guidance
- Level 3: Partial information
- Level 4: Most of the answer
- Level 5: Complete answer (last resort)

#### 3.1.2 Spaced Repetition Scheduler (SM2)

**Algorithm:**
SuperMemo 2 algorithm implementation:

```csharp
public class SM2Scheduler : ISpacedRepetitionScheduler
{
    public ReviewSchedule CalculateNextReview(
        ReviewItem item,
        PerformanceRating rating)
    {
        // SM2 Algorithm
        var easeFactor = item.EaseFactor;
        var interval = item.Interval;
        
        if (rating >= 3) // Correct response
        {
            if (interval == 0)
                interval = 1;
            else if (interval == 1)
                interval = 6;
            else
                interval = (int)(interval * easeFactor);
            
            easeFactor = easeFactor + (0.1f - (5 - rating) * (0.08f + (5 - rating) * 0.02f));
            easeFactor = Math.Max(1.3f, easeFactor);
        }
        else // Incorrect response
        {
            interval = 1;
            easeFactor = Math.Max(1.3f, easeFactor - 0.2f);
        }
        
        var nextReview = DateTimeOffset.UtcNow.AddDays(interval);
        
        return new ReviewSchedule
        {
            NextReviewDate = nextReview,
            Interval = interval,
            EaseFactor = easeFactor
        };
    }
}
```

**Performance Rating Scale:**
- 0: Complete blackout (no recall)
- 1: Incorrect but remembered
- 2: Incorrect but easy to recall
- 3: Correct with difficulty
- 4: Correct with hesitation
- 5: Perfect recall

#### 3.1.3 Mastery Calculator

**Algorithm:**
Weighted scoring with recency bias:

```csharp
public class MasteryCalculator : IMasteryCalculator
{
    public MasteryLevel CalculateMastery(
        ConceptId concept,
        IReadOnlyList<AssessmentResult> history)
    {
        if (history.Count == 0)
            return MasteryLevel.Novice;
        
        // Weight recent assessments more heavily
        var weights = CalculateWeights(history.Count);
        var weightedScore = 0.0;
        var totalWeight = 0.0;
        
        for (int i = 0; i < history.Count; i++)
        {
            var weight = weights[i];
            var score = history[i].Score;
            weightedScore += score * weight;
            totalWeight += weight;
        }
        
        var averageScore = weightedScore / totalWeight;
        
        // Apply mastery decay for older assessments
        var decayFactor = CalculateDecayFactor(history);
        var finalScore = averageScore * decayFactor;
        
        return ScoreToMasteryLevel(finalScore);
    }
    
    private double[] CalculateWeights(int count)
    {
        // Exponential decay: recent = higher weight
        var weights = new double[count];
        var baseWeight = 1.0;
        
        for (int i = count - 1; i >= 0; i--)
        {
            weights[i] = baseWeight;
            baseWeight *= 0.9; // 10% decay per older assessment
        }
        
        return weights;
    }
}
```

**Mastery Levels:**
- **Novice** (0-40%): No understanding
- **Developing** (40-60%): Partial understanding
- **Proficient** (60-80%): Good understanding
- **Advanced** (80-95%): Strong understanding
- **Mastery** (95-100%): Complete understanding

### 3.2 Safety Components

#### 3.2.1 Child Safety Filter

**Architecture:**
- Multi-layer filtering (pattern matching + LLM-based)
- Integration with DotNetAgents `ISanitizer`
- Configurable severity levels

**Filtering Pipeline:**
```csharp
public class ChildSafetyFilter : IContentFilter
{
    private readonly ISanitizer _sanitizer;
    private readonly ILLMModel<ChatMessage[], ChatMessage> _llmModel;
    private readonly IReadOnlyList<BlockedPattern> _blockedPatterns;
    
    public async Task<ContentFilterResult> FilterInputAsync(
        string input,
        FilterContext context,
        CancellationToken ct = default)
    {
        // Layer 1: Pattern matching (fast)
        var patternResult = CheckPatterns(input);
        if (patternResult.IsBlocked)
            return patternResult;
        
        // Layer 2: LLM-based analysis (thorough)
        var llmResult = await AnalyzeWithLLM(input, ct);
        if (llmResult.IsBlocked)
            return llmResult;
        
        // Layer 3: Sanitizer (DotNetAgents)
        var sanitized = await _sanitizer.SanitizeAsync(input, ct);
        
        return new ContentFilterResult
        {
            IsAllowed = true,
            FilteredContent = sanitized
        };
    }
}
```

**Blocked Categories:**
- Violence
- Adult content
- Hate speech
- Self-harm references
- Personal information requests
- Bullying

#### 3.2.2 Age-Adaptive Transformer

**Vocabulary Mapping:**
```csharp
public class AgeAdaptiveTransformer : IAgeAdaptiveTransformer
{
    private readonly Dictionary<GradeLevel, VocabularyLevel> _vocabularyMap;
    
    public Task<string> TransformResponseAsync(
        string response,
        GradeLevel gradeLevel,
        CancellationToken ct = default)
    {
        // 1. Assess complexity
        var complexity = AssessComplexity(response);
        
        // 2. Simplify if needed
        if (complexity.ReadingLevel > GetMaxReadingLevel(gradeLevel))
        {
            response = SimplifyVocabulary(response, gradeLevel);
        }
        
        // 3. Adjust length
        var maxLength = GetMaxLength(gradeLevel);
        if (response.Length > maxLength)
        {
            response = Summarize(response, maxLength);
        }
        
        return Task.FromResult(response);
    }
    
    private ComplexityScore AssessComplexity(string text)
    {
        // Flesch-Kincaid Grade Level
        var sentences = text.Split('.', '!', '?').Length;
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var syllables = CountSyllables(text);
        
        var gradeLevel = 0.39 * (words / sentences) + 11.8 * (syllables / words) - 15.59;
        
        return new ComplexityScore
        {
            ReadingLevel = (int)Math.Round(gradeLevel),
            FleschKincaid = gradeLevel
        };
    }
}
```

**Grade Level Mappings:**
- **K2**: Ages 5-8, Reading Level 1-2
- **G3_5**: Ages 8-11, Reading Level 3-5
- **G6_8**: Ages 11-14, Reading Level 6-8
- **G9_10**: Ages 14-16, Reading Level 9-10
- **G11_12**: Ages 16-18, Reading Level 11-12

### 3.3 Memory Components

#### 3.3.1 Student Profile Memory

**Architecture:**
- Extends DotNetAgents `IMemory` and `IMemoryStore`
- Stores in SQL Server/PostgreSQL with tenant isolation
- Caches frequently accessed profiles

**Data Model:**
```csharp
public record StudentProfile
{
    public string StudentId { get; init; }
    public string TenantId { get; init; }
    public GradeLevel GradeLevel { get; init; }
    public LearningPreferences Preferences { get; init; }
    public IReadOnlyDictionary<string, object> Metadata { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}

public record LearningPreferences
{
    public LearningStyle Style { get; init; } // Visual, Auditory, Kinesthetic
    public int PreferredResponseLength { get; init; }
    public bool PrefersExamples { get; init; }
    public bool PrefersVisuals { get; init; }
}
```

**Storage Strategy:**
- Primary: SQL Server/PostgreSQL (persistent)
- Cache: In-memory (Redis optional for distributed)
- TTL: 1 hour for cached profiles

#### 3.3.2 Mastery State Memory

**Data Model:**
```csharp
public record MasterySnapshot
{
    public string StudentId { get; init; }
    public ConceptId Concept { get; init; }
    public MasteryLevel Level { get; init; }
    public double Score { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public IReadOnlyList<AssessmentResult> AssessmentHistory { get; init; }
}

public record MasteryHistory
{
    public string StudentId { get; init; }
    public ConceptId Concept { get; init; }
    public IReadOnlyList<MasterySnapshot> Snapshots { get; init; }
    public MasteryTrend Trend { get; init; } // Improving, Stable, Declining
}
```

**Database Schema:**
```sql
CREATE TABLE MasterySnapshots (
    Id BIGINT PRIMARY KEY IDENTITY,
    StudentId NVARCHAR(100) NOT NULL,
    TenantId NVARCHAR(100) NOT NULL,
    ConceptId NVARCHAR(100) NOT NULL,
    MasteryLevel INT NOT NULL,
    Score DECIMAL(5,2) NOT NULL,
    Timestamp DATETIMEOFFSET NOT NULL,
    AssessmentHistoryJson NVARCHAR(MAX),
    INDEX IX_MasterySnapshots_Student_Concept (StudentId, ConceptId),
    INDEX IX_MasterySnapshots_Tenant (TenantId)
);
```

### 3.4 Retrieval Components

#### 3.4.1 Curriculum-Aware Retriever

**Architecture:**
- Extends DotNetAgents `IVectorStore`
- Adds grade-level and subject filtering
- Integrates with Pinecone or pgvector

**Implementation:**
```csharp
public class CurriculumAwareRetriever : IVectorStore
{
    private readonly IVectorStore _baseVectorStore;
    private readonly IPrerequisiteChecker _prerequisiteChecker;
    
    public async Task<IReadOnlyList<Document>> RetrieveAsync(
        string query,
        GradeLevel gradeLevel,
        SubjectArea subject,
        VectorSearchOptions options,
        CancellationToken ct = default)
    {
        // 1. Generate embedding for query
        var embedding = await _embeddingModel.GenerateAsync(query, ct);
        
        // 2. Build metadata filter
        var filter = new Dictionary<string, object>
        {
            ["gradeLevel"] = (int)gradeLevel,
            ["subject"] = subject.ToString()
        };
        
        // 3. Search vector store
        var results = await _baseVectorStore.SearchAsync(
            embedding,
            options.TopK,
            filter,
            ct);
        
        // 4. Convert to documents
        return results.Select(r => ConvertToDocument(r)).ToList();
    }
}
```

#### 3.4.2 Prerequisite Checker

**Concept Dependency Graph:**
```csharp
public class ConceptGraph
{
    private readonly Dictionary<ConceptId, ConceptNode> _nodes;
    private readonly Dictionary<ConceptId, List<ConceptId>> _edges; // Prerequisites
    
    public bool HasPrerequisites(
        ConceptId concept,
        IReadOnlyDictionary<ConceptId, MasteryLevel> studentMastery)
    {
        var prerequisites = GetPrerequisites(concept);
        
        foreach (var prereq in prerequisites)
        {
            if (!studentMastery.ContainsKey(prereq))
                return false;
            
            if (studentMastery[prereq] < MasteryLevel.Proficient)
                return false;
        }
        
        return true;
    }
    
    private IReadOnlyList<ConceptId> GetPrerequisites(ConceptId concept)
    {
        // DFS traversal to get all prerequisites
        var prerequisites = new HashSet<ConceptId>();
        var visited = new HashSet<ConceptId>();
        
        DFS(concept, prerequisites, visited);
        
        return prerequisites.ToList();
    }
}
```

### 3.5 Compliance Components

#### 3.5.1 FERPA Compliance Service

**Access Control:**
```csharp
public class FerpaComplianceService : IFerpaComplianceService
{
    public async Task<bool> CanAccessStudentDataAsync(
        string requesterId,
        string studentId,
        DataAccessPurpose purpose,
        CancellationToken ct = default)
    {
        // 1. Check requester role
        var requesterRole = await GetUserRoleAsync(requesterId, ct);
        
        // 2. Check relationship (teacher-student, parent-child)
        var hasRelationship = await CheckRelationshipAsync(
            requesterId, studentId, ct);
        
        // 3. Check purpose
        var allowedPurposes = GetAllowedPurposes(requesterRole);
        
        // 4. Log access attempt
        await LogAccessAsync(requesterId, studentId, purpose, ct);
        
        return hasRelationship && allowedPurposes.Contains(purpose);
    }
}
```

**Access Log Schema:**
```sql
CREATE TABLE AccessLogs (
    Id BIGINT PRIMARY KEY IDENTITY,
    RequesterId NVARCHAR(100) NOT NULL,
    StudentId NVARCHAR(100) NOT NULL,
    Purpose INT NOT NULL,
    Timestamp DATETIMEOFFSET NOT NULL,
    Allowed BIT NOT NULL,
    INDEX IX_AccessLogs_Student_Timestamp (StudentId, Timestamp),
    INDEX IX_AccessLogs_Requester_Timestamp (RequesterId, Timestamp)
);
```

#### 3.5.2 GDPR Compliance Service

**Data Export:**
```csharp
public class GdprComplianceService : IGdprComplianceService
{
    public async Task ExportStudentDataAsync(
        string studentId,
        ExportFormat format,
        CancellationToken ct = default)
    {
        // 1. Collect all student data
        var profile = await _profileMemory.GetProfileAsync(studentId, ct);
        var mastery = await _masteryMemory.GetAllMasteryAsync(studentId, ct);
        var sessions = await _sessionMemory.GetAllSessionsAsync(studentId, ct);
        var assessments = await _assessmentStore.GetAssessmentsAsync(studentId, ct);
        
        // 2. Serialize to requested format
        var export = new StudentDataExport
        {
            Profile = profile,
            Mastery = mastery,
            Sessions = sessions,
            Assessments = assessments
        };
        
        return format switch
        {
            ExportFormat.Json => JsonSerializer.Serialize(export),
            ExportFormat.Csv => ConvertToCsv(export),
            _ => throw new NotSupportedException()
        };
    }
    
    public async Task DeleteStudentDataAsync(
        string studentId,
        CancellationToken ct = default)
    {
        // 1. Anonymize all references
        await AnonymizeReferencesAsync(studentId, ct);
        
        // 2. Delete PII
        await DeletePIIAsync(studentId, ct);
        
        // 3. Delete assessment data (if allowed)
        await DeleteAssessmentsAsync(studentId, ct);
        
        // 4. Log deletion
        await LogDeletionAsync(studentId, ct);
    }
}
```

### 3.6 Infrastructure Components

#### 3.6.1 Multi-Tenancy

**Tenant Context:**
```csharp
public class TenantContext : ITenantContext
{
    public string TenantId { get; }
    public TenantConfiguration Configuration { get; }
    
    // Propagated via AsyncLocal or HttpContext
    private static readonly AsyncLocal<ITenantContext?> _current = new();
    
    public static ITenantContext? Current => _current.Value;
    
    public static void SetCurrent(ITenantContext? context)
    {
        _current.Value = context;
    }
}
```

**Database Isolation:**
- **Option 1**: Separate databases per tenant (highest isolation)
- **Option 2**: Shared database with tenant ID in all tables (recommended)
- **Option 3**: Row-level security (PostgreSQL)

**Recommended Approach:**
```sql
-- All tables include TenantId
CREATE TABLE StudentProfiles (
    Id BIGINT PRIMARY KEY IDENTITY,
    TenantId NVARCHAR(100) NOT NULL,
    StudentId NVARCHAR(100) NOT NULL,
    -- ... other columns
    INDEX IX_StudentProfiles_Tenant_Student (TenantId, StudentId)
);

-- Enforce tenant isolation in queries
SELECT * FROM StudentProfiles 
WHERE TenantId = @TenantId AND StudentId = @StudentId;
```

#### 3.6.2 Event System

**Event Types:**
```csharp
public interface IEducationEvent
{
    DateTimeOffset Timestamp { get; }
    string EventType { get; }
    string StudentId { get; }
    string TenantId { get; }
    IDictionary<string, object> Metadata { get; }
}

public record StudentResponseEvent(
    string StudentId,
    string QuestionId,
    string Response,
    bool IsCorrect
) : IEducationEvent;

public record AssessmentCompletedEvent(
    string StudentId,
    string AssessmentId,
    double Score,
    TimeSpan Duration
) : IEducationEvent;

public record MasteryAchievedEvent(
    string StudentId,
    ConceptId Concept,
    MasteryLevel Level
) : IEducationEvent;
```

**Event Publishing:**
```csharp
public class EducationEventPublisher : IEducationEventPublisher
{
    private readonly ILogger<EducationEventPublisher> _logger;
    private readonly IReadOnlyList<IEducationEventSubscriber> _subscribers;
    
    public async Task PublishAsync<T>(T educationEvent, CancellationToken ct = default)
        where T : IEducationEvent
    {
        // 1. Log event
        _logger.LogInformation(
            "Publishing education event: {EventType} for student {StudentId}",
            educationEvent.EventType,
            educationEvent.StudentId);
        
        // 2. Notify subscribers
        var tasks = _subscribers.Select(s => s.HandleAsync(educationEvent, ct));
        await Task.WhenAll(tasks);
        
        // 3. Store event history
        await _eventStore.SaveAsync(educationEvent, ct);
    }
}
```

---

## 4. Integration with DotNetAgents

### 4.1 Core Abstractions

**Memory Integration:**
```csharp
public class StudentProfileMemory : IStudentProfileMemory, IMemory
{
    private readonly IMemoryStore _baseMemoryStore;
    
    // Implement IMemory from DotNetAgents
    public Task AddMessageAsync(MemoryMessage message, CancellationToken ct = default)
    {
        return _baseMemoryStore.AddMessageAsync(message, ct);
    }
    
    // Education-specific methods
    public Task<StudentProfile> GetProfileAsync(string studentId, CancellationToken ct = default)
    {
        // Implementation
    }
}
```

**Vector Store Integration:**
```csharp
public class CurriculumAwareRetriever : ICurriculumAwareRetriever, IVectorStore
{
    private readonly IVectorStore _baseVectorStore;
    
    // Implement IVectorStore from DotNetAgents
    public Task<string> UpsertAsync(string id, float[] vector, IDictionary<string, object>? metadata, CancellationToken ct = default)
    {
        return _baseVectorStore.UpsertAsync(id, vector, metadata, ct);
    }
    
    // Education-specific methods
    public Task<IReadOnlyList<Document>> RetrieveAsync(
        string query,
        GradeLevel gradeLevel,
        SubjectArea subject,
        VectorSearchOptions options,
        CancellationToken ct = default)
    {
        // Implementation
    }
}
```

**Workflow Integration:**
```csharp
public class SocraticTutorGraph
{
    public StateGraph<SocraticDialogueState> BuildGraph()
    {
        var graph = new StateGraph<SocraticDialogueState>();
        
        // Add nodes
        graph.AddNode("assess", AssessUnderstandingAsync)
              .AddNode("question", GenerateQuestionAsync)
              .AddNode("evaluate", EvaluateResponseAsync)
              .AddNode("hint", ProvideHintAsync)
              .AddNode("celebrate", CelebrateMasteryAsync);
        
        // Add edges
        graph.AddEdge("assess", "question")
              .AddConditionalEdge("question", RouteByResponseAsync)
              .AddEdge("evaluate", "hint", state => state.NeedsHint)
              .AddEdge("evaluate", "celebrate", state => state.HasMastery)
              .AddEdge("hint", "question");
        
        graph.SetEntryPoint("assess");
        graph.AddExitPoint("celebrate");
        
        return graph;
    }
}
```

### 4.2 Dependency Injection

**Service Registration:**
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDotNetAgentsEducation(
        this IServiceCollection services,
        Action<EducationOptions>? configure = null)
    {
        // Register core components
        services.AddSingleton<ISocraticDialogueEngine, SocraticDialogueEngine>();
        services.AddSingleton<ISpacedRepetitionScheduler, SM2Scheduler>();
        services.AddSingleton<IMasteryCalculator, MasteryCalculator>();
        
        // Register safety components
        services.AddSingleton<IContentFilter, ChildSafetyFilter>();
        services.AddSingleton<IConversationMonitor, ConversationMonitor>();
        services.AddSingleton<IAgeAdaptiveTransformer, AgeAdaptiveTransformer>();
        
        // Register memory components
        services.AddScoped<IStudentProfileMemory, StudentProfileMemory>();
        services.AddScoped<IMasteryStateMemory, MasteryStateMemory>();
        services.AddScoped<ILearningSessionMemory, LearningSessionMemory>();
        
        // Register compliance components
        services.AddSingleton<IFerpaComplianceService, FerpaComplianceService>();
        services.AddSingleton<IGdprComplianceService, GdprComplianceService>();
        services.AddSingleton<IEducationAuthorizationService, EducationAuthorizationService>();
        
        // Register infrastructure
        services.AddSingleton<ITenantContextProvider, TenantContextProvider>();
        services.AddSingleton<IEducationEventPublisher, EducationEventPublisher>();
        services.AddSingleton<ILearningAnalytics, LearningAnalytics>();
        
        return services;
    }
}
```

---

## 5. Security & Compliance

### 5.1 Data Encryption

**At Rest:**
- SQL Server: Transparent Data Encryption (TDE)
- PostgreSQL: pgcrypto extension
- Application-level encryption for sensitive fields

**In Transit:**
- TLS 1.3 for all HTTP connections
- Database connections encrypted
- API keys stored in Azure Key Vault / AWS Secrets Manager

### 5.2 Access Control

**Role-Based Access Control (RBAC):**
```csharp
public enum EducationRole
{
    Student,        // Can access own data only
    Teacher,         // Can access students in assigned classes
    Parent,          // Can access own children's data
    Administrator,   // Can access all data in tenant
    Researcher       // Can access anonymized data only
}
```

**Permission Matrix:**
| Role | View Own Data | View Student Data | Edit Assessments | Administer |
|------|---------------|-------------------|------------------|------------|
| Student | ✅ | ❌ | ❌ | ❌ |
| Teacher | ✅ | ✅ (assigned) | ✅ | ❌ |
| Parent | ✅ | ✅ (children) | ❌ | ❌ |
| Administrator | ✅ | ✅ (tenant) | ✅ | ✅ |
| Researcher | ✅ (anonymized) | ✅ (anonymized) | ❌ | ❌ |

### 5.3 Audit Logging

**Audit Events:**
- Student data access
- Assessment submissions
- Content access
- Safety alerts
- Permission changes
- Data exports/deletions

**Retention:**
- FERPA: 7 years
- GDPR: As required by law
- General: 1 year minimum

---

## 6. Performance Optimization

### 6.1 Caching Strategy

**Multi-Level Caching:**
1. **L1: In-Memory** (per-instance)
   - Student profiles (1 hour TTL)
   - Frequently accessed questions (24 hours TTL)
2. **L2: Distributed** (Redis)
   - Mastery snapshots (1 hour TTL)
   - Assessment questions (24 hours TTL)
3. **L3: Database** (persistent)
   - All data with indexes

**Cache Invalidation:**
- Time-based (TTL)
- Event-based (on updates)
- Manual (admin-triggered)

### 6.2 Database Optimization

**Indexes:**
```sql
-- Student profiles
CREATE INDEX IX_StudentProfiles_Tenant_Student 
ON StudentProfiles(TenantId, StudentId);

-- Mastery snapshots
CREATE INDEX IX_MasterySnapshots_Student_Concept_Timestamp 
ON MasterySnapshots(StudentId, ConceptId, Timestamp DESC);

-- Access logs
CREATE INDEX IX_AccessLogs_Student_Timestamp 
ON AccessLogs(StudentId, Timestamp DESC);
```

**Query Optimization:**
- Use parameterized queries
- Batch operations where possible
- Connection pooling
- Read replicas for analytics

### 6.3 LLM Optimization

**Caching:**
- Cache generated questions (24 hours)
- Cache assessments (7 days)
- Cache hints (24 hours)

**Batch Processing:**
- Batch assessment generation
- Batch response evaluation
- Batch misconception detection

**Rate Limiting:**
- Per-student rate limits
- Per-tenant rate limits
- Circuit breakers for LLM providers

---

## 7. Testing Strategy

### 7.1 Unit Testing

**Coverage Requirements:**
- >90% code coverage
- All public APIs tested
- Edge cases covered
- Error paths tested

**Test Patterns:**
```csharp
[Fact]
public async Task GenerateQuestionAsync_WithValidConcept_ReturnsQuestion()
{
    // Arrange
    var engine = new SocraticDialogueEngine(_llmModel, _memory, _logger);
    var concept = new ConceptContext { ConceptId = "photosynthesis" };
    var understanding = StudentUnderstanding.Intermediate;
    
    // Act
    var question = await engine.GenerateQuestionAsync(concept, understanding);
    
    // Assert
    question.Should().NotBeNull();
    question.Type.Should().BeOneOf(
        SocraticQuestionType.Clarifying,
        SocraticQuestionType.Probing,
        SocraticQuestionType.Assumption,
        SocraticQuestionType.Implication,
        SocraticQuestionType.Viewpoint);
}
```

### 7.2 Integration Testing

**Test Scenarios:**
- End-to-end Socratic tutor workflow
- Assessment generation and evaluation
- Mastery tracking and reporting
- LMS/SIS integration
- Multi-tenant isolation

**Test Infrastructure:**
- Testcontainers for databases
- Mock LLM providers
- In-memory vector stores
- Test event publishers

### 7.3 Performance Testing

**Benchmarks:**
- Question generation: <500ms p95
- Response evaluation: <300ms p95
- Content filtering: <100ms p95
- Database queries: <200ms p95

**Load Testing:**
- 10,000 concurrent students
- 1,000 tenants
- 1M student profiles

---

## 8. Deployment

### 8.1 Infrastructure Requirements

**Application Servers:**
- .NET 10 runtime
- Minimum 4 CPU cores
- 8GB RAM
- SSD storage

**Database:**
- SQL Server 2019+ or PostgreSQL 14+
- Read replicas for analytics
- Automated backups

**Caching:**
- Redis 6+ (optional, for distributed caching)
- In-memory cache (default)

### 8.2 Configuration

**Environment Variables:**
```bash
EDUCATION__Database__ConnectionString=...
EDUCATION__LLM__Provider=OpenAI
EDUCATION__LLM__ApiKey=...
EDUCATION__Cache__Type=Redis
EDUCATION__Cache__ConnectionString=...
EDUCATION__Tenant__IsolationMode=Database
```

**Configuration File:**
```json
{
  "Education": {
    "Database": {
      "Provider": "SqlServer",
      "ConnectionString": "..."
    },
    "LLM": {
      "Provider": "OpenAI",
      "ApiKey": "...",
      "Model": "gpt-4"
    },
    "Cache": {
      "Type": "Redis",
      "ConnectionString": "...",
      "DefaultTtl": "01:00:00"
    },
    "Compliance": {
      "FerpaEnabled": true,
      "GdprEnabled": true,
      "AuditLogRetentionDays": 2555
    }
  }
}
```

---

## 9. Monitoring & Observability

### 9.1 Metrics

**Key Metrics:**
- Question generation latency
- Response evaluation latency
- Content filtering latency
- Mastery calculation accuracy
- Cache hit rates
- LLM API costs per student
- Error rates by component

**Integration:**
- OpenTelemetry for distributed tracing
- Prometheus for metrics
- Grafana for visualization

### 9.2 Logging

**Log Levels:**
- **Trace**: Detailed debugging information
- **Debug**: Development information
- **Information**: General information
- **Warning**: Warning messages
- **Error**: Error messages
- **Critical**: Critical failures

**Structured Logging:**
```csharp
_logger.LogInformation(
    "Question generated. Concept: {ConceptId}, Type: {QuestionType}, Duration: {Duration}ms",
    conceptId,
    questionType,
    duration.TotalMilliseconds);
```

### 9.3 Health Checks

**Health Check Endpoints:**
- `/health`: Overall health
- `/health/ready`: Readiness probe
- `/health/live`: Liveness probe
- `/health/database`: Database connectivity
- `/health/cache`: Cache connectivity
- `/health/llm`: LLM provider connectivity

---

## 10. Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0.0 | January 2025 | AI Assistant | Initial technical specification |
