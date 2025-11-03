# Myth.Specification

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Specification?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Specification/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Specification?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Specification/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A fluent, type-safe .NET library implementing the **Specification Pattern** for building complex, composable, and testable queries. Keep your business logic readable, maintainable, and firmly rooted in domain concepts.

## Why Myth.Specification?

Traditional query building often leads to:
- Scattered business rules across services and repositories
- Difficult-to-test query logic
- Duplicate filtering logic in multiple places
- Poor readability and maintainability

Myth.Specification solves these problems by:
- Encapsulating query logic in reusable, composable specifications
- Making business rules explicit and self-documenting
- Providing a fluent, chainable API for building complex queries
- Supporting filtering, sorting, pagination, and validation in one cohesive interface

## Installation

```bash
dotnet add package Myth.Specification
```

## Core Concepts

The library is built around the `ISpec<T>` interface and `SpecBuilder<T>` abstract class, providing:

- **Filtering**: Combine predicates with `And`, `Or`, `Not`, `AndIf`, `OrIf`
- **Sorting**: Chain multiple sort criteria with `Order` and `OrderDescending`
- **Pagination**: Apply skip/take logic with `Skip`, `Take`, or `WithPagination`
- **Post-processing**: Apply distinct operations with `DistinctBy`
- **Validation**: Check if entities satisfy specifications with `IsSatisfiedBy`
- **Execution**: Apply specifications to queryables and enumerate results

## Quick Start

### Basic Specification Building

```csharp
using Myth.Interfaces;
using Myth.Specifications;
using Myth.Extensions;

// Build a specification
var spec = SpecBuilder<Person>
    .Create()
    .And( x => x.IsActive )
    .And( x => x.Age >= 18 )
    .Order( x => x.Name )
    .Skip( 10 )
    .Take( 20 );

// Apply to a queryable
var results = dbContext.People
    .Specify( spec )
    .ToList();
```

### Encapsulating Business Rules

The real power comes from creating reusable specification extensions that represent business concepts:

```csharp
public static class PersonSpecifications {
    public static ISpec<Person> IsAdult( this ISpec<Person> spec ) {
        return spec.And( person => person.Age >= 18 );
    }

    public static ISpec<Person> IsActive( this ISpec<Person> spec ) {
        return spec.And( person => person.Status == PersonStatus.Active );
    }

    public static ISpec<Person> HasEmail( this ISpec<Person> spec ) {
        return spec.And( person => !string.IsNullOrEmpty( person.Email ) );
    }

    public static ISpec<Person> InCity( this ISpec<Person> spec, string city ) {
        return spec.And( person => person.Address.City == city );
    }

    public static ISpec<Person> RegisteredAfter( this ISpec<Person> spec, DateTime date ) {
        return spec.And( person => person.RegistrationDate >= date );
    }
}
```

Now your queries read like business requirements:

```csharp
var spec = SpecBuilder<Person>
    .Create()
    .IsActive()
    .IsAdult()
    .HasEmail()
    .InCity( "New York" )
    .RegisteredAfter( DateTime.UtcNow.AddYears( -1 ) )
    .Order( x => x.Name )
    .WithPagination( pagination );

var activeAdultSubscribers = await repository.GetAsync( spec, cancellationToken );
```

## API Reference

### Building Specifications

#### Creating Specifications

```csharp
// Start with empty specification
var spec = SpecBuilder<Person>.Create();
```

#### Logical Operators

```csharp
// And - adds a filter condition
spec.And( x => x.Age >= 18 );
spec.And( otherSpec );

// AndIf - conditionally adds a filter
spec.AndIf( includeInactive, x => x.IsActive == false );

// Or - adds an alternative filter condition
spec.Or( x => x.IsVip );
spec.Or( vipSpec );

// OrIf - conditionally adds an alternative filter
spec.OrIf( allowGuests, x => x.Role == "Guest" );

// Not - negates the current specification
spec.And( x => x.Status == "Banned" ).Not();
```

#### Sorting

