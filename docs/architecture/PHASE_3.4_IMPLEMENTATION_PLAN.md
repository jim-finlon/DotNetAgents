# Phase 3.4: Behavior Trees & State Machines - Comprehensive Implementation Plan

**Created:** January 2025  
**Status:** ✅ COMPLETE  
**Completed:** January 2025  
**Prerequisites:** Phase 3.3 (Restructuring) - ✅ Complete  
**Actual Duration:** ~1 week

> **Note:** This document is kept for historical reference. All planned features have been implemented. See [State Machines README](../../src/DotNetAgents.Agents.StateMachines/README.md) and [Behavior Trees README](../../src/DotNetAgents.Agents.BehaviorTrees/README.md) for usage documentation.

## Overview

This document provides a comprehensive implementation plan for integrating Behavior Trees and State Machines into the DotNetAgents framework. These patterns will enable sophisticated autonomous agent capabilities while complementing the existing workflow system.

## Architecture Goals

1. **State Machines**: Manage agent lifecycle and operational states
2. **Behavior Trees**: Enable hierarchical, reactive decision-making for autonomous agents
3. **Integration**: Seamless integration with existing workflows, agent registry, and observability
4. **Composability**: Both systems should be composable and reusable
5. **Observability**: Full tracing and metrics support

## Part 1: State Machines Implementation

### Week 1: Core State Machine Infrastructure

#### Task 1.1: Project Setup
- [ ] Create `DotNetAgents.Agents.StateMachines` project
- [ ] Add project references:
  - `DotNetAgents.Abstractions`
  - `DotNetAgents.Agents.Registry`
  - `DotNetAgents.Observability` (for tracing)
- [ ] Configure .NET 10 target framework
- [ ] Add unit test project `DotNetAgents.Agents.StateMachines.Tests`

#### Task 1.2: Core State Machine Model
- [ ] Create `IStateMachine<TState>` interface
  - `CurrentState` property
  - `CanTransition(string fromState, string toState, TState context)` method
  - `TransitionAsync(string toState, TState context, CancellationToken ct)` method
  - `GetAvailableTransitions(TState context)` method
- [ ] Create `StateTransition<TState>` record
  - `FromState`, `ToState`, `Guard`, `OnTransition` action
- [ ] Create `StateDefinition<TState>` class
  - `Name`, `EntryAction`, `ExitAction`, `OnEntry`, `OnExit` events
- [ ] Create `AgentStateMachine<TState>` class (main implementation)
  - State registry
  - Transition registry
  - Current state tracking
  - Thread-safe operations

#### Task 1.3: State Machine Builder API
- [ ] Create `StateMachineBuilder<TState>` fluent API
  - `AddState(string name, Action<TState>? entryAction, Action<TState>? exitAction)`
  - `AddTransition(string from, string to, Func<TState, bool>? guard, Action<TState>? onTransition)`
  - `SetInitialState(string stateName)`
  - `Build()` method
- [ ] Add validation:
  - All transitions reference valid states
  - Initial state exists
  - No orphaned states (unless explicitly allowed)
  - Guard functions are not null (or provide default true)

#### Task 1.4: State Machine Execution Engine
- [ ] Implement state transition logic
  - Validate transition is allowed
  - Execute exit action of current state
  - Execute transition action
  - Execute entry action of new state
  - Update current state
- [ ] Add state persistence support
  - `IStateMachinePersistence<TState>` interface
  - `SaveStateAsync(string machineId, string state, TState context)`
  - `LoadStateAsync(string machineId)`
- [ ] Add state history tracking
  - Track state transitions with timestamps
  - Store transition history (configurable depth)

#### Task 1.5: Integration with Agent Registry
- [ ] Extend `AgentInfo` to include state machine state
- [ ] Create `StateMachineAgentInfo` extension
- [ ] Update `AgentRegistry` to track state machine states
- [ ] Add state machine state to `AgentStatus` enum (if needed) or extend with state machine context
- [ ] Create `AgentStateMachineRegistry` for managing multiple state machines

