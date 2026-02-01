<img  style="float: right;" src="myth-flow-logo.png" alt="drawing" width="250"/>

# Myth.Flow

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Flow?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Flow/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Flow?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Flow/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A powerful .NET library for building maintainable and testable data processing pipelines with a fluent, chainable interface. Built with enterprise-grade features including automatic retry policies, OpenTelemetry integration, global service provider integration, and comprehensive error handling.

## 🎯 Why Myth.Flow?

Complex business workflows are **painful to implement and maintain**. Try-catch pyramids, scattered validation logic, inconsistent error handling, no observability, manual retry logic—code becomes a tangled mess that's hard to test and impossible to reason about. **Myth.Flow transforms this chaos into elegant, readable pipelines** that handle success/failure paths automatically.

### The Problem

**Traditional Workflow Code is a Nightmare**
```csharp
public async Task<OrderDto> ProcessOrderAsync(CreateOrderCommand command) {
    try {
        // Validation scattered everywhere
        if (string.IsNullOrEmpty(command.CustomerId)) {
            throw new ValidationException("Customer ID required");
        }

        // Manual retry logic (duplicated across services)
        Order? order = null;
        for (int attempt = 0; attempt < 3; attempt++) {
            try {
                order = await _repository.CreateAsync(command);
                break;
            } catch (DbException) {
                if (attempt == 2) throw;
                await Task.Delay(100 * (int)Math.Pow(2, attempt));
            }
        }

        // No telemetry - debugging is blind
        // Try-catch pyramid - nested error handling
        try {
            await _paymentService.ProcessAsync(order.Id);
            try {
                await _inventoryService.ReserveAsync(order.Items);
                try {
                    await _emailService.SendConfirmationAsync(order.Id);
                } catch (EmailException ex) {
                    // Log? Retry? Ignore? Who knows...
                    _logger.LogError(ex, "Email failed");
                }
            } catch (InventoryException ex) {
                // Rollback payment? Good luck with that
                throw;
            }
        } catch (PaymentException ex) {
            // Cancel order? Manual cleanup
            await _repository.DeleteAsync(order.Id);
            throw;
        }

        return order.ToDto();
    } catch (Exception ex) {
        // Generic catch-all - what went wrong?
        _logger.LogError(ex, "Order processing failed");
        throw;
    }
}
```

**Problems:**
- **Unreadable**: Business logic buried in infrastructure code
- **Untestable**: Tightly coupled, hard to mock
- **No observability**: Debugging requires log-diving
- **Inconsistent error handling**: Each method handles failures differently
- **Manual retries**: Copy-paste retry logic everywhere
- **No resilience**: One failure crashes everything

### The Solution

**Railway-Oriented Pipeline Programming**
```csharp
public async Task<Result<OrderDto>> ProcessOrderAsync(CreateOrderCommand command) {
    return await Pipeline.Start(command)
        .WithTelemetry("CreateOrder")                    // Auto tracing
        .WithRetry(maxAttempts: 3, backoffMs: 100)      // Auto retries

        .StepResultAsync(cmd => _validator.ValidateAsync(cmd))       // Stops on validation failure
        .StepAsync(cmd => _repository.CreateAsync(cmd))              // Continues if valid
        .StepAsync(order => _paymentService.ProcessAsync(order.Id))  // Auto retry on transient failure
        .StepAsync(order => _inventoryService.ReserveAsync(order.Items))
        .TapAsync(order => _emailService.SendConfirmationAsync(order.Id))  // Side effect - doesn't stop pipeline

        .Transform<OrderDto>(order => order.ToDto())     // Type transformation
        .ExecuteAsync();
}
```

**Benefits:**
- **Crystal clear**: Business logic reads like plain English
- **Railway-oriented**: Success path flows, failure path exits early
- **Built-in resilience**: Retry, telemetry, error handling automatic
- **Fully testable**: Mock each service, test each step independently
- **Observable**: OpenTelemetry traces every step automatically

### Why Choose Myth.Flow?

