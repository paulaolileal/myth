# Myth.Commons

[![NuGet Version](https://img.shields.io/nuget/v/Myth.commons?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Commons/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.commons?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Commons/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

**Myth.Commons** is a foundational .NET library providing essential utilities and patterns for building robust, maintainable enterprise applications. It offers JSON serialization, string manipulation, DDD building blocks, and centralized service provider management for cross-library dependency resolution.

## Features

- **JSON Extensions**: Flexible serialization/deserialization with configurable settings
- **String Utilities**: Rich set of string manipulation methods
- **URL Extensions**: URL encoding helpers
- **Value Objects**: DDD base class for implementing value objects with structural equality
- **Constants**: Type-safe constants using SmartEnum pattern
- **Service Provider Management**: Global service provider for cross-library dependency resolution
- **Scoped Services**: Pattern for executing operations within automatic service scopes
- **Pagination Support**: Value objects and interfaces for paginated results
- **Collection Extensions**: Helper methods for working with enumerables

## Installation

```bash
dotnet add package Myth.Commons
```

## Table of Contents

- [JSON Extensions](#json-extensions)
- [String Extensions](#string-extensions)
- [URL Extensions](#url-extensions)
- [Value Objects](#value-objects)
- [Constants](#constants)
- [Service Provider Management](#service-provider-management)
- [Scoped Services](#scoped-services)
- [Pagination](#pagination)
- [Collection Extensions](#collection-extensions)

## JSON Extensions

Powerful JSON serialization and deserialization with System.Text.Json, featuring global configuration, custom converters, and flexible naming strategies.

### Basic Usage

```csharp
using Myth.Extensions;

// Serialize to JSON
var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };
var json = user.ToJson();
// {"id":1,"name":"John Doe","email":"john@example.com"}

// Deserialize from JSON
var userObj = json.FromJson<User>();
```

### Global Configuration

Configure JSON settings globally for your entire application:

```csharp
JsonExtensions.Configure( settings => settings
    .UseCaseStrategy( CaseStrategy.SnakeCase )
    .IgnoreNull()
    .Minify()
);

var json = user.ToJson();
// {"id":1,"name":"John Doe","email":"john@example.com"}
```

### Per-Operation Configuration

Override global settings for specific operations:

```csharp
// Use snake_case for this operation only
var json = user.ToJson( settings => settings
    .UseCaseStrategy( CaseStrategy.SnakeCase )
);
// {"id":1,"name":"john doe","email":"john@example.com"}

// Minify JSON output
var compactJson = user.ToJson( settings => settings.Minify() );

// Ignore null values
var jsonWithoutNulls = user.ToJson( settings => settings.IgnoreNull() );
```

### Interface to Concrete Type Converters

Handle interfaces and abstract types during serialization/deserialization:

```csharp
// Using generic converter
var json = user.ToJson( settings => settings
    .UseInterfaceConverter<IAddress, Address>()
);

// Using non-generic converter
var json = user.ToJson( settings => settings
    .UseInterfaceConverter( typeof( IAddress ), typeof( Address ) )
);
```

### Custom JSON Converters

Add custom System.Text.Json converters:

```csharp
var json = user.ToJson( settings => settings
    .UseCustomConverter( new CustomDateTimeConverter() )
);
```

### Advanced JSON Settings

Access underlying JsonSerializerOptions for fine-grained control:

```csharp
var json = user.ToJson( settings => {
    settings.IgnoreNull().Minify();
    settings.OtherSettings = options => {
        options.MaxDepth = 64;
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    };
} );
```

### Case Strategies

Two naming conventions are supported:

```csharp
public enum CaseStrategy {
    CamelCase,  // myAwesomeProperty
    SnakeCase   // my_awesome_property
}
```

### Dynamic Object Support

Deserialize to dynamic objects:

```csharp
var json = "{\"name\":\"John\",\"age\":30}";
dynamic obj = json.FromJson<object>();
Console.WriteLine( obj.name ); // John
```

### Exception Handling

All JSON operations throw `JsonParsingException` on failure:

```csharp
try {
    var obj = invalidJson.FromJson<User>();
} catch ( JsonParsingException ex ) {
    Console.WriteLine( $"JSON parsing failed: {ex.Message}" );
    Console.WriteLine( $"Inner exception: {ex.InnerException?.Message}" );
}
```

## String Extensions

Rich set of utilities for string manipulation and analysis.

```csharp
using Myth.Extensions;

// Remove text
var result = "Hello World".Remove( "World" ); // "Hello "

// Minify (remove all whitespace)
var minified = "Hello   World\n\t".Minify(); // "HelloWorld"

// Change case of first letter
var lower = "Hello".ToFirstLower(); // "hello"
var upper = "hello".ToFirstUpper(); // "Hello"

// Extract text between characters
var text = "The 'quick' brown fox";
var extracted = text.GetStringBetween( '\'' ); // "quick"

// Find words
var sentence = "The quick brown fox";
var word = sentence.GetWordThatContains( "qui" ); // "quick"
var before = sentence.GetWordBefore( "brown" ); // "quick"
var after = sentence.GetWordAfter( "quick" ); // "brown"

// Search operations
var hasAny = "Hello World".ContainsAnyOf( "Hi", "Hello", "Hey" ); // true
var startsWithAny = "Hello World".StartsWithAnyOf( "Hi", "Hello" ); // true
```

## URL Extensions

Encode objects for URL usage:

```csharp
using Myth.Extensions;

var text = "Hello World";
var encoded = text.EncodeAsUrl(); // "Hello+World"

var flag = true;
var encodedFlag = flag.EncodeAsUrl(); // true (as boolean)
```

## Value Objects

Base class for implementing Domain-Driven Design value objects with structural equality.

```csharp
using Myth.ValueObjects;

public class Address : ValueObject {
    public string Street { get; }
    public string City { get; }
    public string ZipCode { get; }

    public Address( string street, string city, string zipCode ) {
        Street = street;
        City = city;
        ZipCode = zipCode;
    }

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Street;
        yield return City;
        yield return ZipCode;
    }
}

// Value objects are compared by their values, not reference
var address1 = new Address( "123 Main St", "Springfield", "12345" );
var address2 = new Address( "123 Main St", "Springfield", "12345" );
var address3 = new Address( "456 Oak Ave", "Springfield", "12345" );

Console.WriteLine( address1 == address2 ); // true (same values)
Console.WriteLine( address1 == address3 ); // false (different values)

// Clone value objects
var clone = address1.Clone();
```

### Value Object Benefits

- **Immutability**: Encourages immutable design patterns
- **Equality by Value**: Automatically handles equality comparison based on properties
- **DDD Alignment**: Perfect for domain modeling and tactical DDD patterns
- **Type Safety**: Prevents primitive obsession

## Constants

Type-safe constants using Ardalis.SmartEnum pattern.

```csharp
using Myth.ValueObjects;

public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Pending = new( nameof( Pending ), "PENDING" );
    public static readonly OrderStatus Processing = new( nameof( Processing ), "PROCESSING" );
    public static readonly OrderStatus Completed = new( nameof( Completed ), "COMPLETED" );
    public static readonly OrderStatus Cancelled = new( nameof( Cancelled ), "CANCELLED" );

    private OrderStatus( string name, string value ) : base( name, value ) { }
}

// Usage
var status = OrderStatus.Pending;
string statusValue = status; // Implicit conversion to "PENDING"

// Get from value
var status2 = OrderStatus.FromValue( "PROCESSING" ); // OrderStatus.Processing

// Get from name
var status3 = OrderStatus.FromName( "Completed" ); // OrderStatus.Completed

// List all options
var options = OrderStatus.GetOptions();
// "(Pending): PENDING | (Processing): PROCESSING | (Completed): COMPLETED | (Cancelled): CANCELLED"

// Switch on constants (exhaustive)
var message = status switch {
    var s when s == OrderStatus.Pending => "Order is pending",
    var s when s == OrderStatus.Processing => "Order is being processed",
    var s when s == OrderStatus.Completed => "Order is completed",
    var s when s == OrderStatus.Cancelled => "Order was cancelled",
    _ => throw new InvalidOperationException()
};
```

### Constant Benefits

- **Type Safety**: Compile-time safety instead of magic strings/numbers
- **IntelliSense Support**: IDE autocomplete for all values
- **Pattern Matching**: Works beautifully with C# switch expressions
- **Listing**: Easy enumeration of all values
- **Extensibility**: Add methods and properties to constants

## Service Provider Management

Global service provider management enables cross-library dependency resolution without coupling libraries together.

### ASP.NET Core Applications

Use `BuildApp()` instead of `Build()` to automatically initialize the global service provider:

```csharp
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddFlow();
builder.Services.AddGuard();
builder.Services.AddFlowActions( config => { ... } );

var app = builder.BuildApp(); // Instead of builder.Build()

app.UseGuard();
app.Run();
```

### Console Applications / Background Services

Use the global service provider for non-web applications:

```csharp
var services = new ServiceCollection();
services.AddFlow();
services.AddGuard();
services.AddMyServices();

var serviceProvider = services.BuildServiceProvider();
MythServiceProvider.Initialize( serviceProvider );

// Now all libraries can resolve dependencies
var pipeline = Pipeline.Start( context );
```

### Accessing Global Service Provider

```csharp
using Myth.ServiceProvider;

// Check if initialized
if ( MythServiceProvider.IsInitialized ) {
    var provider = MythServiceProvider.Current;
}

// Get or throw if not initialized
var requiredProvider = MythServiceProvider.GetRequired();

// Get with fallback
var provider = MythServiceProvider.GetOrFallback( localServiceProvider );

// Try initialize (first-wins pattern)
var initialized = MythServiceProvider.TryInitialize( serviceProvider );

// Force initialize (overwrites existing)
MythServiceProvider.Initialize( serviceProvider );
```

### External Library Integration

External libraries can access registered services:

```csharp
public class ThirdPartyLibrary {
    public void DoSomething() {
        var provider = ServiceCollectionExtensions.GetGlobalProvider();
        var validator = provider?.GetService<IValidator>();
        if ( validator != null ) {
            // Use Myth libraries without direct coupling
        }
    }
}
```

### Testing Support

Reset the global provider for isolated unit tests:

```csharp
[Fact]
public void TestWithCleanProvider() {
    MythServiceProvider.Reset();

    var services = new ServiceCollection();
    // ... configure test services
    var provider = services.BuildServiceProvider();
    MythServiceProvider.Initialize( provider );

    // Run test
}
```

## Scoped Services

Pattern for executing operations within automatic service scopes, perfect for transient handlers accessing scoped dependencies like repositories with DbContext.

### Setup

Register the scoped service provider pattern once in your application:

```csharp
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddScopedServiceProvider();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddDbContext<AppDbContext>();

var app = builder.BuildApp();
```

### Usage in Handlers

```csharp
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand> {
    private readonly IScopedService<IOrderRepository> _repository;
    private readonly IScopedService<IEmailService> _emailService;

    public CreateOrderHandler(
        IScopedService<IOrderRepository> repository,
        IScopedService<IEmailService> emailService ) {
        _repository = repository;
        _emailService = emailService;
    }

    public async Task<CommandResult> HandleAsync(
        CreateOrderCommand command,
        CancellationToken ct ) {

        // Execute with automatic scope management
        var order = await _repository.ExecuteAsync( repo =>
            repo.CreateAsync( command.OrderData, ct )
        );

        // Execute void operations
        await _emailService.ExecuteAsync( email =>
            email.SendOrderConfirmationAsync( order.Id, ct )
        );

        return CommandResult.Success();
    }
}
```

### Synchronous Operations

```csharp
// With return value
var result = _scopedService.Execute( service =>
    service.GetData()
);

// Void operation
_scopedService.Execute( service =>
    service.ProcessData()
);
```

### Asynchronous Operations

```csharp
// With return value
var result = await _scopedService.ExecuteAsync( service =>
    service.GetDataAsync()
);

// Void operation
await _scopedService.ExecuteAsync( service =>
    service.ProcessDataAsync()
);
```

### Benefits

- **Automatic Scope Management**: No manual scope creation or disposal
- **Lifetime Safety**: Access scoped services from transient contexts safely
- **Clean API**: Strongly-typed, fluent interface
- **Proper Disposal**: Handles both sync and async disposal correctly
- **DDD Alignment**: Perfect for CQRS handlers accessing repositories

## Pagination

Value objects and interfaces for implementing paginated results.

### Pagination Value Object

```csharp
using Myth.ValueObjects;

// Default pagination (page 1, size 10)
var pagination = Pagination.Default;

// Custom pagination
var customPagination = new Pagination( pageNumber: 2, pageSize: 20 );

// Get all items (single page)
var allItems = Pagination.All;

// ASP.NET Core automatic binding
[HttpGet]
public IActionResult GetOrders( [FromQuery] Pagination pagination ) {
    // Automatically binds from query string: ?$pagenumber=2&$pagesize=20
}
```

### Paginated Results

```csharp
using Myth.Interfaces.Results;
using Myth.Models.Results;

// Create paginated result
var items = await repository.GetOrdersAsync( pagination );
var total = await repository.GetTotalCountAsync();
var totalPages = ( int )Math.Ceiling( ( double )total / pagination.PageSize );

var result = new Paginated<Order>(
    pageNumber: pagination.PageNumber,
    pageSize: pagination.PageSize,
    totalItems: total,
    totalPages: totalPages,
    items: items
);

// Access properties
Console.WriteLine( $"Page {result.PageNumber} of {result.TotalPages}" );
Console.WriteLine( $"Showing {result.Items.Count()} of {result.TotalItems} items" );

// Return in API
return Ok( result );
```

### IPaginated Interface

Implement custom paginated types:

```csharp
public class CustomPaginatedResult<T> : IPaginated<T> {
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public IEnumerable<T> Items { get; set; }

    // Add custom properties
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
```

## Collection Extensions

Helper methods for working with collections.

```csharp
using Myth.Extensions;

var items = new[] { "apple", "banana", "cherry" };

// Join with separator
var result = items.ToStringWithSeparator( ", " );
// "apple, banana, cherry"

// Custom separator
var result2 = items.ToStringWithSeparator( " | " );
// "apple | banana | cherry"

// Default separator (", ")
var result3 = items.ToStringWithSeparator();
// "apple, banana, cherry"
```

## Architecture Patterns

### Domain-Driven Design

Myth.Commons provides essential DDD building blocks:

- **Value Objects**: Implement domain value objects with structural equality
- **Constants**: Type-safe domain constants and enumerations
- **Pagination**: Domain model for paginated queries

```csharp
// Value Object for Money
public class Money : ValueObject {
    public decimal Amount { get; }
    public string Currency { get; }

    public Money( decimal amount, string currency ) {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Amount;
        yield return Currency;
    }

    public Money Add( Money other ) {
        if ( Currency != other.Currency )
            throw new InvalidOperationException( "Cannot add money with different currencies" );

        return new Money( Amount + other.Amount, Currency );
    }
}

// Type-safe constants
public class Currency : Constant<Currency, string> {
    public static readonly Currency USD = new( nameof( USD ), "USD" );
    public static readonly Currency EUR = new( nameof( EUR ), "EUR" );
    public static readonly Currency BRL = new( nameof( BRL ), "BRL" );

    private Currency( string name, string value ) : base( name, value ) { }
}
```

### CQRS Integration

Perfect for CQRS patterns with scoped service management:

```csharp
public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, IPaginated<Order>> {
    private readonly IScopedService<IOrderRepository> _repository;

    public GetOrdersQueryHandler( IScopedService<IOrderRepository> repository ) {
        _repository = repository;
    }

    public async Task<QueryResult<IPaginated<Order>>> HandleAsync(
        GetOrdersQuery query,
        CancellationToken ct ) {

        var result = await _repository.ExecuteAsync( async repo => {
            var items = await repo.GetOrdersAsync( query.Pagination, ct );
            var total = await repo.GetTotalCountAsync( ct );

            return new Paginated<Order>(
                query.Pagination.PageNumber,
                query.Pagination.PageSize,
                total,
                ( int )Math.Ceiling( ( double )total / query.Pagination.PageSize ),
                items
            );
        } );

        return QueryResult<IPaginated<Order>>.Success( result );
    }
}
```

### Clean Architecture

Supports Clean Architecture principles:

- **Infrastructure Independence**: JSON serialization without external dependencies
- **Framework Independence**: Works with any .NET application type
- **Testability**: Easy to mock and test with clear interfaces
- **Separation of Concerns**: Each utility focused on single responsibility

## Best Practices

### JSON Serialization

1. Configure global JSON settings once at application startup
2. Use per-operation settings only when needed
3. Handle `JsonParsingException` for robust error handling
4. Use interface converters for polymorphic types

### Value Objects

1. Make value objects immutable
2. Override `GetAtomicValues()` to include all properties that define equality
3. Consider validation in constructor
4. Use for domain concepts, not just data transfer

### Constants

1. Use for domain-specific enumerations
2. Prefer constants over magic strings/numbers
3. Add domain methods to constant classes
4. Use with pattern matching for exhaustive checks

### Service Provider

1. Initialize global provider once at application startup
2. Use `BuildApp()` for ASP.NET Core applications
3. Use `TryInitialize()` for library code (first-wins pattern)
4. Reset provider in tests for isolation

### Scoped Services

1. Register `AddScopedServiceProvider()` once per application
2. Use for accessing scoped dependencies from transient contexts
3. Perfect for CQRS handlers and background services
4. Automatic disposal - no manual scope management needed

## Dependencies

- **Ardalis.SmartEnum** 8.0.0 - For type-safe constants
- **Microsoft.Extensions.DependencyInjection** 8.0.0 - For DI support
- **System.Text.Json** 8.0.5 - For JSON operations

## Requirements

- .NET 8.0 or later

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) for details.

## Support

For issues, questions, or contributions, please visit the [GitLab repository](https://gitlab.com/dotnet-myth/myth/-/tree/main/Myth.Commons).
