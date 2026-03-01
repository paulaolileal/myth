---
name: myth-tool
description: Use when you need to scaffold code via CLI. Commands: myth setup <ProjectName> for Clean Architecture structure; myth create model/command/query/event/dto/repository/controller/test <aggregate> <name> to generate production-ready Myth-integrated code. Uses Scriban templates. Install globally with: dotnet tool install -g Myth.Tool.
---

# Myth.Tool - Code Generation CLI

## Overview

Myth.Tool is a modern .NET CLI tool for generating production-ready code following CQRS, DDD, and Clean Architecture patterns. It scaffolds complete project structures, generates commands/queries/events with handlers, creates repositories with read/write separation, and auto-generates controllers and tests in seconds.

**Key Features:**
- **Project Scaffolding**: Initialize complete Clean Architecture project structure
- **Domain Models**: Generate entities with validation and value objects
- **CQRS Support**: Auto-generate commands, queries, and events with handlers
- **Repository Pattern**: Create repositories with read/write separation
- **API Controllers**: Generate REST API controllers with CRUD operations
- **Test Generation**: Auto-generate xUnit test suites with FluentAssertions
- **Myth Integration**: Generated code uses Myth.Flow, Myth.Guard, Myth.Repository, Myth.Flow.Actions
- **Template-Based**: Uses Scriban templates for customizable code generation

**Dependencies:**
- .NET 10.0+
- Scriban 6.0.0 (template engine)
- Generates code using: Myth.Flow, Myth.Flow.Actions, Myth.Guard, Myth.Repository, Myth.Commons

---

## Installation

Install as a global .NET tool:

```bash
dotnet tool install -g Myth.Tool
```

Update to latest version:

```bash
dotnet tool update -g Myth.Tool
```

Verify installation:

```bash
myth version
```

---

## CLI Commands

### Project Management

```bash
# Initialize new Myth project with Clean Architecture structure
myth setup <ProjectName> [--clean]

# Display version information
myth version
```

### Code Generation

```bash
# Generate domain entity
myth create model <name> [options]

# Generate CQRS command with handler
myth create command <aggregate> <name> [options]

# Generate CQRS query with handler
myth create query <aggregate> <name> [options]

# Generate domain event with handler
myth create event <aggregate> <name> [options]

# Generate data transfer object
myth create dto <aggregate> <name> [options]

# Generate repository (interfaces and implementations)
myth create repository <name> [options]

# Generate API controller with CRUD operations
myth create controller <name> [options]

# Generate unit tests for controller
myth create test <controller> [options]
```

---

## Options

### Global Options

```bash
--dry-run              # Preview changes without creating files
--force                # Overwrite existing files
--path <path>          # Specify target directory
--namespace <namespace> # Custom namespace
```

### Setup Options

```bash
--clean                # Remove WeatherForecast examples and create clean base context
```

### Create Options

```bash
-p <property>          # Add property (format: Name:Type or Name:Type:required)
--return <type>        # Specify return type for commands/queries
--validate             # Enable validation support
--events <events>      # Specify events to publish (comma-separated)
--type <type>          # Repository type: read|write|readwrite (default: readwrite)
```

---

## Usage Examples

### 1. Project Setup

```bash
# Setup new project with clean structure (no examples)
myth setup OrderManagement --clean

# Setup project keeping WeatherForecast examples
myth setup OrderManagement
```

**Generated Structure:**
```
OrderManagement/
├── OrderManagement.Api/            # Web API Layer
│   └── Controllers/                # REST API Controllers
├── OrderManagement.Domain/         # Domain Layer
│   ├── Entities/                   # Domain Models
│   └── Events/                     # Domain Events
├── OrderManagement.Application/    # Application Layer
│   ├── Commands/                   # CQRS Commands
│   ├── Queries/                    # CQRS Queries
│   ├── Handlers/                   # Handlers
│   └── DTOs/                       # Data Transfer Objects
├── OrderManagement.Data/           # Data Access Layer
│   ├── Repositories/               # Repository Implementations
│   └── Context/                    # EF Core Context
└── OrderManagement.Test/           # Test Projects
    └── Controllers/                # Unit Tests
```

### 2. Generate Domain Model

