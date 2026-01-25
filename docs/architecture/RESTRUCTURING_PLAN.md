# DotNetAgents Restructuring Plan

**Date:** January 2025  
**Status:** ✅ COMPLETE  
**Completed:** January 2025  
**Target:** DotNetAgents v2.0  
**Goal:** Separate core libraries from application implementations, enhance workflows, leverage .NET 10

> **Note:** This document is kept for historical reference. The restructuring has been completed. See [ARCHITECTURE_SUMMARY.md](./ARCHITECTURE_SUMMARY.md) for current architecture overview.

## Executive Summary

This plan restructures DotNetAgents into:
1. **Core Libraries** - Reusable, framework-agnostic components
2. **Application Implementations** - Complete applications built on core libraries
3. **Enhanced Workflow Support** - Human-in-the-loop and complex multi-step workflows
4. **.NET 10 Optimization** - Leverage latest AI features and performance improvements

## Current Issues

1. **Mixed Concerns**: Core libraries mixed with application-specific code (e.g., `DotNetAgents.Education`)
2. **Package Bloat**: Everything in one metapackage, even application-specific code
3. **Limited Workflow Features**: Basic human-in-the-loop, no complex branching
4. **SignalR Dependency**: Blocked by package compatibility issues
5. **Hard to Extend**: Applications like JARVIS require modifying core packages

## Proposed Architecture

### Solution Structure

```
DotNetAgents/
├── solutions/
│   ├── DotNetAgents.Core.sln          # Core libraries only
│   ├── DotNetAgents.Applications.sln   # Application implementations
│   └── DotNetAgents.All.sln            # Everything (for development)
│
├── src/
│   ├── Core/                           # Core libraries (NuGet packages)
│   │   ├── DotNetAgents.Abstractions/
│   │   ├── DotNetAgents.Core/
│   │   ├── DotNetAgents.Workflow/
│   │   ├── DotNetAgents.Agents.Registry/
│   │   ├── DotNetAgents.Agents.Messaging/
│   │   ├── DotNetAgents.Agents.Tasks/
│   │   ├── DotNetAgents.Agents.WorkerPool/
│   │   ├── DotNetAgents.Agents.Supervisor/
│   │   ├── DotNetAgents.Configuration/
│   │   ├── DotNetAgents.Observability/
│   │   ├── DotNetAgents.Security/
│   │   ├── DotNetAgents.Documents/
│   │   ├── DotNetAgents.Tasks/
│   │   ├── DotNetAgents.Knowledge/
│   │   └── DotNetAgents.Mcp/
│   │
│   ├── Providers/                      # LLM provider integrations
│   │   ├── DotNetAgents.Providers.OpenAI/
│   │   ├── DotNetAgents.Providers.Azure/
│   │   └── ... (all 12 providers)
│   │
│   ├── Storage/                        # Storage implementations
│   │   ├── DotNetAgents.Storage.Agents.PostgreSQL/
│   │   ├── DotNetAgents.Storage.TaskKnowledge.PostgreSQL/
│   │   └── DotNetAgents.Storage.TaskKnowledge.SqlServer/
│   │
│   ├── VectorStores/                   # Vector store integrations
│   │   ├── DotNetAgents.VectorStores.Pinecone/
│   │   ├── DotNetAgents.VectorStores.PostgreSQL/
│   │   └── ... (all vector stores)
│   │
│   ├── Messaging/                      # Message bus implementations
│   │   ├── DotNetAgents.Agents.Messaging.Kafka/
│   │   ├── DotNetAgents.Agents.Messaging.RabbitMQ/
│   │   ├── DotNetAgents.Agents.Messaging.Redis/
│   │   └── DotNetAgents.Agents.Messaging.SignalR/  # NEW: .NET 10 compatible
│   │
│   └── Tools/                          # Built-in tools
│       └── DotNetAgents.Tools.BuiltIn/
│
├── applications/                       # Application implementations (NOT NuGet packages)
│   ├── JARVIS/
│   │   ├── JARVIS.Core/                # Core JARVIS logic
│   │   ├── JARVIS.Voice/               # Voice processing
│   │   ├── JARVIS.Web/                 # Web interface
│   │   └── JARVIS.Host/                # Host application
│   │
│   ├── TeachingAssistant/
│   │   ├── TeachingAssistant.Core/    # Core teaching logic
│   │   ├── TeachingAssistant.Education/ # Education domain
│   │   ├── TeachingAssistant.Web/     # Web interface
│   │   └── TeachingAssistant.Host/    # Host application
│   │
│   └── PublishingAssistant/
│       ├── PublishingAssistant.Core/  # Core publishing logic
│       ├── PublishingAssistant.Content/ # Content management
│       ├── PublishingAssistant.Web/   # Web interface
│       └── PublishingAssistant.Host/  # Host application
│
├── tests/
│   ├── Core.Tests/                     # Tests for core libraries
│   ├── Applications.Tests/              # Tests for applications
│   └── Integration.Tests/              # Integration tests
│
└── samples/                            # Sample code using core libraries
    ├── BasicChain/
    ├── MultiAgent/
    └── ...
```

