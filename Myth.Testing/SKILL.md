---
name: myth-testing
description: Use when writing xUnit tests in the Myth ecosystem. BaseTests provides DI with AddService<T>()/ReplaceService<T>(), Faker (_faker) for test data, and configuration helpers. BaseDatabaseTests<TContext> adds EF Core In-Memory database with auto setup/cleanup. Includes HTTP mocking, FluentAssertions extensions for API responses, and Moq.
---

# Myth.Testing - Testing Utilities and Base Classes

## Overview

Myth.Testing is a comprehensive testing library built on xUnit that provides pre-configured base classes, utilities, and patterns to eliminate testing boilerplate. It combines Bogus (Faker), FluentAssertions, Moq, Entity Framework In-Memory, and ASP.NET TestHost into a cohesive, easy-to-use testing framework.

**Key Features:**
- **Base Test Classes**: `BaseTests` and `BaseDatabaseTests<T>` with auto-configured dependency injection
- **Test Data Generation**: Integrated Bogus (Faker) for realistic test data
- **Entity Framework Testing**: In-memory database with automatic setup/cleanup
- **HTTP Client Mocking**: Built-in HTTP mocking for external API testing
- **FluentAssertions Extensions**: Enhanced assertion methods for MVC/API responses
- **Async Testing Utilities**: Timeout management, exception testing, retry patterns
- **Test Fixtures**: Shared resources for expensive setup across multiple tests
- **Configuration Management**: In-memory configuration for isolated testing
- **Service Container**: Full DI support with service management utilities

**Dependencies:**
- .NET 10.0+
- xUnit 2.9.3
- FluentAssertions 8.8.0
- Bogus 35.6.5
- Moq 4.20.72
- EF Core In-Memory 10.0.3
- ASP.NET Core TestHost 10.0.3

---

## Installation

```bash
dotnet add package Myth.Testing
```

All dependencies (xUnit, FluentAssertions, Bogus, Moq, EF Core In-Memory) are included automatically.

---

## API Reference

### BaseTests

Base class for all unit tests with DI, configuration, and Bogus integration.

```csharp
public abstract class BaseTests : IDisposable {
    // Protected fields
    protected readonly Faker _faker; // Pre-configured Bogus instance
    protected IServiceProvider ServiceProvider { get; }
    protected IConfiguration Configuration { get; }

    // Service Management
    protected void AddService<TInterface>(TInterface instance);
    protected void AddService<TInterface, TImplementation>() where TImplementation : class, TInterface;
    protected void AddService<TInterface, TImplementation>(ServiceLifetime lifetime) where TImplementation : class, TInterface;
    protected void ReplaceService<TInterface>(TInterface newInstance);
    protected T GetRequiredService<T>();
    protected T GetService<T>();
    protected bool IsServiceRegistered<TService>();
    protected virtual void ConfigureServices(IServiceCollection services);

    // Configuration
    protected void AddConfigurationItem(string key, string value);
    protected void AddConfigurationSection(string sectionName, Dictionary<string, string> values);

    // Scopes
    protected IServiceScope CreateScope();

    // Lifecycle
    protected virtual void Dispose();
}
```

### BaseDatabaseTests<TContext>

Extends `BaseTests` with Entity Framework Core in-memory database support.

```csharp
public abstract class BaseDatabaseTests<TContext> : BaseTests, IAsyncLifetime
    where TContext : DbContext {

    // Database Lifecycle (OPTIONAL - called automatically when needed)
    Task InitializeDatabaseAsync();
    Task CleanupDatabaseAsync();
    TContext GetContext();

    // Inherited from BaseTests:
    // All service management, configuration, and Faker functionality
}
```

**Note:** `InitializeDatabaseAsync()` and `CleanupDatabaseAsync()` are **optional** - you can call them manually for explicit control, but they are not required in every test method.

---

### BaseMongoDbTests<TContext>

Extends `BaseTests` with **MongoDB** in-memory database support using EphemeralMongo.

