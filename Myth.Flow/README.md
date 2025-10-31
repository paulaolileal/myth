# Myth.Flow

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Flow?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Flow/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Flow?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Flow/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A powerful .NET library for building maintainable and testable data processing pipelines with a fluent, chainable interface. Built with enterprise-grade features including automatic retry policies, OpenTelemetry integration, dependency injection, and comprehensive error handling.

# ⭐ Features

- **Fluent Interface**: Simple, chainable API design for readable code
- **Type Safety**: Strong typing with context transformation support
- **Automatic Retry**: Configurable retry policies with exponential backoff
- **OpenTelemetry Integration**: Built-in tracing and observability
- **Constructor Injection**: Clean dependency injection via constructors (no service locator)
- **Result Pattern**: Railway-oriented programming with `Result<T>`
- **Error Handling**: Comprehensive error handling with custom exceptions
- **Conditional Execution**: Execute steps based on context predicates
- **Side Effects**: Tap into pipeline for logging, metrics, and events
- **Async/Await**: First-class async support throughout

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

## Dependency Injection Setup

### Program.cs (Minimal API)

```csharp
builder.Services.AddFlow(config =>
{
    config.EnableTelemetry = true;
    config.EnableLogging = true;
    config.DefaultRetryAttempts = 3;
    config.DefaultBackoffMs = 100;
});

// Register your services
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<InventoryService>();
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
// Simple configuration
builder.Services.AddFlow();

// Or with configuration options
builder.Services.AddFlow(config => config
    .UseTelemetry()                          // Enable OpenTelemetry tracing
    .UseLogging()                            // Enable logging
    .UseRetry(attempts: 3, backoffMs: 100)   // Default retry policy
    .UseActivitySource("MyApp.Pipeline")     // Custom ActivitySource name
    .UseExceptionFilter<ArgumentException>() // Propagate ArgumentException without handling
    .UseExceptionFilter(typeof(InvalidOperationException)) // Propagate specific exception types
);
```

## Pipeline Configuration

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
        var result = await Pipeline.Start(context)
            .WithTelemetry("OperationName")          // Set operation name for tracing
            .WithRetry(maxAttempts: 5, backoffMs: 200) // Configure retry for subsequent steps
            .StepAsync(ctx => _processingService.ProcessAsync(ctx))
            .ExecuteAsync();

        return result;
    }
}
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

The `Result<T>` pattern allows steps to return success or failure:

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

## Context Transformation

Transform the pipeline context to a different type:

```csharp
.Transform<OutputContext>(ctx => new OutputContext
{
    Id = ctx.Entity.Id,
    Name = ctx.Entity.Name
})

.TransformAsync<OutputContext>(async ctx =>
{
    var data = await _service.GetDataAsync(ctx.Id);
    return new OutputContext { Data = data };
})
```

## Side Effects (Tap)

Execute actions without modifying the context:

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
            // Simple tap
            .Tap(ctx => Console.WriteLine($"Processing: {ctx.Id}"))

            // Async tap with logger
            .TapAsync(async ctx =>
                await _logger.LogAsync($"Step completed: {ctx.Id}"))

            // Tap with event publishing
            .TapAsync(ctx =>
                _eventPublisher.PublishAsync(new OrderCreated(ctx.OrderId)))

            // Tap with metrics
            .Tap(ctx =>
                _metricsService.IncrementCounter("orders_created"))
            .ExecuteAsync();
    }
}
```

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

Configure retry behavior for resilient pipelines:

## Global Retry Configuration

```csharp
builder.Services.AddFlow(config =>
{
    config.DefaultRetryAttempts = 3;
    config.DefaultBackoffMs = 100;
});
```

## Per-Pipeline Retry

```csharp
.WithRetry(maxAttempts: 5, backoffMs: 200)
```

Retry behavior:
- Exponential backoff: delay = backoffMs × attemptNumber
- Retries only on exceptions (not on `Result.Failure`)
- `OperationCanceledException` is never retried
- Individual steps inherit the retry configuration

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
        var result = await Pipeline.Start(context)
            .WithRetry(maxAttempts: 3, backoffMs: 100)
            .StepAsync(ctx => _externalApiService.CallUnreliableApiAsync(ctx)) // Will retry on exceptions
            .ExecuteAsync();

        return result;
    }
}
```

