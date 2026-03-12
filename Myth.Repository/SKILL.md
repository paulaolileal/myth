---
name: myth-repository
description: Use when defining repository contracts. Provides IReadRepositoryAsync<T>, IWriteRepositoryAsync<T>, and IReadWriteRepositoryAsync<T> interfaces with async-first, Specification-aware, and paginated operations. Interface-only library — concrete implementations are in Myth.Repository.EntityFramework.
---

# SKILL.md - Myth.Repository

**Version:** 1.0
**Target Framework:** .NET 8.0
**License:** Apache 2.0

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [API Reference](#api-reference)
- [Usage Examples](#usage-examples)
- [Best Practices](#best-practices)

---

## Overview

Myth.Repository provides standardized repository interfaces for .NET applications following the Repository Pattern. It promotes separation of concerns, supports CQRS through read/write segregation, integrates with the Specification pattern, and provides native pagination support.

### Key Features

- **Interface-Only Library**: Pure abstraction, no concrete implementations
- **Read/Write Segregation**: Separate interfaces for CQRS support
- **Specification Pattern Integration**: Full support for `ISpec<T>`
- **Expression-Based Queries**: LINQ expression support
- **Pagination**: Built-in `IPaginated<T>` support
- **Async-First**: All operations are asynchronous
- **IAsyncDisposable**: Proper resource management
- **Extension Methods**: Fluent pagination helpers

---

## Installation

```bash
dotnet add package Myth.Repository
```

### Dependencies
- .NET 8.0 or higher
- Myth.Commons (for pagination)
- Myth.Specification (for ISpec<T>)

---

## Core Concepts

### 1. Repository Pattern

Repositories abstract data access logic and provide a collection-like interface for domain entities:

```csharp
public interface IUserRepository : IReadWriteRepositoryAsync<User> {
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}
```

### 2. Read/Write Segregation

Separate interfaces for read and write operations enable CQRS:

- **IReadRepositoryAsync<T>**: Query operations
- **IWriteRepositoryAsync<T>**: Modification operations
- **IReadWriteRepositoryAsync<T>**: Combined interface

### 3. Specification Integration

Repositories support the Specification pattern for reusable query logic:

```csharp
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .And(u => u.Role == "Admin")
    .Order(u => u.Name)
    .Skip(20)
    .Take(10);

var users = await repository.SearchAsync(spec);
```

---

## API Reference

### IRepository (Marker Interface)

**Namespace:** `Myth.Interfaces.Repositories.Base`

```csharp
public interface IRepository { }
```

Empty marker interface used for:
- Identifying all repository types
- Auto-registration via DI scanning
- Type constraints

---

### IReadRepositoryAsync<TEntity>

**Namespace:** `Myth.Interfaces.Repositories.Base`

Repository interface for read operations.

#### Queryable Methods

```csharp
IQueryable<TEntity> Where(ISpec<TEntity> specification);
IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
IQueryable<TEntity> AsQueryable();
IEnumerable<TEntity> AsEnumerable();
```

**Examples:**
```csharp
// Get queryable for further composition
var query = repository.AsQueryable()
    .Include(u => u.Profile)
    .Where(u => u.IsActive);

// Filter with expression
var activeUsers = repository.Where(u => u.IsActive);

// Filter with specification
var spec = SpecBuilder<User>.Create().And(u => u.IsActive);
var query = repository.Where(spec);
```

---

#### Search Methods

```csharp
// With specification
Task<IEnumerable<TEntity>> SearchAsync(
    ISpec<TEntity> specification,
    CancellationToken cancellationToken = default);

// With expression
Task<IEnumerable<TEntity>> SearchAsync(
    Expression<Func<TEntity, bool>> filterPredicate,
    Expression<Func<TEntity, bool>>? orderPredicate = null,
    CancellationToken cancellationToken = default);
```

**Examples:**
```csharp
// Search with specification
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .Order(u => u.Name);
var users = await repository.SearchAsync(spec);

// Search with expressions
var users = await repository.SearchAsync(
    filterPredicate: u => u.IsActive,
    orderPredicate: u => u.CreatedAt);
```

---

#### Paginated Search Methods

```csharp
// With specification
Task<IPaginated<TEntity>> SearchPaginatedAsync(
    ISpec<TEntity> specification,
    CancellationToken cancellationToken = default);

// With expression and take/skip
Task<IPaginated<TEntity>> SearchPaginatedAsync(
    Expression<Func<TEntity, bool>> filterPredicate,
    int take = 0,
    int skip = 0,
    Expression<Func<TEntity, bool>>? orderPredicate = null,
    CancellationToken cancellationToken = default);

// With expression and Pagination object
Task<IPaginated<TEntity>> SearchPaginatedAsync(
    Expression<Func<TEntity, bool>> filterPredicate,
    Pagination pagination,
    Expression<Func<TEntity, bool>>? orderPredicate = null,
    CancellationToken cancellationToken = default);
```

**Examples:**
```csharp
// With specification
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .Order(u => u.Name)
    .Skip(20)
    .Take(10);
var page = await repository.SearchPaginatedAsync(spec);

// With take/skip
var page = await repository.SearchPaginatedAsync(
    filterPredicate: u => u.IsActive,
    take: 10,
    skip: 20,
    orderPredicate: u => u.Name);

// With Pagination object
var pagination = new Pagination(pageNumber: 3, pageSize: 10);
var page = await repository.SearchPaginatedAsync(
    filterPredicate: u => u.IsActive,
    pagination: pagination,
    orderPredicate: u => u.Name);
```

---

#### Single Item Retrieval

```csharp
// FirstOrDefault - returns null if not found
Task<TEntity?> FirstOrDefaultAsync(
    ISpec<TEntity> specification,
    CancellationToken cancellationToken = default);
Task<TEntity?> FirstOrDefaultAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default);

// First - throws InvalidOperationException if not found
Task<TEntity> FirstAsync(
    ISpec<TEntity> specification,
    CancellationToken cancellationToken = default);
Task<TEntity> FirstAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default);

// LastOrDefault - returns null if not found
Task<TEntity?> LastOrDefaultAsync(
    ISpec<TEntity> specification,
    CancellationToken cancellationToken = default);
Task<TEntity?> LastOrDefaultAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default);

// Last - throws InvalidOperationException if not found
Task<TEntity> LastAsync(
    ISpec<TEntity> specification,
    CancellationToken cancellationToken = default);
Task<TEntity> LastAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default);
```

**Examples:**
```csharp
// Safe retrieval
var user = await repository.FirstOrDefaultAsync(u => u.Email == email);
if (user == null) {
    // Handle not found
}

// With specification
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .Order(u => u.CreatedAt);
var latestUser = await repository.LastOrDefaultAsync(spec);
```

---

#### Aggregation Methods

```csharp
// Count items matching predicate
Task<int> CountAsync(
    ISpec<TEntity> specification,
    CancellationToken cancellationToken = default);
Task<int> CountAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default);

// Check if any items match predicate
Task<bool> AnyAsync(
    ISpec<TEntity> specification,
    CancellationToken cancellationToken = default);
Task<bool> AnyAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default);

// Check if all items match predicate
Task<bool> AllAsync(
    ISpec<TEntity> specification,
    CancellationToken cancellationToken = default);
Task<bool> AllAsync(
    Expression<Func<TEntity, bool>> predicate,
    CancellationToken cancellationToken = default);
```

**Examples:**
```csharp
// Count
var activeCount = await repository.CountAsync(u => u.IsActive);

// Any
var hasAdmins = await repository.AnyAsync(u => u.Role == "Admin");

// All
var allVerified = await repository.AllAsync(u => u.IsVerified);
```

---

#### Collection Retrieval

```csharp
Task<IEnumerable<TEntity>> ToListAsync(
    CancellationToken cancellationToken = default);
```

**Example:**
```csharp
var allUsers = await repository.ToListAsync();
```

---

### IWriteRepositoryAsync<TEntity>

**Namespace:** `Myth.Interfaces.Repositories.Base`

Repository interface for write operations.

#### Single Operations

```csharp
Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
```

**Examples:**
```csharp
// Add
var user = new User { Name = "John", Email = "john@example.com" };
await repository.AddAsync(user);

// Update
user.Name = "John Doe";
await repository.UpdateAsync(user);

// Remove
await repository.RemoveAsync(user);
```

---

#### Batch Operations

```csharp
Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
```

**Examples:**
```csharp
// Add multiple
var users = new List<User> {
    new() { Name = "User1", Email = "user1@example.com" },
    new() { Name = "User2", Email = "user2@example.com" }
};
await repository.AddRangeAsync(users);

// Update multiple
foreach (var user in users) {
    user.IsActive = true;
}
await repository.UpdateRangeAsync(users);

// Remove multiple
await repository.RemoveRangeAsync(users);
```

---

### IReadWriteRepositoryAsync<TEntity>

**Namespace:** `Myth.Interfaces.Repositories.Base`

```csharp
public interface IReadWriteRepositoryAsync<TEntity> :
    IReadRepositoryAsync<TEntity>,
    IWriteRepositoryAsync<TEntity>,
    IAsyncDisposable
{ }
```

Combines read and write interfaces. Most commonly used interface.

---

### Extension Methods

**Namespace:** `Myth.Extensions`

#### AsPaginated Extensions

```csharp
// With totalItems, take, skip
public static IPaginated<TEntity> AsPaginated<TEntity>(
    this IEnumerable<TEntity> items,
    int totalItems,
    int take = 0,
    int skip = 0);

// Auto-calculate totalItems
public static IPaginated<TEntity> AsPaginated<TEntity>(
    this IEnumerable<TEntity> items,
    int take = 0,
    int skip = 0);

// With Pagination object
public static IPaginated<TEntity> AsPaginated<TEntity>(
    this IEnumerable<TEntity> items,
    Pagination pagination);
```

**Examples:**
```csharp
var users = GetUsers();

// Manual pagination
var paginated = users.AsPaginated(totalItems: 100, take: 10, skip: 20);

// Auto-count
var paginated = users.AsPaginated(take: 10, skip: 0);

// With Pagination object
var pagination = new Pagination(pageNumber: 2, pageSize: 25);
var paginated = users.AsPaginated(pagination);

// Get all (no pagination)
var allPaginated = users.AsPaginated(Pagination.All);
```

---

## Usage Examples

### Example 1: Basic Repository Interface

```csharp
public interface IUserRepository : IReadWriteRepositoryAsync<User> {
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken ct = default);
}

public class UserRepository : IUserRepository {
    private readonly DbContext _context;

    public UserRepository(DbContext context) {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) {
        return await FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken ct = default) {
        return await SearchAsync(u => u.IsActive, u => u.Name, ct);
    }

    // IReadRepositoryAsync methods implementation...
    // IWriteRepositoryAsync methods implementation...
}
```

---

### Example 2: CQRS with Read/Write Segregation

```csharp
// Query side - read-only
public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IEnumerable<UserDto>> {
    private readonly IReadRepositoryAsync<User> _repository;

    public GetUsersQueryHandler(IReadRepositoryAsync<User> repository) {
        _repository = repository;
    }

    public async Task<QueryResult<IEnumerable<UserDto>>> HandleAsync(
        GetUsersQuery query,
        CancellationToken ct) {

        var spec = SpecBuilder<User>.Create()
            .And(u => u.IsActive)
            .AndIf(!string.IsNullOrEmpty(query.Search), u => u.Name.Contains(query.Search))
            .Order(u => u.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize);

        var users = await _repository.SearchAsync(spec, ct);
        var dtos = users.To<UserDto>();

        return QueryResult<IEnumerable<UserDto>>.Success(dtos);
    }
}

// Command side - write-only
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid> {
    private readonly IWriteRepositoryAsync<User> _repository;

    public CreateUserCommandHandler(IWriteRepositoryAsync<User> repository) {
        _repository = repository;
    }

    public async Task<CommandResult<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken ct) {

        var user = new User {
            Name = command.Name,
            Email = command.Email,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(user, ct);

        return CommandResult<Guid>.Success(user.Id);
    }
}
```

---

### Example 3: Pagination with Specifications

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    public async Task<IPaginated<Product>> GetProductsPageAsync(
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default) {

        var spec = SpecBuilder<Product>.Create()
            .And(p => p.IsActive)
            .AndIf(!string.IsNullOrEmpty(category), p => p.Category == category)
            .AndIf(minPrice.HasValue, p => p.Price >= minPrice!.Value)
            .AndIf(maxPrice.HasValue, p => p.Price <= maxPrice!.Value)
            .Order(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        return await _repository.SearchPaginatedAsync(spec, ct);
    }

    public async Task<IPaginated<Product>> GetAllProductsAsync(
        Pagination pagination,
        CancellationToken ct = default) {

        return await _repository.SearchPaginatedAsync(
            filterPredicate: p => p.IsActive,
            pagination: pagination,
            orderPredicate: p => p.CreatedAt,
            cancellationToken: ct);
    }
}
```

---

### Example 4: Custom Repository Methods

```csharp
public interface IOrderRepository : IReadWriteRepositoryAsync<Order> {
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken ct = default);
    Task<IPaginated<Order>> GetOrdersByCustomerAsync(
        Guid customerId,
        Pagination pagination,
        CancellationToken ct = default);
    Task<decimal> GetTotalSalesAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public class OrderRepository : IOrderRepository {
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default) {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken ct = default) {
        var spec = SpecBuilder<Order>.Create()
            .And(o => o.Status == OrderStatus.Pending)
            .Order(o => o.CreatedAt);

        return await SearchAsync(spec, ct);
    }

    public async Task<IPaginated<Order>> GetOrdersByCustomerAsync(
        Guid customerId,
        Pagination pagination,
        CancellationToken ct = default) {

        return await SearchPaginatedAsync(
            filterPredicate: o => o.CustomerId == customerId,
            pagination: pagination,
            orderPredicate: o => o.CreatedAt,
            cancellationToken: ct);
    }

    public async Task<decimal> GetTotalSalesAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct = default) {

        return await _context.Orders
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .Where(o => o.Status == OrderStatus.Completed)
            .SumAsync(o => o.Total, ct);
    }

    // Implement IReadRepositoryAsync and IWriteRepositoryAsync methods...
}
```

---

### Example 5: In-Memory Collection Pagination

```csharp
public class ReportService {
    public IPaginated<ReportItem> GenerateReport(
        List<Order> orders,
        int pageNumber,
        int pageSize) {

        var reportItems = orders
            .Select(o => new ReportItem {
                OrderId = o.Id,
                CustomerName = o.Customer.Name,
                Total = o.Total,
                Date = o.CreatedAt
            })
            .OrderByDescending(r => r.Date)
            .ToList();

        var pagination = new Pagination(pageNumber, pageSize);
        return reportItems.AsPaginated(pagination);
    }

    public IPaginated<T> PaginateList<T>(List<T> items, int page, int size) {
        var skip = (page - 1) * size;
        var pagedItems = items.Skip(skip).Take(size);

        return pagedItems.AsPaginated(
            totalItems: items.Count,
            take: size,
            skip: skip);
    }
}
```

---

## Best Practices

### 1. Always Create Specific Repository Interfaces — Never Inject Generic Interfaces Directly

**CRITICAL:** Always define a dedicated interface per entity that extends the base repository interface. **Never inject `IReadWriteRepositoryAsync<T>`, `IReadRepositoryAsync<T>`, or `IWriteRepositoryAsync<T>` directly** anywhere in the codebase — services, handlers, controllers, etc.

**✅ DO — Define specific interface and always use it:**
```csharp
// Define specific interface per entity
public interface IUserRepository : IReadWriteRepositoryAsync<User> {
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}

// Implement it
public class UserRepository : ReadWriteRepositoryAsync<User>, IUserRepository {
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) {
        return await FirstOrDefaultAsync(u => u.Email == email, ct);
    }
}

