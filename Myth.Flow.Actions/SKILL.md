---
name: myth-flow-actions
description: Use when you need CQRS and event-driven architecture. IDispatcher handles DispatchCommandAsync<T>(), DispatchQueryAsync<T,R>() with optional caching, and PublishEventAsync<T>(). IEventBus supports InMemory, RabbitMQ, and Kafka brokers. Handlers implement ICommandHandler<T>/IQueryHandler<T,R>/IEventHandler<T>, are registered as Scoped, and inject repositories directly. Requires Myth.Flow.
---

# Myth.Flow.Actions - CQRS and Event-Driven Architecture

## Overview

**Myth.Flow.Actions** is an **extension library** for **Myth.Flow** that implements **CQRS** (Command Query Responsibility Segregation) and **Event-Driven Architecture** patterns with built-in support for message brokers, caching, circuit breaker, and dead letter queue.

### 🔴 IMPORTANT: Requires Myth.Flow

**Myth.Flow.Actions is NOT a standalone library.** It **extends and depends on Myth.Flow**, adding CQRS and event-driven capabilities on top of the pipeline infrastructure. You **must install and configure Myth.Flow** before using Flow.Actions.

```bash
# Required installation order
dotnet add package Myth.Flow        # ← Install Flow first (REQUIRED)
dotnet add package Myth.Flow.Actions # ← Then install Actions (extends Flow)
```

**Why Flow.Actions Requires Flow:**
- Uses Myth.Flow's pipeline infrastructure for handler execution
- Leverages Flow's retry policies and telemetry
- Integrates with Flow's service provider management
- Extends Flow's Result pattern for commands and queries
- Shares Flow's configuration system

---

**Key Features:**
- **CQRS Pattern**: Separate command and query handlers with typed responses
- **Event-Driven Architecture**: Event bus with publish/subscribe pattern
- **Multiple Message Brokers**: InMemory, RabbitMQ, and Kafka support
- **Query Caching**: Built-in cache providers (Memory and Redis)
- **Resilience Patterns**: Circuit breaker and dead letter queue for failed messages
- **Auto-Discovery**: Automatic handler registration via assembly scanning
- **Scoped Services**: Handlers registered as Scoped with automatic scope management
- **Pipeline Integration**: Seamless integration with Myth.Flow pipelines

**Dependencies:**
- .NET 8.0+
- **Myth.Flow** ← **REQUIRED** (base pipeline library)
- Myth.Commons (base types and service provider)
- RabbitMQ.Client (optional, for RabbitMQ broker)
- Confluent.Kafka (optional, for Kafka broker)

---

## Installation

```bash
dotnet add package Myth.Flow.Actions
```

**Optional Broker Dependencies:**
```bash
# For RabbitMQ support
dotnet add package RabbitMQ.Client

# For Kafka support
dotnet add package Confluent.Kafka
```

---

## Core Concepts

### 1. CQRS Pattern

**Commands** represent actions that change state:
- `ICommand` - Command without response
- `ICommand<TResponse>` - Command with typed response

**Queries** represent data retrieval operations:
- `IQuery<TResponse>` - Query with typed response

**Separation of Concerns:**
- Commands handle writes (create, update, delete)
- Queries handle reads (get, list, search)
- Different optimization strategies for reads vs writes

### 2. Event-Driven Architecture

**Events** represent something that happened in the domain:
- `IEvent` - Base interface for all events
- Events are immutable and have `EventId` and `OccurredAt`
- Multiple handlers can subscribe to the same event
- Events can be published to message brokers for cross-service communication

### 3. Message Brokers

**Broker Types:**
- **InMemory**: Local in-process broker (default, good for testing)
- **RabbitMQ**: Reliable message broker with persistence
- **Kafka**: High-throughput distributed event streaming

**Use Cases:**
- Asynchronous event processing
- Cross-service communication
- Event sourcing
- Message queuing

### 4. Caching

**Cache Providers:**
- **Memory**: In-memory cache (IMemoryCache)
- **Redis**: Distributed cache for multi-instance scenarios

**Features:**
- TTL (Time To Live) support
- Sliding expiration
- Custom cache key generation
- Pattern-based invalidation

### 5. Resilience Patterns

**Circuit Breaker:**
- Protects resources from cascading failures
- States: Closed, Open, HalfOpen
- Configurable failure threshold and open duration

**Dead Letter Queue:**
- Stores failed messages for later analysis
- Prevents message loss
- Configurable max size
- Supports retry and reprocessing

---

## Namespace Quick Reference

All types from this library live inside the `Myth.Flow.Actions` assembly. The namespaces are:

| Type | Namespace | Notes |
|------|-----------|-------|
| `ICommand` | `Myth.Interfaces` | Marker interface — `using Myth.Interfaces;` |
| `ICommand<TResponse>` | `Myth.Interfaces` | |
| `ICommandHandler<TCommand>` | `Myth.Interfaces` | |
| `ICommandHandler<TCommand, TResponse>` | `Myth.Interfaces` | |
| `IQuery<TResponse>` | `Myth.Interfaces` | |
| `IQueryHandler<TQuery, TResponse>` | `Myth.Interfaces` | |
| `IEvent` | `Myth.Interfaces` | |
| `IEventHandler<TEvent>` | `Myth.Interfaces` | |
| `IDispatcher` | `Myth.Interfaces` | |
| `CommandResult` | `Myth.Models` | `using Myth.Models;` |
| `CommandResult<TResponse>` | `Myth.Models` | |
| `QueryResult<TData>` | `Myth.Models` | |
| `CacheOptions` | `Myth.Models` | |
| `ICacheManager` | `Myth.Interfaces` | Inject in handlers to invalidate/manage cache manually |
| `ValidationException` | `Myth.Exceptions` | Lives in Myth.Guard assembly — `using Myth.Exceptions;` |

