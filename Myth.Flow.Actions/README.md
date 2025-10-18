# Myth.Flow.Actions

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Flow.Actions?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Flow.Actions/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Flow.Actions?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Flow.Actions/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A powerful .NET library implementing CQRS and Event-Driven Architecture patterns with seamless integration to Myth.Flow pipelines. Built for scalability with support for multiple message brokers, caching strategies, and enterprise-grade resilience features.

# ⭐ Features

- **CQRS Pattern**: Clean separation of Commands, Queries, and Events
- **Pipeline Integration**: Fluent integration with Myth.Flow for composable workflows
- **Multiple Message Brokers**: InMemory (dev/test), Kafka, and RabbitMQ support
- **Query Caching**: Built-in caching with Memory and Redis providers
- **Event-Driven Architecture**: Publish/subscribe with multiple handler support
- **Resilience Patterns**: Retry policies with exponential backoff, circuit breakers, and dead letter queues
- **Auto-Discovery**: Automatic handler registration via assembly scanning
- **OpenTelemetry Integration**: Built-in distributed tracing and observability
- **Type Safety**: Fully typed APIs with compile-time safety

# 📦 Installation

```bash
dotnet add package Myth.Flow.Actions
```

## Optional Dependencies

```bash
# For Kafka support
dotnet add package Confluent.Kafka

# For RabbitMQ support
dotnet add package RabbitMQ.Client

# For Redis distributed caching
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

# 🚀 Quick Start

## 1. Configure Services

```csharp
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Settings;

builder.Services.AddFlowActions(config =>
{
    config.BrokerType = MessageBrokerType.InMemory;
    config.TelemetryEnabled = true;
    config.CachingEnabled = true;
    config.AssembliesToScan.Add(typeof(Program).Assembly);
});
```

## 2. Define Commands, Queries, and Events

### Command

```csharp
using Myth.Interfaces;
using Myth.Models;

public record CreateUserCommand : ICommand<Guid>
{
    public required string Email { get; init; }
    public required string Name { get; init; }
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _repository;

    public CreateUserCommandHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommandResult<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            Name = command.Name
        };

        await _repository.AddAsync(user, cancellationToken);

        return CommandResult<Guid>.Success(user.Id);
    }
}
```

### Query

```csharp
public record GetUserQuery : IQuery<UserDto>
{
    public required Guid UserId { get; init; }
}

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    private readonly IUserRepository _repository;

    public GetUserQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<QueryResult<UserDto>> HandleAsync(
        GetUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(query.UserId, cancellationToken);

        if (user == null)
            return QueryResult<UserDto>.Failure("User not found");

        var dto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name
        };

        return QueryResult<UserDto>.Success(dto);
    }
}
```

### Event

```csharp
public record UserCreatedEvent : DomainEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
}

public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;

    public UserCreatedEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task HandleAsync(
        UserCreatedEvent @event,
        CancellationToken cancellationToken = default)
    {
        await _emailService.SendWelcomeEmailAsync(@event.Email, cancellationToken);
    }
}
```

## 3. Use in Pipeline

```csharp
using Myth.Flow;
using Myth.Flow.Actions.Extensions;

public class CreateUserContext
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public Guid? UserId { get; set; }
    public UserDto? User { get; set; }
}

var result = await Pipeline
    .Start(new CreateUserContext
    {
        Email = "user@example.com",
        Name = "John Doe"
    })
    .WithTelemetry("CreateUserFlow")
    // Process command
    .Process<CreateUserContext, CreateUserCommand, Guid>(
        ctx => new CreateUserCommand
        {
            Email = ctx.Email,
            Name = ctx.Name
        },
        (ctx, userId) => ctx.UserId = userId)
    // Query with cache
    .Query<CreateUserContext, GetUserQuery, UserDto>(
        ctx => new GetUserQuery { UserId = ctx.UserId!.Value },
        (ctx, user) => ctx.User = user,
        cacheKey: $"user:{ctx.UserId}",
        ttl: TimeSpan.FromMinutes(10))
    // Publish event
    .Publish<CreateUserContext, UserCreatedEvent>(ctx => new UserCreatedEvent
    {
        UserId = ctx.UserId!.Value,
        Email = ctx.Email
    })
    .ExecuteAsync();

