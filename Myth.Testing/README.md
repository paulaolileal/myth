# Myth.Testing

A comprehensive testing library built on xUnit that provides base classes, utilities, and patterns to simplify and enhance your testing experience in .NET applications.

## Features

- 🧪 **Base Test Classes**: Pre-configured base classes for unit and database tests
- 🔄 **Async Patterns**: Built-in support for async testing with timeout management
- 🗄️ **Database Testing**: Entity Framework integration with in-memory databases
- 🏗️ **Test Data Builders**: Fluent API for building test data with Faker integration
- 🔧 **Service Container**: Dependency injection support for testing
- 📊 **FluentAssertions**: Enhanced assertion extensions for better test readability
- 🎯 **xUnit Integration**: Modern testing framework with fixture support

## Installation

```bash
dotnet add package Myth.Testing
```

## Quick Start

### Basic Unit Tests

```csharp
public class UserServiceTests : BaseTests
{
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Register services
        AddService<UserService>(new UserService());
        _userService = GetRequiredService<UserService>();
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldSucceed()
    {
        // Arrange
        var user = new UserBuilder(_faker)
            .WithName("John Doe")
            .WithEmail("john@example.com")
            .Build();

        // Act
        var result = await _userService.CreateUserAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
    }
}
```

### Database Tests

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
    public async Task CreateUser_ShouldPersistToDatabase()
    {
        // Arrange
        await InitializeDatabaseAsync();

        var user = new UserEntityBuilder(_faker).Build();

        // Act
        var result = await _repository.CreateAsync(user);

        // Assert
        result.Should().NotBeNull();

        var dbUser = await GetContext().Users.FindAsync(result.Id);
        dbUser.Should().NotBeNull();

        await CleanupDatabaseAsync();
    }
}
```

## Core Components

### BaseTests

The foundation class for unit tests providing:

- **Service Container**: Dependency injection setup
- **Configuration**: In-memory configuration management
- **Faker Integration**: Pre-configured Bogus faker for test data
- **Service Management**: Easy service registration and retrieval

```csharp
public class MyTests : BaseTests
{
    public MyTests()
    {
        // Services are automatically configured
        // Faker is available as _faker
        // Configuration is set up with test values
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IMyService, MyService>();
    }
}
```

### BaseDatabaseTests<TContext>

Extends BaseTests with Entity Framework support:

- **In-Memory Database**: Isolated database per test class
- **Async Methods**: Database initialization and cleanup
- **Context Access**: Direct access to DbContext
- **Transaction Support**: Automatic transaction management

```csharp
public class DatabaseTests : BaseDatabaseTests<MyDbContext>
{
    [Fact]
    public async Task Test_WithDatabase()
    {
        await InitializeDatabaseAsync();

        // Your test logic here
        var context = GetContext();

        await CleanupDatabaseAsync();
    }
}
```

### Test Data Builders

Fluent API for creating test data:

```csharp
public class UserBuilder : TestDataBuilder<User, UserBuilder>
{
    public UserBuilder(Faker faker) : base(faker) { }

    public UserBuilder WithName(string name) => With(nameof(User.Name), name);
    public UserBuilder WithEmail(string email) => With(nameof(User.Email), email);

    public override User Build()
    {
        return new User
        {
            Name = GetOverrideOrGenerate(nameof(User.Name), f => f.Name.FullName()),
            Email = GetOverrideOrGenerate(nameof(User.Email), f => f.Internet.Email())
        };
    }
}

// Usage
var user = new UserBuilder(_faker)
    .WithName("John Doe")
    .Build();

var users = new UserBuilder(_faker).BuildList(5);
```

### TestFixture

Shared fixtures for expensive resources:

```csharp
public class DatabaseFixture : TestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyDbContext>(options =>
            options.UseInMemoryDatabase("SharedTestDb"));
    }
}

[Collection("Database Collection")]
public class MyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### Async Testing Extensions

Enhanced async testing patterns:

```csharp
// Timeout testing
await TestExtensions.WithTimeoutAsync(
    () => service.LongRunningOperationAsync(),
    TimeSpan.FromSeconds(5)
);

// Exception testing
await TestExtensions.AssertThrowsAsync<InvalidOperationException>(
    () => service.InvalidOperationAsync()
);

// No exception testing
await TestExtensions.AssertDoesNotThrowAsync(
    () => service.ValidOperationAsync()
);
```

## Service Management

### Basic Service Registration

```csharp
// Register instance
AddService<IUserService>(new UserService());

// Register type with lifetime
AddService<IUserService, UserService>(ServiceLifetime.Scoped);

// Configure multiple services
ConfigureServices(services =>
{
    services.AddTransient<IService1, Service1>();
    services.AddSingleton<IService2, Service2>();
});
```

### Advanced Service Operations

```csharp
// Replace existing service
ReplaceService<IUserService>(new MockUserService());

// Check if service is registered
if (IsServiceRegistered<IUserService>())
{
    // Service exists
}

// Create scoped provider
using var scope = CreateScope();
var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();
```