```csharp
// Sort ascending
spec.Order( x => x.LastName )
    .Order( x => x.FirstName );

// Sort descending
spec.OrderDescending( x => x.RegistrationDate )
    .OrderDescending( x => x.Score );

// Combine ascending and descending
spec.Order( x => x.Category )
    .OrderDescending( x => x.Priority );
```

#### Pagination

```csharp
// Manual pagination
spec.Skip( 20 ).Take( 10 );

// Using Pagination value object
using Myth.ValueObjects;

var pagination = new Pagination( pageNumber: 3, pageSize: 20 );
spec.WithPagination( pagination );

// Special cases
Pagination.Default;  // Page 1, 10 items
Pagination.All;      // No pagination (returns all)

// Combining pagination methods
spec.WithPagination( pagination )
    .Skip( 5 )  // Additional skip
    .Take( 3 ); // Further limit
```

#### Post-Processing

```csharp
// Remove duplicates based on property
spec.DistinctBy( x => x.Email );
```

### Applying Specifications

#### Extension Methods

```csharp
using Myth.Extensions;

IQueryable<Person> people = dbContext.People;

// Apply all transformations (filter + sort + paginate)
var results = people.Specify( spec );

// Apply only filtering
var filtered = people.Filter( spec );

// Apply only sorting
var sorted = people.Sort( spec );

// Apply only pagination/post-processing
var paginated = people.Paginate( spec );
```

#### Direct Methods

```csharp
// Apply all transformations
IQueryable<Person> result = spec.Prepare( queryable );

// Apply individual transformations
IQueryable<Person> filtered = spec.Filtered( queryable );
IQueryable<Person> sorted = spec.Sorted( queryable );
IQueryable<Person> processed = spec.Processed( queryable );

// Get results directly
IQueryable<Person> items = spec.SatisfyingItemsFrom( queryable );
Person? singleItem = spec.SatisfyingItemFrom( queryable );
```

#### Validation

```csharp
// Check if an entity satisfies the specification
Person person = GetPerson();
bool isValid = spec.IsSatisfiedBy( person );
```

### Specification Properties

```csharp
// Access the underlying expression
Expression<Func<T, bool>> predicate = spec.Predicate;
Func<T, bool> query = spec.Query;

// Access transformation functions
Func<IQueryable<T>, IOrderedQueryable<T>> sortFunc = spec.Sort;
Func<IQueryable<T>, IQueryable<T>> postProcessFunc = spec.PostProcess;

// Access pagination tracking
int itemsSkipped = spec.ItemsSkiped;
int itemsTaken = spec.ItemsTaked;
```

## Integration with Repository Pattern

Myth.Specification works seamlessly with repository patterns:

```csharp
public interface IPersonRepository {
    Task<IEnumerable<Person>> GetAsync( ISpec<Person> specification, CancellationToken cancellationToken = default );
    Task<Person?> GetSingleAsync( ISpec<Person> specification, CancellationToken cancellationToken = default );
    Task<int> CountAsync( ISpec<Person> specification, CancellationToken cancellationToken = default );
}

public class PersonRepository : IPersonRepository {
    private readonly DbContext _context;

    public async Task<IEnumerable<Person>> GetAsync( ISpec<Person> specification, CancellationToken cancellationToken = default ) {
        return await _context.People
            .Specify( specification )
            .ToListAsync( cancellationToken );
    }

    public async Task<Person?> GetSingleAsync( ISpec<Person> specification, CancellationToken cancellationToken = default ) {
        return specification.SatisfyingItemFrom( _context.People.AsQueryable() );
    }

    public async Task<int> CountAsync( ISpec<Person> specification, CancellationToken cancellationToken = default ) {
        return await _context.People
            .Filter( specification )
            .CountAsync( cancellationToken );
    }
}
```

## Real-World Examples

### E-commerce Product Search

