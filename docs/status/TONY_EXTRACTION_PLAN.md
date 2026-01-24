# JARVIS - Just A Rather Very Intelligent System
## Extraction & Implementation Plan

**Analysis Date:** January 2025  
**Target System:** JARVIS - A JARVIS-like AI assistant for business management using agents, voice commands, and complex workflows  
**Inspiration:** Tony Stark's JARVIS from Iron Man  
**Source Projects:** 
- `D:\Projects\vcams` (Voice Command Agentic Management System)
- `D:\Projects\VoiceDropbox` (Voice Transcription Service)

---

## Executive Summary

This document outlines the extraction and integration plan for building **JARVIS** (Just A Rather Very Intelligent System), a JARVIS-like AI assistant for business management powered by DotNetAgents. JARVIS will leverage voice commands, agentic workflows, and MCP (Model Context Protocol) integrations to provide an intelligent, conversational business management assistant.

### Vision

Build a true JARVIS-like system that:
- Understands natural language voice commands
- Provides proactive assistance and suggestions
- Learns from user patterns and preferences
- Integrates seamlessly with business systems
- Maintains conversational context
- Executes complex workflows intelligently

### Key Components to Extract/Integrate

1. **Voice Command Processing** - Intent classification, parsing, dialog management
2. **MCP Client Library** - Connect to MCP services for business operations
3. **Voice Transcription** - Convert audio to text (Whisper integration)
4. **Command Orchestration** - Workflow orchestration with confirmation flows
5. **Real-time Updates** - SignalR integration for status updates
6. **Dialog Management** - Multi-turn clarification conversations
7. **Read-back Service** - TTS confirmation patterns

---

## 1. Voice Command Processing

### 1.1 Intent Classification

**Source:** `vcams/src/VoiceCommand.Application/Services/LlmIntentClassifier.cs`

**What to Extract:**
- LLM-based intent classification using structured prompts
- Intent taxonomy (domain, action, subType, parameters)
- Confidence scoring
- Fallback intent handling

**Integration into DotNetAgents:**
- Create `DotNetAgents.Voice` package
- Implement `IIntentClassifier` interface
- Use existing `ILLMModel` abstraction
- Support multiple LLM providers (OpenAI, Anthropic, Ollama, etc.)

**New Components:**
```csharp
// DotNetAgents.Voice/IntentClassification/
public interface IIntentClassifier
{
    Task<Intent> ClassifyAsync(string commandText, CancellationToken cancellationToken = default);
}

public record Intent
{
    public string Domain { get; init; }           // e.g., "tasks", "calendar", "business"
    public string Action { get; init; }            // e.g., "create", "list", "update"
    public string? SubType { get; init; }          // e.g., "personal", "team", "invoice"
    public Dictionary<string, object> Parameters { get; init; }
    public List<string> MissingRequired { get; init; }
    public double Confidence { get; init; }
    public string FullName => $"{Domain}.{Action}";
}
```

**Files to Create:**
- `src/DotNetAgents.Voice/IntentClassification/IIntentClassifier.cs`
- `src/DotNetAgents.Voice/IntentClassification/LLMIntentClassifier.cs`
- `src/DotNetAgents.Voice/IntentClassification/Intent.cs`
- `src/DotNetAgents.Voice/IntentClassification/IntentTaxonomy.cs` (configuration)

### 1.2 Command Parser

**Source:** `vcams/src/VoiceCommand.Application/Services/CommandParser.cs`

**What to Extract:**
- Command parsing orchestration
- Integration with intent classifier
- Error handling and logging

**Integration:**
- Simple wrapper around `IIntentClassifier`
- Can be part of `DotNetAgents.Voice` package

**Files to Create:**
- `src/DotNetAgents.Voice/Parsing/ICommandParser.cs`
- `src/DotNetAgents.Voice/Parsing/CommandParser.cs`

---

## 2. MCP Client Library

### 2.1 MCP Client Core

**Source:** `vcams/src/VoiceCommand.Mcp/`

**What to Extract:**
- HTTP-based MCP client implementation
- Tool discovery (`ListToolsAsync`)
- Tool execution (`CallToolAsync`)
- Health checking
- Service configuration

