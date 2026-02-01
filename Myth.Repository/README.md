<img  style="float: right;" src="myth-repository-logo.png" alt="drawing" width="250"/>

# Myth.Repository

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Repository?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Repository/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Repository?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Repository/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

**Myth.Repository** is a .NET library that provides a clean, standardized set of repository interfaces for data access. It promotes separation of concerns through read/write segregation, integrates seamlessly with the Specification pattern, and supports pagination out of the box. Perfect for building maintainable, testable, and domain-driven applications.

## 🎯 Why Myth.Repository?

**Data access code is often the messiest part of applications.** Controllers directly calling DbContext, business logic mixed with SQL, impossible to test without a real database, tight coupling to EF Core making migrations painful. **Myth.Repository provides clean abstractions** that decouple business logic from data access, enable true unit testing with mocks, support CQRS with read/write segregation, and work with the Specification pattern for reusable queries.

### The Problem: Leaky Data Access

Controllers directly inject `DbContext`, business logic knows about EF Core, queries scattered everywhere, can't test without database, changing ORM requires rewriting entire app.

### The Solution: Repository Pattern

**Clean abstraction**: `IRepository<T>` hides EF Core details. **Read/Write segregation**: `IReadRepository` for queries (CQRS queries), `IWriteRepository` for modifications (CQRS commands). **Specification integration**: Pass `ISpec<T>` for complex queries encapsulating business rules. **Testable**: Mock `IRepository<T>`, no database needed. **Pagination built-in**: `IPaginated<T>` results.

### Key Benefits

**Testability**: Mock repositories in unit tests, no DbContext needed. **CQRS-ready**: Read/write segregation aligns perfectly with CQRS. **DDD-aligned**: Repositories are tactical DDD pattern for aggregate persistence. **ORM-agnostic**: Interfaces work with any implementation (EF Core, Dapper, Cosmos). **Specification integration**: Reusable, testable query logic.

### Real-World Applications

**E-Commerce**: `IProductRepository`, `IOrderRepository` with specifications for complex filters (active products, orders by customer, price ranges). **SaaS Multi-Tenant**: Repositories automatically filter by tenant ID via specification. **Event-Sourced Systems**: Repository stores aggregates, encapsulates event storage logic.

## Features

- **Repository Interfaces**: Standard contracts for data access operations
- **Read/Write Segregation**: Separate interfaces for read and write operations (CQRS-friendly)
- **Specification Pattern Integration**: First-class support for `ISpec<T>` from Myth.Specification
- **Expression Support**: Works with both specifications and LINQ expressions
- **Pagination**: Built-in pagination support with `IPaginated<T>` results
- **Async-First**: All operations are asynchronous with cancellation token support
- **Query Operations**: Rich querying capabilities (Where, First, Last, Any, All, Count)
- **Batch Operations**: Support for bulk inserts, updates, and deletes
- **IQueryable Support**: Access to underlying queryable for advanced scenarios
- **IAsyncDisposable**: Proper resource cleanup with async disposal

## Installation

```bash
dotnet add package Myth.Repository
```

For Entity Framework Core implementation:

```bash
dotnet add package Myth.Repository.EntityFramework
```

## Quick Start

### Define Your Entity

```csharp
public class Product {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string Category { get; set; }
}
```

### Create Repository Interface

```csharp
using Myth.Interfaces.Repositories.Base;

public interface IProductRepository : IReadWriteRepositoryAsync<Product> {
    // Add custom methods if needed
}
```

### Implement Repository (Entity Framework Example)

```csharp
using Myth.Repository.EntityFramework.Repositories;

public class ProductRepository : ReadWriteRepositoryAsync<Product>, IProductRepository {
    public ProductRepository( DbContext context ) : base( context ) {
    }
}
```

### Use in Your Application

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    public ProductService( IProductRepository repository ) {
        _repository = repository;
    }

    public async Task<Product?> GetProductByIdAsync( Guid id, CancellationToken cancellationToken ) {
        return await _repository.FirstOrDefaultAsync( p => p.Id == id, cancellationToken );
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync( CancellationToken cancellationToken ) {
        return await _repository.SearchAsync( p => p.IsActive, cancellationToken );
    }

    public async Task CreateProductAsync( Product product, CancellationToken cancellationToken ) {
        await _repository.AddAsync( product, cancellationToken );
    }
}
```

## Core Interfaces

### IRepository

Base marker interface for all repository types.

```csharp
public interface IRepository { }
```

### IReadRepositoryAsync<TEntity>

Provides read-only operations for querying data.

```csharp
public interface IReadRepositoryAsync<TEntity> : IRepository, IAsyncDisposable {
    // Queryable access
    IQueryable<TEntity> Where( ISpec<TEntity> specification );
    IQueryable<TEntity> Where( Expression<Func<TEntity, bool>> predicate );
    IQueryable<TEntity> AsQueryable( );
    IEnumerable<TEntity> AsEnumerable( );