```csharp
public abstract class BaseMongoDbTests<TContext> : BaseTests
    where TContext : DbContext {

    protected string DatabaseName { get; }

    // Database Lifecycle (AUTOMATIC - no manual calls needed)
    TContext GetContext();
    Task<TContext> GetContextAsync();
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // Inherited from BaseTests:
    // All service management, configuration, and Faker functionality
}
```

**Key Differences from BaseDatabaseTests:**
- Uses **real MongoDB** instance in memory (not EF In-Memory provider)
- Database initialization is **automatic** when you call `GetContext()` or `GetContextAsync()`
- No need to manually call `InitializeDatabaseAsync()` or `CleanupDatabaseAsync()`
- Change tracker is automatically cleared for fresh data in each test
- Cleanup happens automatically on `Dispose()/DisposeAsync()`

**When to Use:**
- Testing MongoDB-specific features (MongoDB.Driver)
- Testing MongoDB Entity Framework Provider
- Integration tests that require real MongoDB behavior

### TestFixture

Base class for shared fixtures across multiple test classes.

```csharp
public abstract class TestFixture : IDisposable {
    protected readonly IServiceProvider ServiceProvider;
    protected readonly Faker Faker;

    protected TestFixture();
    protected abstract void ConfigureServices(IServiceCollection services);

    public T GetRequiredService<T>();
    public T GetService<T>();
    public virtual void Dispose();
}
```

### ModelMock<TModel>

Base class for generating mock model entities for testing with Bogus.

```csharp
public abstract class ModelMock<TModel> where TModel : class {
    protected readonly Faker _faker;

    // Generate multiple entities
    public abstract IEnumerable<TModel> Generate(
        int amount,
        IDictionary<string, object>? metadata = null);

    // Generate single entity
    public virtual TModel Generate(IDictionary<string, object>? metadata = null);
}
```

**Usage:**
```csharp
public class UserMock : ModelMock<User> {
    public UserMock(Faker faker) : base(faker) { }

    public override IEnumerable<User> Generate(int amount, IDictionary<string, object>? metadata = null) {
        var users = new List<User>();
        for (int i = 0; i < amount; i++) {
            users.Add(new User {
                Id = _faker.Random.Guid(),
                Name = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                Age = _faker.Random.Int(18, 65)
            });
        }
        return users;
    }
}

// In test
var userMock = new UserMock(_faker);
var user = userMock.Generate();
var users = userMock.Generate(10);
```

---

### ModelMockAsync<TContext, TModel>

Base class for generating and persisting mock model entities to database for testing.

```csharp
public abstract class ModelMockAsync<TContext, TModel>
    where TContext : DbContext
    where TModel : class {

    protected readonly Faker _faker;
    protected readonly TContext _context;
    protected DbSet<TModel> _collection;

    // Generate and persist multiple entities
    public abstract Task<IEnumerable<TModel>> GenerateAsync(
        int amount,
        IDictionary<string, object>? metadata = null);

    // Generate and persist single entity
    public virtual Task<TModel> GenerateAsync(
        IDictionary<string, object>? metadata = null);

    // Save changes to database
    protected virtual Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**Usage:**
```csharp
public class UserMockAsync : ModelMockAsync<AppDbContext, User> {
    public UserMockAsync(AppDbContext context, Faker faker)
        : base(context, faker) { }

    public override async Task<IEnumerable<User>> GenerateAsync(
        int amount,
        IDictionary<string, object>? metadata = null) {

        var users = new List<User>();
        for (int i = 0; i < amount; i++) {
            var user = new User {
                Id = _faker.Random.Guid(),
                Name = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                Age = _faker.Random.Int(18, 65),
                IsActive = true
            };
            users.Add(user);
            await _collection.AddAsync(user);
        }

        await SaveChangesAsync();
        return users;
    }
}

// In test
var context = GetContext();
var userMock = new UserMockAsync(context, _faker);
var user = await userMock.GenerateAsync();
var users = await userMock.GenerateAsync(10);
```

**Benefits:**
- Automatic database persistence
- Consistent test data generation
- Reusable across multiple tests
- Support for metadata-based customization

---

### HttpClientMock

HTTP client mocking utilities for testing external API dependencies.

```csharp
public static class HttpClientMock {
    // Single endpoint mock
    static HttpClient CreateClient(Action<HttpClientSettings> configure);