> **Common pitfall:** `ICommand` and `CommandResult` are in **different namespaces** (`Myth.Interfaces` vs `Myth.Models`). Both are required at the top of handler files.

---

## API Reference

### IDispatcher

Central dispatcher for all CQRS operations.

```csharp
public interface IDispatcher {
    // Dispatch command without response
    Task<CommandResult> DispatchCommandAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    // Dispatch command with typed response
    Task<CommandResult<TResponse>> DispatchCommandAsync<TCommand, TResponse>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResponse>;

    // Dispatch query with optional caching
    Task<QueryResult<TResponse>> DispatchQueryAsync<TQuery, TResponse>(
        TQuery query,
        CacheOptions? cacheOptions = null,
        CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>;

    // Publish event
    Task PublishEventAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}
```

### Handler Interfaces

**Command Handlers:**
```csharp
// Command without response
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand {
    Task<CommandResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}

// Command with response
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse> {
    Task<CommandResult<TResponse>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}
```

**Query Handler:**
```csharp
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse> {
    Task<QueryResult<TResponse>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default);
}
```

**Event Handler:**
```csharp
public interface IEventHandler<in TEvent>
    where TEvent : IEvent {
    Task HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken = default);
}
```

### Result Types

**CommandResult:**
```csharp
public readonly struct CommandResult {
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    public Dictionary<string, object>? Metadata { get; }
    public HttpStatusCode StatusCode { get; } // always set — never null

    // Success factories
    public static CommandResult Success(Dictionary<string, object>? metadata = null);    // 200
    public static CommandResult Created(Dictionary<string, object>? metadata = null);    // 201
    public static CommandResult NoContent(Dictionary<string, object>? metadata = null);  // 204

    // Failure factories
    public static CommandResult Failure(string message, Exception? exception = null);    // 400
    public static CommandResult Failure(string message, HttpStatusCode statusCode, Exception? exception = null);
    public static CommandResult NotFound(string message);           // 404
    public static CommandResult Forbidden(string message = "Access denied");             // 403
    public static CommandResult Unauthorized(string message = "Unauthorized");           // 401
    public static CommandResult PaymentRequired(string message);    // 402
    public static CommandResult Conflict(string message);           // 409
    public static CommandResult UnprocessableEntity(string message);// 422
}

public readonly struct CommandResult<TResponse> {
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public TResponse? Data { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    public Dictionary<string, object>? Metadata { get; }
    public HttpStatusCode StatusCode { get; } // always set — never null

    // Success factories
    public static CommandResult<TResponse> Success(TResponse data, Dictionary<string, object>? metadata = null); // 200
    public static CommandResult<TResponse> Created(TResponse data, Dictionary<string, object>? metadata = null); // 201
    public static CommandResult<TResponse> NoContent(Dictionary<string, object>? metadata = null);               // 204

    // Failure factories (same as non-generic)
    public static CommandResult<TResponse> Failure(string message, Exception? exception = null);
    public static CommandResult<TResponse> Failure(string message, HttpStatusCode statusCode, Exception? exception = null);
    public static CommandResult<TResponse> NotFound(string message);
    public static CommandResult<TResponse> Forbidden(string message = "Access denied");
    public static CommandResult<TResponse> Unauthorized(string message = "Unauthorized");
    public static CommandResult<TResponse> PaymentRequired(string message);
    public static CommandResult<TResponse> Conflict(string message);
    public static CommandResult<TResponse> UnprocessableEntity(string message);
}
```

**QueryResult:**
```csharp
public readonly struct QueryResult<TData> {
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public TData? Data { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    public bool FromCache { get; }
    public HttpStatusCode StatusCode { get; } // always set — never null

    // Success factories
    public static QueryResult<TData> Success(TData data, bool fromCache = false);        // 200
    public static QueryResult<TData> NoContent(Dictionary<string, object>? metadata = null); // 204

    // Failure factories
    public static QueryResult<TData> Failure(string message, Exception? exception = null);   // 400
    public static QueryResult<TData> Failure(string message, HttpStatusCode statusCode, Exception? exception = null);
    public static QueryResult<TData> NotFound(string message = "Not found");             // 404
    public static QueryResult<TData> Forbidden(string message = "Access denied");        // 403
    public static QueryResult<TData> Unauthorized(string message = "Unauthorized");      // 401
    public static QueryResult<TData> PaymentRequired(string message);                    // 402
    public static QueryResult<TData> Conflict(string message);                           // 409
}
```

**Choosing the right factory:**

| Situation | Factory |
|-----------|---------|
| Query found data | `QueryResult<T>.Success(data)` |
| Query returned intentionally empty (e.g. empty inbox) | `QueryResult<T>.NoContent()` |
| Resource does not exist | `QueryResult<T>.NotFound(msg)` |
| Command updated/deleted, no body needed | `CommandResult.NoContent()` |
| Command created a new resource | `CommandResult.Created()` / `CommandResult<T>.Created(id)` |

