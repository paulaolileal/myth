# Myth.Testing

**Testing utilities, mocks, and base test classes for comprehensive unit and integration testing**

Myth.Testing is a powerful testing library built on xUnit that provides pre-configured base classes, utilities, and patterns to dramatically simplify your testing experience in .NET applications. Whether you're writing unit tests, integration tests, or database tests, Myth.Testing eliminates boilerplate code and provides a clean, consistent API for all your testing needs.

## 🚀 Key Features

- 🧪 **Base Test Classes**: Pre-configured `BaseTests` and `BaseDatabaseTests<T>` with dependency injection
- 🗄️ **Entity Framework Testing**: In-memory database support with automatic setup/cleanup
- 🏗️ **Test Data Generation**: Integrated Bogus (Faker) for realistic test data creation
- 🔧 **Service Container**: Full dependency injection support with service management utilities
- 🌐 **HTTP Client Mocking**: Built-in HTTP mocking for external API testing
- 📊 **FluentAssertions Extensions**: Enhanced assertion methods for MVC/API responses
- ⚡ **Async Testing**: Advanced async patterns with timeout management
- 🎯 **xUnit Integration**: Seamless integration with modern xUnit testing framework
- 🔄 **Fixture Support**: Shared test fixtures for expensive resource management
- ⚙️ **Configuration Management**: In-memory configuration for isolated testing
- 🏃‍♂️ **Performance Testing**: Built-in utilities for testing performance scenarios
- 🧩 **Test Organization**: Patterns for organizing and structuring test suites

## 📦 Installation

```bash
dotnet add package Myth.Testing
```

Or via Package Manager Console:

```powershell
Install-Package Myth.Testing
```

**Dependencies automatically included:**
- xUnit (2.9.2) - Modern testing framework
- FluentAssertions (8.8.0) - Fluent assertion library
- Bogus (35.6.5) - Test data generation
- Moq (4.20.72) - Mocking framework
- Entity Framework Core In-Memory (8.0.21) - Database testing
- ASP.NET Core TestHost (8.0.21) - Integration testing

## 🚀 Quick Start

### Basic Unit Tests with BaseTests

```csharp
public class UserServiceTests : BaseTests
{
    private readonly UserService _userService;
    private readonly Mock<IUserRepository> _mockRepository;

    public UserServiceTests()
    {
        // Setup mocks
        _mockRepository = new Mock<IUserRepository>();

        // Register services with dependency injection
        AddService<IUserRepository>(_mockRepository.Object);
        AddService<UserService, UserService>();

        _userService = GetRequiredService<UserService>();
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldSucceed()
    {
        // Arrange - Create test data using Bogus faker
        var user = new User
        {
            Id = _faker.Random.Guid(),
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Age = _faker.Random.Int(18, 65)
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.CreateUserAsync(user);

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
    public async Task CreateUser_WithInvalidName_ShouldThrow(string invalidName)
    {
        // Arrange
        var user = new User
        {
            Id = _faker.Random.Guid(),
            Name = invalidName,
            Email = _faker.Internet.Email()
        };

        // Act & Assert
        await _userService.Invoking(s => s.CreateUserAsync(user))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*name*");
    }
}
```

### Database Tests with BaseDatabaseTests

```csharp
public class UserRepositoryTests : BaseDatabaseTests<UserDbContext>
{
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        // Register repository with dependency injection
        AddService<UserRepository, UserRepository>();
        _repository = GetRequiredService<UserRepository>();
    }

    [Fact]
    public async Task CreateUser_ShouldPersistToDatabase()
    {
        // Arrange
        await InitializeDatabaseAsync();

        var user = new UserEntity
        {
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
        dbUser.Email.Should().Be(user.Email);

        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task GetUsers_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        await InitializeDatabaseAsync();

        var users = new List<UserEntity>();
        for (int i = 0; i < 10; i++)
        {
            users.Add(new UserEntity
            {
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

        await CleanupDatabaseAsync();
    }
}
```

### Integration Tests with HTTP Client Mocking

```csharp
public class ExternalApiServiceTests : BaseTests
{
    private readonly ExternalApiService _service;
    private readonly HttpClient _httpClient;

    public ExternalApiServiceTests()
    {
        // Setup HTTP client mock
        _httpClient = HttpClientMock.CreateClient(config => config
            .ForRoute("/api/users/{id}")
            .UsingGet()
            .RespondWithSuccess()
            .WithJsonResponse(new { Id = 1, Name = "John Doe", Email = "john@example.com" }));

        AddService<HttpClient>(_httpClient);
        AddService<ExternalApiService, ExternalApiService>();

        _service = GetRequiredService<ExternalApiService>();
    }

    [Fact]
    public async Task GetUser_ShouldReturnMappedUser()
    {
        // Act
        var user = await _service.GetUserAsync(1);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().Be(1);
        user.Name.Should().Be("John Doe");
        user.Email.Should().Be("john@example.com");
    }
}
```

## 🧩 Core Components

### BaseTests Class

The foundation class for all unit tests, providing comprehensive testing infrastructure:

**Key Features:**
- **Dependency Injection**: Full service container with configuration
- **Faker Integration**: Pre-configured Bogus faker instance (`_faker`)
- **Configuration Management**: In-memory configuration for testing
- **Service Registration**: Fluent API for registering services and mocks
- **Async Support**: Built-in async/await patterns with cancellation

```csharp
public class MyServiceTests : BaseTests
{
    private readonly MyService _service;

    public MyServiceTests()
    {
        // Configure services in constructor
        ConfigureServices(services =>
        {
            services.AddTransient<IRepository, MockRepository>();
            services.AddSingleton<ICache, InMemoryCache>();
        });

        // Configuration setup
        AddConfigurationItem("ApiKey", "test-key");
        AddConfigurationItem("Database:ConnectionString", "test-connection");

        // Service retrieval
        _service = GetRequiredService<MyService>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        // Additional service configuration
        services.AddLogging();
    }

    [Fact]
    public async Task TestMethod_ShouldWork()
    {
        // _faker is available here
        var testData = _faker.Lorem.Sentence();

        // Configuration is accessible
        var config = GetRequiredService<IConfiguration>();

        // Your test logic
    }
}
```

**Available Methods:**
```csharp
// Service management
AddService<TInterface>(instance)
AddService<TInterface, TImplementation>()
AddService<TInterface, TImplementation>(ServiceLifetime)
ReplaceService<TInterface>(newInstance)
GetRequiredService<TService>()
GetService<TService>()
IsServiceRegistered<TService>()

// Configuration
AddConfigurationItem(key, value)
AddConfigurationSection(section, values)

// Scopes
CreateScope()
```

### BaseDatabaseTests<TContext> Class

Extends BaseTests with Entity Framework Core support for database testing:

**Key Features:**
- **In-Memory Database**: Isolated database per test class using EF Core InMemory provider
- **Async Lifecycle**: `InitializeDatabaseAsync()` and `CleanupDatabaseAsync()` methods
- **Context Access**: Direct access to DbContext through `GetContext()`
- **Transaction Management**: Automatic transaction handling
- **Seed Data**: Support for database seeding and test data setup