## Configuration Management

```csharp
// Add configuration values
AddConfigurationItem("Database:ConnectionString", "test-connection");
AddConfigurationItem("Api:BaseUrl", "https://test-api.com");

// Configuration is automatically available via DI
var config = GetRequiredService<IConfiguration>();
var connectionString = config["Database:ConnectionString"];
```

## HTTP Client Mocking

Mock external HTTP dependencies for testing:

```csharp
// Single endpoint mock
var httpClient = HttpClientMock.CreateClient(config => config
    .ForRoute("/api/users/{id}")
    .UsingGet()
    .RespondWithSuccess()
    .WithJsonResponse(new { Id = 1, Name = "John Doe" }));

// Multiple endpoints mock
var httpClient = HttpClientMock.CreateClientWithEndpoints(
    config => config.ForRoute("/api/users").UsingGet().RespondWithSuccess(),
    config => config.ForRoute("/api/users").UsingPost().RespondWith(HttpStatusCode.Created)
);

// Use in service tests
public class ApiServiceTests : BaseTests
{
    [Fact]
    public async Task GetUser_ShouldReturnUser()
    {
        var httpClient = HttpClientMock.CreateClient(config => config
            .ForRoute("/api/users/1")
            .UsingGet()
            .RespondWithSuccess()
            .WithJsonResponse(new User { Id = 1, Name = "John" }));

        var service = new ApiService(httpClient);
        var result = await service.GetUserAsync(1);

        result.Should().NotBeNull();
        result.Name.Should().Be("John");
    }
}
```

## FluentAssertions Extensions

Enhanced assertions for MVC/API testing:

```csharp
// Status code assertions
result.Should().BeStatusCodeOk();
result.Should().BeStatusCodeCreated();
result.Should().BeStatusCodeNoContent();

// Extract response data
var user = result.GetAs<User>();
user.Should().NotBeNull();
```

## Best Practices

### 1. Test Organization

```csharp
public class UserServiceTests : BaseTests
{
    private readonly UserService _service;

    public UserServiceTests()
    {
        // Setup in constructor
        ConfigureServices(services =>
        {
            services.AddTransient<IUserRepository, MockUserRepository>();
        });

        _service = GetRequiredService<UserService>();
    }

    [Fact]
    public async Task Method_Scenario_ExpectedBehavior()
    {
        // Arrange
        var input = new UserBuilder(_faker).Build();

        // Act
        var result = await _service.CreateUserAsync(input);

        // Assert
        result.Should().NotBeNull();
    }
}
```

### 2. Database Test Patterns

```csharp
public class UserRepositoryTests : BaseDatabaseTests<UserDbContext>
{
    [Fact]
    public async Task CreateUser_ShouldPersist()
    {
        // Always initialize at the start
        await InitializeDatabaseAsync();

        try
        {
            // Test logic here
            var user = await _repository.CreateAsync(testUser);

            // Verify persistence
            var saved = await GetContext().Users.FindAsync(user.Id);
            saved.Should().NotBeNull();
        }
        finally
        {
            // Always cleanup
            await CleanupDatabaseAsync();
        }
    }
}
```

### 3. Async Patterns

```csharp
[Fact]
public async Task AsyncOperation_ShouldCompleteQuickly()
{
    await TestExtensions.WithTimeoutAsync(
        async () =>
        {
            var result = await _service.ProcessAsync();
            result.Should().NotBeNull();
        },
        TimeSpan.FromSeconds(2)
    );
}
```

### 4. Shared Resources

```csharp
// Use IClassFixture for sharing within a test class
public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
}

// Use Collection for sharing across multiple test classes
[Collection("Integration Tests")]
public class ApiTests
{
    // Tests here share the same fixture instance
}
```

## Migration from NUnit

Key differences when migrating from NUnit:

| NUnit | xUnit | Myth.Testing |
|-------|-------|--------------|
| `[OneTimeSetUp]` | Constructor | Constructor with Setup() |
| `[SetUp]` | Constructor | Manual `InitializeDatabaseAsync()` |
| `[TearDown]` | `IDisposable.Dispose` | Manual `CleanupDatabaseAsync()` |
| `[Test]` | `[Fact]` | `[Fact]` |
| `[TestCase]` | `[Theory]` | `[Theory]` |

## Examples

See the `Examples` folder for comprehensive usage examples:

- `UserServiceTests.cs` - Basic unit testing patterns
- `UserRepositoryTests.cs` - Database testing with Entity Framework
- `SharedFixtureTests.cs` - Shared fixtures and collection patterns

## Contributing

When extending Myth.Testing:

1. Follow the established patterns for base classes
2. Ensure async/await support throughout
3. Provide comprehensive XML documentation
4. Include usage examples
5. Maintain backwards compatibility

## License

This project is licensed under the Apache 2.0 License.