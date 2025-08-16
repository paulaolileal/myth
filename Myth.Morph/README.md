# Myth.Morph

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Morph?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Morph/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Morph?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Morph/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A powerful .NET library for object transformation and mapping. Myth.Morph provides a flexible and extensible system for converting between different types with support for both convention-based and custom mappings.

The main goal is to simplify object mapping scenarios while providing high flexibility and performance through dependency injection integration and schema-based configuration.

# ⭐ Features

- **Simple and Intuitive**: Easy-to-use extension methods for object transformation
- **Flexible Mapping**: Support for automatic, custom, and instance-based mappings
- **Dependency Injection Integration**: Full integration with Microsoft.Extensions.DependencyInjection
- **Generic Type Support**: Automatic mapping for generic collections and interfaces
- **Asynchronous Operations**: Built-in support for async property binding
- **Logging Integration**: Comprehensive logging through Microsoft.Extensions.Logging
- **Exception Safety**: Detailed exception handling with custom exception types
- **Schema-Based Configuration**: Fluent API for configuring complex mappings

# 🕶️ Using

## 🚀 Quick Start

### Installation and Setup

First, register Myth.Morph in your dependency injection container:

```csharp
services.AddMorph();
```

### Basic Usage

Transform objects using the extension methods:

```csharp
// Simple transformation
var destination = source.To<DestinationType>();

// Transform with custom service provider
var destination = source.To<DestinationType>(serviceProvider);

// Transform collections
var destinationList = sourceList.To<DestinationType>();

// Async transformation
var destination = await source.ToAsync<DestinationType>();
```

### Check if Mapping Exists

```csharp
// Check if a mapping exists
bool canMap = source.CanBindTo<DestinationType>();

// Type-safe checking
bool canMap = source.CanBindTo<SourceType, DestinationType>();
```

## 📋 Instance-Based Mapping

Create custom mappings by implementing the `IMorphable<TDestination>` interface:

```csharp
public class UserDto : IMorphable<User>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime BirthDate { get; set; }
    
    public void MorphTo(Schema<User> schema)
    {
        schema
            .Bind(u => u.FullName, () => Name)
            .Bind(u => u.EmailAddress, () => Email)
            .Bind(u => u.Age, sp => CalculateAge(BirthDate))
            .BindAsync(u => u.Profile, async sp => 
            {
                var profileService = sp.GetService<IProfileService>();
                return await profileService.GetProfileAsync(Email);
            })
            .Ignore(u => u.InternalId);
    }
    
    private int CalculateAge(DateTime birthDate) 
        => DateTime.Today.Year - birthDate.Year;
}
```

## ⚙️ Advanced Schema Configuration

### Synchronous Bindings

```csharp
public void MorphTo(Schema<Destination> schema)
{
    // Bind with service provider resolver
    schema.Bind(d => d.Property, sp => 
    {
        var service = sp.GetService<IMyService>();
        return service.GetValue();
    });
    
    // Bind with direct resolver
    schema.Bind(d => d.Property, () => "Direct Value");
    
    // Ignore properties
    schema.Ignore(d => d.UnwantedProperty);
}
```

### Asynchronous Bindings

```csharp
public void MorphTo(Schema<Destination> schema)
{
    // Async binding with service provider
    schema.BindAsync(d => d.AsyncProperty, async sp =>
    {
        var service = sp.GetService<IAsyncService>();
        return await service.GetDataAsync();
    });
    
    // Async binding with direct resolver
    schema.BindAsync(d => d.AsyncProperty, async () =>
    {
        await Task.Delay(100);
        return "Async Value";
    });
}
```

## 🔧 Configuration Options

### Assembly Configuration

```csharp
services.AddMorph(settings =>
{
    // Add specific assemblies
    settings.AddAssembly(Assembly.GetExecutingAssembly());
    settings.AddAssemblies(assembly1, assembly2);
    
    // Clear and add custom assemblies
    settings.ClearAssemblies()
            .AddAssembly(customAssembly);
});
```

### Generic Type Mappings

```csharp
services.AddMorph(settings =>
{
    // Add custom interface to concrete mappings
    settings.AddGenericMorph(typeof(IMyInterface<>), typeof(MyImplementation<>));
    
    // Type-safe generic mapping
    settings.AddGenericMapping<ICustomCollection<>, CustomCollection<>>();
    
    // Clear default mappings and add custom ones
    settings.ClearGenericMappings()
            .AddGenericMapping<IList<>, ArrayList>();
});
```

### Default Generic Mappings

The library includes these default mappings:

- `IList<>` → `List<>`
- `ICollection<>` → `List<>`
- `IDictionary<,>` → `Dictionary<,>`
- `ISet<>` → `HashSet<>`
- `IReadOnlyCollection<>` → `ReadOnlyCollection<>`
- `IReadOnlyList<>` → `List<>`
- `IReadOnlySet<>` → `HashSet<>`

## 🏗️ Complex Mapping Examples

### Repository Pattern Integration

```csharp
public class UserService
{
    private readonly IServiceProvider _serviceProvider;
    
    public UserService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<UserDto> GetUserAsync(int userId)
    {
        var user = await GetUserFromDatabase(userId);
        return user.To<UserDto>(_serviceProvider);
    }
    
    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        var users = await GetUsersFromDatabase();
        return await users.ToAsync<UserDto>(_serviceProvider);
    }
}
```

### Complex Object Transformation

```csharp
public class OrderDto : IMorphable<Order>
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public decimal TotalAmount { get; set; }
    
    public void MorphTo(Schema<Order> schema)
    {
        schema
            .Bind(o => o.OrderId, () => Id)
            .Bind(o => o.Customer, sp =>
            {
                var customerService = sp.GetService<ICustomerService>();
                return customerService.GetCustomerByName(CustomerName);
            })
            .BindAsync(o => o.OrderItems, async sp =>
            {
                // Transform collection asynchronously
                return await Items.ToAsync<OrderItem>(sp);
            })
            .Bind(o => o.Total, () => TotalAmount)
            .BindAsync(o => o.ShippingInfo, async sp =>
            {
                var shippingService = sp.GetService<IShippingService>();
                return await shippingService.CalculateShippingAsync(Id);
            })
            .Ignore(o => o.InternalNotes);
    }
}
```

## 🎯 Use Cases

### API Response Transformation

```csharp
// Transform API responses to domain models
public async Task<User> GetUserFromApi(int userId)
{
    var apiResponse = await httpClient.GetAsync($"users/{userId}");
    var userDto = await apiResponse.Content.ReadFromJsonAsync<UserApiDto>();
    
    return userDto.To<User>();
}
```

### Database Entity Mapping

```csharp
// Transform database entities to DTOs
public async Task<IEnumerable<ProductDto>> GetProductsAsync()
{
    var entities = await dbContext.Products.ToListAsync();
    return entities.To<ProductDto>();
}
```

### Event Sourcing Integration

```csharp
public class UserCreatedEvent : IMorphable<User>
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public void MorphTo(Schema<User> schema)
    {
        schema
            .Bind(u => u.Id, () => UserId)
            .Bind(u => u.FullName, () => Name)
            .Bind(u => u.EmailAddress, () => Email)
            .Bind(u => u.CreatedDate, () => CreatedAt)
            .Bind(u => u.IsActive, () => true);
    }
}
```

# 🚨 Exception Handling

The library provides detailed exception handling:

## Exception Types

- **`BinderNotFoundException`**: Thrown when no mapping exists between source and destination types
- **`BindException`**: Thrown when property or field binding operations fail
- **`InvalidMorphConfigurationException`**: Thrown when the Morph system is not properly configured

## Example Exception Handling

```csharp
try
{
    var result = source.To<DestinationType>();
}
catch (BinderNotFoundException ex)
{
    // Handle missing mapping
    logger.LogError($"No mapping found: {ex.Message}");
}
catch (BindException ex)
{
    // Handle binding error
    logger.LogError($"Binding failed: {ex.Message}");
}
catch (InvalidMorphConfigurationException ex)
{
    // Handle configuration error
    logger.LogError($"Configuration issue: {ex.Message}");
}
```

# 📊 Performance Tips

1. **Reuse Service Provider**: Pass the same service provider instance when transforming multiple objects
2. **Collection Transformation**: Use type-specific collection methods for better performance
3. **Assembly Scanning**: Limit assemblies in configuration to reduce startup time
4. **Async Operations**: Use async methods for I/O-bound operations in bindings

# 🛠️ Troubleshooting

## Common Issues

### "ServiceProvider not configured" Error
```csharp
// Ensure AddMorph() is called in DI configuration
services.AddMorph();
```

### "No mapping found" Error
```csharp
// Check if the source type implements IMorphable<TDestination>
public class MySource : IMorphable<MyDestination>
{
    public void MorphTo(Schema<MyDestination> schema) { /* implementation */ }
}
```

### Generic Collection Mapping Issues
```csharp
// Register appropriate generic mappings
services.AddMorph(settings =>
{
    settings.AddGenericMapping<IMyCollection<>, MyCollection<>>();
});
```

# 📝 Contributing

We welcome contributions! Please read our contributing guidelines and feel free to submit pull requests.

# 📄 License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.