**Using semantic factories in handlers:**
```csharp
// ✅ Query — express each outcome explicitly
public async Task<QueryResult<ProjectDto>> HandleAsync(GetProjectQuery query, CancellationToken ct) {
    var project = await _projectRepository.FirstOrDefaultAsync(p => p.Id == query.ProjectId, ct);

    if (project is null)
        return QueryResult<ProjectDto>.NotFound($"Project {query.ProjectId} not found");

    if (project.OwnerId != query.RequestingUserId)
        return QueryResult<ProjectDto>.Forbidden();

    return QueryResult<ProjectDto>.Success(project.To<ProjectDto>());
}

// ✅ Command — Created for new resource, NoContent for void operations
public async Task<CommandResult<Guid>> HandleAsync(CreateWorkspaceCommand command, CancellationToken ct) {
    var user = await _userRepository.FirstOrDefaultAsync(u => u.Id == command.UserId, ct);

    if (user!.RemainingCredits < command.RequiredCredits)
        return CommandResult<Guid>.PaymentRequired("Insufficient credits to create a workspace");

    var workspace = new Workspace { /* ... */ };
    await _workspaceRepository.AddAsync(workspace, ct);
    return CommandResult<Guid>.Created(workspace.Id);
}
```

**Using StatusCode in controllers — works the same via IDispatcher or Pipeline.Start:**
```csharp
[HttpPost]
public async Task<IActionResult> CreateWorkspace(CreateWorkspaceCommand command, CancellationToken ct) {
    // Via IDispatcher
    var result = await _dispatcher.DispatchCommandAsync<CreateWorkspaceCommand, Guid>(command, ct);

    // StatusCode is always set — map it directly
    return result.IsSuccess
        ? StatusCode((int)result.StatusCode, result.Data)      // 200 or 201
        : StatusCode((int)result.StatusCode, result.ErrorMessage); // 4xx

    // Alternatively via Pipeline — identical StatusCode
    // var result = await Pipeline.Start(command).Process<CreateWorkspaceCommand, Guid>().ExecuteAsync();
    // return StatusCode((int)result.StatusCode!, result.IsSuccess ? result.Value : result.ErrorMessage);
}
```

### CacheOptions

```csharp
public sealed class CacheOptions {
    public bool Enabled { get; set; }
    public string? CacheKey { get; set; }
    public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes(5);
    public bool SlidingExpiration { get; set; }
    public Func<object, string>? KeyGenerator { get; set; }
}
```

### FlowActionsBuilder

```csharp
public sealed class FlowActionsBuilder {
    // Configure message brokers
    FlowActionsBuilder UseInMemory(Action<InMemoryBrokerOptions>? configure = null);
    FlowActionsBuilder UseKafka(Action<KafkaOptions> configure);
    FlowActionsBuilder UseRabbitMQ(Action<RabbitMQOptions> configure);

    // Configure caching
    FlowActionsBuilder UseCaching(Action<CacheConfiguration>? configure = null);

    // Enable dead letter queue
    FlowActionsBuilder UseDeadLetterQueue(bool enabled = true);

    // Register handler assemblies
    FlowActionsBuilder ScanAssemblies(params Assembly[] assemblies);

    // Auto-subscribe event handlers
    FlowActionsBuilder AutoSubscribeEventHandlers(bool enabled = true);
}
```

### IEventBus

```csharp
public interface IEventBus {
    // Publish event to all subscribers
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IEvent;

    // Subscribe to event type
    void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>;

    // Unsubscribe from event type
    void Unsubscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>;
}
```

### IMessageBroker

```csharp
public interface IMessageBroker {
    // Publish event to message broker
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default)
        where TEvent : IEvent;

    // Start consuming messages
    Task StartAsync(CancellationToken cancellationToken = default);

    // Stop consuming messages
    Task StopAsync(CancellationToken cancellationToken = default);
}
```

### ICacheProvider

```csharp
public interface ICacheProvider {
    // Get cached value
    Task<CacheValue<T>> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    // Set cached value
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        bool slidingExpiration = false,
        CancellationToken cancellationToken = default);

    // Remove cached value
    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default);

    // Remove cached values by pattern
    Task RemoveByPatternAsync(
        string pattern,
        CancellationToken cancellationToken = default);
}
```

### ICacheManager

The **public-facing cache management interface** for handlers and application code. Inject it directly to manually get, set, and invalidate cache entries from command or query handlers.

> **ICacheManager vs ICacheProvider**: `ICacheProvider` is an internal infrastructure abstraction used by the Dispatcher. `ICacheManager` is the developer-facing API — use it in your handlers.

```csharp
public interface ICacheManager {
    // Get a cached value by key
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    // Set a value with TTL (and optional sliding expiration)
    Task SetAsync<T>(string key, T value, TimeSpan ttl, bool slidingExpiration = false, CancellationToken cancellationToken = default);

    // Invalidate a specific cache key
    Task InvalidateAsync(string key, CancellationToken cancellationToken = default);

    // Invalidate all keys matching a pattern (e.g., "User:*")
    // Note: MemoryCache has limited pattern support — use Redis for production pattern matching
    Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    // Invalidate cache for a specific query type
    // If query instance is provided → invalidates only that query's entry
    // If query is null → invalidates ALL entries of that query type via pattern
    Task InvalidateByTypeAsync<TQuery>(TQuery? query = default, CancellationToken cancellationToken = default);

    // Generate the same cache key the Dispatcher would use for a query
    // Use this to manually build consistent keys for GetAsync/InvalidateAsync
    string GenerateKey<TQuery>(TQuery query);
}
```