## Phase 1: Enhanced Workflow Support (Before Restructuring)

### 1.1 Human-in-the-Loop Enhancements

**Current State:**
- Basic `ApprovalNode` with simple approve/reject
- `IApprovalHandler` interface
- In-memory and SignalR implementations

**Enhancements Needed:**

1. **Decision Nodes** - Multiple choice decisions, not just approve/reject
   ```csharp
   public class DecisionNode<TState> : GraphNode<TState>
   {
       // Returns selected option from multiple choices
       Task<string> RequestDecisionAsync(string question, List<string> options);
   }
   ```

2. **Input Nodes** - Request specific data from humans
   ```csharp
   public class InputNode<TState> : GraphNode<TState>
   {
       // Request specific input (text, number, file, etc.)
       Task<TValue> RequestInputAsync<TValue>(string prompt, InputType type);
   }
   ```

3. **Review Nodes** - Allow humans to review and modify state
   ```csharp
   public class ReviewNode<TState> : GraphNode<TState>
   {
       // Human can review and modify state before proceeding
       Task<TState> RequestReviewAsync(TState state, string context);
   }
   ```

4. **Conditional Approval** - Branch based on approval decision
   ```csharp
   public class ConditionalApprovalNode<TState> : GraphNode<TState>
   {
       // Different paths based on approval outcome
       // Approved -> path A, Rejected -> path B, Modified -> path C
   }
   ```

5. **Workflow State Inspector** - Enhanced UI for state inspection
   - Visual state viewer
   - State modification interface
   - History/audit trail
   - Rollback capability

### 1.2 Complex Multi-Step Workflow Support

**Current State:**
- Basic state graph with nodes and edges
- Sequential execution
- Simple conditional edges

**Enhancements Needed:**

1. **Dynamic Branching** - Runtime decision on next node
   ```csharp
   public class DynamicBranchNode<TState> : GraphNode<TState>
   {
       // Returns next node name based on state
       Func<TState, string> BranchSelector { get; }
   }
   ```

2. **Parallel Execution** - Execute multiple branches simultaneously
   ```csharp
   public class ParallelNode<TState> : GraphNode<TState>
   {
       // Execute multiple nodes in parallel
       // Wait for all/some to complete
       Task<TState> ExecuteParallelAsync(List<string> nodeNames);
   }
   ```

3. **Loop Nodes** - Repeat nodes until condition met
   ```csharp
   public class LoopNode<TState> : GraphNode<TState>
   {
       // Execute node(s) until condition is false
       Func<TState, bool> ContinueCondition { get; }
       int MaxIterations { get; }
   }
   ```

4. **Sub-Workflows** - Nested workflow execution
   ```csharp
   public class SubWorkflowNode<TState> : GraphNode<TState>
   {
       // Execute another workflow as a node
       StateGraph<TSubState> SubWorkflow { get; }
       Func<TState, TSubState> StateMapper { get; }
   }
   ```

5. **Error Handling & Retry** - Built-in error handling
   ```csharp
   public class RetryNode<TState> : GraphNode<TState>
   {
       // Retry node on failure
       int MaxRetries { get; }
       TimeSpan RetryDelay { get; }
       Func<Exception, bool> ShouldRetry { get; }
   }
   ```

6. **Workflow Templates** - Reusable workflow patterns
   ```csharp
   public class WorkflowTemplate<TState>
   {
       // Parameterized workflow templates
       StateGraph<TState> CreateWorkflow(Dictionary<string, object> parameters);
   }
   ```