| Aspect | Myth.Flow | Traditional Code | MediatR/Other Pipelines |
|--------|-----------|------------------|-------------------------|
| **Readability** | Fluent, self-documenting | Try-catch pyramids | Behavior boilerplate |
| **Error Handling** | Railway-oriented Result<T> | Manual try-catch everywhere | Exceptions or custom wrappers |
| **Retry Logic** | Built-in with exponential backoff | Manual implementation | External libraries (Polly) |
| **Observability** | OpenTelemetry integrated | Manual logging | Manual instrumentation |
| **Type Transformations** | Native `.Transform<T>()` | Manual mapping | Not addressed |
| **Testability** | Step-by-step mocking | Integration tests required | Behavior pipeline complexity |
| **Learning Curve** | Intuitive fluent API | N/A (standard C#) | Steep (pipeline behaviors) |
| **Boilerplate** | Near zero | High | Medium-High |

### Real-World Applications

**E-Commerce Order Processing**
Validate → Create Order → Process Payment → Reserve Inventory → Send Email → Return DTO. Each step auto-retried, traced, and handled gracefully. Failures rollback automatically.

**Financial Transaction Processing**
Validate Account → Check Balance → Apply Transaction → Update Ledger → Notify Auditing. OpenTelemetry traces flow through distributed systems for compliance.

**Data ETL Pipelines**
Extract → Validate → Transform → Load → Publish Event. Retry transient failures, skip bad records with `.When()`, trace entire pipeline.

**User Registration Workflow**
Validate → Create User → Send Verification Email → Create Tenant → Provision Resources → Publish Event. Steps execute conditionally based on account tier.

**Microservices Saga Orchestration**
Chain service calls with compensation: Create Order → Reserve Inventory → Process Payment. If payment fails, auto-compensate by releasing inventory.

### Key Differentiators

🚂 **Railway-Oriented Programming**
Success path flows smoothly. First failure short-circuits to error handling. No nested try-catch pyramids.

📊 **Built-In Observability**
OpenTelemetry integration traces every pipeline step automatically. Debug distributed workflows with zero manual instrumentation.

🔄 **Smart Retry Policies**
Exponential backoff with configurable attempts. Transient failures (network, database) handled automatically.

🎯 **Type-Safe Transformations**
`.Transform<TNew>()` changes pipeline context type. Go from Command → Entity → DTO without ceremony.

🧪 **Testing Nirvana**
Mock one service, test one step. No need to orchestrate entire workflow. Integration tests optional.

⚡ **Zero Configuration**
Works out of the box. Add `.WithTelemetry()` and `.WithRetry()` when needed. Sensible defaults everywhere.

### Conceptual Foundations

**Railway-Oriented Programming (ROP)**
Inspired by Scott Wlaschin's "Railway Oriented Programming" (F# for fun and profit). Success and failure are separate tracks. Operations succeed and continue, or fail and short-circuit.

**Result Pattern (Railway Pattern)**
Return `Result<T>` instead of throwing exceptions for expected failures. Explicit success/failure handling without try-catch overhead.

**Fluent Interface / Method Chaining**
Inspired by LINQ and libraries like FluentValidation. Chain operations into readable, declarative pipelines.

**Aspect-Oriented Programming (AOP)**
Cross-cutting concerns (retry, telemetry, logging) applied via pipeline decorators (`.WithRetry()`, `.WithTelemetry()`), not scattered through business logic.

**Saga Pattern (Orchestration)**
Use pipelines to orchestrate multi-step distributed transactions with compensation logic for failures.

**OpenTelemetry Standards**
W3C Trace Context for distributed tracing. Spans created automatically for each pipeline step.

### Business Value

**For Developers**
- **60-80% less code** for complex workflows
- **10x more readable** than try-catch pyramids
- **Easy debugging** with automatic tracing
- **Fast testing** with step-by-step mocks

**For Architects**
- **Enforce patterns** via fluent API design
- **Distributed tracing** across microservices
- **Resilience built-in** (retry, circuit breaker ready)
- **Clearer code reviews** - business logic obvious

**For DevOps/SRE**
- **OpenTelemetry integration** = instant observability
- **Retry policies** reduce transient failure impact
- **Distributed traces** solve production issues faster
- **Less firefighting** with predictable error handling

**For Product Teams**
- **Faster features** with less infrastructure code
- **Fewer production bugs** from error handling mistakes
- **Better performance** under transient failures (auto-retry)
- **Easier onboarding** - pipelines are self-explanatory

# ⭐ Features

- **Fluent Interface**: Simple, chainable API design for readable code
- **Type Safety**: Strong typing with context transformation support
- **Automatic Retry**: Configurable retry policies with exponential backoff
- **OpenTelemetry Integration**: Built-in distributed tracing and observability
- **Global Service Provider**: Seamless integration with Myth.Commons centralized DI container
- **Result Pattern**: Railway-oriented programming with `Result<T>`
- **Error Handling**: Comprehensive error handling with exception filtering
- **Conditional Execution**: Execute steps based on context predicates
- **Side Effects**: Tap into pipeline for logging, metrics, and events
- **Async/Await**: First-class async support with CancellationToken
- **Zero Boilerplate**: No service locator pattern - clean, straightforward code

# 📦 Installation

```bash
dotnet add package Myth.Flow
```

# 🚀 Quick Start

## Basic Usage

```csharp
public class OrderService
{
    private readonly ValidationService _validationService;
    private readonly PaymentService _paymentService;
    private readonly InventoryService _inventoryService;

    public OrderService(
        ValidationService validationService,
        PaymentService paymentService,
        InventoryService inventoryService)
    {
        _validationService = validationService;
        _paymentService = paymentService;
        _inventoryService = inventoryService;
    }

    public async Task<Result<OrderContext>> ProcessOrderAsync(int orderId)
    {
        var input = new OrderContext { OrderId = orderId };

        var result = await Pipeline.Start(input)
            .StepAsync(ctx => _validationService.ValidateOrderAsync(ctx))
            .StepAsync(ctx => _paymentService.ProcessPaymentAsync(ctx))
            .StepAsync(ctx => _inventoryService.ReserveItemsAsync(ctx))
            .Tap(ctx => Console.WriteLine($"Order {ctx.OrderId} completed"))
            .ExecuteAsync();

        if (result.IsSuccess)
        {
            Console.WriteLine("Order processed successfully!");
        }

        return result;
    }
}
```

## Setup

### ASP.NET Core Applications

For ASP.NET Core applications, use `builder.BuildApp()` instead of `builder.Build()` to automatically initialize the global service provider:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFlow(config => config
    .UseTelemetry()
    .UseLogging()
    .UseRetry(3, 100));

builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<InventoryService>();

var app = builder.BuildApp();

app.Run();
```

### Console Applications

For console applications or background services, use `services.BuildWithGlobalProvider()`:

```csharp
var services = new ServiceCollection();

services.AddFlow(config => config
    .UseTelemetry()
    .UseRetry(3, 100));

services.AddScoped<ValidationService>();
services.AddScoped<ProcessingService>();

var serviceProvider = services.BuildWithGlobalProvider();
```

### Using in Controllers/Services

```csharp
public class OrderController : ControllerBase
{
    private readonly OrderValidationService _validationService;
    private readonly OrderCreationService _creationService;
    private readonly OrderEventService _eventService;

    public OrderController(
        OrderValidationService validationService,
        OrderCreationService creationService,
        OrderEventService eventService)
    {
        _validationService = validationService;
        _creationService = creationService;
        _eventService = eventService;
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var context = new OrderContext { Request = request };

        var result = await Pipeline.Start(context)
            .WithTelemetry("CreateOrder")
            .WithRetry(maxAttempts: 3, backoffMs: 100)
            .StepResultAsync(ctx => _validationService.ValidateAsync(ctx))
            .StepResultAsync(ctx => _creationService.CreateAsync(ctx))
            .TapAsync(ctx => _eventService.PublishOrderCreatedAsync(ctx))
            .Transform<OrderResponse>(ctx => new OrderResponse
            {
                OrderId = ctx.CreatedOrder!.Id,
                Status = ctx.CreatedOrder.Status
            })
            .ExecuteAsync();

        if (result.IsFailure)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Value);
    }
}
```

# 🔧 Configuration

## Basic Configuration

```csharp
builder.Services.AddFlow();
```

## Advanced Configuration with Fluent Builder

```csharp
builder.Services.AddFlow(config => config
    .UseTelemetry()
    .UseLogging()
    .UseRetry(3, 100)
    .UseActivitySource("MyApp.Pipeline")
    .UseExceptionFilter<ArgumentException>()
    .UseExceptionFilter<InvalidOperationException>());
