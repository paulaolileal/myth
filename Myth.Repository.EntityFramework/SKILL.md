---
name: myth-repository-entityframework
description: Use when you need EF Core repository implementations. Provides ReadRepositoryAsync<T>, WriteRepositoryAsync<T>, and ReadWriteRepositoryAsync<T> base classes, plus BaseContext (auto-discovers IEntityTypeConfiguration<T>), IUnitOfWorkRepository for transactions/savepoints, and AddRepositories() for automatic DI registration.
---

# SKILL.md - Myth.Repository.EntityFramework

**Version:** 1.0
**Target Framework:** .NET 10.0
**License:** Apache 2.0

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
- [Usage Examples](#usage-examples)
- [Advanced Features](#advanced-features)
- [Best Practices](#best-practices)

---

## Overview

Myth.Repository.EntityFramework provides complete, production-ready implementations of the Repository Pattern for Entity Framework Core. It eliminates approximately 90% of boilerplate code while maintaining flexibility and type safety.

### Key Features

- **Complete Repository Implementation**: Ready-to-use base classes
- **Auto-Configuration**: Automatic entity configuration discovery
- **Unit of Work Pattern**: Transaction management with savepoints
- **Auto-Registration**: Automatic DI registration of repositories
- **Raw SQL Support**: Execute custom SQL queries
- **Specification Integration**: Full Myth.Specification support
- **Provider Detection**: Query database provider information
- **Entity State Management**: Attach/Detach operations

---

## Installation

```bash
dotnet add package Myth.Repository.EntityFramework
```

### Dependencies
- .NET 10.0
- Microsoft.EntityFrameworkCore (10.0.3+)
- Microsoft.EntityFrameworkCore.Relational (10.0.3+)
- Myth.Repository
- Myth.Specification
- Myth.DependencyInjection

---

## Quick Start

### 1. Define DbContext

```csharp
public class ApplicationDbContext : BaseContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
}
```

**Note:** `BaseContext` automatically discovers and applies all `IEntityTypeConfiguration<T>` from the assembly.

### 2. Define Entity Configuration

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User> {
    public void Configure(EntityTypeBuilder<User> builder) {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(254);
        builder.HasIndex(u => u.Email).IsUnique();
    }
}
```

### 3. Create Repository Interface

```csharp
public interface IUserRepository : IReadWriteRepositoryAsync<User> {
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}
```

### 4. Implement Repository

```csharp
public class UserRepository : ReadWriteRepositoryAsync<User>, IUserRepository {
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) {
        return await FirstOrDefaultAsync(u => u.Email == email, ct);
    }
}
```

### 5. Register in DI

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Auto-register all repositories
builder.Services.AddRepositories();

// Register Unit of Work
builder.Services.AddUnitOfWorkForContext<ApplicationDbContext>();

var app = builder.BuildApp();
app.Run();
```

---

## API Reference

### BaseContext

**Namespace:** `Myth.Contexts`

Abstract base class for DbContext with automatic configuration discovery.

```csharp
public abstract class BaseContext : DbContext {
    protected BaseContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // Automatically applies all IEntityTypeConfiguration<T> from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}
```

**Benefits:**
- No manual `modelBuilder.ApplyConfiguration()` calls needed
- Discovers all entity configurations automatically
- Convention over configuration

---

### Extended Repository Interfaces

#### IReadRepositoryAsync<TEntity> (EF)

**Namespace:** `Myth.Interfaces.Repositories.EntityFramework`

Extends base read repository with EF-specific features.

```csharp
public interface IReadRepositoryAsync<TEntity> : Base.IReadRepositoryAsync<TEntity> {
    string? GetProviderName();
}
```

**Additional Method:**
- `GetProviderName()`: Returns database provider name (e.g., "Microsoft.EntityFrameworkCore.SqlServer")

---

#### IWriteRepositoryAsync<TEntity> (EF)

**Namespace:** `Myth.Interfaces.Repositories.EntityFramework`

Extends base write repository with entity state management.

```csharp
public interface IWriteRepositoryAsync<TEntity> : Base.IWriteRepositoryAsync<TEntity> {
    Task AttachAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AttachRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
```

**Additional Methods:**
- `AttachAsync()`: Reattach detached entity to context
- `AttachRangeAsync()`: Reattach multiple detached entities

---

### IUnitOfWorkRepository

**Namespace:** `Myth.Interfaces.Repositories.EntityFramework`

Interface for transaction management and persistence.

```csharp
public interface IUnitOfWorkRepository : IAsyncDisposable {
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);

    Task CreateSavepointAsync(string savepointName, CancellationToken cancellationToken = default);
    Task RollbackToSavepointAsync(string savepointName, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<int> ExecuteSqlAsync(
        string query,
        IEnumerable<object>? parameters = null,
        CancellationToken cancellationToken = default);
}
```

> **InMemory provider:** `BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`, `CreateSavepointAsync`, and `RollbackToSavepointAsync` are all **no-ops** when the EF Core InMemory provider is used (e.g., in unit tests with `BaseDatabaseTests`). The call succeeds silently without starting or committing a real transaction. This means transactional rollback semantics cannot be tested with InMemory — use a real database provider (SQLite in-memory mode or SQL Server LocalDB) when testing transaction behavior.

---

### Implementation Classes

#### ReadRepositoryAsync<TEntity>

**Namespace:** `Myth.Repositories.EntityFramework`

Complete implementation of read operations.

```csharp
public partial class ReadRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity>
    where TEntity : class
{
    protected readonly BaseContext _context;

    public ReadRepositoryAsync(BaseContext context) {
        _context = context;
    }

    // All IReadRepositoryAsync methods implemented
}
```

**Features:**
- Full LINQ support via AsQueryable()
- Specification pattern support
- Expression-based queries
- Pagination
- Aggregations
- Async operations

---

#### WriteRepositoryAsync<TEntity>

**Namespace:** `Myth.Repositories.EntityFramework`

Complete implementation of write operations.

```csharp
public class WriteRepositoryAsync<T> : IWriteRepositoryAsync<T>
    where T : class
{
    private readonly BaseContext _context;

    public WriteRepositoryAsync(BaseContext context) {
        _context = context;
    }

    // All IWriteRepositoryAsync methods implemented
}
```

**Features:**
- Add/Update/Remove operations
- Batch operations
- Attach/Detach management
- Change tracking optimization

---

#### ReadWriteRepositoryAsync<TEntity>

**Namespace:** `Myth.Repositories.EntityFramework`

Combined read/write repository using composition.

```csharp
public abstract class ReadWriteRepositoryAsync<TEntity> :
    IReadWriteRepositoryAsync<TEntity>
    where TEntity : class
{
    protected readonly BaseContext _context;
    private readonly IReadRepositoryAsync<TEntity> _readRepository;
    private readonly IWriteRepositoryAsync<TEntity> _writeRepository;

    protected ReadWriteRepositoryAsync(BaseContext context) {
        _context = context;
        _readRepository = new ReadRepositoryAsync<TEntity>(context);
        _writeRepository = new WriteRepositoryAsync<TEntity>(context);
    }

    // Delegates to specialized repositories
}
```

**Pattern:** Composition over inheritance for read/write operations.

---

#### BaseUnitOfWorkRepository

**Namespace:** `Myth.Repositories.EntityFramework.Base`

Abstract base class for Unit of Work pattern.

```csharp
public abstract class BaseUnitOfWorkRepository : IUnitOfWorkRepository
{
    protected readonly BaseContext _context;
    private IDbContextTransaction? _transaction;

    protected BaseUnitOfWorkRepository(BaseContext context) {
        _context = context;
    }

    // Transaction management implementation
}
```

**Features:**
- Transaction lifecycle management
- Savepoint support
- SaveChanges coordination
- Raw SQL execution
- Proper async disposal

---

### Extension Methods

**Namespace:** `Myth.Extensions`

#### AddUnitOfWork<TUnitOfWork>

```csharp
public static IServiceCollection AddUnitOfWork<TUnitOfWork>(
    this IServiceCollection services)
    where TUnitOfWork : BaseUnitOfWorkRepository
```

**Usage:**
```csharp
services.AddUnitOfWork<CustomUnitOfWork>();
```

---

#### AddUnitOfWorkForContext<TContext>

```csharp
public static IServiceCollection AddUnitOfWorkForContext<TContext>(
    this IServiceCollection services)
    where TContext : BaseContext
```

**Usage:**
```csharp
services.AddUnitOfWorkForContext<ApplicationDbContext>();
```

---

#### AddRepositories

```csharp
public static IServiceCollection AddRepositories(
    this IServiceCollection services,
    ServiceLifetime lifetime = ServiceLifetime.Scoped)
```

**Features:**
- Auto-discovers all repository implementations
- Registers with appropriate interface
- Filters test types automatically
- Customizable service lifetime

**Usage:**
```csharp
services.AddRepositories();
services.AddRepositories(ServiceLifetime.Transient);
```

---

#### AddRepositoriesFromAssembly

```csharp
public static IServiceCollection AddRepositoriesFromAssembly(
    this IServiceCollection services,
    Assembly assembly,
    ServiceLifetime lifetime = ServiceLifetime.Scoped)
```

**Usage:**
```csharp
services.AddRepositoriesFromAssembly(typeof(UserRepository).Assembly);
```

---

#### AddRepository<TInterface, TImplementation>

```csharp
public static IServiceCollection AddRepository<TInterface, TImplementation>(
    this IServiceCollection services,
    ServiceLifetime lifetime = ServiceLifetime.Scoped)
    where TInterface : class, IRepository
    where TImplementation : class, TInterface
```

**Usage:**
```csharp
services.AddRepository<IUserRepository, UserRepository>();
```

---

## Usage Examples

### Example 1: Complete CRUD Repository

```csharp
// Entity
public class Product {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Configuration
public class ProductConfiguration : IEntityTypeConfiguration<Product> {
    public void Configure(EntityTypeBuilder<Product> builder) {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.HasIndex(p => p.Name);
    }
}

// Repository Interface
public interface IProductRepository : IReadWriteRepositoryAsync<Product> {
    Task<Product?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetInStockAsync(CancellationToken ct = default);
    Task<IPaginated<Product>> SearchProductsAsync(
        string? search,
        decimal? minPrice,
        decimal? maxPrice,
        Pagination pagination,
        CancellationToken ct = default);
}

// Repository Implementation
public class ProductRepository : ReadWriteRepositoryAsync<Product>, IProductRepository {
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Product?> GetByNameAsync(string name, CancellationToken ct = default) {
        return await FirstOrDefaultAsync(p => p.Name == name, ct);
    }

    public async Task<IReadOnlyList<Product>> GetInStockAsync(CancellationToken ct = default) {
        return await SearchAsync(p => p.IsActive && p.Stock > 0, p => p.Name, ct);
    }

    public async Task<IPaginated<Product>> SearchProductsAsync(
        string? search,
        decimal? minPrice,
        decimal? maxPrice,
        Pagination pagination,
        CancellationToken ct = default) {

        var spec = SpecBuilder<Product>.Create()
            .And(p => p.IsActive)
            .AndIf(!string.IsNullOrEmpty(search), p => p.Name.Contains(search!))
            .AndIf(minPrice.HasValue, p => p.Price >= minPrice!.Value)
            .AndIf(maxPrice.HasValue, p => p.Price <= maxPrice!.Value)
            .Order(p => p.Name)
            .WithPagination(pagination);

        return await SearchPaginatedAsync(spec, ct);
    }
}
```

---

### Example 2: Unit of Work with Transactions

```csharp
public class OrderService {
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWorkRepository _unitOfWork;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IUnitOfWorkRepository unitOfWork,
        ILogger<OrderService> logger) {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Order>> CreateOrderAsync(
        CreateOrderCommand command,
        CancellationToken ct = default) {

        await _unitOfWork.BeginTransactionAsync(ct);

        try {
            // Create order
            var order = new Order {
                Id = Guid.NewGuid(),
                CustomerId = command.CustomerId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            await _orderRepository.AddAsync(order, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            // Create savepoint after order creation
            await _unitOfWork.CreateSavepointAsync("AfterOrderCreation", ct);

            // Process each item
            foreach (var item in command.Items) {
                // Check inventory
                var inventory = await _inventoryRepository.FirstOrDefaultAsync(
                    i => i.ProductId == item.ProductId,
                    ct);

                if (inventory == null || inventory.Quantity < item.Quantity) {
                    // Rollback to savepoint
                    await _unitOfWork.RollbackToSavepointAsync("AfterOrderCreation", ct);

                    _logger.LogWarning(
                        "Insufficient inventory for product {ProductId}",
                        item.ProductId);

                    return Result<Order>.Failure("Insufficient inventory");
                }

                // Update inventory
                inventory.Quantity -= item.Quantity;
                await _inventoryRepository.UpdateAsync(inventory, ct);

                // Add order item
                var orderItem = new OrderItem {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = inventory.Price
                };

                await _orderItemRepository.AddAsync(orderItem, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation("Order {OrderId} created successfully", order.Id);

            return Result<Order>.Success(order);
        }
        catch (Exception ex) {
            await _unitOfWork.RollbackAsync(ct);

            _logger.LogError(ex, "Error creating order");

            return Result<Order>.Failure("Failed to create order", ex);
        }
    }
}
```

---

### Example 3: Raw SQL Execution

```csharp
public class ReportService {
    private readonly IUnitOfWorkRepository _unitOfWork;

    public async Task<IEnumerable<SalesReport>> GetSalesReportAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct = default) {

        var query = @"
            SELECT
                p.Id,
                p.Name,
                SUM(oi.Quantity) as TotalQuantity,
                SUM(oi.Quantity * oi.Price) as TotalRevenue
            FROM Products p
            INNER JOIN OrderItems oi ON p.Id = oi.ProductId
            INNER JOIN Orders o ON oi.OrderId = o.Id
            WHERE o.CreatedAt >= @From AND o.CreatedAt <= @To
            GROUP BY p.Id, p.Name
            ORDER BY TotalRevenue DESC";

        var parameters = new object[] {
            new SqlParameter("@From", from),
            new SqlParameter("@To", to)
        };

        await _unitOfWork.ExecuteSqlAsync(query, parameters, ct);

        // Note: For SELECT queries, use DbContext directly or add custom method
    }

    public async Task<int> BulkUpdatePricesAsync(
        decimal increasePercentage,
        CancellationToken ct = default) {

        var query = @"
            UPDATE Products
            SET Price = Price * (1 + @IncreasePercentage / 100)
            WHERE IsActive = 1";

        var parameters = new object[] {
            new SqlParameter("@IncreasePercentage", increasePercentage)
        };

        return await _unitOfWork.ExecuteSqlAsync(query, parameters, ct);
    }
}
```

---

### Example 4: Entity State Management

```csharp
public class UserService {
    private readonly IUserRepository _repository;

    public async Task<User> UpdateDetachedUserAsync(
        User detachedUser,
        CancellationToken ct = default) {

        // Reattach detached entity
        await _repository.AttachAsync(detachedUser, ct);

        // Now can update
        detachedUser.ModifiedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(detachedUser, ct);

        return detachedUser;
    }

    public async Task BulkUpdateUsersAsync(
        List<User> detachedUsers,
        CancellationToken ct = default) {

        // Reattach all detached entities
        await _repository.AttachRangeAsync(detachedUsers, ct);

        // Bulk update
        foreach (var user in detachedUsers) {
            user.ModifiedAt = DateTime.UtcNow;
        }

        await _repository.UpdateRangeAsync(detachedUsers, ct);
    }
}
```

---

### Example 5: Provider Detection

```csharp
public class DiagnosticsService {
    private readonly IUserRepository _repository;

    public string GetDatabaseInfo() {
        var providerName = _repository.GetProviderName();

        return providerName switch {
            "Microsoft.EntityFrameworkCore.SqlServer" => "SQL Server",
            "Microsoft.EntityFrameworkCore.Sqlite" => "SQLite",
            "Microsoft.EntityFrameworkCore.InMemory" => "In-Memory",
            "Npgsql.EntityFrameworkCore.PostgreSQL" => "PostgreSQL",
            _ => "Unknown"
        };
    }
}
```

---

## Advanced Features

### 1. Custom Base Repository

```csharp
public abstract class AuditableRepository<TEntity> : ReadWriteRepositoryAsync<TEntity>
    where TEntity : class, IAuditable
{
    protected AuditableRepository(BaseContext context) : base(context) { }

    public override async Task AddAsync(TEntity entity, CancellationToken ct = default) {
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = GetCurrentUserId();
        await base.AddAsync(entity, ct);
    }

    public override async Task UpdateAsync(TEntity entity, CancellationToken ct = default) {
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedBy = GetCurrentUserId();
        await base.UpdateAsync(entity, ct);
    }

    private string GetCurrentUserId() {
        // Get from HttpContext or similar
        return "current-user-id";
    }
}
```

### 2. Soft Delete Pattern

```csharp
public abstract class SoftDeleteRepository<TEntity> : ReadWriteRepositoryAsync<TEntity>
    where TEntity : class, ISoftDeletable
{
    protected SoftDeleteRepository(BaseContext context) : base(context) { }

    public override Task RemoveAsync(TEntity entity, CancellationToken ct = default) {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        return UpdateAsync(entity, ct);
    }

    public override Task<IReadOnlyList<TEntity>> SearchAsync(
        Expression<Func<TEntity, bool>> filterPredicate,
        Expression<Func<TEntity, bool>>? orderPredicate = null,
        CancellationToken ct = default) {

        // Automatically exclude deleted entities
        var combinedFilter = CombineWithNotDeleted(filterPredicate);
        return base.SearchAsync(combinedFilter, orderPredicate, ct);
    }

    private Expression<Func<TEntity, bool>> CombineWithNotDeleted(
        Expression<Func<TEntity, bool>> filter) {

        Expression<Func<TEntity, bool>> notDeleted = e => !e.IsDeleted;
        return filter.And(notDeleted);
    }
}
```

---

## Change Tracking

### SearchAsync vs SearchAsNoTrackingAsync

`SearchAsync` returns a **change-tracked** `IReadOnlyList<TEntity>`. Entities returned are registered in the EF Core change tracker, so any property modifications will be automatically detected and persisted on the next `SaveChangesAsync` call — no need to call `UpdateAsync`.

```csharp
// ✅ CORRECT: modify entities from SearchAsync and save directly
var orders = await _orderRepository.SearchAsync(spec, ct);
foreach (var order in orders)
    order.Status = OrderStatus.Shipped;

await _unitOfWork.SaveChangesAsync(ct); // EF Core detects all changes
```

`SearchAsNoTrackingAsync` returns entities **without registering them** in the change tracker. Use it for read-only scenarios (reports, projections, API list endpoints) where you will NOT modify the returned entities. It avoids the memory and CPU overhead of change tracking.

```csharp
// ✅ CORRECT: read-only projection — use AsNoTracking
var products = await _productRepository.SearchAsNoTrackingAsync(
    p => p.IsActive && p.Price < 100, ct);

return products.Select(p => new ProductDto(p.Id, p.Name, p.Price)).ToList();
```

| | `SearchAsync` | `SearchAsNoTrackingAsync` |
|---|---|---|
| Change tracking | ✅ Active | ❌ Disabled |
| Modifiable without `UpdateAsync` | ✅ Yes | ❌ No |
| Best for | Writes, soft-delete, in-place updates | Reports, projections, list endpoints |
| Memory overhead | Higher (tracking graph) | Lower |

### IReadOnlyList<T> and the .Count Pitfall

`SearchAsync` returns `IReadOnlyList<TEntity>` (not `IEnumerable<TEntity>`). The result is fully materialized in memory. Use the `.Count` **property** — it is O(1):

```csharp
var results = await repo.SearchAsync(spec, ct);

// ✅ CORRECT — IReadOnlyList.Count property, O(1)
var total = results.Count;

// ✅ Also correct but unnecessary — LINQ extension, O(n)
var total = results.Count();
```

> **Note:** If your repository exposes a custom method returning `IEnumerable<T>`, the `.Count` property always returns `1` (the count of the `IEnumerable` wrapper object, not its elements). Prefer returning `IReadOnlyList<T>` in your own methods too.

---

## Best Practices

### 1. Use BaseContext for Auto-Configuration

**✅ DO:**
```csharp
public class ApplicationDbContext : BaseContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
}
```

**❌ DON'T:**
```csharp
public class ApplicationDbContext : DbContext {
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // Manual registration of each configuration
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        // ...
    }
}
```

---

### 1.5. Never Use DbSets Directly - Always Use Repositories

**CRITICAL:** This is a fundamental principle of the Repository Pattern. **Never access DbSets directly** from DbContext. All database operations should go through repositories.

**✅ DO:**
```csharp
// In DbContext - DbSets are internal implementation detail
public class ApplicationDbContext : BaseContext {
    // DbSets exist for EF Core's internal use only
    internal DbSet<User> Users { get; set; }
    internal DbSet<Product> Products { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }
}

// In Service/Controller - Always use repositories
public class UserService {
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository) {
        _userRepository = userRepository;
    }

    public async Task<User> GetUserAsync(Guid id) {
        // ✅ Use repository
        return await _userRepository.GetByIdAsync(id);
    }
}
```

**❌ DON'T:**
```csharp
// NEVER do this - bypasses repository pattern
public class UserService {
    private readonly ApplicationDbContext _context;

    public async Task<User> GetUserAsync(Guid id) {
        // ❌ Direct DbSet access - AVOID AT ALL COSTS
        return await _context.Users.FindAsync(id);
    }

    public async Task<List<User>> GetActiveUsersAsync() {
        // ❌ Direct LINQ on DbSet - WRONG
        return await _context.Users
            .Where(u => u.IsActive)
            .ToListAsync();
    }
}
```

**Why This Matters:**
- **Separation of Concerns**: Data access logic belongs in repositories
- **Testability**: Repositories can be mocked; DbContext cannot be easily mocked
- **Consistency**: All database operations follow the same pattern
- **Business Logic Encapsulation**: Complex queries and specifications live in repositories
- **Change Management**: Database changes only affect repositories, not entire application
- **Single Responsibility**: Services focus on business logic, repositories handle data access

**Correct Architecture:**
```
Controller/Service → Repository Interface → Repository Implementation → DbContext → Database
                          ↑
                    (inject this)
```

**Incorrect Architecture:**
```
Controller/Service → DbContext → Database
                       ↑
                  (never inject this into services!)
```

### 1.7. Always Create Specific Repository Interfaces — Never Inject Generic Interfaces Directly

**CRITICAL:** Every entity must have its own dedicated repository interface that extends the base. **Never inject `IReadWriteRepositoryAsync<T>`, `IReadRepositoryAsync<T>`, or `IWriteRepositoryAsync<T>` directly** in services, handlers, or controllers. Always define and inject the entity-specific interface.

**✅ DO:**
```csharp
// 1. Define specific interface
public interface IProductRepository : IReadWriteRepositoryAsync<Product> {
    Task<Product?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetInStockAsync(CancellationToken ct = default);
}

// 2. Implement it inheriting from ReadWriteRepositoryAsync
public class ProductRepository : ReadWriteRepositoryAsync<Product>, IProductRepository {
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Product?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await FirstOrDefaultAsync(p => p.Name == name, ct);

    public async Task<IReadOnlyList<Product>> GetInStockAsync(CancellationToken ct = default) =>
        await SearchAsync(p => p.IsActive && p.Stock > 0, p => p.Name, ct);
}

// 3. Inject and use IProductRepository everywhere — never the generic base
public class ProductService {
    private readonly IProductRepository _productRepository; // ✅

    public ProductService(IProductRepository productRepository) {
        _productRepository = productRepository;
    }
}

public class GetProductHandler : IQueryHandler<GetProductQuery, ProductDto> {
    private readonly IProductRepository _productRepository; // ✅

    public GetProductHandler(IProductRepository productRepository) {
        _productRepository = productRepository;
    }
}
```

**❌ DON'T:**
```csharp
// NEVER inject the generic base interface directly anywhere
public class ProductService {
    private readonly IReadWriteRepositoryAsync<Product> _repository; // ❌

    public ProductService(IReadWriteRepositoryAsync<Product> repository) { // ❌
        _repository = repository;
    }
}

public class GetProductHandler {
    private readonly IReadRepositoryAsync<Product> _repository; // ❌

    public GetProductHandler(IReadRepositoryAsync<Product> repository) { // ❌
        _repository = repository;
    }
}
```

**Rule of thumb:** if you find `IReadWriteRepositoryAsync<SomeEntity>` or similar generic interfaces being injected anywhere outside of the repository implementation itself, that's a bug in the architecture — replace it with the entity-specific `ISomeEntityRepository`.

---

### 2. Use Auto-Registration

**✅ DO:**
```csharp
services.AddRepositories();
```

**❌ DON'T:**
```csharp
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();
// ... manual registration for every repository
```

### 3. Use Unit of Work for Multi-Repository Operations

**✅ DO:**
```csharp
await _unitOfWork.BeginTransactionAsync();
await _orderRepository.AddAsync(order);
await _inventoryRepository.UpdateAsync(inventory);
await _unitOfWork.SaveChangesAsync();
await _unitOfWork.CommitAsync();
```

### 4. Always Use CancellationToken

**✅ DO:**
```csharp
public async Task<User?> GetUserAsync(Guid id, CancellationToken ct) {
    return await _repository.FirstOrDefaultAsync(u => u.Id == id, ct);
}
```

### 5. Use Savepoints for Complex Transactions

**✅ DO:**
```csharp
await _unitOfWork.BeginTransactionAsync();
await CreateOrderAsync(order);
await _unitOfWork.CreateSavepointAsync("AfterOrder");
try {
    await ProcessPaymentAsync(order);
} catch {
    await _unitOfWork.RollbackToSavepointAsync("AfterOrder");
    throw;
}
await _unitOfWork.CommitAsync();
```

### 6. Use Specifications with Include for Eager Loading

**✅ DO:**
```csharp
// Define spec with includes
var spec = SpecBuilder<Project>
    .Create()
    .HasId(projectId)
    .IsActive()
    .Include(q => q.Include(p => p.Members)
                   .ThenInclude(m => m.User));

// Repository automatically applies includes
var project = await _projectRepository.FirstOrDefaultAsync(spec, ct);
```

**❌ DON'T:**
```csharp
// Bypasses repository abstraction and leaks EF Core concerns
var project = await _projectRepository
    .Where(spec)
    .Include(p => p.Members)
    .ThenInclude(m => m.User)
    .FirstOrDefaultAsync(ct);
```

**Why?**
- Keeps eager loading logic **inside the specification**, not in the caller
- Repository methods like `FirstOrDefaultAsync`, `SearchAsync`, etc. automatically apply includes from the spec
- Maintains clean separation: specifications define **what to load**, repositories define **how to access**
- Better for testing: mocks don't need to handle EF Core's `Include` extensions

**Complete Example:**
```csharp
// Specification extensions with includes
public static class ProjectSpecifications {
    public static ISpec<Project> WithMembers(this ISpec<Project> spec) =>
        spec.Include(q => q.Include(p => p.Members));

    public static ISpec<Project> WithFullDetails(this ISpec<Project> spec) =>
        spec.Include(q => q.Include(p => p.Members)
                           .ThenInclude(m => m.User)
                           .Include(p => p.Tasks)
                           .Include(p => p.Owner));
}

// Usage in service/handler
public class GetProjectHandler : IQueryHandler<GetProjectQuery, ProjectDto> {
    private readonly IProjectRepository _repository;

    public async Task<QueryResult<ProjectDto>> HandleAsync(
        GetProjectQuery query,
        CancellationToken ct) {

        var spec = SpecBuilder<Project>
            .Create()
            .HasId(query.ProjectId)
            .IsActive()
            .WithFullDetails();  // Include is part of the spec

        var project = await _repository.FirstOrDefaultAsync(spec, ct);
        return QueryResult<ProjectDto>.Success(project.ToDto());
    }
}
```

---

## Summary

Myth.Repository.EntityFramework provides:

- ✅ **Complete Implementation**: Ready-to-use repository classes
- ✅ **Auto-Configuration**: Discover entity configurations
- ✅ **Auto-Registration**: DI registration helpers
- ✅ **Unit of Work**: Transaction management
- ✅ **Savepoints**: Fine-grained transaction control
- ✅ **Raw SQL**: Execute custom queries
- ✅ **State Management**: Attach/Detach operations
- ✅ **Provider Detection**: Database provider info
- ✅ **90% Less Boilerplate**: Focus on domain logic

---

## Additional Resources

- **Repository**: https://gitlab.com/dotnet-myth/myth
- **License**: Apache 2.0
- **Target Framework**: .NET 10.0
- **NuGet Package**: Myth.Repository.EntityFramework

---

*This documentation is maintained for AI agents and developers. For questions or contributions, please refer to the repository.*
