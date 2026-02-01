# Myth.Commons

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Commons?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Commons/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Commons?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Commons/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](README.md)

Foundation library for the Myth ecosystem providing essential utilities, value objects, global service provider management, and JSON extensions.

## 🎯 Why Myth.Commons?

Every enterprise .NET application faces the same fundamental challenges: managing cross-cutting concerns, implementing clean domain models, orchestrating dependency injection across libraries, and handling serialization consistently. **Myth.Commons solves these problems once and for all**, providing battle-tested patterns that eliminate boilerplate and accelerate development.

### The Problem

Building enterprise applications requires:
- **Cross-library integration** without tight coupling or service locator anti-patterns
- **Domain-Driven Design primitives** (Value Objects, type-safe constants) without reinventing the wheel
- **Consistent JSON handling** across HTTP APIs, message queues, and data storage
- **Scoped service management** for transient handlers accessing EF Core DbContext
- **Pagination** implemented correctly with query binding and type safety

Most teams either build these from scratch (wasting weeks) or use fragmented libraries that don't integrate well together.

### The Solution

Myth.Commons provides **production-ready building blocks** that work seamlessly together:

✅ **Global Service Provider** - Thread-safe centralized DI resolution enabling Myth libraries to work together without coupling
✅ **IScopedService Pattern** - Execute operations in managed scopes, perfect for CQRS handlers with EF Core
✅ **Value Objects & Constants** - DDD primitives with structural equality and type safety
✅ **JSON Extensions** - Fluent API with global configuration, custom converters, and consistent error handling
✅ **String & Collection Utilities** - Common operations without external dependencies

### Why Choose Myth.Commons?

| Aspect | Myth.Commons | DIY Approach | Other Libraries |
|--------|--------------|--------------|-----------------|
| **Integration** | Designed for Myth ecosystem | Manual wiring | Fragmented |
| **DDD Support** | Native Value Objects & Constants | Build from scratch | Limited or missing |
| **Service Scopes** | IScopedService pattern | Manual scope management | Not addressed |
| **Global DI** | Thread-safe MythServiceProvider | Service locator anti-pattern | Not provided |
| **JSON** | Fluent, global config, converters | System.Text.Json verbosity | Newtonsoft.Json dependency |
| **Learning Curve** | Intuitive, documented | High (design patterns) | Medium |
| **Production Ready** | Battle-tested | Untested | Varies |

### Real-World Applications

**E-Commerce Platform**
Use Value Objects for `Money`, `Address`, `ProductSku`. Type-safe `OrderStatus` constants. IScopedService for CQRS handlers accessing order repositories with EF Core.

**Financial Services**
Immutable Value Objects for `AccountNumber`, `TransactionAmount`. Global JSON configuration for consistent API responses. Pagination for transaction history.

**SaaS Multi-Tenant Systems**
Global Service Provider enables tenant-scoped services. IScopedService ensures DbContext per request. Type-safe `SubscriptionTier` constants.

**Microservices Architecture**
Shared JSON serialization settings across services. Value Objects for domain events. Global DI for cross-cutting concerns like logging and telemetry.

### Key Differentiators

🏗️ **Architecture-First Design**
Built for Clean Architecture and DDD from day one. Not an afterthought.

🔄 **Seamless Integration**
All Myth libraries use MythServiceProvider for cross-library dependency resolution without coupling.

⚡ **Developer Experience**
Fluent APIs, sensible defaults, comprehensive documentation. Get started in minutes.

🧪 **Testability**
Every pattern designed for easy mocking and unit testing. Reset global state for test isolation.

🎯 **Type Safety**
Eliminate primitive obsession with Value Objects and type-safe Constants. Compile-time checks prevent runtime errors.

📦 **Zero Lock-In**
Use only what you need. Each feature works independently or together.

### Conceptual Foundations

**Domain-Driven Design (DDD)**
Value Objects provide structural equality for domain concepts. Constants eliminate magic strings/numbers while adding domain richness.