7. **State Validation** - Validate state between nodes
   ```csharp
   public class ValidationNode<TState> : GraphNode<TState>
   {
       // Validate state before proceeding
       Func<TState, ValidationResult> Validator { get; }
   }
   ```

### 1.3 .NET 10 Optimizations

1. **AI-Specific Features**
   - Use new AI-optimized collections
   - Leverage SIMD for vector operations
   - Use `System.Numerics.Tensors` for embeddings
   - Optimize memory allocations for LLM responses

2. **Performance Improvements**
   - Use `System.Threading.Channels` for async streaming
   - Leverage `IAsyncEnumerable<T>` throughout
   - Use `ValueTask` where appropriate
   - Implement `IAsyncDisposable` for resources

3. **SignalR .NET 10 Support**
   - Use `Microsoft.AspNetCore.SignalR.Client` (standalone package)
   - Support both server-side (ASP.NET Core) and client-side scenarios
   - Leverage new SignalR performance features

## Phase 2: Restructuring Implementation

### 2.1 Move Application Code

**From Core to Applications:**

1. **JARVIS Components**
   - Move `DotNetAgents.Voice.*` → `applications/JARVIS/JARVIS.Voice/`
   - Move JARVIS-specific code from core

2. **Education Components**
   - Move `DotNetAgents.Education` → `applications/TeachingAssistant/TeachingAssistant.Education/`
   - Keep only generic education interfaces in core if needed

3. **Application-Specific Tools**
   - Move application-specific tools to respective applications
   - Keep only generic tools in `DotNetAgents.Tools.BuiltIn`

### 2.2 Create Solution Files

1. **DotNetAgents.Core.sln**
   - All core libraries
   - All providers
   - All storage implementations
   - All vector stores
   - All messaging implementations
   - All tools

2. **DotNetAgents.Applications.sln**
   - All application projects
   - References core libraries as NuGet packages (or project references during development)

3. **DotNetAgents.All.sln**
   - Everything combined
   - For full solution development

### 2.3 Update Package Strategy

**Core Packages (NuGet):**
- `DotNetAgents.Core` - Core abstractions
- `DotNetAgents.Workflow` - Workflow engine
- `DotNetAgents.Agents.*` - Agent infrastructure
- `DotNetAgents.Providers.*` - LLM providers
- `DotNetAgents.Storage.*` - Storage implementations
- `DotNetAgents.VectorStores.*` - Vector stores
- `DotNetAgents.Messaging.*` - Message buses
- `DotNetAgents.Tools.BuiltIn` - Generic tools

**Application Packages (NOT NuGet, internal use):**
- Applications reference core packages
- Applications can be published as separate solutions/repos if needed
- Applications can have their own NuGet packages if they become reusable

## Phase 3: Implementation Plan

### Phase 3.1: Enhanced Workflow Features (Weeks 1-3)

**Week 1: Human-in-the-Loop Enhancements**
- [ ] Implement `DecisionNode<TState>`
- [ ] Implement `InputNode<TState>`
- [ ] Implement `ReviewNode<TState>`
- [ ] Enhance `ApprovalNode<TState>` with conditional branching
- [ ] Create enhanced `WorkflowStateInspector`
- [ ] Add SignalR support for all human-in-the-loop nodes
- [ ] Unit tests for all new nodes

**Week 2: Complex Workflow Features**
- [ ] Implement `DynamicBranchNode<TState>`
- [ ] Implement `ParallelNode<TState>`
- [ ] Implement `LoopNode<TState>`
- [ ] Implement `SubWorkflowNode<TState>`
- [ ] Implement `RetryNode<TState>`
- [ ] Implement `ValidationNode<TState>`
- [ ] Unit tests for all new nodes

**Week 3: Workflow Templates & Patterns**
- [ ] Implement `WorkflowTemplate<TState>`
- [ ] Create common workflow patterns (approval chain, parallel processing, retry loop)
- [ ] Documentation and examples
- [ ] Integration tests

### Phase 3.2: .NET 10 Optimizations (Week 4)

- [ ] Update all projects to use .NET 10 features
- [ ] Implement `IAsyncDisposable` where needed
- [ ] Optimize vector operations with `System.Numerics.Tensors`
- [ ] Update SignalR to use .NET 10 compatible packages
- [ ] Performance benchmarking
- [ ] Memory optimization

### Phase 3.3: Restructuring (Weeks 5-8)

