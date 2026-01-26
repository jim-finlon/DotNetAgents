# Message Bus Implementations Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Active

## Overview

DotNetAgents supports multiple message bus implementations for agent-to-agent communication, enabling distributed multi-agent systems. All message bus implementations are registered as plugins.

## Table of Contents

1. [Available Implementations](#available-implementations)
2. [In-Memory Message Bus](#in-memory-message-bus)
3. [Kafka Message Bus](#kafka-message-bus)
4. [RabbitMQ Message Bus](#rabbitmq-message-bus)
5. [Redis Pub/Sub Message Bus](#redis-pubsub-message-bus)
6. [SignalR Message Bus](#signalr-message-bus)
7. [Comparison](#comparison)
8. [Best Practices](#best-practices)

## Available Implementations

| Implementation | Package | Use Case | Scalability |
|---------------|---------|----------|-------------|
| In-Memory | `DotNetAgents.Agents.Messaging` | Development, Testing | Single Process |
| Kafka | `DotNetAgents.Agents.Messaging.Kafka` | High-throughput, Distributed | Excellent |
| RabbitMQ | `DotNetAgents.Agents.Messaging.RabbitMQ` | Reliable, Complex Routing | Very Good |
| Redis | `DotNetAgents.Agents.Messaging.Redis` | Fast, Simple Pub/Sub | Good |
| SignalR | `DotNetAgents.Agents.Messaging.SignalR` | Real-time Web, Browser | Good |

## In-Memory Message Bus

### Overview

The in-memory message bus is the default implementation, suitable for single-process applications and testing.

### Registration

```csharp
using DotNetAgents.Agents.Messaging;

services.AddInMemoryAgentMessageBus();
```

### Usage

```csharp
var messageBus = serviceProvider.GetRequiredService<IAgentMessageBus>();

// Subscribe to messages
await messageBus.SubscribeAsync("agent-1", async (message) =>
{
    Console.WriteLine($"Received: {message.Content}");
    return Task.CompletedTask;
});

// Publish message
await messageBus.PublishAsync(new AgentMessage
{
    FromAgentId = "agent-2",
    ToAgentId = "agent-1",
    Content = "Hello from agent-2",
    MessageType = "greeting"
});
```

## Kafka Message Bus

### Overview

Kafka provides high-throughput, distributed messaging with excellent scalability.

### Prerequisites

- Apache Kafka cluster
- `Confluent.Kafka` NuGet package (automatically included)

### Registration

```csharp
using DotNetAgents.Agents.Messaging.Kafka;

services.AddKafkaMessageBus(options =>
{
    options.BootstrapServers = "localhost:9092";
    options.GroupId = "dotnet-agents-group";
    options.TopicPrefix = "agents";
    options.EnableAutoCommit = true;
    options.AutoCommitIntervalMs = 5000;
});
```

### Configuration Options

```csharp
var kafkaOptions = new KafkaMessageBusOptions
{
    BootstrapServers = "localhost:9092",
    GroupId = "dotnet-agents",
    TopicPrefix = "agents",
    EnableAutoCommit = true,
    AutoCommitIntervalMs = 5000,
    SessionTimeoutMs = 30000,
    MaxPollIntervalMs = 300000,
    EnablePartitionEof = true,
    StatisticsIntervalMs = 0,
    LogLevel = 0
};
```

### Usage

```csharp
var messageBus = serviceProvider.GetRequiredService<IAgentMessageBus>();

// Subscribe
await messageBus.SubscribeAsync("agent-1", async (message) =>
{
    Console.WriteLine($"Kafka message: {message.Content}");
    return Task.CompletedTask;
});

// Publish
await messageBus.PublishAsync(new AgentMessage
{
    FromAgentId = "agent-2",
    ToAgentId = "agent-1",
    Content = "Message via Kafka",
    MessageType = "data"
});
```

### Production Configuration

```csharp
services.AddKafkaMessageBus(options =>
{
    options.BootstrapServers = configuration["Kafka:BootstrapServers"];
    options.GroupId = configuration["Kafka:GroupId"];
    options.TopicPrefix = "agents";
    options.EnableAutoCommit = false; // Manual commit for reliability
    options.SessionTimeoutMs = 30000;
    options.MaxPollIntervalMs = 300000;
    
    // Security (if enabled)
    options.SecurityProtocol = SecurityProtocol.SaslSsl;
    options.SaslMechanism = SaslMechanism.Plain;
    options.SaslUsername = configuration["Kafka:Username"];
    options.SaslPassword = configuration["Kafka:Password"];
});
```

## RabbitMQ Message Bus

### Overview

RabbitMQ provides reliable messaging with advanced routing capabilities.

### Prerequisites

- RabbitMQ server
- `RabbitMQ.Client` NuGet package (automatically included)

### Registration

```csharp
using DotNetAgents.Agents.Messaging.RabbitMQ;

services.AddRabbitMQMessageBus(options =>
{
    options.HostName = "localhost";
    options.Port = 5672;
    options.UserName = "guest";
    options.Password = "guest";
    options.VirtualHost = "/";
    options.ExchangeName = "agents";
    options.QueuePrefix = "agent";
});
```

### Configuration Options

```csharp
var rabbitOptions = new RabbitMQMessageBusOptions
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/",
    ExchangeName = "agents",
    QueuePrefix = "agent",
    Durable = true,
    AutoDelete = false,
    PrefetchCount = 10
};
```

### Usage

```csharp
var messageBus = serviceProvider.GetRequiredService<IAgentMessageBus>();

// Subscribe with routing key
await messageBus.SubscribeAsync("agent-1", async (message) =>
{
    Console.WriteLine($"RabbitMQ message: {message.Content}");
    return Task.CompletedTask;
}, routingKey: "agent-1");

// Publish with routing
await messageBus.PublishAsync(new AgentMessage
{
    FromAgentId = "agent-2",
    ToAgentId = "agent-1",
    Content = "Message via RabbitMQ",
    MessageType = "task"
}, routingKey: "agent-1");
```

### Advanced Routing

```csharp
// Topic-based routing
await messageBus.PublishAsync(message, routingKey: "agent.task.high-priority");

// Fanout (broadcast to all)
await messageBus.PublishAsync(message, routingKey: "broadcast");
```

## Redis Pub/Sub Message Bus

### Overview

Redis provides fast, simple pub/sub messaging with low latency.

### Prerequisites

- Redis server
- `StackExchange.Redis` NuGet package (automatically included)

### Registration

```csharp
using DotNetAgents.Agents.Messaging.Redis;

services.AddRedisMessageBus(options =>
{
    options.ConnectionString = "localhost:6379";
    options.ChannelPrefix = "agents";
    options.RetryCount = 3;
    options.RetryDelayMs = 1000;
});
```

### Configuration Options

```csharp
var redisOptions = new RedisMessageBusOptions
{
    ConnectionString = "localhost:6379",
    ChannelPrefix = "agents",
    RetryCount = 3,
    RetryDelayMs = 1000,
    ConnectTimeout = 5000,
    SyncTimeout = 5000
};
```

### Usage

```csharp
var messageBus = serviceProvider.GetRequiredService<IAgentMessageBus>();

// Subscribe to channel
await messageBus.SubscribeAsync("agent-1", async (message) =>
{
    Console.WriteLine($"Redis message: {message.Content}");
    return Task.CompletedTask;
});

// Publish to channel
await messageBus.PublishAsync(new AgentMessage
{
    FromAgentId = "agent-2",
    ToAgentId = "agent-1",
    Content = "Message via Redis",
    MessageType = "notification"
});
```

### Redis Cluster

```csharp
services.AddRedisMessageBus(options =>
{
    options.ConnectionString = "redis-cluster:6379,redis-cluster:6380";
    options.ChannelPrefix = "agents";
    options.AbortOnConnectFail = false;
});
```

## SignalR Message Bus

### Overview

SignalR provides real-time messaging for web applications and browser-based agents.

### Prerequisites

- ASP.NET Core SignalR
- `Microsoft.AspNetCore.SignalR` NuGet package (automatically included)

### Registration

```csharp
using DotNetAgents.Agents.Messaging.SignalR;

// In Startup.cs or Program.cs
services.AddSignalR();
services.AddSignalRMessageBus(options =>
{
    options.HubPath = "/agents/hub";
    options.EnableDetailedErrors = true;
});
```

### Server-Side Usage

```csharp
var messageBus = serviceProvider.GetRequiredService<IAgentMessageBus>();

// Subscribe
await messageBus.SubscribeAsync("agent-1", async (message) =>
{
    Console.WriteLine($"SignalR message: {message.Content}");
    return Task.CompletedTask;
});

// Publish
await messageBus.PublishAsync(new AgentMessage
{
    FromAgentId = "agent-2",
    ToAgentId = "agent-1",
    Content = "Message via SignalR",
    MessageType = "update"
});
```

### Client-Side Usage (JavaScript)

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/agents/hub")
    .build();

connection.on("Message", (message) => {
    console.log("Received:", message.content);
});

await connection.start();

// Send message
await connection.invoke("SendMessage", {
    fromAgentId: "browser-agent",
    toAgentId: "server-agent",
    content: "Hello from browser",
    messageType: "greeting"
});
```

## Comparison

### When to Use Each

| Message Bus | Best For | Not Ideal For |
|------------|----------|---------------|
| **In-Memory** | Development, Testing, Single Process | Distributed Systems |
| **Kafka** | High-throughput, Event Streaming, Distributed | Simple Use Cases |
| **RabbitMQ** | Complex Routing, Reliability, Work Queues | High-throughput Streaming |
| **Redis** | Fast Pub/Sub, Simple Use Cases, Caching | Complex Routing, Persistence |
| **SignalR** | Web Applications, Real-time Browser | Backend-only Systems |

### Performance Characteristics

| Message Bus | Throughput | Latency | Reliability | Scalability |
|------------|------------|---------|-------------|-------------|
| In-Memory | Very High | Very Low | Low | Single Process |
| Kafka | Very High | Low | High | Excellent |
| RabbitMQ | High | Low | Very High | Very Good |
| Redis | High | Very Low | Medium | Good |
| SignalR | Medium | Low | Medium | Good |

## Best Practices

### 1. Error Handling

```csharp
await messageBus.SubscribeAsync("agent-1", async (message) =>
{
    try
    {
        await ProcessMessageAsync(message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing message");
        // Decide: retry, dead letter, or ignore
    }
});
```

### 2. Message Serialization

```csharp
// Use JSON for complex messages
var message = new AgentMessage
{
    Content = JsonSerializer.Serialize(complexObject),
    MessageType = "complex-data"
};
```

### 3. Connection Management

```csharp
// Dispose message bus properly
await using var messageBus = serviceProvider.GetRequiredService<IAgentMessageBus>();
```

### 4. Monitoring

```csharp
// Track message metrics
var metrics = messageBus.GetMetrics();
Console.WriteLine($"Messages sent: {metrics.MessagesSent}");
Console.WriteLine($"Messages received: {metrics.MessagesReceived}");
```

### 5. Security

```csharp
// Use secure connections in production
services.AddKafkaMessageBus(options =>
{
    options.SecurityProtocol = SecurityProtocol.SaslSsl;
    options.SaslMechanism = SaslMechanism.Plain;
    options.SaslUsername = configuration["Kafka:Username"];
    options.SaslPassword = configuration["Kafka:Password"];
});
```

## Related Documentation

- [Multi-Agent Workflows](../architecture/MULTI_AGENT_WORKFLOWS_PLAN.md)
- [Agent Messaging](../../src/DotNetAgents.Agents.Messaging/README.md)
- [Plugin Architecture](../PLUGIN_ARCHITECTURE_MIGRATION.md)