**Integration into DotNetAgents:**
- Create `DotNetAgents.Mcp` package
- Implement MCP protocol specification
- Support both HTTP and stdio transports
- Integrate with existing `ITool` system

**New Components:**
```csharp
// DotNetAgents.Mcp/
public interface IMcpClient
{
    string ServiceName { get; }
    Task<McpListToolsResponse> ListToolsAsync(McpListToolsRequest? request = null, CancellationToken cancellationToken = default);
    Task<McpToolCallResponse> CallToolAsync(McpToolCallRequest request, CancellationToken cancellationToken = default);
    Task<McpServiceHealth> GetHealthAsync(CancellationToken cancellationToken = default);
}

public interface IMcpClientFactory
{
    IMcpClient GetClient(string serviceName);
    IEnumerable<string> GetRegisteredServices();
}

public interface IMcpToolRegistry
{
    Task<List<McpToolDefinition>> GetAllToolsAsync(CancellationToken cancellationToken = default);
    Task<List<McpToolDefinition>> GetToolsForServiceAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<McpToolDefinition?> FindToolAsync(string toolName, CancellationToken cancellationToken = default);
}
```

**Files to Extract/Create:**
- `src/DotNetAgents.Mcp/Abstractions/IMcpClient.cs`
- `src/DotNetAgents.Mcp/McpClient.cs` (HTTP implementation)
- `src/DotNetAgents.Mcp/McpClientFactory.cs`
- `src/DotNetAgents.Mcp/McpToolRegistry.cs`
- `src/DotNetAgents.Mcp/Models/McpToolDefinition.cs`
- `src/DotNetAgents.Mcp/Models/McpToolCallRequest.cs`
- `src/DotNetAgents.Mcp/Models/McpToolCallResponse.cs`
- `src/DotNetAgents.Mcp/Models/McpServiceHealth.cs`
- `src/DotNetAgents.Mcp/Configuration/McpServiceConfig.cs`

### 2.2 MCP Adapter Router

**Source:** `vcams/src/VoiceCommand.Application/Services/McpAdapterRouter.cs`

**What to Extract:**
- Intent-to-service routing logic
- Service adapter pattern
- Domain-specific adapters (Business, Time, Session, etc.)

**Integration:**
- Create adapter interfaces in `DotNetAgents.Mcp`
- Allow registration of custom adapters
- Use dependency injection for adapter resolution

**New Components:**
```csharp
// DotNetAgents.Mcp/Routing/
public interface IMcpAdapterRouter
{
    Task<object?> ExecuteIntentAsync(Intent intent, CancellationToken cancellationToken = default);
}

public interface IMcpAdapter
{
    string ServiceName { get; }
    bool CanHandle(Intent intent);
    Task<object?> ExecuteAsync(Intent intent, CancellationToken cancellationToken = default);
}
```

**Files to Create:**
- `src/DotNetAgents.Mcp/Routing/IMcpAdapterRouter.cs`
- `src/DotNetAgents.Mcp/Routing/McpAdapterRouter.cs`
- `src/DotNetAgents.Mcp/Routing/IMcpAdapter.cs`
- `src/DotNetAgents.Mcp/Adapters/BaseMcpAdapter.cs` (base class)

---

## 3. Voice Transcription

### 3.1 Whisper Integration

**Source:** `VoiceDropbox/voice_watcher.py`

**What to Extract:**
- Folder watching for audio files
- Whisper transcription integration
- File organization and archiving
- Error handling and notifications

**Integration into DotNetAgents:**
- Create `DotNetAgents.Voice.Transcription` package
- Implement C# wrapper for Whisper (via Python subprocess or native C# library)
- Support multiple audio formats
- Integrate with existing document loaders