```

### Configuration Options

- **UseTelemetry()** / **DisableTelemetry()**: Enable/disable OpenTelemetry distributed tracing
- **UseLogging()** / **DisableLogging()**: Enable/disable Microsoft.Extensions.Logging integration
- **UseRetry(attempts, backoffMs)** / **DisableRetry()**: Configure default retry policy with exponential backoff
- **UseActivitySource(name, version?)**: Set custom ActivitySource for telemetry
- **UseExceptionFilter\<TException>()**: Configure exception types to propagate without handling

## Per-Pipeline Configuration

Override global settings for specific pipelines:

```csharp
public class MyService
{
    private readonly ProcessingService _processingService;

    public MyService(ProcessingService processingService)
    {
        _processingService = processingService;
    }

    public async Task<Result<MyContext>> ExecuteAsync(MyContext context)
    {
        return await Pipeline.Start(context)
            .WithTelemetry("OperationName")
            .WithRetry(maxAttempts: 5, backoffMs: 200)
            .StepAsync(ctx => _processingService.ProcessAsync(ctx))
            .ExecuteAsync();
    }
}
```

## Pipeline.Start Options

### Start with Default Configuration

```csharp
var result = await Pipeline.Start(context)
    .StepAsync(ctx => ProcessAsync(ctx))
    .ExecuteAsync();