**Dependency Injection**
Global Service Provider enables cross-library integration following Martin Fowler's "Inversion of Control" pattern without service locator anti-pattern.

**CQRS (Command Query Responsibility Segregation)**
IScopedService pattern solves the "transient handler with scoped DbContext" problem elegantly.

**Clean Architecture**
Infrastructure-independent JSON serialization. Domain models in core, technical details in outer layers.

**Fluent Interface Pattern**
JSON settings use method chaining for readable configuration: `.Minify().IgnoreNull().UseCaseStrategy(CaseStrategy.SnakeCase)`

## Features

- **Global Service Provider Management** - Thread-safe centralized service provider for cross-library integration
- **IScopedService Pattern** - Execute operations within managed service scopes
- **Value Objects** - Base classes for implementing DDD value objects and type-safe constants
- **JSON Extensions** - Fluent JSON serialization/deserialization with global configuration
- **String Utilities** - Comprehensive string manipulation extensions
- **Pagination Support** - Built-in pagination value object

## Installation

```bash
dotnet add package Myth.Commons
```

## Global Service Provider

### MythServiceProvider

Thread-safe static class for managing a global service provider instance.

#### Properties

```csharp
IServiceProvider? Current { get; }  // Current global provider or null
bool IsInitialized { get; }         // Check if initialized
```

#### Methods

```csharp
// First-wins initialization (subsequent calls ignored)
bool TryInitialize(IServiceProvider serviceProvider)

// Force initialization (overwrites existing - use with caution)
void Initialize(IServiceProvider serviceProvider)

// Get required (throws if not initialized)
IServiceProvider GetRequired()

// Get with fallback
IServiceProvider GetOrFallback(IServiceProvider? fallbackServiceProvider)

// Reset for testing
void Reset()
```

#### Example

```csharp
// ASP.NET Core - automatic initialization
var app = builder.BuildApp(); // Initializes MythServiceProvider.Current

// Console App - manual initialization
var serviceProvider = services.BuildServiceProvider();
MythServiceProvider.Initialize(serviceProvider);

// Access global provider
var provider = MythServiceProvider.Current;
var service = provider?.GetService<IMyService>();

// Or require it (throws if not initialized)
var required = MythServiceProvider.GetRequired();
```

### IScopedService<T>

Interface for executing operations within managed service scopes. Essential for transient services that need scoped dependencies.

#### Methods

```csharp
// Synchronous with result
TResult Execute<TResult>(Func<T, TResult> operation)

// Asynchronous with result
Task<TResult> ExecuteAsync<TResult>(Func<T, Task<TResult>> operation)

// Synchronous void
void Execute(Action<T> operation)

// Asynchronous void
Task ExecuteAsync(Func<T, Task> operation)
```

#### Registration

```csharp
services.AddScopedServiceProvider();

// Or automatic with BuildApp()
var app = builder.BuildApp(); // Auto-registers IScopedService<T>
```

#### Example

```csharp
public class OrderHandler : ICommandHandler<CreateOrderCommand> {
    private readonly IScopedService<IOrderRepository> _repository;

    public OrderHandler(IScopedService<IOrderRepository> repository) {
        _repository = repository;
    }

    public async Task<CommandResult> HandleAsync(CreateOrderCommand command, CancellationToken ct) {
        // Executes within a managed scope
        return await _repository.ExecuteAsync(async repo => {
            var order = await repo.CreateAsync(command.Data, ct);
            return CommandResult.Success();
        });
    }
}
```

### ServiceCollection Extensions

```csharp
// ASP.NET Core - replaces builder.Build()
WebApplication BuildApp(this WebApplicationBuilder builder)

// Get global provider
IServiceProvider? GetGlobalProvider()

// Register IScopedService<T> pattern
IServiceCollection AddScopedServiceProvider(this IServiceCollection services)
```

## Value Objects

### ValueObject

Abstract base class for implementing DDD value objects with value-based equality.