#### Task 1.6: Observability & Events
- [ ] Add state transition events
  - `StateTransitioned` event with `(string fromState, string toState, TState context, DateTime timestamp)`
- [ ] Integrate with OpenTelemetry
  - Create `StateMachineActivitySource`
  - Trace state transitions
  - Add metrics: `state_transitions_total`, `state_duration_seconds`
- [ ] Add logging for state transitions
- [ ] Create `IStateMachineObserver<TState>` interface for custom observers

#### Task 1.7: Unit Tests
- [ ] Test state machine creation and validation
- [ ] Test state transitions (valid and invalid)
- [ ] Test guard conditions
- [ ] Test entry/exit actions
- [ ] Test state persistence
- [ ] Test concurrent state transitions
- [ ] Test state history tracking

### Week 2: State Machine Advanced Features & Integration

#### Task 2.1: Hierarchical State Machines
- [ ] Implement nested states (sub-states)
- [ ] Add state machine composition
- [ ] Support parallel states (orthogonal regions)
- [ ] Add state machine inheritance

#### Task 2.2: Timed States & Transitions
- [ ] Add timeout transitions
  - `AddTimeoutTransition(string state, TimeSpan timeout, string targetState)`
- [ ] Add state duration tracking
- [ ] Add scheduled transitions
  - `AddScheduledTransition(string state, DateTime scheduledTime, string targetState)`

#### Task 2.3: State Machine Patterns
- [ ] Create common state machine patterns:
  - `IdleWorkingPattern`: Idle → Working → Idle
  - `ErrorRecoveryPattern`: Any → Error → Recovery → Idle
  - `WorkflowStatePattern`: Uninitialized → Running → Completed/Failed
- [ ] Add pattern builder helpers

#### Task 2.4: Integration with Workflows
- [ ] Create `StateMachineWorkflowNode<TState>` that wraps a state machine
- [ ] Allow workflows to trigger state transitions
- [ ] Allow state machines to gate workflow execution
- [ ] Create example: Workflow that respects agent state machine

#### Task 2.5: Integration with Multi-Agent System
- [ ] State machines for worker pool agents
  - Available → Busy → CoolingDown → Available
- [ ] State machines for supervisor agents
  - Monitoring → Analyzing → Delegating → Waiting
- [ ] State-based agent selection in `IWorkerPool`
- [ ] State transition triggers via message bus

#### Task 2.6: Documentation & Examples
- [ ] Create comprehensive API documentation
- [ ] Create usage examples:
  - Basic state machine
  - State machine with guards
  - State machine with persistence
  - State machine integrated with agent registry
- [ ] Create migration guide from `AgentStatus` enum to state machines
- [ ] Add to main README

## Part 2: Behavior Trees Implementation

### Week 3: Core Behavior Tree Infrastructure

#### Task 3.1: Project Setup
- [ ] Create `DotNetAgents.Agents.BehaviorTrees` project
- [ ] Add project references:
  - `DotNetAgents.Abstractions`
  - `DotNetAgents.Agents.Registry`
  - `DotNetAgents.Agents.StateMachines` (for integration)
  - `DotNetAgents.Workflow` (for workflow integration)
  - `DotNetAgents.Observability`
- [ ] Configure .NET 10 target framework
- [ ] Add unit test project `DotNetAgents.Agents.BehaviorTrees.Tests`

#### Task 3.2: Core Behavior Tree Model
- [ ] Create `BehaviorTreeNodeStatus` enum
  - `Success`, `Failure`, `Running`
- [ ] Create `IBehaviorTreeNode<TContext>` interface
  - `ExecuteAsync(TContext context, CancellationToken ct)` returns `Task<BehaviorTreeNodeStatus>`
  - `Name` property
  - `Reset()` method (for stateful nodes)
- [ ] Create `BehaviorTreeNodeResult<TContext>` record
  - `Status`, `Context`, `Message`, `Data`
- [ ] Create base `BehaviorTreeNode<TContext>` abstract class
  - Common functionality
  - Logging support
  - Observability hooks