**When to use ICacheManager:**

| Scenario | Method |
|----------|--------|
| Invalidate one specific query after a command | `InvalidateByTypeAsync(new GetUserQuery { Id = id })` |
| Invalidate all cached results for a query type | `InvalidateByTypeAsync<GetUserListQuery>()` |
| Invalidate a group of related keys | `InvalidateByPatternAsync("User:*")` |
| Read/write cache bypassing the Dispatcher | `GetAsync` / `SetAsync` |
| Build a key consistent with Dispatcher's algorithm | `GenerateKey(query)` |

**Example — command handler invalidating cache after update:**

```csharp
public class UpdateUserHandler : ICommandHandler<UpdateUserCommand> {
    private readonly IUserRepository _repository;
    private readonly ICacheManager _cacheManager;

    public UpdateUserHandler(IUserRepository repository, ICacheManager cacheManager) {
        _repository = repository;
        _cacheManager = cacheManager;
    }

    public async Task<CommandResult> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken) {

        var user = await _repository.GetByIdAsync(command.Id, cancellationToken);
        user.Name = command.Name;
        await _repository.UpdateAsync(user, cancellationToken);

        // Invalidate the specific user query
        await _cacheManager.InvalidateByTypeAsync(
            new GetUserQuery { UserId = command.Id }, cancellationToken);

        // Also invalidate list queries (no instance needed)
        await _cacheManager.InvalidateByTypeAsync<GetUserListQuery>(cancellationToken);

        return CommandResult.Success();
    }
}
```

### CircuitBreakerPolicy

```csharp
public sealed class CircuitBreakerPolicy {
    // Constructor
    public CircuitBreakerPolicy(
        int failureThreshold,
        TimeSpan openDuration,
        ILogger? logger = null);

    // Execute operation with circuit breaker protection
    Task<T> ExecuteAsync<T>(
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default);

    // Get current circuit state
    CircuitState State { get; }
}

public enum CircuitState {
    Closed,    // Normal operation
    Open,      // Failures exceed threshold
    HalfOpen   // Testing if service recovered
}
```

### DeadLetterQueue

```csharp
public sealed class DeadLetterQueue {
    // Constructor
    public DeadLetterQueue(ILogger<DeadLetterQueue> logger, int maxSize = 10000);

    // Add failed message
    void Enqueue(
        object message,
        Exception exception,
        EventMetadata? metadata = null);

    // Remove message
    bool TryDequeue(out DeadLetterMessage? message);

    // Get all messages
    IEnumerable<DeadLetterMessage> GetAll();

    // Get count
    int Count { get; }

    // Clear all messages
    void Clear();
}
```

---

## Usage Examples

### 1. Basic Setup (ASP.NET Core)

```csharp
using Myth.Flow.Actions;

var builder = WebApplication.CreateBuilder(args);

// Add Flow.Actions with InMemory broker
builder.Services.AddFlow(config => config
    .UseActions(actions => actions
        .UseInMemory()
        .UseCaching()
        .UseDeadLetterQueue()
        .ScanAssemblies(typeof(Program).Assembly)
        .AutoSubscribeEventHandlers()));

var app = builder.BuildApp(); // Use BuildApp() instead of Build()
app.Run();
```

### 2. Command Without Response

```csharp
// Define command
public record DeleteUserCommand(Guid UserId) : ICommand;

// Implement handler (injecting repositories is now SIMPLE and CLEAN)
public class DeleteUserHandler : ICommandHandler<DeleteUserCommand> {
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserHandler> _logger;

    // ✅ Inject repositories directly - scope is managed by Dispatcher
    public DeleteUserHandler(
        IUserRepository userRepository,
        ILogger<DeleteUserHandler> logger) {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<CommandResult> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken) {
        try {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
            if (user == null) {
                return CommandResult.Failure($"User {command.UserId} not found");
            }

            await _userRepository.DeleteAsync(user, cancellationToken);
            _logger.LogInformation("User {UserId} deleted", command.UserId);

            return CommandResult.Success();
        } catch (Exception ex) {
            _logger.LogError(ex, "Error deleting user {UserId}", command.UserId);
            return CommandResult.Failure("Failed to delete user", ex);
        }
    }
}

// Use in controller
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase {
    private readonly IDispatcher _dispatcher;

    public UsersController(IDispatcher dispatcher) {
        _dispatcher = dispatcher;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id) {
        var result = await _dispatcher.DispatchCommandAsync(
            new DeleteUserCommand(id));

        return result.IsSuccess
            ? Ok()
            : BadRequest(result.ErrorMessage);
    }
}
```

### 3. Command With Response

```csharp
// Define command
public record CreateOrderCommand(
    Guid CustomerId,
    List<Guid> ProductIds,
    string ShippingAddress) : ICommand<Guid>;

// Implement handler
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid> {
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEventBus _eventBus;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IEventBus eventBus) {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _eventBus = eventBus;
    }

    public async Task<CommandResult<Guid>> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken) {
        // Validate products
        var products = await _productRepository
            .GetByIdsAsync(command.ProductIds, cancellationToken);

        if (products.Count != command.ProductIds.Count) {
            return CommandResult<Guid>.Failure("Some products not found");
        }

        // Create order
        var order = new Order {
            CustomerId = command.CustomerId,
            Items = products.Select(p => new OrderItem {
                ProductId = p.Id,
                Quantity = 1,
                Price = p.Price
            }).ToList(),
            ShippingAddress = command.ShippingAddress,
            Status = OrderStatus.Pending
        };

        await _orderRepository.AddAsync(order, cancellationToken);

        // Publish event
        await _eventBus.PublishAsync(new OrderCreatedEvent {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = DateTimeOffset.UtcNow,
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.Total
        }, cancellationToken);

        return CommandResult<Guid>.Success(order.Id);
    }
}

// Use in controller
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderRequest request) {
    var result = await _dispatcher.DispatchCommandAsync<CreateOrderCommand, Guid>(
        new CreateOrderCommand(
            request.CustomerId,
            request.ProductIds,
            request.ShippingAddress));

    return result.IsSuccess
        ? CreatedAtAction(nameof(GetOrder), new { id = result.Data }, result.Data)
        : BadRequest(result.ErrorMessage);
}
```

