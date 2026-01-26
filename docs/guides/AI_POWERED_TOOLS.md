# AI-Powered Development Tools Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

DotNetAgents includes AI-powered tools that help developers create chains and workflows more efficiently using natural language.

## Available Tools

### 1. Chain Generator

Generates chain code from natural language descriptions.

#### Usage

```csharp
using DotNetAgents.Tools.Development;
using DotNetAgents.Providers.OpenAI;

var llm = new OpenAIModel(apiKey, modelName: "gpt-4");
var generator = new ChainGenerator(llm);

var result = await generator.GenerateAsync(
    "Create a chain that takes user input, searches a knowledge base, and generates a response using the retrieved context");

Console.WriteLine(result.GeneratedCode);
Console.WriteLine(result.Explanation);
```

#### Example Output

```csharp
// Generated chain code
var chain = ChainBuilder<string, string>.Create()
    .WithLLM(llmModel)
    .WithPromptTemplate("Answer the question using the following context: {context}\n\nQuestion: {question}")
    .WithMemory(memory)
    .Build();
```

### 2. Workflow Builder

Converts natural language to workflow definitions.

#### Usage

```csharp
using DotNetAgents.Tools.Development;

var builder = new WorkflowBuilder(llm);

var workflow = await builder.GenerateAsync(
    "Create a workflow that processes orders: validate order, check inventory, process payment, and send confirmation");

// Use the generated workflow
var designerService = serviceProvider.GetRequiredService<IWorkflowDesignerService>();
await designerService.SaveWorkflowAsync(workflow);
```

#### Example Output

```json
{
  "name": "order-processing",
  "nodes": [
    { "id": "validate", "type": "function", ... },
    { "id": "check-inventory", "type": "function", ... },
    { "id": "process-payment", "type": "function", ... },
    { "id": "send-confirmation", "type": "function", ... }
  ],
  "edges": [...],
  "entryPoint": "validate"
}
```

### 3. Debugging Assistant

Analyzes execution logs and suggests fixes.

#### Usage

```csharp
using DotNetAgents.Tools.Development;

var assistant = new DebuggingAssistant(llm);

var analysis = await assistant.AnalyzeAsync(
    executionLog: errorLog,
    workflowDefinition: workflowDefinition);

foreach (var issue in analysis.Issues)
{
    Console.WriteLine($"[{issue.Severity}] {issue.Message}");
    Console.WriteLine($"Location: {issue.Location}");
    Console.WriteLine($"Root Cause: {issue.RootCause}");
}

foreach (var suggestion in analysis.Suggestions)
{
    Console.WriteLine($"[{suggestion.Priority}] {suggestion.Description}");
    if (!string.IsNullOrEmpty(suggestion.Code))
    {
        Console.WriteLine($"Code: {suggestion.Code}");
    }
}
```

#### Optimization Suggestions

```csharp
var suggestions = await assistant.SuggestOptimizationsAsync(
    definition: workflowJson,
    performanceMetrics: new Dictionary<string, object>
    {
        ["avgExecutionTime"] = 5000,
        ["memoryUsage"] = 1024,
        ["costPerExecution"] = 0.05
    });

foreach (var suggestion in suggestions.Suggestions)
{
    Console.WriteLine($"[{suggestion.Type}] {suggestion.Description}");
    Console.WriteLine($"Impact: {suggestion.Impact}");
    Console.WriteLine($"Effort: {suggestion.Effort}");
}
```

## Best Practices

1. **Provide Clear Descriptions:** Be specific about requirements
2. **Review Generated Code:** Always review and test generated code
3. **Iterate:** Refine descriptions based on results
4. **Use Context:** Provide additional context when needed
5. **Validate:** Always validate generated workflows/chains

## Limitations

- Generated code may need manual refinement
- Complex requirements may need multiple iterations
- LLM quality depends on the model used
- Generated code should be tested thoroughly

## Related Documentation

- [Chain Guide](./CHAINS.md)
- [Workflow Guide](./WORKFLOW.md)
- [Visual Workflow Designer](./VISUAL_WORKFLOW_DESIGNER.md)
