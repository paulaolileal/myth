# Myth.Repository.EntityFramework

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Repository.EntityFramework/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Repository.EntityFramework/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A comprehensive .NET library that provides robust, feature-rich repository implementations for Entity Framework Core with Domain-Driven Design (DDD) patterns, Specification pattern support, and automatic dependency injection.

## Table of Contents
- [Features](#-features)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Core Concepts](#-core-concepts)
- [Repository Types](#-repository-types)
- [Advanced Usage](#-advanced-usage)
- [Unit of Work Pattern](#-unit-of-work-pattern)
- [Dependency Injection](#-dependency-injection)

## ⭐ Features

- **BaseContext**: Abstract DbContext with automatic entity configuration discovery
- **Repository Implementations**:
  - `ReadRepositoryAsync<TEntity>`: Read-only operations
  - `WriteRepositoryAsync<TEntity>`: Write operations (Add, Update, Remove)
  - `ReadWriteRepositoryAsync<TEntity>`: Combined read/write operations
- **Specification Pattern Support**: Seamlessly integrate with Myth.Specification
- **Expression-Based Queries**: Direct LINQ expression support
- **Unit of Work Pattern**: Transaction management with savepoints
- **Automatic Dependency Injection**: Zero-configuration repository registration
- **Pagination Support**: Built-in paginated query results
- **Async/Await**: Fully asynchronous API
- **Entity Attach/Detach**: Fine-grained entity state management

## 📦 Installation

Install via NuGet Package Manager:

```bash
dotnet add package Myth.Repository.EntityFramework
```

Or via Package Manager Console:

```powershell
Install-Package Myth.Repository.EntityFramework
```

## 🚀 Quick Start

### 1. Create Your DbContext

```csharp
using Myth.Contexts;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : BaseContext {
    public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : base( options ) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
}
```

The `BaseContext` automatically discovers and applies all `IEntityTypeConfiguration<T>` implementations in your assembly.

### 2. Create Repository Interface

```csharp
using Myth.Interfaces.Repositories.EntityFramework;

public interface IUserRepository : IReadWriteRepositoryAsync<User> {
    // Add custom methods if needed
    Task<User?> GetByEmailAsync( string email, CancellationToken cancellationToken = default );
}
```

### 3. Implement Repository

```csharp
using Myth.Repositories.EntityFramework;

public class UserRepository : ReadWriteRepositoryAsync<User>, IUserRepository {

    public UserRepository( ApplicationDbContext context ) : base( context ) { }

    public async Task<User?> GetByEmailAsync( string email, CancellationToken cancellationToken = default ) {
        return await FirstOrDefaultAsync( u => u.Email == email, cancellationToken );
    }
}
```

### 4. Register in Startup

```csharp
var builder = WebApplication.CreateBuilder( args );

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>( options =>
    options.UseSqlServer( builder.Configuration.GetConnectionString( "DefaultConnection" ) ) );

// Automatically register ALL repositories
builder.Services.AddRepositories( );

var app = builder.BuildApp( ); // Use BuildApp() for cross-library dependency resolution
app.Run( );
```

### 5. Use in Your Services

```csharp
public class UserService {
    private readonly IUserRepository _userRepository;

    public UserService( IUserRepository userRepository ) {
        _userRepository = userRepository;
    }

    public async Task<User?> GetUserByEmailAsync( string email ) {
        return await _userRepository.GetByEmailAsync( email );
    }

    public async Task CreateUserAsync( User user ) {
        await _userRepository.AddAsync( user );
        await _userRepository.SaveChangesAsync( );
    }
}
```

## 🎯 Core Concepts

### BaseContext

The `BaseContext` is an abstract base class that extends `DbContext` and provides automatic entity configuration:

```csharp
public abstract class BaseContext : DbContext {
    protected override void OnModelCreating( ModelBuilder modelBuilder ) {
        // Automatically applies all IEntityTypeConfiguration<T> from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly( GetType( ).Assembly );
    }
}
```

**Benefits:**
- No manual configuration registration
- Convention-based entity mapping discovery
- Cleaner, more maintainable code

## 📂 Repository Types

### IReadRepositoryAsync<TEntity>

Read-only repository for query operations.

**Core Methods:**

```csharp
// Basic queries
Task<IEnumerable<TEntity>> ToListAsync( CancellationToken cancellationToken = default );
IQueryable<TEntity> AsQueryable( );
IEnumerable<TEntity> AsEnumerable( );

// Filtered queries
IQueryable<TEntity> Where( Expression<Func<TEntity, bool>> predicate );
Task<IEnumerable<TEntity>> SearchAsync( Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, bool>>? orderBy = null, CancellationToken cancellationToken = default );

// Pagination
Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> predicate, int take = 0, int skip = 0, Expression<Func<TEntity, bool>>? orderBy = null, CancellationToken cancellationToken = default );
Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> predicate, Pagination pagination, Expression<Func<TEntity, bool>>? orderBy = null, CancellationToken cancellationToken = default );

// Single item retrieval
Task<TEntity?> FirstOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
Task<TEntity?> LastOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
Task<TEntity> FirstAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
Task<TEntity> LastAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

// Aggregate operations
Task<int> CountAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
Task<bool> AnyAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
Task<bool> AllAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

// Metadata
string? GetProviderName( );
```

**Specification Pattern Support:**

All methods have specification-based overloads:

```csharp
Task<IEnumerable<TEntity>> SearchAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
IQueryable<TEntity> Where( ISpec<TEntity> specification );
Task<int> CountAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
// ... and more
```

### IWriteRepositoryAsync<TEntity>

Write operations for creating, updating, and deleting entities.

**Methods:**

```csharp
// Create
Task AddAsync( TEntity entity, CancellationToken cancellationToken = default );
Task AddRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );

// Update
Task UpdateAsync( TEntity entity, CancellationToken cancellationToken = default );
Task UpdateRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );

// Delete
Task RemoveAsync( TEntity entity, CancellationToken cancellationToken = default );
Task RemoveRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );

// Entity state management
Task AttachAsync( TEntity entity, CancellationToken cancellationToken = default );
Task AttachRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );
```

### IReadWriteRepositoryAsync<TEntity>

Combines both read and write operations. This is the most commonly used repository type.

**Usage:**

```csharp
public class ProductRepository : ReadWriteRepositoryAsync<Product>, IProductRepository {
    public ProductRepository( ApplicationDbContext context ) : base( context ) { }
}
```

## 🔧 Advanced Usage

### Working with Specifications

The library integrates seamlessly with `Myth.Specification`:

```csharp
using Myth.Specification;

// Define specifications
public static class UserSpecifications {
    public static ISpec<User> IsActive( this ISpec<User> spec ) {
        return spec.And( u => u.IsActive );
    }

    public static ISpec<User> HasRole( this ISpec<User> spec, string role ) {
        return spec.And( u => u.Role == role );
    }
}

// Use in repository
public class UserRepository : ReadWriteRepositoryAsync<User>, IUserRepository {
    public UserRepository( ApplicationDbContext context ) : base( context ) { }

    public async Task<IEnumerable<User>> GetActiveAdminsAsync( ) {
        var spec = SpecBuilder<User>.Create( )
            .IsActive( )
            .HasRole( "Admin" )
            .Order( u => u.Name );

        return await SearchAsync( spec );
    }

    public async Task<IPaginated<User>> GetActiveUsersPaginatedAsync( int page, int pageSize ) {
        var spec = SpecBuilder<User>.Create( )
            .IsActive( )
            .Order( u => u.CreatedAt )
            .Skip( ( page - 1 ) * pageSize )
            .Take( pageSize );

        return await SearchPaginatedAsync( spec );
    }
}
```

### Pagination Examples

**Using Expression-Based Pagination:**

```csharp
public async Task<IPaginated<Product>> GetProductsAsync( int page, int pageSize ) {
    var skip = ( page - 1 ) * pageSize;
    return await _productRepository.SearchPaginatedAsync(
        predicate: p => p.IsAvailable,
        take: pageSize,
        skip: skip,
        orderPredicate: p => p.Price
    );
}
```

**Using Pagination Object:**

```csharp
public async Task<IPaginated<Product>> GetProductsAsync( Pagination pagination ) {
    return await _productRepository.SearchPaginatedAsync(
        predicate: p => p.IsAvailable,
        pagination: pagination,
        orderPredicate: p => p.Name
    );
}
```

**Result Structure:**

```csharp
public interface IPaginated<T> {
    IEnumerable<T> Items { get; }
    int TotalItems { get; }
    int PageSize { get; }
    int CurrentPage { get; }
    int TotalPages { get; }
    bool HasPrevious { get; }
    bool HasNext { get; }
}
```

### Custom Repository Methods

Extend base repositories with domain-specific methods:

```csharp
public interface IOrderRepository : IReadWriteRepositoryAsync<Order> {
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync( Guid customerId, CancellationToken cancellationToken = default );
    Task<Order?> GetOrderWithItemsAsync( Guid orderId, CancellationToken cancellationToken = default );
    Task<decimal> GetTotalRevenueAsync( DateTime from, DateTime to, CancellationToken cancellationToken = default );
}

public class OrderRepository : ReadWriteRepositoryAsync<Order>, IOrderRepository {

    public OrderRepository( ApplicationDbContext context ) : base( context ) { }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync( Guid customerId, CancellationToken cancellationToken = default ) {
        return await SearchAsync( o => o.CustomerId == customerId, o => o.OrderDate, cancellationToken );
    }

    public async Task<Order?> GetOrderWithItemsAsync( Guid orderId, CancellationToken cancellationToken = default ) {
        return await AsQueryable( )
            .Include( o => o.OrderItems )
            .ThenInclude( oi => oi.Product )
            .FirstOrDefaultAsync( o => o.Id == orderId, cancellationToken );
    }

    public async Task<decimal> GetTotalRevenueAsync( DateTime from, DateTime to, CancellationToken cancellationToken = default ) {
        var orders = await SearchAsync( o => o.OrderDate >= from && o.OrderDate <= to, null, cancellationToken );
        return orders.Sum( o => o.TotalAmount );
    }
}
```

## 🪄 Unit of Work Pattern

The `IUnitOfWorkRepository` provides transaction management capabilities.

### Interface

```csharp
public interface IUnitOfWorkRepository : IAsyncDisposable {
    Task BeginTransactionAsync( CancellationToken cancellationToken = default );
    Task CommitAsync( CancellationToken cancellationToken = default );
    Task RollbackAsync( CancellationToken cancellationToken = default );
    Task CreateSavepointAsync( string savepointName, CancellationToken cancellationToken = default );
    Task RollbackToSavepointAsync( string savepointName, CancellationToken cancellationToken = default );
    Task<int> SaveChangesAsync( CancellationToken cancellationToken = default );
    Task<int> ExecuteSqlAsync( string query, IEnumerable<object>? parameters = null, CancellationToken cancellationToken = default );
}
```

### Implementation

```csharp
// Create custom Unit of Work
public class ApplicationUnitOfWork : BaseUnitOfWorkRepository {
    public ApplicationUnitOfWork( ApplicationDbContext context ) : base( context ) { }
}

// Or use generic implementation
public class GenericUnitOfWork : UnitOfWorkRepository<ApplicationDbContext> {
    public GenericUnitOfWork( ApplicationDbContext context ) : base( context ) { }
}
```

### Registration

```csharp
// Option 1: Custom implementation
builder.Services.AddUnitOfWork<ApplicationUnitOfWork>( );

// Option 2: Generic implementation
builder.Services.AddUnitOfWorkForContext<ApplicationDbContext>( );
```

### Usage Example

**Basic Transaction:**

```csharp
public class OrderService {
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWorkRepository _unitOfWork;

    public OrderService( IOrderRepository orderRepository, IInventoryRepository inventoryRepository, IUnitOfWorkRepository unitOfWork ) {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> CreateOrderAsync( CreateOrderRequest request ) {
        await _unitOfWork.BeginTransactionAsync( );

        try {
            // Create order
            var order = new Order { CustomerId = request.CustomerId, OrderDate = DateTime.UtcNow };
            await _orderRepository.AddAsync( order );
            await _unitOfWork.SaveChangesAsync( );

            // Update inventory
            foreach ( var item in request.Items ) {
                var inventory = await _inventoryRepository.FirstOrDefaultAsync( i => i.ProductId == item.ProductId );
                if ( inventory == null || inventory.Quantity < item.Quantity )
                    throw new InvalidOperationException( "Insufficient inventory" );

                inventory.Quantity -= item.Quantity;
                await _inventoryRepository.UpdateAsync( inventory );
            }
            await _unitOfWork.SaveChangesAsync( );

            // Commit transaction
            await _unitOfWork.CommitAsync( );
            return Result.Success( );
        }
        catch ( Exception ex ) {
            await _unitOfWork.RollbackAsync( );
            return Result.Failure( ex.Message );
        }
    }
}
```

**Transaction with Savepoints:**

```csharp
public async Task<Result> ComplexOperationAsync( ) {
    await _unitOfWork.BeginTransactionAsync( );

    try {
        // Step 1: Create user
        await CreateUserAsync( );
        await _unitOfWork.SaveChangesAsync( );
        await _unitOfWork.CreateSavepointAsync( "AfterUserCreation" );

        // Step 2: Create profile (might fail)
        try {
            await CreateUserProfileAsync( );
            await _unitOfWork.SaveChangesAsync( );
        }
        catch {
            // Rollback to savepoint, keeping user creation
            await _unitOfWork.RollbackToSavepointAsync( "AfterUserCreation" );
            await CreateDefaultProfileAsync( );
            await _unitOfWork.SaveChangesAsync( );
        }

        await _unitOfWork.CommitAsync( );
        return Result.Success( );
    }
    catch ( Exception ex ) {
        await _unitOfWork.RollbackAsync( );
        return Result.Failure( ex.Message );
    }
}
```

## 🚀 Dependency Injection

### Automatic Registration

The library provides intelligent automatic registration:

```csharp
// Registers ALL repository implementations from application assemblies
builder.Services.AddRepositories( );
```

**Registration Rules:**
- Scoped lifetime by default (recommended for repositories)
- Automatically excludes test types (Test, Mock, Fake, Stub)
- Automatically excludes generic type definitions
- Matches repositories to interfaces by naming convention

### Registration from Specific Assembly

```csharp
// Register repositories from a specific assembly
builder.Services.AddRepositoriesFromAssembly( typeof( UserRepository ).Assembly );

// With custom lifetime
builder.Services.AddRepositoriesFromAssembly( typeof( UserRepository ).Assembly, ServiceLifetime.Transient );
```

### Manual Registration

```csharp
// Register specific repository manually
builder.Services.AddRepository<IUserRepository, UserRepository>( );

// With custom lifetime
builder.Services.AddRepository<IUserRepository, UserRepository>( ServiceLifetime.Transient );
```

### Unit of Work Registration

```csharp
// Register custom Unit of Work implementation
builder.Services.AddUnitOfWork<ApplicationUnitOfWork>( );

// Or register generic Unit of Work for specific context
builder.Services.AddUnitOfWorkForContext<ApplicationDbContext>( );
```

### Complete Setup Example

```csharp
var builder = WebApplication.CreateBuilder( args );

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>( options => {
    options.UseSqlServer( builder.Configuration.GetConnectionString( "DefaultConnection" ) );
    options.EnableSensitiveDataLogging( builder.Environment.IsDevelopment( ) );
    options.EnableDetailedErrors( builder.Environment.IsDevelopment( ) );
} );

// Repository registration
builder.Services.AddRepositories( ); // Auto-register all repositories

// Unit of Work registration
builder.Services.AddUnitOfWorkForContext<ApplicationDbContext>( );

// Build app with cross-library dependency resolution
var app = builder.BuildApp( );

app.Run( );
```

## 📝 Best Practices

1. **Use Repository Interfaces**: Always depend on interfaces, not concrete implementations
2. **Keep Repositories Thin**: Complex business logic belongs in services, not repositories
3. **Leverage Specifications**: Use Myth.Specification for reusable query logic
4. **Use Unit of Work for Transactions**: When operations span multiple repositories
5. **Async All The Way**: Always use async/await for database operations
6. **Dispose Properly**: Repositories implement `IAsyncDisposable` when needed
7. **Use BuildApp()**: Replace `builder.Build()` with `builder.BuildApp()` for cross-library integration

## 🔗 Related Libraries

- **Myth.Repository**: Base repository interfaces and abstractions
- **Myth.Specification**: Specification pattern implementation for query building
- **Myth.Commons**: Common utilities and extensions

## 📄 License

Licensed under the Apache 2.0 License. See [LICENSE](https://opensource.org/licenses/Apache-2.0) for details.