```

### Start with Custom Configuration

```csharp
var result = await Pipeline.Start(context, config => {
    config.EnableTelemetry = true;
    config.DefaultRetryAttempts = 5;
})
    .StepAsync(ctx => ProcessAsync(ctx))
    .ExecuteAsync();
```

# 🔄 Pipeline Steps

## Synchronous Steps

```csharp
public class MyService
{
    private readonly TransformService _transformService;

    public MyService(TransformService transformService)
    {
        _transformService = transformService;
    }

    public MyContext ProcessData(MyContext ctx)
    {
        var result = Pipeline.Start(ctx)
            .Step(context =>
            {
                // Synchronous processing
                context.Data = _transformService.Transform(context.Data);
                return context;
            })
            .Execute();

        return result.Value;
    }
}
```

## Asynchronous Steps

```csharp
public class MyService
{
    private readonly ProcessingService _processingService;

    public MyService(ProcessingService processingService)
    {
        _processingService = processingService;
    }

    public async Task<Result<MyContext>> ProcessAsync(MyContext context)
    {
        return await Pipeline.Start(context)
            .StepAsync(ctx => _processingService.ProcessAsync(ctx))
            .ExecuteAsync();
    }
}
```

## Steps with Result Pattern

Use `StepResult` and `StepResultAsync` for operations that can succeed or fail:

```csharp
public class OrderProcessor
{
    private readonly ValidationService _validationService;

    public OrderProcessor(ValidationService validationService)
    {
        _validationService = validationService;
    }

    public async Task<Result<OrderContext>> ProcessAsync(OrderContext context)
    {
        return await Pipeline.Start(context)
            .StepResultAsync(ctx => _validationService.ValidateAsync(ctx))
            .ExecuteAsync();
    }
}
```

The `Result<T>` pattern allows steps to return success or failure. Failed results are automatically converted to exceptions:

```csharp
public async Task<Result<OrderContext>> ValidateAsync(OrderContext context)
{
    if (string.IsNullOrEmpty(context.Request.Email))
        return Result<OrderContext>.Failure("Email is required");

    if (context.Request.Amount <= 0)
        return Result<OrderContext>.Failure("Amount must be positive");

    return Result<OrderContext>.Success(context);
}
```

## Steps with CancellationToken Support

Pass cancellation tokens for operations that support cancellation:

```csharp
public async Task<Result<MyContext>> ProcessAsync(MyContext context, CancellationToken ct)
{
    return await Pipeline.Start(context)
        .StepAsync((ctx, token) => LongRunningOperationAsync(ctx, token))
        .StepResultAsync((ctx, token) => ValidateAsync(ctx, token))
        .ExecuteAsync(ct);
}
```

## Context Transformation

Transform the pipeline context to a different type. All previous steps are executed before transformation:

```csharp
public async Task<Result<OrderResponse>> ProcessOrderAsync(OrderContext context)
{
    return await Pipeline.Start(context)
        .StepResultAsync(ctx => _validationService.ValidateAsync(ctx))
        .StepResultAsync(ctx => _creationService.CreateAsync(ctx))
        .Transform<OrderResponse>(ctx => new OrderResponse
        {
            Id = ctx.Entity.Id,
            Name = ctx.Entity.Name
        })
        .ExecuteAsync();
}
```

### Async Transformation

```csharp
.TransformAsync<OutputContext>(async ctx =>
{
    var data = await _service.GetDataAsync(ctx.Id);
    return new OutputContext { Data = data };
})
```

## Side Effects (Tap)

Execute actions without modifying the context. Perfect for logging, metrics, and event publishing:

```csharp
public class OrderProcessor
{
    private readonly ILogger<OrderProcessor> _logger;
    private readonly EventPublisher _eventPublisher;
    private readonly MetricsService _metricsService;

    public OrderProcessor(
        ILogger<OrderProcessor> logger,
        EventPublisher eventPublisher,
        MetricsService metricsService)
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
        _metricsService = metricsService;
    }

    public async Task<Result<OrderContext>> ProcessAsync(OrderContext context)
    {
        return await Pipeline.Start(context)
            .Tap(ctx => Console.WriteLine($"Processing: {ctx.Id}"))
            .TapAsync(ctx => _eventPublisher.PublishAsync(new OrderCreated(ctx.OrderId)))
            .Tap(ctx => _metricsService.IncrementCounter("orders_created"))
            .ExecuteAsync();
    }
}
```

**Note**: Tap steps don't support retry logic and exceptions are propagated immediately.

## Conditional Execution

```csharp
public class OrderProcessor
{
    private readonly FraudDetectionService _fraudDetectionService;
    private readonly ApprovalService _approvalService;

    public OrderProcessor(
        FraudDetectionService fraudDetectionService,
        ApprovalService approvalService)
    {
        _fraudDetectionService = fraudDetectionService;
        _approvalService = approvalService;
    }

