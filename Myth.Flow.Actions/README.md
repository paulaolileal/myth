# Myth.Flow.Actions

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Flow.Actions?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Flow.Actions/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Flow.Actions?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Flow.Actions/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A powerful .NET library implementing CQRS and Event-Driven Architecture patterns with seamless integration to Myth.Flow pipelines. Built for scalability with support for multiple message brokers, caching strategies, and enterprise-grade resilience features.

## 🚀 Action-First Pipeline API

This library features a revolutionary **Action-First** approach that eliminates context boilerplate and dramatically simplifies pipeline development:

```csharp
// ❌ OLD: Context-based (lots of boilerplate)
Pipeline.Start(context)
    .Process<Context, Command>(ctx => new Command { ... }, (ctx, result) => ctx.Result = result)

// ✅ NEW: Action-First (clean and direct)
Pipeline.Start(new Command { ... }, serviceProvider)
    .Process<Command, Result>()
```

**Benefits:**
- **70% less boilerplate code** - No context classes needed
- **Type-safe transformations** - Direct object-to-object pipeline flow
- **Intuitive developer experience** - Actions ready for execution
- **Fluent cache configuration** - `x => x.UseCache("key", TimeSpan.FromMinutes(5))`
- **Utility pipelines** - Start without parameters for functional scenarios

# ⭐ Features

- **Action-First API**: Revolutionary pipeline approach with zero context boilerplate
- **CQRS Pattern**: Clean separation of Commands, Queries, and Events
- **Pipeline Integration**: Fluent integration with Myth.Flow for composable workflows
- **Multiple Message Brokers**: InMemory (dev/test), Kafka, and RabbitMQ support
- **Query Caching**: Built-in caching with Memory and Redis providers with fluent configuration
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

// New fluent API (recommended)
builder.Services.AddFlow(config => config
    .UseTelemetry()                                  // Enable pipeline telemetry
    .UseLogging()                                    // Enable pipeline logging
    .UseRetry(attempts: 3, backoffMs: 100)           // Default retry policy
    .UseExceptionFilter<ArgumentException>()         // Propagate ArgumentException
    .UseExceptionFilter<InvalidOperationException>() // Propagate InvalidOperationException
    .UseActions(actions => actions
        .UseInMemory()                               // InMemory message broker
        .EnableCaching()                             // Enable query caching
        .ScanAssemblies(typeof(Program).Assembly)    // Auto-discover handlers
    )
);

// Alternative: Legacy configuration style
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

## 3. Use Action-First Pipeline

### Simple Pipeline Example

```csharp
using Myth.Flow.Actions;

// Direct action execution - no context needed!
var result = await Pipeline
    .Start(new CreateUserCommand { Email = "user@example.com", Name = "John Doe" }, serviceProvider)
    .Process<CreateUserCommand, Guid>()
    .ExecuteAsync();

if (result.IsSuccess)
{
    Console.WriteLine($"User created with ID: {result.Value}");
}
```

### Complex Workflow Example

```csharp
// Chain operations with transformations
var result = await Pipeline
    .Start(new CreateUserCommand { Email = "user@example.com", Name = "John Doe" }, serviceProvider)
    .Process<CreateUserCommand, Guid>()                                        // Command → Guid
    .Transform(userId => new GetUserQuery { UserId = userId })                 // Guid → Query
    .Query<GetUserQuery, UserDto>(x => x.UseCache($"user:{userId}", TimeSpan.FromMinutes(10))) // Query with cache
    .Transform(user => new UserCreatedEvent { UserId = user.Id, Email = user.Email })          // User → Event
    .Publish<UserCreatedEvent>()                                               // Publish event
    .ExecuteAsync();

if (result.IsSuccess)
{
    Console.WriteLine("User creation workflow completed successfully!");
}
```

### Utility Pipeline (Empty Start)

```csharp
// Start without initial data for utility functions
var result = await PipelineExtensions
    .Start(serviceProvider)
    .Transform(() => new GetActiveUsersQuery())
    .Query<GetActiveUsersQuery, List<UserDto>>()
    .Transform(users => new GenerateReportCommand { Users = users })
    .Process<GenerateReportCommand, ReportDto>()
    .ExecuteAsync();
```

## 🛠️ Intermediate Pipeline Steps

