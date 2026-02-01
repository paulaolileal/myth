# Myth.Specification

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Specification?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Specification/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Specification?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Specification/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](README.md)

Implementation of the Specification Pattern for building reusable, composable, and testable query logic in .NET applications.

## 🎯 Why Myth.Specification?

**Query logic scattered in repositories kills maintainability.** Duplicate WHERE clauses everywhere, business rules buried in SQL, impossible to reuse queries, can't test query logic independently. **Myth.Specification encapsulates business rules as reusable, composable, testable objects** that work with EF Core, LINQ, and in-memory collections. Build complex queries by combining simple specifications. Test query logic without database. Change queries without touching repositories.

### The Problem: Query Logic Duplication

Same filtering logic duplicated across 10 repositories. Business rules (`"active products"`) buried in SQL. Can't reuse or test queries. Changing criteria breaks app everywhere.

### The Solution: Specification Pattern

**Encapsulate queries**: `ActiveProducts`, `ExpensiveProducts` as specifications. **Compose**: Combine with `.And()`, `.Or()`, `.Not()`. **Reuse**: Same specification in repository, controller, validation. **Test**: In-memory `.IsSatisfiedBy()` validates without database. **Fluent**: `.Order()`, `.Take()`, `.Skip()` for complete queries.

### Key Benefits

**Reusable**: Write query once, use everywhere. **Testable**: Validate specifications in-memory without DB. **Composable**: Combine simple specs into complex queries. **DDD-aligned**: Specifications are DDD tactical pattern for business rules. **Type-safe**: Expression-based, compile-time checked.

### Real-World Applications

**E-Commerce**: `ActiveProducts()`, `InPriceRange(min, max)`, `InCategory(cat)` composed for product search. **Reports**: Complex filtering as specifications, reused across multiple reports. **Authorization**: User permissions as specifications applied to queries.

## Features

- **Composable Specifications** - Combine specifications with AND, OR, NOT logical operators
- **Conditional Composition** - AndIf/OrIf for conditional specification building
- **Ordering Support** - Ascending and descending ordering with multi-level ThenBy support
- **Pagination** - Built-in Skip, Take, and WithPagination methods
- **Distinct Operations** - DistinctBy for unique results by property
- **Expression-Based** - Full support for `Expression<Func<T, bool>>` and IQueryable
- **Fluent API** - Chainable methods for readable query building
- **In-Memory Validation** - IsSatisfiedBy for entity validation
- **Extension Methods** - Seamless integration with IEnumerable and IQueryable

## Installation

```bash
dotnet add package Myth.Specification
```

## Quick Start

### Basic Specification

```csharp
using Myth.Interfaces;
using Myth.Specifications;

// Create a specification
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .And(p => p.Price > 100)
    .Order(p => p.Name)
    .Take(10);

// Apply to query
var products = await dbContext.Products
    .Where(spec.Predicate)
    .OrderBy(spec.Sort)
    .Take(spec.ItemsTaked)
    .ToListAsync();

// Or use extension method
var products = await dbContext.Products
    .Specify(spec)
    .ToListAsync();
```

### Reusable Specification Extensions

```csharp
public static class ProductSpecifications {
    public static ISpec<Product> IsActive(this ISpec<Product> spec) {
        return spec.And(p => p.IsActive);
    }

    public static ISpec<Product> InCategory(this ISpec<Product> spec, string category) {
        return spec.And(p => p.Category == category);
    }

    public static ISpec<Product> PriceRange(this ISpec<Product> spec, decimal min, decimal max) {
        return spec.And(p => p.Price >= min && p.Price <= max);
    }

    public static ISpec<Product> OrderByPrice(this ISpec<Product> spec, bool descending = false) {
        return descending
            ? spec.OrderDescending(p => p.Price)
            : spec.Order(p => p.Price);
    }
}

// Usage
var spec = SpecBuilder<Product>.Create()
    .IsActive()
    .InCategory("Electronics")
    .PriceRange(100, 1000)
    .OrderByPrice(descending: true)
    .Take(20);

var products = dbContext.Products.Specify(spec).ToList();
```

## ISpec<T> Interface

The core interface for all specifications.

### Properties

```csharp
Expression<Func<T, bool>> Predicate { get; }                        // Filter expression
Func<T, bool> Query { get; }                                        // Compiled predicate
Func<IQueryable<T>, IOrderedQueryable<T>> Sort { get; }            // Ordering function
Func<IQueryable<T>, IQueryable<T>> PostProcess { get; }            // Post-processing (Skip/Take/Distinct)
int ItemsSkiped { get; }                                            // Number of items to skip
int ItemsTaked { get; }                                             // Number of items to take
```