```csharp
public static class ProductSpecifications {
    public static ISpec<Product> InStock( this ISpec<Product> spec ) {
        return spec.And( p => p.StockQuantity > 0 );
    }

    public static ISpec<Product> InCategory( this ISpec<Product> spec, string category ) {
        return spec.And( p => p.Category == category );
    }

    public static ISpec<Product> InPriceRange( this ISpec<Product> spec, decimal min, decimal max ) {
        return spec.And( p => p.Price >= min && p.Price <= max );
    }

    public static ISpec<Product> WithRatingAbove( this ISpec<Product> spec, double rating ) {
        return spec.And( p => p.AverageRating >= rating );
    }

    public static ISpec<Product> OnSale( this ISpec<Product> spec ) {
        return spec.And( p => p.DiscountPercentage > 0 );
    }
}

// In a service
public async Task<ProductSearchResult> SearchProductsAsync( ProductSearchRequest request ) {
    var spec = SpecBuilder<Product>
        .Create()
        .InStock()
        .AndIf( !string.IsNullOrEmpty( request.Category ), s => s.InCategory( request.Category ) )
        .AndIf( request.MinPrice.HasValue && request.MaxPrice.HasValue,
                s => s.InPriceRange( request.MinPrice.Value, request.MaxPrice.Value ) )
        .AndIf( request.MinRating.HasValue, s => s.WithRatingAbove( request.MinRating.Value ) )
        .AndIf( request.OnSaleOnly, s => s.OnSale() )
        .OrderDescending( p => p.AverageRating )
        .WithPagination( request.Pagination );

    var products = await _productRepository.GetAsync( spec );

    return new ProductSearchResult { Products = products };
}
```

### User Management

```csharp
public static class UserSpecifications {
    public static ISpec<User> IsVerified( this ISpec<User> spec ) {
        return spec.And( u => u.EmailVerified );
    }

    public static ISpec<User> HasRole( this ISpec<User> spec, string role ) {
        return spec.And( u => u.Roles.Contains( role ) );
    }

    public static ISpec<User> LastLoginAfter( this ISpec<User> spec, DateTime date ) {
        return spec.And( u => u.LastLoginDate >= date );
    }

    public static ISpec<User> IsActive( this ISpec<User> spec ) {
        return spec.And( u => !u.IsDeleted && !u.IsSuspended );
    }
}

// Find inactive users for cleanup
var inactiveUsers = SpecBuilder<User>
    .Create()
    .IsActive()
    .LastLoginAfter( DateTime.UtcNow.AddMonths( -6 ) )
    .Not();  // Negate to find inactive users
```

### Audit Log Filtering

```csharp
public static class AuditLogSpecifications {
    public static ISpec<AuditLog> ByUser( this ISpec<AuditLog> spec, Guid userId ) {
        return spec.And( log => log.UserId == userId );
    }

    public static ISpec<AuditLog> ByAction( this ISpec<AuditLog> spec, string action ) {
        return spec.And( log => log.Action == action );
    }

    public static ISpec<AuditLog> InDateRange( this ISpec<AuditLog> spec, DateTime start, DateTime end ) {
        return spec.And( log => log.Timestamp >= start && log.Timestamp <= end );
    }

    public static ISpec<AuditLog> WithErrors( this ISpec<AuditLog> spec ) {
        return spec.And( log => !log.Success );
    }
}

// Query audit logs
var spec = SpecBuilder<AuditLog>
    .Create()
    .ByUser( userId )
    .InDateRange( startDate, endDate )
    .OrIf( includeErrors, s => s.WithErrors() )
    .OrderDescending( log => log.Timestamp )
    .WithPagination( pagination );
```

## Advanced Usage

### Specification Composition

```csharp
// Create base specifications
var activeUsersSpec = SpecBuilder<User>
    .Create()
    .IsActive()
    .IsVerified();

var adminSpec = SpecBuilder<User>
    .Create()
    .HasRole( "Admin" );

// Compose specifications
var activeAdmins = activeUsersSpec.And( adminSpec );

// Or combine with additional filters
var recentActiveAdmins = activeAdmins
    .LastLoginAfter( DateTime.UtcNow.AddDays( -7 ) )
    .Order( u => u.LastLoginDate );
```

### Dynamic Specification Building