The Action-First API now supports all Myth.Flow pipeline methods for adding custom logic, validation, telemetry, and resilience patterns **between** action operations:

### Validation and Custom Steps

```csharp
public class UserCommandPipeline
{
    private readonly IValidationService _validationService;
    private readonly IUserService _userService;

    public UserCommandPipeline(
        IValidationService validationService,
        IUserService userService)
    {
        _validationService = validationService;
        _userService = userService;
    }

    public async Task<Result<Guid>> CreateUserAsync(CreateUserCommand command)
    {
        var result = await Pipeline
            .Start(command, serviceProvider)
            // Validate input before processing
            .Step(state => {
                _validationService.ValidateEmail(state.CurrentRequest!.Email);
                return state;
            })
            // Add custom business logic
            .StepAsync(async state => {
                await _userService.CheckUserLimitsAsync(state.CurrentRequest!.Email);
                return state;
            })
            .Process<CreateUserCommand, Guid>()
            .ExecuteAsync();

        return result;
    }
}
```

### Side Effects and Logging

```csharp
public class UserQueryPipeline
{
    private readonly ILogger<UserQueryPipeline> _logger;
    private readonly IMetricsService _metricsService;

    public UserQueryPipeline(
        ILogger<UserQueryPipeline> logger,
        IMetricsService metricsService)
    {
        _logger = logger;
        _metricsService = metricsService;
    }

    public async Task<Result<UserDto>> GetUserAsync(Guid userId)
    {
        var result = await Pipeline
            .Start(new GetUserQuery { UserId = userId }, serviceProvider)
            // Log the start of the operation
            .Tap(state =>
                _logger.LogInformation("Querying user {UserId}", state.CurrentRequest!.UserId))
            .Query<GetUserQuery, UserDto>()
            // Log after successful query
            .TapAsync(async state =>
                await _metricsService.RecordQueryExecutionAsync("GetUser"))
            .ExecuteAsync();

        return result;
    }
}
```

### Conditional Execution

```csharp
public class OrderCommandPipeline
{
    private readonly IPaymentService _paymentService;

    public OrderCommandPipeline(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Result<OrderResult>> ProcessOrderAsync(ProcessOrderCommand command)
    {
        var result = await Pipeline
            .Start(command, serviceProvider)
            // Only validate payment if order requires it
            .When(state => state.CurrentRequest!.RequiresPayment, builder =>
                builder.StepAsync(state =>
                    _paymentService.ValidatePaymentMethodAsync(state.CurrentRequest!.PaymentInfo)))
            .Process<ProcessOrderCommand, OrderResult>()
            .ExecuteAsync();

        return result;
    }
}
```

### Resilience and Telemetry

```csharp
var result = await Pipeline
    .Start(new CallExternalApiQuery { Endpoint = "users" }, serviceProvider)
    // Add telemetry tracking
    .WithTelemetry("ExternalApiCall")
    // Configure retry policy for external calls
    .WithRetry(maxAttempts: 3, backoffMs: 1000)
    .Query<CallExternalApiQuery, ApiResponse>()
    .ExecuteAsync();
```

### Complex Workflows with Multiple Steps

```csharp
public class OrderWorkflowPipeline
{
    private readonly ICustomerService _customerService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<OrderWorkflowPipeline> _logger;
    private readonly INotificationService _notificationService;

    public OrderWorkflowPipeline(
        ICustomerService customerService,
        IInventoryService inventoryService,
        ILogger<OrderWorkflowPipeline> logger,
        INotificationService notificationService)
    {
        _customerService = customerService;
        _inventoryService = inventoryService;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<Result<OrderResult>> CreateOrderAsync(
        Guid customerId,
        List<OrderItem> items)
    {
        var result = await Pipeline
            .Start(new CreateOrderCommand { CustomerId = customerId, Items = items }, serviceProvider)
            // Validate customer
            .StepAsync(async state => {
                await _customerService.ValidateCustomerAsync(state.CurrentRequest!.CustomerId);
                return state;
            })
            // Check inventory
            .StepAsync(async state => {
                await _inventoryService.ReserveItemsAsync(state.CurrentRequest!.Items);
                return state;
            })
            // Log before processing
            .Tap(state =>
                _logger.LogInformation("Processing order for customer {CustomerId}",
                    state.CurrentRequest!.CustomerId))
            // Process the order
            .Process<CreateOrderCommand, OrderResult>()
            // Send notification after success
            .TapAsync(async state =>
                await _notificationService.SendOrderConfirmationAsync(state.CurrentRequest!))
            .ExecuteAsync();

        return result;
    }
}
```