```csharp
public class UserRepositoryTests : BaseDatabaseTests<UserDbContext>
{
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        // Register your repository
        AddService<UserRepository, UserRepository>();
        _repository = GetRequiredService<UserRepository>();
    }

    [Fact]
    public async Task CreateUser_ShouldPersistCorrectly()
    {
        // Initialize database before each test
        await InitializeDatabaseAsync();

        try
        {
            // Arrange
            var user = new UserEntity
            {
                Name = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            var result = await _repository.CreateAsync(user);

            // Assert
            result.Should().NotBeNull();

            // Direct database verification
            var context = GetContext();
            var savedUser = await context.Users.FindAsync(result.Id);
            savedUser.Should().NotBeNull();
        }
        finally
        {
            // Always cleanup
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task SeedData_ShouldBeAccessible()
    {
        await InitializeDatabaseAsync();

        // Seed test data
        var users = new List<UserEntity>();
        for (int i = 0; i < 5; i++)
        {
            users.Add(new UserEntity
            {
                Name = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        var context = GetContext();
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        // Test logic using seeded data
        var allUsers = await _repository.GetAllAsync();
        allUsers.Should().HaveCount(5);

        await CleanupDatabaseAsync();
    }
}
```

**Available Methods:**
```csharp
// Database lifecycle
Task InitializeDatabaseAsync()
Task CleanupDatabaseAsync()
TContext GetContext()

// Inherited from BaseTests
// All service management and configuration methods
```

### Test Data Generation

Myth.Testing includes a pre-configured **Bogus (Faker)** instance available through the `_faker` field in all test classes. Use it directly to generate realistic test data:

**Available Faker Categories:**
- `_faker.Name` - Names and personal information
- `_faker.Internet` - Email addresses, URLs, usernames
- `_faker.Address` - Street addresses, cities, postal codes
- `_faker.Phone` - Phone numbers
- `_faker.Commerce` - Product names, prices
- `_faker.Date` - Dates and times
- `_faker.Lorem` - Lorem ipsum text
- `_faker.Random` - Random values (numbers, booleans, enums)

**Usage Examples:**
```csharp
public class UserServiceTests : BaseTests
{
    [Fact]
    public void CreateTestData_Examples()
    {
        // Simple object creation
        var user = new User
        {
            Id = _faker.Random.Guid(),
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Age = _faker.Random.Int(18, 65),
            IsActive = _faker.Random.Bool(),
            CreatedDate = _faker.Date.Recent()
        };

        // Collections
        var users = new List<User>();
        for (int i = 0; i < 10; i++)
        {
            users.Add(new User
            {
                Id = _faker.Random.Guid(),
                Name = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                Age = _faker.Random.Int(18, 65),
                IsActive = i % 2 == 0, // Mix of active/inactive
                CreatedDate = _faker.Date.Past()
            });
        }

        // Complex objects
        var userWithAddress = new User
        {
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Address = new Address
            {
                Street = _faker.Address.StreetAddress(),
                City = _faker.Address.City(),
                State = _faker.Address.State(),
                ZipCode = _faker.Address.ZipCode(),
                Country = _faker.Address.Country()
            }
        };
    }
}
```

**Helper Methods Pattern:**
Create static helper methods for common test data scenarios:

```csharp
public static class TestDataHelper
{
    public static User CreateValidUser(Faker faker) => new()
    {
        Id = faker.Random.Guid(),
        Name = faker.Name.FullName(),
        Email = faker.Internet.Email(),
        Age = faker.Random.Int(18, 65),
        IsActive = true,
        CreatedDate = DateTime.UtcNow
    };

    public static List<User> CreateUserList(Faker faker, int count, bool allActive = false)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(new User
            {
                Id = faker.Random.Guid(),
                Name = faker.Name.FullName(),
                Email = faker.Internet.Email(),
                Age = faker.Random.Int(18, 65),
                IsActive = allActive || i % 2 == 0,
                CreatedDate = faker.Date.Past()
            });
        }
        return users;
    }
}

// Usage in tests
public class UserServiceTests : BaseTests
{
    [Fact]
    public void TestMethod()
    {
        var user = TestDataHelper.CreateValidUser(_faker);
        var users = TestDataHelper.CreateUserList(_faker, 5, allActive: true);
    }
}
```

### TestFixture Class

Shared fixtures for expensive resources that need to be created once and shared across multiple tests:

**Base TestFixture:**
```csharp
public abstract class TestFixture : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly Faker Faker;

    protected TestFixture()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
        Faker = new Faker();
    }

    protected abstract void ConfigureServices(IServiceCollection services);

    public T GetRequiredService<T>() => ServiceProvider.GetRequiredService<T>();
    public T GetService<T>() => ServiceProvider.GetService<T>();

    public virtual void Dispose()
    {
        ServiceProvider?.Dispose();
    }
}
```

**Database Fixture Example:**
```csharp
public class DatabaseFixture : TestFixture
{
    public const string DatabaseName = "TestDatabase";

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseInMemoryDatabase(DatabaseName)
                   .EnableSensitiveDataLogging());

        services.AddTransient<UserRepository>();
        services.AddLogging();
    }

    public UserDbContext GetContext() => GetRequiredService<UserDbContext>();

    public async Task SeedDataAsync()
    {
        var context = GetContext();

        if (!await context.Users.AnyAsync())
        {
            var users = new List<UserEntity>();
            for (int i = 0; i < 10; i++)
            {
                users.Add(new UserEntity
                {
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

    public async Task CleanDataAsync()
    {
        var context = GetContext();
        context.Users.RemoveRange(context.Users);
        await context.SaveChangesAsync();
    }
}

// Collection definition for sharing across test classes
[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created.
    // Its purpose is simply to be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
}
```

**HTTP Client Fixture Example:**
```csharp
public class HttpClientFixture : TestFixture
{
    public HttpClient HttpClient { get; private set; }

    protected override void ConfigureServices(IServiceCollection services)
    {
        // Configure HttpClient with test settings
        HttpClient = HttpClientMock.CreateClient(config => config
            .ForRoute("/api/users/{id}")
            .UsingGet()
            .RespondWithSuccess()
            .WithJsonResponse(new { Id = 1, Name = "Test User" }));

        services.AddSingleton(HttpClient);
        services.AddTransient<ExternalApiService>();
    }

    public override void Dispose()
    {
        HttpClient?.Dispose();
        base.Dispose();
    }
}
```

**Usage in Test Classes:**
```csharp
// Single test class fixture
public class UserRepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public UserRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnSeededData()
    {
        // Arrange
        await _fixture.SeedDataAsync();
        var repository = _fixture.GetRequiredService<UserRepository>();

        // Act
        var users = await repository.GetAllAsync();

        // Assert
        users.Should().HaveCount(10);
    }
}

// Collection fixture (shared across multiple test classes)
[Collection("Database Collection")]
public class UserServiceIntegrationTests
{
    private readonly DatabaseFixture _fixture;

    public UserServiceIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateUser_ShouldPersistToSharedDatabase()
    {
        // Test implementation using shared fixture
        var repository = _fixture.GetRequiredService<UserRepository>();
        // ... test logic
    }
}

[Collection("Database Collection")]
public class UserValidationIntegrationTests
{
    private readonly DatabaseFixture _fixture;

    public UserValidationIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    // This test class shares the same DatabaseFixture instance
    // with UserServiceIntegrationTests
}
```

### Async Testing Extensions

Enhanced async testing patterns with timeout management and exception handling:

**Timeout Testing:**
```csharp
// Basic timeout testing
await TestExtensions.WithTimeoutAsync(
    async () =>
    {
        var result = await service.LongRunningOperationAsync();
        result.Should().NotBeNull();
    },
    TimeSpan.FromSeconds(5)
);

// Custom timeout with cancellation
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
await TestExtensions.WithTimeoutAsync(
    async () =>
    {
        var result = await service.ProcessDataAsync(cts.Token);
        result.Should().BeTrue();
    },
    TimeSpan.FromSeconds(8),
    cts.Token
);

// Testing that operation completes within expected time
var stopwatch = Stopwatch.StartNew();
await TestExtensions.WithTimeoutAsync(
    () => service.OptimizedOperationAsync(),
    TimeSpan.FromSeconds(2)
);
stopwatch.Stop();
stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
```

**Exception Testing:**
```csharp
// Assert specific exception type
await TestExtensions.AssertThrowsAsync<InvalidOperationException>(
    () => service.InvalidOperationAsync()
);

// Assert exception with message validation
var exception = await TestExtensions.AssertThrowsAsync<ArgumentException>(
    () => service.ProcessInvalidDataAsync(null)
);
exception.Message.Should().Contain("cannot be null");
exception.ParamName.Should().Be("data");

// Assert multiple exception types
await TestExtensions.AssertThrowsAnyAsync<Exception>(
    () => service.UnpredictableOperationAsync()
);

// Assert inner exception
var outerException = await TestExtensions.AssertThrowsAsync<ServiceException>(
    () => service.WrappedOperationAsync()
);
outerException.InnerException.Should().BeOfType<HttpRequestException>();
```

**No Exception Testing:**
```csharp
// Assert operation completes without throwing
await TestExtensions.AssertDoesNotThrowAsync(
    () => service.ValidOperationAsync()
);

// Assert with return value validation
var result = await TestExtensions.AssertDoesNotThrowAsync(
    async () =>
    {
        var data = await service.GetDataAsync();
        return data;
    }
);
result.Should().NotBeNull();

// Assert with complex validation
await TestExtensions.AssertDoesNotThrowAsync(
    async () =>
    {
        var users = await service.GetUsersAsync();
        users.Should().HaveCountGreaterThan(0);
        users.Should().OnlyContain(u => !string.IsNullOrEmpty(u.Name));
    }
);
```

**Retry Testing:**
```csharp
// Test retry policies
var attempts = 0;
await TestExtensions.WithRetryAsync(
    async () =>
    {
        attempts++;
        if (attempts < 3)
            throw new TemporaryException("Temporary failure");

        var result = await service.GetDataAsync();
        result.Should().NotBeNull();
    },
    maxAttempts: 3,
    delay: TimeSpan.FromMilliseconds(100)
);

attempts.Should().Be(3);
```

**Parallel Testing:**
```csharp
// Test concurrent operations
var tasks = Enumerable.Range(1, 10)
    .Select(async i =>
    {
        var result = await service.ProcessAsync(i);
        result.Should().Be(i * 2);
    });

await TestExtensions.WhenAllAsync(tasks);

// Test with controlled concurrency
var semaphore = new SemaphoreSlim(3, 3);
var concurrentTasks = Enumerable.Range(1, 20)
    .Select(async i =>
    {
        await semaphore.WaitAsync();
        try
        {
            await service.ProcessWithLimitAsync(i);
        }
        finally
        {
            semaphore.Release();
        }
    });

await TestExtensions.WhenAllAsync(concurrentTasks);
```

## 🔧 Service Management

Comprehensive dependency injection support for testing scenarios.

### Basic Service Registration

```csharp
public class UserServiceTests : BaseTests
{
    public UserServiceTests()
    {
        // Register concrete instance
        AddService<IUserService>(new UserService());

        // Register type with automatic lifetime (Transient by default)
        AddService<IUserService, UserService>();

        // Register with specific lifetime
        AddService<IUserService, UserService>(ServiceLifetime.Scoped);
        AddService<ICache, MemoryCache>(ServiceLifetime.Singleton);

        // Register multiple implementations
        AddService<INotificationService, EmailNotificationService>();
        AddService<INotificationService, SmsNotificationService>();
    }
}
```

### Advanced Service Configuration

```csharp
public class ComplexServiceTests : BaseTests
{
    public ComplexServiceTests()
    {
        // Bulk service configuration
        ConfigureServices(services =>
        {
            // Add framework services
            services.AddLogging();
            services.AddMemoryCache();
            services.AddHttpClient();

            // Add application services
            services.AddTransient<IRepository, MockRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddSingleton<IEventBus, InMemoryEventBus>();

            // Configure options
            services.Configure<AppSettings>(settings =>
            {
                settings.ApiUrl = "https://test-api.com";
                settings.Timeout = TimeSpan.FromSeconds(30);
            });

            // Add conditional services
            if (Environment.GetEnvironmentVariable("TEST_MODE") == "INTEGRATION")
            {
                services.AddTransient<IExternalService, RealExternalService>();
            }
            else
            {
                services.AddTransient<IExternalService, MockExternalService>();
            }
        });
    }
}
```

### Service Replacement and Mocking

```csharp
public class MockingTests : BaseTests
{
    private readonly Mock<IUserRepository> _mockRepository;

    public MockingTests()
    {
        // Create mocks
        _mockRepository = new Mock<IUserRepository>();

        // Setup mock behavior
        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Name = "Test User" });

        // Register mock
        AddService<IUserRepository>(_mockRepository.Object);

        // Replace existing service
        ReplaceService<IUserService>(new MockUserService());

        // Register factory-based services
        AddService<Func<string, IValidator>>(serviceType =>
        {
            return validationType => validationType switch
            {
                "email" => new EmailValidator(),
                "phone" => new PhoneValidator(),
                _ => new DefaultValidator()
            };
        });
    }

    [Fact]
    public async Task TestWithMock()
    {
        var service = GetRequiredService<IUserService>();

        var result = await service.GetUserAsync(Guid.NewGuid());

        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
```

### Service Discovery and Validation

```csharp
public class ServiceValidationTests : BaseTests
{
    [Fact]
    public void Services_ShouldBeRegisteredCorrectly()
    {
        // Check if service is registered
        IsServiceRegistered<IUserService>().Should().BeTrue();
        IsServiceRegistered<INonExistentService>().Should().BeFalse();

        // Validate service can be resolved
        var service = GetService<IUserService>();
        service.Should().NotBeNull();

        // Validate required service throws if not registered
        Action action = () => GetRequiredService<INonExistentService>();
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ServiceLifetimes_ShouldWorkCorrectly()
    {
        // Register services with different lifetimes
        AddService<ITransientService, TransientService>(ServiceLifetime.Transient);
        AddService<IScopedService, ScopedService>(ServiceLifetime.Scoped);
        AddService<ISingletonService, SingletonService>(ServiceLifetime.Singleton);

        // Test singleton behavior
        var singleton1 = GetRequiredService<ISingletonService>();
        var singleton2 = GetRequiredService<ISingletonService>();
        singleton1.Should().BeSameAs(singleton2);

        // Test scoped behavior
        using var scope1 = CreateScope();
        using var scope2 = CreateScope();

        var scoped1a = scope1.ServiceProvider.GetRequiredService<IScopedService>();
        var scoped1b = scope1.ServiceProvider.GetRequiredService<IScopedService>();
        var scoped2 = scope2.ServiceProvider.GetRequiredService<IScopedService>();

        scoped1a.Should().BeSameAs(scoped1b);
        scoped1a.Should().NotBeSameAs(scoped2);

        // Test transient behavior
        var transient1 = GetRequiredService<ITransientService>();
        var transient2 = GetRequiredService<ITransientService>();
        transient1.Should().NotBeSameAs(transient2);
    }
}
```