### Methods

#### Logical Operations

```csharp
ISpec<T> And(ISpec<T> specification)
ISpec<T> And(Expression<Func<T, bool>> expression)
ISpec<T> AndIf(bool condition, ISpec<T> other)
ISpec<T> AndIf(bool condition, Expression<Func<T, bool>> other)

ISpec<T> Or(ISpec<T> specification)
ISpec<T> Or(Expression<Func<T, bool>> expression)
ISpec<T> OrIf(bool condition, ISpec<T> other)
ISpec<T> OrIf(bool condition, Expression<Func<T, bool>> other)

ISpec<T> Not()
```

#### Ordering

```csharp
ISpec<T> Order<TProperty>(Expression<Func<T, TProperty>> property)
ISpec<T> OrderDescending<TProperty>(Expression<Func<T, TProperty>> property)
```

#### Pagination

```csharp
ISpec<T> Skip(int amount)
ISpec<T> Take(int amount)
ISpec<T> WithPagination(Pagination pagination)  // Uses Myth.Commons Pagination value object
```

#### Distinct

```csharp
ISpec<T> DistinctBy<TProperty>(Expression<Func<T, TProperty>> property)
```

#### Query Execution

```csharp
IQueryable<T> Prepare(IQueryable<T> query)          // Apply filter + sort + post-process
IQueryable<T> Filtered(IQueryable<T> query)         // Apply only filter
IQueryable<T> Sorted(IQueryable<T> query)           // Apply only sorting
IQueryable<T> Processed(IQueryable<T> query)        // Apply only post-processing

T? SatisfyingItemFrom(IQueryable<T> query)          // Get first matching item
IQueryable<T> SatisfyingItemsFrom(IQueryable<T> query)  // Get all matching items
```

#### Validation

```csharp
bool IsSatisfiedBy(T entity)  // Check if entity satisfies specification (in-memory)
```

#### Initialization

```csharp
ISpec<T> InitEmpty()  // Reset to empty specification
```

## SpecBuilder<T>

Abstract base class for creating specifications with a fluent API.

### Creating Specifications

```csharp
// Start with empty specification
var spec = SpecBuilder<Product>.Create();

// Chain operations
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .And(p => p.Price > 0)
    .Order(p => p.Name)
    .Take(10);
```

### Implicit Conversion

```csharp
SpecBuilder<T> → Expression<Func<T, bool>>

// Can be used directly where Expression is expected
Expression<Func<Product, bool>> expr = SpecBuilder<Product>.Create()
    .And(p => p.IsActive);
```

## Logical Operations

### AND

```csharp
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .And(p => p.Price > 100)
    .And(p => p.Stock > 0);

// Conditional AND
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .AndIf(!string.IsNullOrEmpty(searchTerm), p => p.Name.Contains(searchTerm))
    .AndIf(minPrice.HasValue, p => p.Price >= minPrice.Value);
```

### OR

```csharp
var spec = SpecBuilder<Product>.Create()
    .Or(p => p.Category == "Electronics")
    .Or(p => p.Category == "Computers");

// Conditional OR
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .OrIf(includeDiscounted, p => p.IsDiscounted);
```

### NOT

```csharp
var activeSpec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive);

var inactiveSpec = activeSpec.Not();  // Inverts the specification
```

### Complex Combinations

```csharp
// (IsActive AND Price > 100) OR (IsDiscounted AND Stock > 0)
var spec1 = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .And(p => p.Price > 100);

var spec2 = SpecBuilder<Product>.Create()
    .And(p => p.IsDiscounted)
    .And(p => p.Stock > 0);

var combinedSpec = spec1.Or(spec2);
```

## Ordering

### Single Level

```csharp
// Ascending
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .Order(p => p.Name);

// Descending
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .OrderDescending(p => p.Price);
```

### Multi-Level Ordering

The specification automatically uses `ThenBy`/`ThenByDescending` for subsequent ordering:

```csharp
var spec = SpecBuilder<Product>.Create()
    .Order(p => p.Category)           // OrderBy
    .OrderDescending(p => p.Price)    // ThenByDescending
    .Order(p => p.Name);              // ThenBy

// Equivalent to:
query.OrderBy(p => p.Category)
    .ThenByDescending(p => p.Price)
    .ThenBy(p => p.Name);
```

## Pagination

### Skip and Take

```csharp
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .Skip(20)   // Skip first 20 items
    .Take(10);  // Take next 10 items

Console.WriteLine($"Skipped: {spec.ItemsSkiped}");  // 20
Console.WriteLine($"Taken: {spec.ItemsTaked}");      // 10
```

