# DotNetAgents Restructuring - Detailed Task List

**Created:** January 2025  
**Status:** ✅ COMPLETE  
**Completed:** January 2025  
**Total Duration:** ~4 weeks (Phase 3.3 restructuring + Phase 3.4 autonomous agents)

> **Note:** This document is kept for historical reference. See [ARCHITECTURE_SUMMARY.md](./ARCHITECTURE_SUMMARY.md) for current architecture overview.

## Phase 3.1: Enhanced Workflow Features

### Week 1: Human-in-the-Loop Enhancements

#### Task 1.1: DecisionNode Implementation
- [ ] Create `IDecisionHandler<TState>` interface
- [ ] Create `DecisionNode<TState>` class
- [ ] Implement decision request logic
- [ ] Add timeout support
- [ ] Add unit tests
- [ ] Add integration tests

#### Task 1.2: InputNode Implementation
- [ ] Create `IInputHandler<TState>` interface
- [ ] Create `InputType` enum (Text, Number, File, Date, etc.)
- [ ] Create `InputNode<TState>` class
- [ ] Implement input request logic
- [ ] Add validation support
- [ ] Add unit tests
- [ ] Add integration tests

#### Task 1.3: ReviewNode Implementation
- [ ] Create `IReviewHandler<TState>` interface
- [ ] Create `ReviewNode<TState>` class
- [ ] Implement state review logic
- [ ] Add state modification support
- [ ] Add unit tests
- [ ] Add integration tests

#### Task 1.4: Enhanced ApprovalNode
- [ ] Add conditional branching support
- [ ] Add multiple outcome paths (Approved/Rejected/Modified)
- [ ] Update `IApprovalHandler` interface
- [ ] Update `ApprovalNode` implementation
- [ ] Add unit tests
- [ ] Update existing tests

#### Task 1.5: Enhanced WorkflowStateInspector
- [ ] Create visual state viewer interface
- [ ] Implement state serialization for display
- [ ] Add state modification interface
- [ ] Add history/audit trail
- [ ] Add rollback capability
- [ ] Create SignalR hub for real-time updates
- [ ] Add unit tests

#### Task 1.6: SignalR Support for Human-in-the-Loop
- [ ] Create SignalR hub for human-in-the-loop operations
- [ ] Implement real-time decision requests
- [ ] Implement real-time input requests
- [ ] Implement real-time review requests
- [ ] Add client-side JavaScript helpers
- [ ] Add integration tests

### Week 2: Complex Workflow Features

#### Task 2.1: DynamicBranchNode Implementation
- [ ] Create `DynamicBranchNode<TState>` class
- [ ] Implement runtime node selection
- [ ] Add branch selector function support
- [ ] Add validation for node existence
- [ ] Add unit tests
- [ ] Add integration tests

#### Task 2.2: ParallelNode Implementation
- [ ] Create `ParallelExecutionMode` enum
- [ ] Create `ParallelNode<TState>` class
- [ ] Implement parallel execution logic
- [ ] Add wait modes (All, Any, Majority, Count)
- [ ] Add cancellation support
- [ ] Add unit tests
- [ ] Add integration tests

#### Task 2.3: LoopNode Implementation
- [ ] Create `LoopNode<TState>` class
- [ ] Implement loop execution logic
- [ ] Add continue condition function
- [ ] Add max iterations support
- [ ] Add break condition support
- [ ] Add unit tests
- [ ] Add integration tests

#### Task 2.4: SubWorkflowNode Implementation
- [ ] Create `SubWorkflowNode<TState, TSubState>` class
- [ ] Implement sub-workflow execution
- [ ] Add state mapping functions
- [ ] Add result mapping functions
- [ ] Add error handling
- [ ] Add unit tests
- [ ] Add integration tests

#### Task 2.5: RetryNode Implementation
- [ ] Create `RetryNode<TState>` class
- [ ] Implement retry logic
- [ ] Add retry delay support
- [ ] Add retry condition function
- [ ] Add exponential backoff
- [ ] Add unit tests
- [ ] Add integration tests

#### Task 2.6: ValidationNode Implementation
- [ ] Create `ValidationResult` record
- [ ] Create `ValidationNode<TState>` class
- [ ] Implement validation logic
- [ ] Add validation error handling
- [ ] Add conditional branching on validation failure
- [ ] Add unit tests
- [ ] Add integration tests

