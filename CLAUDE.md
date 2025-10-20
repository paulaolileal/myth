# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Myth is a collection of .NET libraries providing reusable functionality for enterprise applications. The codebase is organized as a multi-project solution with core libraries and their corresponding test projects.

## Solution Structure

The solution follows a feature-based folder structure with solution folders:
- **Commons**: Base functionality (Myth.Commons)
- **DependencyInjection**: DI and assembly scanning (Myth.DependencyInjection, Myth.DependencyInjection.Providers)
- **Repository**: Data access patterns (Myth.Repository, Myth.Repository.EntityFramework)
- **Specification**: Query specification pattern (Myth.Specification)
- **Rest**: HTTP communication (Myth.Rest)
- **Morph**: Object transformation and mapping (Myth.Morph)
- **Guard**: Fluent validation and data integrity (Myth.Guard)
- **Flow**: Pipeline orchestration with two key libraries:
  - Myth.Flow: Fluent pipelines with Result pattern
  - Myth.Flow.Actions: CQRS/Event-driven architecture with dispatcher, event bus, and message brokers

Each library has a corresponding `.Test` project using xUnit and FluentAssertions.

## Build Commands

```bash
# Build entire solution
dotnet build

# Build specific configuration
dotnet build --configuration Release

# Build for x64 platform (some projects configured for x64)
dotnet build --configuration Debug
```

## Testing

```bash
# Run all tests
dotnet test

# Run tests excluding Rest tests (as done in CI)
dotnet test --filter "FullyQualifiedName!~Rest"

# Run tests with code coverage
dotnet test --collect "XPlat Code Coverage" --results-directory "Coverage"

# Run tests for specific project
dotnet test Myth.Commons.Test/Myth.Commons.Test.csproj

# Run single test by fully qualified name
dotnet test --filter "FullyQualifiedName=Namespace.ClassName.TestMethodName"

# Run tests matching pattern
dotnet test --filter "FullyQualifiedName~Pattern"
```

Test projects use:
- **xUnit** as the test framework
- **FluentAssertions** for assertion syntax
- **coverlet.collector** for code coverage
- Target framework: .NET 8.0

## Architecture Patterns

### Myth.Flow - Pipeline Pattern
- Fluent API for building data processing pipelines
- Result pattern with `Result<T>` and `Result<TContext>.Success/Failure`
- Built-in retry policies with exponential backoff
- OpenTelemetry integration for distributed tracing
- Dependency injection support via `Pipeline.Start(context, serviceProvider)`
- Use `.Step()` for synchronous, `.StepAsync()` for async, `.StepResultAsync()` for Result-returning operations
- `.Tap()` and `.TapAsync()` for side effects (logging, metrics, events)
- `.When()` for conditional execution
- `.Transform()` for context type transformations
- Configuration: `services.AddFlow(config => { ... })`

### Myth.Flow.Actions - CQRS/Event-Driven Architecture
- **IDispatcher**: Central dispatcher for commands, queries, and events
  - `DispatchCommandAsync<TCommand>()` - executes commands
  - `DispatchQueryAsync<TQuery, TResponse>()` - handles queries with optional caching
  - `PublishEventAsync<TEvent>()` - publishes events
- **IEventBus**: Event publishing and subscription system with multiple broker support
  - In-memory broker for testing
  - RabbitMQ broker for production messaging
  - Kafka broker for high-throughput event streaming
- **Circuit breaker** and **retry policies** for resilience
- **Dead letter queue** for failed message handling
- **Cache providers**: Memory and Redis support
- Configuration: `services.AddFlowActions(builder => { ... })`
- Use `ICommandHandler<TCommand>`, `IQueryHandler<TQuery, TResponse>`, `IEventHandler<TEvent>` interfaces

### Myth.Specification - Query Specification Pattern
- Build queries using `SpecBuilder<T>.Create()`
- Chain filters: `.And()`, `.Or()`, `.Not()`, `.AndIf()`, `.OrIf()`
- Ordering: `.Order()`, `.OrderDescending()`
- Pagination: `.Skip()`, `.Take()`, `.DistinctBy()`
- Extension methods: `.Filter()`, `.Sort()`, `.Paginate()`, `.Specify()`
- Keep business rules in specification extension methods