    // Search operations
    Task<IEnumerable<TEntity>> SearchAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<IEnumerable<TEntity>> SearchAsync( Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default );

    // Paginated search
    Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> filterPredicate, Pagination pagination, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default );

    // Single item retrieval
    Task<TEntity?> FirstOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<TEntity?> FirstOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
    Task<TEntity> FirstAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<TEntity> FirstAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

    Task<TEntity?> LastOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<TEntity?> LastOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
    Task<TEntity> LastAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<TEntity> LastAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

    // Aggregation operations
    Task<int> CountAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<int> CountAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

    Task<bool> AnyAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<bool> AnyAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

    Task<bool> AllAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<bool> AllAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

    // Get all
    Task<IEnumerable<TEntity>> ToListAsync( CancellationToken cancellationToken = default );
}
```

### IWriteRepositoryAsync<TEntity>

Provides write operations for modifying data.

```csharp
public interface IWriteRepositoryAsync<TEntity> : IRepository, IAsyncDisposable {
    // Single operations
    Task AddAsync( TEntity entity, CancellationToken cancellationToken = default );
    Task UpdateAsync( TEntity entity, CancellationToken cancellationToken = default );
    Task RemoveAsync( TEntity entity, CancellationToken cancellationToken = default );

    // Batch operations
    Task AddRangeAsync( IEnumerable<TEntity> entity, CancellationToken cancellationToken = default );
    Task UpdateRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );
    Task RemoveRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );
}
```

### IReadWriteRepositoryAsync<TEntity>

Combines both read and write operations in a single interface.

```csharp
public interface IReadWriteRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity>, IWriteRepositoryAsync<TEntity>, IAsyncDisposable { }
```

## Pagination Support

### IPaginated<T>

Interface representing paginated results:

```csharp
public interface IPaginated<T> {
    int PageNumber { get; }       // Current page number (1-based)
    int PageSize { get; }          // Number of items per page
    int TotalPages { get; }        // Total number of pages
    int TotalItems { get; }        // Total number of items across all pages
    IEnumerable<T> Items { get; }  // Items in current page
}
```

### Pagination Value Object

```csharp
public class Pagination {
    public int PageNumber { get; set; }  // Default: 1
    public int PageSize { get; set; }     // Default: 10

    // Static helpers
    public static readonly Pagination Default = new( 1, 10 );
    public static readonly Pagination All = new( -1, -1 );
}
```

### Extension Method

```csharp
using Myth.Extensions;

// Convert any IEnumerable to paginated result
var items = new List<Product> { /* ... */ };
var paginated = items.AsPaginated( pageSize: 20, skip: 0 );

// Or with Pagination object
var pagination = new Pagination( pageNumber: 2, pageSize: 20 );
var paginated = items.AsPaginated( pagination );
```

## Usage Examples

### Basic CRUD Operations

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    public ProductService( IProductRepository repository ) {
        _repository = repository;
    }

    public async Task CreateAsync( Product product, CancellationToken ct ) {
        await _repository.AddAsync( product, ct );
    }

    public async Task CreateManyAsync( IEnumerable<Product> products, CancellationToken ct ) {
        await _repository.AddRangeAsync( products, ct );
    }

    public async Task UpdateAsync( Product product, CancellationToken ct ) {
        await _repository.UpdateAsync( product, ct );
    }

    public async Task DeleteAsync( Product product, CancellationToken ct ) {
        await _repository.RemoveAsync( product, ct );
    }
}
```

### Querying with Expressions

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    public async Task<Product?> GetByIdAsync( Guid id, CancellationToken ct ) {
        return await _repository.FirstOrDefaultAsync( p => p.Id == id, ct );
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync( string category, CancellationToken ct ) {
        return await _repository.SearchAsync( p => p.Category == category, ct );
    }

    public async Task<int> CountActiveProductsAsync( CancellationToken ct ) {
        return await _repository.CountAsync( p => p.IsActive, ct );
    }

    public async Task<bool> HasExpensiveProductsAsync( CancellationToken ct ) {
        return await _repository.AnyAsync( p => p.Price > 1000, ct );
    }

    public async Task<bool> AreAllActiveAsync( CancellationToken ct ) {
        return await _repository.AllAsync( p => p.IsActive, ct );
    }
}
```

### Querying with Specifications

```csharp
using Myth.Specification;