```csharp
public ISpec<Product> BuildProductSpec( ProductFilter filter ) {
    var spec = SpecBuilder<Product>.Create();

    if ( !string.IsNullOrEmpty( filter.SearchTerm ) ) {
        spec = spec.And( p => p.Name.Contains( filter.SearchTerm ) ||
                              p.Description.Contains( filter.SearchTerm ) );
    }

    if ( filter.Categories?.Any() == true ) {
        spec = spec.And( p => filter.Categories.Contains( p.Category ) );
    }

    if ( filter.MinPrice.HasValue ) {
        spec = spec.And( p => p.Price >= filter.MinPrice.Value );
    }

    if ( filter.MaxPrice.HasValue ) {
        spec = spec.And( p => p.Price <= filter.MaxPrice.Value );
    }

    return spec
        .Order( p => p.Name )
        .WithPagination( filter.Pagination );
}
```

### Testing Specifications

```csharp
[Fact]
public void ActiveAdultSpec_Should_Filter_Correctly() {
    // Arrange
    var person1 = new Person { Age = 25, IsActive = true };
    var person2 = new Person { Age = 17, IsActive = true };
    var person3 = new Person { Age = 30, IsActive = false };

    var spec = SpecBuilder<Person>
        .Create()
        .IsActive()
        .IsAdult();

    // Assert
    spec.IsSatisfiedBy( person1 ).Should().BeTrue();
    spec.IsSatisfiedBy( person2 ).Should().BeFalse();  // Not adult
    spec.IsSatisfiedBy( person3 ).Should().BeFalse();  // Not active
}
```

## Best Practices

### 1. Create Specification Extension Methods

Always encapsulate business logic in extension methods:

```csharp
// Good
public static ISpec<Order> IsPending( this ISpec<Order> spec ) {
    return spec.And( o => o.Status == OrderStatus.Pending );
}

// Avoid
var spec = SpecBuilder<Order>.Create().And( o => o.Status == OrderStatus.Pending );
```

### 2. Use Descriptive Names

Name specifications after business concepts, not technical operations:

```csharp
// Good
.IsEligibleForDiscount()
.RequiresApproval()
.HasExpired()

// Avoid
.CheckStatus()
.FilterByDate()
.ValidateField()
```

### 3. Keep Specifications Focused

Each specification method should represent one business rule:

```csharp
// Good
.IsActive()
.IsVerified()
.HasCompletedProfile()

// Avoid
.IsActiveAndVerifiedWithProfile()
```

### 4. Use Conditional Specifications

Leverage `AndIf` and `OrIf` for optional filters:

```csharp
var spec = SpecBuilder<Product>
    .Create()
    .InStock()
    .AndIf( !string.IsNullOrEmpty( category ), s => s.InCategory( category ) )
    .AndIf( minPrice.HasValue, s => s.MinimumPrice( minPrice.Value ) );
```

### 5. Compose Specifications for Reusability

Build complex specifications from simpler ones:

```csharp
public static ISpec<User> IsActiveSubscriber( this ISpec<User> spec ) {
    return spec
        .IsActive()
        .IsVerified()
        .HasActiveSubscription();
}
```

## Performance Considerations

- **Specifications generate Expression Trees**: All filter operations compile to SQL when used with Entity Framework
- **Lazy Evaluation**: Specifications are not executed until enumerated
- **Efficient Sorting**: Multiple sort operations are chained efficiently
- **Pagination Support**: Skip/Take operations translate directly to SQL OFFSET/FETCH

## Integration with Other Myth Libraries

### Myth.Repository

```csharp
public interface IRepository<T> {
    Task<IEnumerable<T>> FindAsync( ISpec<T> specification, CancellationToken cancellationToken = default );
}
```

### Myth.Flow

```csharp
var result = await Pipeline.Start( searchRequest )
    .Step( ctx => BuildSpecification( ctx ) )
    .StepAsync( async ( ctx, spec ) => await repository.GetAsync( spec ) )
    .ExecuteAsync();
```

### Myth.Guard

```csharp
builder.For( request.Pagination, x => x
    .Respect( p => p.PageNumber > 0 )
    .WithMessage( "Page number must be positive" ) );
```

## Contributing

Contributions are welcome! Please read the contribution guidelines before submitting pull requests.

## License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.