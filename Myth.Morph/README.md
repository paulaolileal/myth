<img  style="float: right;" src="myth-morph-logo.png" alt="drawing" width="250"/>

# Myth.Morph

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Morph?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Morph/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Morph?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Morph/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A lightweight .NET object transformation library designed for clean architecture and Domain-Driven Design. Myth.Morph provides a declarative, schema-based approach to object mapping with zero reflection overhead during transformation and full dependency injection integration.

## 🎯 Why Myth.Morph?

**Object mapping is a hidden performance and maintenance nightmare.** AutoMapper-style libraries use runtime reflection that kills performance and obscures transformation logic. Manual mapping is verbose and error-prone—miss one property and data is lost. DTOs pollute domain models, or worse, domain entities are exposed directly to APIs breaking encapsulation. **Myth.Morph solves this with explicit, compile-time safe transformations** that are fast (schema compiled at startup, zero reflection during mapping), clear (mappings live with types), and DI-aware (async transformations with service access).

### The Problem

**Runtime Reflection = Performance Hell & Magic**
```csharp
// AutoMapper - Convention-based magic
services.AddAutoMapper(typeof(Program));

CreateMap<User, UserDto>(); // Where is this? How does it work? What properties map?

// Runtime reflection on every mapping call - slow
var dto = _mapper.Map<UserDto>(user); // Black box - what happens? No idea until runtime
```

**Manual Mapping = Verbose & Error-Prone**
```csharp
// Manual mapping scattered everywhere
public UserDto ToDto(User user) {
    return new UserDto {
        Id = user.Id,
        Name = user.FullName, // Wait, which property? Easy to get wrong
        Email = user.EmailAddress,
        // Forgot to map Address! Shipping fails in production
    };
}

// Duplicated in 10 places across the codebase
// Update User model? Good luck finding all mappings
```

**Problems:**
- **Performance**: Runtime reflection on every mapping call
- **Maintainability**: Convention-based magic obscures logic
- **Error-prone**: Missed properties, wrong property names
- **No DI access**: Can't call services during mapping
- **Not async**: Can't await database/API calls in transformations

### The Solution

**Explicit, Compile-Time Safe, DI-Aware Transformations**
```csharp
// Mapping defined where it belongs - with the type
public class UserDto : IMorphableFrom<User> {
    public Guid Id { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public AddressDto Address { get; set; }

    public void MorphFrom(Schema<User> schema) {
        schema
            .Auto() // Maps Id (same name)
            .Bind(u => u.FullName, () => DisplayName) // Explicit - compile-time safe
            .Bind(u => u.EmailAddress, () => Email)
            .BindAsync(u => u.Address, async sp => {
                // Async transformation with DI access
                var addressService = sp.GetRequiredService<IAddressService>();
                return await addressService.GetFormattedAddressAsync(u.Address);
            });
    }
}

// Usage - fast and explicit
var dto = user.To<UserDto>(); // Schema compiled at startup, zero reflection here
```

**Benefits:**
- **Fast**: Schema compiled once, zero reflection during mapping
- **Clear**: Transformations are explicit, self-documenting
- **Type-safe**: Compile-time checking via lambdas
- **DI-aware**: Full service provider access in transformations
- **Async-ready**: Await database/API calls naturally

### Why Choose Myth.Morph?