#### Task 3.3: Leaf Nodes
- [ ] Create `ActionNode<TContext>` class
  - Executes an action function
  - Returns Success/Failure based on action result
  - Supports async actions
- [ ] Create `ConditionNode<TContext>` class
  - Evaluates a condition function
  - Returns Success if true, Failure if false
  - Supports async conditions
- [ ] Create `LLMActionNode<TContext>` class
  - Integrates with `AgentExecutor` for LLM-based actions
  - Uses LLM to decide action to take
  - Supports tool calling
- [ ] Create `WorkflowActionNode<TContext>` class
  - Executes a workflow as an action
  - Maps context to workflow state
  - Maps workflow result back to context

#### Task 3.4: Composite Nodes
- [ ] Create `SequenceNode<TContext>` class
  - Executes children in order
  - Returns Success if all succeed
  - Returns Failure on first failure
  - Supports short-circuiting
- [ ] Create `SelectorNode<TContext>` class (Fallback)
  - Executes children until one succeeds
  - Returns Success on first success
  - Returns Failure if all fail
- [ ] Create `ParallelNode<TContext>` class
  - Executes children concurrently
  - Supports success policies: All, Any, Majority, Count
  - Handles cancellation
- [ ] Create `RandomSelectorNode<TContext>` class
  - Randomly selects a child to execute
  - Useful for exploration

#### Task 3.5: Decorator Nodes
- [ ] Create `InverterNode<TContext>` class
  - Inverts Success/Failure of child
- [ ] Create `RepeaterNode<TContext>` class
  - Repeats child N times or until failure
  - Supports infinite repeat
- [ ] Create `CooldownNode<TContext>` class
  - Rate limits child execution
  - Tracks last execution time
- [ ] Create `TimeoutNode<TContext>` class
  - Adds timeout to child execution
  - Returns Failure if timeout exceeded
- [ ] Create `RetryNode<TContext>` class
  - Retries child on failure
  - Supports exponential backoff
- [ ] Create `ConditionalDecoratorNode<TContext>` class
  - Only executes child if condition is met
  - Otherwise returns Success/Failure based on configuration

#### Task 3.6: Behavior Tree Builder API
- [ ] Create `BehaviorTreeBuilder<TContext>` fluent API
  - `AddAction(string name, Func<TContext, CancellationToken, Task<bool>> action)`
  - `AddCondition(string name, Func<TContext, CancellationToken, Task<bool>> condition)`
  - `AddSequence(string name, Action<BehaviorTreeBuilder<TContext>> configure)`
  - `AddSelector(string name, Action<BehaviorTreeBuilder<TContext>> configure)`
  - `AddParallel(string name, ParallelSuccessPolicy policy, Action<BehaviorTreeBuilder<TContext>> configure)`
  - `AddDecorator(string name, IBehaviorTreeNode<TContext> child, Action<IDecoratorBuilder<TContext>> configure)`
  - `Build()` method
- [ ] Add validation:
  - Tree has at least one node
  - All node names are unique
  - No circular references
  - All composite nodes have children

#### Task 3.7: Behavior Tree Executor
- [ ] Create `BehaviorTreeExecutor<TContext>` class
  - `ExecuteAsync(TContext context, CancellationToken ct)` method
  - Supports tick-based execution (for game-like scenarios)
  - Supports one-shot execution (for workflow-like scenarios)
- [ ] Implement execution modes:
  - `ExecuteOnce`: Execute tree once, return final result
  - `ExecuteUntilSuccess`: Keep executing until Success
  - `ExecuteUntilFailure`: Keep executing until Failure
  - `Tick`: Execute one tick (for running nodes)
- [ ] Add execution state tracking
  - Track which nodes are currently running
  - Support interruption and resumption
- [ ] Add blackboard support (shared state)
  - `IBlackboard` interface
  - `BlackboardNode<TContext>` for accessing blackboard