    // Multiple endpoints mock
    static HttpClient CreateClientWithEndpoints(params Action<HttpClientSettings>[] configurations);
}

public class HttpClientSettings {
    HttpClientSettings ForRoute(string route);
    HttpClientSettings UsingGet();
    HttpClientSettings UsingPost();
    HttpClientSettings UsingPut();
    HttpClientSettings UsingPatch();
    HttpClientSettings UsingDelete();

    HttpClientSettings RespondWithSuccess(); // 200 OK
    HttpClientSettings RespondWith(HttpStatusCode statusCode);
    HttpClientSettings WithJsonResponse(object response);
    HttpClientSettings WithRequestValidation(Func<HttpRequestMessage, bool> validator);
}
```

### Async Testing Extensions

```csharp
public static class TestExtensions {
    // Timeout testing
    static Task WithTimeoutAsync(
        Func<Task> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken = default);

    // Exception testing
    static Task<T> AssertThrowsAsync<T>(Func<Task> operation) where T : Exception;
    static Task<Exception> AssertThrowsAnyAsync<Exception>(Func<Task> operation);
    static Task AssertDoesNotThrowAsync(Func<Task> operation);

    // Retry testing
    static Task WithRetryAsync(
        Func<Task> operation,
        int maxAttempts,
        TimeSpan delay);

    // Parallel testing
    static Task<T[]> WhenAllAsync<T>(IEnumerable<Task<T>> tasks);
}
```

### FluentAssertions Extensions

```csharp
// HTTP Response Assertions
response.Should().BeStatusCodeOk();
response.Should().BeStatusCodeCreated();
response.Should().BeStatusCodeNoContent();
response.Should().BeStatusCodeNotFound();
response.Should().BeStatusCodeBadRequest();
response.Should().BeSuccessStatusCode();
response.Should().HaveContentType(string contentType);
response.Should().HaveHeader(string headerName);

// Extract response data
var result = response.GetAs<T>();
```

---

## Usage Examples

### 1. Basic Unit Test

```csharp
public class UserServiceTests : BaseTests {
    private readonly UserService _service;
    private readonly Mock<IUserRepository> _mockRepository;