### Scoped Service Testing

```csharp
public class ScopedServiceTests : BaseTests
{
    [Fact]
    public async Task ScopedOperation_ShouldUseCorrectScope()
    {
        // Register scoped service
        AddService<IDataContext, TestDataContext>(ServiceLifetime.Scoped);
        AddService<IUserRepository, UserRepository>(ServiceLifetime.Scoped);

        // Test within scope
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var context = scope.ServiceProvider.GetRequiredService<IDataContext>();

        // Both services should share the same context instance within scope
        var user = await repository.CreateAsync(new User { Name = "Test" });
        var savedUser = await context.Users.FindAsync(user.Id);

        savedUser.Should().NotBeNull();
        savedUser.Name.Should().Be("Test");
    }

    [Fact]
    public async Task MultipleScopes_ShouldBeIsolated()
    {
        AddService<IDataContext, TestDataContext>(ServiceLifetime.Scoped);

        using var scope1 = CreateScope();
        using var scope2 = CreateScope();

        var context1 = scope1.ServiceProvider.GetRequiredService<IDataContext>();
        var context2 = scope2.ServiceProvider.GetRequiredService<IDataContext>();

        context1.Should().NotBeSameAs(context2);

        // Operations in different scopes should be isolated
        await context1.Users.AddAsync(new User { Name = "User1" });
        await context1.SaveChangesAsync();

        var usersInScope2 = await context2.Users.ToListAsync();
        usersInScope2.Should().BeEmpty();
    }
}
```

## ⚙️ Configuration Management

Comprehensive configuration support for testing scenarios with in-memory configuration:

```csharp
public class ConfigurationTests : BaseTests
{
    public ConfigurationTests()
    {
        // Add simple key-value pairs
        AddConfigurationItem("Database:ConnectionString", "Server=test;Database=TestDb");
        AddConfigurationItem("Api:BaseUrl", "https://test-api.com");
        AddConfigurationItem("Api:Timeout", "30");
        AddConfigurationItem("Features:EnableCache", "true");

        // Add complex configuration sections
        AddConfigurationSection("Logging", new Dictionary<string, string>
        {
            ["LogLevel:Default"] = "Information",
            ["LogLevel:Microsoft"] = "Warning",
            ["LogLevel:System"] = "Warning"
        });

        AddConfigurationSection("Authentication", new Dictionary<string, string>
        {
            ["JWT:Secret"] = "test-secret-key",
            ["JWT:Issuer"] = "test-issuer",
            ["JWT:Audience"] = "test-audience",
            ["JWT:ExpirationMinutes"] = "60"
        });

        // Configuration is automatically available via DI
        var config = GetRequiredService<IConfiguration>();
        var connectionString = config["Database:ConnectionString"];
        var apiUrl = config["Api:BaseUrl"];
        var enableCache = config.GetValue<bool>("Features:EnableCache");
    }

    [Fact]
    public void Configuration_ShouldBeAccessible()
    {
        var config = GetRequiredService<IConfiguration>();

        // Test simple values
        config["Database:ConnectionString"].Should().Be("Server=test;Database=TestDb");
        config["Api:BaseUrl"].Should().Be("https://test-api.com");

        // Test typed values
        config.GetValue<int>("Api:Timeout").Should().Be(30);
        config.GetValue<bool>("Features:EnableCache").Should().BeTrue();

        // Test sections
        var jwtSection = config.GetSection("Authentication:JWT");
        jwtSection["Secret"].Should().Be("test-secret-key");
        jwtSection.GetValue<int>("ExpirationMinutes").Should().Be(60);
    }

    [Fact]
    public void Options_ShouldBindCorrectly()
    {
        // Configure options binding
        ConfigureServices(services =>
        {
            services.Configure<ApiOptions>(GetRequiredService<IConfiguration>().GetSection("Api"));
            services.Configure<JwtOptions>(GetRequiredService<IConfiguration>().GetSection("Authentication:JWT"));
        });

        var apiOptions = GetRequiredService<IOptionsSnapshot<ApiOptions>>();
        var jwtOptions = GetRequiredService<IOptionsSnapshot<JwtOptions>>();

        apiOptions.Value.BaseUrl.Should().Be("https://test-api.com");
        apiOptions.Value.Timeout.Should().Be(30);

        jwtOptions.Value.Secret.Should().Be("test-secret-key");
        jwtOptions.Value.ExpirationMinutes.Should().Be(60);
    }
}

public class ApiOptions
{
    public string BaseUrl { get; set; }
    public int Timeout { get; set; }
}

public class JwtOptions
{
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationMinutes { get; set; }
}
```

## 🌐 HTTP Client Mocking

Comprehensive HTTP client mocking for testing external API dependencies:

### Basic HTTP Mocking

```csharp
public class HttpClientMockingTests : BaseTests
{
    [Fact]
    public async Task SingleEndpoint_ShouldReturnMockedResponse()
    {
        // Single endpoint mock with success response
        var httpClient = HttpClientMock.CreateClient(config => config
            .ForRoute("/api/users/{id}")
            .UsingGet()
            .RespondWithSuccess()
            .WithJsonResponse(new { Id = 1, Name = "John Doe", Email = "john@example.com" }));

        AddService<HttpClient>(httpClient);
        var service = GetRequiredService<ExternalApiService>();

        var result = await service.GetUserAsync(1);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task MultipleEndpoints_ShouldHandleDifferentRoutes()
    {
        // Multiple endpoints mock
        var httpClient = HttpClientMock.CreateClientWithEndpoints(
            config => config
                .ForRoute("/api/users")
                .UsingGet()
                .RespondWithSuccess()
                .WithJsonResponse(new[] { new { Id = 1, Name = "User1" }, new { Id = 2, Name = "User2" } }),

            config => config
                .ForRoute("/api/users")
                .UsingPost()
                .RespondWith(HttpStatusCode.Created)
                .WithJsonResponse(new { Id = 3, Name = "New User" }),

            config => config
                .ForRoute("/api/users/{id}")
                .UsingDelete()
                .RespondWith(HttpStatusCode.NoContent)
        );

        AddService<HttpClient>(httpClient);
        var service = GetRequiredService<ExternalApiService>();

        // Test GET
        var users = await service.GetUsersAsync();
        users.Should().HaveCount(2);

        // Test POST
        var newUser = await service.CreateUserAsync(new { Name = "New User" });
        newUser.Id.Should().Be(3);

        // Test DELETE
        var deleteResult = await service.DeleteUserAsync(1);
        deleteResult.Should().BeTrue();
    }
}
```

### Advanced HTTP Mocking Scenarios