if (result.IsSuccess)
{
    Console.WriteLine($"User created: {result.Value.User?.Name}");
}
```

# 🔧 Configuration

## InMemory Broker

```csharp
services.AddFlowActions(config =>
{
    config.BrokerType = MessageBrokerType.InMemory;
    config.AssembliesToScan.Add(typeof(Program).Assembly);
});
```

## Kafka

```csharp
services.AddFlowActions(config =>
{
    config.BrokerType = MessageBrokerType.Kafka;
    config.BrokerConfigurationFactory = () => new KafkaOptions
    {
        BootstrapServers = "localhost:9092",
        GroupId = "my-service",
        ClientId = "my-service-instance-1",
        EnableAutoCommit = false,
        SessionTimeoutMs = 30000,
        AutoOffsetReset = "earliest"
    };
    config.TelemetryEnabled = true;
    config.AssembliesToScan.Add(typeof(Program).Assembly);
});
```

## RabbitMQ

```csharp
services.AddFlowActions(config =>
{
    config.BrokerType = MessageBrokerType.RabbitMQ;
    config.BrokerConfigurationFactory = () => new RabbitMQOptions
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "guest",
        Password = "guest",
        VirtualHost = "/",
        ExchangeName = "my-service-events",
        ExchangeType = "topic"
    };
    config.CachingEnabled = true;
    config.CacheConfiguration = cache =>
    {
        cache.ProviderType = CacheProviderType.Distributed;
        cache.ConnectionString = "localhost:6379";
        cache.DefaultTtl = TimeSpan.FromMinutes(5);
    };
    config.AssembliesToScan.AddRange(AppDomain.CurrentDomain.GetAssemblies());
});
```

# 📚 Pipeline Extensions

## Process (Commands)

```csharp
// Command without response
.Process<TContext, TCommand>(
    ctx => new TCommand { /* ... */ })

// Command with response
.Process<TContext, TCommand, TResponse>(
    ctx => new TCommand { /* ... */ },
    (ctx, response) => ctx.Result = response)
```

## Query (Read Operations)

```csharp
// Query with optional cache configuration
.Query<TContext, TQuery, TResponse>(
    ctx => new TQuery { /* ... */ },
    (ctx, result) => ctx.Data = result,
    options =>
    {
        options.Enabled = true;
        options.CacheKey = $"key:{ctx.Id}";
        options.Ttl = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = true;
    })

// Query with simple cache key
.Query<TContext, TQuery, TResponse>(
    ctx => new TQuery { /* ... */ },
    (ctx, result) => ctx.Data = result,
    cacheKey: "my-cache-key",
    ttl: TimeSpan.FromMinutes(10),
    slidingExpiration: false)
```

## Publish (Events)

```csharp
// Publish event from factory
.Publish<TContext, TEvent>(
    ctx => new TEvent { /* ... */ })

// Publish context as event (when TContext implements IEvent)
.Publish<TEvent>()
```

# 🔍 Direct Dispatcher Usage

For scenarios where you don't need the full pipeline:

```csharp
public class UserService
{
    private readonly IDispatcher _dispatcher;

    public UserService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<Guid> CreateUserAsync(string email, string name)
    {
        // Execute command
        var command = new CreateUserCommand { Email = email, Name = name };
        var result = await _dispatcher.DispatchCommandAsync<CreateUserCommand, Guid>(command);

        if (result.IsFailure)
            throw new InvalidOperationException(result.ErrorMessage);

        // Publish event
        await _dispatcher.PublishEventAsync(new UserCreatedEvent
        {
            UserId = result.Data,
            Email = email
        });

        return result.Data;
    }

    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        // Execute query with caching
        var query = new GetUserQuery { UserId = userId };
        var cacheOptions = new CacheOptions
        {
            Enabled = true,
            CacheKey = $"user:{userId}",
            Ttl = TimeSpan.FromMinutes(10)
        };

        var result = await _dispatcher.DispatchQueryAsync<GetUserQuery, UserDto>(
            query,
            cacheOptions);

        return result.IsSuccess ? result.Data : null;
    }
}
```

# 🛡️ Resilience Features

## Retry Policy

```csharp
using Myth.Flow.Resilience;

var retryPolicy = new RetryPolicy(
    maxAttempts: 3,
    baseBackoffMs: 1000,
    exponentialBackoff: true,
    logger: logger);