```bash
myth create model User \
  -p Id:Guid \
  -p Name:string:required \
  -p Email:string:required \
  -p Age:int \
  -p CreatedAt:DateTime \
  --validate
```

**Generated Code:**
```csharp
using Myth.Interfaces;
using Myth.Builders;

namespace YourProject.Domain.Entities;

public class User : IValidatable<User> {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Validate(ValidationBuilder<User> builder, ValidationContextKey? context = null) {
        builder.For(Name, x => x.NotEmpty().MaximumLength(200));
        builder.For(Email, x => x.NotEmpty().Email().MaximumLength(255));
        builder.For(Age, x => x.GreaterThanOrEqualTo(0).LessThan(150));
    }
}
```

### 3. Generate Command with Handler

```bash
myth create command User CreateUser \
  -p Name:string:required \
  -p Email:string:required \
  -p Age:int \
  --return Guid \
  --validate \
  --events UserCreated
```

**Generated Command:**
```csharp
using Myth.Interfaces;
using Myth.Models;

namespace YourProject.Application.Commands;

public record CreateUserCommand(
    string Name,
    string Email,
    int Age
) : ICommand<Guid>;
```

**Generated Handler:**
```csharp
using Myth.Interfaces;
using Myth.Models;
using YourProject.Domain.Entities;
using YourProject.Data.Repositories;

namespace YourProject.Application.Handlers;

public class CreateUserHandler : ICommandHandler<CreateUserCommand, Guid> {
    private readonly IUserRepository _repository;
    private readonly IValidator _validator;
    private readonly IDispatcher _dispatcher;

    public CreateUserHandler(
        IUserRepository repository,
        IValidator validator,
        IDispatcher dispatcher) {
        _repository = repository;
        _validator = validator;
        _dispatcher = dispatcher;
    }

    public async Task<CommandResult<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken) {
        try {
            // Validate command
            await _validator.ValidateAsync(command, ValidationContextKey.Create, cancellationToken);

            // Create entity
            var user = new User {
                Id = Guid.NewGuid(),
                Name = command.Name,
                Email = command.Email,
                Age = command.Age,
                CreatedAt = DateTime.UtcNow
            };

            // Persist
            await _repository.AddAsync(user, cancellationToken);

            // Publish event
            await _dispatcher.PublishEventAsync(new UserCreatedEvent {
                EventId = Guid.NewGuid().ToString(),
                OccurredAt = DateTimeOffset.UtcNow,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            }, cancellationToken);

            return CommandResult<Guid>.Success(user.Id);
        } catch (Exception ex) {
            return CommandResult<Guid>.Failure(ex.Message, ex);
        }
    }
}
```

### 4. Generate Query with Handler

```bash
myth create query User GetUser \
  -p Id:Guid:required \
  --return GetUserResponse
```

**Generated Query:**
```csharp
using Myth.Interfaces;
using Myth.Models;

namespace YourProject.Application.Queries;

public record GetUserQuery(
    Guid Id
) : IQuery<GetUserResponse>;
```

**Generated Handler:**
```csharp
using Myth.Interfaces;
using Myth.Models;
using YourProject.Data.Repositories;

namespace YourProject.Application.Handlers;

public class GetUserHandler : IQueryHandler<GetUserQuery, GetUserResponse> {
    private readonly IUserRepository _repository;

    public GetUserHandler(IUserRepository repository) {
        _repository = repository;
    }

    public async Task<QueryResult<GetUserResponse>> HandleAsync(
        GetUserQuery query,
        CancellationToken cancellationToken) {
        try {
            var user = await _repository.GetByIdAsync(query.Id, cancellationToken);

            if (user == null) {
                return QueryResult<GetUserResponse>.Failure($"User with ID {query.Id} not found");
            }

            var response = new GetUserResponse {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Age = user.Age,
                CreatedAt = user.CreatedAt
            };

            return QueryResult<GetUserResponse>.Success(response);
        } catch (Exception ex) {
            return QueryResult<GetUserResponse>.Failure(ex.Message, ex);
        }
    }
}
```

### 5. Generate Event with Handler

```bash
myth create event User UserCreated \
  -p UserId:Guid:required \
  -p Name:string:required \
  -p Email:string:required
```

