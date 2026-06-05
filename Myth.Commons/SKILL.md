---
name: myth-commons
description: Use when you need base Myth utilities: JSON serialization/deserialization with CamelCase/snake_case config, string manipulation, Value Objects with structural equality, Typed Constants (type-safe enum alternative), Pagination models, Global Service Provider for cross-library DI, and IScopedService<T> for scope management in singleton/hosted services.
---

# SKILL.md - Myth.Commons

**Version:** 1.0
**Target Framework:** .NET 8.0
**License:** Apache 2.0

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [API Reference](#api-reference)
  - [JSON Serialization](#json-serialization)
  - [String Extensions](#string-extensions)
  - [Value Objects](#value-objects)
  - [Constants Pattern](#constants-pattern)
  - [Pagination](#pagination)
  - [Service Provider](#service-provider)
  - [Scoped Services](#scoped-services)
  - [Extension Methods](#extension-methods)
- [Usage Examples](#usage-examples)
- [Testing](#testing)
- [Best Practices](#best-practices)

---

## Overview

Myth.Commons is a foundational .NET library providing essential utilities and patterns for enterprise applications. It includes:

- **JSON Serialization/Deserialization** with fluent configuration (CamelCase, snake_case, minification)
- **String Manipulation** utilities (transformation, search, extraction)
- **Value Objects** pattern for domain modeling with structural equality
- **Typed Constants** pattern as a type-safe alternative to enums
- **Pagination** models for paginated results
- **Global Service Provider** for cross-library dependency resolution
- **Scoped Service Pattern** for automatic scope management in handlers

---

## Installation

```bash
dotnet add package Myth.Commons
```

### Dependencies
- .NET 8.0 or higher
- System.Text.Json (built-in)
- Microsoft.AspNetCore.Mvc.Core (for attribute support)

---

## Core Concepts

### 1. Value Objects
Immutable objects compared by their values rather than identity.

```csharp
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

var addr1 = new Address("123 Main St", "NY");
var addr2 = new Address("123 Main St", "NY");
var isEqual = addr1 == addr2;  // true (structural equality)
```

### 2. Typed Constants
Type-safe alternative to enums supporting strings, integers, and other comparable types.

```csharp
public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Pending = CreateWithCallerName("P");
    public static readonly OrderStatus Processing = CreateWithCallerName("R");
    public static readonly OrderStatus Completed = CreateWithCallerName("C");

    private OrderStatus(string name, string value) : base(name, value) { }
}

// Usage
var status = OrderStatus.Pending;
string code = status;  // "P" (implicit conversion)
var found = OrderStatus.FromValue("P");  // Returns OrderStatus.Pending
```

### 3. Global Service Provider
Enables cross-library dependency resolution without tight coupling.

```csharp
// ASP.NET Core - use BuildApp() instead of Build()
var builder = WebApplication.CreateBuilder(args);
var app = builder.BuildApp();  // Initializes MythServiceProvider.Current

// Console/Background Services
var services = new ServiceCollection();
services.AddMyServices();
var provider = services.BuildServiceProvider();
MythServiceProvider.Initialize(provider);
```

---

## API Reference

### JSON Serialization

#### `JsonExtensions`
**Namespace:** `Myth.Extensions`

##### Serialization

```csharp
// Basic serialization
string json = myObject.ToJson();

// With configuration
string json = myObject.ToJson(settings => settings
    .Minify()
    .IgnoreNull()
    .UseCaseStrategy(CaseStrategy.SnakeCase)
    .UseInterfaceConverter<IMyInterface, MyImplementation>());
```

##### Deserialization

```csharp
// Safe deserialization (returns null if invalid)
MyModel? model = json.SafeFromJson<MyModel>();

// Deserialization with exception on failure
MyModel model = json.FromJsonOrThrow<MyModel>(
    HttpStatusCode.OK,
    "application/json");

// Standard deserialization
MyModel? model = json.FromJson<MyModel>();

// With custom settings
MyModel? model = json.FromJson<MyModel>(settings => settings
    .UseCaseStrategy(CaseStrategy.SnakeCase));
```

##### Validation

```csharp
bool isValid = content.IsValidJson();

if (!content.IsValidJson()) {
    throw new InvalidJsonResponseException(
        HttpStatusCode.BadRequest,
        content,
        "text/plain");
}
```

##### Global Configuration

```csharp
// Set global defaults
JsonExtensions.Configure(settings => settings
    .Minify()
    .IgnoreNull()
    .UseInterfaceConverter<IUser, User>());
```

#### `JsonSettings`
**Namespace:** `Myth.Models`

Fluent configuration for JSON serialization.

```csharp
public class JsonSettings {
    JsonSettings IgnoreNull();
    JsonSettings UseCaseStrategy(CaseStrategy strategy);
    JsonSettings Minify();
    JsonSettings UseInterfaceConverter<TInterface, TConcrete>() where TConcrete : TInterface;
    JsonSettings UseInterfaceConverter(Type interfaceType, Type concreteType);
    JsonSettings UseCustomConverter(JsonConverter converter);

    Action<JsonSerializerOptions>? OtherSettings { get; set; }

    JsonSettings Copy();
    object Clone();
}
```

**Example:**
```csharp
var json = user.ToJson(settings => {
    settings.Minify();
    settings.IgnoreNull();
    settings.UseCaseStrategy(CaseStrategy.SnakeCase);
    settings.UseInterfaceConverter<IAddress, Address>();
    settings.OtherSettings = options => {
        options.MaxDepth = 64;
    };
});
```

#### `CaseStrategy` Enum
**Namespace:** `Myth.Constants`

```csharp
public enum CaseStrategy {
    CamelCase,   // myAwesomeProperty
    SnakeCase    // my_awesome_property
}
```

---

### String Extensions

#### `StringExtension`
**Namespace:** `Myth.Extensions`

##### Transformation

```csharp
string Remove(this string value, string text);
string Minify(this string text);
string ToFirstLower(this string text);
string ToFirstUpper(this string text);
```

**Examples:**
```csharp
var text = "Hello World";

text.Remove("World");      // "Hello "
text.Minify();             // "HelloWorld" (removes all whitespace)
text.ToFirstLower();       // "hello World"
text.ToFirstUpper();       // "Hello World"
```

##### Search & Extraction

```csharp
string GetStringBetween(this string text, char startCharacter, char? endCharacter = null);
string? GetWordThatContains(this string text, string word);
string GetWordBefore(this string text, string word);
string? GetWordAfter(this string text, string word);
bool ContainsAnyOf(this string text, params string[] substrings);
bool StartsWithAnyOf(this string text, params string[] substrings);
```

**Examples:**
```csharp
var text = "Lorem ipsum dolor sit amet";

text.GetStringBetween('L', 'm');       // "ore"
text.GetWordThatContains("lor");       // "dolor"
text.GetWordBefore("sit");             // "dolor"
text.GetWordAfter("ipsum");            // "dolor"
text.ContainsAnyOf("dolor", "test");   // true (case-insensitive)
text.StartsWithAnyOf("lorem", "test"); // true (case-insensitive)
```

---

### Value Objects

#### `ValueObject` Abstract Class
**Namespace:** `Myth.ValueObjects`

Base class for implementing Value Objects with structural equality.

```csharp
public abstract class ValueObject {
    protected abstract IEnumerable<object> GetAtomicValues();

    public static bool operator ==(ValueObject left, ValueObject right);
    public static bool operator !=(ValueObject left, ValueObject right);
    public override bool Equals(object? obj);
    public override int GetHashCode();
    public ValueObject Clone();
}
```

**Implementation Example:**
```csharp
public class Money : ValueObject {
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency) {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Amount;
        yield return Currency;
    }

    // Business logic methods
    public Money Add(Money other) {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");

        return new Money(Amount + other.Amount, Currency);
    }
}
```

**Usage:**
```csharp
var price1 = new Money(100m, "USD");
var price2 = new Money(100m, "USD");
var price3 = new Money(200m, "USD");

bool areEqual = price1 == price2;       // true
bool areDifferent = price1 != price3;   // true

var total = price1.Add(price2);         // Money(200, "USD")
var cloned = price1.Clone();            // Deep copy
```

---

### Constants Pattern

#### `Constant<TSelf, TValue>` Abstract Class
**Namespace:** `Myth.ValueObjects`

Type-safe alternative to enums supporting any comparable type.

```csharp
public abstract class Constant<TSelf, TValue> : IEquatable<Constant<TSelf, TValue>>, IComparable<Constant<TSelf, TValue>>
    where TSelf : Constant<TSelf, TValue>
    where TValue : IEquatable<TValue>, IComparable<TValue> {

    // Properties
    public string Name { get; }
    public TValue Value { get; }

    // Constructor
    protected Constant(string name, TValue value);

    // Factory method with automatic name from member name
    protected static TSelf CreateWithCallerName(TValue value, [CallerMemberName] string memberName = "");

    // Static lookup methods
    public static IReadOnlyList<TSelf> GetAll();
    public static TSelf FromValue(TValue value);
    public static TSelf FromName(string name);
    public static bool TryFromValue(TValue value, out TSelf? result);
    public static bool TryFromName(string name, out TSelf? result);
    public static string GetOptions();

    // Static properties
    public static IEnumerable<TSelf> All { get; }
    public static class Values {
        public static IEnumerable<TValue> All { get; }
    }

    // Implicit conversion to value type
    public static implicit operator TValue(Constant<TSelf, TValue> constant);

    // Comparison operators
    public static bool operator ==(Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right);
    public static bool operator !=(Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right);
    public static bool operator <(Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right);
    public static bool operator >(Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right);
    public static bool operator <=(Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right);
    public static bool operator >=(Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right);
}
```

**Implementation Example:**
```csharp
public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Pending = CreateWithCallerName("P");
    public static readonly OrderStatus Processing = CreateWithCallerName("R");
    public static readonly OrderStatus Completed = CreateWithCallerName("C");
    public static readonly OrderStatus Cancelled = CreateWithCallerName("X");

    private OrderStatus(string name, string value) : base(name, value) { }
}

public class Priority : Constant<Priority, int> {
    public static readonly Priority Low = CreateWithCallerName(1);
    public static readonly Priority Medium = CreateWithCallerName(2);
    public static readonly Priority High = CreateWithCallerName(3);
    public static readonly Priority Critical = CreateWithCallerName(4);

    private Priority(string name, int value) : base(name, value) { }
}
```

**Usage:**
```csharp
// Assignment and implicit conversion
var status = OrderStatus.Pending;
string code = status;                    // "P" (implicit conversion)
string name = status.Name;               // "Pending"

// Lookup by value
var found = OrderStatus.FromValue("P");  // Returns OrderStatus.Pending
var options = OrderStatus.GetOptions();  // "P: Pending | R: Processing | C: Completed | X: Cancelled"

// Safe lookup
if (OrderStatus.TryFromValue("P", out var result)) {
    Console.WriteLine(result.Name);      // "Pending"
}

// Lookup by name
var byName = OrderStatus.FromName("Pending");

// Iteration
foreach (var status in OrderStatus.All) {
    Console.WriteLine($"{status.Value}: {status.Name}");
}

// Get all values
var allCodes = OrderStatus.Values.All;   // ["P", "R", "C", "X"]

// Comparison
if (order.Priority >= Priority.High) {
    SendUrgentNotification();
}

// Entity Framework compatible
public class Order {
    public int Id { get; set; }
    public string Status { get; set; }   // Stores the value ("P", "R", etc)
}

// Usage with EF
order.Status = OrderStatus.Pending;      // Implicit conversion to string
var status = OrderStatus.FromValue(order.Status);  // Convert back to constant
```

**Exception Handling:**
```csharp
try {
    var status = OrderStatus.FromValue("INVALID");
} catch (ConstantNotFoundException ex) {
    // Handle not found
    Console.WriteLine(ex.Message);
}
```

---

### Pagination

#### `Pagination` Value Object
**Namespace:** `Myth.ValueObjects`

Standard pagination model with ASP.NET Core query string binding support.

```csharp
public class Pagination : ValueObject {
    public Pagination();
    public Pagination(int pageNumber, int pageSize);

    [FromQuery(Name = "$pagenumber")]
    public int PageNumber { get; set; }

    [FromQuery(Name = "$pagesize")]
    public int PageSize { get; set; }

    public static readonly Pagination Default;  // Page 1, Size 10
    public static readonly Pagination All;      // Page -1, Size -1 (no pagination)
}
```

**Controller Usage:**
```csharp
[HttpGet]
public IActionResult GetUsers([FromQuery] Pagination pagination) {
    // URL: /api/users?$pagenumber=2&$pagesize=20
    // pagination.PageNumber = 2
    // pagination.PageSize = 20

    var users = _userRepository.GetPaginated(pagination);
    return Ok(users);
}
```

**Programmatic Usage:**
```csharp
var defaultPagination = Pagination.Default;        // Page 1, Size 10
var customPagination = new Pagination(2, 25);      // Page 2, Size 25
var noPagination = Pagination.All;                 // Get all items
```

#### `IPaginated` Interface
**Namespace:** `Myth.Interfaces.Results`

```csharp
public interface IPaginated {
    int PageNumber { get; }
    int PageSize { get; }
    int TotalPages { get; }
    int TotalItems { get; }
}
```

#### `IPaginated<T>` Interface
**Namespace:** `Myth.Interfaces.Results`

```csharp
public interface IPaginated<T> : IPaginated {
    IEnumerable<T> Items { get; }
}
```

#### `Paginated<TEntity>` Class
**Namespace:** `Myth.Models.Results`

Concrete implementation of paginated results.

```csharp
public class Paginated<TEntity> : IPaginated<TEntity> {
    public Paginated(
        int pageNumber,
        int pageSize,
        int totalItems,
        int totalPages,
        IEnumerable<TEntity> items);

    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalItems { get; }
    public int TotalPages { get; }
    public IEnumerable<TEntity> Items { get; }
}
```

**Usage:**
```csharp
public async Task<IPaginated<User>> GetUsersAsync(Pagination pagination) {
    var query = _context.Users.AsQueryable();
    var totalItems = await query.CountAsync();
    var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

    var items = await query
        .Skip((pagination.PageNumber - 1) * pagination.PageSize)
        .Take(pagination.PageSize)
        .ToListAsync();

    return new Paginated<User>(
        pagination.PageNumber,
        pagination.PageSize,
        totalItems,
        totalPages,
        items);
}
```

---

### Service Provider

#### `MythServiceProvider`
**Namespace:** `Myth.ServiceProvider`

Thread-safe global service provider for cross-library dependency resolution.

```csharp
public static class MythServiceProvider {
    // Current provider instance (null if not initialized)
    public static IServiceProvider? Current { get; }

    // Check if initialized
    public static bool IsInitialized { get; }

    // Try initialize (first-wins pattern, returns false if already initialized)
    public static bool TryInitialize(IServiceProvider serviceProvider);

    // Force initialize (overwrites existing)
    public static void Initialize(IServiceProvider serviceProvider);

    // Get provider or throw exception
    public static IServiceProvider GetRequired();

    // Get provider or use fallback
    public static IServiceProvider GetOrFallback(IServiceProvider? fallbackServiceProvider);

    // Reset (for testing)
    public static void Reset();
}
```

**ASP.NET Core Setup:**
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFlow();
builder.Services.AddGuard();
builder.Services.AddFlowActions(config => { /* ... */ });

// Use BuildApp() instead of Build() to initialize MythServiceProvider
var app = builder.BuildApp();

app.Run();
```

**Console/Background Service Setup:**
```csharp
var services = new ServiceCollection();
services.AddFlow();
services.AddGuard();

var serviceProvider = services.BuildServiceProvider();
MythServiceProvider.Initialize(serviceProvider);

// Now all Myth libraries can resolve dependencies
```

**Library Usage:**
```csharp
public class ExternalLibraryService {
    public void DoWork() {
        if (MythServiceProvider.IsInitialized) {
            var provider = MythServiceProvider.Current;
            var validator = provider!.GetService<IValidator>();
            // Use service
        }
    }
}
```

**Testing:**
```csharp
[Fact]
public void Test_WithCustomProvider() {
    // Reset between tests
    MythServiceProvider.Reset();

    var services = new ServiceCollection();
    services.AddSingleton<IMyService, MockService>();
    var provider = services.BuildServiceProvider();

    MythServiceProvider.Initialize(provider);

    // Test code
}
```

---

### Scoped Services

#### `IScopedService<T>`
**Namespace:** `Myth.ServiceProvider`

Interface for executing operations with automatic scope management.

```csharp
public interface IScopedService<T> where T : class {
    TResult Execute<TResult>(Func<T, TResult> operation);
    Task<TResult> ExecuteAsync<TResult>(Func<T, Task<TResult>> operation);
    void Execute(Action<T> operation);
    Task ExecuteAsync(Func<T, Task> operation);
}
```

**Setup:**
```csharp
// In Startup/Program.cs
builder.Services.AddScopedServiceProvider();

// Register your scoped services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

**Usage in Singleton Services:**
```csharp
public class OrderBackgroundService : BackgroundService {
    private readonly IScopedService<IOrderRepository> _orderRepository;
    private readonly IScopedService<IEmailService> _emailService;

    public OrderBackgroundService(
        IScopedService<IOrderRepository> orderRepository,
        IScopedService<IEmailService> emailService) {
        _orderRepository = orderRepository;
        _emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            // Automatically creates scope, executes, and disposes
            var pendingOrders = await _orderRepository.ExecuteAsync(repo =>
                repo.GetPendingOrdersAsync(stoppingToken));

            foreach (var order in pendingOrders) {
                await _emailService.ExecuteAsync(service =>
                    service.SendOrderConfirmationAsync(order, stoppingToken));
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

**Usage in Command Handlers:**
```csharp
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Guid> {
    private readonly IScopedService<IOrderRepository> _orderRepository;
    private readonly IScopedService<IProductRepository> _productRepository;

    public CreateOrderHandler(
        IScopedService<IOrderRepository> orderRepository,
        IScopedService<IProductRepository> productRepository) {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task<CommandResult<Guid>> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken) {

        // Validate products exist
        var products = await _productRepository.ExecuteAsync(repo =>
            repo.GetByIdsAsync(command.ProductIds, cancellationToken));

        if (products.Count != command.ProductIds.Count)
            return CommandResult<Guid>.Failure("Some products not found");

        // Create order
        var orderId = await _orderRepository.ExecuteAsync(async repo => {
            var order = new Order {
                CustomerId = command.CustomerId,
                Items = command.ProductIds.Select(id => new OrderItem { ProductId = id }).ToList(),
                CreatedAt = DateTime.UtcNow
            };

            await repo.AddAsync(order, cancellationToken);
            return order.Id;
        });

        return CommandResult<Guid>.Success(orderId);
    }
}
```

**Void Operations:**
```csharp
// Synchronous void
_repository.Execute(repo => {
    repo.LogAccess(userId);
});

// Asynchronous void
await _repository.ExecuteAsync(async repo => {
    await repo.LogAccessAsync(userId);
});
```

---

### Extension Methods

#### `ServiceCollectionExtensions`
**Namespace:** `Myth.Extensions`

```csharp
public static class ServiceCollectionExtensions {
    // Build WebApplication with initialized MythServiceProvider
    public static WebApplication BuildApp(this WebApplicationBuilder builder);

    // Get global provider instance
    public static IServiceProvider? GetGlobalProvider();

    // Register IScopedService<T> pattern
    public static IServiceCollection AddScopedServiceProvider(this IServiceCollection services);
}
```

#### `EnumerableExtension`
**Namespace:** `Myth.Extensions`

```csharp
public static class EnumerableExtension {
    public static string ToStringWithSeparator(this IEnumerable<string> list, string separator = ", ");
}
```

**Example:**
```csharp
var tags = new[] { "C#", ".NET", "ASP.NET" };
var result = tags.ToStringWithSeparator();        // "C#, .NET, ASP.NET"
var custom = tags.ToStringWithSeparator(" | ");   // "C# | .NET | ASP.NET"
```

#### `UrlExtension`
**Namespace:** `Myth.Extensions`

```csharp
public static class UrlExtension {
    public static object? EncodeAsUrl(this object value);
}
```

**Example:**
```csharp
var url = "https://api.example.com?query=test@value";
var encoded = url.EncodeAsUrl();  // Encodes special characters for URL
```

---

## Usage Examples

### Example 1: Complete Domain Model with Value Objects and Constants

```csharp
// Domain Constants
public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Draft = CreateWithCallerName("D");
    public static readonly OrderStatus Submitted = CreateWithCallerName("S");
    public static readonly OrderStatus Processing = CreateWithCallerName("P");
    public static readonly OrderStatus Shipped = CreateWithCallerName("H");
    public static readonly OrderStatus Delivered = CreateWithCallerName("V");
    public static readonly OrderStatus Cancelled = CreateWithCallerName("C");

    private OrderStatus(string name, string value) : base(name, value) { }
}

public class PaymentMethod : Constant<PaymentMethod, int> {
    public static readonly PaymentMethod CreditCard = CreateWithCallerName(1);
    public static readonly PaymentMethod DebitCard = CreateWithCallerName(2);
    public static readonly PaymentMethod PayPal = CreateWithCallerName(3);
    public static readonly PaymentMethod BankTransfer = CreateWithCallerName(4);

    private PaymentMethod(string name, int value) : base(name, value) { }
}

// Value Objects
public class Money : ValueObject {
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency) {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Amount;
        yield return Currency;
    }

    public Money Add(Money other) {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {other.Currency} to {Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(decimal factor) {
        return new Money(Amount * factor, Currency);
    }

    public static Money Zero(string currency) => new Money(0, currency);
}

public class Address : ValueObject {
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state, string zipCode, string country) {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }

    public override string ToString() =>
        $"{Street}, {City}, {State} {ZipCode}, {Country}";
}

// Entity
public class Order {
    public Guid Id { get; set; }
    public string Status { get; set; } = OrderStatus.Draft;
    public int PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public string TotalCurrency { get; set; } = "USD";
    public string ShippingStreet { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingState { get; set; } = string.Empty;
    public string ShippingZipCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Rich domain methods
    public OrderStatus GetStatus() => OrderStatus.FromValue(Status);

    public void SetStatus(OrderStatus status) {
        Status = status;
    }

    public PaymentMethod GetPaymentMethod() => PaymentMethod.FromValue(PaymentMethod);

    public void SetPaymentMethod(PaymentMethod method) {
        PaymentMethod = method;
    }

    public Money GetTotal() => new Money(TotalAmount, TotalCurrency);

    public void SetTotal(Money total) {
        TotalAmount = total.Amount;
        TotalCurrency = total.Currency;
    }

    public Address GetShippingAddress() => new Address(
        ShippingStreet,
        ShippingCity,
        ShippingState,
        ShippingZipCode,
        ShippingCountry);

    public void SetShippingAddress(Address address) {
        ShippingStreet = address.Street;
        ShippingCity = address.City;
        ShippingState = address.State;
        ShippingZipCode = address.ZipCode;
        ShippingCountry = address.Country;
    }

    public bool CanBeCancelled() =>
        GetStatus() == OrderStatus.Draft || GetStatus() == OrderStatus.Submitted;

    public void Cancel() {
        if (!CanBeCancelled())
            throw new InvalidOperationException($"Cannot cancel order in {GetStatus().Name} status");

        SetStatus(OrderStatus.Cancelled);
    }
}

// Usage
public class OrderService {
    public async Task<Order> CreateOrderAsync(CreateOrderRequest request) {
        var order = new Order {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        order.SetStatus(OrderStatus.Draft);
        order.SetPaymentMethod(PaymentMethod.FromValue(request.PaymentMethodId));

        var total = Money.Zero("USD");
        foreach (var item in request.Items) {
            var itemPrice = new Money(item.Price, "USD");
            total = total.Add(itemPrice.Multiply(item.Quantity));
        }
        order.SetTotal(total);

        var address = new Address(
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Country);
        order.SetShippingAddress(address);

        await _repository.AddAsync(order);

        return order;
    }

    public async Task CancelOrderAsync(Guid orderId) {
        var order = await _repository.GetByIdAsync(orderId);
        order.Cancel();
        await _repository.UpdateAsync(order);
    }
}
```

### Example 2: JSON API Client with Typed Responses

```csharp
public class ApiClient {
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient) {
        _httpClient = httpClient;

        // Configure global JSON settings
        JsonExtensions.Configure(settings => settings
            .UseCaseStrategy(CaseStrategy.SnakeCase)
            .IgnoreNull());
    }

    public async Task<TResponse> GetAsync<TResponse>(string endpoint) {
        var response = await _httpClient.GetAsync(endpoint);
        var content = await response.Content.ReadAsStringAsync();

        if (!content.IsValidJson()) {
            throw new InvalidJsonResponseException(
                response.StatusCode,
                content,
                response.Content.Headers.ContentType?.ToString());
        }

        return content.FromJsonOrThrow<TResponse>(
            response.StatusCode,
            response.Content.Headers.ContentType?.ToString());
    }

    public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data) {
        var json = data.ToJson(settings => settings
            .Minify()
            .IgnoreNull());

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        return responseContent.FromJsonOrThrow<TResponse>(
            response.StatusCode,
            response.Content.Headers.ContentType?.ToString());
    }

    public async Task<IPaginated<T>> GetPaginatedAsync<T>(
        string endpoint,
        Pagination pagination) {

        var url = $"{endpoint}?$pagenumber={pagination.PageNumber}&$pagesize={pagination.PageSize}";
        var response = await GetAsync<PaginatedResponse<T>>(url);

        return new Paginated<T>(
            response.PageNumber,
            response.PageSize,
            response.TotalItems,
            response.TotalPages,
            response.Items);
    }
}

// DTOs
public class PaginatedResponse<T> {
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public List<T> Items { get; set; } = new();
}

public class UserDto {
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
}

// Usage
var client = new ApiClient(httpClient);

// Get single resource
var user = await client.GetAsync<UserDto>("/api/users/123");

// Get paginated list
var pagination = new Pagination(2, 20);
var users = await client.GetPaginatedAsync<UserDto>("/api/users", pagination);

foreach (var u in users.Items) {
    Console.WriteLine($"{u.FullName} - {u.EmailAddress}");
}
Console.WriteLine($"Page {users.PageNumber} of {users.TotalPages}");

// Post data
var createRequest = new CreateUserRequest {
    FullName = "John Doe",
    EmailAddress = "john@example.com"
};
var created = await client.PostAsync<CreateUserRequest, UserDto>("/api/users", createRequest);
```

### Example 3: Repository with Scoped Service Pattern

```csharp
public interface IOrderRepository {
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IPaginated<Order>> GetPaginatedAsync(Pagination pagination, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
}

public class OrderRepository : IOrderRepository {
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) {
        return await _context.Orders.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IPaginated<Order>> GetPaginatedAsync(
        Pagination pagination,
        CancellationToken cancellationToken = default) {

        var query = _context.Orders.AsQueryable();
        var totalItems = await query.CountAsync(cancellationToken);

        if (pagination == Pagination.All) {
            var allItems = await query.ToListAsync(cancellationToken);
            return new Paginated<Order>(1, totalItems, totalItems, 1, allItems);
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return new Paginated<Order>(
            pagination.PageNumber,
            pagination.PageSize,
            totalItems,
            totalPages,
            items);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default) {

        return await _context.Orders
            .Where(o => o.Status == status.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default) {
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default) {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

// Background Service using IScopedService
public class OrderProcessorService : BackgroundService {
    private readonly IScopedService<IOrderRepository> _orderRepository;
    private readonly ILogger<OrderProcessorService> _logger;

    public OrderProcessorService(
        IScopedService<IOrderRepository> orderRepository,
        ILogger<OrderProcessorService> logger) {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Order Processor Service started");

        while (!stoppingToken.IsCancellationRequested) {
            try {
                // Automatically creates scope for each iteration
                var pendingOrders = await _orderRepository.ExecuteAsync(repo =>
                    repo.GetByStatusAsync(OrderStatus.Submitted, stoppingToken));

                foreach (var order in pendingOrders) {
                    await ProcessOrderAsync(order, stoppingToken);
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error processing orders");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("Order Processor Service stopped");
    }

    private async Task ProcessOrderAsync(Order order, CancellationToken cancellationToken) {
        _logger.LogInformation("Processing order {OrderId}", order.Id);

        // Each operation gets its own scope
        await _orderRepository.ExecuteAsync(async repo => {
            order.SetStatus(OrderStatus.Processing);
            await repo.UpdateAsync(order, cancellationToken);
        });

        // Simulate processing
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        await _orderRepository.ExecuteAsync(async repo => {
            order.SetStatus(OrderStatus.Shipped);
            await repo.UpdateAsync(order, cancellationToken);
        });

        _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
    }
}

// Registration in Program.cs
builder.Services.AddScopedServiceProvider();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddHostedService<OrderProcessorService>();
```

### Example 4: String Manipulation Utilities

```csharp
public class TextProcessor {
    public string ProcessMarkdown(string markdown) {
        // Extract code blocks
        var codeBlocks = new List<string>();
        var lines = markdown.Split('\n');

        foreach (var line in lines) {
            if (line.StartsWithAnyOf("```", "~~~")) {
                var language = line.GetStringBetween('`', ' ') ?? line.GetStringBetween('~', ' ');
                codeBlocks.Add(language);
            }
        }

        return codeBlocks.ToStringWithSeparator(" | ");
    }

    public string ExtractMetadata(string content) {
        // Content format: "Author: John Doe | Title: My Article | Tags: tech, programming"

        var author = content.GetWordAfter("Author:") ?? "Unknown";
        var title = content.GetWordAfter("Title:") ?? "Untitled";
        var tagsSection = content.GetWordAfter("Tags:");

        return $"{author} - {title}";
    }

    public bool ValidateUrl(string url) {
        return url.StartsWithAnyOf("http://", "https://", "ftp://");
    }

    public string SanitizeInput(string input) {
        return input
            .Remove("<script>")
            .Remove("</script>")
            .Remove("javascript:")
            .Minify();
    }

    public string FormatPropertyName(string propertyName, CaseStrategy strategy) {
        return strategy == CaseStrategy.CamelCase
            ? propertyName.ToFirstLower()
            : ConvertToSnakeCase(propertyName);
    }

    private string ConvertToSnakeCase(string input) {
        // Simple snake_case conversion
        var result = string.Concat(input.Select((x, i) =>
            i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()));
        return result.ToLower();
    }
}

// Usage
var processor = new TextProcessor();

var markdown = @"
# Title
```csharp
var x = 10;
```
```javascript
const y = 20;
```
";
var languages = processor.ProcessMarkdown(markdown);  // "csharp | javascript"

var metadata = "Author: John Doe | Title: My Article | Tags: tech, programming";
var formatted = processor.ExtractMetadata(metadata);  // "John Doe - My Article"

var isValid = processor.ValidateUrl("https://example.com");  // true

var dirty = "Hello <script>alert('xss')</script> World";
var clean = processor.SanitizeInput(dirty);  // "HelloWorld"
```

---

## Testing

### Unit Testing Example

```csharp
using Xunit;
using FluentAssertions;
using Myth.Extensions;
using Myth.ValueObjects;

public class ValueObjectTests {
    [Fact]
    public void ValueObjects_WithSameValues_ShouldBeEqual() {
        // Arrange
        var address1 = new Address("123 Main St", "New York");
        var address2 = new Address("123 Main St", "New York");

        // Act & Assert
        address1.Should().Be(address2);
        (address1 == address2).Should().BeTrue();
        address1.GetHashCode().Should().Be(address2.GetHashCode());
    }

    [Fact]
    public void ValueObjects_WithDifferentValues_ShouldNotBeEqual() {
        // Arrange
        var address1 = new Address("123 Main St", "New York");
        var address2 = new Address("456 Oak Ave", "Boston");

        // Act & Assert
        address1.Should().NotBe(address2);
        (address1 != address2).Should().BeTrue();
    }

    [Fact]
    public void Clone_ShouldCreateEqualCopy() {
        // Arrange
        var original = new Address("123 Main St", "New York");

        // Act
        var cloned = original.Clone();

        // Assert
        cloned.Should().Be(original);
        cloned.Should().NotBeSameAs(original);
    }
}

public class ConstantTests {
    [Fact]
    public void FromValue_WithValidValue_ShouldReturnConstant() {
        // Arrange & Act
        var status = OrderStatus.FromValue("P");

        // Assert
        status.Should().Be(OrderStatus.Pending);
        status.Name.Should().Be("Pending");
        status.Value.Should().Be("P");
    }

    [Fact]
    public void FromValue_WithInvalidValue_ShouldThrowException() {
        // Arrange, Act & Assert
        Assert.Throws<ConstantNotFoundException>(() =>
            OrderStatus.FromValue("INVALID"));
    }

    [Fact]
    public void TryFromValue_WithValidValue_ShouldReturnTrue() {
        // Arrange & Act
        var success = OrderStatus.TryFromValue("P", out var status);

        // Assert
        success.Should().BeTrue();
        status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void TryFromValue_WithInvalidValue_ShouldReturnFalse() {
        // Arrange & Act
        var success = OrderStatus.TryFromValue("INVALID", out var status);

        // Assert
        success.Should().BeFalse();
        status.Should().BeNull();
    }

    [Fact]
    public void GetAll_ShouldReturnAllConstants() {
        // Arrange & Act
        var all = OrderStatus.GetAll();

        // Assert
        all.Should().HaveCount(4);
        all.Should().Contain(OrderStatus.Pending);
        all.Should().Contain(OrderStatus.Processing);
    }

    [Fact]
    public void ImplicitConversion_ShouldConvertToValue() {
        // Arrange
        var status = OrderStatus.Pending;

        // Act
        string value = status;

        // Assert
        value.Should().Be("P");
    }
}

public class JsonExtensionsTests {
    [Fact]
    public void ToJson_WithMinify_ShouldRemoveWhitespace() {
        // Arrange
        var obj = new { Name = "John", Age = 30 };

        // Act
        var json = obj.ToJson(s => s.Minify());

        // Assert
        json.Should().NotContain("\n");
        json.Should().NotContain("  ");
    }

    [Fact]
    public void ToJson_WithSnakeCase_ShouldUseSnakeCase() {
        // Arrange
        var obj = new { FullName = "John Doe", EmailAddress = "john@example.com" };

        // Act
        var json = obj.ToJson(s => s.UseCaseStrategy(CaseStrategy.SnakeCase));

        // Assert
        json.Should().Contain("full_name");
        json.Should().Contain("email_address");
    }

    [Fact]
    public void FromJson_WithValidJson_ShouldDeserialize() {
        // Arrange
        var json = "{\"name\":\"John\",\"age\":30}";

        // Act
        var obj = json.FromJson<TestModel>();

        // Assert
        obj.Should().NotBeNull();
        obj!.Name.Should().Be("John");
        obj.Age.Should().Be(30);
    }

    [Fact]
    public void SafeFromJson_WithInvalidJson_ShouldReturnNull() {
        // Arrange
        var invalidJson = "not a json";

        // Act
        var obj = invalidJson.SafeFromJson<TestModel>();

        // Assert
        obj.Should().BeNull();
    }

    [Fact]
    public void FromJsonOrThrow_WithInvalidJson_ShouldThrowException() {
        // Arrange
        var invalidJson = "<html>Error</html>";

        // Act & Assert
        Assert.Throws<InvalidJsonResponseException>(() =>
            invalidJson.FromJsonOrThrow<TestModel>(HttpStatusCode.BadRequest, "text/html"));
    }

    [Fact]
    public void IsValidJson_WithValidJson_ShouldReturnTrue() {
        // Arrange
        var json = "{\"name\":\"John\"}";

        // Act & Assert
        json.IsValidJson().Should().BeTrue();
    }

    [Fact]
    public void IsValidJson_WithInvalidJson_ShouldReturnFalse() {
        // Arrange
        var invalidJson = "not json";

        // Act & Assert
        invalidJson.IsValidJson().Should().BeFalse();
    }
}

public class MythServiceProviderTests {
    [Fact]
    public void Initialize_ShouldSetCurrentProvider() {
        // Arrange
        MythServiceProvider.Reset();
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        // Act
        MythServiceProvider.Initialize(provider);

        // Assert
        MythServiceProvider.IsInitialized.Should().BeTrue();
        MythServiceProvider.Current.Should().Be(provider);
    }

    [Fact]
    public void TryInitialize_WhenNotInitialized_ShouldReturnTrue() {
        // Arrange
        MythServiceProvider.Reset();
        var provider = new ServiceCollection().BuildServiceProvider();

        // Act
        var result = MythServiceProvider.TryInitialize(provider);

        // Assert
        result.Should().BeTrue();
        MythServiceProvider.Current.Should().Be(provider);
    }

    [Fact]
    public void TryInitialize_WhenAlreadyInitialized_ShouldReturnFalse() {
        // Arrange
        MythServiceProvider.Reset();
        var provider1 = new ServiceCollection().BuildServiceProvider();
        var provider2 = new ServiceCollection().BuildServiceProvider();
        MythServiceProvider.Initialize(provider1);

        // Act
        var result = MythServiceProvider.TryInitialize(provider2);

        // Assert
        result.Should().BeFalse();
        MythServiceProvider.Current.Should().Be(provider1);
    }
}

// Test models
public class TestModel {
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
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
```

---

## Best Practices

### 1. Value Objects
- **Always override GetAtomicValues()**: Include all properties that determine equality
- **Keep immutable**: Use readonly properties and set values in constructor
- **Validate in constructor**: Ensure invariants are maintained
- **Add business methods**: Encapsulate domain logic in value object methods

```csharp
// ✅ Good
public class Money : ValueObject {
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency) {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required");

        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Amount;
        yield return Currency;
    }

    public Money Add(Money other) {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(Amount + other.Amount, Currency);
    }
}

// ❌ Bad
public class Money : ValueObject {
    public decimal Amount { get; set; }  // Mutable!
    public string Currency { get; set; } // Mutable!

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Amount;
        // Missing Currency!
    }
}
```

### 2. Typed Constants
- **Use CreateWithCallerName**: Automatically sets name from property name
- **Make constructor private**: Prevent external instantiation
- **Choose appropriate value type**: String for codes, int for priorities/levels
- **Add XML documentation**: Explain what each constant represents

```csharp
// ✅ Good
/// <summary>
/// Represents the possible statuses of an order in the system.
/// </summary>
public class OrderStatus : Constant<OrderStatus, string> {
    /// <summary>Order has been created but not yet submitted</summary>
    public static readonly OrderStatus Draft = CreateWithCallerName("D");

    /// <summary>Order has been submitted and is awaiting processing</summary>
    public static readonly OrderStatus Submitted = CreateWithCallerName("S");

    private OrderStatus(string name, string value) : base(name, value) { }
}

// ❌ Bad
public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Draft = new OrderStatus("Draft", "D");  // Manual name
    public static readonly OrderStatus Submitted = new OrderStatus("Submitted", "S");

    public OrderStatus(string name, string value) : base(name, value) { }  // Public constructor!
}
```

### 3. JSON Serialization
- **Configure globally**: Use JsonExtensions.Configure() for application-wide settings
- **Use SafeFromJson** when dealing with untrusted input
- **Use FromJsonOrThrow** in API clients for better error handling
- **Validate with IsValidJson** before processing

```csharp
// ✅ Good - Application startup
JsonExtensions.Configure(settings => settings
    .UseCaseStrategy(CaseStrategy.SnakeCase)
    .IgnoreNull());

// ✅ Good - API client
public async Task<UserDto> GetUserAsync(int id) {
    var response = await _httpClient.GetAsync($"/users/{id}");
    var content = await response.Content.ReadAsStringAsync();

    if (!content.IsValidJson()) {
        throw new InvalidJsonResponseException(
            response.StatusCode,
            content,
            response.Content.Headers.ContentType?.ToString());
    }

    return content.FromJsonOrThrow<UserDto>(
        response.StatusCode,
        response.Content.Headers.ContentType?.ToString());
}

// ❌ Bad - No error handling
public async Task<UserDto> GetUserAsync(int id) {
    var response = await _httpClient.GetAsync($"/users/{id}");
    var content = await response.Content.ReadAsStringAsync();
    return content.FromJson<UserDto>()!;  // Can throw, no context
}
```

### 4. Service Provider
- **Use BuildApp()** in ASP.NET Core applications instead of Build()
- **Initialize early** in console/background services
- **Reset in tests** to ensure clean state between tests
- **Check IsInitialized** before accessing Current

```csharp
// ✅ Good - ASP.NET Core
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMyServices();
var app = builder.BuildApp();  // Automatically initializes MythServiceProvider

// ✅ Good - Console app
var services = new ServiceCollection();
services.AddMyServices();
var provider = services.BuildServiceProvider();
MythServiceProvider.Initialize(provider);

// ✅ Good - Testing
public class MyTests : IDisposable {
    public MyTests() {
        MythServiceProvider.Reset();
        // Setup test services
    }

    public void Dispose() {
        MythServiceProvider.Reset();
    }
}

// ❌ Bad - Direct access without checking
var service = MythServiceProvider.Current.GetService<IMyService>();  // Can be null!
```

### 5. Scoped Services
- **Use IScopedService<T>** in singleton services that need scoped dependencies
- **Register with AddScopedServiceProvider()** in startup
- **Prefer direct injection** in scoped contexts (controllers, handlers)

```csharp
// ✅ Good - Background service
public class OrderProcessor : BackgroundService {
    private readonly IScopedService<IOrderRepository> _repository;

    public OrderProcessor(IScopedService<IOrderRepository> repository) {
        _repository = repository;
    }

    protected override async Task ExecuteAsync(CancellationToken ct) {
        while (!ct.IsCancellationRequested) {
            await _repository.ExecuteAsync(repo => repo.ProcessPendingOrdersAsync(ct));
            await Task.Delay(TimeSpan.FromMinutes(1), ct);
        }
    }
}

// ✅ Good - Controller (direct injection)
public class OrdersController : ControllerBase {
    private readonly IOrderRepository _repository;  // Direct injection, not IScopedService

    public OrdersController(IOrderRepository repository) {
        _repository = repository;
    }
}

// ❌ Bad - Manual scope creation
public class OrderProcessor : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactory;

    protected override async Task ExecuteAsync(CancellationToken ct) {
        while (!ct.IsCancellationRequested) {
            using var scope = _scopeFactory.CreateScope();  // Manual scope management
            var repo = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            await repo.ProcessPendingOrdersAsync(ct);
        }
    }
}
```

### 6. Pagination
- **Use Pagination.Default** for default page settings (page 1, size 10)
- **Use Pagination.All** when you need all items without paging
- **Bind to query string** with [FromQuery] in controllers
- **Calculate TotalPages** correctly in repository implementations

```csharp
// ✅ Good - Controller
[HttpGet]
public async Task<IActionResult> GetUsers([FromQuery] Pagination pagination) {
    var result = await _repository.GetPaginatedAsync(pagination);
    return Ok(result);
}

// ✅ Good - Repository
public async Task<IPaginated<User>> GetPaginatedAsync(Pagination pagination) {
    var query = _context.Users;
    var totalItems = await query.CountAsync();

    if (pagination == Pagination.All) {
        var allItems = await query.ToListAsync();
        return new Paginated<User>(1, totalItems, totalItems, 1, allItems);
    }

    var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);
    var items = await query
        .Skip((pagination.PageNumber - 1) * pagination.PageSize)
        .Take(pagination.PageSize)
        .ToListAsync();

    return new Paginated<User>(
        pagination.PageNumber,
        pagination.PageSize,
        totalItems,
        totalPages,
        items);
}

// ❌ Bad - Hardcoded values
public async Task<IActionResult> GetUsers(int page = 1, int size = 10) {
    // Should use Pagination value object
}
```

### 7. String Extensions
- **Use ToFirstLower/ToFirstUpper** for property name transformations
- **Use ContainsAnyOf/StartsWithAnyOf** for case-insensitive checks
- **Use Minify** only when whitespace needs complete removal
- **Chain methods** for complex transformations

```csharp
// ✅ Good
var propertyName = "UserId";
var camelCase = propertyName.ToFirstLower();  // "userId"

var text = "JavaScript Tutorial";
var isScripting = text.ContainsAnyOf("javascript", "python", "ruby");  // true

// ✅ Good - Chaining
var processed = input
    .Remove("<script>")
    .Remove("</script>")
    .ToFirstUpper();

// ❌ Bad - Manual case comparison
if (text.ToLower().Contains("javascript") || text.ToLower().Contains("python")) {
    // Use ContainsAnyOf instead
}
```

---

## Exceptions

### `ConstantNotFoundException`
**Namespace:** `Myth.Exceptions`

Thrown when a constant lookup fails.

```csharp
try {
    var status = OrderStatus.FromValue("INVALID");
} catch (ConstantNotFoundException ex) {
    // Handle: log, return default, throw custom exception, etc.
    _logger.LogWarning(ex, "Invalid order status: {Status}", "INVALID");
}
```

### `InvalidJsonResponseException`
**Namespace:** `Myth.Exceptions`

Thrown when HTTP response contains non-JSON content.

**Properties:**
- `StatusCode`: HTTP status code
- `RawContent`: Original response content
- `ContentType`: Response content type

```csharp
try {
    var user = await httpContent.FromJsonOrThrow<User>(HttpStatusCode.OK, "application/json");
} catch (InvalidJsonResponseException ex) {
    _logger.LogError(
        "Invalid JSON response. Status: {Status}, ContentType: {ContentType}, Content: {Content}",
        ex.StatusCode,
        ex.ContentType,
        ex.RawContent);
}
```

### `JsonParsingException`
**Namespace:** `Myth.Exceptions`

Thrown when JSON serialization/deserialization fails.

```csharp
try {
    var json = complexObject.ToJson();
} catch (JsonParsingException ex) {
    _logger.LogError(ex, "Failed to serialize object");
}
```

---

## Advanced Topics

### Custom Interface Converters

When working with interfaces in JSON, use interface converters:

```csharp
// Define interface and implementation
public interface IAddress {
    string Street { get; }
    string City { get; }
}

public class Address : IAddress {
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

// Configure globally
JsonExtensions.Configure(settings => settings
    .UseInterfaceConverter<IAddress, Address>());

// Or per-call
var json = myObject.ToJson(settings => settings
    .UseInterfaceConverter<IAddress, Address>());

// Now you can deserialize to interface
var obj = json.FromJson<MyModel>();  // MyModel has IAddress property
```

### Custom JSON Converters

For complex scenarios, use custom converters:

```csharp
public class CustomDateConverter : JsonConverter<DateTime> {
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var dateString = reader.GetString();
        return DateTime.ParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}

// Use it
var json = obj.ToJson(settings => settings
    .UseCustomConverter(new CustomDateConverter()));
```

### Extending Value Objects

Add helper methods to ValueObject base class:

```csharp
public static class ValueObjectExtensions {
    public static TValueObject? SafeClone<TValueObject>(this TValueObject? valueObject)
        where TValueObject : ValueObject {
        return valueObject?.Clone() as TValueObject;
    }
}

// Usage
var cloned = myValueObject.SafeClone();
```

---

## Troubleshooting

### Issue: MythServiceProvider.Current is null

**Cause:** Provider not initialized or BuildApp() not called.

**Solution:**
```csharp
// ASP.NET Core
var app = builder.BuildApp();  // Not builder.Build()

// Console/Background
MythServiceProvider.Initialize(serviceProvider);
```

### Issue: Constant not found exception

**Cause:** Trying to lookup constant with invalid value or name.

**Solution:** Use Try methods for safe lookup:
```csharp
if (OrderStatus.TryFromValue(code, out var status)) {
    // Use status
} else {
    // Handle invalid code
}
```

### Issue: JSON deserialization returns null

**Cause:** Invalid JSON format or type mismatch.

**Solution:** Use validation and better error handling:
```csharp
if (!content.IsValidJson()) {
    throw new InvalidJsonResponseException(statusCode, content, contentType);
}

var result = content.FromJsonOrThrow<MyModel>(statusCode, contentType);
```

### Issue: Value Object equality not working

**Cause:** GetAtomicValues() not implemented correctly.

**Solution:** Ensure all properties are included:
```csharp
protected override IEnumerable<object> GetAtomicValues() {
    yield return Property1;
    yield return Property2;
    // Include ALL properties that determine equality
}
```

---

## Performance Considerations

1. **JSON Serialization**: Use Minify() only when needed; it adds processing overhead
2. **Value Objects**: GetHashCode() is cached but Equals() compares all atomic values
3. **Constants**: Lookup is O(n) - consider caching results for repeated lookups
4. **String Extensions**: Most operations create new strings; avoid in tight loops
5. **Scoped Services**: Creates new scope per operation - appropriate overhead for isolation

---

## Thread Safety

- **MythServiceProvider**: Thread-safe initialization with lock
- **JsonExtensions.Configure**: Not thread-safe; configure once at startup
- **Value Objects**: Immutable, therefore thread-safe
- **Constants**: Immutable and cached, thread-safe

---

## Migration Guide

### From System.Text.Json directly to JsonExtensions

```csharp
// Before
var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions {
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
});

// After
var json = obj.ToJson(settings => settings
    .UseCaseStrategy(CaseStrategy.SnakeCase)
    .IgnoreNull());
```

### From enum to Constant

```csharp
// Before
public enum OrderStatus {
    Pending,
    Processing,
    Completed
}

// After
public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Pending = CreateWithCallerName("P");
    public static readonly OrderStatus Processing = CreateWithCallerName("R");
    public static readonly OrderStatus Completed = CreateWithCallerName("C");

    private OrderStatus(string name, string value) : base(name, value) { }
}
```

---

## Additional Resources

- **Repository**: https://gitlab.com/dotnet-myth/myth
- **License**: Apache 2.0
- **Target Framework**: .NET 8.0
- **NuGet Package**: Myth.Commons

---

*This documentation is maintained for AI agents and developers. For questions or contributions, please refer to the repository.*