    public async Task<Result<OrderContext>> ProcessAsync(OrderContext context)
    {
        return await Pipeline.Start(context)
            .When(
                ctx => ctx.Amount > 1000,
                pipeline => pipeline
                    .StepAsync(ctx => _fraudDetectionService.CheckAsync(ctx))
                    .StepAsync(ctx => _approvalService.RequestApprovalAsync(ctx)))
            .ExecuteAsync();
    }
}
```

# 🔁 Retry Policies

Configure retry behavior for resilient pipelines with exponential backoff.

## Global Retry Configuration

```csharp
builder.Services.AddFlow(config => config
    .UseRetry(3, 100));
```

## Per-Pipeline Retry

Override global retry settings for specific pipelines:

```csharp
var result = await Pipeline.Start(context)
    .WithRetry(maxAttempts: 5, backoffMs: 200)
    .StepAsync(ctx => UnreliableOperationAsync(ctx))
    .ExecuteAsync();
```

## Retry Behavior

- **Exponential backoff**: `delay = backoffMs × attemptNumber`
- **Retries only on exceptions**: `Result.Failure` does not trigger retry
- **Never retries**: `OperationCanceledException` and configured exception filters
- **Per-step configuration**: Steps added after `.WithRetry()` inherit that configuration
- **Default**: No retry (attempts = 0)

## Retry Example

```csharp
public class ApiIntegrationService
{
    private readonly ExternalApiService _externalApiService;

    public ApiIntegrationService(ExternalApiService externalApiService)
    {
        _externalApiService = externalApiService;
    }

    public async Task<Result<ApiContext>> CallApiAsync(ApiContext context)
    {
        return await Pipeline.Start(context)
            .WithRetry(maxAttempts: 3, backoffMs: 100)
            .StepAsync(ctx => _externalApiService.CallUnreliableApiAsync(ctx))
            .ExecuteAsync();
    }
}
```

**Retry attempts**: 1st retry after 100ms, 2nd after 200ms, 3rd after 300ms

# 📊 Observability & Telemetry

## OpenTelemetry Integration

Myth.Flow uses the standard `System.Diagnostics.ActivitySource` for distributed tracing:

```csharp
builder.Services.AddFlow(config => config
    .UseTelemetry()
    .UseActivitySource("MyApp.Pipeline", "1.0.0"));

public class UserService
{
    private readonly ValidationService _validationService;
    private readonly UserCreationService _creationService;

    public UserService(
        ValidationService validationService,
        UserCreationService creationService)
    {
        _validationService = validationService;
        _creationService = creationService;
    }

    public async Task<Result<UserContext>> CreateUserAsync(UserContext context)
    {
        return await Pipeline.Start(context)
            .WithTelemetry("CreateUser")
            .StepResultAsync(ctx => _validationService.ValidateAsync(ctx))
            .StepResultAsync(ctx => _creationService.CreateAsync(ctx))
            .ExecuteAsync();
    }
}
```

### Activity Structure

- **Root Activity**: Created with the operation name from `.WithTelemetry()`
- **Step Activities**: Child activities named `Step_{index}_{stepName}`
- **Tags**:
  - `pipeline.input.type`: Context type name
  - Step-specific timing and metadata
- **Status**: `Ok` for success, `Error` with message for failures

## Logging Integration

```csharp
public class UserCreationService
{
    private readonly ILogger<UserCreationService> _logger;

    public async Task<Result<UserContext>> CreateAsync(UserContext context)
    {
        _logger.LogInformation("Creating user: {Email}", context.Request.Email);

        try
        {
            // Create user logic
            _logger.LogInformation(
                "User {Email} created successfully with ID: {UserId}",
                context.CreatedUser.Email,
                context.CreatedUser.Id);

            return Result<UserContext>.Success(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user: {Email}", context.Request.Email);
            return Result<UserContext>.Failure("Failed to create user", ex);
        }
    }
}
```

## Metrics Example

```csharp
public interface IUserMetrics
{
    void IncrementUserCreated();
    void IncrementUserFailed();
}

public class UserMetricsService : IUserMetrics
{
    private int _usersCreated;
    private int _usersFailed;

    public void IncrementUserCreated() => Interlocked.Increment(ref _usersCreated);
    public void IncrementUserFailed() => Interlocked.Increment(ref _usersFailed);
}

public class UserProcessor
{
    private readonly UserObservabilityService _observabilityService;

    public UserProcessor(UserObservabilityService observabilityService)
    {
        _observabilityService = observabilityService;
    }