// Inject and use the specific interface throughout the codebase
public class UserService {
    private readonly IUserRepository _userRepository; // ✅ specific interface

    public UserService(IUserRepository userRepository) {
        _userRepository = userRepository;
    }
}

public class GetUserHandler : IQueryHandler<GetUserQuery, UserDto> {
    private readonly IUserRepository _userRepository; // ✅ specific interface

    public GetUserHandler(IUserRepository userRepository) {
        _userRepository = userRepository;
    }
}
```

**❌ DON'T — Inject generic interfaces directly:**
```csharp
// NEVER inject the generic base interface directly
public class UserService {
    private readonly IReadWriteRepositoryAsync<User> _repository; // ❌ too generic

    public UserService(IReadWriteRepositoryAsync<User> repository) { // ❌ wrong
        _repository = repository;
    }
}

public class GetUserHandler : IQueryHandler<GetUserQuery, UserDto> {
    private readonly IReadRepositoryAsync<User> _repository; // ❌ too generic

    public GetUserHandler(IReadRepositoryAsync<User> repository) { // ❌ wrong
        _repository = repository;
    }
}

// Also don't use a generic-only repository
public class GenericRepository<T> : IReadWriteRepositoryAsync<T> { } // ❌ no entity-specific interface
```

**Why this matters:**
- **Discoverability**: A dedicated `IUserRepository` makes it immediately clear what data access operations exist for that entity
- **Extensibility**: You can easily add entity-specific methods without changing base interfaces
- **Mockability**: Mocking `IUserRepository` in tests is trivial and strongly typed
- **Separation of concerns**: Each repository encapsulates all data-access logic for its entity
- **CQRS support**: You can still inject just `IReadRepositoryAsync<T>` side if needed, but via a narrower specific interface:

```csharp
// For strict CQRS separation, define read/write interfaces explicitly
public interface IUserReadRepository : IReadRepositoryAsync<User> {
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}