### 4. Query With Caching

```csharp
// Define query
public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;

// Implement handler
public class GetUserByIdHandler : IQueryHandler<GetUserByIdQuery, UserDto> {
    private readonly IUserRepository _userRepository;

    public GetUserByIdHandler(IUserRepository userRepository) {
        _userRepository = userRepository;
    }

    public async Task<QueryResult<UserDto>> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken) {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (user == null) {
            return QueryResult<UserDto>.Failure($"User {query.UserId} not found");
        }

        var dto = new UserDto {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };

        return QueryResult<UserDto>.Success(dto);
    }
}

// Use with caching in controller
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(Guid id) {
    var cacheOptions = new CacheOptions {
        Enabled = true,
        CacheKey = $"user:{id}",
        Ttl = TimeSpan.FromMinutes(10),
        SlidingExpiration = true
    };

    var result = await _dispatcher.DispatchQueryAsync<GetUserByIdQuery, UserDto>(
        new GetUserByIdQuery(id),
        cacheOptions);

    if (!result.IsSuccess) {
        return NotFound(result.ErrorMessage);
    }

    // Check if result came from cache
    Response.Headers.Add("X-Cache", result.FromCache ? "HIT" : "MISS");

    return Ok(result.Data);
}
```

### 5. Event Publishing and Handling

```csharp
// Define event
public record OrderCreatedEvent : IEvent {
    public string EventId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
}

// Implement event handler (send email)
public class SendOrderConfirmationHandler : IEventHandler<OrderCreatedEvent> {
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;

    public SendOrderConfirmationHandler(
        IEmailService emailService,
        IUserRepository userRepository) {
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task HandleAsync(
        OrderCreatedEvent @event,
        CancellationToken cancellationToken) {
        var user = await _userRepository.GetByIdAsync(
            @event.CustomerId, cancellationToken);

        await _emailService.SendOrderConfirmationAsync(
            user.Email,
            @event.OrderId,
            @event.TotalAmount,
            cancellationToken);
    }
}

// Implement event handler (update analytics)
public class UpdateOrderAnalyticsHandler : IEventHandler<OrderCreatedEvent> {
    private readonly IAnalyticsService _analyticsService;

    public UpdateOrderAnalyticsHandler(IAnalyticsService analyticsService) {
        _analyticsService = analyticsService;
    }

    public async Task HandleAsync(
        OrderCreatedEvent @event,
        CancellationToken cancellationToken) {
        await _analyticsService.TrackOrderCreatedAsync(
            @event.OrderId,
            @event.TotalAmount,
            cancellationToken);
    }
}

// Publish event from command handler
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid> {
    private readonly IDispatcher _dispatcher;
    // ... other dependencies

    public async Task<CommandResult<Guid>> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken) {
        // Create order...

        // Publish event (both handlers will be invoked)
        await _dispatcher.PublishEventAsync(new OrderCreatedEvent {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = DateTimeOffset.UtcNow,
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.Total
        }, cancellationToken);

        return CommandResult<Guid>.Success(order.Id);
    }
}
```

### 6. RabbitMQ Configuration

```csharp
builder.Services.AddFlow(config => config
    .UseActions(actions => actions
        .UseRabbitMQ(options => {
            options.HostName = "localhost";
            options.UserName = "guest";
            options.Password = "guest";
            options.VirtualHost = "/";
            options.Port = 5672;
            options.ExchangeName = "my-app-events";
            options.ExchangeType = "topic";
            options.Durable = true;
        })
        .UseCaching(cache => {
            cache.ProviderType = CacheProviderType.Redis;
            cache.ConnectionString = "localhost:6379";
            cache.DefaultTtl = TimeSpan.FromMinutes(15);
        })
        .UseDeadLetterQueue(enabled: true)
        .ScanAssemblies(typeof(Program).Assembly)
        .AutoSubscribeEventHandlers()));
```

### 7. Kafka Configuration

```csharp
builder.Services.AddFlow(config => config
    .UseActions(actions => actions
        .UseKafka(options => {
            options.BootstrapServers = "localhost:9092";
            options.GroupId = "my-app-consumer-group";
            options.Topic = "my-app-events";
            options.AutoOffsetReset = AutoOffsetReset.Earliest;
            options.EnableAutoCommit = true;
        })
        .ScanAssemblies(typeof(Program).Assembly)
        .AutoSubscribeEventHandlers()));
```

### 8. Circuit Breaker Usage

