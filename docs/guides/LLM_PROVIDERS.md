# LLM Provider Comparison and Usage Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

DotNetAgents supports 12 LLM providers, each registered as a plugin. This guide helps you choose the right provider and use it effectively.

## Table of Contents

1. [Available Providers](#available-providers)
2. [Provider Categories](#provider-categories)
3. [Cloud Providers](#cloud-providers)
4. [Local Providers](#local-providers)
5. [Comparison](#comparison)
6. [Switching Providers](#switching-providers)
7. [Best Practices](#best-practices)

## Available Providers

| Provider | Package | Type | Models |
|----------|---------|------|--------|
| **OpenAI** | `DotNetAgents.Providers.OpenAI` | Cloud | GPT-3.5, GPT-4, GPT-4 Turbo |
| **Azure OpenAI** | `DotNetAgents.Providers.Azure` | Cloud | GPT-3.5, GPT-4 (Azure-hosted) |
| **Anthropic** | `DotNetAgents.Providers.Anthropic` | Cloud | Claude 3 (Opus, Sonnet, Haiku) |
| **Google** | `DotNetAgents.Providers.Google` | Cloud | Gemini Pro, Gemini Ultra |
| **AWS Bedrock** | `DotNetAgents.Providers.AWS` | Cloud | Multiple (Claude, Llama, Titan) |
| **Cohere** | `DotNetAgents.Providers.Cohere` | Cloud | Command, Command-Light |
| **Groq** | `DotNetAgents.Providers.Groq` | Cloud | Llama 2, Mixtral (Fast inference) |
| **Mistral AI** | `DotNetAgents.Providers.Mistral` | Cloud | Mistral 7B, Mixtral 8x7B |
| **Together AI** | `DotNetAgents.Providers.Together` | Cloud | Multiple open models |
| **Ollama** | `DotNetAgents.Providers.Ollama` | Local | Any Ollama-compatible model |
| **LM Studio** | `DotNetAgents.Providers.LMStudio` | Local | Any local model |
| **vLLM** | `DotNetAgents.Providers.vLLM` | Local | High-performance local inference |

## Provider Categories

### Cloud Providers

Cloud providers offer managed LLM services with high availability and scalability.

**Advantages:**
- ✅ No infrastructure management
- ✅ High availability
- ✅ Automatic scaling
- ✅ Latest models

**Considerations:**
- ⚠️ Requires internet connection
- ⚠️ API costs
- ⚠️ Data privacy concerns
- ⚠️ Rate limits

### Local Providers

Local providers run models on your infrastructure.

**Advantages:**
- ✅ Complete data privacy
- ✅ No API costs
- ✅ No rate limits
- ✅ Offline operation

**Considerations:**
- ⚠️ Requires GPU resources
- ⚠️ Infrastructure management
- ⚠️ Model availability
- ⚠️ Setup complexity

## Cloud Providers

### OpenAI

**Best for:** General-purpose tasks, high-quality responses

```csharp
using DotNetAgents.Providers.OpenAI;

services.AddOpenAI(options =>
{
    options.ApiKey = configuration["OpenAI:ApiKey"];
    options.Model = "gpt-4-turbo-preview";
    options.Temperature = 0.7;
    options.MaxTokens = 2000;
});
```

**Models:**
- `gpt-4-turbo-preview` - Latest GPT-4 Turbo
- `gpt-4` - GPT-4
- `gpt-3.5-turbo` - Fast, cost-effective

### Azure OpenAI

**Best for:** Enterprise deployments, Azure integration

```csharp
using DotNetAgents.Providers.Azure;

services.AddAzureOpenAI(options =>
{
    options.Endpoint = "https://your-resource.openai.azure.com";
    options.ApiKey = configuration["Azure:ApiKey"];
    options.DeploymentName = "gpt-4";
    options.ApiVersion = "2024-02-15-preview";
});
```

**Advantages:**
- ✅ Enterprise SLA
- ✅ Azure integration
- ✅ Data residency options
- ✅ Private endpoints

### Anthropic Claude

**Best for:** Long context, safety-critical applications

```csharp
using DotNetAgents.Providers.Anthropic;

services.AddAnthropic(options =>
{
    options.ApiKey = configuration["Anthropic:ApiKey"];
    options.Model = "claude-3-opus-20240229";
    options.MaxTokens = 4096;
});
```

**Models:**
- `claude-3-opus-20240229` - Most capable
- `claude-3-sonnet-20240229` - Balanced
- `claude-3-haiku-20240307` - Fast, cost-effective

**Advantages:**
- ✅ Long context (200K tokens)
- ✅ Strong safety features
- ✅ Excellent reasoning

### Google Gemini

**Best for:** Multimodal tasks, Google ecosystem

```csharp
using DotNetAgents.Providers.Google;

services.AddGoogle(options =>
{
    options.ApiKey = configuration["Google:ApiKey"];
    options.Model = "gemini-pro";
    options.Temperature = 0.7;
});
```

**Models:**
- `gemini-pro` - Text generation
- `gemini-pro-vision` - Multimodal

**Advantages:**
- ✅ Multimodal support
- ✅ Competitive pricing
- ✅ Google Cloud integration

### AWS Bedrock

**Best for:** AWS infrastructure, multiple model access

```csharp
using DotNetAgents.Providers.AWS;

services.AddAWS(options =>
{
    options.Region = "us-east-1";
    options.AccessKey = configuration["AWS:AccessKey"];
    options.SecretKey = configuration["AWS:SecretKey"];
    options.ModelId = "anthropic.claude-v2";
});
```

**Available Models:**
- Anthropic Claude
- Meta Llama 2
- Amazon Titan
- AI21 Jurassic

**Advantages:**
- ✅ Multiple models in one service
- ✅ AWS integration
- ✅ Enterprise features

### Groq

**Best for:** Fast inference, real-time applications

```csharp
using DotNetAgents.Providers.Groq;

services.AddGroq(options =>
{
    options.ApiKey = configuration["Groq:ApiKey"];
    options.Model = "mixtral-8x7b-32768";
});
```

**Models:**
- `mixtral-8x7b-32768` - Fast Mixtral
- `llama2-70b-4096` - Fast Llama 2

**Advantages:**
- ✅ Extremely fast inference
- ✅ Low latency
- ✅ Cost-effective

### Cohere

**Best for:** Enterprise, multilingual

```csharp
using DotNetAgents.Providers.Cohere;

services.AddCohere(options =>
{
    options.ApiKey = configuration["Cohere:ApiKey"];
    options.Model = "command";
});
```

**Models:**
- `command` - High quality
- `command-light` - Fast, cost-effective

### Mistral AI

**Best for:** European data residency, open models

```csharp
using DotNetAgents.Providers.Mistral;

services.AddMistral(options =>
{
    options.ApiKey = configuration["Mistral:ApiKey"];
    options.Model = "mistral-large-latest";
});
```

**Models:**
- `mistral-large-latest` - Most capable
- `mistral-medium-latest` - Balanced
- `mistral-small-latest` - Fast

### Together AI

**Best for:** Open models, research

```csharp
using DotNetAgents.Providers.Together;

services.AddTogether(options =>
{
    options.ApiKey = configuration["Together:ApiKey"];
    options.Model = "meta-llama/Llama-2-70b-chat-hf";
});
```

**Advantages:**
- ✅ Access to open models
- ✅ Research-friendly
- ✅ Competitive pricing

## Local Providers

### Ollama

**Best for:** Local development, privacy-sensitive tasks

```csharp
using DotNetAgents.Providers.Ollama;

services.AddOllama(options =>
{
    options.BaseUrl = "http://localhost:11434";
    options.Model = "llama2";
    options.Timeout = TimeSpan.FromMinutes(5);
});
```

**Setup:**
```bash
# Install Ollama
curl -fsSL https://ollama.ai/install.sh | sh

# Pull a model
ollama pull llama2
```

**Advantages:**
- ✅ Completely local
- ✅ No API costs
- ✅ Easy setup
- ✅ Multiple models

### LM Studio

**Best for:** Local development, model experimentation

```csharp
using DotNetAgents.Providers.LMStudio;

services.AddLMStudio(options =>
{
    options.BaseUrl = "http://localhost:1234";
    options.Model = "local-model";
});
```

**Advantages:**
- ✅ User-friendly interface
- ✅ Easy model switching
- ✅ Local execution

### vLLM

**Best for:** High-performance local inference

```csharp
using DotNetAgents.Providers.vLLM;

services.AddVLLM(options =>
{
    options.BaseUrl = "http://localhost:8000";
    options.Model = "meta-llama/Llama-2-7b-chat-hf";
    options.TensorParallelSize = 1;
});
```

**Advantages:**
- ✅ High throughput
- ✅ Efficient GPU usage
- ✅ Production-ready

**Requirements:**
- GPU with CUDA support
- Docker or native installation

## Comparison

### Feature Matrix

| Provider | Speed | Quality | Cost | Context | Best For |
|----------|-------|---------|------|---------|----------|
| **OpenAI** | Fast | Excellent | High | 128K | General purpose |
| **Azure OpenAI** | Fast | Excellent | High | 128K | Enterprise |
| **Anthropic** | Medium | Excellent | High | 200K | Long context, safety |
| **Google** | Fast | Very Good | Medium | 32K | Multimodal |
| **AWS Bedrock** | Medium | Very Good | Medium | Varies | AWS ecosystem |
| **Groq** | Very Fast | Good | Low | 32K | Real-time |
| **Cohere** | Fast | Very Good | Medium | 4K | Enterprise |
| **Mistral** | Fast | Very Good | Medium | 32K | European |
| **Together** | Medium | Good | Low | Varies | Open models |
| **Ollama** | Slow | Good | Free | Varies | Local dev |
| **LM Studio** | Slow | Good | Free | Varies | Local dev |
| **vLLM** | Fast | Good | Free* | Varies | Local production |

*Free after initial infrastructure cost

### Cost Comparison (Approximate)

| Provider | Cost per 1M tokens (Input) | Cost per 1M tokens (Output) |
|----------|----------------------------|------------------------------|
| **OpenAI GPT-4** | $30 | $60 |
| **OpenAI GPT-3.5** | $0.50 | $1.50 |
| **Anthropic Claude** | $15 | $75 |
| **Google Gemini** | $0.25 | $0.50 |
| **Groq** | $0.27 | $0.27 |
| **Mistral** | $2.00 | $6.00 |
| **Local** | Infrastructure only | Infrastructure only |

## Switching Providers

### Provider Abstraction

All providers implement `ILLMModel<TInput, TOutput>`, making switching easy:

```csharp
// Switch from OpenAI to Anthropic
// Just change registration:
services.AddAnthropic(options => { /* ... */ });

// Code using ILLMModel remains unchanged
var llm = serviceProvider.GetRequiredService<ILLMModel<ChatMessage[], ChatMessage>>();
var response = await llm.GenerateAsync(messages);
```

### Multi-Provider Setup

```csharp
// Register multiple providers
services.AddOpenAI(options => { options.Model = "gpt-4"; });
services.AddAnthropic(options => { options.Model = "claude-3-opus"; });

// Use provider factory
var openAI = serviceProvider.GetRequiredService<ILLMModelFactory>()
    .CreateModel<ChatMessage[], ChatMessage>("openai");
var claude = serviceProvider.GetRequiredService<ILLMModelFactory>()
    .CreateModel<ChatMessage[], ChatMessage>("anthropic");
```

## Best Practices

### 1. Error Handling

```csharp
try
{
    var response = await llm.GenerateAsync(messages);
}
catch (LLMException ex)
{
    _logger.LogError(ex, "LLM call failed");
    // Retry or fallback
}
```

### 2. Rate Limiting

```csharp
// Use retry policies for rate limits
services.AddOpenAI(options =>
{
    options.ApiKey = apiKey;
    options.RetryPolicy = new RetryPolicy
    {
        MaxRetries = 3,
        BackoffType = BackoffType.Exponential
    };
});
```

### 3. Cost Management

```csharp
// Track token usage
var response = await llm.GenerateAsync(messages);
var usage = response.Usage;

Console.WriteLine($"Input tokens: {usage.InputTokens}");
Console.WriteLine($"Output tokens: {usage.OutputTokens}");
Console.WriteLine($"Total cost: ${usage.EstimatedCost}");
```

### 4. Model Selection

```csharp
// Use appropriate model for task
var simpleTask = await llm.GenerateAsync(messages, model: "gpt-3.5-turbo");
var complexTask = await llm.GenerateAsync(messages, model: "gpt-4");
```

### 5. Configuration Management

```csharp
// Use configuration for provider settings
services.AddOpenAI(configuration.GetSection("OpenAI"));

// appsettings.json
{
  "OpenAI": {
    "ApiKey": "your-key",
    "Model": "gpt-4-turbo-preview",
    "Temperature": 0.7,
    "MaxTokens": 2000
  }
}
```

## Related Documentation

- [Provider Interfaces](../../src/DotNetAgents.Abstractions/LLM/ILLMModel.cs)
- [Plugin Architecture](../PLUGIN_ARCHITECTURE_MIGRATION.md)
- [Error Handling](./ERROR_HANDLING.md)