// Define specifications
public static class ProductSpecifications {
    public static ISpec<Product> IsActive( this ISpec<Product> spec ) {
        return spec.And( p => p.IsActive );
    }

    public static ISpec<Product> InCategory( this ISpec<Product> spec, string category ) {
        return spec.And( p => p.Category == category );
    }

    public static ISpec<Product> PriceRange( this ISpec<Product> spec, decimal min, decimal max ) {
        return spec.And( p => p.Price >= min && p.Price <= max );
    }

    public static ISpec<Product> OrderByName( this ISpec<Product> spec ) {
        return spec.Order( p => p.Name );
    }
}

// Use in repository
public class ProductService {
    private readonly IProductRepository _repository;

    public async Task<IEnumerable<Product>> GetActiveProductsInCategoryAsync(
        string category,
        CancellationToken ct ) {

        var spec = SpecBuilder<Product>.Create( )
            .IsActive( )
            .InCategory( category )
            .OrderByName( );

        return await _repository.SearchAsync( spec, ct );
    }

    public async Task<IEnumerable<Product>> GetAffordableProductsAsync(
        decimal maxPrice,
        CancellationToken ct ) {

        var spec = SpecBuilder<Product>.Create( )
            .IsActive( )
            .PriceRange( 0, maxPrice )
            .OrderByName( );

        return await _repository.SearchAsync( spec, ct );
    }
}
```

### Pagination

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    // Paginate with specification
    public async Task<IPaginated<Product>> GetProductsPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct ) {

        var pagination = new Pagination( pageNumber, pageSize );

        var spec = SpecBuilder<Product>.Create( )
            .IsActive( )
            .OrderByName( )
            .Skip( (pageNumber - 1) * pageSize )
            .Take( pageSize );

        return await _repository.SearchPaginatedAsync( spec, ct );
    }

    // Paginate with expression
    public async Task<IPaginated<Product>> GetProductsByCategoryPageAsync(
        string category,
        Pagination pagination,
        CancellationToken ct ) {

        return await _repository.SearchPaginatedAsync(
            filterPredicate: p => p.Category == category && p.IsActive,
            pagination: pagination,
            orderPredicate: p => p.Name,
            cancellationToken: ct );
    }

    // Use paginated results
    public async Task DisplayProductsAsync( CancellationToken ct ) {
        var result = await GetProductsPageAsync( pageNumber: 1, pageSize: 20, ct );

        Console.WriteLine( $"Page {result.PageNumber} of {result.TotalPages}" );
        Console.WriteLine( $"Total items: {result.TotalItems}" );

        foreach ( var product in result.Items ) {
            Console.WriteLine( $"- {product.Name}: ${product.Price}" );
        }
    }
}
```

### Advanced Querying with IQueryable

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    public async Task<IEnumerable<ProductSummary>> GetProductSummariesAsync( CancellationToken ct ) {
        var queryable = _repository
            .AsQueryable( )
            .Where( p => p.IsActive )
            .GroupBy( p => p.Category )
            .Select( g => new ProductSummary {
                Category = g.Key,
                Count = g.Count( ),
                AveragePrice = g.Average( p => p.Price )
            } );

        return await queryable.ToListAsync( ct );
    }
}
```

### Read/Write Segregation (CQRS)

```csharp
// Read-only service
public class ProductQueryService {
    private readonly IReadRepositoryAsync<Product> _repository;

    public ProductQueryService( IReadRepositoryAsync<Product> repository ) {
        _repository = repository;
    }

    public async Task<IEnumerable<Product>> GetAllAsync( CancellationToken ct ) {
        return await _repository.ToListAsync( ct );
    }
}

// Write-only service
public class ProductCommandService {
    private readonly IWriteRepositoryAsync<Product> _repository;

    public ProductCommandService( IWriteRepositoryAsync<Product> repository ) {
        _repository = repository;
    }

    public async Task CreateAsync( Product product, CancellationToken ct ) {
        await _repository.AddAsync( product, ct );
    }
}
```

## Integration with Other Myth Libraries

### With Myth.Specification

```csharp
var spec = SpecBuilder<Product>.Create( )
    .IsActive( )
    .InCategory( "Electronics" )
    .PriceRange( 100, 500 )
    .OrderByName( )
    .Take( 50 );