public interface IUserWriteRepository : IWriteRepositoryAsync<User> { }

public interface IUserRepository : IUserReadRepository, IUserWriteRepository { }
```

### 2. Leverage Specifications for Complex Queries

**✅ DO:**
```csharp
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .And(u => u.Role == "Admin")
    .Order(u => u.Name)
    .Skip(offset)
    .Take(limit);

var users = await repository.SearchAsync(spec);
```

**❌ DON'T:**
```csharp
// Scattered query logic
var users = await repository
    .AsQueryable()
    .Where(u => u.IsActive)
    .Where(u => u.Role == "Admin")
    .OrderBy(u => u.Name)
    .Skip(offset)
    .Take(limit)
    .ToListAsync();
```

### 3. Separate Read and Write for CQRS

**✅ DO:**
```csharp
public class QueryHandler {
    private readonly IReadRepositoryAsync<User> _repository;
}

public class CommandHandler {
    private readonly IWriteRepositoryAsync<User> _repository;
}
```

### 4. Use Pagination for Large Result Sets

**✅ DO:**
```csharp
var page = await repository.SearchPaginatedAsync(
    filterPredicate: u => u.IsActive,
    pagination: new Pagination(pageNumber, pageSize));
```

**❌ DON'T:**
```csharp
// Load all into memory
var allUsers = await repository.ToListAsync();
var page = allUsers.Skip(skip).Take(take);
```

### 5. Always Use CancellationToken

**✅ DO:**
```csharp
public async Task<User?> GetUserAsync(Guid id, CancellationToken ct) {
    return await _repository.FirstOrDefaultAsync(u => u.Id == id, ct);
}
```

### 6. Keep Repositories Lean

**✅ DO:**
```csharp
public interface IOrderRepository : IReadWriteRepositoryAsync<Order> {
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetPendingAsync(CancellationToken ct = default);
}
```

**❌ DON'T:**
```csharp
// Business logic in repository
public interface IOrderRepository : IReadWriteRepositoryAsync<Order> {
    Task<decimal> CalculateDiscountAsync(Order order);  // Should be in service
    Task SendConfirmationEmailAsync(Order order);        // Should be in service
}
```

---

## Integration with Myth Ecosystem

### With Myth.Specification

```csharp
var spec = SpecBuilder<Product>.Create()
    .And(p => p.IsActive)
    .And(p => p.Price > 100)
    .Order(p => p.Name)
    .Skip(20)
    .Take(10);