### Available Intermediate Methods

- **`Step()`** - Synchronous operations (use constructor-injected dependencies)
- **`StepAsync()`** - Asynchronous operations (use constructor-injected dependencies)
- **`StepResult()`** - Operations returning `Result<T>` for error handling
- **`StepResultAsync()`** - Async operations returning `Result<T>`
- **`Tap()`** - Side effects (logging, metrics, events) using injected services
- **`TapAsync()`** - Async side effects using injected services
- **`When(predicate, configure)`** - Conditional pipeline execution
- **`WithRetry(maxAttempts, backoffMs)`** - Retry policies with exponential backoff
- **`WithTelemetry(operationName)`** - OpenTelemetry distributed tracing

All methods maintain the fluent API design and can be chained together for complex workflows while preserving type safety and the Action-First approach. Use constructor injection to provide dependencies to your pipeline class, then reference those dependencies in the pipeline steps.

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

# 📚 Action-First Pipeline API

## Start Pipeline

```csharp
// Start with a request object
PipelineExtensions.Start(command, serviceProvider)
PipelineExtensions.Start(query, serviceProvider)
PipelineExtensions.Start(event, serviceProvider)

// Start without initial data (for utility functions)
PipelineExtensions.Start(serviceProvider)
```

## Process (Commands)

```csharp
// Command without response (when TCommand : ICommand)
.Process<TCommand>()

// Command with typed response (when TCommand : ICommand<TResponse>)
.Process<TCommand, TResponse>()
```

## Query (Read Operations)

```csharp
// Query without caching
.Query<TQuery, TResponse>()

// Query with cache configuration using fluent API
.Query<TQuery, TResponse>(x => x
    .UseCache("cache-key", TimeSpan.FromMinutes(10))
    .WithSlidingExpiration())

// Query with simple cache configuration
.Query<TQuery, TResponse>(x => x.UseCache($"key:{someId}", TimeSpan.FromMinutes(5)))
```

## Publish (Events)

```csharp
// Publish event (when TEvent : IEvent)
.Publish<TEvent>()
```

## Transform

```csharp
// Transform current request to new type
.Transform<TNext>(current => new TNext { /* ... */ })

// Async transformation
.TransformAsync<TNext>(async current => await CreateNextAsync(current))

// Conditional transformation
.TransformIf<TNext>(
    condition: current => current.IsValid,
    transform: current => new TNext { /* ... */ })

// Conditional with true/false branches
.TransformIf<TNext>(
    condition: current => current.Type == "Premium",
    transformTrue: current => new PremiumAction { /* ... */ },
    transformFalse: current => new StandardAction { /* ... */ })

// Empty pipeline transformation (when starting without data)
.Transform<TRequest>(() => new TRequest { /* ... */ })
.TransformAsync<TRequest>(async () => await CreateRequestAsync())
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

## High-Value Order Processing

```csharp
// Action-first pipeline with conditional logic using TransformIf
var result = await PipelineExtensions
    .Start(new ValidateOrderCommand { OrderId = orderId }, serviceProvider)
    .Process<ValidateOrderCommand, OrderDto>()
    .TransformIf<FraudCheckCommand>(
        order => order.TotalAmount > 1000,
        order => new FraudCheckCommand { OrderId = order.Id })
    .Process<FraudCheckCommand>()
    .Transform(fraudResult => new ProcessPaymentCommand { OrderId = orderId })
    .Process<ProcessPaymentCommand>()
    .Transform(paymentResult => new OrderCompletedEvent { OrderId = orderId })
    .Publish<OrderCompletedEvent>()
    .ExecuteAsync();