#### Task 3.8: Unit Tests
- [ ] Test leaf nodes (Action, Condition)
- [ ] Test composite nodes (Sequence, Selector, Parallel)
- [ ] Test decorator nodes (Inverter, Repeater, Cooldown, Timeout)
- [ ] Test behavior tree builder
- [ ] Test executor (all execution modes)
- [ ] Test blackboard integration
- [ ] Test cancellation and interruption

### Week 4: Advanced Features & Integration

#### Task 4.1: LLM Integration
- [ ] Create `LLMSelectorNode<TContext>` class
  - Uses LLM to select which child to execute
  - Provides context to LLM for decision-making
  - Supports tool calling for information gathering
- [ ] Create `LLMConditionNode<TContext>` class
  - Uses LLM to evaluate conditions
  - Natural language condition evaluation
- [ ] Create `LLMPlanNode<TContext>` class
  - Uses LLM to generate a plan (sub-tree)
  - Dynamically creates behavior tree structure
  - Integrates with `WorkflowTemplate` for plan execution

#### Task 4.2: Integration with State Machines
- [ ] Create `StateMachineNode<TContext>` class
  - Executes state machine transitions as BT actions
  - Checks state machine state as BT conditions
- [ ] Create `StateConditionNode<TContext>` class
  - Checks if state machine is in specific state
- [ ] Create `StateTransitionNode<TContext>` class
  - Triggers state machine transition
- [ ] Create example: BT that manages agent lifecycle via state machine

#### Task 4.3: Integration with Workflows
- [ ] Create `WorkflowNode<TContext>` class
  - Executes workflow as BT action
  - Maps context to workflow state
  - Maps workflow result to context
- [ ] Create `WorkflowConditionNode<TContext>` class
  - Checks workflow execution status
- [ ] Create example: BT that orchestrates multiple workflows

#### Task 4.4: Integration with Multi-Agent System
- [ ] Create `DelegateToAgentNode<TContext>` class
  - Delegates action to another agent
  - Waits for agent response
  - Handles agent unavailability
- [ ] Create `BroadcastToAgentsNode<TContext>` class
  - Broadcasts request to multiple agents
  - Collects responses
  - Applies success policy (All, Any, Majority)
- [ ] Create `AgentSelectionNode<TContext>` class
  - Selects agent based on capabilities/state
  - Uses `IWorkerPool` for selection

#### Task 4.5: Visual Debugging & Observability
- [ ] Create behavior tree visualizer
  - JSON export of tree structure
  - Graphviz/DOT format export
  - HTML visualization
- [ ] Add execution tracing
  - Track which nodes executed
  - Track execution results
  - Track execution time
- [ ] Integrate with OpenTelemetry
  - Create `BehaviorTreeActivitySource`
  - Trace node executions
  - Add metrics: `bt_node_executions_total`, `bt_execution_duration_seconds`
- [ ] Create `BehaviorTreeInspector` class
  - Inspect current execution state
  - View node statuses
  - View blackboard contents

#### Task 4.6: Behavior Tree Patterns
- [ ] Create common behavior tree patterns:
  - `ReactivePattern`: React to events, interrupt current behavior
  - `GoalOrientedPattern`: Pursue goal until achieved or impossible
  - `ExplorationPattern`: Explore options, select best
  - `MonitoringPattern`: Monitor conditions, react when changed
- [ ] Add pattern builder helpers

#### Task 4.7: Documentation & Examples
- [ ] Create comprehensive API documentation
- [ ] Create usage examples:
  - Basic behavior tree
  - Behavior tree with LLM integration
  - Behavior tree integrated with state machine
  - Behavior tree orchestrating workflows
  - Autonomous research agent example
  - Self-monitoring agent example
- [ ] Create visual diagrams of example trees
- [ ] Add to main README

## Part 3: Integration & Unified Autonomous Agent Framework

### Week 5: Unified Framework & Examples

#### Task 5.1: Autonomous Agent Base Class
- [ ] Create `AutonomousAgent<TState>` class
  - Combines state machine (lifecycle) and behavior tree (decision-making)
  - Manages agent execution loop
  - Handles state transitions and behavior execution