```csharp
public class Address : ValueObject {
    public string Street { get; }
    public string City { get; }
    public string Country { get; }

    public Address(string street, string city, string country) {
        Street = street;
        City = city;
        Country = country;
    }

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Street;
        yield return City;
        yield return Country;
    }
}

// Usage
var addr1 = new Address("123 Main St", "New York", "USA");
var addr2 = new Address("123 Main St", "New York", "USA");

Console.WriteLine(addr1.Equals(addr2));  // True (value equality)
Console.WriteLine(addr1 == addr2);        // True
var cloned = addr1.Clone();               // Shallow clone
```

### Constant<TSelf, TValue>

Abstract base class for creating type-safe, enumeration-like constants with rich metadata.

**Type Constraints:**
- `TSelf : Constant<TSelf, TValue>`
- `TValue : IEquatable<TValue>, IComparable<TValue>`

#### Properties

```csharp
string Name { get; }   // Constant name
TValue Value { get; }  // Constant value
```

#### Static Methods

```csharp
IReadOnlyList<TSelf> GetAll()                        // All instances
TSelf FromValue(TValue value)                        // Find by value (throws if not found)
TSelf FromName(string name)                          // Find by name (case insensitive, throws if not found)
bool TryFromValue(TValue value, out TSelf? result)   // Try find by value
bool TryFromName(string name, out TSelf? result)     // Try find by name
string GetOptions()                                  // Formatted string of all options
IEnumerable<TSelf> All { get; }                      // Enumerable of all instances
```

#### Nested Class

```csharp
static class Values {
    static IEnumerable<TValue> All { get; }  // All values only
}
```

#### Protected Methods

```csharp
// Constructor
protected Constant(string name, TValue value)

// Automatic name from caller member
protected static TSelf CreateWithCallerName(TValue value, [CallerMemberName] string memberName = "")
```

#### Operators

```csharp
implicit operator TValue(Constant<TSelf, TValue> constant)  // Implicit conversion to value
== != < > <= >=  // Comparison operators
```

#### Example

```csharp
public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Pending = CreateWithCallerName("P");
    public static readonly OrderStatus Processing = CreateWithCallerName("PR");
    public static readonly OrderStatus Completed = CreateWithCallerName("C");
    public static readonly OrderStatus Cancelled = CreateWithCallerName("X");

    private OrderStatus(string name, string value) : base(name, value) { }
}

// Usage
var status = OrderStatus.FromValue("P");              // Returns Pending
var all = OrderStatus.GetAll();                        // All 4 statuses
var allValues = OrderStatus.Values.All;                // ["P", "PR", "C", "X"]
string options = OrderStatus.GetOptions();             // "P: Pending | PR: Processing | C: Completed | X: Cancelled"

if (OrderStatus.TryFromName("pending", out var result)) {
    Console.WriteLine(result.Value);  // "P"
}

// Implicit conversion
string value = OrderStatus.Pending;  // "P"

// Comparison
var isLess = OrderStatus.Pending < OrderStatus.Completed;
```

### Pagination

Value object for pagination parameters with ASP.NET Core query binding support.

```csharp
public class Pagination : ValueObject {
    [FromQuery(Name = "$pagenumber")]
    public int PageNumber { get; set; }

    [FromQuery(Name = "$pagesize")]
    public int PageSize { get; set; }

    // Constructors
    public Pagination(int pageNumber, int pageSize)
    public Pagination()  // Defaults: pageNumber=1, pageSize=10

    // Static defaults
    static readonly Pagination Default  // pageNumber=1, pageSize=10
    static readonly Pagination All      // pageNumber=-1, pageSize=-1
}

// Usage in controller
[HttpGet]
public async Task<IActionResult> Get([FromQuery] Pagination pagination) {
    // pagination.PageNumber from query string: ?$pagenumber=2
    // pagination.PageSize from query string: ?$pagesize=20
}
```

### Paginated<TEntity>

Class for paginated results.

```csharp
public class Paginated<TEntity> : IPaginated<TEntity> {
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalItems { get; }
    public int TotalPages { get; }
    public IEnumerable<TEntity> Items { get; }

    public Paginated(
        int pageNumber,
        int pageSize,
        int totalItems,
        int totalPages,
        IEnumerable<TEntity> items)
}
```

