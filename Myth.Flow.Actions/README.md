# Myth.Flow.Actions

CQRS extension library for **Myth.Flow** pipeline framework, adding support for Commands, Queries, and Events with multiple message broker backends.

## 🚀 Features

- ✅ **CQRS Pattern**: Command, Query, and Event abstractions
- ✅ **Pipeline Integration**: Seamless integration with Myth.Flow
- ✅ **Multiple Brokers**: InMemory, Kafka, RabbitMQ support
- ✅ **Caching Layer**: Built-in query result caching
- ✅ **Event System**: Complete event subscription and handling
- ✅ **Resilience**: Retry policies, circuit breakers, dead letter queues
- ✅ **Auto-Discovery**: Automatic handler registration via assembly scanning
- ✅ **Telemetry**: Built-in OpenTelemetry support
- ✅ **Type-Safe**: Fully typed APIs with strong compile-time safety

## 📦 Installation

```bash
dotnet add package Myth.Flow.Actions
```

### Optional Dependencies

```bash
# For Kafka support
dotnet add package Confluent.Kafka

# For RabbitMQ support
dotnet add package RabbitMQ.Client

# For distributed caching
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

## 🎯 Quick Start

### 1. Configure Services

```csharp
using Myth.Flow.Actions.Configuration;

services.AddFlowActions(options =>
{
    options.UseInMemory()                    // or UseKafka() / UseRabbitMQ()
           .EnableTelemetry()
           .EnableCaching(cache =>
           {
               cache.ProviderType = CacheProviderType.Memory;
               cache.DefaultTtl = TimeSpan.FromMinutes(10);
           })
           .EnableRetry(retry =>
           {
               retry.MaxAttempts = 3;
               retry.BackoffMs = 1000;
               retry.ExponentialBackoff = true;
           })
           .ScanAssemblies(typeof(Program).Assembly);
});
```

### 2. Define Commands, Queries, and Events

```csharp
using Myth.Flow.Actions.Abstractions;
using Myth.Flow.Actions.Models;

// Command
public record CreateUserCommand : ICommand<Guid>
{
    public required string Email { get; init; }
    public required string Name { get; init; }
}

// Command Handler
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

// Query
public record GetUserQuery : IQuery<UserDto>
{
    public required Guid UserId { get; init; }
}

// Query Handler
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

// Event
public record UserCreatedEvent : DomainEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
}

// Event Handler
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

### 3. Use in Pipeline

```csharp
using Myth.Flow;
using Myth.Flow.Actions.Pipeline;

public class CreateUserRequest
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public Guid? UserId { get; set; }
    public UserDto? User { get; set; }
}

var result = await Pipeline
    .Start(new CreateUserRequest
    {
        Email = "user@example.com",
        Name = "John Doe"
    })
    // Process command
    .Process<CreateUserRequest, CreateUserCommand, Guid>(
        ctx => new CreateUserCommand
        {
            Email = ctx.Email,
            Name = ctx.Name
        },
        (ctx, userId) => ctx.UserId = userId)
    // Query with cache
    .QueryCached<CreateUserRequest, GetUserQuery, UserDto>(
        ctx => new GetUserQuery { UserId = ctx.UserId!.Value },
        (ctx, user) => ctx.User = user,
        cacheKey: $"user:{ctx.UserId}",
        ttl: TimeSpan.FromMinutes(10))
    // Publish event
    .Publish<CreateUserRequest, UserCreatedEvent>(ctx => new UserCreatedEvent
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

## 🔧 Configuration Options

### InMemory (Development/Testing)

```csharp
services.AddFlowActions(options =>
{
    options.UseInMemory()
           .ScanAssemblies(typeof(Program).Assembly);
});
```

### Kafka (Production)

```csharp
services.AddFlowActions(options =>
{
    options.UseKafka(kafka =>
           {
               kafka.BootstrapServers = "localhost:9092";
               kafka.GroupId = "my-service";
               kafka.ClientId = "my-service-1";
               kafka.EnableAutoCommit = false;
               kafka.SessionTimeoutMs = 30000;
               kafka.AutoOffsetReset = "earliest";
               kafka.CompressionType = "snappy";
           })
           .EnableRetry(retry =>
           {
               retry.MaxAttempts = 3;
               retry.BackoffMs = 1000;
               retry.ExponentialBackoff = true;
           })
           .EnableDeadLetterQueue()
           .ScanAssemblies(typeof(Program).Assembly);
});
```

### RabbitMQ (Production)

```csharp
services.AddFlowActions(options =>
{
    options.UseRabbitMQ(rabbit =>
           {
               rabbit.HostName = "localhost";
               rabbit.Port = 5672;
               rabbit.UserName = "guest";
               rabbit.Password = "guest";
               rabbit.VirtualHost = "/";
               rabbit.ExchangeName = "my-service";
               rabbit.ExchangeType = "topic";
               rabbit.PrefetchCount = 10;
           })
           .EnableCaching(cache =>
           {
               cache.ProviderType = CacheProviderType.Distributed;
               cache.ConnectionString = "localhost:6379";
               cache.DefaultTtl = TimeSpan.FromMinutes(5);
           })
           .ScanAssemblies(AppDomain.CurrentDomain.GetAssemblies());
});
```

## 📚 Pipeline Extensions

### Process (Commands)

```csharp
// Command without response
.Process<TContext, TCommand>(ctx => new TCommand { ... })

// Command with response
.Process<TContext, TCommand, TResponse>(
    ctx => new TCommand { ... },
    (ctx, response) => ctx.Result = response)