var products = await repository.SearchAsync(spec);
var paginated = await repository.SearchPaginatedAsync(spec);
```

### With Myth.Commons

```csharp
// Use Pagination value object
var pagination = Pagination.Default;  // Page 1, Size 10
var results = await repository.SearchPaginatedAsync(
    filterPredicate: p => p.IsActive,
    pagination: pagination);

// IPaginated<T> result
Console.WriteLine($"Page {results.PageNumber} of {results.TotalPages}");
Console.WriteLine($"Total items: {results.TotalItems}");
```

### With Myth.Repository.EntityFramework

```csharp
// Use concrete implementation
services.AddScoped<IUserRepository, UserRepository>();

// Or auto-register
services.AddRepositories();
```

---

## Summary

Myth.Repository provides:

- ✅ **Interface-Only**: Pure abstraction layer
- ✅ **Read/Write Segregation**: CQRS support
- ✅ **Specification Integration**: Reusable query logic
- ✅ **Expression-Based**: LINQ support
- ✅ **Pagination**: Built-in pagination
- ✅ **Async-First**: Full async/await
- ✅ **Extension Methods**: Fluent helpers
- ✅ **Type-Safe**: Compile-time checking

---

## Additional Resources

- **Repository**: https://gitlab.com/dotnet-myth/myth
- **License**: Apache 2.0
- **Target Framework**: .NET 8.0
- **NuGet Package**: Myth.Repository
- **Implementation**: Myth.Repository.EntityFramework

---

*This documentation is maintained for AI agents and developers. For questions or contributions, please refer to the repository.*