```csharp
public class ExternalApiHandler : ICommandHandler<CallExternalApiCommand, string> {
    private readonly HttpClient _httpClient;
    private readonly CircuitBreakerPolicy _circuitBreaker;

    public ExternalApiHandler(
        HttpClient httpClient,
        ILogger<ExternalApiHandler> logger) {
        _httpClient = httpClient;
        // Configure circuit breaker
        _circuitBreaker = new CircuitBreakerPolicy(
            failureThreshold: 5,
            openDuration: TimeSpan.FromSeconds(30),
            logger: logger);
    }

    public async Task<CommandResult<string>> HandleAsync(
        CallExternalApiCommand command,
        CancellationToken cancellationToken) {
        try {
            var response = await _circuitBreaker.ExecuteAsync(
                async () => await _httpClient.GetStringAsync(
                    command.Url, cancellationToken),
                cancellationToken);

            return CommandResult<string>.Success(response);
        } catch (InvalidOperationException ex) when (ex.Message.Contains("Circuit breaker is open")) {
            return CommandResult<string>.Failure("Service temporarily unavailable", ex);
        } catch (Exception ex) {
            return CommandResult<string>.Failure("Failed to call external API", ex);
        }
    }
}
```

### 9. Dead Letter Queue Monitoring

```csharp
[ApiController]
[Route("api/admin/dead-letter-queue")]
public class DeadLetterQueueController : ControllerBase {
    private readonly DeadLetterQueue _deadLetterQueue;

    public DeadLetterQueueController(DeadLetterQueue deadLetterQueue) {
        _deadLetterQueue = deadLetterQueue;
    }

    [HttpGet]
    public IActionResult GetAll() {
        var messages = _deadLetterQueue.GetAll();
        return Ok(new {
            Count = _deadLetterQueue.Count,
            Messages = messages.Select(m => new {
                MessageType = m.Message.GetType().Name,
                Error = m.Exception.Message,
                EnqueuedAt = m.EnqueuedAt
            })
        });
    }

    [HttpPost("retry")]
    public async Task<IActionResult> RetryMessage() {
        if (_deadLetterQueue.TryDequeue(out var deadLetter)) {
            // Retry logic here...
            return Ok("Message requeued for retry");
        }
        return NotFound("No messages in queue");
    }

    [HttpDelete]
    public IActionResult Clear() {
        _deadLetterQueue.Clear();
        return Ok("Dead letter queue cleared");
    }
}
```

### 10. Integration with Myth.Flow Pipeline

```csharp
public class ProcessOrderPipeline {
    public async Task<Result<OrderResult>> ExecuteAsync(
        CreateOrderCommand command,
        IServiceProvider serviceProvider) {
        return await Pipeline.Start(command, serviceProvider)
            .WithTelemetry("ProcessOrder")
            .StepResultAsync<IValidator>((validator, cmd) =>
                validator.ValidateAsync(cmd, ValidationContextKey.Create))
            .StepResultAsync<IDispatcher>((dispatcher, cmd) =>
                dispatcher.DispatchCommandAsync<CreateOrderCommand, Guid>(cmd)
                    .ContinueWith(t => t.Result.IsSuccess
                        ? Result<Guid>.Success(t.Result.Data!)
                        : Result<Guid>.Failure(t.Result.ErrorMessage!)))
            .TapAsync<IEventBus>((eventBus, orderId) =>
                eventBus.PublishAsync(new OrderProcessedEvent {
                    EventId = Guid.NewGuid().ToString(),
                    OccurredAt = DateTimeOffset.UtcNow,
                    OrderId = orderId
                }))
            .Transform(orderId => new OrderResult { OrderId = orderId })
            .ExecuteAsync();
    }
}
```

---

## Advanced Patterns

### 1. Custom Cache Key Generation

```csharp
var cacheOptions = new CacheOptions {
    Enabled = true,
    KeyGenerator = query => {
        var q = (SearchProductsQuery)query;
        return $"products:search:{q.Category}:{q.MinPrice}:{q.MaxPrice}:{q.Page}";
    },
    Ttl = TimeSpan.FromMinutes(5),
    SlidingExpiration = true
};

var result = await _dispatcher.DispatchQueryAsync<SearchProductsQuery, List<ProductDto>>(
    query, cacheOptions);
```

### 2. Conditional Event Publishing

```csharp
public class UpdateUserHandler : ICommandHandler<UpdateUserCommand> {
    private readonly IUserRepository _userRepository;
    private readonly IDispatcher _dispatcher;

    public async Task<CommandResult> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken) {
        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        var emailChanged = user.Email != command.Email;

        user.Email = command.Email;
        user.Name = command.Name;

        await _userRepository.UpdateAsync(user, cancellationToken);

        // Publish event only if email changed
        if (emailChanged) {
            await _dispatcher.PublishEventAsync(new UserEmailChangedEvent {
                EventId = Guid.NewGuid().ToString(),
                OccurredAt = DateTimeOffset.UtcNow,
                UserId = user.Id,
                OldEmail = user.Email,
                NewEmail = command.Email
            }, cancellationToken);
        }

        return CommandResult.Success();
    }
}
```

### 3. Enriching Command Results with Metadata

```csharp
public class CreateUserHandler : ICommandHandler<CreateUserCommand, Guid> {
    public async Task<CommandResult<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken) {
        var user = new User { /* ... */ };
        await _userRepository.AddAsync(user, cancellationToken);

        var metadata = new Dictionary<string, object> {
            ["CreatedAt"] = DateTimeOffset.UtcNow,
            ["CreatedBy"] = "System",
            ["IpAddress"] = command.IpAddress
        };

        return CommandResult<Guid>.Success(user.Id, metadata);
    }
}
```