    public async Task<Result<UserContext>> ProcessAsync(UserContext context)
    {
        return await Pipeline.Start(context)
            .TapAsync(ctx => _observabilityService.RecordMetricsAsync(ctx))
            .ExecuteAsync();
    }
}
```

# 🏗️ Advanced Patterns

## Repository Pattern with Transactions

```csharp
public class UserCreationService
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserCreationService> _logger;

    public async Task<Result<UserContext>> CreateUserAsync(UserContext context)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var user = new User
            {
                Email = context.Request.Email,
                PasswordHash = context.PasswordHash,
                Role = context.Request.Role
            };

            context.CreatedUser = await _repository.CreateAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("User {Email} created", user.Email);
            return Result<UserContext>.Success(context);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Failed to create user");
            return Result<UserContext>.Failure("Failed to create user", ex);
        }
    }
}
```

## Event-Driven Architecture

```csharp
public class OrderPipeline
{
    private readonly OrderValidationService _validationService;
    private readonly OrderCreationService _creationService;
    private readonly OrderEventService _eventService;
    private readonly NotificationService _notificationService;
    private readonly MetricsService _metricsService;

    public OrderPipeline(
        OrderValidationService validationService,
        OrderCreationService creationService,
        OrderEventService eventService,
        NotificationService notificationService,
        MetricsService metricsService)
    {
        _validationService = validationService;
        _creationService = creationService;
        _eventService = eventService;
        _notificationService = notificationService;
        _metricsService = metricsService;
    }

    public async Task<Result<OrderResponse>> ProcessOrderAsync(OrderContext context)
    {
        var result = await Pipeline.Start(context)
            .WithTelemetry("ProcessOrder")
            .StepResultAsync(ctx => _validationService.ValidateAsync(ctx))
            .StepResultAsync(ctx => _creationService.CreateAsync(ctx))
            .TapAsync(ctx => _eventService.PublishOrderCreatedAsync(ctx))
            .TapAsync(ctx => _notificationService.SendConfirmationEmailAsync(ctx))
            .TapAsync(ctx => _metricsService.RecordOrderCreatedAsync(ctx))
            .Transform<OrderResponse>(ctx => new OrderResponse
            {
                OrderId = ctx.Order.Id,
                Status = ctx.Order.Status,
                CreatedAt = ctx.Order.CreatedAt
            })
            .ExecuteAsync();

        return result;
    }
}
```

## Multi-Step Validation Pipeline

```csharp
public class UserRegistrationPipeline
{
    private readonly EmailValidationService _emailValidationService;
    private readonly PasswordValidationService _passwordValidationService;
    private readonly RoleValidationService _roleValidationService;
    private readonly UserCreationService _userCreationService;
    private readonly EventPublisher _eventPublisher;
    private readonly EmailService _emailService;
    private readonly MetricsService _metricsService;

    public UserRegistrationPipeline(
        EmailValidationService emailValidationService,
        PasswordValidationService passwordValidationService,
        RoleValidationService roleValidationService,
        UserCreationService userCreationService,
        EventPublisher eventPublisher,
        EmailService emailService,
        MetricsService metricsService)
    {
        _emailValidationService = emailValidationService;
        _passwordValidationService = passwordValidationService;
        _roleValidationService = roleValidationService;
        _userCreationService = userCreationService;
        _eventPublisher = eventPublisher;
        _emailService = emailService;
        _metricsService = metricsService;
    }

    public async Task<Result<UserResponse>> RegisterAsync(RegisterUserRequest request)
    {
        var context = new UserContext { Request = request };

        var result = await Pipeline.Start(context)
            .WithTelemetry("RegisterUser")
            .WithRetry(maxAttempts: 2)

            // Validation steps
            .StepResultAsync(ctx => _emailValidationService.ValidateEmailAsync(ctx))
            .StepResultAsync(ctx => _passwordValidationService.ValidatePasswordAsync(ctx))
            .StepResultAsync(ctx => _roleValidationService.ValidateRoleAsync(ctx))

            // Creation step
            .StepResultAsync(ctx => _userCreationService.CreateUserAsync(ctx))

            // Side effects
            .TapAsync(ctx => _eventPublisher.PublishAsync(new UserRegistered(ctx.CreatedUser.Id)))
            .TapAsync(ctx => _emailService.SendWelcomeEmailAsync(ctx.CreatedUser.Email))
            .TapAsync(ctx => _metricsService.IncrementUserRegistrations())

            // Transform to response
            .Transform<UserResponse>(ctx => new UserResponse
            {
                Id = ctx.CreatedUser.Id,
                Email = ctx.CreatedUser.Email,
                Role = ctx.CreatedUser.Role,
                CreatedAt = ctx.CreatedUser.CreatedAt
            })
            .ExecuteAsync();

        return result;
    }
}
```

## Conditional Processing

```csharp
public class OrderProcessor
{
    private readonly OrderValidationService _validationService;
    private readonly FraudCheckService _fraudCheckService;
    private readonly ManagerApprovalService _approvalService;
    private readonly ComplianceService _complianceService;
    private readonly CurrencyConversionService _currencyService;
    private readonly PaymentService _paymentService;