```csharp
public class AdvancedHttpMockingTests : BaseTests
{
    [Fact]
    public async Task ErrorResponses_ShouldBeHandledCorrectly()
    {
        var httpClient = HttpClientMock.CreateClientWithEndpoints(
            // Success case
            config => config
                .ForRoute("/api/users/1")
                .UsingGet()
                .RespondWithSuccess()
                .WithJsonResponse(new { Id = 1, Name = "Found User" }),

            // Not found case
            config => config
                .ForRoute("/api/users/999")
                .UsingGet()
                .RespondWith(HttpStatusCode.NotFound)
                .WithJsonResponse(new { Error = "User not found" }),

            // Server error case
            config => config
                .ForRoute("/api/users/error")
                .UsingGet()
                .RespondWith(HttpStatusCode.InternalServerError)
                .WithJsonResponse(new { Error = "Internal server error" })
        );

        AddService<HttpClient>(httpClient);
        var service = GetRequiredService<ExternalApiService>();

        // Test success
        var user = await service.GetUserAsync(1);
        user.Should().NotBeNull();

        // Test not found
        await service.Invoking(s => s.GetUserAsync(999))
            .Should().ThrowAsync<HttpRequestException>();

        // Test server error
        await service.Invoking(s => s.GetUserAsync(-1)) // assuming this maps to /error
            .Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task RequestValidation_ShouldWorkWithMocks()
    {
        var httpClient = HttpClientMock.CreateClient(config => config
            .ForRoute("/api/users")
            .UsingPost()
            .WithRequestValidation(request =>
            {
                // Validate request headers
                request.Headers.Should().ContainKey("Authorization");
                request.Headers["Authorization"].Should().StartWith("Bearer ");

                // Validate request content
                var content = request.Content.ReadAsStringAsync().Result;
                content.Should().Contain("\"name\":");

                return true;
            })
            .RespondWith(HttpStatusCode.Created)
            .WithJsonResponse(new { Id = 1, Status = "Created" }));

        // Test with valid request
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        var response = await client.PostAsJsonAsync("/api/users", new { name = "Test User" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```

### HTTP Client Service Integration

```csharp
public class ApiServiceIntegrationTests : BaseTests
{
    private readonly HttpClient _httpClient;
    private readonly ExternalApiService _apiService;

    public ApiServiceIntegrationTests()
    {
        // Setup comprehensive HTTP mock
        _httpClient = HttpClientMock.CreateClientWithEndpoints(
            config => config.ForRoute("/api/users/{id}").UsingGet().RespondWithSuccess().WithJsonResponse(new { Id = 1, Name = "John" }),
            config => config.ForRoute("/api/users").UsingPost().RespondWith(HttpStatusCode.Created).WithJsonResponse(new { Id = 2 }),
            config => config.ForRoute("/api/users/{id}").UsingPut().RespondWithSuccess(),
            config => config.ForRoute("/api/users/{id}").UsingDelete().RespondWith(HttpStatusCode.NoContent)
        );

        // Register in DI container
        AddService<HttpClient>(_httpClient);
        AddService<ExternalApiService, ExternalApiService>();

        _apiService = GetRequiredService<ExternalApiService>();
    }

    [Fact]
    public async Task FullCrudOperations_ShouldWorkWithMocks()
    {
        // Create
        var createResult = await _apiService.CreateUserAsync(new CreateUserRequest { Name = "New User" });
        createResult.Id.Should().Be(2);

        // Read
        var getResult = await _apiService.GetUserAsync(1);
        getResult.Should().NotBeNull();
        getResult.Name.Should().Be("John");

        // Update
        var updateResult = await _apiService.UpdateUserAsync(1, new UpdateUserRequest { Name = "Updated John" });
        updateResult.Should().BeTrue();

        // Delete
        var deleteResult = await _apiService.DeleteUserAsync(1);
        deleteResult.Should().BeTrue();
    }
}
```

## 📊 FluentAssertions Extensions

Enhanced assertions for MVC/API testing with custom extension methods:

```csharp
public class FluentAssertionsExtensionsTests : BaseTests
{
    [Fact]
    public async Task HttpResponse_ShouldHaveCustomAssertions()
    {
        var httpClient = HttpClientMock.CreateClient(config => config
            .ForRoute("/api/users/1")
            .UsingGet()
            .RespondWithSuccess()
            .WithJsonResponse(new User { Id = 1, Name = "John Doe" }));

        var response = await httpClient.GetAsync("/api/users/1");

        // Status code assertions
        response.Should().BeStatusCodeOk();
        response.Should().BeSuccessStatusCode();

        // Content type assertions
        response.Should().HaveContentType("application/json");

        // Header assertions
        response.Should().HaveHeader("Content-Type");

        // Extract and validate response data
        var user = response.GetAs<User>();
        user.Should().NotBeNull();
        user.Id.Should().Be(1);
        user.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task ApiResponses_ShouldSupportVariousStatusCodes()
    {
        var httpClient = HttpClientMock.CreateClientWithEndpoints(
            config => config.ForRoute("/api/ok").UsingGet().RespondWithSuccess(),
            config => config.ForRoute("/api/created").UsingPost().RespondWith(HttpStatusCode.Created),
            config => config.ForRoute("/api/nocontent").UsingDelete().RespondWith(HttpStatusCode.NoContent),
            config => config.ForRoute("/api/notfound").UsingGet().RespondWith(HttpStatusCode.NotFound),
            config => config.ForRoute("/api/badrequest").UsingPost().RespondWith(HttpStatusCode.BadRequest)
        );

        // Test various status codes
        var okResponse = await httpClient.GetAsync("/api/ok");
        okResponse.Should().BeStatusCodeOk();

        var createdResponse = await httpClient.PostAsync("/api/created", null);
        createdResponse.Should().BeStatusCodeCreated();

        var noContentResponse = await httpClient.DeleteAsync("/api/nocontent");
        noContentResponse.Should().BeStatusCodeNoContent();

        var notFoundResponse = await httpClient.GetAsync("/api/notfound");
        notFoundResponse.Should().BeStatusCodeNotFound();

        var badRequestResponse = await httpClient.PostAsync("/api/badrequest", null);
        badRequestResponse.Should().BeStatusCodeBadRequest();
    }

    [Fact]
    public void Collections_ShouldHaveEnhancedAssertions()
    {
        var users = new List<User>();
        for (int i = 0; i < 5; i++)
        {
            users.Add(new User
            {
                Id = _faker.Random.Guid(),
                Name = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                Age = _faker.Random.Int(18, 65)
            });
        }

        // Enhanced collection assertions
        users.Should().HaveCountGreaterThan(0);
        users.Should().OnlyContain(u => !string.IsNullOrEmpty(u.Name));
        users.Should().ContainItemsAssignableTo<User>();

        // Specific property assertions
        users.Should().AllSatisfy(user =>
        {
            user.Id.Should().NotBeEmpty();
            user.Name.Should().NotBeNullOrWhiteSpace();
            user.Email.Should().MatchRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        });
    }
}

// Custom FluentAssertions extensions
public static class HttpResponseAssertions
{
    public static AndConstraint<HttpResponseMessage> BeStatusCodeOk(this HttpResponseMessageAssertions assertions)
    {
        assertions.Subject.StatusCode.Should().Be(HttpStatusCode.OK);
        return new AndConstraint<HttpResponseMessage>(assertions.Subject);
    }

    public static AndConstraint<HttpResponseMessage> BeStatusCodeCreated(this HttpResponseMessageAssertions assertions)
    {
        assertions.Subject.StatusCode.Should().Be(HttpStatusCode.Created);
        return new AndConstraint<HttpResponseMessage>(assertions.Subject);
    }

    public static AndConstraint<HttpResponseMessage> BeStatusCodeNoContent(this HttpResponseMessageAssertions assertions)
    {
        assertions.Subject.StatusCode.Should().Be(HttpStatusCode.NoContent);
        return new AndConstraint<HttpResponseMessage>(assertions.Subject);
    }

    public static AndConstraint<HttpResponseMessage> BeStatusCodeNotFound(this HttpResponseMessageAssertions assertions)
    {
        assertions.Subject.StatusCode.Should().Be(HttpStatusCode.NotFound);
        return new AndConstraint<HttpResponseMessage>(assertions.Subject);
    }

    public static AndConstraint<HttpResponseMessage> BeStatusCodeBadRequest(this HttpResponseMessageAssertions assertions)
    {
        assertions.Subject.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        return new AndConstraint<HttpResponseMessage>(assertions.Subject);
    }

    public static T GetAs<T>(this HttpResponseMessage response)
    {
        var content = response.Content.ReadAsStringAsync().Result;
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
```