### 4. Transactional Commands with Multiple Repositories

```csharp
public class TransferFundsHandler : ICommandHandler<TransferFundsCommand> {
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<CommandResult> HandleAsync(
        TransferFundsCommand command,
        CancellationToken cancellationToken) {
        try {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var fromAccount = await _accountRepository.GetByIdAsync(
                command.FromAccountId, cancellationToken);
            var toAccount = await _accountRepository.GetByIdAsync(
                command.ToAccountId, cancellationToken);

            if (fromAccount.Balance < command.Amount) {
                return CommandResult.Failure("Insufficient funds");
            }

            fromAccount.Debit(command.Amount);
            toAccount.Credit(command.Amount);

            await _accountRepository.UpdateAsync(fromAccount, cancellationToken);
            await _accountRepository.UpdateAsync(toAccount, cancellationToken);

            var transaction = new Transaction { /* ... */ };
            await _transactionRepository.AddAsync(transaction, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            return CommandResult.Success();
        } catch (Exception ex) {
            await _unitOfWork.RollbackAsync(cancellationToken);
            return CommandResult.Failure("Transfer failed", ex);
        }
    }
}
```

### 5. Event Handler with Retry Logic

```csharp
public class SendEmailHandler : IEventHandler<UserRegisteredEvent> {
    private readonly IEmailService _emailService;
    private readonly ILogger<SendEmailHandler> _logger;
    private const int MaxRetries = 3;

    public async Task HandleAsync(
        UserRegisteredEvent @event,
        CancellationToken cancellationToken) {
        var retries = 0;
        while (retries < MaxRetries) {
            try {
                await _emailService.SendWelcomeEmailAsync(
                    @event.Email, @event.Name, cancellationToken);
                _logger.LogInformation("Welcome email sent to {Email}", @event.Email);
                return;
            } catch (Exception ex) {
                retries++;
                _logger.LogWarning(ex,
                    "Failed to send email (attempt {Retry}/{MaxRetries})",
                    retries, MaxRetries);

                if (retries >= MaxRetries) {
                    _logger.LogError(ex,
                        "Failed to send email after {MaxRetries} attempts", MaxRetries);
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retries)), cancellationToken);
            }
        }
    }
}
```

---

## Best Practices

### 1. Handler Registration

**✅ DO:**
- Use assembly scanning for automatic handler discovery
- Register handlers as Scoped to support database contexts
- Keep one handler per command/query/event
- Use descriptive names: `CreateOrderHandler`, `GetUserByIdHandler`

**❌ DON'T:**
- Register handlers manually unless necessary
- Create handlers with multiple responsibilities
- Use singleton handlers with scoped dependencies

### 2. Command/Query Design

**✅ DO:**
- Use records for immutable commands/queries
- Include all required data in the command/query
- Validate commands in handlers (or use Myth.Guard)
- Return typed responses for better type safety

**❌ DON'T:**
- Put business logic in commands/queries (they are DTOs)
- Use mutable properties
- Include dependencies in commands/queries

### 3. Event Design

**✅ DO:**
- Make events immutable and past-tense ("OrderCreated", not "CreateOrder")
- Include timestamp and event ID
- Include all relevant data (avoid requiring DB lookups in handlers)
- Keep events focused and single-purpose

**❌ DON'T:**
- Use events for request/response communication
- Include circular references
- Make events too granular (event spam)

### 4. Error Handling

**✅ DO:**
- Return CommandResult/QueryResult for expected failures
- Use meaningful error messages
- Log errors with context
- Use dead letter queue for failed events

**❌ DON'T:**
- Throw exceptions for validation errors
- Swallow exceptions silently
- Return null for failures

> **By design — predictable results, not exceptions:** The Dispatcher wraps all unhandled exceptions from handlers in `CommandResult.Failure()` / `QueryResult.Failure()`. The pipeline always produces a predictable outcome: either `IsSuccess` or `IsFailure` — never an unhandled crash. Use `result.Exception` to access the captured exception when `IsFailure` is true.
>
> For exceptions that must escape the pipeline entirely (infrastructure errors, `OperationCanceledException`), register them via `UseExceptionFilter<T>()` in the pipeline configuration. These bypass the catch-all and propagate to the caller, optionally wrapped in `PipelineException`.

### 5. Caching Strategy

**✅ DO:**
- Cache queries only (not commands)
- Use appropriate TTL based on data volatility
- Inject `ICacheManager` in command handlers to invalidate related query caches after writes
- Use `InvalidateByTypeAsync(queryInstance)` for specific entry invalidation and `InvalidateByTypeAsync<TQuery>()` for bulk invalidation
- Use sliding expiration for frequently accessed data

**❌ DON'T:**
- Cache everything by default
- Use overly long TTLs
- Cache sensitive data without encryption

### 6. Dependency Injection in Handlers

**✅ DO (RECOMMENDED):**
```csharp
// Inject repositories directly - Dispatcher creates scope automatically
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid> {
    private readonly IOrderRepository _repository;

    public CreateOrderHandler(IOrderRepository repository) {
        _repository = repository;
    }
}
```

**❌ DON'T (AVOID):**
```csharp
// Don't create scopes manually - unnecessary complexity
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid> {
    private readonly IServiceScopeFactory _scopeFactory;

    public async Task<CommandResult<Guid>> HandleAsync(...) {
        using var scope = _scopeFactory.CreateScope(); // ❌ Avoid this
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
    }
}
```

---