### Myth.Morph - Object Transformation
- Transform objects using `.To<TDestination>()` extension method
- Check mapping availability: `.CanBindTo<TDestination>()`
- Async transformations: `.ToAsync<TDestination>()`
- Implement `IMorphable<TDestination>` for custom mappings
- Use `Schema<T>` for property binding: `.Bind()`, `.BindAsync()`, `.Ignore()`
- Configuration: `services.AddMorph(settings => { ... })`

### Myth.Guard - Fluent Validation
- **Declarative, context-aware validation** for entities and DTOs
- **Fluent API** with chainable validation rules for all common types
- **Context-based validation**: Different rules per operation (Create, Update, Delete, etc.)
- **Type-specific rules**: String, numeric, collection, DateTime, boolean, and enum validation
- **Async validation** with service provider access for database/API checks
- **ASP.NET Core middleware** for automatic validation exception handling
- **Structured error responses** with field-level details and HTTP status codes
- Entities implement `IValidatable<T>` with `Validate(ValidationBuilder<T> builder, ValidationContextKey? context)` method
- Use `IValidator.ValidateAsync()` to validate and throw, or `ValidateAndReturnAsync()` for result checking
- Configuration: `services.AddGuard()` and `app.UseGuard()` for middleware
- Pre-defined contexts: `ValidationContextKey.Create`, `.Update`, `.Delete`, `.Search`, etc.
- Custom rules: `.Respect()` for sync predicates, `.RespectAsync()` for async with service access
- Rule modifiers: `.WithMessage()`, `.WithCode()`, `.WithStatusCode()`, `.When()`, `.Unless()`, `.SetStopOnFailure()`

### Myth.Repository
- Generic repository interfaces: `IRepository<TEntity>`
- Read/Write separation patterns
- Pagination support
- Works with Myth.Specification for querying

### Myth.DependencyInjection
- **TypeProvider**: Access application assemblies and types
  - `TypeProvider.ApplicationAssemblies` - get all loaded assemblies
  - `TypeProvider.ApplicationTypes` - get all types from application assemblies
- **Auto-registration**: `services.AddServiceFromType<IInterface>()` - automatically finds and registers implementations

### Myth.Commons
- JSON serialization/deserialization extensions
- String manipulation utilities
- Base classes for Value Objects and Constants
- URL extension methods

## Key Conventions

### Namespace Organization
- All projects use `Myth` as the root namespace (configured via `<RootNamespace>Myth</RootNamespace>`)
- Internal structure follows: `Myth.[Category].[Subcategory]`
- Example: `Myth.Flow.Actions`, `Myth.Repository.EntityFramework`

### Testing Conventions
- Test projects named `{ProjectName}.Test`
- Tests use FluentAssertions: `.Should().Be()`, `.Should().NotBeNull()`
- Async test methods end with `Async`
- Use `CancellationToken` parameters in async operations

### Result Pattern Usage
- **Myth.Flow**: Use `Result<TContext>` for pipeline steps returning success/failure
  - Check: `result.IsSuccess`, `result.IsFailure`
  - Access: `result.Value`, `result.ErrorMessage`, `result.Exception`
  - Create: `Result<T>.Success(value)`, `Result<T>.Failure(message, exception)`
- **Myth.Flow.Actions**: Use `CommandResult`, `CommandResult<TResponse>`, `QueryResult<TResponse>`

### Dependency Injection Integration
- Most libraries require `services.Add{LibraryName}()` in Startup/Program.cs
- Services use constructor injection
- `IServiceProvider` often needed for factory patterns and dynamic resolution

## CI/CD Configuration

The project uses GitLab CI with three stages:
1. **build**: `dotnet build --configuration Release`
2. **test**: Runs tests excluding Rest tests, generates coverage reports
3. **publish**: Packs projects (excluding test projects) and pushes to NuGet on tags

Version is extracted from git tags (format: `v{version}`).

## Coding conventions 
- Do not create comments not useful 
- Name variables with significant names 
- Avoid repeating code 
- Write code with good spacing and readability 
- Functions must have only one responsibility
- Always follow SOLID and CLEAN CODE patterns 
- Every new method should have a summary XML comment explaining its purpose and use tags for parameters, exceptions and return values
- Avoid create more than one class or interface in the same file

## Code style
- Do not use brackets in if with only one line
- Use more spaces for better readability, like `public void Test( Cancellation Token )` or `var test = new List<Test> {`
- Use more empty lines to separate code blocks
- Always use one empty line before the return
- Always keep the open bracket in the same line of the method or class declaration

## Common Development Workflows