**New Components:**
```csharp
// DotNetAgents.Voice.Transcription/
public interface IVoiceTranscriptionService
{
    Task<TranscriptionResult> TranscribeAsync(string audioFilePath, TranscriptionOptions? options = null, CancellationToken cancellationToken = default);
    Task<TranscriptionResult> TranscribeAsync(Stream audioStream, TranscriptionOptions? options = null, CancellationToken cancellationToken = default);
}

public interface IVoiceFileWatcher
{
    event EventHandler<AudioFileDetectedEventArgs> AudioFileDetected;
    void StartWatching(string folderPath);
    void StopWatching();
}

public record TranscriptionResult
{
    public string Text { get; init; }
    public string Language { get; init; }
    public TimeSpan Duration { get; init; }
    public double Confidence { get; init; }
    public Dictionary<string, object> Metadata { get; init; }
}
```

**Files to Create:**
- `src/DotNetAgents.Voice.Transcription/IVoiceTranscriptionService.cs`
- `src/DotNetAgents.Voice.Transcription/WhisperTranscriptionService.cs` (Python subprocess wrapper)
- `src/DotNetAgents.Voice.Transcription/IVoiceFileWatcher.cs`
- `src/DotNetAgents.Voice.Transcription/VoiceFileWatcher.cs`
- `src/DotNetAgents.Voice.Transcription/TranscriptionResult.cs`
- `src/DotNetAgents.Voice.Transcription/TranscriptionOptions.cs`

**Note:** Consider using a native C# Whisper library (e.g., `Whisper.net`) instead of Python subprocess for better performance and integration.

---

## 4. Command Orchestration & Workflow

### 4.1 Command Workflow Orchestrator

**Source:** `vcams/src/VoiceCommand.Application/Orchestration/CommandWorkflowOrchestrator.cs`

**What to Extract:**
- Command lifecycle management
- Status transitions
- Integration with dialog manager
- MCP call execution
- Real-time status updates

**Integration:**
- Use existing `DotNetAgents.Workflow` engine
- Create voice-specific workflow nodes
- Integrate with `StateGraph<TState>`

**New Components:**
```csharp
// DotNetAgents.Voice/Orchestration/
public record CommandState
{
    public Guid CommandId { get; init; }
    public Guid UserId { get; init; }
    public string RawText { get; init; }
    public string Source { get; init; }
    public CommandStatus Status { get; init; }
    public Intent? Intent { get; init; }
    public string? TargetService { get; init; }
    public bool Confirmed { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public object? Result { get; init; }
    public string? Error { get; init; }
}

public interface ICommandWorkflowOrchestrator
{
    Task<CommandState> ExecuteAsync(CommandState state, CancellationToken cancellationToken = default);
}
```

**Files to Create:**
- `src/DotNetAgents.Voice/Orchestration/CommandState.cs`
- `src/DotNetAgents.Voice/Orchestration/CommandStatus.cs` (enum)
- `src/DotNetAgents.Voice/Orchestration/ICommandWorkflowOrchestrator.cs`
- `src/DotNetAgents.Voice/Orchestration/CommandWorkflowOrchestrator.cs`
- `src/DotNetAgents.Voice/Orchestration/CommandWorkflowBuilder.cs` (fluent API)

### 4.2 Dialog Management

**Source:** `vcams/src/VoiceCommand.Application/Services/DialogStateManager.cs`

**What to Extract:**
- Multi-turn clarification dialogs
- Parameter collection
- Dialog state persistence
- Turn management

**Integration:**
- Use existing `DotNetAgents.Workflow` for dialog state
- Create dialog-specific workflow nodes
- Integrate with `IMemory` for dialog persistence

**New Components:**
```csharp
// DotNetAgents.Voice/Dialog/
public interface IDialogManager
{
    Task<DialogState> CreateDialogAsync(string commandId, List<string> missingParameters, CancellationToken cancellationToken = default);
    Task<DialogState?> GetDialogAsync(string commandId, CancellationToken cancellationToken = default);
    Task<DialogState> ProcessResponseAsync(string commandId, string userResponse, CancellationToken cancellationToken = default);
    Task CompleteDialogAsync(string commandId, CancellationToken cancellationToken = default);
}

public record DialogState
{
    public Guid DialogId { get; init; }
    public string CommandId { get; init; }
    public List<string> MissingParameters { get; init; }
    public int CurrentParameterIndex { get; init; }
    public Dictionary<string, object> CollectedValues { get; init; }
    public int TurnCount { get; init; }
    public DialogStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
```

