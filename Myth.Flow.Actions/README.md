<img  style="float: right;" src="myth-flow-actions-logo.png" alt="drawing" width="250"/>

# Myth.Flow.Actions

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Flow.Actions?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Flow.Actions/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Flow.Actions?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Flow.Actions/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A powerful .NET library implementing CQRS and Event-Driven Architecture patterns with seamless integration to Myth.Flow pipelines. Built for scalability with support for multiple message brokers, caching strategies, and enterprise-grade resilience features.

## 🎯 Why Myth.Flow.Actions?

Enterprise applications **struggle with scalability and maintainability** when business logic, queries, and side effects are tangled together. Controllers directly calling repositories, services calling other services, no clear boundaries—code becomes a monolith that can't scale horizontally, can't be tested properly, and can't adapt to changing business needs. **Myth.Flow.Actions brings CQRS and Event-Driven Architecture to .NET with zero ceremony**, transforming tightly coupled monoliths into scalable, message-driven systems.

### The Problem

**Monolithic Services Are Bottlenecks**
```csharp
// OrderController.cs - Everything in one place
public class OrderController : ControllerBase {
    private readonly OrderRepository _orderRepo;
    private readonly InventoryRepository _inventoryRepo;
    private readonly PaymentService _paymentService;
    private readonly EmailService _emailService;
    private readonly AuditService _auditService;

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request) {
        // Write operation mixed with reads
        var customer = await _customerRepo.GetByIdAsync(request.CustomerId);

        // Business logic in controller
        var order = new Order { ... };
        await _orderRepo.AddAsync(order);

        // Side effects inline - blocking operation
        await _paymentService.ProcessAsync(order);
        await _inventoryService.ReserveAsync(order.Items);

        // Emails sent synchronously - slow response times
        await _emailService.SendConfirmationAsync(customer.Email);

        // Audit coupled to business logic
        await _auditService.LogOrderCreatedAsync(order);

        return Ok(order);
    }
}
```

**Problems:**
- **No separation**: Commands (writes) and queries (reads) mixed together
- **Tight coupling**: Controller knows about payment, inventory, email, auditing
- **Slow responses**: Synchronous side effects block HTTP requests
- **Hard to scale**: Can't scale reads and writes independently
- **Untestable chaos**: Mocking 6 dependencies per test
- **No event history**: Business events lost, no audit trail

### The Solution

**CQRS + Event-Driven Architecture**
```csharp
// Command - Write operation
public record CreateOrderCommand : ICommand<Guid> {
    public Guid CustomerId { get; init; }
    public List<OrderItem> Items { get; init; }
}

// Handler - Business logic isolated
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid> {
    private readonly IOrderRepository _repository;
    private readonly IDispatcher _dispatcher;

    public async Task<CommandResult<Guid>> HandleAsync(CreateOrderCommand command, CancellationToken ct) {
        var order = new Order(command.CustomerId, command.Items);
        await _repository.AddAsync(order, ct);

        // Publish event - async, decoupled
        await _dispatcher.PublishEventAsync(new OrderCreatedEvent {
            OrderId = order.Id,
            CustomerId = command.CustomerId,
            Items = command.Items
        }, ct);

        return CommandResult<Guid>.Success(order.Id);
    }
}

// Event Handlers - Side effects decoupled
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent> {
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct) {
        // Process payment asynchronously
        await _paymentService.ProcessAsync(@event.OrderId, ct);
    }
}

public class OrderNotificationHandler : IEventHandler<OrderCreatedEvent> {
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken ct) {
        // Send email asynchronously
        await _emailService.SendConfirmationAsync(@event.CustomerId, ct);
    }
}

// Controller - Thin, focused
[HttpPost("orders")]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command) {
    var result = await _dispatcher.DispatchCommandAsync(command);
    return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
}
```

**Benefits:**
- **Clear separation**: Commands modify state, queries read state, events notify
- **Decoupled**: Event handlers run independently, can scale separately
- **Fast responses**: Side effects async via message queue
- **Scalable**: Scale read and write databases independently
- **Testable**: Mock only what each handler needs
- **Event-sourced ready**: Full event history for auditing and replay

### Why Choose Myth.Flow.Actions?

