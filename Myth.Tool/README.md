<img  style="float: right;" src="myth-tool-logo.png" alt="drawing" width="250"/>

# Myth.Tool - Code Generation CLI

A modern CLI tool for generating Myth architecture code with CQRS, DDD, and Clean Architecture patterns.

## Features

- **Project Setup**: Initialize Myth project structure from templates
- **Domain Models**: Generate domain models with validation and value objects
- **CQRS Commands**: Create commands with handlers and validation
- **CQRS Queries**: Generate queries with handlers and DTOs
- **CQRS Events**: Create domain events with handlers
- **Repository Pattern**: Generate repositories with read/write separation
- **API Controllers**: Create REST API controllers with CRUD operations
- **Unit Tests**: Generate comprehensive test suites with xUnit and FluentAssertions

## Installation

Install as a global .NET tool:

```bash
dotnet tool install -g Myth.Tool
```

Update to latest version:

```bash
dotnet tool update -g Myth.Tool
```

## Quick Start

Setup a new Myth project:

```bash
myth setup MyProject --clean
```

Create a domain model:

```bash
myth create model WeatherForecast \
  -p WeatherForecastId:Guid \
  -p Date:DateOnly \
  -p TemperatureF:int \
  -p Summary:string \
  --validate
```

Create a command with handler:

```bash
myth create command WeatherForecast CreateWeatherForecast \
  -p Date:DateOnly \
  -p TemperatureF:int \
  -p Summary:string \
  --return Guid \
  --validate
```

## Commands

### Project Management

- `myth setup <ProjectName> [--clean]` - Setup Myth project structure from template
- `myth version` - Display version information

### Code Generation

- `myth create model <name>` - Generate domain entity
- `myth create command <aggregate> <name>` - Generate CQRS command with handler
- `myth create query <aggregate> <name>` - Generate CQRS query with handler
- `myth create event <aggregate> <name>` - Generate domain event with handler
- `myth create dto <aggregate> <name>` - Generate data transfer object
- `myth create repository <name>` - Generate repository interfaces and implementations
- `myth create controller <name>` - Generate API controller with CRUD operations
- `myth create test <controller>` - Generate unit tests for controllers

## Options

### Global Options
- `--dry-run` - Preview changes without creating files
- `--force` - Overwrite existing files
- `--path <path>` - Specify target directory
- `--namespace <namespace>` - Custom namespace

### Setup Options
- `--clean` - Remove WeatherForecast examples and create clean base context

### Create Options
- `-p <property>` - Add property (format: Name:Type or Name:Type:required)
- `--return <type>` - Specify return type for commands/queries
- `--validate` - Enable validation support
- `--events <events>` - Specify events to publish (comma-separated)
- `--type <type>` - Repository type: read|write|readwrite (default: readwrite)

## Examples

### Project Setup

```bash
# Setup new project with clean structure
myth setup OrderManagement --clean

# Setup project keeping examples
myth setup OrderManagement
```

### Complete CRUD Generation

```bash
# Generate domain model
myth create model User \
  -p Id:Guid \
  -p Name:string:required \
  -p Email:string:required \
  -p CreatedAt:DateTime \
  --validate

# Generate command
myth create command User CreateUser \
  -p Name:string:required \
  -p Email:string:required \
  --return Guid \
  --validate \
  --events UserCreated

# Generate query
myth create query User GetUser \
  -p Id:Guid:required \
  --return GetUserResponse

# Generate repository
myth create repository User --type readwrite

# Generate controller
myth create controller User

# Generate tests
myth create test UserController
```

### Advanced Examples

```bash
# Create read-only repository
myth create repository UserRead --type read

# Create command with multiple events
myth create command Order ProcessOrder \
  -p OrderId:Guid:required \
  -p Status:string:required \
  --return bool \
  --events OrderProcessed,OrderStatusChanged

# Create DTO for complex data transfer
myth create dto Order CreateOrderRequest \
  -p CustomerId:Guid:required \
  -p Items:"List<OrderItemDto>":required \
  -p DeliveryDate:DateTime
```

## Project Structure

The tool generates code following Clean Architecture principles:

```
YourProject/
├── 🏗️ YourProject.Api/                    # Web API Layer
│   └── Controllers/                       # REST API Controllers
├── 🎯 YourProject.Domain/                 # Domain Layer
│   ├── Entities/                          # Domain Models
│   └── Events/                            # Domain Events
├── 🔄 YourProject.Application/            # Application Layer
│   ├── Commands/                          # CQRS Commands
│   ├── Queries/                           # CQRS Queries
│   ├── Handlers/                          # Command/Query/Event Handlers
│   └── DTOs/                              # Data Transfer Objects
├── 💾 YourProject.Data/                   # Data Access Layer
│   ├── Repositories/                      # Repository Implementations
│   └── Context/                           # Entity Framework Context
└── 🧪 YourProject.Test/                   # Test Projects
    └── Controllers/                       # Unit Tests
```

## Generated Code Features

### Integration with Myth Framework

Generated code uses:
- **Myth.Flow** - Pipeline pattern for business logic orchestration
- **Myth.Flow.Actions** - CQRS implementation (ICommand, IQuery, IEvent, IDispatcher)
- **Myth.Guard** - Fluent validation (IValidatable, ValidationBuilder)
- **Myth.Repository** - Repository pattern with read/write separation
- **Myth.Commons** - Common extensions and utilities

### Command Handlers
- Implement `ICommandHandler<TCommand>` or `ICommandHandler<TCommand, TResponse>`
- Support validation via Myth.Guard
- Event publishing via IDispatcher
- Pipeline integration for complex workflows

### Query Handlers
- Implement `IQueryHandler<TQuery, TResponse>`
- Repository integration for data access
- Support for caching and optimization

### Domain Events
- Implement `IEvent` interface
- Event handlers implement `IEventHandler<TEvent>`
- Async event processing support

### Repositories
- Support read/write separation
- Generic repository pattern
- Entity Framework Core integration
- Unit of Work pattern

### Controllers
- Full CRUD operations
- Async/await patterns
- Proper HTTP status codes
- Request/response DTOs

### Tests
- xUnit framework with FluentAssertions
- In-memory database testing
- Service mocking and dependency injection
- Inherits from `BaseDatabaseTests<TContext>`

## Contributing

This tool is part of the Myth framework. For contributions and issues, please visit the [Myth repository](https://gitlab.com/dotnet-myth/myth).

## License

Licensed under the Apache 2.0 License.
