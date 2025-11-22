<img  style="float: right;" src="myth-flow-actions-logo.png" alt="drawing" width="250"/>

# Myth.Flow.Actions

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Flow.Actions?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Flow.Actions/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Flow.Actions?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Flow.Actions/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A powerful .NET library implementing CQRS and Event-Driven Architecture patterns with seamless integration to Myth.Flow pipelines. Built for scalability with support for multiple message brokers, caching strategies, and enterprise-grade resilience features.

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

#### User-Controlled Caching via HTTP Headers

```csharp
[HttpGet( "{userId}" )]
public async Task<IActionResult> GetUser(
    Guid userId,
    [FromHeader] CacheControl? cacheControl ) {

    var query = new GetUserQuery { UserId = userId };

    var result = await _dispatcher.DispatchQueryAsync<GetUserQuery, UserDto>(
        query,
        cacheOptions: null );

    if ( result.IsSuccess )
        return Ok( result.Data ); // Headers applied automatically by Dispatcher

    return NotFound( );
}
```

> **Note**: HTTP cache headers are automatically applied when cache metadata is present - no additional configuration required!

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

### HTTP Cache-Control Integration

Myth.Flow.Actions provides seamless integration with HTTP Cache-Control headers using type-safe constants from Myth.Commons.

#### Built-in Cache Directives

```csharp
using Myth.ValueObjects;

// Available cache directives (type-safe constants)
CacheControl.NoCache       // no-cache directive
CacheControl.NoStore       // no-store directive
CacheControl.Public        // public directive
CacheControl.Private       // private directive
CacheControl.MustRevalidate // must-revalidate directive
CacheControl.MaxAge        // max-age directive
CacheControl.SMaxAge       // s-maxage directive
```

#### User-Controlled Caching in Controllers

Enable users to control caching via HTTP headers using `[FromHeader]` attribute binding:

```csharp
[HttpGet( "{id}" )]
public async Task<IActionResult> GetProduct(
    Guid id,
    [FromHeader] CacheControl? cacheControl ) {

    var query = new GetProductQuery { ProductId = id };

    var result = await _dispatcher.DispatchQueryAsync<GetProductQuery, ProductDto>(
        query,
        cacheOptions: cacheControl?.ToCacheOptions( ) );

    if ( result.IsSuccess )
        return Ok( result.Data ); // Headers applied automatically by Dispatcher

    return NotFound( );
}
```

#### Fluent Cache Configuration

Configure cache behavior using the fluent `.UseCache()` API:

```csharp
.Query<GetProductQuery, ProductDto>( ( query, cache ) => cache
    .UseCache( cacheControl ) // Use user-provided cache control
    .WithKey( $"product:{query.ProductId}" )
    .WithTtl( TimeSpan.FromMinutes( 15 ))
    .WithETag( product => $"\"{product.Id}-{product.UpdatedAt.Ticks}\"" )
    .WithVary( "Accept-Language", "User-Agent" ))
```

#### Cache Policy Methods

Use built-in cache policy methods for common scenarios:

```csharp
// Public cache with max-age
.UseCache( cache => cache.Public( TimeSpan.FromMinutes( 30 )))

// Private cache with max-age
.UseCache( cache => cache.Private( TimeSpan.FromMinutes( 10 )))

// Disable caching
.UseCache( cache => cache.NoCache( ))

// Immutable cache for static content
.UseCache( cache => cache.Immutable( TimeSpan.FromDays( 365 )))
```

#### Automatic HTTP Header Application

HTTP cache headers are automatically applied when cache metadata is present:

```csharp
// Headers automatically added to response:
// Cache-Control: public, max-age=1800
// ETag: "12345-637891234567890"
// Expires: Thu, 01 Jan 2025 12:30:00 GMT
// Vary: Accept-Language, User-Agent
// Age: 45

var result = await _dispatcher.DispatchQueryAsync<GetUserQuery, UserDto>( query );
if ( result.IsSuccess ) {
    return Ok( result.Data ); // Headers applied automatically by Dispatcher
}
```

> **Important**: HTTP cache headers are applied automatically by the Dispatcher when cache metadata is present. No manual header application is needed in controllers.

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

## Swagger Integration

### CacheControl Enum Display

For better developer experience in Swagger UI, you can configure `CacheControl` to display as a dropdown with predefined values.

**✅ Compatible with all Swashbuckle versions (6.x, 7.x, 8.x+)** - Safe fallback handling ensures your application won't crash due to version mismatches.

```csharp
using Myth.Flow.Actions.Extensions;

// In Program.cs or Startup.cs
builder.Services.AddSwaggerGen(options => {
    options.AddFlowActionsSchemaFilters(); // Adds all Flow.Actions filters

    // Or add only the CacheControl filter
    options.AddCacheControlSchemaFilter();
});
```

This configuration makes `CacheControl` parameters appear as a dropdown in Swagger UI with common cache directives:
- `no-cache`
- `no-store`
- `public`
- `private`
- `must-revalidate`
- `proxy-revalidate`
- `no-transform`
- `immutable`
- `max-age=3600`
- `public, max-age=1800`
- `private, max-age=300`
- `public, immutable, max-age=31536000`

### Usage in Controllers

```csharp
[HttpGet("users")]
public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers(
    [FromHeader("Cache-Control")] CacheControl? cacheControl = null) {

    var query = new GetUsersQuery();

    var result = await _dispatcher.DispatchQueryAsync(query, cache => {
        if (cacheControl != null) {
            cache.UseCache(cacheControl);
        } else {
            cache.Public(TimeSpan.FromMinutes(5));
        }
    });

    return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
}
```

The model binder automatically parses the `Cache-Control` header and validates the syntax, providing type-safe access to cache directives in your controllers.

## Contributing

Contributions are welcome! Please follow the existing code style and add tests for new features.

## License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.

## Related Projects

- [Myth.Flow](../Myth.Flow/README.md) - Core pipeline orchestration framework
- [Myth.Commons](../Myth.Commons/README.md) - Common utilities and extensions
- [Myth.Repository](../Myth.Repository/README.md) - Repository pattern implementation
