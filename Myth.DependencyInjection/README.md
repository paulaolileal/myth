<img  style="float: right;" src="myth-dependency-injection-logo.png" alt="drawing" width="250"/>

# Myth.DependencyInjection

[![NuGet Version](https://img.shields.io/nuget/v/Myth.DependencyInjection?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.DependencyInjection/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.DependencyInjection?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.DependencyInjection/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](README.md)

A .NET library that simplifies dependency injection with automatic type discovery and convention-based service registration. Eliminate boilerplate code and enable plugin architectures with powerful assembly scanning and type resolution capabilities.

## 🎯 Why Myth.DependencyInjection?

In enterprise .NET applications, **manual service registration becomes a productivity killer**. Imagine maintaining hundreds of repository, service, and handler registrations by hand—every new class requires three places to change (interface, implementation, registration). One forgotten registration causes runtime errors. Code reviews get cluttered with registration changes. **Myth.DependencyInjection solves this once and for all.**

### The Problem

**Manual Registration Hell**
```csharp
// Startup.cs becomes a nightmare to maintain
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<ICustomerRepository, CustomerRepository>();
services.AddScoped<IInvoiceRepository, InvoiceRepository>();
// ... 200 more lines ...
// New developer adds IPaymentRepository but forgets to register it → runtime crash
```

**Problems with manual registration:**
- **Maintenance burden**: Every new service = 3 places to change
- **Error-prone**: Forgotten registrations cause runtime failures
- **Messy PRs**: Code reviews filled with registration noise
- **Onboarding pain**: New developers must understand registration conventions
- **No discoverability**: Hard to know what's registered without reading Startup.cs

### The Solution

**Convention-Based Auto-Registration**
```csharp
// One line registers ALL repositories following your naming convention
services.AddServiceFromType<IRepository>();

// One line registers ALL command handlers
services.AddServiceFromType<ICommandHandler>(ServiceLifetime.Transient);
```

**Add a new repository?** Just follow the naming convention (`IUserRepository` → `UserRepository`). It's automatically discovered and registered. No Startup.cs changes. No forgotten registrations. **It just works.**

### Why Choose Myth.DependencyInjection?

| Aspect | Myth.DependencyInjection | Manual Registration | Scrutor/Other Tools |
|--------|--------------------------|---------------------|---------------------|
| **Lines of Code** | 1 line per layer | 1 line per service | Multiple configuration calls |
| **Maintenance** | Add new service, done | Update Startup.cs every time | Configure scanning rules |
| **Runtime Safety** | Convention enforced | Easy to forget | Depends on configuration |
| **Developer Experience** | Intuitive naming convention | Tedious and error-prone | Learning curve for API |
| **Type Discovery** | Built-in TypeProvider | Manual reflection | Limited or missing |
| **Plugin Support** | Native type scanning | Complex custom code | Not primary focus |
| **Performance** | Optimized assembly scanning | N/A (manual) | Varies |

### Real-World Applications

**CQRS-Based Microservices**
Auto-register 100+ command/query handlers without touching Startup.cs. Convention ensures all handlers follow the same pattern.

**Multi-Tenant SaaS Platforms**
Dynamically discover and register tenant-specific services. TypeProvider enables plugin-based multi-tenancy where each tenant can load custom implementations.

**Domain-Driven Design (DDD) Applications**
Auto-register repositories by layer (IRepository, IDomainService, IApplicationService). Clean separation without registration clutter.

**Modular Monolith Architecture**
Each module registers its services with one call. Adding a new module is as simple as calling `AddServiceFromType<IModuleService>()`.

**Plugin-Based Architectures**
Build extensible systems where plugins drop into a folder and are automatically discovered, loaded, and registered. No hardcoded plugin lists.

### Key Differentiators

🎯 **Convention Over Configuration**
Follow a simple naming convention (`IUserRepository` → `UserRepository`) and registration is automatic. No XML, no attributes, no manual wiring.

🔍 **Powerful Type Discovery**
TypeProvider gives you assembly scanning, type filtering, and namespace detection out of the box. Build code generators, analyzers, and plugin systems easily.

⚡ **Minimal Ceremony**
One line of code to register an entire layer. Reduces Startup.cs by 80-90% in large applications.

🏗️ **Architecture Enforcement**
Naming conventions enforce consistency. Teams naturally follow patterns because that's how services get registered.

📦 **Seamless Plugin Support**
Scan assemblies at runtime, discover types implementing `IPlugin`, and dynamically load them. Perfect for extensible SaaS platforms.

🧪 **Testability**
TypeProvider makes it trivial to write tests that verify all handlers are registered, or that your architecture rules are followed.

### Conceptual Foundations

**Convention over Configuration**
Inspired by Ruby on Rails and ASP.NET MVC, Myth.DependencyInjection reduces boilerplate by inferring registration from conventions rather than explicit configuration.

**Reflection and Assembly Scanning**
Uses .NET's reflection capabilities to scan assemblies, discover types, and build the dependency graph automatically. Inspired by tools like Scrutor and StructureMap.

**Marker Interface Pattern**
Use marker interfaces (e.g., `IRepository`, `IDomainService`) to categorize services by architectural layer, then register entire layers with one call.

**Plugin Architecture**
Enables Inversion of Control at the architectural level—your application doesn't know what plugins exist; they declare themselves by implementing interfaces.

**Fail-Fast Principle**
Throws `InterfaceNotFoundException` at startup (not runtime) if conventions aren't followed, ensuring problems are caught early.

### Business Value

**For Developers**
- **80% less registration code** in large applications
- **No more forgotten registrations** causing production bugs
- **Focus on business logic**, not infrastructure plumbing
- **Faster onboarding**—new developers see the convention and start following it

**For Architects**
- **Enforce architectural patterns** through naming conventions
- **Enable plugin ecosystems** for extensible products
- **Reduce code review noise**—no Startup.cs diffs for every PR
- **Scalable architecture** as teams and services grow

**For Product Teams**
- **Faster feature delivery**—less time on infrastructure
- **Fewer runtime errors** from registration mistakes
- **Lower maintenance costs** as codebase scales
- **Easier codebase navigation** with consistent patterns

## Features

- **Type Discovery**: Automatically discover and scan application assemblies and types
- **Convention-based Registration**: Auto-register services based on interface naming conventions
- **Assembly Scanning**: Load and analyze all assemblies in your application domain
- **Type Filtering**: Find types implementing specific interfaces or base classes
- **Namespace Detection**: Automatically detect your application's base namespace
- **Minimal Configuration**: Reduce boilerplate DI registration code
- **Plugin Support**: Enable plugin architectures with dynamic type loading

## Installation

```bash
dotnet add package Myth.DependencyInjection
```

## Quick Start

### Automatic Service Registration

Register all implementations of an interface automatically based on naming conventions:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;

var services = new ServiceCollection();

// Automatically finds and registers all repository implementations
services.AddServiceFromType<IRepository>();

// Result: IPersonRepository -> PersonRepository (Scoped)
//         IOrderRepository -> OrderRepository (Scoped)
//         IProductRepository -> ProductRepository (Scoped)
```

### Type Discovery

Discover and analyze types in your application:

```csharp
using Myth.ValueProviders;

// Get base namespace
var baseNamespace = TypeProvider.BaseApplicationNamespace;
// Returns: "MyApp" (from MyApp.Domain, MyApp.Services, etc.)

// Get all application assemblies
var assemblies = TypeProvider.ApplicationAssemblies;
// Returns: All loaded assemblies from your application

// Get all concrete types
var types = TypeProvider.ApplicationTypes;
// Returns: All non-abstract, non-interface types

// Find types implementing specific interface
var handlers = TypeProvider.GetTypesAssignableFrom<ICommandHandler>();
// Returns: All types implementing ICommandHandler
```

## Usage Scenarios

### 1. Repository Pattern Auto-Registration

Eliminate manual repository registration:

```csharp
// Before: Manual registration for each repository
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IProductRepository, ProductRepository>();
// ... dozens more

// After: One-line registration
services.AddServiceFromType<IRepository>();
```

Convention: The implementation class name must contain the interface name.
- `IUserRepository` -> `UserRepository` (valid)
- `IOrderRepository` -> `OrderRepository` (valid)
- `IProductRepository` -> `ProductRepositoryImpl` (valid)

### 2. CQRS Handler Registration

Automatically register command and query handlers:

```csharp
// Register all command handlers
services.AddServiceFromType<ICommandHandler>(ServiceLifetime.Transient);

// Register all query handlers
services.AddServiceFromType<IQueryHandler>(ServiceLifetime.Transient);

// Register all event handlers
services.AddServiceFromType<IEventHandler>(ServiceLifetime.Scoped);
```

### 3. Plugin Architecture

Dynamically discover and load plugins:

```csharp
// Find all plugin types
var pluginTypes = TypeProvider.GetTypesAssignableFrom<IPlugin>();

foreach (var pluginType in pluginTypes) {
    var plugin = (IPlugin)Activator.CreateInstance(pluginType);
    plugin.Initialize();

    Console.WriteLine($"Loaded plugin: {plugin.Name}");
}
```

### 4. Custom Service Lifetime

Specify service lifetime when registering:

```csharp
// Register as Singleton
services.AddServiceFromType<ICache>(ServiceLifetime.Singleton);

// Register as Transient
services.AddServiceFromType<IValidator>(ServiceLifetime.Transient);

// Register as Scoped (default)
services.AddServiceFromType<IUnitOfWork>(ServiceLifetime.Scoped);
```

### 5. Domain-Driven Design (DDD)

Auto-register domain services and repositories:

```csharp
public class Startup {
    public void ConfigureServices(IServiceCollection services) {
        // Infrastructure layer
        services.AddServiceFromType<IRepository>(ServiceLifetime.Scoped);

        // Domain services
        services.AddServiceFromType<IDomainService>(ServiceLifetime.Scoped);

        // Application services
        services.AddServiceFromType<IApplicationService>(ServiceLifetime.Scoped);
    }
}
```

### 6. Assembly Analysis and Documentation

Generate documentation about your application structure:

```csharp
using Myth.ValueProviders;

public class AssemblyAnalyzer {
    public void PrintApplicationStructure() {
        Console.WriteLine($"Base Namespace: {TypeProvider.BaseApplicationNamespace}");

        Console.WriteLine("\nAssemblies:");
        foreach (var assembly in TypeProvider.ApplicationAssemblies) {
            Console.WriteLine($"  - {assembly.GetName().Name} v{assembly.GetName().Version}");
        }

        Console.WriteLine("\nService Implementations:");
        var services = TypeProvider.GetTypesAssignableFrom<IService>();
        foreach (var service in services) {
            Console.WriteLine($"  - {service.FullName}");
        }
    }
}
```

## API Reference

### TypeProvider

Static class for type and assembly discovery.

#### Properties

```csharp
// Gets the first part of your application namespace
public static string? BaseApplicationNamespace { get; }

// Gets all assemblies from your application
public static IEnumerable<Assembly> ApplicationAssemblies { get; }

// Gets all concrete types exported by your application
public static IEnumerable<Type> ApplicationTypes { get; }
```

#### Methods

```csharp
// Gets types derived from or implementing the specified type
public static IEnumerable<Type> GetTypesAssignableFrom<TType>()
```

**Examples:**

```csharp
// Get base namespace
var ns = TypeProvider.BaseApplicationNamespace;
// "MyCompany" from "MyCompany.ECommerce.Domain"

// Get all assemblies
var assemblies = TypeProvider.ApplicationAssemblies;

// Get all types
var allTypes = TypeProvider.ApplicationTypes;

// Get types implementing interface
var repositories = TypeProvider.GetTypesAssignableFrom<IRepository>();
var handlers = TypeProvider.GetTypesAssignableFrom<IHandler>();
var validators = TypeProvider.GetTypesAssignableFrom<IValidator>();
```

### ServiceCollectionExtensions

Extension methods for `IServiceCollection`.

#### Methods

```csharp
// Adds all implementations of the specified type to the service collection
public static IServiceCollection AddServiceFromType<TType>(
    this IServiceCollection services,
    ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
)
```

**Parameters:**
- `TType`: The base interface or type to search for implementations
- `serviceLifetime`: The service lifetime (Scoped, Transient, or Singleton). Default: Scoped

**Returns:** The service collection for chaining

**Throws:**
- `InterfaceNotFoundException`: When an implementation doesn't have a corresponding interface

**Naming Convention:**
The method finds implementations by matching the interface name within the implementation name.
- Interface: `IPersonRepository` -> Implementation: `PersonRepository` (match: "PersonRepository")
- Interface: `IOrderService` -> Implementation: `OrderService` (match: "OrderService")

**Examples:**

```csharp
// Scoped (default)
services.AddServiceFromType<IRepository>();

// Transient
services.AddServiceFromType<IValidator>(ServiceLifetime.Transient);

// Singleton
services.AddServiceFromType<ICache>(ServiceLifetime.Singleton);
```

## Best Practices

### 1. Organize by Layers

Structure your DI registration by architectural layer:

```csharp
public void ConfigureServices(IServiceCollection services) {
    // Persistence Layer
    services.AddServiceFromType<IRepository>(ServiceLifetime.Scoped);

    // Domain Layer
    services.AddServiceFromType<IDomainService>(ServiceLifetime.Scoped);

    // Application Layer
    services.AddServiceFromType<IApplicationService>(ServiceLifetime.Scoped);

    // Infrastructure Layer
    services.AddServiceFromType<IExternalService>(ServiceLifetime.Transient);
}
```

### 2. Use Marker Interfaces

Create marker interfaces for auto-registration:

```csharp
// Marker interface for domain services
public interface IDomainService { }

public interface IOrderService : IDomainService {
    Task<Order> CreateOrderAsync(CreateOrderCommand command);
}

public class OrderService : IOrderService {
    public async Task<Order> CreateOrderAsync(CreateOrderCommand command) {
        // Implementation
    }
}

// Auto-register all domain services
services.AddServiceFromType<IDomainService>();
```

### 3. Follow Naming Conventions

Ensure consistent naming for automatic discovery:

```csharp
// Good: Name matches interface
public interface IUserRepository { }
public class UserRepository : IUserRepository { }

// Good: Implementation name contains interface name
public interface IProductRepository { }
public class ProductRepositoryImpl : IProductRepository { }

// Bad: No naming correlation
public interface IOrderRepository { }
public class OrderDataAccess : IOrderRepository { }  // Won't be auto-discovered
```

### 4. Combine with Manual Registration

Use auto-registration for conventions, manual for exceptions:

```csharp
// Auto-register most services
services.AddServiceFromType<IRepository>();
services.AddServiceFromType<IService>();

// Manually register special cases
services.AddSingleton<IConfiguration>(configuration);
services.AddScoped<ICurrentUser, CurrentUserAccessor>();
services.AddTransient<IEmailSender, SendGridEmailSender>();
```

### 5. Validate Registrations

Check that types were registered correctly:

```csharp
var serviceProvider = services.BuildServiceProvider();

// Verify critical services are registered
var userRepo = serviceProvider.GetService<IUserRepository>();
if (userRepo == null) {
    throw new InvalidOperationException("IUserRepository not registered");
}

// Or use GetRequiredService to throw if not found
var orderService = serviceProvider.GetRequiredService<IOrderService>();
```

## How It Works

### Type Discovery Process

1. **Assembly Loading**: Scans `AppDomain.CurrentDomain.BaseDirectory` for all `.dll` files
2. **Dynamic Loading**: Loads assemblies not yet loaded into the current AppDomain
3. **Filtering**: Excludes dynamic assemblies and keeps only concrete types
4. **Caching**: Results are computed once per property access for performance

### Auto-Registration Process

1. **Type Scanning**: Finds all types implementing the specified base type/interface
2. **Interface Matching**: For each implementation, finds the corresponding interface by name
3. **Registration**: Creates a `ServiceDescriptor` and adds to the service collection
4. **Validation**: Throws `InterfaceNotFoundException` if no matching interface is found

## Performance Considerations

- **Assembly Scanning**: Performed once per `ApplicationAssemblies` access; results are not cached between accesses
- **Type Discovery**: Filters are applied efficiently using LINQ
- **Registration Time**: Auto-registration happens at application startup, not runtime
- **Large Codebases**: For applications with hundreds of assemblies, consider manual registration for critical paths

## Integration with Other Myth Libraries

Myth.DependencyInjection works seamlessly with other Myth libraries:

```csharp
using Myth.Extensions;
using Myth.ValueProviders;

var builder = WebApplication.CreateBuilder(args);

// Auto-register repositories (Myth.Repository)
builder.Services.AddServiceFromType<IRepository>();

// Add Flow pipeline support (Myth.Flow)
builder.Services.AddFlow();

// Add Guard validation (Myth.Guard)
builder.Services.AddGuard();

// Build with global provider support
var app = builder.BuildApp();

app.Run();
```

## Troubleshooting

### InterfaceNotFoundException

**Problem:** `InterfaceNotFoundException: Not found a interface that corresponds to type`

**Solution:** Ensure your implementation class name contains the interface name:

```csharp
// Problematic
public interface IUserRepository { }
public class UserDataAccess : IUserRepository { }  // Name doesn't contain "IUserRepository"

// Fixed
public interface IUserRepository { }
public class UserRepository : IUserRepository { }  // Contains "UserRepository"
```

### Types Not Discovered

**Problem:** Expected types are not found by `GetTypesAssignableFrom<T>()`

**Solution:**
1. Ensure the assembly is loaded (referenced in your project)
2. Verify types are concrete (not abstract or interfaces)
3. Check that types are public and accessible

### Multiple Interfaces per Implementation

**Problem:** A class implements multiple interfaces

**Solution:** The auto-registration finds the first interface containing the class name. For precise control, register manually:

```csharp
// Auto-registration picks one interface
services.AddServiceFromType<IService>();

// Manual registration for multiple interfaces
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IReadOnlyUserRepository, UserRepository>();
```

## License

This project is licensed under the Apache 2.0 License - see the [LICENSE](https://opensource.org/licenses/Apache-2.0) for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Related Libraries

- **Myth.Flow**: Pipeline orchestration with Result pattern
- **Myth.Flow.Actions**: CQRS and event-driven architecture
- **Myth.Guard**: Fluent validation and data integrity
- **Myth.Repository**: Generic repository pattern implementation
- **Myth.Specification**: Query specification pattern