### Adding New Library
1. Create new project directory
2. Add to solution: Project and Test project
3. Create solution folder if needed
4. Add project references
5. Create README.md with features and usage examples
6. Ensure test project references xUnit, FluentAssertions, and coverlet.collector

### Working with Myth.Flow Pipelines
```csharp
var result = await Pipeline.Start(context)
    .WithTelemetry("OperationName")
    .WithRetry(maxAttempts: 3, backoffMs: 100)
    .StepResultAsync<ValidationService>((svc, ctx) => svc.ValidateAsync(ctx))
    .StepResultAsync<ProcessingService>((svc, ctx) => svc.ProcessAsync(ctx))
    .TapAsync<EventService>((svc, ctx) => svc.PublishEventAsync(ctx))
    .Transform<ResponseDto>(ctx => new ResponseDto { ... })
    .ExecuteAsync();
```

### Working with Myth.Flow.Actions
```csharp
// Dispatch command
var commandResult = await dispatcher.DispatchCommandAsync(new CreateOrderCommand { ... });

// Query with caching
var queryResult = await dispatcher.DispatchQueryAsync(
    new GetOrderQuery { OrderId = 123 },
    new CacheOptions { Enabled = true, Ttl = TimeSpan.FromMinutes(5) }
);

// Publish event
await dispatcher.PublishEventAsync(new OrderCreatedEvent { ... });
```

### Creating Specifications
```csharp
public static class PersonSpecifications {
    public static ISpec<Person> IsActive(this ISpec<Person> spec) {
        return spec.And(p => p.IsActive);
    }

    public static ISpec<Person> HasRole(this ISpec<Person> spec, string role) {
        return spec.And(p => p.Role == role);
    }
}

var spec = SpecBuilder<Person>.Create()
    .IsActive()
    .HasRole("Admin")
    .Order(p => p.Name)
    .Skip(10)
    .Take(20);
```

### Using Morph Transformations
```csharp
// Implement IMorphable
public class UserDto : IMorphable<User> {
    public void MorphTo(Schema<User> schema) {
        schema
            .Bind(u => u.FullName, () => Name)
            .BindAsync(u => u.Profile, async sp => {
                var service = sp.GetService<IProfileService>();
                return await service.GetProfileAsync(Email);
            })
            .Ignore(u => u.InternalId);
    }
}

// Transform
var user = userDto.To<User>(serviceProvider);
var users = await userDtos.ToAsync<User>(serviceProvider);
```

### Working with Myth.Guard Validation
```csharp
// Implement IValidatable on your entity/DTO
public class CreateUserDto : IValidatable<CreateUserDto> {
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public List<string> Tags { get; set; }

    public void Validate(ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context = null) {
        // Global rules (apply to all contexts)
        builder.For(Name, x => x.NotEmpty().MinimumLength(3).MaximumLength(100));
        builder.For(Email, x => x.NotEmpty().Email());
        builder.For(Age, x => x.GreaterThan(0).LessThan(150));
        builder.For(Tags, x => x.NotEmpty().CountBetween(1, 10));

        // Context-specific rules
        builder.InContext(ValidationContextKey.Create, b => {
            // Additional validation only for Create operations
            b.For(Email, x => x
                .RespectAsync(async (email, ct, sp) => {
                    var userService = sp.GetRequiredService<IUserService>();
                    return await userService.IsEmailAvailableAsync(email, ct);
                })
                .WithMessage("Email already exists")
                .WithCode("EMAIL_EXISTS"));
        });
    }
}

// Use in controller
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserDto dto) {
    // Validate and throw ValidationException on failure
    await _validator.ValidateAsync(dto, ValidationContextKey.Create);

    // Or validate and check result without throwing
    var result = await _validator.ValidateAndReturnAsync(dto, ValidationContextKey.Create);
    if (!result.IsValid) {
        return BadRequest(result.Errors);
    }

    // Process user creation...
}

// Configuration in Program.cs
services.AddGuard();  // Register validation services
app.UseGuard();       // Add middleware for exception handling
```

## Platform Targets

Most projects target .NET 8.0 (`net8.0`) with `AnyCPU` platform. Some older projects may have x64-specific configurations.

## Package Publishing

All non-test projects are configured as NuGet packages with:
- Apache 2.0 license
- Package icon: `Logo.jpg`
- Repository URL: https://gitlab.com/dotnet-myth/myth
- Each package has its own README.md included