```

## Order to Shipment Workflow

```csharp
// Direct transformations between different action types
var result = await PipelineExtensions
    .Start(new CreateOrderCommand
    {
        Items = items,
        CustomerId = customerId,
        ShippingAddress = address
    }, serviceProvider)
    .Process<CreateOrderCommand, Guid>()                           // Create order → OrderId
    .Transform(orderId => new GetOrderQuery { OrderId = orderId }) // OrderId → Query
    .Query<GetOrderQuery, OrderDto>()                              // Get full order details
    .Transform(order => new CreateShipmentCommand                  // Order → Shipment command
    {
        OrderId = order.Id,
        ShipmentId = Guid.NewGuid(),
        Address = order.ShippingAddress,
        Items = order.Items
    })
    .Process<CreateShipmentCommand, ShipmentDto>()                 // Process shipment
    .Transform(shipment => new ShipmentCreatedEvent               // Shipment → Event
    {
        OrderId = shipment.OrderId,
        ShipmentId = shipment.Id,
        TrackingNumber = shipment.TrackingNumber
    })
    .Publish<ShipmentCreatedEvent>()                              // Notify about shipment
    .ExecuteAsync();
```

## Report Generation Pipeline

```csharp
// Utility pipeline starting without initial data
var result = await PipelineExtensions
    .Start(serviceProvider)
    .Transform(() => new GetMonthlyOrdersQuery { Month = DateTime.Now.Month })
    .Query<GetMonthlyOrdersQuery, List<OrderDto>>(x => x.UseCache("monthly-orders", TimeSpan.FromHours(1)))
    .Transform(orders => new GenerateReportCommand
    {
        Orders = orders,
        ReportType = ReportType.Monthly,
        GeneratedBy = currentUserId
    })
    .Process<GenerateReportCommand, ReportDto>()
    .Transform(report => new ReportGeneratedEvent
    {
        ReportId = report.Id,
        GeneratedBy = currentUserId
    })
    .Publish<ReportGeneratedEvent>()
    .ExecuteAsync();
```

# 🧪 Testing

## Testing Action-First Pipelines

```csharp
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Flow.Actions.Extensions;

public class UserPipelineTests
{
    private readonly IServiceProvider _serviceProvider;

    public UserPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlow();
        services.AddFlowActions(config =>
        {
            config.UseInMemory()
                   .EnableCaching(cache => cache.ProviderType = CacheProviderType.Memory)
                   .ScanAssemblies(typeof(CreateUserCommand).Assembly);
        });

        services.AddScoped<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<IEmailService, FakeEmailService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task CreateUser_WithActionFirstAPI_ShouldSucceed()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act - Using action-first API
        var result = await PipelineExtensions
            .Start(command, _serviceProvider)
            .Process<CreateUserCommand, Guid>()
            .ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CompleteUserWorkflow_ShouldChainOperations()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "workflow@example.com",
            Name = "Workflow User"
        };

        // Act - Chain multiple operations
        var result = await PipelineExtensions
            .Start(command, _serviceProvider)
            .Process<CreateUserCommand, Guid>()                                        // Create user
            .Transform(userId => new GetUserQuery { UserId = userId })                 // Transform to query
            .Query<GetUserQuery, UserDto>(x => x.UseCache($"user:{userId}", TimeSpan.FromMinutes(5))) // Get user with cache
            .Transform(user => new UserCreatedEvent { UserId = user.Id, Email = user.Email })          // Transform to event
            .Publish<UserCreatedEvent>()                                               // Publish event
            .ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task EmptyPipeline_WithTransforms_ShouldWork()
    {
        // Act - Start without initial data
        var result = await PipelineExtensions
            .Start(_serviceProvider)
            .Transform(() => new GetActiveUsersQuery())
            .Query<GetActiveUsersQuery, List<UserDto>>()
            .ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task ConditionalWorkflow_ShouldExecuteBasedOnCondition()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "premium@example.com",
            Name = "Premium User"
        };

        // Act - Conditional transformation
        var result = await PipelineExtensions
            .Start(command, _serviceProvider)
            .Process<CreateUserCommand, Guid>()
            .Transform(userId => new GetUserQuery { UserId = userId })
            .Query<GetUserQuery, UserDto>()
            .TransformIf<SendWelcomeEmailCommand>(
                user => user.Email.Contains("premium"),
                user => new SendWelcomeEmailCommand { Email = user.Email, IsPremium = true })
            .Process<SendWelcomeEmailCommand>()
            .ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}

## Testing Individual Handlers

```csharp
public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var handler = new CreateUserCommandHandler(repository);
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);
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