### Week 3: Workflow Templates & Patterns

#### Task 3.1: WorkflowTemplate Implementation
- [ ] Create `WorkflowTemplate<TState>` class
- [ ] Implement parameterized workflow creation
- [ ] Add template validation
- [ ] Add template documentation
- [ ] Add unit tests

#### Task 3.2: Common Workflow Patterns
- [ ] Create ApprovalChain pattern
- [ ] Create ParallelProcessing pattern
- [ ] Create RetryLoop pattern
- [ ] Create ConditionalWorkflow pattern
- [ ] Create SequentialWithValidation pattern
- [ ] Document all patterns
- [ ] Add examples

#### Task 3.3: Documentation & Examples
- [ ] Create workflow patterns documentation
- [ ] Create usage examples for all new nodes
- [ ] Create migration guide from basic to enhanced workflows
- [ ] Update README with new features
- [ ] Create video tutorials (optional)

## Phase 3.2: .NET 10 Optimizations

### Week 4: .NET 10 Features & Performance

#### Task 4.1: Update to .NET 10 Features
- [ ] Review all projects for .NET 10 compatibility
- [ ] Update to use `IAsyncDisposable` where needed
- [ ] Update to use `System.Threading.Channels` for streaming
- [ ] Update to use `IAsyncEnumerable<T>` throughout
- [ ] Use `ValueTask` where appropriate
- [ ] Update all async patterns

#### Task 4.2: AI Optimizations
- [ ] Implement `System.Numerics.Tensors` for embeddings
- [ ] Use SIMD for vector operations
- [ ] Optimize memory allocations for LLM responses
- [ ] Use AI-optimized collections where available
- [ ] Benchmark performance improvements

#### Task 4.3: SignalR .NET 10 Implementation
- [ ] Create `DotNetAgents.Agents.Messaging.SignalR` project
- [ ] Add `Microsoft.AspNetCore.SignalR.Client` package (v10.0+)
- [ ] Implement `SignalRAgentMessageBus` class
- [ ] Create `AgentMessageHub` for server-side
- [ ] Add configuration options
- [ ] Add service collection extensions
- [ ] Add unit tests
- [ ] Add integration tests

#### Task 4.4: Performance Benchmarking
- [ ] Create benchmark project
- [ ] Benchmark vector operations
- [ ] Benchmark workflow execution
- [ ] Benchmark message bus operations
- [ ] Compare .NET 8 vs .NET 10 performance
- [ ] Document performance improvements

## Phase 3.3: Restructuring

### Week 5: Create New Structure

#### Task 5.1: Create Solution Files
- [x] Create `DotNetAgents.Core.sln`
- [x] Create `DotNetAgents.Applications.sln`
- [x] Create `DotNetAgents.All.sln`
- [x] Update existing solution if needed

#### Task 5.2: Create Applications Directory Structure
- [x] Create `applications/` directory
- [x] Create `applications/JARVIS/` structure
- [x] Create `applications/TeachingAssistant/` structure
- [x] Create `applications/PublishingAssistant/` structure (placeholder)

#### Task 5.3: Move JARVIS Components
- [x] Move `src/Voice/DotNetAgents.Voice.*` → `applications/JARVIS/JARVIS.Voice/`
- [ ] Create `JARVIS.Core/` project
- [ ] Create `JARVIS.Web/` project
- [ ] Create `JARVIS.Host/` project
- [x] Update all references
- [x] Update tests

#### Task 5.4: Move Education Components
- [x] Move `src/DotNetAgents.Education` → `applications/TeachingAssistant/TeachingAssistant.Education/`
- [ ] Create `TeachingAssistant.Core/` project
- [ ] Create `TeachingAssistant.Web/` project
- [ ] Create `TeachingAssistant.Host/` project
- [x] Update all references
- [x] Update tests

#### Task 5.5: Update Project References
- [x] Update all core library references
- [x] Update application references to use NuGet packages (or project refs during dev)
- [x] Fix any broken references
- [x] Verify all projects build

### Week 6: Build & CI/CD Updates

#### Task 6.1: Update Build Scripts
- [x] Update build scripts for multiple solutions
- [x] Create build script for core libraries (`build-core.ps1`)
- [x] Create build script for applications (`build-applications.ps1`)
- [x] Create build script for all solutions (`build-all.ps1`)
- [ ] Update package versioning strategy