| Aspect | Myth.Morph | AutoMapper | Manual Mapping | Mapster |
|--------|------------|------------|----------------|---------|
| **Performance** | Fast (pre-compiled schema) | Slow (runtime reflection) | Fast | Fast |
| **Explicitness** | Explicit bindings | Convention magic | Explicit | Mixed |
| **Location** | With type (DDD) | Separate profile | Scattered everywhere | Separate config |
| **Type Safety** | Compile-time (lambdas) | Runtime | Compile-time | Runtime + codegen |
| **DI Access** | Native in transformations | Limited (resolvers) | Manual | No |
| **Async Support** | First-class `.BindAsync()` | Limited | Manual | Limited |
| **Learning Curve** | Low (fluent API) | Medium (profiles, conventions) | None (standard C#) | Medium |
| **Debugging** | Clear (explicit code) | Hard (convention magic) | Easy | Medium |

### Real-World Applications

**CQRS APIs (Commands → Entities → DTOs)**
Map `CreateUserCommand` → `User` entity → `UserDto` response. Different transformations per operation. Async validation/enrichment during mapping.

**Clean Architecture (Entities → ViewModels)**
Domain entities stay pure. ViewModels know how to be created from entities. Clear separation between layers. No domain pollution with DTOs.

**Microservices (External APIs → Domain Models)**
Transform third-party API responses into domain models with async enrichment (call internal APIs, check cache, fetch from DB).

**Data Migration Pipelines (Legacy → Modern Schema)**
Explicit transformations from old schema to new. Async lookups for foreign keys. Logging/validation during transformation.

**Event-Driven Systems (Entities → Events)**
Transform domain entities into integration events. Async loading of related data before publishing. Schema evolution handling.

### Key Differentiators

⚡ **Pre-Compiled Schemas**
Transformations compiled at startup via `AddMorph()`. Zero reflection during actual mapping. Massive performance win over AutoMapper.

🎯 **Self-Documenting**
Transformations live with the type that owns them. Open `UserDto`, see exactly how it maps from `User`. No hunting through profile classes.

🔧 **DI-Aware**
Full `IServiceProvider` access in transformation logic. Call repositories, APIs, caching layers during mapping. Async-first with `.BindAsync()`.

🏗️ **Bidirectional Patterns**
`IMorphableTo<T>` (source defines "to" destination) OR `IMorphableFrom<T>` (destination defines "from" source). Choose what makes sense for your domain.

📦 **Collection Transformations**
`users.To<List<UserDto>>()` automatically transforms element-by-element. Supports `IEnumerable<T>`, `List<T>`, `T[]`, `IQueryable<T>`.

🧱 **EF Core Proxy Support**
Automatic detection and handling of lazy-loading proxies. Inheritance hierarchy traversal with configurable depth.

### Conceptual Foundations

**Schema-Based Mapping**
Inspired by database schema migrations. Define transformation schema once, apply many times. Compile schema for performance.

**Explicit over Implicit**
LINQ philosophy: explicit bindings with compile-time safety beats convention-based magic. Errors caught early, not at runtime.

**Single Responsibility (DDD)**
Mappings are responsibility of the type that needs them. DTOs know how to be created from entities, not the reverse.

**Fluent Interface**
Method chaining for readable configuration: `.Auto().Bind(...).BindAsync(...).Ignore(...)`. Inspired by FluentValidation, LINQ.

**Dependency Injection Integration**
First-class DI support following .NET conventions. Service provider passed to transformations for rich logic.

### Business Value

**For Developers**
- **50% less mapping code** vs manual approaches
- **10x faster execution** vs AutoMapper (reflection)
- **Clear debugging**: Explicit bindings, no magic
- **Async transformations**: Await DB/API calls naturally

**For Architects**
- **DDD-aligned**: Mappings belong to types, not separate profiles
- **Clean separation**: Entities, DTOs, ViewModels clearly separated
- **Performance**: Pre-compiled schemas eliminate reflection overhead
- **Scalable**: Async transformations don't block

**For DevOps/SRE**
- **Predictable performance**: No reflection hot paths
- **Easy monitoring**: Clear call stacks, no magic
- **Debuggable**: Explicit code, not convention magic

**For Product Teams**
- **Faster development**: Less boilerplate than manual mapping
- **Fewer bugs**: Type-safe bindings catch errors early
- **Better API design**: Clean DTOs, entities stay pure
- **Easier refactoring**: Compiler finds all mappings

## Features

- **Bidirectional Mapping Patterns**: Support for both `IMorphableTo<T>` and `IMorphableFrom<T>` patterns
- **Entity Framework Proxy Support**: Automatic detection and handling of EF Core lazy-loading proxies
- **Inheritance Hierarchy Resolution**: Configurable depth traversal for complex type hierarchies
- **Declarative Schema Configuration**: Define transformations using fluent API with compile-time safety
- **Automatic Property Mapping**: Convention-based mapping for matching property names
- **Manual Binding**: Four binding strategies for maximum flexibility
- **Async Support**: First-class async/await support for I/O-bound transformations
- **Dependency Injection**: Full service provider access in transformation logic
- **Generic Collections**: Automatic mapping of collections with element transformation
- **Nested Objects**: Recursive transformation support for complex object graphs
- **Ignore Properties**: Explicit exclusion of properties from mapping
- **Comprehensive Logging**: Detailed trace logging for debugging transformations

## Installation

```bash
dotnet add package Myth.Morph
```

## Quick Start

### 1. Register Services

```csharp
// In Program.cs or Startup.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMorph();

var app = builder.BuildApp(); // Use BuildApp() instead of Build()
```

For console applications:

```csharp
var services = new ServiceCollection();
services.AddMorph();

var provider = services.BuildWithGlobalProvider();
```

### 2. Define Transformations

Myth.Morph supports two mapping patterns:

#### IMorphableTo Pattern (Source → Destination)

The source type defines how to transform **to** the destination:

```csharp
public class CreateUserDto : IMorphableTo<User> {
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime BirthDate { get; set; }

    public void MorphTo( Schema<User> schema ) {
        schema
            .Bind(u => u.FullName, () => Name)
            .Bind(u => u.EmailAddress, () => Email)
            .Bind(u => u.Age, () => DateTime.Today.Year - BirthDate.Year);
    }
}
```

#### IMorphableFrom Pattern (Destination ← Source)

The destination type defines how to be created **from** the source:

```csharp
public class UserDto : IMorphableFrom<User> {
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }

    public void MorphFrom( Schema<User> schema ) {
        schema
            .Bind(() => DisplayName, u => $"{u.FirstName} {u.LastName}")
            .Bind(() => Email, u => u.EmailAddress)
            .Bind(() => Age, u => DateTime.Today.Year - u.BirthDate.Year);
    }
}
```

> **Note**: Use `IMorphableTo` when the source knows about the destination (DTOs → Entities).
> Use `IMorphableFrom` when the destination knows about the source (Entities → DTOs).

### 3. Transform Objects

```csharp
var dto = new CreateUserDto {
    Name = "John Doe",
    Email = "john@example.com",
    BirthDate = new DateTime(1990, 1, 1)
};

var user = dto.To<User>();
```

## Binding Strategies

Myth.Morph provides four binding strategies to handle different transformation scenarios.

### 1. Direct Value Binding

Map a property to a computed value:

```csharp
public void MorphTo( Schema<User> schema ) {
    schema.Bind(u => u.FullName, () => $"{FirstName} {LastName}");
}
```

### 2. Service Provider Binding

Access services from DI container for complex transformations:

```csharp
public void MorphTo( Schema<Order> schema ) {
    schema.Bind(o => o.Customer, sp => {
        var customerService = sp.GetRequiredService<ICustomerService>();
        return customerService.GetCustomerById(CustomerId);
    });
}
```

### 3. Async Direct Binding

For async operations without service provider:

```csharp
public void MorphTo( Schema<User> schema ) {
    schema.BindAsync(u => u.Avatar, async () => {
        await Task.Delay(100); // Simulate async work
        return "default-avatar.png";
    });
}
```

### 4. Async Service Provider Binding

Combine async operations with DI:

```csharp
public void MorphTo( Schema<Product> schema ) {
    schema.BindAsync(p => p.Reviews, async sp => {
        var reviewService = sp.GetRequiredService<IReviewService>();
        return await reviewService.GetReviewsAsync(ProductId);
    });
}
```

## Automatic Property Mapping

Properties with matching names and compatible types are automatically mapped:

```csharp
public class UserDto : IMorphable<User> {
    public string Name { get; set; }      // Auto-maps to User.Name
    public string Email { get; set; }     // Auto-maps to User.Email
    public int Age { get; set; }          // Auto-maps to User.Age

    public void MorphTo( Schema<User> schema ) {
        // Only define custom mappings - automatic mapping handles the rest
        schema.Ignore(u => u.InternalId);
    }
}
```

### Automatic Mapping Features

- **Name matching**: Properties with identical names are mapped automatically
- **Type conversion**: Handles primitive type conversions (int to long, etc.)
- **Nested objects**: Recursively transforms nested objects using registered mappings
- **Null handling**: Safely handles null values and nullable types
- **Collection mapping**: Automatically maps compatible collection types

## Ignoring Properties

Exclude properties from both manual and automatic mapping:

```csharp
public void MorphTo( Schema<User> schema ) {
    schema
        .Ignore(u => u.InternalId)
        .Ignore(u => u.CreatedBy)
        .Ignore(u => u.ModifiedBy);
}
```

## Collection Transformations

Transform collections with type-safe extension methods:

```csharp
// Transform enumerable
IEnumerable<UserDto> dtos = GetUserDtos();
IEnumerable<User> users = dtos.To<User>();

// Transform with service provider
IEnumerable<User> users = dtos.To<User>(serviceProvider);

// Async collection transformation
IEnumerable<User> users = await dtos.ToAsync<User>();

// Type-safe collection transformation
List<UserDto> dtoList = GetUserDtos();
IEnumerable<User> users = dtoList.To<UserDto, User>();
```

## Nested Object Mapping

Myth.Morph automatically handles nested transformations:

```csharp
public class OrderDto : IMorphable<Order> {
    public int OrderId { get; set; }
    public List<OrderItemDto> Items { get; set; }

    public void MorphTo( Schema<Order> schema ) {
        schema
            .Bind(o => o.Id, () => OrderId)
            .BindAsync(o => o.Items, async sp =>
                // Nested collection transformation
                await Items.ToAsync<OrderItem>(sp)
            );
    }
}

// OrderItemDto also implements IMorphable<OrderItem>
public class OrderItemDto : IMorphable<OrderItem> {
    public string ProductName { get; set; }
    public decimal Price { get; set; }

    public void MorphTo( Schema<OrderItem> schema ) {
        // Automatic mapping handles properties
    }
}
```

## Entity Framework Proxy Support

Myth.Morph automatically detects and handles Entity Framework Core lazy-loading proxies (Castle.Proxies), ensuring seamless transformations even when working with proxied entities.

### How It Works

When EF Core uses lazy-loading proxies, it creates dynamic proxy types that inherit from your entity:

```csharp
// Your entity
public class User {
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<Order> Orders { get; set; } // virtual for lazy loading
}

// EF Core creates: Castle.Proxies.UserProxy : User
```

Myth.Morph automatically resolves the proxy to the base entity type, ensuring your mappings work correctly:

```csharp
// This works even if user is a proxy
var userDto = user.To<UserDto>();

// Collections with proxies also work
var userDtos = users.To<UserDto>(); // users may contain proxy instances
```

### Configuring Inheritance Fallback

Enable inheritance fallback for advanced proxy and inheritance scenarios:

```csharp
services.AddMorph(settings => {
    settings.EnableInheritanceFallback = true;
    settings.MaxInheritanceDepth = 5; // How deep to traverse the hierarchy
    settings.IncludeInterfacesInFallback = false; // Whether to check interfaces
});
```

### Example with EF Core

```csharp
public class UserRepository {
    private readonly DbContext _context;

    public async Task<UserDto> GetUserWithOrdersAsync(int userId) {
        // EF Core may return a proxy with lazy-loaded Orders
        var user = await _context.Users
            .Include(u => u.Orders)
            .FirstAsync(u => u.Id == userId);

        // Myth.Morph automatically handles the proxy
        return user.To<UserDto>();
    }
}

public class UserDto : IMorphableFrom<User> {
    public int Id { get; set; }
    public string Name { get; set; }
    public List<OrderDto> Orders { get; set; }

    public void MorphFrom(Schema<User> schema) {
        // Automatic mapping works even with proxies
        schema.BindAsync(() => Orders, async (u, sp) =>
            await u.Orders.ToAsync<OrderDto>(sp)
        );
    }
}
```

### Proxy Detection

Myth.Morph detects proxies by:
- Checking for `Castle.Proxies` namespace
- Identifying dynamic assemblies
- Analyzing type hierarchy patterns

No configuration needed - it just works! 🎯

## Advanced Configuration

### Custom Assembly Scanning

Limit assemblies scanned for IMorphable implementations:

```csharp
services.AddMorph(settings => {
    settings.AddAssembly(Assembly.GetExecutingAssembly());
    settings.AddAssemblies(typeof(UserDto).Assembly, typeof(OrderDto).Assembly);
});
```

### Generic Type Mappings

Register mappings between generic interfaces and concrete types:

```csharp
services.AddMorph(settings => {
    settings.AddGenericMorph(typeof(IList<>), typeof(List<>));
    settings.AddGenericMorph(typeof(ICustomCollection<>), typeof(CustomCollection<>));

    // Type-safe generic mapping
    settings.AddGenericMapping<IMyInterface<>, MyImplementation<>>();
});
```

### Default Generic Mappings

Myth.Morph includes these default mappings:

- `IList<>` → `List<>`
- `ICollection<>` → `List<>`
- `IDictionary<,>` → `Dictionary<,>`
- `ISet<>` → `HashSet<>`
- `IReadOnlyCollection<>` → `ReadOnlyCollection<>`
- `IReadOnlyList<>` → `List<>`
- `IReadOnlySet<>` → `HashSet<>`

### Clear Default Mappings

```csharp
services.AddMorph(settings => {
    settings.ClearGenericMappings()
            .AddGenericMapping<IList<>, ArrayList>(); // Use custom mapping
});
```

## Real-World Examples

### CQRS Command to Entity

Use `IMorphableTo` when commands/DTOs transform into entities:

```csharp
public class CreateProductCommand : IMorphableTo<Product> {
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string CategoryId { get; set; }

    public void MorphTo( Schema<Product> schema ) {
        schema
            .Bind(p => p.Name, () => Name)
            .Bind(p => p.Price, () => Price)
            .Bind(p => p.Category, sp => {
                var categoryRepo = sp.GetRequiredService<ICategoryRepository>();
                return categoryRepo.GetById(CategoryId);
            })
            .Bind(p => p.CreatedAt, () => DateTime.UtcNow)
            .Bind(p => p.IsActive, () => true);
    }
}
```

### API Response to Domain Model

```csharp
public class UserApiResponse : IMorphable<User> {
    public string Id { get; set; }
    public string FullName { get; set; }
    public string EmailAddress { get; set; }

    public void MorphTo( Schema<User> schema ) {
        schema
            .Bind(u => u.UserId, () => Guid.Parse(Id))
            .Bind(u => u.Name, () => FullName)
            .Bind(u => u.Email, () => EmailAddress)
            .BindAsync(u => u.Preferences, async sp => {
                var prefService = sp.GetRequiredService<IPreferenceService>();
                return await prefService.GetUserPreferencesAsync(Id);
            });
    }
}
```

### Entity to DTO with Computed Properties

Use `IMorphableFrom` when entities transform into DTOs for read operations:

```csharp
public class Product {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class ProductDto : IMorphableFrom<Product> {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string DisplayName { get; set; }
    public string Status { get; set; }

    public void MorphFrom( Schema<Product> schema ) {
        // Id, Name, Price auto-mapped from matching properties
        schema
            .Bind(() => DisplayName, p => $"{p.Name} (#{p.Id})")
            .Bind(() => Status, p => p.IsActive ? "Active" : "Inactive")
            .Ignore(p => p.CreatedAt); // Don't map this property
    }
}

// Transform entity to DTO (works with EF proxies too!)
var product = await _context.Products.FindAsync(1);
var dto = product.To<ProductDto>();
```

### Event Sourcing Integration

```csharp
public class UserRegisteredEvent : IMorphable<User> {
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime RegisteredAt { get; set; }

    public void MorphTo( Schema<User> schema ) {
        schema
            .Bind(u => u.Id, () => UserId)
            .Bind(u => u.Email, () => Email)
            .Bind(u => u.FullName, () => Name)
            .Bind(u => u.CreatedDate, () => RegisteredAt)
            .Bind(u => u.IsActive, () => true)
            .Bind(u => u.EmailVerified, () => false);
    }
}
```

### Repository Pattern Integration

```csharp
public class ProductService {
    private readonly IProductRepository _repository;
    private readonly IServiceProvider _serviceProvider;

    public ProductService( IProductRepository repository, IServiceProvider serviceProvider ) {
        _repository = repository;
        _serviceProvider = serviceProvider;
    }

    public async Task<ProductDto> GetProductAsync( int productId ) {
        var product = await _repository.GetByIdAsync(productId);
        return product.To<ProductDto>(_serviceProvider);
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync() {
        var products = await _repository.GetAllAsync();
        return await products.ToAsync<ProductDto>(_serviceProvider);
    }

    public async Task<Product> CreateProductAsync( CreateProductDto dto ) {
        var product = dto.To<Product>(_serviceProvider);
        return await _repository.AddAsync(product);
    }
}
```

## Checking Mapping Availability

Verify if a mapping exists before attempting transformation:

```csharp
var dto = new UserDto();

// Check if mapping exists
if (dto.CanBindTo<User>()) {
    var user = dto.To<User>();
}

// Type-safe check
if (dto.CanBindTo<UserDto, User>()) {
    var user = dto.To<User>();
}
```

## Exception Handling

Myth.Morph provides specific exceptions for different error scenarios:

### Exception Types

- **`BinderNotFoundException`**: No mapping registered between source and destination types
- **`BindException`**: Property or field binding operation failed
- **`InvalidMorphConfigurationException`**: SchemaRegistry not properly configured in DI

### Example

```csharp
try {
    var user = dto.To<User>();
} catch ( BinderNotFoundException ex ) {
    logger.LogError("No mapping found from {Source} to {Dest}", ex.SourceType, ex.DestType);
} catch ( BindException ex ) {
    logger.LogError("Property binding failed: {Message}", ex.Message);
} catch ( InvalidMorphConfigurationException ex ) {
    logger.LogError("Morph not configured: {Message}", ex.Message);
}
```

## Performance Considerations

### Best Practices

1. **Reuse Service Provider**: Pass the same service provider instance when transforming multiple objects

```csharp
var users = new List<User>();
foreach (var dto in dtos) {
    users.Add(dto.To<User>(serviceProvider)); // Reuse provider
}
```

2. **Use Async for I/O**: Always use async bindings for database or API calls

```csharp
schema.BindAsync(u => u.Profile, async sp => {
    var service = sp.GetRequiredService<IProfileService>();
    return await service.GetProfileAsync(UserId); // Async I/O
});
```

3. **Limit Assembly Scanning**: Reduce startup time by specifying assemblies

```csharp
services.AddMorph(settings => {
    settings.ClearAssemblies()
            .AddAssembly(typeof(MyDto).Assembly);
});
```

4. **Batch Collections**: Transform collections in bulk rather than individually

```csharp
// Good
var users = dtos.To<User>();

// Avoid
var users = dtos.Select(d => d.To<User>()).ToList();
```

## Logging

Myth.Morph provides comprehensive logging at different levels:

- **Information**: Registry initialization, assembly scanning
- **Debug**: Mapping executions, type resolution
- **Trace**: Property bindings, automatic mappings
- **Warning**: Partial type loading, mapping failures
- **Error**: Configuration issues, binding exceptions

Enable logging in your application:

```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

## Troubleshooting

### "ServiceProvider not configured" Error

```csharp
// Ensure AddMorph() is called and BuildApp() is used
services.AddMorph();
var app = builder.BuildApp(); // Not builder.Build()
```

### "No mapping found" Error

```csharp
// Ensure source implements IMorphable<TDestination>
public class MyDto : IMorphable<MyEntity> {
    public void MorphTo( Schema<MyEntity> schema ) { }
}
```

### "SchemaRegistry not found in DI" Error

```csharp
// Missing AddMorph() registration
services.AddMorph(); // Add this
```

### Mapping Not Working for Generic Collections

```csharp
// Register generic mapping if needed
services.AddMorph(settings => {
    settings.AddGenericMapping<IMyCollection<>, MyCollection<>>();
});
```

### Property Not Being Mapped

Check these common issues:

1. Property name mismatch (case-sensitive)
2. Property not writable (no setter)
3. Property explicitly ignored
4. Type incompatibility

## Integration with Other Myth Libraries

### With Myth.Flow

```csharp
var result = await Pipeline.Start(createUserDto)
    .Step((dto, sp) => dto.To<User>(sp))
    .StepAsync<IUserRepository>((repo, user) => repo.AddAsync(user))
    .ExecuteAsync();
```

### With Myth.Guard

```csharp
public class CreateUserDto : IMorphable<User>, IValidatable<CreateUserDto> {
    public string Name { get; set; }
    public string Email { get; set; }

    public void Validate( ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context ) {
        builder.For(Name, x => x.NotEmpty().MinimumLength(3));
        builder.For(Email, x => x.Email());
    }

    public void MorphTo( Schema<User> schema ) {
        schema.Bind(u => u.FullName, () => Name);
        schema.Bind(u => u.EmailAddress, () => Email);
    }
}

// Use in controller
await _validator.ValidateAsync(dto);
var user = dto.To<User>();
```

### With Myth.Repository

```csharp
public class UserRepository : IUserRepository {
    public async Task<UserDto> GetUserDtoAsync( int userId ) {
        var user = await _dbContext.Users.FindAsync(userId);
        return user.To<UserDto>();
    }
}
```

## Contributing

Contributions are welcome! Please read our contributing guidelines and feel free to submit pull requests.

## License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.