**Files to Create:**
- `src/DotNetAgents.Voice/Dialog/IDialogManager.cs`
- `src/DotNetAgents.Voice/Dialog/DialogManager.cs`
- `src/DotNetAgents.Voice/Dialog/DialogState.cs`
- `src/DotNetAgents.Voice/Dialog/DialogStatus.cs` (enum)
- `src/DotNetAgents.Voice/Dialog/DialogTurn.cs` (record)

---

## 5. Read-back & Confirmation Service

### 5.1 Read-back Service

**Source:** `vcams/src/VoiceCommand.Application/Services/ReadBackService.cs`

**What to Extract:**
- Template-based read-back generation
- Domain-specific templates
- Parameter substitution
- Confirmation prompts

**Integration:**
- Create `DotNetAgents.Voice.Confirmation` package
- Support custom templates
- Integrate with TTS services

**New Components:**
```csharp
// DotNetAgents.Voice/Confirmation/
public interface IReadBackService
{
    string GenerateReadBack(Intent intent, Dictionary<string, object> parameters);
    string GenerateReadBackWithConfirmation(Intent intent, Dictionary<string, object> parameters);
    void RegisterTemplate(string intentName, string template);
}

public interface ITextToSpeechService
{
    Task<Stream> SynthesizeAsync(string text, TtsOptions? options = null, CancellationToken cancellationToken = default);
    Task<string> SynthesizeToFileAsync(string text, string outputPath, TtsOptions? options = null, CancellationToken cancellationToken = default);
}
```

**Files to Create:**
- `src/DotNetAgents.Voice/Confirmation/IReadBackService.cs`
- `src/DotNetAgents.Voice/Confirmation/ReadBackService.cs`
- `src/DotNetAgents.Voice/Confirmation/ITextToSpeechService.cs`
- `src/DotNetAgents.Voice/Confirmation/TtsOptions.cs`
- `src/DotNetAgents.Voice/Confirmation/ReadBackTemplates.cs` (configuration)

---

## 6. Real-time Updates (SignalR)

### 6.1 SignalR Integration

**Source:** `vcams/src/VoiceCommand.Api/Hubs/`

**What to Extract:**
- SignalR hub for command status updates
- Real-time notification patterns
- Client connection management

**Integration:**
- Create `DotNetAgents.Voice.SignalR` package
- Provide SignalR hub base classes
- Integrate with workflow engine for status updates

**New Components:**
```csharp
// DotNetAgents.Voice.SignalR/
public interface ICommandNotificationService
{
    Task SendStatusUpdateAsync(Guid userId, Guid commandId, CommandStatus status, string? message = null, object? data = null, CancellationToken cancellationToken = default);
    Task SendClarificationRequestAsync(Guid userId, Guid commandId, string prompt, string parameterName, int turn, int maxTurns, CancellationToken cancellationToken = default);
    Task SendConfirmationRequestAsync(Guid userId, Guid commandId, string readBackText, CancellationToken cancellationToken = default);
    Task SendCommandCompletedAsync(Guid userId, Guid commandId, object? result, CancellationToken cancellationToken = default);
    Task SendCommandFailedAsync(Guid userId, Guid commandId, string error, CancellationToken cancellationToken = default);
}

public abstract class CommandHub : Hub
{
    public abstract Task JoinUserGroup(string userId);
    public abstract Task LeaveUserGroup(string userId);
}
```

**Files to Create:**
- `src/DotNetAgents.Voice.SignalR/ICommandNotificationService.cs`
- `src/DotNetAgents.Voice.SignalR/SignalRCommandNotificationService.cs`
- `src/DotNetAgents.Voice.SignalR/CommandHub.cs`
- `src/DotNetAgents.Voice.SignalR/CommandNotificationExtensions.cs` (DI registration)

---

## 7. Package Structure

### Proposed New Packages