## 🎯 Best Practices

### 1. Test Organization and Structure

```csharp
namespace MyApp.Tests.Services
{
    public class UserServiceTests : BaseTests
    {
        private readonly UserService _service;
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly Mock<IEmailService> _mockEmailService;

        public UserServiceTests()
        {
            // Setup mocks in constructor
            _mockRepository = new Mock<IUserRepository>();
            _mockEmailService = new Mock<IEmailService>();

            // Configure services with proper lifetimes
            ConfigureServices(services =>
            {
                services.AddTransient<IUserRepository>(_ => _mockRepository.Object);
                services.AddTransient<IEmailService>(_ => _mockEmailService.Object);
                services.AddTransient<UserService>();
                services.AddLogging();
            });

            _service = GetRequiredService<UserService>();
        }

        [Fact]
        public async Task CreateUser_WithValidData_ShouldSucceedAndSendEmail()
        {
            // Arrange
            var userDto = new User
            {
                Id = _faker.Random.Guid(),
                Name = "John Doe",
                Email = "john@example.com",
                Age = _faker.Random.Int(18, 65),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            _mockRepository
                .Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User user, CancellationToken _) => user);

            _mockEmailService
                .Setup(e => e.SendWelcomeEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateUserAsync(userDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("John Doe");
            result.Email.Should().Be("john@example.com");

            // Verify interactions
            _mockRepository.Verify(r => r.CreateAsync(It.IsAny<User>(), default), Times.Once);
            _mockEmailService.Verify(e => e.SendWelcomeEmailAsync("john@example.com", "John Doe"), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public async Task CreateUser_WithInvalidName_ShouldThrowValidationException(string invalidName)
        {
            // Arrange
            var userDto = new User
            {
                Id = _faker.Random.Guid(),
                Name = invalidName,
                Email = _faker.Internet.Email(),
                Age = _faker.Random.Int(18, 65)
            };

            // Act & Assert
            var exception = await _service.Invoking(s => s.CreateUserAsync(userDto))
                .Should().ThrowAsync<ValidationException>();

            exception.Which.Message.Should().Contain("name");
        }
    }
}
```

### 2. Database Test Patterns and Best Practices

```csharp
namespace MyApp.Tests.Repositories
{
    public class UserRepositoryTests : BaseDatabaseTests<UserDbContext>
    {
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            AddService<UserRepository, UserRepository>();
            _repository = GetRequiredService<UserRepository>();
        }

        [Fact]
        public async Task CreateUser_ShouldPersistCorrectly()
        {
            // Always initialize database first
            await InitializeDatabaseAsync();

            try
            {
                // Arrange
                var user = new UserEntityBuilder(_faker)
                    .WithName("Test User")
                    .WithEmail("test@example.com")
                    .Build();

                // Act
                var result = await _repository.CreateAsync(user);

                // Assert
                result.Should().NotBeNull();
                result.Id.Should().NotBeEmpty();

                // Verify in database
                var context = GetContext();
                var savedUser = await context.Users.FindAsync(result.Id);
                savedUser.Should().NotBeNull();
                savedUser.Name.Should().Be("Test User");
                savedUser.Email.Should().Be("test@example.com");
                savedUser.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            }
            finally
            {
                // Always cleanup to ensure test isolation
                await CleanupDatabaseAsync();
            }
        }

        [Fact]
        public async Task GetUsersByStatus_ShouldReturnCorrectUsers()
        {
            await InitializeDatabaseAsync();

            try
            {
                // Arrange - Seed test data
                var activeUsers = new UserEntityBuilder(_faker)
                    .WithIsActive(true)
                    .BuildList(3);

                var inactiveUsers = new UserEntityBuilder(_faker)
                    .WithIsActive(false)
                    .BuildList(2);

                var context = GetContext();
                await context.Users.AddRangeAsync(activeUsers.Concat(inactiveUsers));
                await context.SaveChangesAsync();

                // Act
                var retrievedActiveUsers = await _repository.GetByStatusAsync(isActive: true);

                // Assert
                retrievedActiveUsers.Should().HaveCount(3);
                retrievedActiveUsers.Should().OnlyContain(u => u.IsActive);
                retrievedActiveUsers.Should().BeEquivalentTo(activeUsers, options => options.Excluding(u => u.Id));
            }
            finally
            {
                await CleanupDatabaseAsync();
            }
        }

        [Fact]
        public async Task ConcurrentUpdates_ShouldHandleOptimisticConcurrency()
        {
            await InitializeDatabaseAsync();

            try
            {
                // Arrange
                var user = new UserEntityBuilder(_faker).Build();
                var context = GetContext();
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                // Act - Simulate concurrent updates
                var user1 = await _repository.GetByIdAsync(user.Id);
                var user2 = await _repository.GetByIdAsync(user.Id);

                user1.Name = "Updated by User 1";
                user2.Name = "Updated by User 2";

                await _repository.UpdateAsync(user1);

                // Assert - Second update should fail due to concurrency
                await _repository.Invoking(r => r.UpdateAsync(user2))
                    .Should().ThrowAsync<DbUpdateConcurrencyException>();
            }
            finally
            {
                await CleanupDatabaseAsync();
            }
        }
    }
}
```

### 3. Async Testing Patterns

```csharp
public class AsyncServiceTests : BaseTests
{
    private readonly AsyncProcessingService _service;

    public AsyncServiceTests()
    {
        AddService<AsyncProcessingService, AsyncProcessingService>();
        _service = GetRequiredService<AsyncProcessingService>();
    }

    [Fact]
    public async Task ProcessData_ShouldCompleteWithinTimeout()
    {
        // Test async operations with timeout
        await TestExtensions.WithTimeoutAsync(
            async () =>
            {
                var data = new DataBuilder(_faker).Build();
                var result = await _service.ProcessDataAsync(data);

                result.Should().NotBeNull();
                result.IsProcessed.Should().BeTrue();
            },
            TimeSpan.FromSeconds(5)
        );
    }

    [Fact]
    public async Task ProcessLargeDataset_ShouldCompleteInParallel()
    {
        // Test parallel processing
        var datasets = new DataBuilder(_faker).BuildList(10);

        var tasks = datasets.Select(async data =>
        {
            var result = await _service.ProcessDataAsync(data);
            result.Should().NotBeNull();
            return result;
        });

        var results = await TestExtensions.WhenAllAsync(tasks);

        results.Should().HaveCount(10);
        results.Should().OnlyContain(r => r.IsProcessed);
    }

    [Fact]
    public async Task ProcessWithCancellation_ShouldRespectCancellationToken()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

        var exception = await _service.Invoking(s => s.ProcessLongRunningOperationAsync(cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();

        exception.Which.CancellationToken.Should().Be(cts.Token);
    }
}
```