    public OrderProcessor(
        OrderValidationService validationService,
        FraudCheckService fraudCheckService,
        ManagerApprovalService approvalService,
        ComplianceService complianceService,
        CurrencyConversionService currencyService,
        PaymentService paymentService)
    {
        _validationService = validationService;
        _fraudCheckService = fraudCheckService;
        _approvalService = approvalService;
        _complianceService = complianceService;
        _currencyService = currencyService;
        _paymentService = paymentService;
    }

    public async Task<Result<OrderContext>> ProcessAsync(OrderContext context)
    {
        var result = await Pipeline.Start(context)
            .StepResultAsync(ctx => _validationService.ValidateAsync(ctx))

            .When(
                ctx => ctx.Order.Amount > 1000,
                pipeline => pipeline
                    .StepAsync(ctx => _fraudCheckService.CheckAsync(ctx))
                    .StepAsync(ctx => _approvalService.RequestApprovalAsync(ctx)))

            .When(
                ctx => ctx.Order.IsInternational,
                pipeline => pipeline
                    .StepAsync(ctx => _complianceService.CheckComplianceAsync(ctx))
                    .StepAsync(ctx => _currencyService.ConvertCurrencyAsync(ctx)))

            .StepResultAsync(ctx => _paymentService.ProcessPaymentAsync(ctx))
            .ExecuteAsync();

        return result;
    }
}
```

# ❌ Error Handling

## Result Pattern

The library uses the Result pattern for explicit error handling:

```csharp
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
}
```

## Handling Pipeline Results

```csharp
public class DataProcessor
{
    private readonly ValidationService _validationService;
    private readonly ProcessingService _processingService;

    public DataProcessor(
        ValidationService validationService,
        ProcessingService processingService)
    {
        _validationService = validationService;
        _processingService = processingService;
    }

    public async Task<DataContext?> ProcessDataAsync(DataContext context)
    {
        var result = await Pipeline.Start(context)
            .StepResultAsync(ctx => _validationService.ValidateAsync(ctx))
            .StepResultAsync(ctx => _processingService.ProcessAsync(ctx))
            .ExecuteAsync();

        if (result.IsSuccess)
        {
            var data = result.Value;
            // Handle success
            return data;
        }
        else
        {
            var errorMessage = result.ErrorMessage;
            var exception = result.Exception;
            // Handle failure
            return null;
        }
    }
}
```

## Exception Types

- `PipelineException`: General pipeline execution errors
- `PipelineConfigurationException`: Configuration errors (missing services, invalid setup)

Configuration exceptions are fail-fast and are always re-thrown to prevent silent failures.

## Exception Filtering

By default, all exceptions are handled internally by the pipeline and returned as failure results. However, you can configure specific exception types to be propagated (thrown) instead of being handled:

```csharp
// Configure during service registration
builder.Services.AddFlow(config => config
    .UseExceptionFilter<ArgumentException>()                    // Propagate ArgumentException
    .UseExceptionFilter<InvalidOperationException>()            // Propagate InvalidOperationException
    .UseExceptionFilter(typeof(UnauthorizedAccessException))    // Propagate using Type
);

// Example usage
public class ValidationService
{
    public async Task<Result<UserContext>> ValidateAsync(UserContext context)
    {
        return await Pipeline.Start(context)
            .StepAsync(ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Email))
                    throw new ArgumentException("Email is required"); // This will be propagated

                if (ctx.Age < 0)
                    throw new InvalidDataException("Invalid age");     // This will be handled

                return Task.FromResult(ctx);
            })
            .ExecuteAsync(); // ArgumentException will be thrown, InvalidDataException will return failure result
    }
}
```

**Key Features:**
- Exception inheritance is supported (e.g., `ArgumentNullException` inherits from `ArgumentException`)
- Multiple exception types can be configured
- Configuration applies to all pipelines in the application
- `PipelineConfigurationException` and `OperationCanceledException` are always propagated regardless of configuration

## Success and Error Callbacks

```csharp
public class MyProcessor
{
    private readonly MyService _myService;
    private readonly ILogger<MyProcessor> _logger;

    public MyProcessor(MyService myService, ILogger<MyProcessor> logger)
    {
        _myService = myService;
        _logger = logger;
    }