| Aspect | Myth.Flow.Actions | Manual CQRS | MediatR | NServiceBus/MassTransit |
|--------|-------------------|-------------|---------|-------------------------|
| **Setup Complexity** | One call: `UseActions()` | Build everything | Medium (behaviors) | High (transport config) |
| **Message Brokers** | InMemory, Kafka, RabbitMQ built-in | DIY integration | Not included | Core feature (complex) |
| **Query Caching** | Built-in (Memory/Redis) | Manual implementation | Not included | Not addressed |
| **Dispatcher** | Centralized IDispatcher | Manual routing | IMediator | Not centralized |
| **Retry/Resilience** | Built-in via Myth.Flow | Manual Polly | Manual behaviors | Built-in (complex config) |
| **OpenTelemetry** | Auto-instrumented | Manual | Manual | Manual |
| **Event Fan-Out** | Multiple handlers per event | Manual fan-out | Single handler | Built-in |
| **Dead Letter Queue** | Automatic | DIY | Not included | Requires config |
| **Learning Curve** | Low (follows Myth patterns) | N/A (DIY) | Medium | Steep |
| **Cost** | Free, OSS | N/A | Free, OSS | Commercial |

### Real-World Applications

**E-Commerce Order Processing**
Command creates order → Event triggers payment, inventory, shipping, email handlers independently. Scale each concern separately. Retry payment failures without affecting shipping.

**Financial Trading Platform**
Separate read models (queries) optimized for real-time dashboards from write models (commands) handling trades. Event-source trades for regulatory compliance and replay.

**SaaS Multi-Tenant Platform**
Commands modify tenant data. Events propagate changes to search indexes, analytics, billing systems. Scale reads (searches) independently from writes (data changes).

**IoT Device Management**
Commands configure devices. Events from devices trigger analytics, alerting, dashboard updates. Queue events when devices offline, process when reconnected.

**Healthcare Patient Records**
Commands modify EHR. Events notify care team, update dashboards, trigger workflows. Audit trail via event log for compliance (HIPAA).

### Key Differentiators

📨 **Centralized Dispatcher**
One `IDispatcher` handles commands, queries, and events. No need to inject multiple handlers—just dispatch.

🔄 **Multiple Event Handlers**
Publish one event, fan out to multiple handlers automatically. Perfect for cross-cutting concerns (logging, auditing, notifications).

🚀 **Message Broker Abstraction**
Switch between InMemory (dev), Kafka (high-throughput), RabbitMQ (enterprise) with configuration change. No code changes.

💾 **Query Caching Built-In**
Cache query results in Memory or Redis with TTL. Invalidate on command success. Massive read performance boost.

⚡ **Auto-Retry & Dead Letter Queue**
Transient failures auto-retry. Permanent failures go to DLQ for manual review. Never lose messages.

🔍 **OpenTelemetry Integration**
Every command, query, event automatically traced with distributed context. Debug microservices without log-diving.

🧩 **Assembly Scanning**
Auto-discover and register all handlers via reflection. Add new handler, it's automatically wired up.

### Conceptual Foundations

**CQRS (Command Query Responsibility Segregation)**
Invented by Greg Young and popularized by Martin Fowler. Separate write models (commands) from read models (queries). Optimize each independently.

**Event-Driven Architecture (EDA)**
Publish business events when state changes. Subscribers react asynchronously. Enables loose coupling and scalability.

**Event Sourcing (ES)**
Store state changes as events, not current state. Rebuild state by replaying events. Full audit trail, time travel debugging.

**Message Brokers / Pub-Sub**
Kafka for high-throughput, ordered streams. RabbitMQ for reliable delivery, dead-letter queues. InMemory for local dev/testing.

**Domain Events (DDD)**
Capture business facts: `OrderPlaced`, `PaymentProcessed`, `ItemShipped`. Not technical events. Part of Ubiquitous Language.

**Eventual Consistency**
Accepting that distributed systems can't be immediately consistent. Commands succeed, events propagate eventually.

**Saga Pattern**
Orchestrate multi-step transactions across services via events. Compensating actions for rollbacks.

### Business Value

**For Developers**
- **Clear patterns**: CQRS separates concerns, code is self-explanatory
- **Less boilerplate**: Auto-discovery, built-in dispatcher, zero manual wiring
- **Easy testing**: Mock only handler dependencies, not entire service graph
- **Faster features**: Add event handler without touching existing code

**For Architects**
- **Scalability**: Scale reads and writes independently
- **Decoupling**: Microservices communicate via events, not direct calls
- **Resilience**: Message brokers ensure delivery, retries handle transients
- **Audit trail**: Event log provides complete business history