### 4. Integration Test Patterns with Shared Resources

```csharp
// Shared fixture for expensive resources
public class ApiIntegrationFixture : TestFixture
{
    public WebApplicationFactory<Program> Factory { get; private set; }
    public HttpClient HttpClient { get; private set; }

    protected override void ConfigureServices(IServiceCollection services)
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace services for testing
                    services.AddScoped<IEmailService, MockEmailService>();
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("IntegrationTestDb"));
                });
            });

        HttpClient = Factory.CreateClient();
        services.AddSingleton(Factory);
        services.AddSingleton(HttpClient);
    }

    public override void Dispose()
    {
        HttpClient?.Dispose();
        Factory?.Dispose();
        base.Dispose();
    }
}

[Collection("API Integration Tests")]
public class UserControllerIntegrationTests : IClassFixture<ApiIntegrationFixture>
{
    private readonly ApiIntegrationFixture _fixture;
    private readonly HttpClient _httpClient;

    public UserControllerIntegrationTests(ApiIntegrationFixture fixture)
    {
        _fixture = fixture;
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    public async Task CreateUser_ShouldReturnCreatedUser()
    {
        // Arrange
        var createRequest = new CreateUserRequest
        {
            Name = "Integration Test User",
            Email = "integration@test.com"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/users", createRequest);

        // Assert
        response.Should().BeStatusCodeCreated();
        response.Should().HaveHeader("Location");

        var user = response.GetAs<UserResponse>();
        user.Should().NotBeNull();
        user.Name.Should().Be("Integration Test User");
        user.Email.Should().Be("integration@test.com");
    }

    [Fact]
    public async Task GetUsers_ShouldReturnPaginatedResults()
    {
        // Arrange - Create test data through API
        var users = new List<CreateUserRequest>();
        for (int i = 1; i <= 15; i++)
        {
            users.Add(new CreateUserRequest
            {
                Name = $"User {i}",
                Email = $"user{i}@test.com"
            });
        }

        foreach (var user in users)
        {
            await _httpClient.PostAsJsonAsync("/api/users", user);
        }

        // Act
        var response = await _httpClient.GetAsync("/api/users?page=1&pageSize=10");

        // Assert
        response.Should().BeStatusCodeOk();

        var result = response.GetAs<PagedResult<UserResponse>>();
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().BeGreaterOrEqualTo(15);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}

// Collection definition for sharing fixture across test classes
[CollectionDefinition("API Integration Tests")]
public class ApiIntegrationCollection : ICollectionFixture<ApiIntegrationFixture>
{
    // This class has no code, and is never created.
    // Its purpose is simply to be the place to apply [CollectionDefinition] and all the ICollectionFixture<> interfaces.
}
```

### 5. Performance and Load Testing

```csharp
public class PerformanceTests : BaseTests
{
    private readonly PerformanceService _service;

    public PerformanceTests()
    {
        AddService<PerformanceService, PerformanceService>();
        _service = GetRequiredService<PerformanceService>();
    }

    [Fact]
    public async Task ProcessData_ShouldCompleteWithinPerformanceThreshold()
    {
        // Arrange
        var data = new DataBuilder(_faker).BuildList(1000);
        var stopwatch = Stopwatch.StartNew();

        // Act
        var results = await _service.ProcessDataBatchAsync(data);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000, "Processing 1000 items should complete within 2 seconds");

        results.Should().HaveCount(1000);
        results.Should().OnlyContain(r => r.IsProcessed);
    }

    [Fact]
    public async Task ConcurrentProcessing_ShouldHandleHighLoad()
    {
        // Arrange
        const int concurrentRequests = 50;
        var data = new DataBuilder(_faker).Build();

        // Act
        var tasks = Enumerable.Range(1, concurrentRequests)
            .Select(async i =>
            {
                var result = await _service.ProcessDataAsync(data);
                return new { RequestId = i, Result = result };
            });

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(concurrentRequests);
        results.Should().OnlyContain(r => r.Result.IsProcessed);

        // Verify no duplicate processing
        var processedIds = results.Select(r => r.Result.Id).ToList();
        processedIds.Should().OnlyHaveUniqueItems();
    }
}
```

## 🔄 Migration from NUnit

Comprehensive guide for migrating from NUnit to xUnit with Myth.Testing:

### Framework Comparison

| NUnit | xUnit | Myth.Testing |
|-------|-------|--------------|
| `[OneTimeSetUp]` | Constructor | Constructor with `ConfigureServices()` |
| `[SetUp]` | Constructor | Manual `InitializeDatabaseAsync()` |
| `[TearDown]` | `IDisposable.Dispose` | Manual `CleanupDatabaseAsync()` |
| `[OneTimeTearDown]` | `IDisposable.Dispose` | `TestFixture.Dispose()` |
| `[Test]` | `[Fact]` | `[Fact]` |
| `[TestCase]` | `[Theory]` | `[Theory]` with `[InlineData]` |
| `[TestCaseSource]` | `[Theory]` | `[Theory]` with `[MemberData]` |
| `Assert.AreEqual` | `Assert.Equal` | `.Should().Be()` (FluentAssertions) |
| `Assert.IsNotNull` | `Assert.NotNull` | `.Should().NotBeNull()` |
| `Assert.Throws` | `Assert.Throws` | `.Should().Throw<T>()` |

### Migration Examples

**Before (NUnit):**
```csharp
[TestFixture]
public class UserServiceTests
{
    private UserService _service;
    private Mock<IUserRepository> _mockRepository;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Global setup
    }

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IUserRepository>();
        _service = new UserService(_mockRepository.Object);
    }

    [TearDown]
    public void TearDown()
    {
        // Cleanup after each test
    }

    [Test]
    public async Task CreateUser_ShouldReturnUser()
    {
        // Arrange
        var user = new User { Name = "Test" };
        _mockRepository.Setup(r => r.CreateAsync(user)).ReturnsAsync(user);

        // Act
        var result = await _service.CreateAsync(user);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test", result.Name);
    }

    [TestCase("")]
    [TestCase(null)]
    public void CreateUser_WithInvalidName_ShouldThrow(string name)
    {
        var user = new User { Name = name };
        Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(user));
    }
}
```

**After (xUnit + Myth.Testing):**
```csharp
public class UserServiceTests : BaseTests
{
    private readonly UserService _service;
    private readonly Mock<IUserRepository> _mockRepository;

    public UserServiceTests()
    {
        // Constructor replaces [OneTimeSetUp] and [SetUp]
        _mockRepository = new Mock<IUserRepository>();

        // Configure services with DI
        ConfigureServices(services =>
        {
            services.AddTransient<IUserRepository>(_ => _mockRepository.Object);
            services.AddTransient<UserService>();
        });

        _service = GetRequiredService<UserService>();
    }

    [Fact]
    public async Task CreateUser_ShouldReturnUser()
    {
        // Arrange
        var user = new UserBuilder(_faker).WithName("Test").Build();
        _mockRepository.Setup(r => r.CreateAsync(user)).ReturnsAsync(user);

        // Act
        var result = await _service.CreateAsync(user);

        // Assert - Using FluentAssertions
        result.Should().NotBeNull();
        result.Name.Should().Be("Test");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task CreateUser_WithInvalidName_ShouldThrow(string name)
    {
        // Arrange
        var user = new UserBuilder(_faker).WithName(name).Build();

        // Act & Assert
        await _service.Invoking(s => s.CreateAsync(user))
            .Should().ThrowAsync<ArgumentException>();
    }
}
```