var result = await retryPolicy.ExecuteAsync(async () =>
{
    return await externalService.CallAsync();
});
```

## Circuit Breaker

```csharp
var circuitBreaker = new CircuitBreakerPolicy(
    failureThreshold: 5,
    openDuration: TimeSpan.FromSeconds(30),
    logger: logger);

var result = await circuitBreaker.ExecuteAsync(async () =>
{
    return await unreliableService.CallAsync();
});

// Check circuit state
if (circuitBreaker.State == CircuitState.Open)
{
    // Circuit is open, service calls are blocked
}
```

## Dead Letter Queue

```csharp
services.AddFlowActions(config =>
{
    config.BrokerType = MessageBrokerType.InMemory;
    config.BrokerConfigurationFactory = () => new InMemoryBrokerOptions
    {
        EnableDeadLetterQueue = true,
        MaxRetries = 3
    };
});

// Access dead letter queue
public class MonitoringService
{
    private readonly DeadLetterQueue _dlq;

    public MonitoringService(DeadLetterQueue dlq)
    {
        _dlq = dlq;
    }

    public IEnumerable<DeadLetterMessage> GetFailedMessages()
    {
        return _dlq.GetAll();
    }

    public void RetryFailedMessage()
    {
        if (_dlq.TryDequeue(out var message))
        {
            // Retry processing the failed message
        }
    }
}
```

# 📊 Telemetry & Observability

## OpenTelemetry Integration

```csharp
services.AddFlowActions(config =>
{
    config.TelemetryEnabled = true;
    config.BrokerType = MessageBrokerType.InMemory;
    config.AssembliesToScan.Add(typeof(Program).Assembly);
});

// Activities are automatically created with the following names:
// - Command.{CommandName}
// - Query.{QueryName}
// - Event.{EventName}
// - EventBus.Publish.{EventName}
// - EventHandler.{HandlerName}
```

## Activity Tags

Each activity includes relevant tags:
- `pipeline.input.type`: The context type name
- `cache.hit`: Whether the query result was served from cache
- Additional custom tags from metadata

# 🎯 Advanced Patterns

## Multiple Event Handlers

All handlers for an event execute in parallel:

```csharp
public class UserCreatedEmailHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        // Send welcome email
    }
}

public class UserCreatedAnalyticsHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        // Track analytics
    }
}

public class UserCreatedNotificationHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        // Send push notification
    }
}

// All three handlers execute concurrently when event is published
```

## Conditional Pipeline Steps

```csharp
var result = await Pipeline.Start(context)
    .Process<Context, ValidateOrderCommand>(ctx => new ValidateOrderCommand { OrderId = ctx.OrderId })
    .When(
        ctx => ctx.Order?.TotalAmount > 1000,
        pipeline => pipeline
            .Process<Context, FraudCheckCommand>(ctx => new FraudCheckCommand { OrderId = ctx.OrderId })
            .Process<Context, ManagerApprovalCommand>(ctx => new ManagerApprovalCommand { OrderId = ctx.OrderId }))
    .Process<Context, ProcessPaymentCommand>(ctx => new ProcessPaymentCommand { OrderId = ctx.OrderId })
    .Publish<Context, OrderCompletedEvent>(ctx => new OrderCompletedEvent { OrderId = ctx.OrderId })
    .ExecuteAsync();
```

## Context Transformation

```csharp
var result = await Pipeline.Start(orderContext)
    .Process<OrderContext, CreateOrderCommand, Guid>(
        ctx => new CreateOrderCommand { /* ... */ },
        (ctx, orderId) => ctx.OrderId = orderId)
    .Transform(ctx => new ShipmentContext
    {
        OrderId = ctx.OrderId,
        ShipmentId = Guid.NewGuid(),
        Address = ctx.ShippingAddress
    })
    .Process<ShipmentContext, CreateShipmentCommand>(
        ctx => new CreateShipmentCommand { /* ... */ })
    .ExecuteAsync();
```

# 🧪 Testing

```csharp
using Xunit;
using Microsoft.Extensions.DependencyInjection;

public class UserCommandHandlerTests
{
    [Fact]
    public async Task CreateUser_WithValidData_ShouldSucceed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlowActions(config =>
        {
            config.BrokerType = MessageBrokerType.InMemory;
            config.AssembliesToScan.Add(typeof(CreateUserCommand).Assembly);
        });

