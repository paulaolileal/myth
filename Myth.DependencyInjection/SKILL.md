---
name: myth-dependency-injection
description: Use when you need automatic service registration via assembly scanning. TypeProvider gives access to all application assemblies and types. AddServiceFromType<T>() auto-registers implementations by naming convention (IUserRepository → UserRepository). Supports Scoped, Transient, and Singleton lifetimes.
---

# SKILL.md - Myth.DependencyInjection

**Version:** 1.0
**Target Framework:** .NET 8.0
**License:** Apache 2.0

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [API Reference](#api-reference)
  - [TypeProvider](#typeprovider)
  - [Auto-Registration](#auto-registration)
  - [Exceptions](#exceptions)
- [Usage Examples](#usage-examples)
- [Advanced Patterns](#advanced-patterns)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

Myth.DependencyInjection provides powerful tools for assembly scanning, type discovery, and automatic service registration in .NET applications. It simplifies dependency injection setup by automatically discovering and registering implementations based on conventions.

### Key Features

- **Assembly Discovery**: Automatic discovery of all application assemblies
- **Type Scanning**: Scan and filter types across the entire application
- **Auto-Registration**: Convention-based automatic service registration
- **Interface Discovery**: Find implementations by interface or base class
- **Flexible Lifetimes**: Support for Scoped, Transient, and Singleton lifetimes

---

## Installation

```bash
dotnet add package Myth.DependencyInjection
```

### Dependencies
- .NET 8.0 or higher
- Microsoft.Extensions.DependencyInjection.Abstractions
- Myth.Commons

---

## Core Concepts

### 1. Assembly Scanning

The library automatically scans all application assemblies, including:
- Already loaded assemblies in `AppDomain.CurrentDomain`
- DLL files in the application base directory
- Excludes dynamic assemblies

### 2. Type Discovery

Provides filtered access to all concrete types in the application:
- Non-abstract types only
- Non-interface types only
- Supports filtering by base class or interface

### 3. Convention-Based Registration

Automatically registers services using naming conventions:
- Interface: `IUserRepository` → Implementation: `UserRepository` ✅
- Interface: `IOrderService` → Implementation: `OrderService` ✅
- Interface name must be contained in implementation name

---

## API Reference

### TypeProvider

**Namespace:** `Myth.ValueProviders`

Static class providing access to application assemblies and types.

#### Properties

```csharp
public static class TypeProvider {
    // Base namespace (first part before dot)
    public static string? BaseApplicationNamespace { get; }

    // All application assemblies (loaded + from disk)
    public static IEnumerable<Assembly> ApplicationAssemblies { get; }

    // All concrete types (non-abstract, non-interface)
    public static IEnumerable<Type> ApplicationTypes { get; }
}
```

**Examples:**

```csharp
// Get base namespace
var baseNamespace = TypeProvider.BaseApplicationNamespace;
// Returns: "MyCompany" for "MyCompany.MyApp.Domain"

// Get all assemblies
var assemblies = TypeProvider.ApplicationAssemblies;
foreach (var assembly in assemblies) {
    Console.WriteLine(assembly.GetName().Name);
}

// Get all types
var types = TypeProvider.ApplicationTypes;
var classCount = types.Count();
Console.WriteLine($"Found {classCount} concrete types");
```

#### Methods

```csharp
// Get all types that implement/inherit from TType
public static IEnumerable<Type> GetTypesAssignableFrom<TType>()
```

**Example:**

```csharp
// Find all repositories
var repositoryTypes = TypeProvider.GetTypesAssignableFrom<IRepository>();

foreach (var type in repositoryTypes) {
    Console.WriteLine($"Found repository: {type.Name}");
}

// Find all command handlers
var handlerTypes = TypeProvider.GetTypesAssignableFrom<ICommandHandler>();

// Find all domain services
var serviceTypes = TypeProvider.GetTypesAssignableFrom<IDomainService>();
```

---

### Auto-Registration

**Namespace:** `Myth.Extensions`

Extension methods for automatic service registration.

#### AddServiceFromType<TType>

```csharp
public static IServiceCollection AddServiceFromType<TType>(
    this IServiceCollection services,
    ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
)
```

**Parameters:**
- `services`: Service collection to register implementations
- `TType`: Base interface or class to find implementations
- `serviceLifetime`: Service lifetime (default: Scoped)

**Returns:** The service collection for chaining

**Behavior:**
1. Finds all types implementing `TType` using `TypeProvider.GetTypesAssignableFrom<TType>()`
2. For each implementation:
   - Searches for interface whose name contains the implementation name
   - Registers interface → implementation with specified lifetime
   - Throws `InterfaceNotFoundException` if no matching interface found

**Naming Convention:**
- Interface name must **contain** the implementation class name (without "I" prefix)
- Case-sensitive matching

**Examples:**

```csharp
// Register all repositories as Scoped (default)
services.AddServiceFromType<IRepository>();

// Register all command handlers as Transient
services.AddServiceFromType<ICommandHandler>(ServiceLifetime.Transient);

// Register all domain services as Singleton
services.AddServiceFromType<IDomainService>(ServiceLifetime.Singleton);
```

**Valid Naming Patterns:**

| Interface | Implementation | Match? |
|-----------|---------------|--------|
| `IUserRepository` | `UserRepository` | ✅ Yes |
| `IOrderService` | `OrderService` | ✅ Yes |
| `IProductRepository` | `ProductRepositoryImpl` | ✅ Yes (contains "ProductRepository") |
| `ICustomerService` | `CustomerServiceImplementation` | ✅ Yes (contains "CustomerService") |
| `IUserRepository` | `UserDataAccess` | ❌ No (doesn't contain "UserRepository") |
| `IOrderRepository` | `OrderDao` | ❌ No (doesn't contain "OrderRepository") |

---

### Exceptions

#### InterfaceNotFoundException

**Namespace:** `Myth.Exceptions`

```csharp
public class InterfaceNotFoundException : Exception {
    public InterfaceNotFoundException(string? message)
}
```

**When Thrown:**
- During `AddServiceFromType<T>()` when an implementation doesn't have a matching interface
- Message format: "Not found a interface that corresponds to type {TypeName}"

**Example:**

```csharp
// This will throw InterfaceNotFoundException
// if UserDataAccess doesn't implement IUserDataAccess or similar
services.AddServiceFromType<IRepository>();
```

**Solution:**
Ensure implementation names follow convention:
```csharp
// ❌ Bad
public interface IUserRepository : IRepository { }
public class UserDataAccess : IUserRepository { }  // Name doesn't match!

// ✅ Good
public interface IUserRepository : IRepository { }
public class UserRepository : IUserRepository { }  // Name matches!
```

---

## Usage Examples

### Example 1: Basic Repository Auto-Registration

```csharp
// Define marker interface
public interface IRepository { }

// Define specific repository interfaces
public interface IUserRepository : IRepository {
    Task<User?> GetByIdAsync(Guid id);
    Task<IEnumerable<User>> GetAllAsync();
}

public interface IOrderRepository : IRepository {
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
}

// Implement repositories
public class UserRepository : IUserRepository {
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id) {
        return await _context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllAsync() {
        return await _context.Users.ToListAsync();
    }
}

public class OrderRepository : IOrderRepository {
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id) {
        return await _context.Orders.FindAsync(id);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId) {
        return await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
    }
}

// Auto-register in Program.cs
var builder = WebApplication.CreateBuilder(args);

// Before: Manual registration
// builder.Services.AddScoped<IUserRepository, UserRepository>();
// builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// ... repeat for every repository

// After: One line registers all repositories!
builder.Services.AddServiceFromType<IRepository>();

var app = builder.BuildApp();
app.Run();
```

---

### Example 2: CQRS Pattern with Auto-Registration

```csharp
// Command Handler Pattern
public interface ICommandHandler { }

public interface ICommandHandler<TCommand> : ICommandHandler {
    Task HandleAsync(TCommand command, CancellationToken cancellationToken);
}

// Query Handler Pattern
public interface IQueryHandler { }

public interface IQueryHandler<TQuery, TResult> : IQueryHandler {
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}

// Implementations
public class CreateUserCommand {
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand> {
    private readonly IUserRepository _repository;

    public CreateUserCommandHandler(IUserRepository repository) {
        _repository = repository;
    }

    public async Task HandleAsync(CreateUserCommand command, CancellationToken ct) {
        var user = new User {
            Name = command.Name,
            Email = command.Email
        };
        await _repository.AddAsync(user, ct);
    }
}

public class GetUserByIdQuery {
    public Guid UserId { get; set; }
}

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, User?> {
    private readonly IUserRepository _repository;

    public GetUserByIdQueryHandler(IUserRepository repository) {
        _repository = repository;
    }

    public async Task<User?> HandleAsync(GetUserByIdQuery query, CancellationToken ct) {
        return await _repository.GetByIdAsync(query.UserId);
    }
}

// Registration in Program.cs
builder.Services.AddServiceFromType<ICommandHandler>(ServiceLifetime.Transient);
builder.Services.AddServiceFromType<IQueryHandler>(ServiceLifetime.Transient);

// Now all command and query handlers are registered!
```

---

### Example 3: Domain-Driven Design Layers

```csharp
// Define marker interfaces for each layer
public interface IInfrastructureService { }
public interface IDomainService { }
public interface IApplicationService { }

// Infrastructure Layer
public interface IEmailService : IInfrastructureService {
    Task SendEmailAsync(string to, string subject, string body);
}

public class EmailService : IEmailService {
    public async Task SendEmailAsync(string to, string subject, string body) {
        // Implementation
    }
}

// Domain Layer
public interface IOrderDomainService : IDomainService {
    decimal CalculateTotal(Order order);
    bool CanBeCancelled(Order order);
}

public class OrderDomainService : IOrderDomainService {
    public decimal CalculateTotal(Order order) {
        return order.Items.Sum(i => i.Price * i.Quantity);
    }

    public bool CanBeCancelled(Order order) {
        return order.Status == OrderStatus.Pending ||
               order.Status == OrderStatus.Confirmed;
    }
}

// Application Layer
public interface IOrderApplicationService : IApplicationService {
    Task<OrderDto> CreateOrderAsync(CreateOrderCommand command);
}

public class OrderApplicationService : IOrderApplicationService {
    private readonly IOrderRepository _repository;
    private readonly IOrderDomainService _domainService;

    public OrderApplicationService(
        IOrderRepository repository,
        IOrderDomainService domainService) {
        _repository = repository;
        _domainService = domainService;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderCommand command) {
        var order = new Order { /* ... */ };
        await _repository.AddAsync(order);
        return order.To<OrderDto>();
    }
}

// Program.cs - Register all layers
builder.Services.AddServiceFromType<IInfrastructureService>();
builder.Services.AddServiceFromType<IDomainService>();
builder.Services.AddServiceFromType<IApplicationService>();
```

---

### Example 4: Plugin Architecture

```csharp
// Plugin discovery and dynamic loading
public interface IPlugin {
    string Name { get; }
    string Version { get; }
    void Initialize();
}

public class PaymentPlugin : IPlugin {
    public string Name => "Payment Processor";
    public string Version => "1.0.0";

    public void Initialize() {
        Console.WriteLine("Payment plugin initialized");
    }
}

public class NotificationPlugin : IPlugin {
    public string Name => "Notification Service";
    public string Version => "2.1.0";

    public void Initialize() {
        Console.WriteLine("Notification plugin initialized");
    }
}

// Plugin loader
public class PluginLoader {
    public void LoadPlugins() {
        var pluginTypes = TypeProvider.GetTypesAssignableFrom<IPlugin>();

        Console.WriteLine($"Found {pluginTypes.Count()} plugins:");

        foreach (var pluginType in pluginTypes) {
            var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;
            Console.WriteLine($"  - {plugin.Name} v{plugin.Version}");
            plugin.Initialize();
        }
    }
}

// Usage
var loader = new PluginLoader();
loader.LoadPlugins();
```

---

### Example 5: Assembly Analysis and Diagnostics

```csharp
public class AssemblyAnalyzer {
    public void PrintApplicationStructure() {
        Console.WriteLine("=== Application Structure ===\n");

        // Base namespace
        Console.WriteLine($"Base Namespace: {TypeProvider.BaseApplicationNamespace}\n");

        // Assemblies
        Console.WriteLine("Assemblies:");
        foreach (var assembly in TypeProvider.ApplicationAssemblies) {
            var name = assembly.GetName();
            Console.WriteLine($"  - {name.Name} v{name.Version}");
        }

        // Type statistics
        var allTypes = TypeProvider.ApplicationTypes;
        Console.WriteLine($"\nTotal Types: {allTypes.Count()}");

        // Repositories
        var repositories = TypeProvider.GetTypesAssignableFrom<IRepository>();
        Console.WriteLine($"\nRepositories ({repositories.Count()}):");
        foreach (var repo in repositories) {
            Console.WriteLine($"  - {repo.Name}");
        }

        // Services
        var services = TypeProvider.GetTypesAssignableFrom<IService>();
        Console.WriteLine($"\nServices ({services.Count()}):");
        foreach (var service in services) {
            Console.WriteLine($"  - {service.Name}");
        }

        // Handlers
        var handlers = TypeProvider.GetTypesAssignableFrom<IHandler>();
        Console.WriteLine($"\nHandlers ({handlers.Count()}):");
        foreach (var handler in handlers) {
            Console.WriteLine($"  - {handler.Name}");
        }
    }

    public void ValidateRegistrations() {
        var services = new ServiceCollection();
        services.AddServiceFromType<IRepository>();

        var provider = services.BuildServiceProvider();

        // Validate critical services
        var requiredRepositories = new[] {
            typeof(IUserRepository),
            typeof(IOrderRepository),
            typeof(IProductRepository)
        };

        foreach (var repoType in requiredRepositories) {
            var service = provider.GetService(repoType);
            if (service == null) {
                throw new InvalidOperationException(
                    $"Required service {repoType.Name} is not registered!");
            }
            Console.WriteLine($"✓ {repoType.Name} is registered");
        }
    }
}

// Usage
var analyzer = new AssemblyAnalyzer();
analyzer.PrintApplicationStructure();
analyzer.ValidateRegistrations();
```

---

## Advanced Patterns

### Pattern 1: Marker Interfaces with Shared Behavior

```csharp
// Marker interface
public interface IDomainService { }

// Specific interfaces inherit from marker
public interface IOrderService : IDomainService {
    Task<Order> CreateOrderAsync(CreateOrderCommand command);
}

public interface IProductService : IDomainService {
    Task<Product> GetProductAsync(Guid id);
}

// Implementations
public class OrderService : IOrderService {
    public async Task<Order> CreateOrderAsync(CreateOrderCommand command) {
        // Implementation
    }
}

public class ProductService : IProductService {
    public async Task<Product> GetProductAsync(Guid id) {
        // Implementation
    }
}

// One line registers ALL domain services
services.AddServiceFromType<IDomainService>();
```

---

### Pattern 2: Mixed Auto-Registration and Manual Registration

```csharp
// Auto-register most services
services.AddServiceFromType<IRepository>();
services.AddServiceFromType<IDomainService>();

// Manual registration for special cases
services.AddSingleton<IConfiguration>(configuration);
services.AddScoped<ICurrentUser, HttpContextCurrentUserAccessor>();
services.AddTransient<IEmailSender, SendGridEmailSender>();

// Override auto-registered service if needed
services.AddScoped<ISpecialRepository, CustomSpecialRepository>();
```

---

### Pattern 3: Conditional Registration

```csharp
// Register only if not already registered
var services = new ServiceCollection();
services.AddServiceFromType<IRepository>();

// Check if service is registered
var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IUserRepository));
if (descriptor == null) {
    services.AddScoped<IUserRepository, FallbackUserRepository>();
}
```

---

### Pattern 4: Multi-Interface Implementations

When a class implements multiple interfaces, auto-registration picks the **first** interface whose name contains the class name.

```csharp
// Problematic: Multiple interfaces
public interface IUserRepository : IRepository { }
public interface IReadOnlyUserRepository : IRepository { }

public class UserRepository : IUserRepository, IReadOnlyUserRepository {
    // Auto-registration will register only IUserRepository
}

// Solution: Manual registration for additional interfaces
services.AddServiceFromType<IRepository>();  // Registers IUserRepository
services.AddScoped<IReadOnlyUserRepository, UserRepository>();  // Add manually
```

---

### Pattern 5: Generic Type Discovery

```csharp
// Find all implementations of generic interfaces
public interface IValidator<T> {
    Task<bool> ValidateAsync(T entity);
}

public class UserValidator : IValidator<User> {
    public async Task<bool> ValidateAsync(User entity) {
        return !string.IsNullOrEmpty(entity.Name);
    }
}

// Discover validators
var validatorTypes = TypeProvider.ApplicationTypes
    .Where(t => t.GetInterfaces()
        .Any(i => i.IsGenericType &&
                  i.GetGenericTypeDefinition() == typeof(IValidator<>)));

foreach (var validatorType in validatorTypes) {
    var interfaceType = validatorType.GetInterfaces()
        .First(i => i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IValidator<>));

    services.AddScoped(interfaceType, validatorType);
    Console.WriteLine($"Registered {interfaceType.Name} → {validatorType.Name}");
}
```

---

## Best Practices

### 1. Naming Conventions

**✅ DO:**
- Use consistent naming: `I{Name}Repository` → `{Name}Repository`
- Keep interface and implementation names aligned
- Use descriptive names that indicate purpose

```csharp
// Good
public interface IUserRepository : IRepository { }
public class UserRepository : IUserRepository { }

public interface IOrderService : IDomainService { }
public class OrderService : IOrderService { }
```

**❌ DON'T:**
- Use arbitrary implementation names
- Mix naming patterns

```csharp
// Bad
public interface IUserRepository : IRepository { }
public class UserDataAccess : IUserRepository { }  // Won't auto-register!

public interface IOrderRepository : IRepository { }
public class OrderDao : IOrderRepository { }  // Won't auto-register!
```

---

### 2. Marker Interfaces

**✅ DO:**
- Use marker interfaces to group related services
- Keep marker interfaces empty
- Use meaningful names

```csharp
// Good
public interface IRepository { }
public interface IDomainService { }
public interface IApplicationService { }
```

**❌ DON'T:**
- Add methods to marker interfaces (defeats the purpose)
- Use generic names like `IService` (too broad)

---

### 3. Service Lifetimes

**Choose appropriate lifetimes:**

```csharp
// Stateless services, database access - Scoped (default)
services.AddServiceFromType<IRepository>(ServiceLifetime.Scoped);

// Short-lived, no state - Transient
services.AddServiceFromType<ICommandHandler>(ServiceLifetime.Transient);
services.AddServiceFromType<IValidator>(ServiceLifetime.Transient);

// Configuration, caching, shared state - Singleton
services.AddServiceFromType<ICacheService>(ServiceLifetime.Singleton);
```

---

### 4. Validation

**Validate registrations at startup:**

```csharp
var app = builder.Build();

// Validate in development
if (app.Environment.IsDevelopment()) {
    using var scope = app.Services.CreateScope();
    var provider = scope.ServiceProvider;

    // Try to resolve critical services
    var userRepo = provider.GetRequiredService<IUserRepository>();
    var orderService = provider.GetRequiredService<IOrderService>();

    Console.WriteLine("✓ All critical services registered successfully");
}

app.Run();
```

---

### 5. Assembly Loading

**Be aware of assembly loading:**

```csharp
// TypeProvider scans all loaded assemblies
// Ensure your assemblies are referenced/loaded

// Force assembly load if needed
Assembly.LoadFrom("MyPlugin.dll");

// Now TypeProvider will see types from MyPlugin
var pluginTypes = TypeProvider.GetTypesAssignableFrom<IPlugin>();
```

---

## Troubleshooting

### Issue 1: InterfaceNotFoundException

**Problem:**
```
InterfaceNotFoundException: Not found a interface that corresponds to type UserDataAccess
```

**Cause:** Implementation class name doesn't contain interface name.

**Solution:**
```csharp
// ❌ Before
public interface IUserRepository : IRepository { }
public class UserDataAccess : IUserRepository { }

// ✅ After
public interface IUserRepository : IRepository { }
public class UserRepository : IUserRepository { }
```

---

### Issue 2: Services Not Discovered

**Problem:** Some types are not being registered.

**Possible Causes:**
1. **Type is abstract or interface**
   ```csharp
   // Won't be discovered
   public abstract class BaseRepository : IRepository { }
   public interface IRepository { }
   ```

2. **Assembly not loaded**
   ```csharp
   // Ensure assembly is referenced
   Assembly.LoadFrom("MyAssembly.dll");
   ```

3. **Type not public**
   ```csharp
   // Won't be discovered
   internal class UserRepository : IUserRepository { }

   // Will be discovered
   public class UserRepository : IUserRepository { }
   ```

---

### Issue 3: Multiple Interface Matches

**Problem:** Class implements multiple interfaces, wrong one gets registered.

**Cause:** Auto-registration picks **first** matching interface.

**Solution:**
```csharp
// Auto-register base case
services.AddServiceFromType<IRepository>();

// Manually register additional interfaces
services.AddScoped<IReadOnlyUserRepository, UserRepository>();
services.AddScoped<ICachedUserRepository, UserRepository>();
```

---

### Issue 4: Service Already Registered

**Problem:** Service gets registered multiple times.

**Solution:** Check before auto-registering:
```csharp
if (!services.Any(d => d.ServiceType == typeof(IUserRepository))) {
    services.AddServiceFromType<IRepository>();
}
```

---

### Issue 5: Performance with Large Codebases

**Problem:** Assembly scanning is slow in large applications.

**Solutions:**

1. **Cache assemblies:**
   ```csharp
   private static IEnumerable<Assembly>? _cachedAssemblies;

   public static IEnumerable<Assembly> GetAssemblies() {
       return _cachedAssemblies ??= TypeProvider.ApplicationAssemblies.ToList();
   }
   ```

2. **Use manual registration for critical paths:**
   ```csharp
   // Auto-register less critical services
   services.AddServiceFromType<IBackgroundService>();

   // Manual registration for critical services
   services.AddScoped<IUserRepository, UserRepository>();
   services.AddScoped<IAuthService, AuthService>();
   ```

---

## Integration with Myth Ecosystem

### With Myth.Commons

```csharp
using Myth.Extensions;
using Myth.ValueProviders;

var builder = WebApplication.CreateBuilder(args);

// Auto-register services
builder.Services.AddServiceFromType<IRepository>();

// Use BuildApp() for global service provider
var app = builder.BuildApp();

app.Run();
```

---

### With Myth.Flow.Actions

```csharp
// Auto-register all handlers
builder.Services.AddServiceFromType<ICommandHandler>(ServiceLifetime.Scoped);
builder.Services.AddServiceFromType<IQueryHandler>(ServiceLifetime.Scoped);
builder.Services.AddServiceFromType<IEventHandler>(ServiceLifetime.Scoped);

// Configure Flow.Actions
builder.Services.AddFlowActions(config => {
    config.UseInMemory();
    config.UseCaching();
});
```

---

## Performance Considerations

1. **Assembly Scanning**: Happens once per property access (no persistent cache)
2. **Type Discovery**: Uses LINQ for efficient filtering
3. **Registration Time**: Auto-registration happens at startup (one-time cost)
4. **Runtime Performance**: No overhead after registration (standard DI resolution)

---

## Testing

### Unit Test Example

```csharp
using Xunit;
using FluentAssertions;
using Myth.Extensions;
using Myth.ValueProviders;

public class TypeProviderTests {
    [Fact]
    public void BaseApplicationNamespace_ShouldReturnFirstPart() {
        // Arrange & Act
        var baseNamespace = TypeProvider.BaseApplicationNamespace;

        // Assert
        baseNamespace.Should().NotBeNullOrEmpty();
        baseNamespace.Should().Be("Myth");
    }

    [Fact]
    public void ApplicationAssemblies_ShouldReturnAssemblies() {
        // Arrange & Act
        var assemblies = TypeProvider.ApplicationAssemblies;

        // Assert
        assemblies.Should().NotBeEmpty();
    }

    [Fact]
    public void GetTypesAssignableFrom_ShouldReturnImplementations() {
        // Arrange & Act
        var types = TypeProvider.GetTypesAssignableFrom<IRepository>();

        // Assert
        types.Should().NotBeEmpty();
        types.Should().AllSatisfy(t => typeof(IRepository).IsAssignableFrom(t));
    }
}

public class ServiceCollectionExtensionsTests {
    [Fact]
    public void AddServiceFromType_ShouldRegisterServices() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddServiceFromType<IRepository>();

        // Assert
        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IUserRepository));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        descriptor.ImplementationType.Should().Be(typeof(UserRepository));
    }

    [Fact]
    public void AddServiceFromType_WithCustomLifetime_ShouldUseSpecifiedLifetime() {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddServiceFromType<ICommandHandler>(ServiceLifetime.Transient);

        // Assert
        var descriptor = services.First();
        descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }
}
```

---

## Summary

Myth.DependencyInjection provides powerful tools for:

- ✅ **Assembly Discovery**: Automatic scanning of all application assemblies
- ✅ **Type Scanning**: Filter and find types by base class or interface
- ✅ **Auto-Registration**: Convention-based automatic service registration
- ✅ **Flexible Lifetimes**: Support for Scoped, Transient, and Singleton
- ✅ **Integration**: Seamless integration with Myth ecosystem

**Key Benefits:**

1. **Reduces Boilerplate**: One line replaces dozens of manual registrations
2. **Prevents Errors**: Convention-based registration reduces typos
3. **Improves Maintainability**: New services auto-register when following conventions
4. **Supports Scaling**: Easily handle hundreds of services
5. **Type Safety**: Compile-time checking with generics

---

## Additional Resources

- **Repository**: https://gitlab.com/dotnet-myth/myth
- **License**: Apache 2.0
- **Target Framework**: .NET 8.0
- **NuGet Package**: Myth.DependencyInjection

---

*This documentation is maintained for AI agents and developers. For questions or contributions, please refer to the repository.*