**Generated Event:**
```csharp
using Myth.Interfaces;

namespace YourProject.Domain.Events;

public record UserCreatedEvent : IEvent {
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
```

**Generated Event Handler:**
```csharp
using Myth.Interfaces;
using YourProject.Domain.Events;

namespace YourProject.Application.Handlers;

public class UserCreatedHandler : IEventHandler<UserCreatedEvent> {
    private readonly ILogger<UserCreatedHandler> _logger;

    public UserCreatedHandler(ILogger<UserCreatedHandler> logger) {
        _logger = logger;
    }

    public async Task HandleAsync(
        UserCreatedEvent @event,
        CancellationToken cancellationToken) {
        _logger.LogInformation(
            "User created: {UserId} - {Name} ({Email})",
            @event.UserId,
            @event.Name,
            @event.Email);

        // Add event handling logic here
        await Task.CompletedTask;
    }
}
```

### 6. Generate Repository

```bash
# Generate repository with read/write separation
myth create repository User --type readwrite

# Generate read-only repository
myth create repository UserRead --type read

# Generate write-only repository
myth create repository UserWrite --type write
```

**Generated Interface:**
```csharp
using YourProject.Domain.Entities;

namespace YourProject.Data.Repositories;

public interface IUserRepository : IReadRepositoryAsync<User>, IWriteRepositoryAsync<User> {
    // Custom repository methods here
}
```

**Generated Implementation:**
```csharp
using Microsoft.EntityFrameworkCore;
using YourProject.Domain.Entities;
using YourProject.Data.Context;

namespace YourProject.Data.Repositories;

public class UserRepository : ReadWriteRepositoryAsync<User>, IUserRepository {
    public UserRepository(YourProjectDbContext context) : base(context) {
    }

    // Implement custom repository methods here
}
```

### 7. Generate API Controller

```bash
myth create controller User
```

**Generated Controller:**
```csharp
using Microsoft.AspNetCore.Mvc;
using Myth.Interfaces;
using YourProject.Application.Commands;
using YourProject.Application.Queries;

namespace YourProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase {
    private readonly IDispatcher _dispatcher;
    private readonly ILogger<UserController> _logger;

    public UserController(IDispatcher dispatcher, ILogger<UserController> logger) {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken) {
        var result = await _dispatcher.DispatchQueryAsync<GetUserQuery, GetUserResponse>(
            new GetUserQuery(id),
            cancellationToken: cancellationToken);

        return result.IsSuccess
            ? Ok(result.Data)
            : NotFound(result.ErrorMessage);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken) {
        var result = await _dispatcher.DispatchCommandAsync<CreateUserCommand, Guid>(
            command,
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetUser), new { id = result.Data }, result.Data)
            : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken) {
        var result = await _dispatcher.DispatchCommandAsync(
            command with { Id = id },
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken) {
        var result = await _dispatcher.DispatchCommandAsync(
            new DeleteUserCommand(id),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : NotFound(result.ErrorMessage);
    }
}
```

### 8. Generate Tests

```bash
myth create test UserController
```

**Generated Test Class:**
```csharp
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using YourProject.Api.Controllers;
using YourProject.Application.Commands;
using YourProject.Application.Queries;

namespace YourProject.Test.Controllers;

public class UserControllerTests : BaseDatabaseTests<YourProjectDbContext> {
    private readonly UserController _controller;
    private readonly Mock<IDispatcher> _mockDispatcher;

    public UserControllerTests() {
        _mockDispatcher = new Mock<IDispatcher>();

        AddService<IDispatcher>(_mockDispatcher.Object);
        AddService<UserController, UserController>();

        _controller = GetRequiredService<UserController>();
    }

    [Fact]
    public async Task GetUser_WithValidId_ShouldReturnOk() {
        // Arrange
        var userId = _faker.Random.Guid();
        var response = new GetUserResponse {
            Id = userId,
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Age = _faker.Random.Int(18, 65),
            CreatedAt = DateTime.UtcNow
        };

        _mockDispatcher
            .Setup(d => d.DispatchQueryAsync<GetUserQuery, GetUserResponse>(
                It.IsAny<GetUserQuery>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(QueryResult<GetUserResponse>.Success(response));

        // Act
        var result = await _controller.GetUser(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task CreateUser_WithValidCommand_ShouldReturnCreated() {
        // Arrange
        var command = new CreateUserCommand(
            _faker.Name.FullName(),
            _faker.Internet.Email(),
            _faker.Random.Int(18, 65)
        );

        var userId = _faker.Random.Guid();

        _mockDispatcher
            .Setup(d => d.DispatchCommandAsync<CreateUserCommand, Guid>(
                It.IsAny<CreateUserCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CommandResult<Guid>.Success(userId));

        // Act
        var result = await _controller.CreateUser(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult.Value.Should().Be(userId);
    }
}
```