## Troubleshooting

### Issue: Handler Not Found

**Problem:** `InvalidOperationException: No handler registered for command/query/event`

**Solution:**
1. Ensure assembly is scanned: `.ScanAssemblies(typeof(Program).Assembly)`
2. Verify handler implements correct interface
3. Check handler class is public and not abstract
4. Ensure AddFlow() is called before BuildApp()

### Issue: Scoped Service in Handler

**Problem:** `Cannot resolve scoped service from root provider`

**Solution:**
- Handlers are registered as Scoped by default
- Dispatcher automatically creates a scope when executing handlers
- Simply inject scoped services (repositories, DbContext) directly in handler constructors
- No need to manually manage scopes with IServiceScopeFactory

### Issue: Exception from handler not propagating to caller

**Problem:** An exception thrown inside a handler or `.TapAsync()` step does not reach the test / caller — `ExecuteAsync()` returns `Result.Failure` instead of throwing.

**Cause:** This is intentional. The Dispatcher captures all exceptions in `CommandResult/QueryResult.Failure()`. The pipeline is designed for predictable results, not for exception-based control flow.

**Solution — assert on result state:**
```csharp
var result = await Pipeline
    .Start(command)
    .Process<CreateOrderCommand, Guid>()
    .ExecuteAsync();

result.IsFailure.Should().BeTrue();
result.Exception.Should().BeOfType<ValidationException>();
result.ErrorMessage.Should().NotBeEmpty();
```

**Solution — force propagation for specific types:**
```csharp
// In service configuration (Myth.Flow)
services.AddFlow(config => config
    .UseExceptionFilter<ValidationException>()
    .UseExceptionFilter<UnauthorizedAccessException>());
```
Registered types bypass the catch-all. Inside a `Transform`/`Process`/`Query` step they surface as `PipelineException` with the original exception as `InnerException`; inside plain `.Step()` / `.TapAsync()` they propagate as the raw type.

### Issue: Cache Not Working

**Problem:** Cache always returns MISS

**Solution:**
1. Ensure caching is configured: `.UseCaching()`
2. Set `CacheOptions.Enabled = true`
3. Verify cache provider is registered (Memory or Redis)
4. Check cache key is consistent between calls

### Issue: Events Not Being Handled

**Problem:** Event published but handlers not invoked

**Solution:**
1. Enable auto-subscription: `.AutoSubscribeEventHandlers()`
2. Or manually subscribe: `eventBus.Subscribe<TEvent, THandler>()`
3. Verify event handlers are discovered in scanned assemblies
4. Check message broker is started for cross-service events

### Issue: Circuit Breaker Always Open

**Problem:** Circuit breaker stays open indefinitely

**Solution:**
1. Check `openDuration` is not too long
2. Verify underlying service is healthy
3. Monitor failure count and threshold
4. Use circuit breaker state monitoring

### Issue: Dead Letter Queue Full

**Problem:** Messages not being enqueued

**Solution:**
1. Increase `maxSize` in DeadLetterQueue constructor
2. Process failed messages regularly
3. Implement retry logic for failed messages
4. Monitor and alert on DLQ size

### Issue: RabbitMQ/Kafka Connection Fails

**Problem:** `MessageBrokerException: Connection failed`

**Solution:**
1. Verify broker connection string and credentials
2. Check broker is running and accessible
3. Validate network connectivity and firewall rules
4. Review broker-specific configuration (exchange, topic, etc.)

---

## Integration with Other Myth Libraries

### With Myth.Flow

```csharp
// Combine pipelines with CQRS
return await Pipeline.Start(command, serviceProvider)
    .StepResultAsync<IDispatcher>((dispatcher, cmd) =>
        dispatcher.DispatchCommandAsync<TCommand, TResult>(cmd))
    .ExecuteAsync();
```

### With Myth.Guard

```csharp
// Validate commands before handling
return await Pipeline.Start(command, serviceProvider)
    .StepResultAsync<IValidator>((validator, cmd) =>
        validator.ValidateAsync(cmd, ValidationContextKey.Create))
    .StepResultAsync<IDispatcher>((dispatcher, cmd) =>
        dispatcher.DispatchCommandAsync(cmd))
    .ExecuteAsync();
```

### With Myth.Repository

```csharp
// Handlers inject repositories directly
public class GetUsersHandler : IQueryHandler<GetUsersQuery, List<UserDto>> {
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository) {
        _userRepository = userRepository;
    }

    public async Task<QueryResult<List<UserDto>>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken) {
        var spec = SpecBuilder<User>.Create()
            .IsActive()
            .Order(u => u.Name);

        var users = await _userRepository.ListAsync(spec, cancellationToken);
        var dtos = users.Select(u => u.To<UserDto>()).ToList();

        return QueryResult<List<UserDto>>.Success(dtos);
    }
}
```

---

## Summary

Myth.Flow.Actions provides a complete CQRS and Event-Driven Architecture implementation with:

- **Separation of Concerns**: Commands, Queries, and Events are clearly separated
- **Resilience**: Circuit breaker and dead letter queue for fault tolerance
- **Scalability**: Message brokers for distributed event processing
- **Performance**: Query caching with multiple provider support
- **Developer Experience**: Auto-discovery, scoped handlers, and fluent configuration
- **Integration**: Seamless integration with Myth.Flow, Myth.Guard, and Myth.Repository

Use Myth.Flow.Actions to build scalable, maintainable, and resilient applications following industry-standard patterns.