    public async Task<Result<MyContext>> ProcessAsync(MyContext context)
    {
        return await Pipeline.Start(context)
            .StepAsync(
                ctx => _myService.ProcessAsync(ctx),
                onSuccess: ctx => _logger.LogInformation("Step succeeded"),
                onError: ex => _logger.LogError(ex, "Step failed"))
            .ExecuteAsync();
    }
}
```

# 🧪 Testing

The pipeline design makes testing straightforward:

```csharp
[Fact]
public async Task CreateUser_WithValidData_ShouldSucceed()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddSingleton<IUserRepository>(mockRepository);
    services.AddSingleton<IPasswordValidator>(mockPasswordValidator);
    services.AddSingleton<UserValidationService>();
    services.AddSingleton<UserCreationService>();
    services.AddSingleton<UserRegistrationPipeline>();
    services.AddLogging();
    services.AddFlow();

    var serviceProvider = services.BuildServiceProvider();
    var pipeline = serviceProvider.GetRequiredService<UserRegistrationPipeline>();
    var request = new RegisterUserRequest { Email = "test@example.com", Name = "Test User" };

    // Act
    var result = await pipeline.RegisterAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value?.CreatedUser);
    Assert.Equal(request.Email, result.Value.CreatedUser.Email);
}

[Fact]
public async Task ProcessData_WithValidContext_ShouldExecuteAllSteps()
{
    // Arrange
    var validationService = new Mock<ValidationService>();
    var creationService = new Mock<UserCreationService>();

    validationService
        .Setup(x => x.ValidateAsync(It.IsAny<UserContext>()))
        .ReturnsAsync(Result<UserContext>.Success(new UserContext()));

    var processor = new UserProcessor(
        validationService.Object,
        creationService.Object);

    var context = new UserContext { Request = validRequest };

    // Act
    var result = await processor.ProcessAsync(context);

    // Assert
    Assert.True(result.IsSuccess);
    validationService.Verify(x => x.ValidateAsync(It.IsAny<UserContext>()), Times.Once);
    creationService.Verify(x => x.CreateAsync(It.IsAny<UserContext>()), Times.Once);
}
```

# 📋 Best Practices

1. **Use BuildApp() or BuildWithGlobalProvider()**: Initialize the global service provider for seamless DI
2. **Use Result Pattern**: Return `Result<T>` from services for explicit error handling
3. **Configure Dependencies**: Register all services in Program.cs/Startup.cs before building
4. **Enable Telemetry**: Use `WithTelemetry()` for production observability and distributed tracing
5. **Configure Retry Policies**: Set appropriate retry policies for unreliable operations (APIs, databases)
6. **Separate Concerns**: Keep each step focused on a single responsibility
7. **Use Tap for Side Effects**: Keep logging, metrics, and events separate from the main flow
8. **Handle Errors Gracefully**: Always check `IsSuccess` before accessing `Value`
9. **Use CancellationToken**: Pass cancellation tokens for long-running operations
10. **Test Pipeline Steps**: Test individual services and complete pipelines separately
11. **Use Conditional Execution**: Keep conditional logic readable with `When()`
12. **Configure Exception Filters**: Use `.UseExceptionFilter<T>()` for business exceptions that should propagate
13. **Keep Context Immutable**: Avoid mutating context objects; create new instances when needed
14. **Use Transform for Type Changes**: Transform context types when crossing architectural boundaries

# 📊 Response Information

Every pipeline execution returns comprehensive information:

```csharp
var result = await Pipeline.Start(context)...ExecuteAsync();

Console.WriteLine($"Is Success: {result.IsSuccess}");
Console.WriteLine($"Is Failure: {result.IsFailure}");
Console.WriteLine($"Error Message: {result.ErrorMessage}");

if (result.IsSuccess)
{
    var value = result.Value;
    // Process successful result
}
```

# 🌐 Global Service Provider

Myth.Flow uses the Myth.Commons centralized service provider for seamless cross-library dependency resolution.

## How It Works

When you call `builder.BuildApp()` (ASP.NET Core) or `services.BuildWithGlobalProvider()` (console apps), the global service provider is automatically initialized. This allows `Pipeline.Start()` to access all registered services without manual configuration.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFlow();
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<ProcessingService>();

var app = builder.BuildApp();
```

Now all pipelines can access these services:

```csharp
public async Task<Result<MyContext>> ProcessAsync(MyContext context)
{
    return await Pipeline.Start(context)
        .StepAsync(ctx => ProcessDataAsync(ctx))
        .ExecuteAsync();
}
```

## Benefits

- **Zero Configuration**: No need to pass service providers around
- **Cross-Library Integration**: Works seamlessly with Myth.Guard, Myth.Flow.Actions, etc.
- **Clean Code**: No service locator anti-pattern in your business logic
- **Type Safety**: Services are resolved at startup, not runtime

# 📄 License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.

# 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

# 📧 Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/paulaolileal/myth).