**For DevOps/SRE**
- **Observability**: OpenTelemetry traces every message
- **Reliability**: Dead letter queues capture failures
- **Flexibility**: Switch message brokers without code changes
- **Monitoring**: Track command/query/event metrics

**For Product Teams**
- **Faster features**: Decouple features via events—parallel development
- **Better performance**: Cache queries, async side effects
- **Compliance ready**: Event logs for auditing (SOX, GDPR, HIPAA)
- **Cost efficiency**: Scale only what needs scaling

## Features

- **CQRS Pattern**: Clean separation of Commands, Queries, and Events with centralized dispatcher
- **Event-Driven Architecture**: Publish/subscribe with multiple handler support and message brokers
- **Pipeline Integration**: Fluent integration with Myth.Flow for composable workflows
- **Multiple Message Brokers**: InMemory (dev/test), Kafka, and RabbitMQ support
- **Query Caching**: Built-in caching with Memory and Redis providers
- **Resilience Patterns**: Retry policies with exponential backoff, circuit breakers, and dead letter queues
- **Auto-Discovery**: Automatic handler registration via assembly scanning
- **OpenTelemetry Integration**: Built-in distributed tracing and observability
- **Type Safety**: Fully typed APIs with compile-time safety

## Installation

```bash
dotnet add package Myth.Flow.Actions
```

### Optional Dependencies

```bash
# For Kafka support
dotnet add package Confluent.Kafka

# For RabbitMQ support
dotnet add package RabbitMQ.Client

# For Redis distributed caching
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

## Quick Start

### 1. Configure Services

```csharp
using Myth.Flow.Actions.Extensions;

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddFlow( config => config
    .UseTelemetry( )
    .UseLogging( )
    .UseRetry( attempts: 3, backoffMs: 100 )
    .UseActions( actions => actions
        .UseInMemory( )
        .UseCaching( )
        .ScanAssemblies( typeof( Program ).Assembly )));

var app = builder.BuildApp( );
app.Run( );
```

## 💉 Dependency Injection in Handlers

**IMPORTANT**: Starting with the current version, **handlers are registered as Scoped** and the Dispatcher **automatically creates a scope** before resolving them. This means you can:

✅ **Inject repositories directly** (that use DbContext)
✅ **Inject Scoped services** without `IServiceScopeFactory`
✅ **Simplify your handlers** - no need to create scopes manually

### How It Works

The Dispatcher manages the lifecycle automatically:

```csharp
// The Dispatcher does this internally for each command/query:
using var scope = MythServiceProvider.GetRequired().CreateScope();
var handler = scope.ServiceProvider.GetService<ICommandHandler<TCommand>>();
var result = await handler.HandleAsync(command, cancellationToken);
// Scope is automatically disposed, releasing DbContext and other resources
```

### Practical Example

```csharp
// ✅ DO THIS - Direct injection (simple and clean)
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid> {
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ILogger<CreateOrderHandler> logger) {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<CommandResult<Guid>> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken) {

        // Use repositories directly - the scope has already been created by the Dispatcher
        var products = await _productRepository.GetByIdsAsync(
            command.ProductIds,
            cancellationToken);

        var order = new Order { /* ... */ };
        await _orderRepository.AddAsync(order, cancellationToken);

        _logger.LogInformation("Order {OrderId} created", order.Id);
        return CommandResult<Guid>.Success(order.Id);
    }
}