```
DotNetAgents.Voice/
├── DotNetAgents.Voice.csproj
├── IntentClassification/
├── Parsing/
├── Orchestration/
├── Dialog/
└── Confirmation/

DotNetAgents.Voice.Transcription/
├── DotNetAgents.Voice.Transcription.csproj
├── IVoiceTranscriptionService.cs
├── WhisperTranscriptionService.cs
└── VoiceFileWatcher.cs

DotNetAgents.Mcp/
├── DotNetAgents.Mcp.csproj
├── Abstractions/
├── McpClient.cs
├── Routing/
└── Adapters/

DotNetAgents.Voice.SignalR/
├── DotNetAgents.Voice.SignalR.csproj
├── ICommandNotificationService.cs
└── CommandHub.cs
```

---

## 8. Integration with Existing DotNetAgents

### 8.1 Workflow Integration

- Use `DotNetAgents.Workflow.StateGraph<TState>` for command workflows
- Create voice-specific workflow nodes:
  - `ParseCommandNode`
  - `ClarifyParametersNode`
  - `ConfirmCommandNode`
  - `ExecuteMcpCallNode`
  - `SendNotificationNode`

### 8.2 Tool Integration

- MCP tools integrate with existing `ITool` interface
- Register MCP tools in `IToolRegistry`
- Use existing tool execution infrastructure

### 8.3 Memory Integration

- Use `DotNetAgents.Core.Memory` for dialog state persistence
- Use `DotNetAgents.Tasks` for command tracking
- Use `DotNetAgents.Knowledge` for command history

### 8.4 LLM Integration

- Use existing `ILLMModel` for intent classification
- Support all existing LLM providers
- Leverage existing caching and retry logic

---

## 9. Implementation Phases

### Phase 1: Core Voice Processing (Weeks 1-2)
- [ ] Create `DotNetAgents.Voice` package
- [ ] Implement `IIntentClassifier` and `LLMIntentClassifier`
- [ ] Implement `ICommandParser`
- [ ] Create `Intent` and `IntentTaxonomy` models
- [ ] Unit tests for intent classification

### Phase 2: MCP Client Library (Weeks 3-4)
- [ ] Create `DotNetAgents.Mcp` package
- [ ] Implement `IMcpClient` and HTTP client
- [ ] Implement `IMcpClientFactory` and `IMcpToolRegistry`
- [ ] Create MCP models (ToolDefinition, ToolCallRequest/Response)
- [ ] Implement `IMcpAdapterRouter`
- [ ] Unit tests for MCP client

### Phase 3: Voice Transcription (Week 5)
- [ ] Create `DotNetAgents.Voice.Transcription` package
- [ ] Implement `IVoiceTranscriptionService` (Whisper integration)
- [ ] Implement `IVoiceFileWatcher`
- [ ] Support multiple audio formats
- [ ] Unit tests for transcription

### Phase 4: Command Orchestration (Weeks 6-7)
- [ ] Implement `CommandState` and `CommandStatus`
- [ ] Implement `ICommandWorkflowOrchestrator`
- [ ] Create workflow nodes for command processing
- [ ] Integrate with `DotNetAgents.Workflow`
- [ ] Unit tests for orchestration

### Phase 5: Dialog Management (Week 8)
- [ ] Implement `IDialogManager`
- [ ] Create `DialogState` model
- [ ] Integrate with workflow engine
- [ ] Unit tests for dialog management

### Phase 6: Read-back & Confirmation (Week 9)
- [ ] Implement `IReadBackService`
- [ ] Create template system
- [ ] Implement `ITextToSpeechService` (optional - can use external service)
- [ ] Unit tests for read-back

### Phase 7: SignalR Integration (Week 10)
- [ ] Create `DotNetAgents.Voice.SignalR` package
- [ ] Implement `ICommandNotificationService`
- [ ] Create `CommandHub` base class
- [ ] Integrate with orchestration
- [ ] Unit tests for SignalR

### Phase 8: Integration & Testing (Weeks 11-12)
- [ ] End-to-end integration tests
- [ ] Sample application (Tony prototype)
- [ ] Documentation
- [ ] Performance testing

---

## 10. Sample Application: JARVIS

### JARVIS Architecture