### 9. Complete CRUD Generation Workflow

```bash
# 1. Setup project
myth setup UserManagement --clean

# 2. Generate domain model
myth create model User \
  -p Id:Guid \
  -p Name:string:required \
  -p Email:string:required \
  -p Age:int \
  -p IsActive:bool \
  -p CreatedAt:DateTime \
  --validate

# 3. Generate commands
myth create command User CreateUser \
  -p Name:string:required \
  -p Email:string:required \
  -p Age:int \
  --return Guid \
  --validate \
  --events UserCreated

myth create command User UpdateUser \
  -p Id:Guid:required \
  -p Name:string:required \
  -p Email:string:required \
  -p Age:int \
  --validate

myth create command User DeleteUser \
  -p Id:Guid:required

# 4. Generate queries
myth create query User GetUser \
  -p Id:Guid:required \
  --return GetUserResponse

myth create query User ListUsers \
  -p Page:int \
  -p PageSize:int \
  --return "List<UserListItemDto>"

# 5. Generate repository
myth create repository User --type readwrite

# 6. Generate controller
myth create controller User

# 7. Generate tests
myth create test UserController
```

### 10. Advanced Examples

```bash
# Create command with multiple events
myth create command Order ProcessOrder \
  -p OrderId:Guid:required \
  -p Status:string:required \
  --return bool \
  --events OrderProcessed,OrderStatusChanged,InventoryUpdated

# Create DTO for complex data transfer
myth create dto Order CreateOrderRequest \
  -p CustomerId:Guid:required \
  -p Items:"List<OrderItemDto>":required \
  -p ShippingAddress:"AddressDto":required \
  -p DeliveryDate:DateTime

# Create domain model with value objects
myth create model Order \
  -p Id:Guid \
  -p CustomerId:Guid:required \
  -p OrderNumber:"OrderNumber" \
  -p TotalAmount:"Money":required \
  -p Status:"OrderStatus":required \
  -p CreatedAt:DateTime \
  --validate

# Preview changes without creating files
myth create model Product \
  -p Id:Guid \
  -p Name:string:required \
  --dry-run

# Force overwrite existing files
myth create command User CreateUser \
  -p Name:string \
  --force
```

---

## Generated Code Integration

All generated code integrates seamlessly with Myth libraries:

### Myth.Flow.Actions Integration

```csharp
// Commands implement ICommand or ICommand<TResponse>
public record CreateUserCommand(...) : ICommand<Guid>;

// Handlers implement ICommandHandler<TCommand> or ICommandHandler<TCommand, TResponse>
public class CreateUserHandler : ICommandHandler<CreateUserCommand, Guid> {
    // Uses IDispatcher for event publishing
    private readonly IDispatcher _dispatcher;
}

// Queries implement IQuery<TResponse>
public record GetUserQuery(...) : IQuery<GetUserResponse>;

// Query handlers implement IQueryHandler<TQuery, TResponse>
public class GetUserHandler : IQueryHandler<GetUserQuery, GetUserResponse> { }

// Events implement IEvent
public record UserCreatedEvent : IEvent {
    public string EventId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}

// Event handlers implement IEventHandler<TEvent>
public class UserCreatedHandler : IEventHandler<UserCreatedEvent> { }
```

### Myth.Guard Integration

```csharp
// Entities implement IValidatable<T>
public class User : IValidatable<User> {
    public void Validate(ValidationBuilder<User> builder, ValidationContextKey? context = null) {
        builder.For(Name, x => x.NotEmpty().MaximumLength(200));
        builder.For(Email, x => x.NotEmpty().Email());
    }
}

// Handlers use IValidator for validation
public class CreateUserHandler : ICommandHandler<CreateUserCommand, Guid> {
    private readonly IValidator _validator;

    public async Task<CommandResult<Guid>> HandleAsync(...) {
        await _validator.ValidateAsync(command, ValidationContextKey.Create, cancellationToken);
    }
}
```

