---
name: myth-flow
description: Use when you need to orchestrate data processing pipelines. Pipeline.Start(ctx) chains .Step()/.StepAsync()/.StepResultAsync(), .Tap()/.TapAsync() for side effects, .When() for conditional branches, and .Transform() for context type changes. Built-in Result<T> pattern, retry with exponential backoff, OpenTelemetry tracing, and per-step DI resolution.
---

# SKILL.md - Myth.Flow

**Version:** 1.0
**Target Framework:** .NET 8.0
**License:** Apache 2.0

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [API Reference](#api-reference)
  - [Pipeline Static Class](#pipeline-static-class)
  - [IPipelineBuilder Interface](#ipipelinebuilder-interface)
  - [Result Pattern](#result-pattern)
  - [Configuration](#configuration)
- [Usage Examples](#usage-examples)
- [Advanced Patterns](#advanced-patterns)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

Myth.Flow is a fluent pipeline library for .NET that enables building data processing workflows declaratively. It provides a type-safe, composable way to chain operations with built-in support for telemetry, retry policies, transformations, the Result pattern, and conditional execution.

### Key Features

- **Fluent Pipeline API**: Chain operations with intuitive method calls
- **Result Pattern**: Built-in support for success/failure handling
- **OpenTelemetry Integration**: Automatic distributed tracing
- **Retry Policies**: Exponential backoff with configurable attempts
- **Transformations**: Transform context between pipeline steps
- **Side Effects (Tap)**: Execute operations without modifying context
- **Conditional Execution**: Execute pipeline branches based on predicates
- **Cancellation Support**: Full CancellationToken support
- **Exception Filtering**: Selective exception propagation
- **Async-First**: Fully asynchronous execution

---

## Installation

```bash
dotnet add package Myth.Flow
```

### Dependencies
- .NET 8.0 or higher
- Myth.Commons
- System.Diagnostics.DiagnosticSource (OpenTelemetry)
- Microsoft.Extensions.Logging.Abstractions

---

## Core Concepts

### 1. Pipeline

A pipeline is a sequence of operations (steps) executed in order. Each step receives a context and returns a modified context.

```csharp
var result = await Pipeline.Start(context)
    .Step(ctx => Process(ctx))
    .StepAsync(ctx => ProcessAsync(ctx))
    .ExecuteAsync();
```

### 2. Pipeline Data (Context)

The pipeline processes data through a series of steps. The data can be **any type** - it doesn't need to be named "Context". Each step receives the current state and returns a modified state.

```csharp
// You can use ANY type as pipeline data
// Using a class named "...Context" is just a naming convention, not a requirement

// Option 1: Use a dedicated data holder class
public class CreateUserContext {
    public CreateUserRequest Request { get; set; }
    public User? CreatedUser { get; set; }
    public List<string> Errors { get; set; } = new();
}

// Option 2: Use your domain models directly
var result = await Pipeline.Start(user)
    .StepAsync(u => ValidateUserAsync(u))
    .StepAsync(u => SaveUserAsync(u))
    .ExecuteAsync();

// Option 3: Use DTOs or requests
var result = await Pipeline.Start(request)
    .StepAsync(r => ProcessRequestAsync(r))
    .ExecuteAsync();

// Option 4: Use simple types
var result = await Pipeline.Start(userId)
    .StepAsync(id => GetUserAsync(id))
    .ExecuteAsync();
```

**Note:** The generic type parameter `TContext` in API signatures is just a type variable name - it works with any type, not just classes named "Context".

### 3. Result Pattern

Operations can return `Result<T>` to indicate success or failure without throwing exceptions.

```csharp
public async Task<Result<User>> CreateUserAsync(CreateUserContext ctx) {
    if (string.IsNullOrEmpty(ctx.Request.Email))
        return Result<User>.Failure("Email is required");

    var user = await _repository.CreateAsync(ctx.Request);
    return Result<User>.Success(user);
}
```

### 4. Steps vs Taps

- **Steps**: Transform the context (return modified context)
- **Taps**: Perform side effects without modifying context (logging, events, metrics)

---

## API Reference

### Pipeline Static Class

**Namespace:** `Myth.Flow`

Entry point for creating pipelines.

#### Methods

```csharp
// Create pipeline with global service provider
public static IPipelineBuilder<TInput> Start<TInput>(TInput input)

// Create pipeline with configuration
public static IPipelineBuilder<TInput> Start<TInput>(
    TInput input,
    Action<PipelineConfiguration> configure)
```

**Examples:**

```csharp
// Using global service provider (requires BuildApp())
var result = await Pipeline.Start(context)
    .StepAsync(ctx => ProcessAsync(ctx))
    .ExecuteAsync();

// With custom configuration
var result = await Pipeline.Start(context, config => {
    config.EnableTelemetry = true;
    config.DefaultRetryAttempts = 3;
})
.StepAsync(ctx => ProcessAsync(ctx))
.ExecuteAsync();
```

---

### IPipelineBuilder Interface

**Namespace:** `Myth.Interfaces`

Fluent interface for building pipelines.

#### Step Methods

##### Synchronous Step

```csharp
IPipelineBuilder<TContext> Step(
    Func<TContext, TContext> handler,
    Action<TContext>? onSuccess = null,
    Action<Exception>? onError = null)
```

**Example:**
```csharp
.Step(ctx => {
    ctx.IsValidated = true;
    return ctx;
},
onSuccess: ctx => _logger.LogInformation("Validated"),
onError: ex => _logger.LogError(ex, "Validation failed"))
```

---

##### Asynchronous Step

```csharp
IPipelineBuilder<TContext> StepAsync(
    Func<TContext, Task<TContext>> handler,
    Action<TContext>? onSuccess = null,
    Action<Exception>? onError = null)

IPipelineBuilder<TContext> StepAsync(
    Func<TContext, CancellationToken, Task<TContext>> handler)
```

**Examples:**
```csharp
// Basic async
.StepAsync(async ctx => {
    ctx.User = await _repository.GetUserAsync(ctx.UserId);
    return ctx;
})

// With cancellation token
.StepAsync(async (ctx, ct) => {
    ctx.Data = await _api.FetchDataAsync(ctx.Id, ct);
    return ctx;
})
```

---

##### Result-Based Steps

```csharp
IPipelineBuilder<TContext> StepResult(
    Func<TContext, Result<TContext>> handler)

IPipelineBuilder<TContext> StepResultAsync(
    Func<TContext, Task<Result<TContext>>> handler)

IPipelineBuilder<TContext> StepResultAsync(
    Func<TContext, CancellationToken, Task<Result<TContext>>> handler)
```

**Behavior:** If Result.IsFailure, pipeline stops and ExecuteAsync returns failure.

**Examples:**
```csharp
// Synchronous
.StepResult(ctx => {
    if (ctx.Age < 18)
        return Result<MyContext>.Failure("Must be 18 or older");
    return Result<MyContext>.Success(ctx);
})

// Asynchronous
.StepResultAsync(async ctx => {
    var isValid = await _validator.ValidateAsync(ctx.Data);
    if (!isValid)
        return Result<MyContext>.Failure("Validation failed");
    return Result<MyContext>.Success(ctx);
})

// With cancellation token
.StepResultAsync(async (ctx, ct) => {
    var result = await _service.ProcessAsync(ctx, ct);
    return result.IsSuccess
        ? Result<MyContext>.Success(ctx)
        : Result<MyContext>.Failure(result.Error);
})
```

---

#### Transformation Methods

```csharp
IPipelineBuilder<TNewContext> Transform<TNewContext>(
    Func<TContext, TNewContext> mapper)

IPipelineBuilder<TNewContext> TransformAsync<TNewContext>(
    Func<TContext, Task<TNewContext>> mapper)
```

**Use Case:** Change the context type mid-pipeline.

**Examples:**
```csharp
// Transform to DTO
await Pipeline.Start(createUserContext)
    .StepAsync(ctx => CreateUserAsync(ctx))
    .Transform(ctx => new UserDto {
        Id = ctx.CreatedUser.Id,
        Email = ctx.CreatedUser.Email,
        Name = ctx.CreatedUser.Name
    })
    .ExecuteAsync();

// Async transformation
.TransformAsync(async ctx => {
    var dto = ctx.User.To<UserDto>();
    dto.ProfileUrl = await _storage.GetUrlAsync(ctx.User.ProfileImageId);
    return dto;
})
```

---

#### Side Effect Methods (Tap)

```csharp
IPipelineBuilder<TContext> Tap(Action<TContext> action)

IPipelineBuilder<TContext> TapAsync(Func<TContext, Task> action)
```

**Use Case:** Execute operations without modifying context (logging, events, metrics).

**Examples:**
```csharp
// Logging
.Tap(ctx => _logger.LogInformation("Processing user {UserId}", ctx.UserId))

// Publish event
.TapAsync(async ctx => {
    await _eventBus.PublishAsync(new UserCreatedEvent {
        UserId = ctx.CreatedUser.Id,
        Email = ctx.CreatedUser.Email
    });
})

// Update metrics
.TapAsync(async ctx => {
    await _metrics.IncrementCounterAsync("users_created");
})
```

---

#### Conditional Execution

```csharp
IPipelineBuilder<TContext> When(
    Func<TContext, bool> predicate,
    Action<IPipelineBuilder<TContext>> configurePipeline)
```

**Use Case:** Execute pipeline branch only if condition is true.

**Example:**
```csharp
await Pipeline.Start(order)
    .StepAsync(o => ValidateOrderAsync(o))
    .When(
        o => o.Amount > 1000,
        pipeline => pipeline
            .TapAsync(o => SendHighValueNotificationAsync(o))
            .TapAsync(o => LogHighValueOrderAsync(o))
            .StepAsync(o => ApplySpecialDiscountAsync(o)))
    .When(
        o => o.IsFirstOrder,
        pipeline => pipeline
            .TapAsync(o => SendWelcomeEmailAsync(o)))
    .StepAsync(o => SaveOrderAsync(o))
    .ExecuteAsync();
```

---

#### Configuration Methods

```csharp
IPipelineBuilder<TContext> WithTelemetry(string operationName)

IPipelineBuilder<TContext> WithRetry(int maxAttempts, int backoffMs = 100)
```

**Examples:**
```csharp
// Enable telemetry
.WithTelemetry("CreateUser")

// Enable retry with exponential backoff
.WithRetry(maxAttempts: 3, backoffMs: 200)

// Both
await Pipeline.Start(context)
    .WithTelemetry("ProcessOrder")
    .WithRetry(maxAttempts: 5, backoffMs: 100)
    .StepResultAsync(ctx => ProcessAsync(ctx))
    .ExecuteAsync();
```

**Retry Behavior:**
- Backoff time = backoffMs × attempt number
- Attempt 1: 100ms, Attempt 2: 200ms, Attempt 3: 300ms
- Does NOT retry `OperationCanceledException`
- Does NOT retry filtered exceptions (see Exception Filter)

---

#### Execution Methods

```csharp
Task<Result<TContext>> ExecuteAsync(CancellationToken cancellationToken = default)

Result<TContext> Execute()
```

**Examples:**
```csharp
// Async execution (recommended)
var result = await pipeline.ExecuteAsync();
if (result.IsSuccess) {
    Console.WriteLine(result.Value);
} else {
    Console.WriteLine($"Error: {result.ErrorMessage}");
    if (result.Exception != null) {
        _logger.LogError(result.Exception, "Pipeline failed");
    }
}

// With cancellation
var cts = new CancellationTokenSource();
var result = await pipeline.ExecuteAsync(cts.Token);

// Synchronous (use sparingly)
var result = pipeline.Execute();
```

---

### Result Pattern

**Namespace:** `Myth.Models`

#### Result<T> Struct

```csharp
public readonly struct Result<T> {
    public bool IsSuccess { get; }
    public bool IsFailure { get; }  // !IsSuccess
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
}
```

#### Static Factory Methods

```csharp
// Create success result
public static Result<T> Success(T value)

// Create failure result
public static Result<T> Failure(string errorMessage, Exception? exception = null)
```

**Examples:**
```csharp
// Success
return Result<User>.Success(user);

// Failure with message
return Result<User>.Failure("User not found");

// Failure with message and exception
try {
    var user = await _repository.GetAsync(id);
    return Result<User>.Success(user);
} catch (Exception ex) {
    return Result<User>.Failure("Failed to retrieve user", ex);
}

// Usage
var result = await GetUserAsync(userId);
if (result.IsSuccess) {
    var user = result.Value;
    Console.WriteLine(user.Name);
} else {
    Console.WriteLine($"Error: {result.ErrorMessage}");
    if (result.Exception != null) {
        _logger.LogError(result.Exception, "Error getting user");
    }
}
```

---

### Configuration

#### PipelineConfiguration Class

**Namespace:** `Myth.Models`

```csharp
public class PipelineConfiguration {
    public bool EnableTelemetry { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public int DefaultRetryAttempts { get; set; } = 0;
    public int DefaultBackoffMs { get; set; } = 100;
    public ActivitySource? ActivitySource { get; set; }
    public HashSet<Type> ExceptionTypesToPropagate { get; set; } = new();
}
```

---

#### FlowConfigurationBuilder Class

**Namespace:** `Myth.Builders`

Fluent builder for Flow configuration.

##### Methods

```csharp
// Telemetry
FlowConfigurationBuilder UseTelemetry()
FlowConfigurationBuilder DisableTelemetry()

// Logging
FlowConfigurationBuilder UseLogging()
FlowConfigurationBuilder DisableLogging()

// Retry
FlowConfigurationBuilder UseRetry(int attempts, int backoffMs = 100)
FlowConfigurationBuilder DisableRetry()

// ActivitySource (OpenTelemetry)
FlowConfigurationBuilder UseActivitySource(ActivitySource activitySource)
FlowConfigurationBuilder UseActivitySource(string name, string? version = null)

// Exception filtering
FlowConfigurationBuilder UseExceptionFilter(params Type[] exceptionTypes)
FlowConfigurationBuilder UseExceptionFilter<TException>() where TException : Exception

// Build
PipelineConfiguration Build()
```

**Example:**
```csharp
builder.Services.AddFlow(config => config
    .UseTelemetry()
    .UseLogging()
    .UseRetry(3, 200)
    .UseActivitySource("MyApp.Pipeline", "1.0.0")
    .UseExceptionFilter<ArgumentException>()
    .UseExceptionFilter<InvalidOperationException>());
```

---

#### ServiceCollectionExtensions

**Namespace:** `Myth.Extensions`

```csharp
// Register Flow with default configuration
IServiceCollection AddFlow(this IServiceCollection services)

// Register Flow with custom configuration
IServiceCollection AddFlow(
    this IServiceCollection services,
    Action<FlowConfigurationBuilder> configure)
```

**Examples:**
```csharp
// Default
services.AddFlow();

// Custom
services.AddFlow(builder => builder
    .UseTelemetry()
    .UseRetry(3, 100)
    .DisableLogging());
```

---

### Exceptions

#### PipelineException

**Namespace:** `Myth.Exceptions`

```csharp
public class PipelineException : Exception {
    public PipelineException(string message)
    public PipelineException(string message, Exception? innerException)
}
```

**When Thrown:**
- StepResult returns failure
- StepResultAsync returns failure

---

#### PipelineConfigurationException

**Namespace:** `Myth.Exceptions`

```csharp
public class PipelineConfigurationException : InvalidOperationException {
    public PipelineConfigurationException(string message)
    public PipelineConfigurationException(string message, Exception? innerException)
}
```

**When Thrown:**
- Configuration errors (always propagated, fail-fast)

---

## Usage Examples

### Example 1: Simple CRUD Pipeline

```csharp
public class CreateUserContext {
    public CreateUserRequest Request { get; set; }
    public User? CreatedUser { get; set; }
}

public async Task<Result<User>> CreateUserAsync(CreateUserRequest request) {
    var context = new CreateUserContext { Request = request };

    var result = await Pipeline.Start(context)
        .StepResultAsync(async ctx => {
            // Validate
            if (string.IsNullOrEmpty(ctx.Request.Email))
                return Result<CreateUserContext>.Failure("Email is required");

            return Result<CreateUserContext>.Success(ctx);
        })
        .StepResultAsync(async ctx => {
            // Check if email exists
            var exists = await _userRepository.AnyAsync(u => u.Email == ctx.Request.Email);
            if (exists)
                return Result<CreateUserContext>.Failure("Email already exists");

            return Result<CreateUserContext>.Success(ctx);
        })
        .StepAsync(async ctx => {
            // Create user
            ctx.CreatedUser = new User {
                Email = ctx.Request.Email,
                Name = ctx.Request.Name,
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(ctx.CreatedUser);
            return ctx;
        })
        .TapAsync(async ctx => {
            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(ctx.CreatedUser.Email);
        })
        .ExecuteAsync();

    return result.IsSuccess
        ? Result<User>.Success(result.Value.CreatedUser!)
        : Result<User>.Failure(result.ErrorMessage!);
}
```

---

### Example 2: Pipeline with Transformation

```csharp
public async Task<Result<UserDto>> CreateAndReturnDtoAsync(CreateUserRequest request) {
    var result = await Pipeline.Start(new CreateUserContext { Request = request })
        .WithTelemetry("CreateUser")
        .WithRetry(maxAttempts: 2, backoffMs: 100)
        .StepResultAsync(ctx => ValidateAsync(ctx))
        .StepAsync(ctx => CreateUserAsync(ctx))
        .TapAsync(ctx => PublishUserCreatedEventAsync(ctx))
        .Transform<UserDto>(ctx => new UserDto {
            Id = ctx.CreatedUser!.Id,
            Email = ctx.CreatedUser.Email,
            Name = ctx.CreatedUser.Name,
            CreatedAt = ctx.CreatedUser.CreatedAt
        })
        .ExecuteAsync();

    return result.IsSuccess
        ? Result<UserDto>.Success(result.Value)
        : Result<UserDto>.Failure(result.ErrorMessage!);
}
```

---

### Example 3: Conditional Pipeline Branches

```csharp
public async Task<Result<Order>> ProcessOrderAsync(CreateOrderRequest request) {
    var result = await Pipeline.Start(new OrderContext { Request = request })
        .StepAsync(ctx => ValidateOrderAsync(ctx))
        .StepAsync(ctx => CalculateTotalAsync(ctx))
        .When(
            ctx => ctx.Total > 1000,
            pipeline => pipeline
                .StepAsync(ctx => ApplyBulkDiscountAsync(ctx))
                .TapAsync(ctx => NotifyFinanceTeamAsync(ctx)))
        .When(
            ctx => ctx.Request.CustomerId != null,
            pipeline => pipeline
                .StepAsync(ctx => LoadCustomerHistoryAsync(ctx))
                .When(
                    ctx => ctx.IsFirstOrder,
                    p => p.TapAsync(ctx => SendWelcomeGiftAsync(ctx))))
        .StepAsync(ctx => SaveOrderAsync(ctx))
        .TapAsync(ctx => SendConfirmationEmailAsync(ctx))
        .ExecuteAsync();

    return result.IsSuccess
        ? Result<Order>.Success(result.Value.Order!)
        : Result<Order>.Failure(result.ErrorMessage!);
}
```

---

### Example 4: Exception Filtering

```csharp
// Configuration
services.AddFlow(config => config
    .UseExceptionFilter<ArgumentException>()
    .UseExceptionFilter<InvalidOperationException>());

// Filtered exception in a regular Step — propagates as the raw type
try {
    var result = await Pipeline.Start(data)
        .StepAsync(async ctx => {
            if (ctx.Value < 0)
                throw new ArgumentException("Value cannot be negative"); // propagated raw

            return ctx;
        })
        .ExecuteAsync();
} catch (ArgumentException ex) {
    _logger.LogError(ex, "Validation error");
}

// Filtered exception inside Transform/TransformAsync — always wrapped in PipelineException
// Transform guarantees PipelineException as the single exception type at the boundary.
// ShouldPropagateException walks the InnerException chain, so the PipelineException
// still propagates when its inner exception is a filtered type.
try {
    var result = await Pipeline.Start(data)
        .TapAsync(ctx => ThrowIfInvalid(ctx))           // throws ArgumentException
        .Transform(ctx => new OutputContext(ctx))
        .ExecuteAsync();
} catch (PipelineException ex) when (ex.InnerException is ArgumentException argEx) {
    // argEx.Message == "..." — original exception preserved as InnerException
    _logger.LogError(argEx, "Validation error propagated through Transform");
}

// Unfiltered exceptions always return Result.Failure
var result = await Pipeline.Start(data)
    .StepAsync(async ctx => {
        throw new IOException("File not found");  // Not filtered
    })
    .ExecuteAsync();
// result.IsFailure == true
// result.ErrorMessage == "File not found"
// result.Exception is IOException
```

---

### Example 5: Multiple Side Effects

```csharp
public async Task<Result<ProcessedData>> ProcessDataAsync(InputData input) {
    var result = await Pipeline.Start(input)
        .WithTelemetry("ProcessData")
        .StepAsync(ctx => EnrichDataAsync(ctx))
        .Tap(ctx => _logger.LogInformation("Data enriched: {Count} items", ctx.Items.Count))
        .StepAsync(ctx => ValidateDataAsync(ctx))
        .TapAsync(async ctx => {
            await _metrics.RecordGaugeAsync("data.items", ctx.Items.Count);
        })
        .StepAsync(ctx => TransformDataAsync(ctx))
        .TapAsync(async ctx => {
            await _cache.SetAsync($"processed:{ctx.Id}", ctx);
        })
        .TapAsync(async ctx => {
            await _eventBus.PublishAsync(new DataProcessedEvent {
                Id = ctx.Id,
                ItemCount = ctx.Items.Count,
                ProcessedAt = DateTime.UtcNow
            });
        })
        .Transform<ProcessedData>(ctx => new ProcessedData {
            Id = ctx.Id,
            Items = ctx.Items,
            ProcessedAt = DateTime.UtcNow
        })
        .ExecuteAsync();

    return result.IsSuccess
        ? Result<ProcessedData>.Success(result.Value)
        : Result<ProcessedData>.Failure(result.ErrorMessage!);
}
```

---

## Advanced Patterns

### Pattern 1: Reusable Pipeline Steps

```csharp
public class UserPipelineSteps {
    private readonly IUserRepository _repository;
    private readonly IValidator _validator;

    public async Task<Result<TContext>> ValidateUserAsync<TContext>(TContext ctx)
        where TContext : IUserContext {
        var validationResult = await _validator.ValidateAsync(ctx.User);

        return validationResult.IsValid
            ? Result<TContext>.Success(ctx)
            : Result<TContext>.Failure(validationResult.ErrorMessage);
    }

    public async Task<TContext> LoadUserDetailsAsync<TContext>(TContext ctx)
        where TContext : IUserContext {
        ctx.User = await _repository.GetByIdAsync(ctx.UserId);
        return ctx;
    }
}

// Usage
var result = await Pipeline.Start(context)
    .StepResultAsync(ctx => _userSteps.ValidateUserAsync(ctx))
    .StepAsync(ctx => _userSteps.LoadUserDetailsAsync(ctx))
    .ExecuteAsync();
```

---

### Pattern 2: Pipeline Factory

```csharp
public class OrderPipelineFactory {
    public IPipelineBuilder<OrderContext> CreateOrderPipeline(OrderContext context) {
        return Pipeline.Start(context)
            .WithTelemetry("CreateOrder")
            .WithRetry(3, 100)
            .StepResultAsync(ctx => _validator.ValidateAsync(ctx))
            .StepAsync(ctx => _calculator.CalculateTotalAsync(ctx))
            .When(ctx => ctx.Total > 1000,
                p => p.StepAsync(ctx => _discount.ApplyBulkDiscountAsync(ctx)))
            .StepAsync(ctx => _repository.SaveOrderAsync(ctx))
            .TapAsync(ctx => _events.PublishOrderCreatedAsync(ctx));
    }

    public IPipelineBuilder<OrderContext> UpdateOrderPipeline(OrderContext context) {
        return Pipeline.Start(context)
            .WithTelemetry("UpdateOrder")
            .StepAsync(ctx => _repository.LoadOrderAsync(ctx))
            .StepResultAsync(ctx => _validator.ValidateUpdateAsync(ctx))
            .StepAsync(ctx => _repository.UpdateOrderAsync(ctx))
            .TapAsync(ctx => _events.PublishOrderUpdatedAsync(ctx));
    }
}

// Usage
var result = await _factory.CreateOrderPipeline(context).ExecuteAsync();
```

---

### Pattern 3: Nested Pipelines

```csharp
public async Task<Result<CompleteOrderContext>> ProcessCompleteOrderAsync(OrderRequest request) {
    var result = await Pipeline.Start(new CompleteOrderContext { Request = request })
        .StepResultAsync(async ctx => {
            // Nested pipeline for inventory check
            var inventoryResult = await Pipeline.Start(ctx.Request.Items)
                .StepAsync(items => CheckInventoryAsync(items))
                .StepAsync(items => ReserveInventoryAsync(items))
                .ExecuteAsync();

            if (inventoryResult.IsFailure)
                return Result<CompleteOrderContext>.Failure("Inventory check failed");

            ctx.ReservedItems = inventoryResult.Value;
            return Result<CompleteOrderContext>.Success(ctx);
        })
        .StepAsync(ctx => CreateOrderAsync(ctx))
        .StepResultAsync(async ctx => {
            // Nested pipeline for payment
            var paymentResult = await Pipeline.Start(ctx.Order)
                .StepAsync(order => ValidatePaymentAsync(order))
                .StepAsync(order => ProcessPaymentAsync(order))
                .ExecuteAsync();

            if (paymentResult.IsFailure)
                return Result<CompleteOrderContext>.Failure("Payment failed");

            ctx.PaymentConfirmation = paymentResult.Value;
            return Result<CompleteOrderContext>.Success(ctx);
        })
        .ExecuteAsync();

    return result.IsSuccess
        ? Result<CompleteOrderContext>.Success(result.Value)
        : Result<CompleteOrderContext>.Failure(result.ErrorMessage!);
}
```

---

## Best Practices

### 1. Use Result Pattern for Validation

**✅ DO:**
```csharp
.StepResultAsync(async ctx => {
    var isValid = await _validator.ValidateAsync(ctx.Data);
    return isValid
        ? Result<MyContext>.Success(ctx)
        : Result<MyContext>.Failure("Validation failed");
})
```

**❌ DON'T:**
```csharp
.StepAsync(async ctx => {
    var isValid = await _validator.ValidateAsync(ctx.Data);
    if (!isValid)
        throw new ValidationException("Validation failed");  // Don't throw
    return ctx;
})
```

---

### 2. Use Tap for Side Effects

**✅ DO:**
```csharp
.TapAsync(async ctx => {
    await _logger.LogInformationAsync("User created: {UserId}", ctx.UserId);
    await _eventBus.PublishAsync(new UserCreatedEvent { UserId = ctx.UserId });
})
```

**❌ DON'T:**
```csharp
.StepAsync(async ctx => {
    await _logger.LogInformationAsync("User created: {UserId}", ctx.UserId);
    return ctx;  // Unnecessarily uses Step for logging
})
```

---

### 3. Configure Globally in Startup

**✅ DO:**
```csharp
// Program.cs
services.AddFlow(config => config
    .UseTelemetry()
    .UseRetry(3, 100)
    .UseLogging());
```

**❌ DON'T:**
```csharp
// Don't configure per pipeline
Pipeline.Start(ctx, config => {
    config.EnableTelemetry = true;  // Prefer global config
})
```

---

### 4. Use Meaningful Telemetry Names

**✅ DO:**
```csharp
.WithTelemetry("CreateUser")
.WithTelemetry("ProcessOrder")
.WithTelemetry("CalculateInvoice")
```

**❌ DON'T:**
```csharp
.WithTelemetry("Pipeline1")
.WithTelemetry("Process")
```

---

### 5. Handle Cancellation

**✅ DO:**
```csharp
.StepAsync(async (ctx, ct) => {
    ctx.Data = await _api.FetchDataAsync(ctx.Id, ct);
    return ctx;
})

var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var result = await pipeline.ExecuteAsync(cts.Token);
```

---

### 6. Transform Context Types Appropriately

**✅ DO:**
```csharp
// Transform to response DTO at the end
await Pipeline.Start(context)
    .StepAsync(ctx => ProcessAsync(ctx))
    .Transform<ResponseDto>(ctx => new ResponseDto {
        Id = ctx.Result.Id,
        Status = ctx.Result.Status
    })
    .ExecuteAsync();
```

---

## Troubleshooting

### Issue 1: MythServiceProvider.Current is null

**Problem:**
```
NullReferenceException when Pipeline.Start tries to resolve services
```

**Cause:** Global service provider not initialized.

**Solution:**
```csharp
// Use BuildApp() instead of Build()
var app = builder.BuildApp();

// Or for console apps
var provider = services.BuildServiceProvider();
MythServiceProvider.Initialize(provider);
```

---

### Issue 2: Retry Not Working

**Problem:** Retry not executing on failure.

**Possible Causes:**
1. **Exception is filtered**
   ```csharp
   // These exceptions won't retry
   services.AddFlow(c => c.UseExceptionFilter<ArgumentException>());
   ```

2. **OperationCanceledException** - Never retried

**Solution:** Check exception type and filter configuration.

---

### Issue 3: StepResult Throws PipelineException

**Problem:** StepResult with failure throws exception.

**Expected Behavior:** This is by design. Use ExecuteAsync to get Result.

**Solution:**
```csharp
var result = await pipeline.ExecuteAsync();
if (result.IsFailure) {
    // Handle failure
    Console.WriteLine(result.ErrorMessage);
}
```

---

### Issue 4: Telemetry Not Appearing

**Problem:** No telemetry activities in traces.

**Checklist:**
1. Enable telemetry: `.UseTelemetry()`
2. Add operation name: `.WithTelemetry("OperationName")`
3. Configure ActivitySource
4. Ensure OpenTelemetry is configured in app

**Solution:**
```csharp
services.AddFlow(config => config
    .UseTelemetry()
    .UseActivitySource("MyApp", "1.0.0"));

// Add per-pipeline
await Pipeline.Start(ctx)
    .WithTelemetry("CreateUser")
    .ExecuteAsync();
```

---

## Performance Considerations

1. **Async All the Way**: Use StepAsync, not Step for I/O operations
2. **Retry Overhead**: Each retry adds backoff delay
3. **Telemetry**: Minimal overhead with OpenTelemetry
4. **Transform**: Creates new pipeline builder, minimal cost
5. **Tap**: Lightweight, use freely for side effects

---

## Integration with Myth Ecosystem

### With Myth.Commons

```csharp
using Myth.Extensions;
using Myth.ServiceProvider;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFlow();
var app = builder.BuildApp();  // Initializes MythServiceProvider
```

### With Myth.Guard

```csharp
.StepResultAsync(async ctx => {
    var validationResult = await _validator.ValidateAndReturnAsync(ctx.Data);
    return validationResult.IsValid
        ? Result<MyContext>.Success(ctx)
        : Result<MyContext>.Failure(validationResult.Errors.First().Message);
})
```

### With Myth.Repository

```csharp
.StepAsync(async ctx => {
    ctx.User = await _userRepository.FirstOrDefaultAsync(u => u.Id == ctx.UserId);
    return ctx;
})
```

---

## Summary

Myth.Flow provides:

- ✅ **Fluent Pipeline API**: Intuitive, chainable method calls
- ✅ **Result Pattern**: Type-safe success/failure handling
- ✅ **OpenTelemetry**: Built-in distributed tracing
- ✅ **Retry Policies**: Automatic retry with exponential backoff
- ✅ **Transformations**: Change context types mid-pipeline
- ✅ **Side Effects**: Tap for non-modifying operations
- ✅ **Conditional Logic**: Execute branches based on predicates
- ✅ **Cancellation**: Full CancellationToken support
- ✅ **Exception Filtering**: Control exception propagation

---

## Additional Resources

- **Repository**: https://gitlab.com/dotnet-myth/myth
- **License**: Apache 2.0
- **Target Framework**: .NET 8.0
- **NuGet Package**: Myth.Flow

---

*This documentation is maintained for AI agents and developers. For questions or contributions, please refer to the repository.*