        services.AddScoped<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<IEmailService, FakeEmailService>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        // Act
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Name = "Test User"
        };

        var result = await dispatcher.DispatchCommandAsync<CreateUserCommand, Guid>(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Data);
    }

    [Fact]
    public async Task GetUser_WithCaching_ShouldReturnFromCache()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        services.AddFlowActions(config =>
        {
            config.BrokerType = MessageBrokerType.InMemory;
            config.CachingEnabled = true;
            config.AssembliesToScan.Add(typeof(GetUserQuery).Assembly);
        });

        services.AddScoped<IUserRepository, InMemoryUserRepository>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var userId = Guid.NewGuid();
        var query = new GetUserQuery { UserId = userId };
        var cacheOptions = new CacheOptions
        {
            Enabled = true,
            CacheKey = $"user:{userId}",
            Ttl = TimeSpan.FromMinutes(10)
        };

        // Act
        var result1 = await dispatcher.DispatchQueryAsync<GetUserQuery, UserDto>(query, cacheOptions);
        var result2 = await dispatcher.DispatchQueryAsync<GetUserQuery, UserDto>(query, cacheOptions);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.False(result1.FromCache);
        Assert.True(result2.IsSuccess);
        Assert.True(result2.FromCache); // Second call should be from cache
    }
}
```

# 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    Myth.Flow Pipeline                         │
├──────────────────────────────────────────────────────────────┤
│  .Process()  │  .Query()  │  .Publish()  │  .When()  │ .Tap()│
└──────────────┴────────────┴──────────────┴───────────┴───────┘
                            ▼
┌──────────────────────────────────────────────────────────────┐
│                       IDispatcher                             │
├──────────────────────────────────────────────────────────────┤
│  DispatchCommandAsync  │  DispatchQueryAsync  │  PublishEvent│
└────────────────────────┴──────────────────────┴──────────────┘
                            ▼
┌─────────────────────┬────────────────────┬───────────────────┐
│  Command Handlers   │  Query Handlers    │    IEventBus      │
│  (Write Operations) │  (Read + Cache)    │  (Pub/Sub)        │
└─────────────────────┴────────────────────┴───────────────────┘
                                               ▼
                            ┌──────────────────────────────────┐
                            │      IMessageBroker              │
                            ├──────────────────────────────────┤
                            │ InMemory │ Kafka │ RabbitMQ      │
                            └──────────────────────────────────┘
                                               ▼
                            ┌──────────────────────────────────┐
                            │      Event Handlers              │
                            ├──────────────────────────────────┤
                            │ Parallel execution per event     │
                            └──────────────────────────────────┘
```

# 🎯 Best Practices

1. **Commands**: Use for state-changing operations, imperative naming (CreateUser, UpdateOrder)
2. **Queries**: Use for read operations, leverage caching, prefix with Get/Find
3. **Events**: Use for decoupled communication, past tense naming (UserCreated, OrderProcessed)
4. **Handlers**: Keep focused and testable, single responsibility principle
5. **Pipeline**: Chain operations logically, use .When() for conditional flows
6. **Testing**: Use InMemory broker for fast, isolated unit tests
7. **Production**: Use Kafka/RabbitMQ with retry policies and dead letter queues
8. **Caching**: Cache expensive queries, use appropriate TTL values
9. **Telemetry**: Enable for production to track command/query/event flows
10. **Result Pattern**: Always check IsSuccess before accessing Data

# 📝 Naming Conventions

- **Commands**: `{Verb}{Noun}Command` (CreateUserCommand, UpdateOrderCommand)
- **Queries**: `{Get|Find}{Noun}Query` (GetUserQuery, FindOrdersQuery)
- **Events**: `{Noun}{PastTenseVerb}Event` (UserCreatedEvent, OrderProcessedEvent)
- **Handlers**: `{Request}Handler` (CreateUserCommandHandler, UserCreatedEventHandler)
- **Results**: Use CommandResult, QueryResult with proper success/failure handling

# 🤝 Contributing

Contributions are welcome! Please follow the existing code style and add tests for new features.

# 📄 License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.

# 🔗 Related Projects

- [Myth.Flow](../Myth.Flow/README.md) - Core pipeline orchestration framework
- [Myth.Commons](../Myth.Commons/README.md) - Common utilities and extensions
- [Myth.Repository](../Myth.Repository/README.md) - Repository pattern implementation