### Myth.Repository Integration

```csharp
// Repositories use read/write separation
public interface IUserRepository : IReadRepositoryAsync<User>, IWriteRepositoryAsync<User> { }

// Implementation extends base repository classes
public class UserRepository : ReadWriteRepositoryAsync<User>, IUserRepository {
    public UserRepository(YourProjectDbContext context) : base(context) { }
}
```

### Myth.Testing Integration

```csharp
// Tests inherit from BaseDatabaseTests<TContext>
public class UserControllerTests : BaseDatabaseTests<YourProjectDbContext> {
    // Uses _faker for test data generation
    // Uses FluentAssertions for assertions
    // Uses Moq for mocking
}
```

---

## Best Practices

### 1. Project Organization

**✅ DO:**
- Use `--clean` flag for production projects
- Follow Clean Architecture folder structure
- Generate tests for all controllers
- Use meaningful aggregate names

**❌ DON'T:**
- Mix domain logic in controllers
- Skip test generation
- Use generic names like "Data" or "Item"

### 2. Property Naming

**✅ DO:**
- Use PascalCase for property names
- Mark required properties with `:required`
- Use appropriate C# types
- Follow naming conventions

**❌ DON'T:**
- Use camelCase or snake_case
- Forget to mark required properties
- Use language-specific types (use int instead of Int32)

### 3. Code Generation Workflow

**✅ DO:**
- Generate in order: Model → Commands/Queries → Events → Repository → Controller → Tests
- Use `--validate` for entities and commands
- Specify events to publish in commands
- Use `--dry-run` to preview changes

**❌ DON'T:**
- Generate controller before repository
- Skip validation setup
- Forget to specify return types

### 4. Event Publishing

**✅ DO:**
- Publish events after successful operations
- Use past-tense event names (UserCreated, OrderProcessed)
- Include relevant data in events

**❌ DON'T:**
- Use present-tense names (CreateUser, ProcessOrder)
- Publish events before persistence
- Include sensitive data in events

---

## Customization

Generated code can be customized by modifying Scriban templates:

### Template Locations

```
Myth.Tool/
└── Templates/
    ├── Model.sbn-tpl            # Domain entity template
    ├── Command.sbn-tpl          # Command template
    ├── CommandHandler.sbn-tpl   # Command handler template
    ├── Query.sbn-tpl            # Query template
    ├── QueryHandler.sbn-tpl     # Query handler template
    ├── Event.sbn-tpl            # Event template
    ├── EventHandler.sbn-tpl     # Event handler template
    ├── Repository.sbn-tpl       # Repository template
    ├── Controller.sbn-tpl       # Controller template
    └── Test.sbn-tpl             # Test template
```

---

## Troubleshooting

### Issue: Tool not found after installation

**Solution:**
```bash
# Ensure .NET tools path is in your PATH
export PATH="$PATH:$HOME/.dotnet/tools"  # Linux/macOS
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"  # Windows PowerShell

# Verify installation
dotnet tool list -g
```

### Issue: Generated files have incorrect namespace

**Solution:**
```bash
# Specify custom namespace
myth create model User --namespace MyCompany.UserManagement.Domain

# Or update after generation manually
```

### Issue: Template not found errors

**Solution:**
```bash
# Reinstall the tool
dotnet tool uninstall -g Myth.Tool
dotnet tool install -g Myth.Tool
```

---

## Summary

Myth.Tool accelerates development by generating production-ready code following industry-standard patterns:

- **10x Faster Development**: Days of setup → minutes
- **Consistent Patterns**: All code follows Myth architecture
- **Complete CRUD**: Generate models, commands, queries, repositories, controllers, and tests
- **Testing Included**: Auto-generated xUnit test suites
- **Myth Integration**: Uses Myth.Flow, Myth.Guard, Myth.Repository, Myth.Flow.Actions
- **Customizable**: Scriban templates for flexible code generation

Use Myth.Tool to bootstrap new projects, add features quickly, and maintain consistent code quality across your team.