// ❌ DON'T DO THIS - No longer necessary to use IServiceScopeFactory
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid> {
    private readonly IServiceScopeFactory _scopeFactory; // ❌ Unnecessary!

    public async Task<CommandResult<Guid>> HandleAsync(...) {
        // ❌ Manual scope creation is no longer needed!
        using var scope = _scopeFactory.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        // ...
    }
}
```

### When to Still Use IScopedService<T>?

The `IScopedService<T>` pattern is still available and useful in specific cases:

- **Background Services** (hosted services that are singleton)
- **Singleton Services** that need to access Scoped services
- **Special cases** where you need multiple scopes within the same operation

```csharp
// For background services or singletons that need scoped services
public class OrderProcessingBackgroundService : BackgroundService {
    private readonly IScopedService<IOrderRepository> _orderRepository;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            await _orderRepository.ExecuteAsync(async repo => {
                var pendingOrders = await repo.GetPendingOrdersAsync();
                // Process orders...
            });
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

### 2. Define Commands, Queries, and Events

#### Command

```csharp
using Myth.Interfaces;
using Myth.Models;

public record CreateUserCommand : ICommand<Guid> {
    public required string Email { get; init; }
    public required string Name { get; init; }
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid> {
    private readonly IUserRepository _repository;

    public CreateUserCommandHandler( IUserRepository repository ) {
        _repository = repository;
    }

    public async Task<CommandResult<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default ) {
        var user = new User {
            Id = Guid.NewGuid( ),
            Email = command.Email,
            Name = command.Name
        };

        await _repository.AddAsync( user, cancellationToken );

        return CommandResult<Guid>.Success( user.Id );
    }
}
```

#### Query

```csharp
public record GetUserQuery : IQuery<UserDto> {
    public required Guid UserId { get; init; }
}

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto> {
    private readonly IUserRepository _repository;

    public GetUserQueryHandler( IUserRepository repository ) {
        _repository = repository;
    }

    public async Task<QueryResult<UserDto>> HandleAsync(
        GetUserQuery query,
        CancellationToken cancellationToken = default ) {
        var user = await _repository.GetByIdAsync( query.UserId, cancellationToken );

        if ( user == null )
            return QueryResult<UserDto>.Failure( "User not found" );

        var dto = new UserDto {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name
        };

        return QueryResult<UserDto>.Success( dto );
    }
}
```

#### Event

```csharp
public record UserCreatedEvent : DomainEvent {
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
}

public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent> {
    private readonly IEmailService _emailService;

    public UserCreatedEventHandler( IEmailService emailService ) {
        _emailService = emailService;
    }

    public async Task HandleAsync(
        UserCreatedEvent @event,
        CancellationToken cancellationToken = default ) {
        await _emailService.SendWelcomeEmailAsync( @event.Email, cancellationToken );
    }
}
```

### 3. Use the Dispatcher

#### Simple Command Execution

```csharp
public class UserService {
    private readonly IDispatcher _dispatcher;

    public UserService( IDispatcher dispatcher ) {
        _dispatcher = dispatcher;
    }

    public async Task<Guid> CreateUserAsync( string email, string name ) {
        var command = new CreateUserCommand { Email = email, Name = name };
        var result = await _dispatcher.DispatchCommandAsync<CreateUserCommand, Guid>( command );

        if ( result.IsFailure )
            throw new InvalidOperationException( result.ErrorMessage );

        return result.Data;
    }
}
```

#### Query with Caching

```csharp
public async Task<UserDto?> GetUserAsync( Guid userId ) {
    var query = new GetUserQuery { UserId = userId };
    var cacheOptions = new CacheOptions {
        Enabled = true,
        CacheKey = $"user:{userId}",
        Ttl = TimeSpan.FromMinutes( 10 )
    };

    var result = await _dispatcher.DispatchQueryAsync<GetUserQuery, UserDto>(
        query,
        cacheOptions );

    return result.IsSuccess ? result.Data : null;
}
```

#### Event Publishing

```csharp
public async Task PublishUserCreatedAsync( Guid userId, string email ) {
    await _dispatcher.PublishEventAsync( new UserCreatedEvent {
        UserId = userId,
        Email = email
    });
}
```

### 4. Pipeline Integration

```csharp
public async Task<Result<UserDto>> CreateAndRetrieveUserAsync( string email, string name ) {
    var command = new CreateUserCommand { Email = email, Name = name };

    var result = await Pipeline
        .Start( command )
        .Process<CreateUserCommand, Guid>( )
        .Transform( userId => new GetUserQuery { UserId = userId })
        .Query<GetUserQuery, UserDto>( ( query, cache ) => cache.UseCache(
            $"user:{query.UserId}",
            TimeSpan.FromMinutes( 10 )))
        .Transform( user => new UserCreatedEvent { UserId = user.Id, Email = user.Email })
        .Publish<UserCreatedEvent>( )
        .ExecuteAsync( );

    return result;
}
```

## Configuration

### Message Brokers

#### InMemory Broker

```csharp
services.AddFlow( config => config
    .UseActions( actions => actions
        .UseInMemory( options => {
            options.UseDeadLetterQueue = true;
            options.MaxRetries = 3;
        })
        .ScanAssemblies( typeof( Program ).Assembly )));
```

#### Kafka

```csharp
services.AddFlow( config => config
    .UseTelemetry( )
    .UseActions( actions => actions
        .UseKafka( options => {
            options.BootstrapServers = "localhost:9092";
            options.GroupId = "my-service";
            options.ClientId = "my-service-instance-1";
            options.EnableAutoCommit = false;
            options.SessionTimeoutMs = 30000;
            options.AutoOffsetReset = "earliest";
        })
        .ScanAssemblies( typeof( Program ).Assembly )));
```

#### RabbitMQ

```csharp
services.AddFlow( config => config
    .UseTelemetry( )
    .UseActions( actions => actions
        .UseRabbitMQ( options => {
            options.HostName = "localhost";
            options.Port = 5672;
            options.UserName = "guest";
            options.Password = "guest";
            options.VirtualHost = "/";
            options.ExchangeName = "my-service-events";
            options.ExchangeType = "topic";
        })
        .ScanAssemblies( typeof( Program ).Assembly )));
```

### Caching

#### Memory Cache

```csharp
services.AddFlow( config => config
    .UseActions( actions => actions
        .UseInMemory( )
        .UseCaching( cache => {
            cache.ProviderType = CacheProviderType.Memory;
            cache.DefaultTtl = TimeSpan.FromMinutes( 5 );
        })
        .ScanAssemblies( typeof( Program ).Assembly )));
```

#### Redis Cache

```csharp
services.AddFlow( config => config
    .UseActions( actions => actions
        .UseInMemory( )
        .UseCaching( cache => {
            cache.ProviderType = CacheProviderType.Distributed;
            cache.ConnectionString = "localhost:6379";
            cache.DefaultTtl = TimeSpan.FromMinutes( 10 );
        })
        .ScanAssemblies( typeof( Program ).Assembly )));
```

#### Cache Management

Myth.Flow.Actions provides `ICacheManager` for manual cache invalidation and management:

```csharp
public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, Guid> {
    private readonly IUserRepository _repository;
    private readonly ICacheManager _cacheManager;

    public async Task<CommandResult<Guid>> HandleAsync(
        UpdateUserCommand command,
        CancellationToken ct) {

        // Update user
        var user = await _repository.GetByIdAsync(command.Id, ct);
        user.Name = command.Name;
        await _repository.UpdateAsync(user, ct);

        // Invalidate specific query cache
        await _cacheManager.InvalidateByTypeAsync(
            new GetUserQuery { UserId = command.Id }, ct);

        // Or invalidate all queries of a type
        await _cacheManager.InvalidateByTypeAsync<GetUserListQuery>(ct);

        // Or invalidate by pattern (useful for clearing related caches)
        await _cacheManager.InvalidateByPatternAsync("GetUser*", ct);

        return CommandResult<Guid>.Success(command.Id);
    }
}
```

**ICacheManager Methods:**

```csharp
public interface ICacheManager {
    // Get/Set cache values manually
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, bool slidingExpiration = false, CancellationToken ct = default);

    // Invalidate specific key
    Task InvalidateAsync(string key, CancellationToken ct = default);

    // Invalidate by pattern (e.g., "User:*")
    Task InvalidateByPatternAsync(string pattern, CancellationToken ct = default);

    // Invalidate by query type
    Task InvalidateByTypeAsync<TQuery>(TQuery? query = default, CancellationToken ct = default);

    // Generate cache key compatible with Dispatcher
    string GenerateKey<TQuery>(TQuery query);
}
```

**Usage Patterns:**

```csharp
// 1. Invalidate specific query after command
await _cacheManager.InvalidateByTypeAsync(new GetUserQuery { Id = userId });

// 2. Invalidate all queries of a type
await _cacheManager.InvalidateByTypeAsync<GetUserListQuery>();

// 3. Invalidate by pattern (Redis supports wildcards)
await _cacheManager.InvalidateByPatternAsync("User:*");

// 4. Manual cache key generation
var key = _cacheManager.GenerateKey(new GetUserQuery { Id = 123 });
await _cacheManager.InvalidateAsync(key);
```

**Note:** Pattern-based invalidation (`InvalidateByPatternAsync`) has limited support in MemoryCache. For production scenarios requiring pattern matching, use Redis cache provider.

## Core Interfaces

### IDispatcher

Central dispatcher for all CQRS operations:

```csharp
public interface IDispatcher {
    Task<CommandResult> DispatchCommandAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default )
        where TCommand : ICommand;

    Task<CommandResult<TResponse>> DispatchCommandAsync<TCommand, TResponse>(
        TCommand command,
        CancellationToken cancellationToken = default )
        where TCommand : ICommand<TResponse>;

    Task<QueryResult<TResponse>> DispatchQueryAsync<TQuery, TResponse>(
        TQuery query,
        CacheOptions? cacheOptions = null,
        CancellationToken cancellationToken = default )
        where TQuery : IQuery<TResponse>;

    Task PublishEventAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default )
        where TEvent : IEvent;
}
```

### IEventBus

Event publishing and subscription:

```csharp
public interface IEventBus {
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default )
        where TEvent : IEvent;

    void Subscribe<TEvent, THandler>( )
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>;

    void Unsubscribe<TEvent, THandler>( )
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>;
}
```

### Command, Query, and Event Interfaces

```csharp
public interface ICommand : IRequest<CommandResult> { }

public interface ICommand<TResponse> : IRequest<CommandResult<TResponse>> { }

public interface IQuery<TResponse> : IRequest<QueryResult<TResponse>> { }

public interface IEvent {
    string EventId { get; }
    DateTimeOffset OccurredAt { get; }
}
```

### Handler Interfaces

```csharp
public interface ICommandHandler<TCommand>
    where TCommand : ICommand {
    Task<CommandResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default );
}

public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse> {
    Task<CommandResult<TResponse>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default );
}

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> {
    Task<QueryResult<TResponse>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default );
}

public interface IEventHandler<TEvent>
    where TEvent : IEvent {
    Task HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken = default );
}
```

## Result Types

### CommandResult

```csharp
public readonly struct CommandResult {
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    public Dictionary<string, object>? Metadata { get; }

    public static CommandResult Success( Dictionary<string, object>? metadata = null );
    public static CommandResult Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null );
}

public readonly struct CommandResult<TResponse> {
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public TResponse? Data { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    public Dictionary<string, object>? Metadata { get; }

    public static CommandResult<TResponse> Success( TResponse data, Dictionary<string, object>? metadata = null );
    public static CommandResult<TResponse> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null );
}
```

### QueryResult

```csharp
public readonly struct QueryResult<TData> {
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public TData? Data { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    public bool FromCache { get; }
    public Dictionary<string, object>? Metadata { get; }

    public static QueryResult<TData> Success( TData data, bool fromCache = false, Dictionary<string, object>? metadata = null );
    public static QueryResult<TData> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null );
}
```

## Pipeline Extensions

### Starting Pipelines

```csharp
Pipeline.Start<TRequest>( TRequest request )

Pipeline.Start( )
```

### Processing Commands

```csharp
.Process<TCommand>( )

.Process<TCommand, TResponse>( )
```

### Executing Queries

```csharp
.Query<TQuery, TResponse>( )

.Query<TQuery, TResponse>( ( query, cache ) => cache.UseCache( "key", TimeSpan.FromMinutes( 5 )))
```

### Publishing Events

```csharp
.Publish<TEvent>( )
```

### Transformations

```csharp
.Transform<TNext>( current => new TNext { ... })

.TransformAsync<TNext>( async current => await CreateNextAsync( current ))

.TransformIf<TNext>(
    condition: current => current.IsValid,
    transform: current => new TNext { ... })

.TransformIf<TNext>(
    condition: current => current.Type == "Premium",
    transformTrue: current => new PremiumAction { ... },
    transformFalse: current => new StandardAction { ... })
```

## Resilience Features

### Retry Policy

```csharp
using Myth.Flow.Resilience;

var retryPolicy = new RetryPolicy(
    maxAttempts: 3,
    baseBackoffMs: 1000,
    exponentialBackoff: true,
    logger: logger );

var result = await retryPolicy.ExecuteAsync( async ( ) => {
    return await externalService.CallAsync( );
});
```

### Circuit Breaker

```csharp
var circuitBreaker = new CircuitBreakerPolicy(
    failureThreshold: 5,
    openDuration: TimeSpan.FromSeconds( 30 ),
    logger: logger );

var result = await circuitBreaker.ExecuteAsync( async ( ) => {
    return await unreliableService.CallAsync( );
});

if ( circuitBreaker.State == CircuitState.Open ) {
    // Circuit is open, service calls are blocked
}
```

### Dead Letter Queue

```csharp
services.AddFlow( config => config
    .UseActions( actions => actions
        .UseInMemory( options => {
            options.UseDeadLetterQueue = true;
            options.MaxRetries = 3;
        })
        .ScanAssemblies( typeof( Program ).Assembly )));

public class MonitoringService {
    private readonly DeadLetterQueue _dlq;

    public MonitoringService( DeadLetterQueue dlq ) {
        _dlq = dlq;
    }

    public IEnumerable<DeadLetterMessage> GetFailedMessages( ) {
        return _dlq.GetAll( );
    }

    public void RetryFailedMessage( ) {
        if ( _dlq.TryDequeue( out var message )) {
            // Retry processing the failed message
        }
    }
}
```

## Telemetry and Observability

### OpenTelemetry Integration

```csharp
services.AddFlow( config => config
    .UseTelemetry( )
    .UseActions( actions => actions
        .UseInMemory( )
        .ScanAssemblies( typeof( Program ).Assembly )));

// Activities are automatically created with the following names:
// - Command.{CommandName}
// - Query.{QueryName}
// - Event.{EventName}
// - EventBus.Publish.{EventName}
// - EventHandler.{HandlerName}
```

### Activity Tags

Each activity includes relevant tags:
- `pipeline.input.type`: The context type name
- `cache.hit`: Whether the query result was served from cache
- Additional custom tags from metadata

## Advanced Patterns

### Multiple Event Handlers

All handlers for an event execute in parallel:

```csharp
public class UserCreatedEmailHandler : IEventHandler<UserCreatedEvent> {
    public async Task HandleAsync( UserCreatedEvent @event, CancellationToken ct ) {
        // Send welcome email
    }
}

public class UserCreatedAnalyticsHandler : IEventHandler<UserCreatedEvent> {
    public async Task HandleAsync( UserCreatedEvent @event, CancellationToken ct ) {
        // Track analytics
    }
}

public class UserCreatedNotificationHandler : IEventHandler<UserCreatedEvent> {
    public async Task HandleAsync( UserCreatedEvent @event, CancellationToken ct ) {
        // Send push notification
    }
}

// All three handlers execute concurrently when event is published
```

### Complex Workflows

```csharp
public async Task<Result<ShipmentDto>> ProcessOrderWorkflowAsync(
    Guid customerId,
    List<OrderItem> items,
    Address address ) {
    var command = new CreateOrderCommand {
        CustomerId = customerId,
        Items = items,
        ShippingAddress = address
    };

    var result = await Pipeline
        .Start( command )
        .Process<CreateOrderCommand, Guid>( )
        .Transform( orderId => new GetOrderQuery { OrderId = orderId })
        .Query<GetOrderQuery, OrderDto>( )
        .Transform( order => new CreateShipmentCommand {
            OrderId = order.Id,
            ShipmentId = Guid.NewGuid( ),
            Address = order.ShippingAddress,
            Items = order.Items
        })
        .Process<CreateShipmentCommand, ShipmentDto>( )
        .Transform( shipment => new ShipmentCreatedEvent {
            OrderId = shipment.OrderId,
            ShipmentId = shipment.Id,
            TrackingNumber = shipment.TrackingNumber
        })
        .Publish<ShipmentCreatedEvent>( )
        .ExecuteAsync( );

    return result;
}
```

### Conditional Workflows

```csharp
public async Task<Result<OrderDto>> ValidateHighValueOrderAsync( Guid orderId ) {
    var command = new ValidateOrderCommand { OrderId = orderId };

    var result = await Pipeline
        .Start( command )
        .Process<ValidateOrderCommand, OrderDto>( )
        .TransformIf<FraudCheckCommand>(
            order => order.TotalAmount > 1000,
            order => new FraudCheckCommand { OrderId = order.Id })
        .Process<FraudCheckCommand>( )
        .Transform( fraudResult => new ProcessPaymentCommand { OrderId = orderId })
        .Process<ProcessPaymentCommand>( )
        .Transform( paymentResult => new OrderCompletedEvent { OrderId = orderId })
        .Publish<OrderCompletedEvent>( )
        .ExecuteAsync( );

    return result;
}
```

## Testing

### Testing Handlers

```csharp
using Xunit;
using FluentAssertions;

public class CreateUserCommandHandlerTests {
    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess( ) {
        // Arrange
        var repository = new InMemoryUserRepository( );
        var handler = new CreateUserCommandHandler( repository );
        var command = new CreateUserCommand {
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act
        var result = await handler.HandleAsync( command );

        // Assert
        result.IsSuccess.Should( ).BeTrue( );
        result.Data.Should( ).NotBe( Guid.Empty );
    }
}
```

### Testing Pipelines

```csharp
using Microsoft.Extensions.DependencyInjection;

public class UserPipelineTests {
    private readonly IServiceProvider _serviceProvider;

    public UserPipelineTests( ) {
        var services = new ServiceCollection( );
        services.AddLogging( );
        services.AddFlow( config => config
            .UseActions( actions => actions
                .UseInMemory( )
                .UseCaching( )
                .ScanAssemblies( typeof( CreateUserCommand ).Assembly )));

        services.AddScoped<IUserRepository, InMemoryUserRepository>( );
        services.AddScoped<IEmailService, FakeEmailService>( );

        _serviceProvider = services.BuildWithGlobalProvider( );
    }

    [Fact]
    public async Task CreateAndRetrieveUser_ShouldChainOperations( ) {
        // Arrange
        var command = new CreateUserCommand {
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act
        var result = await Pipeline
            .Start( command )
            .Process<CreateUserCommand, Guid>( )
            .Transform( userId => new GetUserQuery { UserId = userId })
            .Query<GetUserQuery, UserDto>( ( query, cache ) => cache.UseCache(
                $"user:{query.UserId}",
                TimeSpan.FromMinutes( 5 )))
            .Transform( user => new UserCreatedEvent { UserId = user.Id, Email = user.Email })
            .Publish<UserCreatedEvent>( )
            .ExecuteAsync( );

        // Assert
        result.IsSuccess.Should( ).BeTrue( );
        result.Value.Should( ).NotBeNull( );
    }
}
```

### Exception Handling in Tests

The pipeline is designed for **predictable results**. All exceptions thrown inside handlers or `.TapAsync()` steps are captured in `CommandResult.Failure()` / `QueryResult.Failure()` — they do not propagate to the caller unless explicitly configured.

**Assert on result state, not on thrown exceptions:**
```csharp
var result = await Pipeline
    .Start( command )
    .Process<CreateOrderCommand, Guid>( )
    .ExecuteAsync( );

// ✅ Correct — check result state
result.IsFailure.Should( ).BeTrue( );
result.Exception.Should( ).BeOfType<ValidationException>( );

// ❌ Incorrect — exception won't reach here by default
await act.Should( ).ThrowAsync<ValidationException>( );
```

To force specific exception types to propagate out of the pipeline, register them via `UseExceptionFilter<T>()` in the Myth.Flow configuration.

## Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    Myth.Flow Pipeline                         │
├──────────────────────────────────────────────────────────────┤
│  .Process()  │  .Query()  │  .Publish()  │  .Transform()     │
└──────────────┴────────────┴──────────────┴───────────────────┘
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

## Best Practices

1. **Commands**: Use for state-changing operations, imperative naming (CreateUser, UpdateOrder)
2. **Queries**: Use for read operations, leverage caching, prefix with Get/Find
3. **Events**: Use for decoupled communication, past tense naming (UserCreated, OrderProcessed)
4. **Handlers**: Keep focused and testable, single responsibility principle
5. **Pipeline**: Chain operations logically, use conditional flows when needed
6. **Testing**: Use InMemory broker for fast, isolated unit tests
7. **Production**: Use Kafka/RabbitMQ with retry policies and dead letter queues
8. **Caching**: Cache expensive queries, use appropriate TTL values
9. **Telemetry**: Enable for production to track command/query/event flows
10. **Result Pattern**: Always check IsSuccess before accessing Data

## Naming Conventions

- **Commands**: `{Verb}{Noun}Command` (CreateUserCommand, UpdateOrderCommand)
- **Queries**: `{Get|Find}{Noun}Query` (GetUserQuery, FindOrdersQuery)
- **Events**: `{Noun}{PastTenseVerb}Event` (UserCreatedEvent, OrderProcessedEvent)
- **Handlers**: `{Request}Handler` (CreateUserCommandHandler, UserCreatedEventHandler)
- **Results**: Use CommandResult, QueryResult with proper success/failure handling

## Contributing

Contributions are welcome! Please follow the existing code style and add tests for new features.

## License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.

## Related Projects

- [Myth.Flow](../Myth.Flow/README.md) - Core pipeline orchestration framework
- [Myth.Commons](../Myth.Commons/README.md) - Common utilities and extensions
- [Myth.Repository](../Myth.Repository/README.md) - Repository pattern implementation