### WithPagination

Uses the `Pagination` value object from Myth.Commons:

```csharp
var pagination = new Pagination(pageNumber: 2, pageSize: 20);

var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .WithPagination(pagination);

// Automatically calculates: Skip((2-1) * 20) = Skip(20), Take(20)
```

### Special Pagination Values

```csharp
// Get all items (no pagination)
var spec = SpecBuilder<Product>.Create()
    .WithPagination(Pagination.All);

// Default pagination (page 1, size 10)
var spec = SpecBuilder<Product>.Create()
    .WithPagination(Pagination.Default);
```

## Distinct Operations

```csharp
// Get distinct products by brand
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .DistinctBy(p => p.Brand);

// Get distinct products by category and brand combination
var spec = SpecBuilder<Product>.Create()
    .DistinctBy(p => new { p.Category, p.Brand });
```

## Extension Methods

### IQueryable<T> Extensions

```csharp
// Apply filter only
IQueryable<T> Filter<T>(this IQueryable<T> values, ISpec<T> spec)

// Apply sort only
IQueryable<T> Sort<T>(this IQueryable<T> values, ISpec<T> spec)

// Apply pagination only
IQueryable<T> Paginate<T>(this IQueryable<T> values, ISpec<T> spec)
```

### IEnumerable<T> Extensions

```csharp
// Filter using compiled query
IEnumerable<T> Where<T>(this IEnumerable<T> values, ISpec<T> spec)

// Apply complete specification (convert to IQueryable first)
IQueryable<T> Specify<T>(this IEnumerable<T> values, ISpec<T> spec)
```

### Usage Examples

```csharp
var products = dbContext.Products;

// Apply only filter
var filtered = products.Filter(spec);

// Apply only sorting
var sorted = products.Sort(spec);

// Apply only pagination
var paginated = products.Paginate(spec);

// Apply complete specification
var result = products.Specify(spec);

// In-memory filtering
var memoryList = new List<Product> { /* ... */ };
var filtered = memoryList.Where(spec);  // Uses compiled predicate
```

## Query Execution Methods

### Prepare

Applies filter + sort + post-processing in sequence:

```csharp
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .Order(p => p.Name)
    .Take(10);

var query = spec.Prepare(dbContext.Products);
// Equivalent to:
// dbContext.Products
//     .Where(p => p.IsActive)
//     .OrderBy(p => p.Name)
//     .Take(10)
```

### Filtered

Applies only the filter predicate:

```csharp
var filtered = spec.Filtered(dbContext.Products);
// Only applies: .Where(predicate)
```

### Sorted

Applies only the sorting function:

```csharp
var sorted = spec.Sorted(dbContext.Products);
// Only applies: .OrderBy(...).ThenBy(...)
```

### Processed

Applies only post-processing (Skip/Take/Distinct):

```csharp
var processed = spec.Processed(dbContext.Products);
// Only applies: .Skip(...).Take(...).DistinctBy(...)
```

### SatisfyingItemFrom

Gets the first item that satisfies the specification:

```csharp
var product = spec.SatisfyingItemFrom(dbContext.Products);
// Equivalent to: Prepare(query).FirstOrDefault()
```

### SatisfyingItemsFrom

Gets all items that satisfy the specification:

```csharp
var products = spec.SatisfyingItemsFrom(dbContext.Products);
// Equivalent to: Prepare(query)
```

## In-Memory Validation

### IsSatisfiedBy

Check if an entity satisfies the specification without database query:

```csharp
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .And(p => p.Price > 100);

var product = new Product { IsActive = true, Price = 150 };

if (spec.IsSatisfiedBy(product)) {
    Console.WriteLine("Product matches specification");
}
```

**Note**: Throws `InvalidSpecificationException` if `Predicate` is null.

## Complete Example