```
JARVIS - Just A Rather Very Intelligent System
├── Voice Interface (Mobile/Web)
│   ├── Voice recording
│   ├── Transcription
│   ├── TTS playback
│   └── Real-time status updates
├── API Gateway
│   ├── Authentication
│   ├── Rate limiting
│   └── Request routing
├── Command Processing
│   ├── Intent classification
│   ├── Command parsing
│   ├── Dialog management
│   └── Confirmation flow
├── Workflow Engine (DotNetAgents.Workflow)
│   ├── Command workflows
│   ├── Business process workflows
│   └── State management
└── MCP Services
    ├── Business Manager (invoices, clients, projects)
    ├── Time Management (calendar, reminders)
    ├── Task Management (DotNetAgents.Tasks)
    └── Knowledge Base (DotNetAgents.Knowledge)
```

### JARVIS Features

1. **Natural Language Voice Commands**
   - "JARVIS, create an invoice for Acme Corp for five thousand dollars"
   - "JARVIS, schedule a meeting with John tomorrow at 2pm"
   - "JARVIS, what tasks do I have pending?"
   - "JARVIS, create a project for New Website Development"
   - "JARVIS, analyze my schedule and suggest optimizations"
   - "JARVIS, what's my cash flow looking like this month?"

2. **Intelligent Business Operations**
   - Invoice creation and management with smart suggestions
   - Client management with relationship insights
   - Project tracking with predictive analytics
   - Time tracking with productivity insights
   - Expense management with categorization

3. **Proactive Assistance**
   - "You have a meeting in 15 minutes"
   - "I noticed you haven't followed up on invoice INV-001"
   - "Based on your patterns, I suggest scheduling buffer time"
   - "Three clients haven't responded - should I send reminders?"

4. **Workflow Automation**
   - Multi-step business processes
   - Approval workflows with intelligent routing
   - Automated reminders and follow-ups
   - Report generation and analysis
   - Cross-domain insights (tasks + calendar + business data)

5. **Learning & Adaptation**
   - Learns user preferences and patterns
   - Adapts to user's communication style
   - Suggests optimizations based on historical data
   - Context-aware responses

---

## 11. Dependencies

### New NuGet Packages Needed

- `Microsoft.AspNetCore.SignalR` (for SignalR integration)
- `System.IO.Abstractions` (for file watching)
- `Whisper.net` or similar (for transcription - optional, can use Python subprocess)

### Existing DotNetAgents Packages Used

- `DotNetAgents.Core` - Core abstractions
- `DotNetAgents.Workflow` - Workflow engine
- `DotNetAgents.Tasks` - Task management
- `DotNetAgents.Knowledge` - Knowledge repository
- `DotNetAgents.Providers.*` - LLM providers

---

## 12. Testing Strategy

### Unit Tests
- Intent classification accuracy
- Command parsing edge cases
- MCP client error handling
- Dialog state management
- Read-back template generation

### Integration Tests
- End-to-end command processing
- MCP service integration
- Workflow execution
- SignalR real-time updates

### Performance Tests
- Intent classification latency
- Transcription throughput
- Concurrent command processing
- MCP call performance

---

## 13. Documentation

### Required Documentation

1. **API Documentation**
   - Voice command API reference
   - MCP client API reference
   - SignalR hub documentation

2. **Integration Guides**
   - How to add custom MCP services
   - How to create custom intent classifiers
   - How to extend dialog management

3. **Samples**
   - Basic voice command example
   - MCP service integration example
   - Tony business management example

---

## 14. Success Criteria

1. ✅ Voice commands can be processed end-to-end
2. ✅ Intent classification accuracy > 90%
3. ✅ MCP services can be discovered and called
4. ✅ Multi-turn dialogs work correctly
5. ✅ Real-time status updates via SignalR
6. ✅ Tony prototype demonstrates full business management workflow
7. ✅ All components have >85% test coverage
8. ✅ Documentation is complete

---

## 15. Next Steps

1. **Review and Approve Plan** - Review this extraction plan with stakeholders
2. **Set Up Projects** - Create new package projects in DotNetAgents solution
3. **Start Phase 1** - Begin with voice command processing core
4. **Iterate** - Build incrementally, test frequently, integrate continuously

---

**Last Updated:** January 2025  
**Status:** Planning Phase  
**Vision Document:** See [`JARVIS_VISION.md`](./JARVIS_VISION.md) for design principles and vision