### Database Testing Migration

**Before (NUnit + Entity Framework):**
```csharp
[TestFixture]
public class UserRepositoryTests
{
    private DbContext _context;
    private UserRepository _repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new UserDbContext(options);
        _repository = new UserRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }
}
```

**After (Myth.Testing):**
```csharp
public class UserRepositoryTests : BaseDatabaseTests<UserDbContext>
{
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        AddService<UserRepository, UserRepository>();
        _repository = GetRequiredService<UserRepository>();
    }

    [Fact]
    public async Task CreateUser_ShouldPersist()
    {
        await InitializeDatabaseAsync();

        try
        {
            // Test logic here
            var user = new UserEntityBuilder(_faker).Build();
            var result = await _repository.CreateAsync(user);

            result.Should().NotBeNull();

            var context = GetContext();
            var saved = await context.Users.FindAsync(result.Id);
            saved.Should().NotBeNull();
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }
}
```

## 🚀 Advanced Use Cases

### Integration with Other Myth Libraries

```csharp
public class MythIntegrationTests : BaseTests
{
    public MythIntegrationTests()
    {
        ConfigureServices(services =>
        {
            // Add Myth.Guard for validation
            services.AddGuard();

            // Add Myth.Flow for pipelines
            services.AddFlow();

            // Add Myth.Morph for transformations
            services.AddMorph();

            // Add your services
            services.AddTransient<UserService>();
            services.AddTransient<ValidationPipelineService>();
        });
    }

    [Fact]
    public async Task ValidationPipeline_ShouldWorkWithMythLibraries()
    {
        // Arrange
        var userDto = new UserBuilder(_faker).Build();
        var validator = GetRequiredService<IValidator>();
        var pipeline = GetRequiredService<ValidationPipelineService>();

        // Act - Test integration between Myth.Guard and Myth.Flow
        var result = await pipeline.ProcessUserAsync(userDto);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }
}
```

### Testing with Myth.Flow Pipelines

```csharp
public class PipelineTests : BaseTests
{
    private readonly UserProcessingPipeline _pipeline;

    public PipelineTests()
    {
        ConfigureServices(services =>
        {
            services.AddFlow();
            services.AddTransient<UserProcessingPipeline>();
            services.AddTransient<IUserValidator, MockUserValidator>();
            services.AddTransient<IUserRepository, MockUserRepository>();
        });

        _pipeline = GetRequiredService<UserProcessingPipeline>();
    }

    [Fact]
    public async Task Pipeline_ShouldProcessUserSuccessfully()
    {
        // Arrange
        var context = new UserProcessingContext
        {
            User = new UserBuilder(_faker).Build()
        };

        // Act
        var result = await _pipeline.ExecuteAsync(context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.User.Should().NotBeNull();
        result.Value.IsProcessed.Should().BeTrue();
    }
}
```

### Performance Testing with Myth.Testing

```csharp
public class PerformanceIntegrationTests : BaseTests
{
    [Fact]
    public async Task BulkUserProcessing_ShouldMeetPerformanceRequirements()
    {
        // Arrange
        var users = new UserBuilder(_faker).BuildList(10000);
        var service = GetRequiredService<BulkUserService>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var results = await service.ProcessUsersAsync(users);

        // Assert
        stopwatch.Stop();

        // Performance assertions
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000, "Processing 10k users should complete within 5 seconds");

        // Functional assertions
        results.Should().HaveCount(10000);
        results.Should().OnlyContain(r => r.IsProcessed);

        // Memory usage assertions (if applicable)
        GC.GetTotalMemory(false).Should().BeLessThan(100 * 1024 * 1024, "Memory usage should stay under 100MB");
    }
}
```

## 📚 Examples and Resources

### Complete Test Suite Example

```csharp
namespace MyApp.Tests
{
    // Unit tests
    public class UserServiceUnitTests : BaseTests { /* ... */ }

    // Integration tests with database
    public class UserRepositoryIntegrationTests : BaseDatabaseTests<UserDbContext> { /* ... */ }

    // Integration tests with HTTP
    public class UserApiIntegrationTests : BaseTests { /* ... */ }

    // Performance tests
    public class UserPerformanceTests : BaseTests { /* ... */ }

    // End-to-end tests
    [Collection("E2E Tests")]
    public class UserE2ETests : IClassFixture<WebApplicationFactory<Program>> { /* ... */ }
}
```

### Test Data Management

```csharp
public static class TestDataSets
{
    public static class Users
    {
        public static User ValidUser(Faker faker) => new()
        {
            Id = faker.Random.Guid(),
            Name = faker.Name.FullName(),
            Email = faker.Internet.Email(),
            Age = faker.Random.Int(18, 65),
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        public static User AdminUser(Faker faker) => new()
        {
            Id = faker.Random.Guid(),
            Name = faker.Name.FullName(),
            Email = faker.Internet.Email(),
            Role = "Admin",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        public static List<User> ActiveUsers(Faker faker, int count = 5)
        {
            var users = new List<User>();
            for (int i = 0; i < count; i++)
            {
                users.Add(new User
                {
                    Id = faker.Random.Guid(),
                    Name = faker.Name.FullName(),
                    Email = faker.Internet.Email(),
                    IsActive = true,
                    CreatedDate = faker.Date.Past()
                });
            }
            return users;
        }
    }
}

// Usage in tests
public class UserServiceDataTests : BaseTests
{
    [Fact]
    public async Task ProcessUser_WithValidData_ShouldSucceed()
    {
        var user = TestDataSets.Users.ValidUser(_faker);
        // ... test logic
    }
}
```

## 🤝 Contributing

When extending or contributing to Myth.Testing:

### Development Guidelines

1. **Follow Established Patterns**: Maintain consistency with existing base classes and patterns
2. **Async/Await Support**: Ensure all new features support async operations properly
3. **Comprehensive Documentation**: Provide XML documentation for all public APIs
4. **Usage Examples**: Include practical examples in documentation
5. **Backwards Compatibility**: Avoid breaking changes to existing APIs
6. **Test Coverage**: Write tests for new features and maintain high code coverage

### Code Quality Standards

```csharp
/// <summary>
/// Base class for testing services that require database access with Entity Framework Core.
/// Provides automatic database setup, cleanup, and dependency injection.
/// </summary>
/// <typeparam name="TContext">The Entity Framework DbContext type</typeparam>
public abstract class BaseDatabaseTests<TContext> : BaseTests, IAsyncLifetime
    where TContext : DbContext
{
    /// <summary>
    /// Initializes the in-memory database for testing.
    /// Call this method at the beginning of each test.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InitializeDatabaseAsync()
    {
        // Implementation with proper error handling
    }

    /// <summary>
    /// Cleans up the database after testing.
    /// Call this method at the end of each test to ensure test isolation.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task CleanupDatabaseAsync()
    {
        // Implementation with proper error handling
    }
}
```

### Pull Request Guidelines

1. Include comprehensive tests for new features
2. Update documentation (README.md and XML docs)
3. Follow the existing code style and conventions
4. Add examples demonstrating new functionality
5. Ensure all existing tests continue to pass

## 📄 License

This project is licensed under the Apache 2.0 License. See the [LICENSE](../LICENSE) file for details.

---

**Myth.Testing** - Making .NET testing simple, powerful, and enjoyable! 🧪✨