## JSON Extensions

Fluent JSON serialization/deserialization with global configuration support.

### Global Configuration

```csharp
JsonExtensions.Configure(settings => {
    settings.CaseStrategy = CaseStrategy.SnakeCase;
    settings.IgnoreNullValues = true;
    settings.MinifyResult = true;
});
```

### Methods

```csharp
// Validation
bool IsValidJson(this string content)

// Serialization
string ToJson(this object content, Action<JsonSettings>? settings = null)

// Deserialization
TResponse? FromJson<TResponse>(this string content, Action<JsonSettings>? settings = null)
object? FromJson(this string content, Type responseType, Action<JsonSettings>? settings = null)

// Safe deserialization (returns default if invalid)
T? SafeFromJson<T>(this string content, Action<JsonSettings>? settings = null)

// Deserialize or throw with HTTP context
T FromJsonOrThrow<T>(
    this string content,
    HttpStatusCode statusCode,
    string? contentType = null,
    Action<JsonSettings>? settings = null)
```

### JsonSettings

```csharp
public class JsonSettings {
    bool MinifyResult { get; set; }              // Default: false
    bool IgnoreNullValues { get; set; }          // Default: false
    CaseStrategy CaseStrategy { get; set; }      // Default: CamelCase
    IList<JsonConverter> Converters { get; set; }
    Action<JsonSerializerOptions>? OtherSettings { get; set; }

    // Fluent methods
    JsonSettings IgnoreNull()
    JsonSettings Minify()
    JsonSettings UseCaseStrategy(CaseStrategy strategy)
    JsonSettings UseInterfaceConverter<TInterface, TConcrete>() where TConcrete : TInterface
    JsonSettings UseInterfaceConverter(Type interfaceType, Type concreteType)
    JsonSettings UseCustomConverter(JsonConverter converter)
    JsonSettings Copy()
}
```

### CaseStrategy Enum

```csharp
public enum CaseStrategy {
    CamelCase,   // myAwesomeProperty
    SnakeCase    // my_awesome_property
}
```

### Examples

```csharp
// Basic serialization
var user = new User { Name = "John", Email = "john@example.com" };
var json = user.ToJson();

// With settings
var minified = user.ToJson(s => s.Minify().IgnoreNull());

// Deserialization
var user = json.FromJson<User>();

// Safe deserialization
var user = json.SafeFromJson<User>(); // Returns null if invalid

// Validation
if (content.IsValidJson()) {
    var data = content.FromJson<Data>();
}

// With HTTP context
try {
    var user = json.FromJsonOrThrow<User>(HttpStatusCode.BadRequest);
} catch (InvalidJsonResponseException ex) {
    Console.WriteLine($"Status: {ex.StatusCode}, Content: {ex.RawContent}");
}

// Global configuration
JsonExtensions.Configure(settings => {
    settings.CaseStrategy = CaseStrategy.SnakeCase;
    settings.IgnoreNullValues = true;
    settings.UseInterfaceConverter<IUser, User>();
});

// Now all ToJson/FromJson calls use these settings
var json = user.ToJson(); // Uses snake_case and ignores nulls
```

## String Extensions

```csharp
// Remove text
string Remove(this string value, string text)

// Minify (remove all whitespace)
string Minify(this string text)

// Case transformations
string ToFirstLower(this string text)
string ToFirstUpper(this string text)

// Substring operations
string GetStringBetween(this string text, char startCharacter, char? endCharacter = null)
string? GetWordThatContains(this string text, string word)
string GetWordBefore(this string text, string word)
string? GetWordAfter(this string text, string word)

// Checks
bool ContainsAnyOf(this string text, params string[] substrings)  // Case insensitive
bool StartsWithAnyOf(this string text, params string[] substrings) // Case insensitive
```

### Examples