```csharp
// Define reusable specifications
public static class OrderSpecifications {
    public static ISpec<Order> ForCustomer(this ISpec<Order> spec, Guid customerId) {
        return spec.And(o => o.CustomerId == customerId);
    }

    public static ISpec<Order> WithStatus(this ISpec<Order> spec, params OrderStatus[] statuses) {
        return spec.And(o => statuses.Contains(o.Status));
    }

    public static ISpec<Order> CreatedAfter(this ISpec<Order> spec, DateTime date) {
        return spec.And(o => o.CreatedAt >= date);
    }

    public static ISpec<Order> MinimumAmount(this ISpec<Order> spec, decimal amount) {
        return spec.And(o => o.TotalAmount >= amount);
    }

    public static ISpec<Order> RecentFirst(this ISpec<Order> spec) {
        return spec.OrderDescending(o => o.CreatedAt);
    }

    public static ISpec<Order> SearchByNumber(this ISpec<Order> spec, string orderNumber) {
        return spec.AndIf(
            !string.IsNullOrEmpty(orderNumber),
            o => o.OrderNumber.Contains(orderNumber));
    }
}

// Repository method
public class OrderRepository : IOrderRepository {
    private readonly DbContext _context;

    public async Task<IPaginated<Order>> SearchOrders(OrderSearchDto search) {
        var spec = SpecBuilder<Order>.Create()
            .ForCustomer(search.CustomerId)
            .WithStatus(OrderStatus.Pending, OrderStatus.Processing)
            .CreatedAfter(DateTime.UtcNow.AddMonths(-3))
            .MinimumAmount(search.MinAmount ?? 0)
            .SearchByNumber(search.OrderNumber)
            .RecentFirst()
            .WithPagination(search.Pagination);

        var orders = await _context.Orders
            .Specify(spec)
            .ToListAsync();

        var totalCount = await _context.Orders
            .Filter(spec)
            .CountAsync();

        return new Paginated<Order>(
            pageNumber: search.Pagination.PageNumber,
            pageSize: search.Pagination.PageSize,
            totalItems: totalCount,
            totalPages: (int)Math.Ceiling((double)totalCount / search.Pagination.PageSize),
            items: orders);
    }

    public async Task<bool> HasPendingOrders(Guid customerId) {
        var spec = SpecBuilder<Order>.Create()
            .ForCustomer(customerId)
            .WithStatus(OrderStatus.Pending);

        return await _context.Orders
            .Filter(spec)
            .AnyAsync();
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase {
    private readonly IOrderRepository _repository;

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] OrderSearchDto search) {
        var result = await _repository.SearchOrders(search);
        return Ok(result);
    }

    [HttpGet("validate/{id}")]
    public async Task<IActionResult> Validate(Guid id) {
        var order = await _repository.GetByIdAsync(id);

        if (order == null)
            return NotFound();

        var validSpec = SpecBuilder<Order>.Create()
            .WithStatus(OrderStatus.Pending, OrderStatus.Processing)
            .MinimumAmount(10);

        if (validSpec.IsSatisfiedBy(order)) {
            return Ok(new { message = "Order is valid for processing" });
        }

        return BadRequest(new { message = "Order does not meet requirements" });
    }
}
```

## Exceptions

### InvalidSpecificationException

Thrown when `Predicate` is null in `IsSatisfiedBy()`:

```csharp
[Serializable]
public sealed class InvalidSpecificationException : Exception
```

### SpecificationException

Thrown when errors occur during specification execution:

```csharp
public class SpecificationException : Exception
```

Thrown by:
- `Filtered()` - Error applying filter
- `Sorted()` - Error applying sort
- `Processed()` - Error applying post-processing

## Best Practices

1. **Create Extension Methods** - Define reusable specifications as extension methods on `ISpec<T>`
2. **Keep Specifications Simple** - Each specification should represent a single business rule
3. **Use Conditional Composition** - Use `AndIf`/`OrIf` for optional filters
4. **Separate Concerns** - Keep query logic separate from repository implementation
5. **Test Specifications** - Use `IsSatisfiedBy()` for unit testing specifications
6. **Use Pagination Value Objects** - Leverage `Myth.Commons.Pagination` for consistent paging
7. **Name Meaningfully** - Use descriptive names like `IsActive()`, `RecentFirst()`
8. **Avoid Complex Expressions** - Break complex filters into multiple specifications

## Integration with Myth.Repository

When used with Myth.Repository.EntityFramework:

```csharp
public interface IProductRepository : IReadRepositoryAsync<Product> {
    Task<IPaginated<Product>> SearchAsync(ProductSearchDto search);
}

public class ProductRepository : ReadRepositoryAsync<Product>, IProductRepository {
    public ProductRepository(DbContext context) : base(context) { }

    public async Task<IPaginated<Product>> SearchAsync(ProductSearchDto search) {
        var spec = SpecBuilder<Product>.Create()
            .And(p => p.IsActive)
            .AndIf(!string.IsNullOrEmpty(search.Category), p => p.Category == search.Category)
            .AndIf(search.MinPrice.HasValue, p => p.Price >= search.MinPrice.Value)
            .WithPagination(search.Pagination);

        // Use built-in repository method with specification
        return await GetPaginatedAsync(spec);
    }
}
```

## License

Licensed under the Apache License 2.0. See LICENSE file for details.