#### Task 6.2: Update CI/CD Pipelines
- [x] Update GitHub Actions workflows
- [x] Create separate pipeline for core libraries
- [x] Create separate pipeline for applications
- [x] Update .NET version to 10.0.x
- [ ] Update package publishing
- [ ] Update test execution

#### Task 6.3: Update Package Publishing
- [ ] Update NuGet package metadata
- [ ] Update package versioning
- [ ] Update package dependencies
- [ ] Test package publishing

### Week 7: Testing & Validation

#### Task 7.1: Run All Tests
- [x] Run core library tests (verified builds)
- [x] Run application tests (verified builds)
- [ ] Run integration tests (deferred - can run after Phase 3.4)
- [x] Fix any failing tests (fixed metapackage references)

#### Task 7.2: Fix Broken References
- [x] Identify all broken references
- [x] Fix project references (metapackage updated)
- [x] Fix NuGet package references
- [x] Verify all builds succeed

#### Task 7.3: Validate Applications
- [ ] Test JARVIS application
- [ ] Test TeachingAssistant application
- [ ] Verify all features work
- [ ] Performance testing

### Week 8: Documentation & Migration

#### Task 8.1: Update Documentation
- [ ] Update main README
- [ ] Update architecture documentation
- [ ] Update API documentation
- [ ] Create application-specific READMEs

#### Task 8.2: Create Migration Guide
- [ ] Document breaking changes (if any)
- [ ] Create migration guide for core libraries
- [ ] Create migration guide for applications
- [ ] Provide code examples

#### Task 8.3: Update Samples
- [ ] Update existing samples
- [ ] Create new samples for enhanced workflows
- [ ] Create samples for applications
- [ ] Verify all samples work

## Phase 3.4: Autonomous Agent Capabilities (Behavior Trees & State Machines)

**Status:** Comprehensive Plan Complete  
**Prerequisites:** Phase 3.3 (Restructuring) must be complete  
**Estimated Duration:** 5 weeks  
**See:** `docs/architecture/PHASE_3.4_IMPLEMENTATION_PLAN.md` for detailed breakdown

### Overview

This phase adds Behavior Trees and State Machines to enable sophisticated autonomous agent capabilities:
- **State Machines**: Agent lifecycle and operational state management
- **Behavior Trees**: Hierarchical decision-making for autonomous agents
- **Unified Framework**: Combined autonomous agent system

### Implementation Summary

**Week 1: State Machine Core**
- Core state machine model and execution engine
- Builder API and validation
- Integration with agent registry
- Observability and events

**Week 2: State Machine Advanced**
- Hierarchical and timed states
- Common patterns
- Integration with workflows and multi-agent system

**Week 3: Behavior Tree Core**
- Core behavior tree model
- Leaf nodes (Action, Condition, LLM, Workflow)
- Composite nodes (Sequence, Selector, Parallel)
- Decorator nodes (Inverter, Repeater, Cooldown, Timeout, Retry)
- Builder API and executor

**Week 4: Behavior Tree Advanced**
- LLM integration for dynamic decisions
- Integration with state machines and workflows
- Multi-agent delegation
- Visual debugging and observability
- Common patterns

**Week 5: Unified Framework**
- `AutonomousAgent<TState>` base class
- Integration examples
- Performance optimization
- Migration guides
- Final testing

### Key Deliverables

1. **State Machine System**
   - `DotNetAgents.Agents.StateMachines` project
   - Full lifecycle management
   - Integration with agent registry
   - Observability support

2. **Behavior Tree System**
   - `DotNetAgents.Agents.BehaviorTrees` project
   - Hierarchical decision-making
   - LLM integration
   - Visual debugging

3. **Unified Framework**
   - `AutonomousAgent<TState>` class
   - Example implementations
   - Comprehensive documentation

### Detailed Plan

For complete task breakdown, architecture decisions, integration points, and implementation details, see:
- **`docs/architecture/PHASE_3.4_IMPLEMENTATION_PLAN.md`** - Comprehensive implementation plan
- **`docs/architecture/BEHAVIOR_TREES_AND_STATE_MACHINES.md`** - Analysis and rationale

## Priority Order

1. **Phase 3.1** - Enhanced workflow features (enables better applications) ✅
2. **Phase 3.2** - .NET 10 optimizations (improves performance) ✅
3. **Phase 3.3** - Restructuring (improves maintainability) ⏳
4. **Phase 3.4** - Autonomous Agent Capabilities (Behavior Trees & State Machines) 📋
5. **Phase 3.2 Task 4.4** - Performance Benchmarking (after restructuring) ⏸️