    public UserServiceTests() {
        // Setup mocks
        _mockRepository = new Mock<IUserRepository>();

        // Register services
        AddService<IUserRepository>(_mockRepository.Object);
        AddService<UserService, UserService>();

        _service = GetRequiredService<UserService>();
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldSucceed() {
        // Arrange - Use _faker for test data
        var user = new User {
            Id = _faker.Random.Guid(),
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Age = _faker.Random.Int(18, 65)
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _service.CreateUserAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be(user.Name);

        _mockRepository.Verify(r => r.CreateAsync(user, default), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task CreateUser_WithInvalidName_ShouldThrow(string invalidName) {
        // Arrange
        var user = new User {
            Id = _faker.Random.Guid(),
            Name = invalidName,
            Email = _faker.Internet.Email()
        };

        // Act & Assert
        await _service.Invoking(s => s.CreateUserAsync(user))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
    }
}
```

### 2. Database Test (EF Core In-Memory)

```csharp
public class UserRepositoryTests : BaseDatabaseTests<UserDbContext> {
    private readonly UserRepository _repository;

    public UserRepositoryTests() {
        AddService<UserRepository, UserRepository>();
        _repository = GetRequiredService<UserRepository>();
    }

    [Fact]
    public async Task CreateUser_ShouldPersistToDatabase() {
        // Arrange - initialization is optional, called automatically if needed
        var user = new UserEntity {
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        // Verify persistence
        var dbUser = await GetContext().Users.FindAsync(result.Id);
        dbUser.Should().NotBeNull();
        dbUser.Name.Should().Be(user.Name);

        // Cleanup is optional - automatically handled by xUnit disposal
    }

    [Fact]
    public async Task GetUsers_WithFilters_ShouldReturnFilteredResults() {
        // Arrange - Seed data
        var users = new List<UserEntity>();
        for (int i = 0; i < 10; i++) {
            users.Add(new UserEntity {
                Name = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                IsActive = i % 2 == 0,
                CreatedAt = DateTime.UtcNow
            });
        }

        await GetContext().Users.AddRangeAsync(users);
        await GetContext().SaveChangesAsync();

        // Act
        var activeUsers = await _repository.GetActiveUsersAsync();

        // Assert
        activeUsers.Should().HaveCount(5);
        activeUsers.Should().OnlyContain(u => u.IsActive);
    }
}
```

---

### 2.5. MongoDB Database Test

```csharp
public class OrderRepositoryMongoTests : BaseMongoDbTests<OrderDbContext> {
    private readonly OrderRepository _repository;

    public OrderRepositoryMongoTests() {
        AddService<OrderRepository, OrderRepository>();
        _repository = GetRequiredService<OrderRepository>();
    }

    [Fact]
    public async Task CreateOrder_ShouldPersistToMongoDB() {
        // Arrange - database auto-initializes on first GetContext() call
        var order = new Order {
            CustomerId = _faker.Random.Guid(),
            Items = new List<OrderItem> {
                new() {
                    ProductId = _faker.Random.Guid(),
                    Quantity = _faker.Random.Int(1, 10),
                    Price = _faker.Random.Decimal(10, 1000)
                }
            },
            Status = OrderStatus.Pending
        };

        // Act
        var context = await GetContextAsync(); // Auto-initializes database
        await context.Orders.AddAsync(order);
        await SaveChangesAsync(); // Use provided save method

        // Assert
        var dbOrder = await context.Orders
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        dbOrder.Should().NotBeNull();
        dbOrder!.Items.Should().HaveCount(1);
        dbOrder.Status.Should().Be(OrderStatus.Pending);

        // No manual cleanup needed - automatic on disposal
    }

    [Fact]
    public async Task GetOrders_WithSpecification_ShouldFilterCorrectly() {
        // Arrange - Seed using ModelMockAsync
        var orderMock = new OrderMockAsync(await GetContextAsync(), _faker);
        var orders = await orderMock.GenerateAsync(15);

        var spec = SpecBuilder<Order>.Create()
            .And(o => o.Status == OrderStatus.Pending)
            .Order(o => o.CreatedAt);

        // Act
        var pendingOrders = await _repository.FindAsync(spec);

        // Assert
        pendingOrders.Should().NotBeEmpty();
        pendingOrders.Should().OnlyContain(o => o.Status == OrderStatus.Pending);
    }
}
```

### 3. HTTP Client Mocking

```csharp
public class ExternalApiServiceTests : BaseTests {
    private readonly ExternalApiService _service;
    private readonly HttpClient _httpClient;

    public ExternalApiServiceTests() {
        // Setup HTTP client mock
        _httpClient = HttpClientMock.CreateClient(config => config
            .ForRoute("/api/users/{id}")
            .UsingGet()
            .RespondWithSuccess()
            .WithJsonResponse(new {
                Id = 1,
                Name = "John Doe",
                Email = "john@example.com"
            }));

        AddService<HttpClient>(_httpClient);
        AddService<ExternalApiService, ExternalApiService>();

        _service = GetRequiredService<ExternalApiService>();
    }

    [Fact]
    public async Task GetUser_ShouldReturnMappedUser() {
        // Act
        var user = await _service.GetUserAsync(1);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().Be(1);
        user.Name.Should().Be("John Doe");
    }
}
```

### 4. Multiple HTTP Endpoints

```csharp
var httpClient = HttpClientMock.CreateClientWithEndpoints(
    // GET /api/users
    config => config
        .ForRoute("/api/users")
        .UsingGet()
        .RespondWithSuccess()
        .WithJsonResponse(new[] {
            new { Id = 1, Name = "User1" },
            new { Id = 2, Name = "User2" }
        }),

    // POST /api/users
    config => config
        .ForRoute("/api/users")
        .UsingPost()
        .RespondWith(HttpStatusCode.Created)
        .WithJsonResponse(new { Id = 3, Name = "New User" }),

    // DELETE /api/users/{id}
    config => config
        .ForRoute("/api/users/{id}")
        .UsingDelete()
        .RespondWith(HttpStatusCode.NoContent)
);
```

### 5. Configuration Management

```csharp
public class ConfigurationTests : BaseTests {
    public ConfigurationTests() {
        // Add simple key-value pairs
        AddConfigurationItem("Database:ConnectionString", "Server=test;Database=TestDb");
        AddConfigurationItem("Api:BaseUrl", "https://test-api.com");
        AddConfigurationItem("Api:Timeout", "30");

        // Add configuration sections
        AddConfigurationSection("Logging", new Dictionary<string, string> {
            ["LogLevel:Default"] = "Information",
            ["LogLevel:Microsoft"] = "Warning"
        });

        AddConfigurationSection("JWT", new Dictionary<string, string> {
            ["Secret"] = "test-secret-key",
            ["Issuer"] = "test-issuer",
            ["ExpirationMinutes"] = "60"
        });
    }

    [Fact]
    public void Configuration_ShouldBeAccessible() {
        var config = GetRequiredService<IConfiguration>();

        // Test simple values
        config["Database:ConnectionString"].Should().Be("Server=test;Database=TestDb");
        config["Api:BaseUrl"].Should().Be("https://test-api.com");

        // Test typed values
        config.GetValue<int>("Api:Timeout").Should().Be(30);

        // Test sections
        var jwtSection = config.GetSection("JWT");
        jwtSection["Secret"].Should().Be("test-secret-key");
        jwtSection.GetValue<int>("ExpirationMinutes").Should().Be(60);
    }
}
```

### 6. Service Management and DI

```csharp
public class ComplexServiceTests : BaseTests {
    public ComplexServiceTests() {
        // Bulk service configuration
        ConfigureServices(services => {
            // Framework services
            services.AddLogging();
            services.AddMemoryCache();
            services.AddHttpClient();

            // Application services
            services.AddTransient<IRepository, MockRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IEventBus, InMemoryEventBus>();

            // Configure options
            services.Configure<AppSettings>(settings => {
                settings.ApiUrl = "https://test-api.com";
                settings.Timeout = TimeSpan.FromSeconds(30);
            });
        });
    }

    [Fact]
    public void Services_ShouldBeRegisteredCorrectly() {
        // Check registration
        IsServiceRegistered<IRepository>().Should().BeTrue();

        // Validate resolution
        var service = GetRequiredService<IRepository>();
        service.Should().NotBeNull();

        // Test singleton behavior
        var singleton1 = GetRequiredService<IEventBus>();
        var singleton2 = GetRequiredService<IEventBus>();
        singleton1.Should().BeSameAs(singleton2);
    }

    [Fact]
    public void ScopedServices_ShouldBeIsolated() {
        using var scope1 = CreateScope();
        using var scope2 = CreateScope();

        var scoped1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var scoped2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();

        scoped1.Should().NotBeSameAs(scoped2);
    }
}
```

### 7. Test Fixtures (Shared Resources)

```csharp
// Define fixture
public class DatabaseFixture : TestFixture {
    public const string DatabaseName = "TestDatabase";

    protected override void ConfigureServices(IServiceCollection services) {
        services.AddDbContext<UserDbContext>(options =>
            options.UseInMemoryDatabase(DatabaseName));

        services.AddTransient<UserRepository>();
        services.AddLogging();
    }

    public UserDbContext GetContext() => GetRequiredService<UserDbContext>();

    public async Task SeedDataAsync() {
        var context = GetContext();

        if (!await context.Users.AnyAsync()) {
            var users = new List<UserEntity>();
            for (int i = 0; i < 10; i++) {
                users.Add(new UserEntity {
                    Name = Faker.Name.FullName(),
                    Email = Faker.Internet.Email(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }
    }

    public async Task CleanDataAsync() {
        var context = GetContext();
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();
    }
}

// Collection definition
[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }

// Use fixture in tests
[Collection("Database Collection")]
public class UserServiceIntegrationTests {
    private readonly DatabaseFixture _fixture;

    public UserServiceIntegrationTests(DatabaseFixture fixture) {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateUser_ShouldPersistToSharedDatabase() {
        await _fixture.SeedDataAsync();
        var repository = _fixture.GetRequiredService<UserRepository>();

        var users = await repository.GetAllAsync();
        users.Should().HaveCount(10);
    }
}
```

### 8. Async Testing Utilities

```csharp
[Fact]
public async Task ProcessData_ShouldCompleteWithinTimeout() {
    await TestExtensions.WithTimeoutAsync(
        async () => {
            var result = await _service.LongRunningOperationAsync();
            result.Should().NotBeNull();
        },
        TimeSpan.FromSeconds(5)
    );
}

[Fact]
public async Task ProcessWithCancellation_ShouldRespectToken() {
    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

    await _service.Invoking(s => s.ProcessLongRunningOperationAsync(cts.Token))
        .Should().ThrowAsync<OperationCanceledException>();
}

[Fact]
public async Task ProcessData_ShouldNotThrow() {
    await TestExtensions.AssertDoesNotThrowAsync(
        () => _service.ValidOperationAsync()
    );
}

[Fact]
public async Task ProcessInvalidData_ShouldThrowSpecificException() {
    var exception = await TestExtensions.AssertThrowsAsync<ArgumentException>(
        () => _service.ProcessInvalidDataAsync(null)
    );

    exception.Message.Should().Contain("cannot be null");
    exception.ParamName.Should().Be("data");
}
```

### 9. Test Data Generation Patterns

#### Pattern 1: Direct Faker Usage (Simple)

```csharp
// Direct usage - good for one-off tests
var user = new User {
    Id = _faker.Random.Guid(),
    Name = _faker.Name.FullName(),
    Email = _faker.Internet.Email(),
    Age = _faker.Random.Int(18, 65),
    Address = new Address {
        Street = _faker.Address.StreetAddress(),
        City = _faker.Address.City(),
        ZipCode = _faker.Address.ZipCode()
    }
};
```

---

#### Pattern 2: ModelMock (Reusable, No Database)

**Use When:** You need consistent test data generation without database persistence.

```csharp
// Define mock
public class UserMock : ModelMock<User> {
    public UserMock(Faker faker) : base(faker) { }

    public override IEnumerable<User> Generate(
        int amount,
        IDictionary<string, object>? metadata = null) {

        var isActive = metadata?.ContainsKey("IsActive") == true
            ? (bool)metadata["IsActive"]
            : true;

        return Enumerable.Range(0, amount).Select(_ => new User {
            Id = _faker.Random.Guid(),
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Age = _faker.Random.Int(18, 65),
            IsActive = isActive,
            CreatedDate = DateTime.UtcNow
        });
    }
}

// Usage in tests
public class UserServiceTests : BaseTests {
    private readonly UserMock _userMock;

    public UserServiceTests() {
        _userMock = new UserMock(_faker);
    }

    [Fact]
    public void ProcessUsers_ShouldHandleBatch() {
        // Generate 10 users
        var users = _userMock.Generate(10).ToList();

        users.Should().HaveCount(10);
        users.Should().OnlyContain(u => u.IsActive);
    }

    [Fact]
    public void ProcessInactiveUsers_ShouldFilter() {
        // Generate with custom metadata
        var metadata = new Dictionary<string, object> {
            ["IsActive"] = false
        };
        var users = _userMock.Generate(5, metadata).ToList();

        users.Should().HaveCount(5);
        users.Should().OnlyContain(u => !u.IsActive);
    }
}
```

---

#### Pattern 3: ModelMockAsync (Database-Backed)

**Use When:** You need to generate AND persist test data to database automatically.

```csharp
// Define async mock
public class OrderMockAsync : ModelMockAsync<AppDbContext, Order> {
    public OrderMockAsync(AppDbContext context, Faker faker)
        : base(context, faker) { }

    public override async Task<IEnumerable<Order>> GenerateAsync(
        int amount,
        IDictionary<string, object>? metadata = null) {

        var status = metadata?.ContainsKey("Status") == true
            ? (OrderStatus)metadata["Status"]
            : OrderStatus.Pending;

        var orders = new List<Order>();
        for (int i = 0; i < amount; i++) {
            var order = new Order {
                Id = _faker.Random.Guid(),
                CustomerId = _faker.Random.Guid(),
                Status = status,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem> {
                    new() {
                        ProductId = _faker.Random.Guid(),
                        Quantity = _faker.Random.Int(1, 10),
                        Price = _faker.Random.Decimal(10, 1000)
                    }
                }
            };

            await _collection.AddAsync(order);
            orders.Add(order);
        }

        await SaveChangesAsync(); // Persist to database
        return orders;
    }
}

// Usage in database tests
public class OrderRepositoryTests : BaseDatabaseTests<AppDbContext> {
    [Fact]
    public async Task GetPendingOrders_ShouldReturnCorrectly() {
        // Arrange - Generate and persist to database
        var context = GetContext();
        var orderMock = new OrderMockAsync(context, _faker);

        // Generate 5 pending orders
        var pendingOrders = await orderMock.GenerateAsync(5);

        // Generate 3 confirmed orders
        var metadata = new Dictionary<string, object> {
            ["Status"] = OrderStatus.Confirmed
        };
        var confirmedOrders = await orderMock.GenerateAsync(3, metadata);

        // Act
        var repository = GetRequiredService<OrderRepository>();
        var result = await repository.GetByStatusAsync(OrderStatus.Pending);

        // Assert
        result.Should().HaveCount(5);
        result.Should().OnlyContain(o => o.Status == OrderStatus.Pending);
    }
}
```

---

#### Pattern 4: Helper Methods (Legacy)

// Builder pattern
public class UserBuilder {
    private readonly Faker _faker;
    private string _name;
    private string _email;
    private int _age = 25;
    private bool _isActive = true;

    public UserBuilder(Faker faker) {
        _faker = faker;
        _name = faker.Name.FullName();
        _email = faker.Internet.Email();
    }

    public UserBuilder WithName(string name) {
        _name = name;
        return this;
    }

    public UserBuilder WithEmail(string email) {
        _email = email;
        return this;
    }

    public UserBuilder WithIsActive(bool isActive) {
        _isActive = isActive;
        return this;
    }

    public User Build() => new() {
        Id = _faker.Random.Guid(),
        Name = _name,
        Email = _email,
        Age = _age,
        IsActive = _isActive,
        CreatedDate = DateTime.UtcNow
    };

    public List<User> BuildList(int count) {
        var users = new List<User>();
        for (int i = 0; i < count; i++) {
            users.Add(Build());
        }
        return users;
    }
}

// Usage
var user = new UserBuilder(_faker)
    .WithName("John Doe")
    .WithEmail("john@example.com")
    .Build();

var activeUsers = new UserBuilder(_faker)
    .WithIsActive(true)
    .BuildList(10);
```

### 10. Performance Testing

```csharp
public class PerformanceTests : BaseTests {
    [Fact]
    public async Task ProcessData_ShouldMeetPerformanceRequirements() {
        // Arrange
        var data = new DataBuilder(_faker).BuildList(1000);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var results = await _service.ProcessDataBatchAsync(data);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000,
            "Processing 1000 items should complete within 2 seconds");

        results.Should().HaveCount(1000);
        results.Should().OnlyContain(r => r.IsProcessed);
    }

    [Fact]
    public async Task ConcurrentProcessing_ShouldHandleHighLoad() {
        const int concurrentRequests = 50;

        var tasks = Enumerable.Range(1, concurrentRequests)
            .Select(async i => {
                var result = await _service.ProcessDataAsync(data);
                return new { RequestId = i, Result = result };
            });

        var results = await Task.WhenAll(tasks);

        results.Should().HaveCount(concurrentRequests);
        results.Should().OnlyContain(r => r.Result.IsProcessed);
    }
}
```

---

## Best Practices

### 1. Test Organization

**✅ DO:**
- Inherit from `BaseTests` for unit tests
- Inherit from `BaseDatabaseTests<T>` for EF Core in-memory database tests
- Inherit from `BaseMongoDbTests<T>` for MongoDB tests (automatic initialization)
- Use constructor for test setup
- Use `_faker` for realistic test data
- *Optionally* call `InitializeDatabaseAsync()` and `CleanupDatabaseAsync()` for explicit control

**❌ DON'T:**
- Create HttpClient manually (use HttpClientMock)
- Share state between tests
- Use hardcoded test data
- Manually call initialization for `BaseMongoDbTests<T>` (it's automatic)

### 2. Service Management

**✅ DO:**
- Register services in constructor or ConfigureServices
- Use appropriate service lifetime (Transient, Scoped, Singleton)
- Verify service registration with `IsServiceRegistered<T>()`
- Create scopes for scoped services

**❌ DON'T:**
- Register services in test methods
- Share service provider across tests
- Mix singleton and scoped services incorrectly

### 3. Database Testing

**✅ DO (BaseDatabaseTests<T>):**
- *Optionally* call `InitializeDatabaseAsync()` and `CleanupDatabaseAsync()` for explicit control
- Seed data in test methods, not constructors
- Verify database state after operations
- Use try/finally if you need explicit cleanup timing

**✅ DO (BaseMongoDbTests<T>):**
- Use `GetContext()` or `GetContextAsync()` to get database context (auto-initializes)
- Use `SaveChangesAsync()` to persist changes
- Let automatic cleanup handle database disposal (DisposeAsync)
- Seed data using `ModelMockAsync<TContext, TModel>` for consistency

**❌ DON'T:**
- Share database state between tests
- Use real production databases in tests
- Manually initialize database when using `BaseMongoDbTests<T>` (it's automatic)

### 4. Test Data Generation

**✅ DO:**
- Use `_faker` for generating realistic test data
- Create builder patterns for complex objects
- Create helper methods for common test data scenarios
- Generate unique data per test

**❌ DON'T:**
- Use hardcoded values (IDs, names, emails)
- Share test data instances between tests
- Use production data in tests

### 5. Async Testing

**✅ DO:**
- Use `async`/`await` for all async tests
- Pass `CancellationToken` to services
- Use `TestExtensions.WithTimeoutAsync()` for operations with time limits
- Test cancellation scenarios

**❌ DON'T:**
- Use `.Result` or `.Wait()` (causes deadlocks)
- Skip timeout testing
- Ignore cancellation tokens

---

## Migration from NUnit

| NUnit | xUnit + Myth.Testing |
|-------|---------------------|
| `[OneTimeSetUp]` | Constructor with `ConfigureServices()` |
| `[SetUp]` | Constructor or `InitializeDatabaseAsync()` |
| `[TearDown]` | `CleanupDatabaseAsync()` or `Dispose()` |
| `[Test]` | `[Fact]` |
| `[TestCase]` | `[Theory]` with `[InlineData]` |
| `Assert.AreEqual` | `.Should().Be()` |
| `Assert.IsNotNull` | `.Should().NotBeNull()` |
| `Assert.Throws` | `.Should().Throw<T>()` |

---

## Summary

Myth.Testing eliminates 80% of test setup boilerplate by providing:

- **Pre-configured base classes** with DI, configuration, and Faker
- **In-memory database** testing with automatic lifecycle management
- **HTTP client mocking** for external API testing
- **Test data generation** with Bogus integration
- **Enhanced assertions** with FluentAssertions
- **Async testing utilities** for timeouts, exceptions, and retries
- **Shared fixtures** for expensive resource management

Use Myth.Testing to write clean, maintainable tests with minimal setup code.