# 📊 Observability & Telemetry

## OpenTelemetry Integration

The library automatically creates activities for distributed tracing:

```csharp
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
        var result = await Pipeline.Start(context)
            .WithTelemetry("CreateUser")
            .StepResultAsync(ctx => _validationService.ValidateAsync(ctx))
            .StepResultAsync(ctx => _creationService.CreateAsync(ctx))
            .ExecuteAsync();

        return result;
    }
}
```

Each step creates a child activity with tags:
- `pipeline.input.type`: Context type name
- Step-specific tags and timing information

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

1. **Use Constructor Injection**: Inject all required services via constructor for better testability and maintainability
2. **Use Result Pattern**: Return `Result<T>` from services for explicit error handling
3. **Configure Dependency Injection**: Register all services in Program.cs/Startup.cs
4. **Enable Telemetry**: Use `WithTelemetry()` for production observability
5. **Configure Retry Policies**: Set appropriate retry policies for unreliable operations
6. **Separate Concerns**: Keep steps focused on single responsibilities
7. **Use Tap for Side Effects**: Keep side effects (logging, metrics, events) separate from main flow
8. **Handle Errors Gracefully**: Always check `IsSuccess` before accessing `Value`
9. **Add Observability**: Integrate logging and metrics for production monitoring
10. **Test Pipeline Steps**: Test individual services and complete pipelines separately
11. **Use Conditional Execution**: Keep conditional logic readable with `When()`
12. **Avoid Service Locator**: Don't resolve services inside pipeline steps; use constructor injection instead

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

# 🔄 Migration Guide

## Moving from Service Locator Pattern

If you're upgrading from an earlier version that used the service locator pattern, here's how to migrate your code:

### Old Pattern (Service Locator - Deprecated)

```csharp
var result = await Pipeline.Start(context)
    .StepAsync<ValidationService>((svc, ctx) => svc.ValidateAsync(ctx))
    .StepAsync<ProcessingService>((svc, ctx) => svc.ProcessAsync(ctx))
    .ExecuteAsync();
```

### New Pattern (Constructor Injection - Recommended)

```csharp
public class MyPipeline
{
    private readonly ValidationService _validationService;
    private readonly ProcessingService _processingService;

    public MyPipeline(
        ValidationService validationService,
        ProcessingService processingService)
    {
        _validationService = validationService;
        _processingService = processingService;
    }

    public async Task<Result<MyContext>> ExecuteAsync(MyContext context)
    {
        var result = await Pipeline.Start(context)
            .StepAsync(ctx => _validationService.ValidateAsync(ctx))
            .StepAsync(ctx => _processingService.ProcessAsync(ctx))
            .ExecuteAsync();

        return result;
    }
}
```

### Benefits of Constructor Injection

1. **Better Testability**: Easy to mock dependencies in unit tests
2. **Explicit Dependencies**: Clear what services are required
3. **Compile-Time Safety**: Missing dependencies caught at startup, not runtime
4. **SOLID Principles**: Follows Dependency Inversion Principle
5. **IDE Support**: Better IntelliSense and code navigation
6. **No Hidden Dependencies**: All dependencies visible in constructor

### Migration Steps

1. **Identify Services**: Find all `.Step<TService>()`, `.StepAsync<TService>()`, `.Tap<TService>()` calls
2. **Add Constructor Parameters**: Add these services as constructor parameters
3. **Store as Fields**: Save constructor parameters as private readonly fields
4. **Update Pipeline Calls**: Remove `<TService>` generic parameter and use injected fields
5. **Register Services**: Ensure all services are registered in DI container
6. **Test**: Verify your pipelines work with the new injection approach

# 📄 License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.

# 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

# 📧 Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/paulaolileal/myth).