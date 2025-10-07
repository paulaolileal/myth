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
- **Dependency Injection**: Full ASP.NET Core DI integration
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
var input = new OrderContext { OrderId = 123 };

var result = await Pipeline.Start(input)
    .StepAsync<ValidationService>((svc, ctx) => svc.ValidateOrderAsync(ctx))
    .StepAsync<PaymentService>((svc, ctx) => svc.ProcessPaymentAsync(ctx))
    .StepAsync<InventoryService>((svc, ctx) => svc.ReserveItemsAsync(ctx))
    .Tap(ctx => Console.WriteLine($"Order {ctx.OrderId} completed"))
    .ExecuteAsync();

if (result.IsSuccess)
{
    Console.WriteLine("Order processed successfully!");
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
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var context = new OrderContext { Request = request };

        var result = await Pipeline.Start(context)
            .WithTelemetry("CreateOrder")
            .WithRetry(maxAttempts: 3, backoffMs: 100)
            .StepResultAsync<OrderValidationService>(
                (svc, ctx) => svc.ValidateAsync(ctx))
            .StepResultAsync<OrderCreationService>(
                (svc, ctx) => svc.CreateAsync(ctx))
            .TapAsync<OrderEventService>(
                (svc, ctx) => svc.PublishOrderCreatedAsync(ctx))
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
builder.Services.AddFlow(config =>
{
    config.EnableTelemetry = true;           // Enable OpenTelemetry tracing
    config.EnableLogging = true;             // Enable logging
    config.DefaultRetryAttempts = 3;         // Default retry attempts
    config.DefaultBackoffMs = 100;           // Default backoff in milliseconds
    config.ActivitySource = activitySource;  // Custom ActivitySource (optional)
});
```

## Pipeline Configuration

```csharp
var result = await Pipeline.Start(context)
    .WithTelemetry("OperationName")          // Set operation name for tracing
    .WithRetry(maxAttempts: 5, backoffMs: 200) // Configure retry for subsequent steps
    .StepAsync<MyService>((svc, ctx) => svc.ProcessAsync(ctx))
    .ExecuteAsync();
```

# 🔄 Pipeline Steps

## Synchronous Steps

```csharp
.Step<MyService>((svc, ctx) => 
{
    // Synchronous processing
    ctx.Data = svc.Transform(ctx.Data);
    return ctx;
})
```

## Asynchronous Steps

```csharp
.StepAsync<MyService>((svc, ctx) => 
    svc.ProcessAsync(ctx))
```

## Steps with Result Pattern

```csharp
.StepResultAsync<ValidationService>((svc, ctx) => 
    svc.ValidateAsync(ctx))
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
// Simple tap
.Tap(ctx => Console.WriteLine($"Processing: {ctx.Id}"))

// Async tap
.TapAsync(async ctx => 
    await _logger.LogAsync($"Step completed: {ctx.Id}"))

// Tap with service injection
.TapAsync<EventPublisher>((svc, ctx) => 
    svc.PublishAsync(new OrderCreated(ctx.OrderId)))

.Tap<MetricsService>((svc, ctx) => 
    svc.IncrementCounter("orders_created"))
```

## Conditional Execution

```csharp
.When(
    ctx => ctx.Amount > 1000,
    pipeline => pipeline
        .StepAsync<FraudDetectionService>((svc, ctx) => 
            svc.CheckAsync(ctx))
        .StepAsync<ApprovalService>((svc, ctx) => 
            svc.RequestApprovalAsync(ctx)))
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
var result = await Pipeline.Start(context)
    .WithRetry(maxAttempts: 3, backoffMs: 100)
    .StepAsync<ExternalApiService>((svc, ctx) => 
        svc.CallUnreliableApiAsync(ctx)) // Will retry on exceptions
    .ExecuteAsync();
```

# 📊 Observability & Telemetry

## OpenTelemetry Integration

The library automatically creates activities for distributed tracing:

```csharp
var result = await Pipeline.Start(context)
    .WithTelemetry("CreateUser")
    .StepResultAsync<ValidationService>((svc, ctx) => svc.ValidateAsync(ctx))
    .StepResultAsync<UserCreationService>((svc, ctx) => svc.CreateAsync(ctx))
    .ExecuteAsync();
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

// Use in pipeline
.TapAsync<UserObservabilityService>((svc, ctx) => 
    svc.RecordMetricsAsync(ctx))
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
var result = await Pipeline.Start(context)
    .WithTelemetry("ProcessOrder")
    .StepResultAsync<OrderValidationService>(
        (svc, ctx) => svc.ValidateAsync(ctx))
    .StepResultAsync<OrderCreationService>(
        (svc, ctx) => svc.CreateAsync(ctx))
    .TapAsync<OrderEventService>(
        (svc, ctx) => svc.PublishOrderCreatedAsync(ctx))
    .TapAsync<NotificationService>(
        (svc, ctx) => svc.SendConfirmationEmailAsync(ctx))
    .TapAsync<MetricsService>(
        (svc, ctx) => svc.RecordOrderCreatedAsync(ctx))
    .Transform<OrderResponse>(ctx => new OrderResponse
    {
        OrderId = ctx.Order.Id,
        Status = ctx.Order.Status,
        CreatedAt = ctx.Order.CreatedAt
    })
    .ExecuteAsync();
```

## Multi-Step Validation Pipeline

```csharp
public class UserRegistrationPipeline
{
    public async Task<Result<UserResponse>> RegisterAsync(RegisterUserRequest request)
    {
        var context = new UserContext { Request = request };

        var result = await Pipeline.Start(context)
            .WithTelemetry("RegisterUser")
            .WithRetry(maxAttempts: 2)
            
            // Validation steps
            .StepResultAsync<EmailValidationService>(
                (svc, ctx) => svc.ValidateEmailAsync(ctx))
            .StepResultAsync<PasswordValidationService>(
                (svc, ctx) => svc.ValidatePasswordAsync(ctx))
            .StepResultAsync<RoleValidationService>(
                (svc, ctx) => svc.ValidateRoleAsync(ctx))
            
            // Creation step
            .StepResultAsync<UserCreationService>(
                (svc, ctx) => svc.CreateUserAsync(ctx))
            
            // Side effects
            .TapAsync<EventPublisher>(
                (svc, ctx) => svc.PublishAsync(new UserRegistered(ctx.CreatedUser.Id)))
            .TapAsync<EmailService>(
                (svc, ctx) => svc.SendWelcomeEmailAsync(ctx.CreatedUser.Email))
            .TapAsync<MetricsService>(
                (svc, ctx) => svc.IncrementUserRegistrations())
            
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
var result = await Pipeline.Start(context)
    .StepResultAsync<OrderValidationService>(
        (svc, ctx) => svc.ValidateAsync(ctx))
    
    .When(
        ctx => ctx.Order.Amount > 1000,
        pipeline => pipeline
            .StepAsync<FraudCheckService>(
                (svc, ctx) => svc.CheckAsync(ctx))
            .StepAsync<ManagerApprovalService>(
                (svc, ctx) => svc.RequestApprovalAsync(ctx)))
    
    .When(
        ctx => ctx.Order.IsInternational,
        pipeline => pipeline
            .StepAsync<ComplianceService>(
                (svc, ctx) => svc.CheckComplianceAsync(ctx))
            .StepAsync<CurrencyConversionService>(
                (svc, ctx) => svc.ConvertCurrencyAsync(ctx)))
    
    .StepResultAsync<PaymentService>(
        (svc, ctx) => svc.ProcessPaymentAsync(ctx))
    .ExecuteAsync();
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
var result = await Pipeline.Start(context)
    .StepResultAsync<ValidationService>((svc, ctx) => svc.ValidateAsync(ctx))
    .StepResultAsync<ProcessingService>((svc, ctx) => svc.ProcessAsync(ctx))
    .ExecuteAsync();

if (result.IsSuccess)
{
    var data = result.Value;
    // Handle success
}
else
{
    var errorMessage = result.ErrorMessage;
    var exception = result.Exception;
    // Handle failure
}
```

## Exception Types

- `PipelineException`: General pipeline execution errors
- `PipelineConfigurationException`: Configuration errors (missing services, invalid setup)

Configuration exceptions are fail-fast and are always re-thrown to prevent silent failures.

## Success and Error Callbacks

```csharp
.StepAsync<MyService>(
    (svc, ctx) => svc.ProcessAsync(ctx),
    onSuccess: ctx => _logger.LogInformation("Step succeeded"),
    onError: ex => _logger.LogError(ex, "Step failed"))
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
    services.AddLogging();
    services.AddFlow();

    var serviceProvider = services.BuildServiceProvider();
    var context = new UserContext { Request = validRequest };

    // Act
    var result = await Pipeline.Start(context, serviceProvider)
        .StepResultAsync<UserValidationService>(
            (svc, ctx) => svc.ValidateAsync(ctx))
        .StepResultAsync<UserCreationService>(
            (svc, ctx) => svc.CreateAsync(ctx))
        .ExecuteAsync();

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value?.CreatedUser);
    Assert.Equal(validRequest.Email, result.Value.CreatedUser.Email);
}
```

# 📋 Best Practices

1. **Use Result Pattern**: Return `Result<T>` from services for explicit error handling
2. **Configure Dependency Injection**: Always use DI for better testability and maintainability
3. **Enable Telemetry**: Use `WithTelemetry()` for production observability
4. **Configure Retry Policies**: Set appropriate retry policies for unreliable operations
5. **Separate Concerns**: Keep steps focused on single responsibilities
6. **Use Tap for Side Effects**: Keep side effects (logging, metrics, events) separate from main flow
7. **Handle Errors Gracefully**: Always check `IsSuccess` before accessing `Value`
8. **Add Observability**: Integrate logging and metrics for production monitoring
9. **Test Pipeline Steps**: Test individual services and complete pipelines separately
10. **Use Conditional Execution**: Keep conditional logic readable with `When()`

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

# 📄 License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.

# 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

# 📧 Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/paulaolileal/myth).