**Week 5: Create New Structure**
- [ ] Create new solution files
- [ ] Create `applications/` directory structure
- [ ] Move JARVIS components
- [ ] Move Education components
- [ ] Update all project references

**Week 6: Update Build & CI/CD**
- [ ] Update build scripts
- [ ] Update CI/CD pipelines for multiple solutions
- [ ] Update package publishing
- [ ] Update documentation

**Week 7: Testing & Validation**
- [ ] Run all tests
- [ ] Fix any broken references
- [ ] Validate applications still work
- [ ] Performance testing

**Week 8: Documentation & Migration Guide**
- [ ] Update all documentation
- [ ] Create migration guide
- [ ] Update samples
- [ ] Update README files

## Detailed Component Specifications

### Enhanced Human-in-the-Loop Components

#### DecisionNode<TState>

```csharp
namespace DotNetAgents.Workflow.HumanInLoop;

public class DecisionNode<TState> : GraphNode<TState> where TState : class
{
    public DecisionNode(
        string name,
        IDecisionHandler<TState> decisionHandler,
        List<string> options,
        string? prompt = null,
        TimeSpan? timeout = null);
    
    // Returns state with Decision property set to selected option
}
```

#### InputNode<TState>

```csharp
public class InputNode<TState> : GraphNode<TState> where TState : class
{
    public InputNode(
        string name,
        IInputHandler<TState> inputHandler,
        string propertyName,
        InputType inputType,
        string? prompt = null,
        object? defaultValue = null);
    
    // Sets property on state with user input
}
```

#### ReviewNode<TState>

```csharp
public class ReviewNode<TState> : GraphNode<TState> where TState : class
{
    public ReviewNode(
        string name,
        IReviewHandler<TState> reviewHandler,
        string? context = null,
        bool allowModification = true);
    
    // Returns potentially modified state
}
```

### Complex Workflow Components

#### ParallelNode<TState>

```csharp
public class ParallelNode<TState> : GraphNode<TState> where TState : class
{
    public ParallelNode(
        string name,
        List<string> parallelNodeNames,
        ParallelExecutionMode mode = ParallelExecutionMode.WaitForAll);
    
    // Executes nodes in parallel, waits based on mode
}

public enum ParallelExecutionMode
{
    WaitForAll,      // Wait for all to complete
    WaitForAny,      // Proceed when any completes
    WaitForMajority, // Proceed when majority complete
    WaitForCount     // Proceed when N complete
}
```

#### LoopNode<TState>

```csharp
public class LoopNode<TState> : GraphNode<TState> where TState : class
{
    public LoopNode(
        string name,
        string loopNodeName,
        Func<TState, bool> continueCondition,
        int maxIterations = 100);
    
    // Executes node repeatedly until condition is false
}
```

#### SubWorkflowNode<TState>

```csharp
public class SubWorkflowNode<TState, TSubState> : GraphNode<TState> 
    where TState : class
    where TSubState : class
{
    public SubWorkflowNode(
        string name,
        StateGraph<TSubState> subWorkflow,
        Func<TState, TSubState> stateMapper,
        Func<TSubState, TState> resultMapper);
    
    // Executes sub-workflow and maps results back
}
```

## Migration Strategy

### For Existing Users

1. **Core Libraries**: No changes, same NuGet packages
2. **Applications**: 
   - JARVIS: Move to new `applications/JARVIS/` structure
   - Education: Move to `applications/TeachingAssistant/`
3. **Workflow Enhancements**: New nodes are additive, existing code continues to work

### Breaking Changes

- None for core libraries
- Application code needs to be moved (but functionality unchanged)
- New workflow features are opt-in

## Success Criteria

1. ✅ Core libraries remain stable and backward compatible
2. ✅ Applications are cleanly separated from core
3. ✅ Enhanced workflow features work seamlessly
4. ✅ .NET 10 optimizations provide measurable performance improvements
5. ✅ SignalR works with .NET 10
6. ✅ All tests pass
7. ✅ Documentation is complete
8. ✅ Migration path is clear

## Timeline

- **Weeks 1-3**: Enhanced workflow features
- **Week 4**: .NET 10 optimizations
- **Weeks 5-8**: Restructuring
- **Total**: 8 weeks

## Next Steps

1. Review and approve this plan
2. Create detailed task breakdown
3. Begin Phase 3.1 implementation
4. Set up new solution structure
5. Start migration process
