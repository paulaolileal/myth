---
name: myth-morph
description: Use when you need to map/transform objects between types (e.g. DTO to entity). Implement IMorphableTo<T> or IMorphableFrom<T> and use Schema<T>.Bind()/BindAsync()/Ignore() for custom mappings. Call .To<T>() and .ToAsync<T>() extension methods to execute transformations. Supports DI, async, collections, and bidirectional mapping.
---

# SKILL.md - Myth.Morph

**Version:** 1.0
**Target Framework:** .NET 8.0
**License:** Apache 2.0

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [API Reference](#api-reference)
  - [Interfaces](#interfaces)
  - [Schema Class](#schema-class)
  - [Extension Methods](#extension-methods)
  - [Configuration](#configuration)
- [Usage Examples](#usage-examples)
- [Advanced Patterns](#advanced-patterns)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

Myth.Morph is a powerful object transformation and mapping library for .NET that provides flexible, declarative, and bidirectional object mapping capabilities. It goes beyond simple property copying to support complex transformations, dependency injection integration, and async operations.

### Key Features

- **Declarative Mapping**: Define mappings using interfaces (`IMorphableTo<T>`, `IMorphableFrom<T>`)
- **Automatic Mapping**: Properties with matching names are mapped automatically
- **Bidirectional**: Support for both directions (Entity ↔ DTO)
- **Dependency Injection**: Full integration with DI container
- **Async Support**: BindAsync and ToAsync for async transformations
- **Collection Mapping**: Automatic transformation of lists, sets, and dictionaries
- **Expression-Based**: Type-safe property mapping with lambda expressions
- **Generic Types**: Built-in support for generic interfaces and collections
- **Inheritance Support**: Automatic handling of type hierarchies and proxies

---

## Installation

```bash
dotnet add package Myth.Morph
```

### Dependencies
- .NET 8.0 or higher
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions
- Myth.Commons

---

## Core Concepts

### 1. Morphable Interfaces

Myth.Morph uses three main interfaces to define mappings:

- **`IMorphableTo<TDestination>`**: Source knows how to transform into destination
- **`IMorphableFrom<TSource>`**: Destination knows how to be created from source
- **`IBidirectionalMorphable<TSource, TDestination>`**: Combines both directions

### 2. Transformation Patterns

**Entity → DTO (IMorphableTo)**
```csharp
public class User : IMorphableTo<UserDto> {
    public void MorphTo(Schema<UserDto> schema) {
        schema.Bind(dto => dto.UserId, () => Id);
    }
}
```

**DTO ← Entity (IMorphableFrom)**
```csharp
public class UserDto : IMorphableFrom<User> {
    public void MorphFrom(Schema<User> schema) {
        schema.Bind(() => UserId, src => src.Id);
    }
}
```

### 3. Automatic Mapping

Properties with the **same name** are mapped automatically—no configuration needed!

```csharp
public class User {
    public int Id { get; set; }
    public string Name { get; set; }
}

public class UserDto : IMorphableFrom<User> {
    public int Id { get; set; }     // Auto-mapped
    public string Name { get; set; } // Auto-mapped

    public void MorphFrom(Schema<User> schema) {
        // Id and Name are mapped automatically!
    }
}
```

---

## API Reference

### Interfaces

#### IMorphableTo<TDestination>

**Namespace:** `Myth.Interfaces`

Implemented by source types that know how to transform into destination types.

```csharp
public interface IMorphableTo<TDestination> {
    void MorphTo(Schema<TDestination> schema);
}
```

**Example:**
```csharp
public class Product : IMorphableTo<ProductDto> {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    public void MorphTo(Schema<ProductDto> schema) {
        schema
            .Bind(dto => dto.ProductId, () => Id)
            .Bind(dto => dto.DisplayName, () => Name.ToUpper())
            .Bind(dto => dto.FormattedPrice, () => $"${Price:F2}");
    }
}
```

---

#### IMorphableFrom<TSource>

**Namespace:** `Myth.Interfaces`

Implemented by destination types that know how to be created from source types.

```csharp
public interface IMorphableFrom<TSource> {
    void MorphFrom(Schema<TSource> schema);
}
```

**Example:**
```csharp
public class UserDto : IMorphableFrom<User> {
    public Guid UserId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }

    public void MorphFrom(Schema<User> schema) {
        schema
            .Bind(() => UserId, src => src.Id)
            .Bind(() => FullName, src => $"{src.FirstName} {src.LastName}")
            .Bind(() => Email, src => src.EmailAddress);
    }
}
```

---

#### IBidirectionalMorphable<TSource, TDestination>

**Namespace:** `Myth.Interfaces`

Combines both `IMorphableTo<TDestination>` and `IMorphableFrom<TSource>` for bidirectional mapping.

```csharp
public interface IBidirectionalMorphable<TSource, TDestination>
    : IMorphableTo<TDestination>, IMorphableFrom<TSource> { }
```

**Example:**
```csharp
public class UserEntity : IBidirectionalMorphable<UserDto, UserDto> {
    public Guid Id { get; set; }
    public string Name { get; set; }

    public void MorphTo(Schema<UserDto> schema) {
        schema.Bind(dto => dto.UserId, () => Id);
    }

    public void MorphFrom(Schema<UserDto> schema) {
        schema.Bind(() => Id, src => src.UserId);
    }
}
```

---

### Schema Class

**Namespace:** `Myth.Morph`

Configures property bindings between source and destination.

#### Bind Methods (Synchronous)

##### 1. Bind with Service Provider Access
```csharp
public Schema<TDestination> Bind<TMember>(
    Expression<Func<TDestination, TMember>> destination,
    Func<IServiceProvider, TMember> resolver)
```

**Use Case:** Access services from DI container during transformation.

**Example:**
```csharp
public void MorphTo(Schema<UserDto> schema) {
    schema.Bind(dto => dto.RoleName, sp => {
        var roleService = sp.GetService<IRoleService>();
        return roleService?.GetRoleName(RoleId) ?? "Unknown";
    });
}
```

---

##### 2. Bind with Direct Value
```csharp
public Schema<TDestination> Bind<TMember>(
    Expression<Func<TDestination, TMember>> destination,
    Func<TMember> resolver)
```

**Use Case:** Simple property mapping or transformations.

**Example:**
```csharp
public void MorphTo(Schema<ProductDto> schema) {
    schema
        .Bind(dto => dto.ProductId, () => Id)
        .Bind(dto => dto.UpperName, () => Name.ToUpper())
        .Bind(dto => dto.DiscountedPrice, () => Price * 0.9m);
}
```

---

##### 3. Bind for IMorphableFrom (Expression-based)
```csharp
public Schema<TDestination> Bind<TValue>(
    Expression<Func<TValue>> destinationPropertyGetter,
    Expression<Func<TDestination, TValue>> sourceExpression)
```

**Use Case:** Map from source expression to destination property in `IMorphableFrom`.

**Example:**
```csharp
public class OrderDto : IMorphableFrom<Order> {
    public string CustomerName { get; set; }
    public decimal Total { get; set; }

    public void MorphFrom(Schema<Order> schema) {
        schema
            .Bind(() => CustomerName, src => src.Customer.Name)
            .Bind(() => Total, src => src.Items.Sum(i => i.Price));
    }
}
```

---

##### 4. Bind for IMorphableFrom (with Service Provider)
```csharp
public Schema<TDestination> Bind<TValue>(
    Expression<Func<TValue>> destinationPropertyGetter,
    Func<TDestination, IServiceProvider, TValue> sourceExpression)
```

**Use Case:** Map with access to services in `IMorphableFrom`.

**Example:**
```csharp
public void MorphFrom(Schema<Order> schema) {
    schema.Bind(() => LocalizedStatus, (src, sp) => {
        var locService = sp.GetService<ILocalizationService>();
        return locService?.Translate(src.Status) ?? src.Status;
    });
}
```

---

#### BindAsync Methods (Asynchronous)

##### 1. BindAsync with Service Provider
```csharp
public Schema<TDestination> BindAsync<TMember>(
    Expression<Func<TDestination, TMember>> destination,
    Func<IServiceProvider, Task<TMember>> resolver)
```

**Example:**
```csharp
public void MorphTo(Schema<UserDto> schema) {
    schema.BindAsync(dto => dto.ProfileUrl, async sp => {
        var service = sp.GetService<IStorageService>();
        return await service!.GetProfileUrlAsync(Id);
    });
}
```

---

##### 2. BindAsync with Direct Async Function
```csharp
public Schema<TDestination> BindAsync<TMember>(
    Expression<Func<TDestination, TMember>> destination,
    Func<Task<TMember>> resolver)
```

**Example:**
```csharp
public void MorphTo(Schema<ProductDto> schema) {
    schema.BindAsync(dto => dto.Stock, async () => {
        return await GetStockLevelAsync();
    });
}
```

---

##### 3. BindAsync for IMorphableFrom
```csharp
public Schema<TDestination> BindAsync<TValue>(
    Expression<Func<TValue>> destinationPropertyGetter,
    Func<TDestination, Task<TValue>> sourceExpressionAsync)
```

**Example:**
```csharp
public void MorphFrom(Schema<User> schema) {
    schema.BindAsync(() => AvatarUrl, async src => {
        return await GetAvatarUrlAsync(src.Id);
    });
}
```

---

#### Ignore Method

```csharp
public Schema<TDestination> Ignore<TValue>(
    Expression<Func<TDestination, TValue>> destSelector)
```

**Use Case:** Exclude property from automatic mapping.

**Example:**
```csharp
public void MorphTo(Schema<UserDto> schema) {
    schema
        .Ignore(dto => dto.InternalId)
        .Ignore(dto => dto.CalculatedField);
}
```

---

### Extension Methods

**Namespace:** `Myth.Extensions`

#### To<TDestination> (Synchronous)

##### Single Object
```csharp
public static TDestination To<TDestination>(
    this object source,
    IServiceProvider? sp = null)
```

**Example:**
```csharp
var user = new User { Id = 1, Name = "John" };
var dto = user.To<UserDto>();

// With service provider
var dto = user.To<UserDto>(serviceProvider);
```

---

##### Collection (Generic)
```csharp
public static IEnumerable<TDestination> To<TDestination>(
    this IEnumerable<object> sourceList,
    IServiceProvider? sp = null)
```

**Example:**
```csharp
var users = GetUsers();
var dtos = users.To<UserDto>();
```

---

##### Collection (Typed)
```csharp
public static IEnumerable<TDestination> To<TSource, TDestination>(
    this IEnumerable<TSource> sourceList,
    IServiceProvider? sp = null)
```

**Example:**
```csharp
List<User> users = GetUsers();
var dtos = users.To<User, UserDto>(serviceProvider);
```

---

#### ToAsync<TDestination> (Asynchronous)

##### Single Object
```csharp
public static async Task<TDestination> ToAsync<TDestination>(
    this object source,
    IServiceProvider? sp = null)
```

**Example:**
```csharp
var user = await GetUserAsync();
var dto = await user.ToAsync<UserDto>();
```

---

##### Collection
```csharp
public static async Task<IEnumerable<TDestination>> ToAsync<TDestination>(
    this IEnumerable<object> sourceList,
    IServiceProvider? sp = null)
```

**Example:**
```csharp
var users = await GetUsersAsync();
var dtos = await users.ToAsync<UserDto>(serviceProvider);
```

---

#### CanBindTo (Validation)

```csharp
public static bool CanBindTo<TDestination>(
    this object source,
    IServiceProvider? sp = null)

public static bool CanBindTo<TSource, TDestination>(
    this TSource source,
    IServiceProvider? sp = null)
```

**Use Case:** Check if mapping is available before attempting transformation.

**Example:**
```csharp
if (entity.CanBindTo<EntityDto>()) {
    var dto = entity.To<EntityDto>();
} else {
    throw new InvalidOperationException("Mapping not configured");
}
```

---

### Configuration

**Namespace:** `Myth.Extensions`

#### AddMorph

```csharp
public static IServiceCollection AddMorph(
    this IServiceCollection services,
    Action<MorphSettings>? settings = null)
```

**Example:**
```csharp
// Basic registration
services.AddMorph();

// With configuration
services.AddMorph(settings => settings
    .AddAssembly(typeof(UserDto).Assembly)
    .WithInheritanceFallback(enabled: true, maxDepth: 5)
    .AddGenericMapping<IMyInterface<>, MyImplementation<>>()
);
```

---

#### MorphSettings

**Namespace:** `Myth.Settings`

Configuration options for Myth.Morph.

##### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EnableInheritanceFallback` | `bool` | `true` | Enable searching up inheritance hierarchy |
| `MaxInheritanceDepth` | `int` | `5` | Max depth to search (-1 = unlimited) |
| `IncludeInterfacesInFallback` | `bool` | `true` | Include interfaces in fallback search |
| `ResolveToBaseType` | `bool` | `false` | Resolve derived types to base type |

##### Methods

```csharp
// Assembly management
MorphSettings AddAssembly(Assembly assembly)
MorphSettings AddAssemblies(params Assembly[] assemblies)
MorphSettings ClearAssemblies()
bool ContainsAssembly(Assembly assembly)
int GetAssemblyCount()

// Generic mappings
MorphSettings AddGenericMorph(Type ifaceGeneric, Type concreteGeneric)
MorphSettings AddGenericMapping<TInterface, TConcrete>()
MorphSettings ClearGenericMappings()
bool ContainsGenericMapping(Type interfaceType, Type concreteType)
int GetGenericMappingCount()

// Inheritance configuration
MorphSettings WithInheritanceFallback(
    bool enabled = true,
    int maxDepth = 5,
    bool includeInterfaces = true,
    bool resolveToBaseType = false)

MorphSettings WithResolveToBaseType()
MorphSettings DisableInheritanceFallback()
```

##### Default Generic Mappings

Built-in mappings for common types:
- `IList<>` → `List<>`
- `ICollection<>` → `List<>`
- `IDictionary<,>` → `Dictionary<,>`
- `ISet<>` → `HashSet<>`
- `IReadOnlyCollection<>` → `ReadOnlyCollection<>`
- `IReadOnlyList<>` → `List<>`
- `IReadOnlySet<>` → `HashSet<>`
- `IPaginated<>` → `Paginated<>`

---

## Usage Examples

### Example 1: Basic Entity to DTO Mapping

```csharp
// Entity
public class User : IMorphableTo<UserDto> {
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    public void MorphTo(Schema<UserDto> schema) {
        schema
            .Bind(dto => dto.UserId, () => Id)
            .Bind(dto => dto.FullName, () => $"{FirstName} {LastName}")
            .Bind(dto => dto.EmailAddress, () => Email)
            // IsActive and CreatedAt are auto-mapped
            .Ignore(dto => dto.InternalId);
    }
}

// DTO
public class UserDto {
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; }      // Auto-mapped
    public DateTime CreatedAt { get; set; } // Auto-mapped
    public int InternalId { get; set; }     // Ignored
}

// Usage
var user = await _context.Users.FindAsync(userId);
var dto = user.To<UserDto>();
```

---

### Example 2: DTO from Entity with Dependency Injection

```csharp
public class OrderDto : IMorphableFrom<Order> {
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LocalizedStatus { get; set; } = string.Empty;
    public decimal Total { get; set; }

    public void MorphFrom(Schema<Order> schema) {
        schema
            .Bind(() => OrderId, src => src.Id)
            .Bind(() => CustomerName, src => src.Customer.Name)
            .Bind(() => Total, src => src.Items.Sum(i => i.Price * i.Quantity))
            .Bind(() => LocalizedStatus, (src, sp) => {
                var locService = sp.GetService<ILocalizationService>();
                return locService?.GetStatusText(src.Status) ?? src.Status;
            });
    }
}

// Usage
var order = await _repository.GetByIdAsync(orderId);
var dto = order.To<OrderDto>(serviceProvider);
```

---

### Example 3: Async Transformation with External Data

```csharp
public class UserProfileDto : IMorphableFrom<User> {
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int FollowerCount { get; set; }
    public List<string> RecentPosts { get; set; } = new();

    public void MorphFrom(Schema<User> schema) {
        schema
            .Bind(() => UserId, src => src.Id)
            .Bind(() => Name, src => src.FullName)
            .BindAsync(() => AvatarUrl, async src => {
                return await GetAvatarUrlAsync(src.Id);
            })
            .BindAsync(() => FollowerCount, async src => {
                return await CountFollowersAsync(src.Id);
            })
            .BindAsync(() => RecentPosts, async src => {
                var posts = await GetRecentPostsAsync(src.Id, 5);
                return posts.Select(p => p.Title).ToList();
            });
    }

    private async Task<string> GetAvatarUrlAsync(Guid userId) {
        // External API call
        return $"https://cdn.example.com/avatars/{userId}.jpg";
    }

    private async Task<int> CountFollowersAsync(Guid userId) {
        // Database query
        return await _context.Followers.CountAsync(f => f.UserId == userId);
    }

    private async Task<List<Post>> GetRecentPostsAsync(Guid userId, int count) {
        // Database query
        return await _context.Posts
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}

// Usage
var user = await _userRepository.GetByIdAsync(userId);
var profileDto = await user.ToAsync<UserProfileDto>();
```

---

### Example 4: Collection Mapping

```csharp
// Entity
public class Product : IMorphableTo<ProductDto> {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public void MorphTo(Schema<ProductDto> schema) {
        schema
            .Bind(dto => dto.ProductId, () => Id)
            .Bind(dto => dto.DisplayName, () => Name)
            .Bind(dto => dto.FormattedPrice, () => $"${Price:F2}")
            .Bind(dto => dto.InStock, () => Stock > 0);
    }
}

// DTO
public class ProductDto {
    public int ProductId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string FormattedPrice { get; set; } = string.Empty;
    public bool InStock { get; set; }
}

// Usage
var products = await _context.Products.ToListAsync();

// Transform list
var productDtos = products.To<ProductDto>();

// Transform to specific collection types
var productList = products.To<Product, ProductDto>().ToList();
var productArray = products.To<ProductDto>().ToArray();
```

---

### Example 5: Paginated Results

```csharp
public class PaginatedProductsResponse {
    public List<ProductDto> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public async Task<PaginatedProductsResponse> GetProductsAsync(
    Pagination pagination) {

    var totalItems = await _context.Products.CountAsync();

    var products = await _context.Products
        .Skip((pagination.PageNumber - 1) * pagination.PageSize)
        .Take(pagination.PageSize)
        .ToListAsync();

    // Transform products to DTOs
    var productDtos = products.To<ProductDto>();

    return new PaginatedProductsResponse {
        Items = productDtos.ToList(),
        TotalItems = totalItems,
        PageNumber = pagination.PageNumber,
        PageSize = pagination.PageSize
    };
}

// Using IPaginated<T>
var paginatedProducts = new Paginated<Product>(
    pageNumber: 1,
    pageSize: 20,
    totalItems: 100,
    totalPages: 5,
    items: products);

// Transform IPaginated<Product> to IPaginated<ProductDto>
var paginatedDtos = paginatedProducts.To<IPaginated<ProductDto>>();
```

> **How it works:** `Paginated<T>` uses a primary constructor with `private set` on all properties. Myth.Morph detects this automatically and uses constructor-driven mapping: it reads each source property by name, maps collection elements (`IEnumerable<Product>` → `IEnumerable<ProductDto>`), and constructs the destination by passing the mapped values directly to the constructor. No additional configuration is needed beyond having `IMorphableTo`/`IMorphableFrom` defined for the element types.

> **Requirement:** The `IPaginated<>` → `Paginated<>` mapping must be registered (it is included in the default mappings). Verify your `AddMorph` call includes it or relies on the defaults.

---

### Example 6: Automatic Property Mapping

```csharp
// Simple DTO with matching property names
public class SimpleUserDto : IMorphableFrom<User> {
    // These are auto-mapped (same names as User entity)
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public void MorphFrom(Schema<User> schema) {
        // Nothing to configure - all properties auto-mapped!
    }
}

// Only configure custom mappings
public class CustomUserDto : IMorphableFrom<User> {
    public Guid Id { get; set; }          // Auto-mapped
    public string Email { get; set; }     // Auto-mapped
    public string DisplayName { get; set; } = string.Empty;

    public void MorphFrom(Schema<User> schema) {
        // Only configure what's different
        schema.Bind(() => DisplayName, src => $"{src.FirstName} {src.LastName}");
        // Id and Email are auto-mapped
    }
}
```

---

### Example 7: Constant<T,V> Conversion

```csharp
// Constant type
public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Pending = CreateWithCallerName("P");
    public static readonly OrderStatus Confirmed = CreateWithCallerName("C");
    public static readonly OrderStatus Shipped = CreateWithCallerName("S");

    private OrderStatus(string name, string value) : base(name, value) { }
}

// Entity
public class Order : IMorphableTo<OrderDto> {
    public Guid Id { get; set; }
    public string Status { get; set; } = OrderStatus.Pending;  // Stores "P", "C", "S"

    public void MorphTo(Schema<OrderDto> schema) {
        schema.Bind(dto => dto.OrderId, () => Id);
        // Status is automatically converted from string to OrderStatus constant
    }
}

// DTO
public class OrderDto {
    public Guid OrderId { get; set; }
    public string Status { get; set; } = string.Empty;  // Will be "P", "C", or "S"
}

// Usage
var order = new Order { Id = Guid.NewGuid(), Status = OrderStatus.Confirmed };
var dto = order.To<OrderDto>();
// dto.Status == "C"
```

---

## Advanced Patterns

### Pattern 1: Layered DTOs

```csharp
// Minimal DTO for lists
public class UserSummaryDto : IMorphableFrom<User> {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public void MorphFrom(Schema<User> schema) {
        // Minimal mapping for performance
        schema.Bind(() => Name, src => src.FullName);
    }
}

// Detailed DTO for single items
public class UserDetailDto : IMorphableFrom<User> {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
    public List<OrderDto> RecentOrders { get; set; } = new();

    public void MorphFrom(Schema<User> schema) {
        schema
            .Bind(() => Name, src => src.FullName)
            .BindAsync(() => RecentOrders, async src => {
                var orders = await GetRecentOrdersAsync(src.Id);
                return orders.To<OrderDto>().ToList();
            });
    }
}

// Usage
// List endpoint - lightweight
var users = await _context.Users.ToListAsync();
var summaries = users.To<UserSummaryDto>();

// Detail endpoint - comprehensive
var user = await _context.Users.FindAsync(id);
var detail = await user.ToAsync<UserDetailDto>();
```

---

### Pattern 2: Conditional Mapping

```csharp
public class ConditionalDto : IMorphableFrom<Entity> {
    public string PublicData { get; set; } = string.Empty;
    public string SensitiveData { get; set; } = string.Empty;

    public void MorphFrom(Schema<Entity> schema) {
        schema
            .Bind(() => PublicData, src => src.PublicField)
            .Bind(() => SensitiveData, (src, sp) => {
                var currentUser = sp.GetService<ICurrentUser>();
                return currentUser?.IsAdmin == true
                    ? src.SensitiveField
                    : "[REDACTED]";
            });
    }
}
```

---

### Pattern 3: Nested Object Transformation

```csharp
public class OrderDto : IMorphableFrom<Order> {
    public Guid OrderId { get; set; }
    public CustomerDto Customer { get; set; } = new();
    public List<OrderItemDto> Items { get; set; } = new();
    public AddressDto ShippingAddress { get; set; } = new();

    public void MorphFrom(Schema<Order> schema) {
        schema
            .Bind(() => OrderId, src => src.Id)
            .Bind(() => Customer, src => src.Customer.To<CustomerDto>())
            .Bind(() => Items, src => src.Items.To<OrderItemDto>().ToList())
            .Bind(() => ShippingAddress, src => src.ShippingAddress.To<AddressDto>());
    }
}
```

---

### Pattern 4: Validation Before Mapping

```csharp
public static class MorphExtensions {
    public static TDestination ToWithValidation<TDestination>(
        this object source,
        IValidator validator,
        IServiceProvider? sp = null) {

        if (!source.CanBindTo<TDestination>(sp)) {
            throw new InvalidOperationException(
                $"No mapping configured from {source.GetType().Name} to {typeof(TDestination).Name}");
        }

        var dto = source.To<TDestination>(sp);

        var validationResult = validator.Validate(dto);
        if (!validationResult.IsValid) {
            throw new ValidationException(validationResult.Errors);
        }

        return dto;
    }
}

// Usage
var dto = entity.ToWithValidation<EntityDto>(validator, serviceProvider);
```

---

## Best Practices

### 1. Choose the Right Interface

**✅ Use IMorphableTo when:**
- Entity knows its DTO structure
- Transformation logic belongs with the entity
- Working in domain layer

```csharp
public class User : IMorphableTo<UserDto> {
    public void MorphTo(Schema<UserDto> schema) {
        schema.Bind(dto => dto.FullName, () => $"{FirstName} {LastName}");
    }
}
```

**✅ Use IMorphableFrom when:**
- DTO knows how to extract from entity
- Working in application/presentation layer
- DTO is in a separate assembly

```csharp
public class UserDto : IMorphableFrom<User> {
    public void MorphFrom(Schema<User> schema) {
        schema.Bind(() => FullName, src => $"{src.FirstName} {src.LastName}");
    }
}
```

---

### 2. Leverage Automatic Mapping

**✅ DO:**
```csharp
public class UserDto : IMorphableFrom<User> {
    public Guid Id { get; set; }      // Auto-mapped
    public string Email { get; set; } // Auto-mapped
    public string DisplayName { get; set; }

    public void MorphFrom(Schema<User> schema) {
        // Only configure what's different
        schema.Bind(() => DisplayName, src => $"{src.FirstName} {src.LastName}");
    }
}
```

**❌ DON'T:**
```csharp
public class UserDto : IMorphableFrom<User> {
    public Guid Id { get; set; }
    public string Email { get; set; }

    public void MorphFrom(Schema<User> schema) {
        // Unnecessary - these are auto-mapped!
        schema.Bind(() => Id, src => src.Id);
        schema.Bind(() => Email, src => src.Email);
    }
}
```

---

### 3. Use Async for External Calls

**✅ DO:**
```csharp
public void MorphFrom(Schema<User> schema) {
    schema.BindAsync(() => ProfileImageUrl, async src => {
        return await _storageService.GetUrlAsync(src.ProfileImageId);
    });
}

// Use ToAsync
var dto = await user.ToAsync<UserDto>();
```

**❌ DON'T:**
```csharp
public void MorphFrom(Schema<User> schema) {
    schema.Bind(() => ProfileImageUrl, src => {
        // Blocking async call!
        return _storageService.GetUrlAsync(src.ProfileImageId).Result;
    });
}
```

---

### 4. Ignore Calculated Properties

**✅ DO:**
```csharp
public class UserDto {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int InternalCalculation { get; set; }  // Not from source
}

public class UserEntity : IMorphableTo<UserDto> {
    public void MorphTo(Schema<UserDto> schema) {
        schema.Ignore(dto => dto.InternalCalculation);
    }
}
```

---

### 5. Register Morph in Startup

**✅ DO:**
```csharp
// Program.cs
services.AddMorph(settings => settings
    .AddAssembly(typeof(UserDto).Assembly)
    .WithInheritanceFallback());
```

---

## Troubleshooting

### Issue 1: BinderNotFoundException

**Problem:**
```
BinderNotFoundException: No mapping found from User to UserDto
```

**Cause:** No morphable interface implemented.

**Solution:**
```csharp
// Implement one of the morphable interfaces
public class User : IMorphableTo<UserDto> {
    public void MorphTo(Schema<UserDto> schema) {
        // Configure bindings
    }
}
```

---

### Issue 2: InvalidMorphConfigurationException

**Problem:** Morph not configured in DI.

**Solution:**
```csharp
// Add in Program.cs
services.AddMorph();
```

---

### Issue 3: Async Bindings Not Executing

**Problem:** Using `To()` instead of `ToAsync()` with async bindings.

**Solution:**
```csharp
// ❌ Wrong
var dto = user.To<UserDto>();  // Async bindings won't execute!

// ✅ Correct
var dto = await user.ToAsync<UserDto>();
```

---

### Issue 4: Properties Not Auto-Mapped

**Problem:** Properties with different names not mapping.

**Cause:** Auto-mapping only works for exact name matches.

**Solution:**
```csharp
// Configure explicit binding
schema.Bind(() => UserId, src => src.Id);
```

---

## Performance Considerations

1. **Caching**: Mappings are cached after first use
2. **Reflection**: First transformation is slower (reflection), subsequent are fast
3. **Async Overhead**: Only use async when needed (external calls, database queries)
4. **Collection Size**: Large collections benefit from parallel processing
5. **Proxy Resolution**: EF proxies are automatically resolved to real types

---

## Integration with Myth Ecosystem

```csharp
using Myth.Extensions;
using Myth.Interfaces;

// Works with Myth.Repository
var paginatedUsers = await _repository.GetPaginatedAsync(pagination);
var paginatedDtos = paginatedUsers.To<IPaginated<UserDto>>();

// Works with Myth.Guard
var dto = user.To<UserDto>();
await _validator.ValidateAsync(dto);

// Works with Myth.Commons Constant types
var order = new Order { Status = OrderStatus.Pending };
var dto = order.To<OrderDto>();  // Status auto-converted
```

---

## Summary

Myth.Morph provides:

- ✅ **Declarative Mapping**: Clean, readable transformation definitions
- ✅ **Automatic Mapping**: Less boilerplate for matching properties
- ✅ **Type Safety**: Full compile-time checking
- ✅ **DI Integration**: Access services during transformation
- ✅ **Async Support**: First-class async transformation
- ✅ **Bidirectional**: Support both mapping directions
- ✅ **Collections**: Automatic collection transformation
- ✅ **Extensible**: Custom mappings and configurations

---

## Additional Resources

- **Repository**: https://gitlab.com/dotnet-myth/myth
- **License**: Apache 2.0
- **Target Framework**: .NET 8.0
- **NuGet Package**: Myth.Morph

---

*This documentation is maintained for AI agents and developers. For questions or contributions, please refer to the repository.*