```

### Query (Read Operations)

```csharp
// Query without cache
.Query<TContext, TQuery, TResponse>(
    ctx => new TQuery { ... },
    (ctx, result) => ctx.Data = result)

// Query with cache
.QueryCached<TContext, TQuery, TResponse>(
    ctx => new TQuery { ... },
    (ctx, result) => ctx.Data = result,
    cacheKey: "my-key",
    ttl: TimeSpan.FromMinutes(10))

// Query with dynamic cache key
.Query<TContext, TQuery, TResponse>(
    ctx => new TQuery { ... },
    (ctx, result) => ctx.Data = result,
    options =>
    {
        options.Enabled = true;
        options.CacheKey = $"user:{ctx.UserId}";
        options.Ttl = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = true;
    })
```

### Publish (Events)

```csharp
// Publish event
.Publish<TContext, TEvent>(ctx => new TEvent { ... })

// Publish event when context is the event itself
.Publish<TEvent>()  // where TContext : IEvent
```

## 🎭 Advanced Patterns

### Conditional Processing

```csharp
.When(
    ctx => ctx.Order?.Status == OrderStatus.Pending,
    pipeline => pipeline
        .Process<TContext, ProcessPaymentCommand>(...)
        .Publish<TContext, OrderPaidEvent>(...))
```

### Transformation

```csharp
.Transform(ctx => new ShipmentContext
{
    ShipmentId = Guid.NewGuid(),
    OrderId = ctx.OrderId
})
```

### Multiple Event Handlers

```csharp
// Automatically all handlers for an event will be executed
public class UserCreatedEmailHandler : IEventHandler<UserCreatedEvent> { ... }
public class UserCreatedAnalyticsHandler : IEventHandler<UserCreatedEvent> { ... }
public class UserCreatedNotificationHandler : IEventHandler<UserCreatedEvent> { ... }

// All three handlers execute in parallel when UserCreatedEvent is published
```

## 🔍 Direct Dispatcher Usage

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
        // Process command
        var command = new CreateUserCommand { Email = email, Name = name };
        var result = await _dispatcher.DispatchCommandAsync<CreateUserCommand, Guid>(command);

        if (result.IsFailure)
            throw new Exception(result.ErrorMessage);

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
        // Query with cache
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

## 🧪 Testing

```csharp
public class UserServiceTests
{
    [Fact]
    public async Task CreateUser_ShouldSucceed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlow();
        services.AddFlowActions(options =>
        {
            options.UseInMemory()
                   .ScanAssemblies(typeof(CreateUserCommand).Assembly);
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
}
```

## 📊 Telemetry

Myth.Flow.Actions includes built-in OpenTelemetry support:

```csharp
services.AddFlowActions(options =>
{
    options.EnableTelemetry()
           .UseInMemory()
           .ScanAssemblies(typeof(Program).Assembly);
});

// Activities are automatically created for:
// - Command.{CommandName}
// - Query.{QueryName}
// - Event.{EventName}
// - EventHandler.{HandlerName}
```

## 🛡️ Resilience

### Retry Policy

```csharp
options.EnableRetry(retry =>
{
    retry.MaxAttempts = 3;
    retry.BackoffMs = 1000;
    retry.ExponentialBackoff = true;
});
```

### Dead Letter Queue

```csharp
options.EnableDeadLetterQueue();

// Access DLQ
public class MyService
{
    private readonly DeadLetterQueue _dlq;

    public MyService(DeadLetterQueue dlq)
    {
        _dlq = dlq;
    }

    public IEnumerable<DeadLetterMessage> GetFailedMessages()
    {
        return _dlq.GetAll();
    }
}
```

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Myth.Flow Pipeline                    │
├─────────────────────────────────────────────────────────┤
│  .Process()  │  .Query()  │  .Publish()  │  .Transform()│
└──────────────┴────────────┴──────────────┴──────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│                      Dispatcher                          │
├─────────────────────────────────────────────────────────┤
│  Commands  │  Queries (+ Cache)  │  Events              │
└────────────┴─────────────────────┴──────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│                    Message Broker                        │
├─────────────────────────────────────────────────────────┤
│  InMemory  │  Kafka  │  RabbitMQ  │  (Extensible)       │
└────────────┴─────────┴────────────┴─────────────────────┘
                        ▼
┌─────────────────────────────────────────────────────────┐
│                   Event Handlers                         │
├─────────────────────────────────────────────────────────┤
│  Multiple handlers per event, executed in parallel       │
└─────────────────────────────────────────────────────────┘
```

## 🎯 Best Practices

1. **Commands**: Use for write operations that change state
2. **Queries**: Use for read operations, leverage caching
3. **Events**: Use for decoupled communication, past tense naming
4. **Handlers**: Keep them focused, single responsibility
5. **Pipeline**: Chain operations logically, transform when needed
6. **Testing**: Use InMemory broker for fast, isolated tests
7. **Production**: Use Kafka/RabbitMQ with retry and DLQ enabled

## 📝 Naming Conventions

- **Commands**: Imperative verbs (CreateUser, UpdateOrder, DeleteProduct)
- **Queries**: Get/Find prefix (GetUser, FindOrders, SearchProducts)
- **Events**: Past tense (UserCreated, OrderUpdated, ProductDeleted)
- **Handlers**: {Request}Handler (CreateUserCommandHandler, GetUserQueryHandler)

## 🤝 Contributing

Contributions are welcome! Please follow the existing code style and add tests for new features.

## 📄 License

MIT License - see LICENSE file for details

## 🔗 Related Projects

- [Myth.Flow](https://github.com/your-repo/myth-flow) - Core pipeline framework