- [ ] Create `AutonomousAgentBuilder<TState>` fluent API
  - `WithStateMachine(Action<StateMachineBuilder<TState>> configure)`
  - `WithBehaviorTree(Action<BehaviorTreeBuilder<TState>> configure)`
  - `WithBlackboard(IBlackboard blackboard)`
  - `Build()` method

#### Task 5.2: Integration Examples
- [ ] Create `AutonomousResearchAgent` example
  - State machine: Idle → Researching → Synthesizing → Idle
  - Behavior tree: Check if answered → Research → Synthesize → Ask for help
- [ ] Create `SelfMonitoringAgent` example
  - State machine: Healthy → Unhealthy → Recovering → Healthy
  - Behavior tree: Monitor health → Recover if needed → Process tasks
- [ ] Create `MultiModalAgent` example
  - State machine: Idle → Processing → Waiting → Idle
  - Behavior tree: Route by urgency → Handle immediately → Delegate → Process normally

#### Task 5.3: Performance Optimization
- [ ] Optimize behavior tree execution
  - Cache condition evaluations
  - Skip unnecessary node evaluations
  - Parallel execution where possible
- [ ] Optimize state machine transitions
  - Cache guard evaluations
  - Batch state updates
- [ ] Add performance benchmarks
- [ ] Document performance characteristics

#### Task 5.4: Migration Guide
- [ ] Document migration from `AgentStatus` enum to state machines
- [ ] Document migration from simple agent logic to behavior trees
- [ ] Provide code examples for common migration scenarios
- [ ] Document when to use workflows vs. behavior trees vs. state machines

#### Task 5.5: Final Integration Testing
- [ ] Integration tests for state machines + behavior trees
- [ ] Integration tests with workflows
- [ ] Integration tests with multi-agent system
- [ ] End-to-end autonomous agent tests
- [ ] Performance tests

## Architecture Decisions

### State Machines

1. **State Representation**: States are strings (flexible, easy to serialize)
2. **Context Type**: Generic `TState` for type safety
3. **Thread Safety**: All operations are thread-safe
4. **Persistence**: Optional, via `IStateMachinePersistence` interface
5. **Events**: Use .NET events for state transitions

### Behavior Trees

1. **Context Type**: Generic `TContext` for flexibility
2. **Node Status**: Three-state (Success, Failure, Running)
3. **Execution Model**: Both tick-based and one-shot supported
4. **Blackboard**: Optional shared state via `IBlackboard`
5. **Composability**: All nodes implement same interface

### Integration Points

1. **With Workflows**: Behavior trees can contain workflow nodes, workflows can trigger state transitions
2. **With Agent Registry**: State machines track agent operational state
3. **With Multi-Agent**: Behavior trees can delegate to other agents
4. **With Observability**: Full tracing and metrics for both systems

## Success Criteria

1. ✅ State machines manage agent lifecycle effectively
2. ✅ Behavior trees enable sophisticated autonomous decision-making
3. ✅ Both systems integrate seamlessly with existing framework
4. ✅ Full observability and debugging support
5. ✅ Comprehensive documentation and examples
6. ✅ Performance meets requirements (to be benchmarked)

## Dependencies

- Phase 3.3 (Restructuring) must be complete
- Existing systems:
  - `DotNetAgents.Workflow` (for workflow integration)
  - `DotNetAgents.Agents.Registry` (for agent tracking)
  - `DotNetAgents.Observability` (for tracing/metrics)
  - `DotNetAgents.Core` (for LLM integration)

## Risk Mitigation

1. **Complexity**: Start with simple implementations, add complexity gradually
2. **Performance**: Benchmark early and often
3. **Integration**: Create integration tests early
4. **Documentation**: Document as we build, not after

## Timeline Summary

- **Week 1**: State Machine Core
- **Week 2**: State Machine Advanced & Integration
- **Week 3**: Behavior Tree Core
- **Week 4**: Behavior Tree Advanced & Integration
- **Week 5**: Unified Framework & Examples

**Total: 5 weeks** (can be adjusted based on priorities)
