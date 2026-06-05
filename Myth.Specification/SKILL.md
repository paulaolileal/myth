---
name: myth-specification
description: Use when you need to build reusable, composable database queries. SpecBuilder<T>.Create() chains .And()/.Or()/.Not()/.AndIf()/.OrIf() with Expression<Func<T,bool>>, plus .Order()/.OrderDescending(), .Skip()/.Take(), and .DistinctBy(). Translates to SQL via EF Core and also supports in-memory validation.
---

# SKILL.md - Myth.Specification

**Version:** 1.0
**Target Framework:** .NET 8.0
**License:** Apache 2.0

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [API Reference](#api-reference)
  - [ISpec Interface](#ispec-interface)
  - [SpecBuilder](#specbuilder)
  - [Extension Methods](#extension-methods)
  - [Exceptions](#exceptions)
- [Usage Examples](#usage-examples)
- [Advanced Patterns](#advanced-patterns)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Overview

Myth.Specification implements the **Specification Pattern** (from Domain-Driven Design) to encapsulate business rules and query logic in reusable, composable objects. It provides a fluent API for building complex queries with filtering, sorting, and pagination.

### Key Features

- **Fluent API**: Chainable methods for building complex specifications
- **Composable**: Combine specifications using AND, OR, NOT logic
- **Expression Trees**: Compatible with Entity Framework Core (translates to SQL)
- **Sorting**: Multi-level ordering with OrderBy/ThenBy support
- **Pagination**: Built-in support for Skip/Take and Pagination value objects
- **Distinct**: Remove duplicates by property
- **In-Memory Validation**: Validate entities against specifications
- **Separation of Concerns**: Keep business rules separate from repositories

---

## Installation

```bash
dotnet add package Myth.Specification
```

### Dependencies
- .NET 8.0 or higher
- Myth.Commons (for Pagination value object)

---

## Core Concepts

### 1. Specification Pattern

Specifications encapsulate query logic as objects that can be:
- **Composed**: Combined using logical operators (AND, OR, NOT)
- **Reused**: Shared across multiple queries
- **Tested**: Unit tested in isolation
- **Named**: Give business meaning to complex queries

### 2. Expression Trees

Specifications use `Expression<Func<T, bool>>` which:
- Can be translated to SQL by Entity Framework
- Can be compiled for in-memory evaluation
- Maintain full type safety

### 3. Fluent Interface

All methods return `ISpec<T>` for method chaining:
```csharp
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .And(u => u.Role == "Admin")
    .Order(u => u.Name)
    .Skip(10)
    .Take(20);
```

---

## API Reference

### ISpec Interface

**Namespace:** `Myth.Interfaces`

Core interface for all specifications.

#### Properties

```csharp
public interface ISpec<T> {
    // Filter expression
    Expression<Func<T, bool>> Predicate { get; }

    // Compiled predicate for in-memory use
    Func<T, bool> Query { get; }

    // Pagination tracking
    int ItemsSkiped { get; }
    int ItemsTaked { get; }

    // Sorting function
    Func<IQueryable<T>, IOrderedQueryable<T>> Sort { get; }

    // Post-processing (Skip/Take/Distinct)
    Func<IQueryable<T>, IQueryable<T>> PostProcess { get; }

    // Eager loading (Include/ThenInclude)
    Func<IQueryable<T>, IQueryable<T>>? Includes { get; }
}
```

#### Logical Methods

```csharp
// AND operations
ISpec<T> And(ISpec<T> specification);
ISpec<T> And(Expression<Func<T, bool>> expression);
ISpec<T> AndIf(bool condition, ISpec<T> specification);
ISpec<T> AndIf(bool condition, Expression<Func<T, bool>> expression);

// OR operations
ISpec<T> Or(ISpec<T> specification);
ISpec<T> Or(Expression<Func<T, bool>> expression);
ISpec<T> OrIf(bool condition, ISpec<T> specification);
ISpec<T> OrIf(bool condition, Expression<Func<T, bool>> expression);

// NOT operation
ISpec<T> Not();
```

**Examples:**

```csharp
// AND
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .And(u => u.Age >= 18);

// Conditional AND
var includeDeleted = false;
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .AndIf(!includeDeleted, u => !u.IsDeleted);

// OR
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .Or(u => u.Role == "Admin");

// NOT
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsDeleted)
    .Not();  // Inverts to !IsDeleted
```

#### Sorting Methods

```csharp
// Ascending order (uses OrderBy or ThenBy)
ISpec<T> Order<TProperty>(Expression<Func<T, TProperty>> property);

// Descending order (uses OrderByDescending or ThenByDescending)
ISpec<T> OrderDescending<TProperty>(Expression<Func<T, TProperty>> property);
```

**Examples:**

```csharp
// Single order
var spec = SpecBuilder<User>.Create()
    .Order(u => u.Name);

// Multiple orders (automatically uses ThenBy)
var spec = SpecBuilder<User>.Create()
    .Order(u => u.Department)
    .Order(u => u.Name)
    .OrderDescending(u => u.CreatedAt);
```

#### Pagination Methods

```csharp
// Skip elements
ISpec<T> Skip(int amount);

// Take elements
ISpec<T> Take(int amount);

// Apply pagination using value object
ISpec<T> WithPagination(Pagination pagination);
```

**Examples:**

```csharp
// Manual pagination
var spec = SpecBuilder<User>.Create()
    .Skip(20)
    .Take(10);  // Page 3, size 10

// Using Pagination value object
var pagination = new Pagination(pageNumber: 2, pageSize: 25);
var spec = SpecBuilder<User>.Create()
    .WithPagination(pagination);

// Predefined pagination
var spec = SpecBuilder<User>.Create()
    .WithPagination(Pagination.Default);  // Page 1, size 10

var spec = SpecBuilder<User>.Create()
    .WithPagination(Pagination.All);  // No pagination
```

#### Eager Loading Methods

```csharp
// Include navigation properties for eager loading (EF Core)
ISpec<T> Include(Func<IQueryable<T>, IQueryable<T>> includeQuery);
```

**Examples:**

```csharp
// Single Include
var spec = SpecBuilder<Project>
    .Create()
    .HasId(projectId)
    .Include(q => q.Include(p => p.Members));

// Multiple Includes
var spec = SpecBuilder<Project>
    .Create()
    .Include(q => q.Include(p => p.Members)
                   .Include(p => p.Tasks));

// Nested ThenInclude
var spec = SpecBuilder<Project>
    .Create()
    .Include(q => q.Include(p => p.Members)
                   .ThenInclude(m => m.User)
                   .ThenInclude(u => u.Profile));

// Complex Include chains
var spec = SpecBuilder<Order>
    .Create()
    .IsActive()
    .Include(q => q.Include(o => o.Customer)
                   .Include(o => o.Items)
                       .ThenInclude(i => i.Product)
                   .Include(o => o.ShippingAddress));
```

> **Note:** The `Include()` method uses EF Core's `Include` and `ThenInclude` extension methods. The includes are applied before filters in the query pipeline. This ensures that navigation properties are available for filtering and are loaded efficiently with the main query.

#### Post-Processing Methods

```csharp
// Remove duplicates by property
ISpec<T> DistinctBy<TProperty>(Expression<Func<T, TProperty>> property);
```

**Example:**

```csharp
var spec = SpecBuilder<User>.Create()
    .DistinctBy(u => u.Email);
```

#### Execution Methods

```csharp
// Apply all operations (Include + Filter + Sort + Post-process)
IQueryable<T> Prepare(IQueryable<T> query);

// Apply only includes (eager loading)
IQueryable<T> Included(IQueryable<T> query);

// Apply only filter
IQueryable<T> Filtered(IQueryable<T> query);

// Apply only sort
IQueryable<T> Sorted(IQueryable<T> query);

// Apply only post-processing (Skip/Take/Distinct)
IQueryable<T> Processed(IQueryable<T> query);

// Get first matching item
T? SatisfyingItemFrom(IQueryable<T> query);

// Get all matching items
IQueryable<T> SatisfyingItemsFrom(IQueryable<T> query);
```

**Examples:**

```csharp
var spec = SpecBuilder<Project>
    .Create()
    .And(p => p.IsActive)
    .Include(q => q.Include(p => p.Members))
    .Order(p => p.Name)
    .Skip(10)
    .Take(10);

// Apply everything (includes + filter + sort + pagination)
var projects = spec.Prepare(_context.Projects).ToList();

// Apply only includes
var included = spec.Included(_context.Projects);

// Apply only filter
var filtered = spec.Filtered(_context.Projects);

// Apply only sort
var sorted = spec.Sorted(_context.Projects);

// Apply only pagination
var paginated = spec.Processed(_context.Projects);

// Get single item (applies full spec)
var project = spec.SatisfyingItemFrom(_context.Projects);

// Get query for further processing
var query = spec.SatisfyingItemsFrom(_context.Projects);
var result = await query.ToListAsync();
```

#### Validation Methods

```csharp
// Validate entity in-memory
bool IsSatisfiedBy(T entity);
```

**Example:**

```csharp
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .And(u => !string.IsNullOrEmpty(u.Email))
    .And(u => u.Age >= 18);

var user = new User { IsActive = true, Email = "test@example.com", Age = 25 };
var isValid = spec.IsSatisfiedBy(user);  // true
```

#### Initialization Methods

```csharp
// Create empty specification
ISpec<T> InitEmpty();
```

---

### SpecBuilder

**Namespace:** `Myth.Specifications`

Factory class for creating specifications.

```csharp
public abstract class SpecBuilder<T> : ISpec<T> {
    // Create new empty specification
    public static ISpec<T> Create();

    // Implicit conversion to Expression
    public static implicit operator Expression<Func<T, bool>>(SpecBuilder<T> spec);
}
```

**Usage:**

```csharp
// Create empty spec
var spec = SpecBuilder<User>.Create();

// Use as Expression<Func<T, bool>>
Expression<Func<User, bool>> expression = spec;
```

---

### Extension Methods

**Namespace:** `Myth.Extensions`

Extension methods for IEnumerable and IQueryable.

#### For IEnumerable<T>

```csharp
// Filter using compiled predicate
IEnumerable<T> Where<T>(this IEnumerable<T> values, ISpec<T> spec);

// Apply full specification (converts to IQueryable)
IQueryable<T> Specify<T>(this IEnumerable<T> values, ISpec<T> spec);
```

**Examples:**

```csharp
var users = GetUsersFromMemory();

// Filter only
var active = users.Where(spec);

// Full specification
var result = users.Specify(spec).ToList();
```

#### For IQueryable<T>

```csharp
// Apply filter only
IQueryable<T> Filter<T>(this IQueryable<T> values, ISpec<T> spec);

// Apply sort only
IQueryable<T> Sort<T>(this IQueryable<T> values, ISpec<T> spec);

// Apply post-processing only (Skip/Take/Distinct)
IQueryable<T> Paginate<T>(this IQueryable<T> values, ISpec<T> spec);

// Apply full specification (same as Prepare)
IQueryable<T> Specify<T>(this IQueryable<T> values, ISpec<T> spec);
```

**Examples:**

```csharp
var query = _context.Users.AsQueryable();

// Apply filter only
var filtered = query.Filter(spec);

// Apply sort only
var sorted = query.Sort(spec);

// Apply pagination only
var paginated = query.Paginate(spec);

// Apply everything
var result = query.Specify(spec).ToList();
```

---

### Exceptions

#### InvalidSpecificationException

**Namespace:** `Myth.Exceptions`

```csharp
public sealed class InvalidSpecificationException : Exception {
    public InvalidSpecificationException(string message)
}
```

**When Thrown:**
- When `IsSatisfiedBy()` is called on specification with null Predicate

**Example:**

```csharp
try {
    var isValid = spec.IsSatisfiedBy(entity);
} catch (InvalidSpecificationException ex) {
    _logger.LogError(ex, "Invalid specification");
}
```

#### SpecificationException

**Namespace:** `Myth.Exceptions`

```csharp
public class SpecificationException : Exception {
    public SpecificationException(string message, Exception? exception)
}
```

**When Thrown:**
- Error applying filter: "Error on apply filter specification!"
- Error applying sort: "Error on apply sort specification!"
- Error applying post-process: "Error on apply post process specification!"

**Example:**

```csharp
try {
    var result = spec.Prepare(query).ToList();
} catch (SpecificationException ex) {
    _logger.LogError(ex, "Specification error: {Message}", ex.Message);
}
```

---

## Usage Examples

### Example 1: Basic User Query with Filters

```csharp
public class UserRepository {
    private readonly ApplicationDbContext _context;

    public async Task<List<User>> GetActiveAdultsAsync() {
        var spec = SpecBuilder<User>.Create()
            .And(u => u.IsActive)
            .And(u => u.Age >= 18)
            .Order(u => u.Name);

        return await _context.Users
            .Specify(spec)
            .ToListAsync();
    }
}
```

---

### Example 2: Reusable Specifications as Extension Methods

```csharp
public static class UserSpecifications {
    public static ISpec<User> IsActive(this ISpec<User> spec) {
        return spec.And(u => u.IsActive && !u.IsDeleted);
    }

    public static ISpec<User> HasRole(this ISpec<User> spec, string role) {
        return spec.And(u => u.Role == role);
    }

    public static ISpec<User> EmailContains(this ISpec<User> spec, string searchTerm) {
        return spec.And(u => u.Email.Contains(searchTerm));
    }

    public static ISpec<User> CreatedAfter(this ISpec<User> spec, DateTime date) {
        return spec.And(u => u.CreatedAt >= date);
    }

    public static ISpec<User> OrderByName(this ISpec<User> spec) {
        return spec.Order(u => u.Name);
    }
}

// Usage
public async Task<List<User>> SearchUsersAsync(string searchTerm, string role) {
    var spec = SpecBuilder<User>.Create()
        .IsActive()
        .HasRole(role)
        .EmailContains(searchTerm)
        .OrderByName()
        .WithPagination(new Pagination(1, 20));

    return await _context.Users.Specify(spec).ToListAsync();
}
```

---

### Example 3: Conditional Filtering

```csharp
public async Task<List<Product>> SearchProductsAsync(ProductSearchRequest request) {
    var spec = SpecBuilder<Product>.Create();

    // Always active
    spec = spec.And(p => p.IsActive);

    // Conditional filters
    spec = spec.AndIf(
        !string.IsNullOrEmpty(request.Name),
        p => p.Name.Contains(request.Name));

    spec = spec.AndIf(
        request.MinPrice.HasValue,
        p => p.Price >= request.MinPrice!.Value);

    spec = spec.AndIf(
        request.MaxPrice.HasValue,
        p => p.Price <= request.MaxPrice!.Value);

    spec = spec.AndIf(
        !string.IsNullOrEmpty(request.Category),
        p => p.Category == request.Category);

    // Sorting
    if (request.SortBy == "price") {
        spec = request.SortDescending
            ? spec.OrderDescending(p => p.Price)
            : spec.Order(p => p.Price);
    } else {
        spec = spec.Order(p => p.Name);
    }

    // Pagination
    spec = spec.WithPagination(new Pagination(
        request.PageNumber,
        request.PageSize));

    return await _context.Products.Specify(spec).ToListAsync();
}
```

---

### Example 4: Complex Business Rules

```csharp
public static class OrderSpecifications {
    public static ISpec<Order> CanBeShipped(this ISpec<Order> spec) {
        return spec.And(o =>
            o.Status == OrderStatus.Confirmed &&
            o.PaymentStatus == PaymentStatus.Paid &&
            !o.IsDeleted &&
            o.Items.Any());
    }

    public static ISpec<Order> RequiresAttention(this ISpec<Order> spec) {
        return spec.Or(o =>
            (o.Status == OrderStatus.Pending && o.CreatedAt < DateTime.UtcNow.AddDays(-2)) ||
            (o.Status == OrderStatus.Confirmed && o.PaymentStatus == PaymentStatus.Pending) ||
            (o.Status == OrderStatus.Shipped && o.ShippedAt < DateTime.UtcNow.AddDays(-7)));
    }

    public static ISpec<Order> ForCustomer(this ISpec<Order> spec, Guid customerId) {
        return spec.And(o => o.CustomerId == customerId);
    }

    public static ISpec<Order> InDateRange(
        this ISpec<Order> spec,
        DateTime startDate,
        DateTime endDate) {
        return spec.And(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
    }
}

// Usage
public async Task<List<Order>> GetShippableOrdersAsync() {
    var spec = SpecBuilder<Order>.Create()
        .CanBeShipped()
        .Order(o => o.Priority)
        .OrderDescending(o => o.CreatedAt);

    return await _context.Orders.Specify(spec).ToListAsync();
}

public async Task<List<Order>> GetOrdersRequiringAttentionAsync() {
    var spec = SpecBuilder<Order>.Create()
        .RequiresAttention()
        .OrderDescending(o => o.CreatedAt);

    return await _context.Orders.Specify(spec).ToListAsync();
}
```

---

### Example 5: Paginated API Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase {
    private readonly ApplicationDbContext _context;

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<UserDto>>> GetUsers(
        [FromQuery] Pagination pagination,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null) {

        var spec = SpecBuilder<User>.Create();

        // Apply filters
        spec = spec.AndIf(isActive.HasValue, u => u.IsActive == isActive!.Value);
        spec = spec.AndIf(!string.IsNullOrEmpty(role), u => u.Role == role);
        spec = spec.AndIf(!string.IsNullOrEmpty(search), u =>
            u.Name.Contains(search!) ||
            u.Email.Contains(search!));

        // Count total before pagination
        var totalItems = await _context.Users.Filter(spec).CountAsync();

        // Apply sorting and pagination
        spec = spec.Order(u => u.Name).WithPagination(pagination);

        // Execute query
        var users = await _context.Users
            .Specify(spec)
            .Select(u => new UserDto {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            })
            .ToListAsync();

        return new PaginatedResponse<UserDto> {
            Items = users,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize)
        };
    }
}
```

---

### Example 6: In-Memory Validation

```csharp
public class UserValidator {
    private readonly ISpec<User> _validUserSpec;

    public UserValidator() {
        _validUserSpec = SpecBuilder<User>.Create()
            .And(u => !string.IsNullOrEmpty(u.Name))
            .And(u => !string.IsNullOrEmpty(u.Email))
            .And(u => u.Email.Contains("@"))
            .And(u => u.Age >= 18)
            .And(u => u.Age <= 120);
    }

    public bool IsValid(User user) {
        return _validUserSpec.IsSatisfiedBy(user);
    }

    public List<string> Validate(User user) {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(user.Name))
            errors.Add("Name is required");

        if (string.IsNullOrEmpty(user.Email))
            errors.Add("Email is required");
        else if (!user.Email.Contains("@"))
            errors.Add("Email must be valid");

        if (user.Age < 18)
            errors.Add("User must be at least 18 years old");

        if (user.Age > 120)
            errors.Add("Invalid age");

        return errors;
    }
}

// Usage
var user = new User { Name = "John", Email = "john@example.com", Age = 25 };
var validator = new UserValidator();

if (validator.IsValid(user)) {
    await _repository.AddAsync(user);
} else {
    var errors = validator.Validate(user);
    return BadRequest(errors);
}
```

---

### Example 7: Combining Specifications

```csharp
public class ReportService {
    public async Task<OrderReport> GenerateMonthlyReportAsync(
        int year,
        int month,
        Guid? customerId = null) {

        // Base specification for date range
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var baseSpec = SpecBuilder<Order>.Create()
            .And(o => o.CreatedAt >= startDate)
            .And(o => o.CreatedAt < endDate);

        // Customer filter
        var spec = baseSpec;
        if (customerId.HasValue) {
            spec = spec.And(o => o.CustomerId == customerId.Value);
        }

        // Get statistics
        var allOrders = await _context.Orders.Filter(spec).ToListAsync();

        var completedSpec = spec.And(o => o.Status == OrderStatus.Completed);
        var completedOrders = await _context.Orders.Filter(completedSpec).ToListAsync();

        var cancelledSpec = spec.And(o => o.Status == OrderStatus.Cancelled);
        var cancelledOrders = await _context.Orders.Filter(cancelledSpec).ToListAsync();

        return new OrderReport {
            Period = $"{year}-{month:D2}",
            TotalOrders = allOrders.Count,
            CompletedOrders = completedOrders.Count,
            CancelledOrders = cancelledOrders.Count,
            TotalRevenue = completedOrders.Sum(o => o.Total),
            AverageOrderValue = completedOrders.Any()
                ? completedOrders.Average(o => o.Total)
                : 0
        };
    }
}
```

---

### Example 8: Eager Loading with Include

```csharp
// Define spec extensions with includes
public static class ProjectSpecifications {
    public static ISpec<Project> HasId(this ISpec<Project> spec, Guid id) =>
        spec.And(p => p.Id == id);

    public static ISpec<Project> IsActive(this ISpec<Project> spec) =>
        spec.And(p => !p.IsDeleted && p.Status == ProjectStatus.Active);

    public static ISpec<Project> WithMembers(this ISpec<Project> spec) =>
        spec.Include(q => q.Include(p => p.Members));

    public static ISpec<Project> WithMembersAndRoles(this ISpec<Project> spec) =>
        spec.Include(q => q.Include(p => p.Members)
                           .ThenInclude(m => m.Role));

    public static ISpec<Project> WithFullDetails(this ISpec<Project> spec) =>
        spec.Include(q => q.Include(p => p.Members)
                           .ThenInclude(m => m.User)
                           .Include(p => p.Tasks)
                           .ThenInclude(t => t.Assignee)
                           .Include(p => p.Owner));
}

// Usage in repository
public class ProjectRepository {
    private readonly ApplicationDbContext _context;

    // Simple query with eager loading
    public async Task<Project?> GetByIdWithMembersAsync(Guid id, CancellationToken ct) {
        var spec = SpecBuilder<Project>
            .Create()
            .HasId(id)
            .IsActive()
            .WithMembers();

        return await _context.Projects
            .Specify(spec)
            .FirstOrDefaultAsync(ct);
    }

    // Complex query with multiple includes
    public async Task<List<Project>> GetActiveProjectsWithDetailsAsync(
        string? searchTerm,
        Pagination pagination,
        CancellationToken ct) {

        var spec = SpecBuilder<Project>
            .Create()
            .IsActive()
            .AndIf(!string.IsNullOrEmpty(searchTerm), p => p.Name.Contains(searchTerm!))
            .WithFullDetails()
            .Order(p => p.Name)
            .WithPagination(pagination);

        return await _context.Projects
            .Specify(spec)
            .ToListAsync(ct);
    }

    // Using repository methods with specs containing includes
    public async Task<Project?> GetProjectDetailsAsync(Guid projectId, CancellationToken ct) {
        var spec = SpecBuilder<Project>
            .Create()
            .HasId(projectId)
            .Include(q => q.Include(p => p.Members)
                           .ThenInclude(m => m.User)
                           .ThenInclude(u => u.Profile)
                           .Include(p => p.Tasks)
                           .Include(p => p.Documents));

        // Repository's FirstOrDefaultAsync automatically applies the full spec
        // including includes, filters, and sorting
        return await _repository.FirstOrDefaultAsync(spec, ct);
    }
}
```

**Key Points:**

- `.Include()` is applied **before filters** in the query pipeline, ensuring navigation properties are eagerly loaded
- Multiple includes can be chained together
- `ThenInclude()` loads nested navigation properties
- Includes work seamlessly with repository methods like `FirstOrDefaultAsync`, `SearchAsync`, etc.
- Spec extensions can encapsulate common include patterns (e.g., `WithFullDetails()`)

---

## Advanced Patterns

### Pattern 1: Specification Factory

```csharp
public class UserSpecificationFactory {
    public ISpec<User> CreateForRole(string role) {
        return SpecBuilder<User>.Create()
            .And(u => u.IsActive)
            .And(u => u.Role == role)
            .Order(u => u.Name);
    }

    public ISpec<User> CreateForSearch(string searchTerm) {
        return SpecBuilder<User>.Create()
            .And(u => u.IsActive)
            .And(u =>
                u.Name.Contains(searchTerm) ||
                u.Email.Contains(searchTerm))
            .Order(u => u.Name);
    }

    public ISpec<User> CreateForDepartment(string department) {
        return SpecBuilder<User>.Create()
            .And(u => u.IsActive)
            .And(u => u.Department == department)
            .Order(u => u.Department)
            .Order(u => u.Name);
    }
}
```

---

### Pattern 2: Specification Composition

```csharp
public class OrderQueryService {
    private ISpec<Order> BaseSpec => SpecBuilder<Order>.Create()
        .And(o => !o.IsDeleted);

    private ISpec<Order> ActiveSpec => BaseSpec
        .And(o => o.Status != OrderStatus.Cancelled);

    private ISpec<Order> PendingSpec => ActiveSpec
        .And(o => o.Status == OrderStatus.Pending);

    private ISpec<Order> ShippableSpec => ActiveSpec
        .And(o => o.Status == OrderStatus.Confirmed)
        .And(o => o.PaymentStatus == PaymentStatus.Paid);

    public async Task<List<Order>> GetPendingOrdersAsync() {
        return await _context.Orders
            .Specify(PendingSpec.Order(o => o.CreatedAt))
            .ToListAsync();
    }

    public async Task<List<Order>> GetShippableOrdersAsync() {
        return await _context.Orders
            .Specify(ShippableSpec.OrderDescending(o => o.Priority))
            .ToListAsync();
    }
}
```

---

### Pattern 3: Repository with Specifications

```csharp
public interface IRepository<T> where T : class {
    Task<List<T>> FindAsync(ISpec<T> specification);
    Task<T?> FindOneAsync(ISpec<T> specification);
    Task<int> CountAsync(ISpec<T> specification);
    Task<bool> AnyAsync(ISpec<T> specification);
}

public class Repository<T> : IRepository<T> where T : class {
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(DbContext context) {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<List<T>> FindAsync(ISpec<T> specification) {
        return await _dbSet.Specify(specification).ToListAsync();
    }

    public async Task<T?> FindOneAsync(ISpec<T> specification) {
        return await _dbSet.Filter(specification).FirstOrDefaultAsync();
    }

    public async Task<int> CountAsync(ISpec<T> specification) {
        return await _dbSet.Filter(specification).CountAsync();
    }

    public async Task<bool> AnyAsync(ISpec<T> specification) {
        return await _dbSet.Filter(specification).AnyAsync();
    }
}

// Usage
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive)
    .Order(u => u.Name)
    .Skip(20)
    .Take(10);

var users = await _userRepository.FindAsync(spec);
var count = await _userRepository.CountAsync(spec);
```

---

## Best Practices

### 1. Create Reusable Specifications in Static Classes

**IMPORTANT:** Always create specification extension methods in **static classes** named after your entity/model. This provides clear business meaning and improves code organization.

**✅ DO:**
```csharp
// Create static class for each entity/model
public static class UserSpecifications {
    public static ISpec<User> IsActive(this ISpec<User> spec) =>
        spec.And(u => u.IsActive && !u.IsDeleted);

    public static ISpec<User> HasRole(this ISpec<User> spec, string role) =>
        spec.And(u => u.Role == role);

    public static ISpec<User> CreatedAfter(this ISpec<User> spec, DateTime date) =>
        spec.And(u => u.CreatedAt >= date);
}

// Usage is clear and expressive
var activeAdmins = SpecBuilder<User>.Create()
    .IsActive()
    .HasRole("Admin")
    .CreatedAfter(DateTime.UtcNow.AddDays(-30));
```

**❌ DON'T:**
```csharp
// Repeating logic everywhere (violates DRY principle)
var spec1 = SpecBuilder<User>.Create().And(u => u.IsActive && !u.IsDeleted);
var spec2 = SpecBuilder<User>.Create().And(u => u.IsActive && !u.IsDeleted);

// Or mixing specifications in non-static classes
public class UserRepository {
    public ISpec<User> IsActive() => ... // Wrong place!
}
```

**Benefits of Static Classes:**
- **Business Clarity**: `UserSpecifications.IsActive()` clearly expresses business intent
- **Discoverability**: IntelliSense shows all available specifications for the entity
- **Organization**: All business rules for an entity are in one place
- **Reusability**: Can be used across repositories, services, and controllers
- **Testability**: Easy to unit test specifications in isolation

---

### 2. Use Meaningful Names

**✅ DO:**
```csharp
public static ISpec<Order> CanBeShipped(this ISpec<Order> spec) { }
public static ISpec<Order> RequiresAttention(this ISpec<Order> spec) { }
public static ISpec<Order> IsOverdue(this ISpec<Order> spec) { }
```

**❌ DON'T:**
```csharp
public static ISpec<Order> Spec1(this ISpec<Order> spec) { }
public static ISpec<Order> Check(this ISpec<Order> spec) { }
```

---

### 3. Keep Specifications Focused

**✅ DO:**
```csharp
// Small, focused specifications
public static ISpec<User> IsActive(this ISpec<User> spec) =>
    spec.And(u => u.IsActive);

public static ISpec<User> IsAdult(this ISpec<User> spec) =>
    spec.And(u => u.Age >= 18);

// Compose them
var spec = SpecBuilder<User>.Create()
    .IsActive()
    .IsAdult();
```

**❌ DON'T:**
```csharp
// Large, monolithic specification
public static ISpec<User> ComplexSpec(this ISpec<User> spec) =>
    spec.And(u => u.IsActive && u.Age >= 18 && u.HasRole("Admin") && ...);
```

---

### 4. Separate Filter Count from Paginated Query

**✅ DO:**
```csharp
// Count without pagination
var totalCount = await _context.Users.Filter(spec).CountAsync();

// Then apply pagination
spec = spec.WithPagination(pagination);
var users = await _context.Users.Specify(spec).ToListAsync();
```

**❌ DON'T:**
```csharp
// This counts only paginated results!
spec = spec.WithPagination(pagination);
var count = await _context.Users.Specify(spec).CountAsync();  // Wrong!
```

---

### 5. Use AndIf for Optional Filters

**✅ DO:**
```csharp
var spec = SpecBuilder<Product>.Create()
    .AndIf(!string.IsNullOrEmpty(name), p => p.Name.Contains(name))
    .AndIf(minPrice.HasValue, p => p.Price >= minPrice!.Value)
    .AndIf(maxPrice.HasValue, p => p.Price <= maxPrice!.Value);
```

**❌ DON'T:**
```csharp
var spec = SpecBuilder<Product>.Create();
if (!string.IsNullOrEmpty(name))
    spec = spec.And(p => p.Name.Contains(name));
if (minPrice.HasValue)
    spec = spec.And(p => p.Price >= minPrice.Value);
```

---

### 6. Format Specification Chains on Multiple Lines

Break the `SpecBuilder` chain across multiple lines to improve readability. Each method call should be on its own line with consistent indentation.

**✅ DO:**
```csharp
var spec = SpecBuilder<Idea>
    .Create()
    .HasId(command.IdeaId)
    .IsActive()
    .Order(i => i.CreatedAt);
```

**❌ DON'T:**
```csharp
var spec = SpecBuilder<Idea>.Create().HasId(command.IdeaId).IsActive().Order(i => i.CreatedAt);
```

**Benefits:**
- **Readability**: Each filter is visible at a glance
- **Diffs**: Git diffs are cleaner when adding/removing individual steps
- **Debugging**: Easier to comment out a single step during debugging

---

### 7. Use Repository Methods Directly with Specifications

When the repository exposes methods that accept `ISpec<T>` directly (e.g., `FirstOrDefaultAsync`, `ToListAsync`, `CountAsync`), prefer those over manually chaining `.Where(spec)` on a queryable, as mixing EF Core query chains with specifications breaks the abstraction and makes the intent less clear.

**✅ DO:**
```csharp
var spec = SpecBuilder<Project>
    .Create()
    .HasId(command.ProjectId)
    .IsActive();

var project = await projectRepository.FirstOrDefaultAsync(spec, cancellationToken);
```

**❌ DON'T:**
```csharp
var spec = SpecBuilder<Project>
    .Create()
    .HasId(command.ProjectId)
    .IsActive();

// Bypasses the repository abstraction and leaks EF Core concerns
var project = await projectRepository
    .Where(spec)
    .Include(p => p.Members)
    .FirstOrDefaultAsync(cancellationToken);
```

> **Note:** If you need to apply `.Include()` or other EF Core-specific operations, keep those inside the repository implementation, not in the caller. The caller should only provide the specification.

---

## Troubleshooting

### Issue 1: Expression Cannot Be Translated to SQL

**Problem:**
```csharp
var spec = SpecBuilder<User>.Create()
    .And(u => SomeLocalMethod(u.Name));  // Error!
```

**Cause:** EF Core cannot translate custom methods to SQL.

**Solution:**
```csharp
// Use EF Core compatible expressions
var spec = SpecBuilder<User>.Create()
    .And(u => u.Name.Contains("search"));

// Or evaluate locally
var users = await _context.Users.ToListAsync();
var filtered = users.Where(spec);
```

---

### Issue 2: InvalidSpecificationException

**Problem:** `IsSatisfiedBy` throws exception.

**Cause:** Predicate is null (using NullSpec or empty spec).

**Solution:**
```csharp
// Ensure specification has filters
var spec = SpecBuilder<User>.Create()
    .And(u => u.IsActive);  // Add at least one filter

var isValid = spec.IsSatisfiedBy(user);
```

---

### Issue 3: Ordering Not Applied

**Problem:** Results not sorted as expected.

**Cause:** Forgetting to call `Sort()` or `Specify()`.

**Solution:**
```csharp
// ❌ Wrong - only filters
var users = await _context.Users.Filter(spec).ToListAsync();

// ✅ Correct - includes sorting
var users = await _context.Users.Specify(spec).ToListAsync();
// Or
var users = await _context.Users.Filter(spec).Sort(spec).ToListAsync();
```

---

### Issue 4: Pagination Not Working with Pagination.All

**Problem:** All items returned instead of paginated.

**Cause:** `Pagination.All` explicitly disables pagination.

**Solution:**
```csharp
// Use specific pagination
var pagination = request.GetAll
    ? Pagination.All
    : new Pagination(request.PageNumber, request.PageSize);
```

---

## Performance Considerations

1. **Filter Before Sort**: EF Core can optimize filtered queries better
2. **Count Before Pagination**: Get total count before applying Skip/Take
3. **Projection**: Use Select() after specifications to reduce data transfer
4. **AsNoTracking**: Use for read-only queries
5. **Compiled Queries**: Consider for frequently used specifications

```csharp
// Optimized query
var totalCount = await _context.Users.Filter(spec).CountAsync();

var users = await _context.Users
    .AsNoTracking()
    .Specify(spec)
    .Select(u => new UserDto { Id = u.Id, Name = u.Name })
    .ToListAsync();
```

---

## Integration with Myth Ecosystem

```csharp
using Myth.Extensions;
using Myth.Interfaces;
using Myth.Specifications;
using Myth.ValueObjects;

// Works with Myth.Repository
public class UserRepository : IUserRepository {
    public async Task<IPaginated<User>> GetPaginatedAsync(
        ISpec<User> specification,
        Pagination pagination) {

        var spec = specification.WithPagination(pagination);
        var users = await _context.Users.Specify(spec).ToListAsync();

        return new Paginated<User>(
            pagination.PageNumber,
            pagination.PageSize,
            await _context.Users.Filter(specification).CountAsync(),
            /* calculate total pages */,
            users);
    }
}
```

---

## Summary

Myth.Specification provides:

- ✅ **Reusable Business Rules**: Encapsulate query logic
- ✅ **Composable**: Combine with AND, OR, NOT
- ✅ **Type Safe**: Full IntelliSense and compile-time checking
- ✅ **EF Core Compatible**: Translates to SQL
- ✅ **Testable**: Unit test specifications in isolation
- ✅ **Maintainable**: Change business rules in one place

---

## Additional Resources

- **Repository**: https://gitlab.com/dotnet-myth/myth
- **License**: Apache 2.0
- **Target Framework**: .NET 8.0
- **NuGet Package**: Myth.Specification

---

*This documentation is maintained for AI agents and developers. For questions or contributions, please refer to the repository.*