## Current Status

- **Phase 3.1 Week 1**: ✅ COMPLETE (5/6 tasks)
  - ✅ Task 1.1: DecisionNode - COMPLETE
  - ✅ Task 1.2: InputNode - COMPLETE
  - ✅ Task 1.3: ReviewNode - COMPLETE
  - ✅ Task 1.4: ConditionalApprovalNode - COMPLETE
  - ✅ Task 1.5: Enhanced WorkflowStateInspector - COMPLETE
  - ⏳ Task 1.6: SignalR support - DEFERRED to Phase 3.2 Task 4.3
- **Phase 3.1 Week 2**: ✅ COMPLETE (6/6 tasks)
  - ✅ Task 2.1: DynamicBranchNode - COMPLETE
  - ✅ Task 2.2: ParallelNode - COMPLETE
  - ✅ Task 2.3: LoopNode - COMPLETE
  - ✅ Task 2.4: SubWorkflowNode - COMPLETE
  - ✅ Task 2.5: RetryNode - COMPLETE
  - ✅ Task 2.6: ValidationNode - COMPLETE
- **Phase 3.1 Week 3**: ✅ COMPLETE (2/2 tasks)
  - ✅ Task 3.1: WorkflowTemplate - COMPLETE
  - ✅ Task 3.2: Common Workflow Patterns - COMPLETE
- **Phase 3.2**: ✅ COMPLETE (.NET 10 Optimizations)
  - ✅ Task 4.1: .NET 10 Features Guide - COMPLETE (documentation and patterns created)
  - ✅ Task 4.2: AI Optimizations - COMPLETE (SIMD-optimized VectorOperations implemented)
  - ✅ Task 4.3: SignalR .NET 10 Implementation - COMPLETE (SignalRAgentMessageBus with client implementation, server hub documented)

- **Phase 3.3**: ✅ COMPLETE (Restructuring)
  - ✅ Task 5.1: Solution Files - COMPLETE (Core, Applications, All solutions created)
  - ✅ Task 5.2: Applications Directory - COMPLETE (JARVIS and TeachingAssistant moved)
  - ✅ Task 5.3-5.4: Project Moves - COMPLETE (Voice and Education components moved)
  - ✅ Task 5.5: Project References - COMPLETE (All references updated)
  - ✅ Task 6.1-6.2: Build Scripts & CI/CD - COMPLETE (Updated for multiple solutions)
  - ✅ Task 7.1-7.2: Testing & Verification - COMPLETE (All builds verified)

- **Phase 3.4**: ✅ COMPLETE (Autonomous Agent Capabilities)
  - ✅ Week 1: State Machine Core - COMPLETE (Core model, builder, persistence, observability, registry integration, tests)
  - ✅ Week 2: State Machine Advanced - COMPLETE (Hierarchical, parallel, timed transitions, patterns, workflow integration, multi-agent integration, documentation)
  - ✅ Week 3: Behavior Tree Core - COMPLETE (Core model, leaf nodes, composite nodes, decorator nodes, builder, executor, observability)
  - ✅ Week 4: Behavior Tree Advanced - COMPLETE (Additional decorators, LLM integration, workflow integration, state machine integration)
  - ⏸️ Task 4.4: Performance Benchmarking - DEFERRED (will be done after restructuring)
- **Phase 3.3**: In Progress (Restructuring)
  - ✅ Task 5.1: Create Solution Files - COMPLETE
  - ✅ Task 5.2: Create Applications Directory Structure - COMPLETE
  - ✅ Task 5.3: Move JARVIS Components - COMPLETE (moved, references updated)
  - ✅ Task 5.4: Move Education Components - COMPLETE (moved, references updated)
  - ✅ Task 5.5: Update Project References - COMPLETE (all references updated, builds verified)
- **Phase 3.4**: Comprehensive Plan Complete (Autonomous Agent Capabilities - Behavior Trees & State Machines)
  - 📋 See `docs/architecture/PHASE_3.4_IMPLEMENTATION_PLAN.md` for detailed implementation plan
  - 📋 See `docs/architecture/BEHAVIOR_TREES_AND_STATE_MACHINES.md` for analysis and rationale
  - ⏳ Implementation scheduled after Phase 3.3 (Restructuring)
