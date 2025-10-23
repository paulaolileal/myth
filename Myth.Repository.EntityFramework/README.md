# Myth.Repository.EntityFramework

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Repository.EntityFramework/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Repository.EntityFramework/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

It is a .NET library for defining database access repositories using Entity Framework.

# ⭐ Features
- Definition of base context
- Automatic reading of entity mapping files
- Writing implementation
- Implementation of reading using expressions
- Implementation of reading using specification
- Work with transactions
- **Automatic repository registration with dependency injection**

# 🔮 Usage
To use it, simply inherit the created context from [BaseContext](/Contexts/BaseContext.cs). After that, just create the repositories by passing it as a parameter.

## 🕶️ Reading'

Several methods can be used to read:

- `GetProviderName`: Returns the database connection provider
- `AsQueryable`: Returns a collection for execution in the database
- `AsEnumerable`: Returns a collection for execution in memory]
- `ToListAsync`: Returns all items in the collection
- `Where`: Returns the collection with a filter
- `SearchAsync`: Returns the filtered collection
- `SearchPaginatedAsync`: Returns the filtered and paginated collection
- `CountAsync`: Counts the items in the collection
- `AnyAsync`: Checks if any item in the collection meets a requirement
- `AllAsync`: Checks whether all items in the collection meet a requirement
- `FirstOrDefaultAsync`: Returns the first item in the collection
- `LastOrDefaultAsync`: Returns the last item in the collection

## ✍️ Writing

The following methods can be used for writing:

- `AddAsync`: Adds an item to the collection
- `AddRangeAsync`: Adds multiple items to the collection
- `RemoveAsync`: Removes an item from the collection
- `RemoveRangeAsync`: Removes multiple items from the collection
- `UpdateAsync`: Updates a collection item
- `UpdateRangeAsync`: Updates multiple items in the collection
- `AttachAsync`: Attach an item to the collection
- `AttachRangeAsync`: Attach multiple items to the collection
- `SaveChangesAsync`: Saves all changes
- `ExecuteSqlRawAsync`: Executes a query in the database

## 🪄 Unit of work

The entity's independent functionalities are as follows:

- `SaveChangesAsync`: Saves all changes
- `ExecuteSqlRawAsync`: Executes a query in the database
- `BeginTransactionAsync`: Starts a transaction
- `CommitAsync`: Executes all transaction changes
- `RollbackAsync`: Undoes transaction changes
- `CreateSavepointAsync`: Creates a transaction checkpoint
- `RollbackToSavepointAsync`: Returns to a transaction checkpoint

## 🚀 Dependency Injection Registration

The library provides automatic repository registration for simplified dependency injection setup.

### Auto-Registration Methods

- `AddRepositories()`: Automatically registers all repository implementations found in application assemblies
- `AddRepositoriesFromAssembly(assembly)`: Registers repositories from a specific assembly
- `AddRepository<TInterface, TImplementation>()`: Manually registers a specific repository
- `AddUnitOfWork<TUnitOfWork>()`: Registers a Unit of Work implementation

### Basic Usage

```csharp
// Program.cs - ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework DbContext
builder.Services.AddDbContext<MyContext>(options =>
    options.UseSqlServer(connectionString));

// Automatically register ALL repositories
builder.Services.AddRepositories();

var app = builder.BuildApp(); // Use BuildApp() instead of Build()
```

### Advanced Configuration

```csharp
// Custom service lifetime
builder.Services.AddRepositories(ServiceLifetime.Transient);

// Register from specific assembly
builder.Services.AddRepositoriesFromAssembly(typeof(UserRepository).Assembly);

// Manual registration
builder.Services.AddRepository<IUserRepository, UserRepository>();

// Register Unit of Work
builder.Services.AddUnitOfWork<MyUnitOfWork>();
```

### Repository Implementation Example

```csharp
// 1. Create your repository interface
public interface IUserRepository : IReadWriteRepositoryAsync<User> {
    // Add custom methods if needed
}

// 2. Create your repository implementation
public class UserRepository : ReadWriteRepositoryAsync<User>, IUserRepository {
    public UserRepository(MyContext context) : base(context) { }

    // Implement custom methods if needed
}

// 3. The repository will be automatically registered by AddRepositories()
```

### Usage in Controllers/Services

```csharp
[ApiController]
public class UsersController : ControllerBase {
    private readonly IUserRepository _repository;

    public UsersController(IUserRepository repository) {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IEnumerable<User>> GetUsers() {
        return await _repository.ToListAsync();
    }
}
```

### Registration Behavior

- **Service Lifetime**: Scoped by default (recommended for repositories)
- **Interface Priority**: EntityFramework-specific interfaces are registered before base interfaces
- **Test Filtering**: Types with "Test", "Mock", "Fake", or "Stub" in their names are automatically excluded
- **Generic Types**: Generic type definitions are automatically excluded