var products = await _repository.SearchAsync( spec, cancellationToken );
```

### With Myth.Flow.Actions (CQRS)

```csharp
public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IEnumerable<Product>> {
    private readonly IReadRepositoryAsync<Product> _repository;

    public async Task<QueryResult<IEnumerable<Product>>> HandleAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken ) {

        var products = await _repository.SearchAsync(
            p => p.Category == query.Category,
            cancellationToken );

        return QueryResult<IEnumerable<Product>>.Success( products );
    }
}
```

### With Myth.Guard (Validation)

```csharp
public class ProductCommandService {
    private readonly IWriteRepositoryAsync<Product> _repository;
    private readonly IValidator _validator;

    public async Task CreateAsync( Product product, CancellationToken ct ) {
        await _validator.ValidateAsync( product, ValidationContextKey.Create, ct );
        await _repository.AddAsync( product, ct );
    }
}
```

## Architecture Patterns

### Domain-Driven Design (DDD)

```csharp
// Aggregate Root
public class Order : IAggregateRoot {
    public Guid Id { get; private set; }
    private List<OrderItem> _items = new( );
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly( );

    public void AddItem( Product product, int quantity ) {
        _items.Add( new OrderItem( product, quantity ) );
    }
}

// Repository in domain layer (interface)
public interface IOrderRepository : IReadWriteRepositoryAsync<Order> {
    Task<Order?> GetByIdWithItemsAsync( Guid id, CancellationToken ct );
}

// Implementation in infrastructure layer
public class OrderRepository : ReadWriteRepositoryAsync<Order>, IOrderRepository {
    public OrderRepository( DbContext context ) : base( context ) { }

    public async Task<Order?> GetByIdWithItemsAsync( Guid id, CancellationToken ct ) {
        return await AsQueryable( )
            .Include( o => o.Items )
            .FirstOrDefaultAsync( o => o.Id == id, ct );
    }
}
```

### Unit of Work Pattern

See `Myth.Repository.EntityFramework` for `IUnitOfWorkRepository` implementation.

```csharp
public class OrderService {
    private readonly IUnitOfWorkRepository _unitOfWork;

    public async Task ProcessOrderAsync( Order order, CancellationToken ct ) {
        await _unitOfWork.BeginTransactionAsync( ct );

        try {
            await _unitOfWork.AddAsync( order, ct );
            await _unitOfWork.SaveChangesAsync( ct );
            await _unitOfWork.CommitTransactionAsync( ct );
        }
        catch {
            await _unitOfWork.RollbackTransactionAsync( ct );
            throw;
        }
    }
}
```

## Best Practices

1. **Use Specifications for Complex Queries**: Encapsulate business rules in specification extensions
2. **Leverage Read/Write Segregation**: Use separate interfaces when following CQRS patterns
3. **Always Use Cancellation Tokens**: Support graceful cancellation in long-running operations
4. **Dispose Repositories Properly**: Use `await using` or dependency injection for automatic disposal
5. **Paginate Large Result Sets**: Use `SearchPaginatedAsync` to avoid loading too much data
6. **Keep Repositories Thin**: Move business logic to services or domain entities
7. **Use IQueryable Sparingly**: Prefer specifications and expressions for better testability
8. **Implement Custom Methods**: Add domain-specific methods to repository interfaces when needed

## Testing

```csharp
public class ProductServiceTests {
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly ProductService _service;

    public ProductServiceTests( ) {
        _mockRepository = new Mock<IProductRepository>( );
        _service = new ProductService( _mockRepository.Object );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists( ) {
        var productId = Guid.NewGuid( );
        var product = new Product { Id = productId, Name = "Test" };

        _mockRepository
            .Setup( r => r.FirstOrDefaultAsync( It.IsAny<Expression<Func<Product, bool>>>( ), default ) )
            .ReturnsAsync( product );

        var result = await _service.GetByIdAsync( productId, default );

        result.Should( ).NotBeNull( );
        result.Id.Should( ).Be( productId );
    }
}
```

## Related Packages

- **Myth.Repository.EntityFramework**: Entity Framework Core implementation with Unit of Work support
- **Myth.Specification**: Build complex, reusable query specifications
- **Myth.Commons**: Value objects and pagination support (IPaginated<T>)

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) for details.