```csharp
// Remove
"Hello World".Remove("World");  // "Hello "

// Minify
"Hello\n  World\t!".Minify();  // "HelloWorld!"

// Case
"hello".ToFirstUpper();  // "Hello"
"HELLO".ToFirstLower();  // "hELLO"

// Between
"(value)".GetStringBetween('(', ')');  // "value"
"[data]".GetStringBetween('[');        // "data"

// Word operations
"My awesome property".GetWordThatContains("some");  // "awesome"
"Hello beautiful world".GetWordBefore("world");     // "beautiful"
"Hello beautiful world".GetWordAfter("beautiful");  // "world"

// Checks
"hello world".ContainsAnyOf("foo", "world");     // true
"hello world".StartsWithAnyOf("HELLO", "foo");   // true (case insensitive)
```

## Other Extensions

### Enumerable Extensions

```csharp
string ToStringWithSeparator(this IEnumerable<string> list, string separator = ", ")
```

Example:
```csharp
var names = new[] { "John", "Jane", "Bob" };
var result = names.ToStringWithSeparator();      // "John, Jane, Bob"
var result = names.ToStringWithSeparator(" | "); // "John | Jane | Bob"
```

### URL Extensions

```csharp
object? EncodeAsUrl(this object value)
```

Example:
```csharp
"hello world".EncodeAsUrl();  // "hello+world"
true.EncodeAsUrl();           // Boolean
123.EncodeAsUrl();            // 123
```

## Exceptions

### JsonParsingException

```csharp
public class JsonParsingException : Exception {
    public JsonParsingException(string? message, Exception? innerException)
}
```

### InvalidJsonResponseException

```csharp
public class InvalidJsonResponseException : Exception {
    public HttpStatusCode StatusCode { get; }
    public string RawContent { get; }
    public string? ContentType { get; }

    public InvalidJsonResponseException(
        HttpStatusCode statusCode,
        string rawContent,
        string? contentType = null)

    public InvalidJsonResponseException(
        HttpStatusCode statusCode,
        string rawContent,
        string? contentType,
        Exception innerException)
}
```

### ConstantNotFoundException

```csharp
public class ConstantNotFoundException : Exception {
    public ConstantNotFoundException(string message)
}
```

**Note**: This exception exists in two namespaces:
- `Myth.Exceptions.ConstantNotFoundException`
- `Myth.ValueObjects.ConstantNotFoundException`

## Interfaces

### IPaginated

```csharp
public interface IPaginated {
    int PageNumber { get; }
    int PageSize { get; }
    int TotalPages { get; }
    int TotalItems { get; }
}
```

### IPaginated<T>

```csharp
public interface IPaginated<T> : IPaginated {
    IEnumerable<T> Items { get; }
}
```

## Complete Example

```csharp
// Program.cs - ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// Configure global JSON
JsonExtensions.Configure(settings => {
    settings.CaseStrategy = CaseStrategy.SnakeCase;
    settings.IgnoreNullValues = true;
});

// Build app (initializes MythServiceProvider automatically)
var app = builder.BuildApp();

app.MapGet("/orders", async (
    [FromQuery] Pagination pagination,
    IScopedService<IOrderRepository> repository) => {

    return await repository.ExecuteAsync(async repo => {
        var orders = await repo.GetPaginatedAsync(pagination);
        return Results.Ok(orders);
    });
});

app.Run();

// Value Objects
public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Pending = CreateWithCallerName("P");
    public static readonly OrderStatus Completed = CreateWithCallerName("C");

    private OrderStatus(string name, string value) : base(name, value) { }
}

public class Address : ValueObject {
    public string Street { get; }
    public string City { get; }

    public Address(string street, string city) {
        Street = street;
        City = city;
    }

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Street;
        yield return City;
    }
}

// Repository with IScopedService
public class OrderService {
    private readonly IScopedService<IOrderRepository> _repository;

    public async Task<Order> CreateAsync(CreateOrderDto dto) {
        return await _repository.ExecuteAsync(async repo => {
            var order = dto.ToJson().FromJson<Order>();
            await repo.AddAsync(order);
            return order;
        });
    }
}
```

## License

Licensed under the Apache License 2.0. See LICENSE file for details.
