# Myth.Tool - Code Generation CLI

A modern CLI tool for generating Myth architecture code with CQRS, DDD, and Clean Architecture patterns.

## Features

- **Domain Models**: Generate domain models with validation and value objects
- **CQRS Commands**: Create commands with handlers and validation
- **CQRS Queries**: Generate queries with handlers and DTOs
- **CQRS Events**: Create domain events with handlers
- **Repository Pattern**: Generate repositories and data access layer
- **API Controllers**: Create REST API controllers with CRUD operations
- **Unit Tests**: Generate comprehensive test suites

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

Initialize a new Myth project:

```bash
myth init
```

Create a domain model:

```bash
myth create model WeatherForecast \
  -p WeatherForecastId:Guid:get \
  -p Date:DateOnly \
  -p TemperatureF:int \
  -p Summary:Summary:get \
  --validate
```

Create a command:

```bash
myth create command WeatherForecast CreateWeatherForecast \
  -p Date:DateOnly \
  -p TemperatureF:int \
  -p Summary:Summary \
  --return Guid \
  --validate
```

## Commands

### Project Management

- `myth init` - Initialize Myth project structure
- `myth list templates` - List available templates
- `myth list artifacts` - List generated artifacts
- `myth validate` - Validate project structure

### Code Generation

- `myth create model <name>` - Generate domain model
- `myth create command <aggregate> <name>` - Generate CQRS command
- `myth create query <aggregate> <name>` - Generate CQRS query
- `myth create event <aggregate> <name>` - Generate domain event
- `myth create dto <aggregate> <name>` - Generate DTO
- `myth create repository <aggregate>` - Generate repository
- `myth create controller <aggregate>` - Generate API controller
- `myth create test <aggregate>` - Generate tests

## Options

- `--dry-run` - Preview changes without creating files
- `--force` - Overwrite existing files
- `--path <path>` - Specify target directory
- `--namespace <namespace>` - Custom namespace
- `--verbose` - Detailed logging

## Examples

```bash
# Create complete CRUD for an aggregate
myth create model User -p Id:Guid -p Name:string -p Email:string --validate
myth create command User CreateUser -p Name:string -p Email:string --return Guid
myth create query User GetUser -p Id:Guid --return GetUserResponse
myth create repository User
myth create controller User --include-crud

# Create with custom properties
myth create model Order \
  -p OrderId:Guid:get \
  -p CustomerId:Guid \
  -p OrderDate:DateTime \
  -p Status:OrderStatus:get \
  --validate

# Create with relationships
myth create command Order CreateOrder \
  -p CustomerId:Guid \
  -p Items:"List<CreateOrderItemDto>" \
  --return Guid \
  --event OrderCreated
```

## Project Structure

The tool generates code following Clean Architecture principles:

```
YourProject/
├── 🏗️ YourProject.Api/                    # Web API Layer
├── 🎯 YourProject.Domain/                 # Domain Layer
├── 🔄 YourProject.Application/            # Application Layer
├── 💾 YourProject.Data/                   # Data Access Layer
├── 🌐 YourProject.ExternalData/           # External Integrations
└── 🧪 YourProject.Test/                   # Test Projects
```

## Contributing

This tool is part of the Myth framework. For contributions and issues, please visit the [Myth repository](https://gitlab.com/dotnet-myth/myth).

## License

Licensed under the Apache 2.0 License.