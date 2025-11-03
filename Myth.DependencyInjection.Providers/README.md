# Myth.DependencyInjection.Providers

[![NuGet Version](https://img.shields.io/nuget/v/Myth.DependencyInjection.Providers?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.DependencyInjection.Providers/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.DependencyInjection.Providers?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.DependencyInjection.Providers/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A .NET library that provides pre-configured dependency injection setup for common third-party libraries used in ASP.NET Core applications. Simplifies the integration of API versioning, advanced Swagger/OpenAPI documentation with modern UI features, and AutoMapper with production-ready defaults.

## Why Myth.DependencyInjection.Providers?

Modern ASP.NET Core applications require consistent setup across multiple projects for versioning, API documentation, and object mapping. This library eliminates boilerplate configuration by providing:

- Production-ready API versioning with multiple reader support (URL, header, media type)
- **Advanced Swagger/OpenAPI documentation** with hierarchical navigation, real-time search, and modern UI
- Versioned documentation with automatic endpoint generation and enhanced developer experience
- AutoMapper integration with built-in pagination support
- Extension methods for simplified object mapping throughout your application
- Minimal configuration with sensible defaults and powerful customization options

## Features

- **API Versioning**: Full-featured versioning with URL segment, header, and media type support
- **Advanced Swagger/OpenAPI**: Modern documentation UI with enhanced developer experience
  - 🌲 **Hierarchical TreeView** - Endpoints organized by tags with multi-level support
  - 🔍 **Real-time Search** - Dynamic filtering by name, method, description, and path
  - 🎨 **Light/Dark Theme** - Auto-detection with manual toggle and preference persistence
  - ⚡ **Direct Execution** - One-click API testing without "Try it out" button
  - 💾 **Persistent Cache** - Save parameters and request bodies across browser sessions
  - 🔐 **Advanced Authentication** - Bearer, Basic, and API Key support with dropdown selection
  - ⌨️ **Keyboard Shortcuts** - Power user features (Ctrl+Enter, Ctrl+F, etc.)
  - 📊 **Performance Monitoring** - Request timing, colored status codes, and visual feedback
  - ✨ **Enhanced UX** - JSON beautify, model collapse, validation, and request history
- **AutoMapper Integration**: Simplified configuration with pagination type mappings and global access
- **Type Mapping Extensions**: Static extension methods for convenient object transformations
- **Authentication Integration**: ASP.NET Core authentication validation and secure credential storage
- **Developer Experience**: Fluent APIs, minimal boilerplate, and 100% backward compatibility

## Installation

```bash
dotnet add package Myth.DependencyInjection.Providers
```

## Quick Start

### Complete Setup Example

```csharp
using Myth.Extensions;

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers( );

builder.Services.AddVersioning( 1.0 );

// Basic configuration (100% backward compatible)
builder.Services.AddDocs( settings => {
    settings.UseTitle( "My API" )
           .UseDescription( "A comprehensive API for managing resources" )
           .UseContact( "API Team", "api@mycompany.com", "https://mycompany.com/api" )
           .UseBearerAuthorization( );
} );

// OR with advanced features enabled
builder.Services.AddDocs( settings => {
    settings.UseTitle( "Advanced API Documentation" )
           .UseDescription( "API with modern UI and enhanced developer experience" )
           .UseContact( "API Team", "api@mycompany.com", "https://mycompany.com/api" )

           // Enable all advanced features with sensible defaults
           .UseAdvancedFeatures( )

           // Or configure individually
           .UseTreeView( enableHierarchy: true, tagSeparator: "/" )
           .UseSearch( enableRealTime: true )
           .UseTheme( SwaggerTheme.Auto, allowUserToggle: true )
           .UseCache( enablePersistence: true, expirationMinutes: 60 )
           .UseAuthentication( enableDropdown: true, validateTokens: false )
           .UseUI( enableDirectExecution: true, enableKeyboardShortcuts: true )
           .UsePerformance( enableTiming: true, enableStatusColors: true );
} );

builder.Services.AddTypeMapping( conf => {
    conf.CreateMap<UserEntity, UserDto>( );
    conf.CreateMap<OrderEntity, OrderDto>( );
} );

var app = builder.Build( );

// Optional: Protect Swagger with authentication in production
if ( app.Environment.IsProduction( ) ) {
    app.UseSwaggerAuthentication( );
}

app.UseDocs( );
app.UseAuthorization( );
app.MapControllers( );

app.Run( );
```

## API Versioning

### Configuration

The `AddVersioning` extension configures ASP.NET Core API versioning with multiple version readers:

```csharp
services.AddVersioning( 1.0 );
```

**Features:**
- URL segment versioning: `/api/v1/users`
- Header versioning: `X-API-Version: 1.0`
- Media type versioning: `Accept: application/json;v=1.0`
- Automatic version reporting in response headers
- Default version assumption for unspecified requests

### Controller Setup

Controllers must be decorated with version attributes:

```csharp
[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class UsersController : ControllerBase {

    [HttpGet]
    public IActionResult GetUsers( ) {
        return Ok( new[] { "User1", "User2" } );
    }
}
```

### Multiple Versions

```csharp
[ApiController]
[ApiVersion( "1.0" )]
[ApiVersion( "2.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class ProductsController : ControllerBase {

    [HttpGet]
    [MapToApiVersion( "1.0" )]
    public IActionResult GetProductsV1( ) {
        return Ok( "Version 1 response" );
    }

    [HttpGet]
    [MapToApiVersion( "2.0" )]
    public IActionResult GetProductsV2( ) {
        return Ok( "Version 2 response" );
    }
}
```

### Deprecating Versions

```csharp
[ApiController]
[ApiVersion( "1.0", Deprecated = true )]
[ApiVersion( "2.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class LegacyController : ControllerBase {
}
```

## Advanced Swagger/OpenAPI Documentation

### Modern UI with Enhanced Features

The library provides a completely redesigned Swagger UI with modern features that significantly improve the developer experience while maintaining 100% backward compatibility.

#### Basic Configuration (Backward Compatible)

```csharp
// Traditional configuration still works exactly the same
services.AddDocs( settings => {
    settings.UseTitle( "E-Commerce API" )
           .UseDescription( "RESTful API for e-commerce operations" )
           .UseContact( "Development Team", "dev@ecommerce.com", "https://ecommerce.com/docs" )
           .UseBearerAuthorization( );
} );

app.UseDocs( );
```

#### Advanced Configuration with Modern Features

```csharp
services.AddDocs( settings => {
    settings.UseTitle( "Modern E-Commerce API" )
           .UseDescription( "API with advanced documentation features" )
           .UseContact( "API Team", "api@ecommerce.com", "https://ecommerce.com/docs" )

           // Authentication with dropdown support
           .UseAuthentication(
               enableDropdown: true,        // Show auth method selector
               validateTokens: true,        // Validate against ASP.NET Core auth
               requireAuth: false           // Require auth to access Swagger
           )
           .UseBearerAuthorization( )       // Primary auth method

           // Hierarchical navigation
           .UseTreeView(
               enableHierarchy: true,       // Group endpoints by tags
               tagSeparator: "/"            // Support nested categories
           )

           // Real-time search
           .UseSearch(
               enableRealTime: true,        // Search as you type
               searchFields: SearchFields.Name | SearchFields.Description | SearchFields.Path
           )

           // Theme support
           .UseTheme(
               defaultTheme: SwaggerTheme.Auto,  // Respect system preference
               allowUserToggle: true             // Show theme toggle button
           )

           // Persistent cache
           .UseCache(
               enablePersistence: true,     // Save data across sessions
               expirationMinutes: 120,      // Cache expiration
               enableHistory: true          // Keep request history
           )

           // Enhanced UX
           .UseUI(
               enableKeyboardShortcuts: true,   // Ctrl+Enter, Ctrl+F, etc.
               enableDirectExecution: true,     // No "Try it out" button
               enableJsonBeautify: true,        // Auto-format JSON
               enableModelCollapse: true        // Collapsible model sections
           )

           // Performance monitoring
           .UsePerformance(
               enableTiming: true,          // Show request timing
               enableStatusColors: true,    // Color-code HTTP status
               enableProgressIndicators: true
           );
} );

// Optional: Protect Swagger in production
if ( app.Environment.IsProduction( ) ) {
    app.UseSwaggerAuthentication( );  // Require auth to access Swagger
}

app.UseDocs( );
```

#### Quick Setup with All Features

```csharp
services.AddDocs( settings => {
    settings.UseTitle( "My Advanced API" )
           .UseDescription( "API with all modern features enabled" )
           .UseContact( "Dev Team", "dev@company.com", "https://company.com" )
           .UseBearerAuthorization( )
           .UseAdvancedFeatures( );  // Enable everything with sensible defaults
} );
```

### Hierarchical Organization with Tags

To take advantage of the TreeView feature, organize your endpoints using hierarchical tags:

```csharp
[ApiController]
[Route( "api/[controller]" )]
public class UsersController : ControllerBase {

    [HttpGet]
    [Tags( "Users/Management" )]        // Creates: Users → Management
    public IActionResult GetUsers( ) { }

    [HttpPost]
    [Tags( "Users/Management/Create" )] // Creates: Users → Management → Create
    public IActionResult CreateUser( ) { }

    [HttpGet( "profile" )]
    [Tags( "Users/Profile" )]           // Creates: Users → Profile
    public IActionResult GetProfile( ) { }

    [HttpPut( "profile/avatar" )]
    [Tags( "Users/Profile/Avatar" )]    // Creates: Users → Profile → Avatar
    public IActionResult UpdateAvatar( ) { }
}
```

This creates a hierarchical structure in the Swagger UI:
```
📋 API Endpoints
└── 🔹 Users (4)
    ├── 📁 Management (2)
    │   └── 📁 Create (1)
    └── 📁 Profile (2)
        └── 📁 Avatar (1)
```

### Authentication Methods

#### API Key Authentication

```csharp
services.AddDocs( settings => {
    settings.UseApiKeyAuthorization( )
           .UseAuthentication( enableDropdown: true );
} );
```

#### Multiple Authentication Methods

```csharp
services.AddDocs( settings => {
    settings.UseTitle( "Secure API" )
           .UseAuthentication(
               enableDropdown: true,        // Show dropdown to switch methods
               validateTokens: true,        // Validate tokens server-side
               requireAuth: app.Environment.IsProduction()  // Require auth in prod only
           )
           .UseBearerAuthorization( );     // Default method
} );
```

### Key Features Overview

#### 🔍 **Real-Time Search**
- Search endpoints by name, HTTP method, description, or path
- Instant results with highlighting
- Navigate directly to matching endpoints
- Configurable search fields and debouncing

#### 🌲 **Hierarchical TreeView**
- Organize endpoints by tags with unlimited nesting levels
- Expand/collapse sections individually
- Show endpoint counts per category
- Clean, intuitive navigation structure

#### ⚡ **Direct Execution**
- No "Try it out" button - execute requests directly
- Method-specific buttons (🔍 Fetch, 📤 Create, 🗑️ Delete)
- Validation of required fields before execution
- Visual loading indicators and progress feedback

#### 💾 **Persistent Cache**
- Save request parameters and bodies across browser sessions
- Individual cache controls per endpoint (Load/Save/Clear)
- Request history with configurable retention
- Secure, domain-isolated storage

#### ⌨️ **Keyboard Shortcuts**
- `Ctrl+Enter`: Execute current request
- `Ctrl+F`: Focus search box
- `Ctrl+Shift+T`: Toggle theme
- `Ctrl+Shift+F`: Beautify JSON
- `Ctrl+Delete`: Clear current form

#### 📊 **Performance Monitoring**
- Real-time request timing display
- Color-coded HTTP status codes
- Response size and header information
- Request history with performance metrics

#### 🎨 **Modern Theming**
- Automatic dark/light mode based on system preference
- Manual theme toggle with persistence
- Smooth transitions and modern color schemes
- High contrast and accessibility support

### Legacy API Compatibility

The new advanced features are fully backward compatible. Existing code continues to work without changes:

```csharp
// This still works exactly the same
services.AddSwaggerVersioned( settings => {
    settings.Title = "Legacy API";
    settings.Options.UseBasicAuthorization( );
} );

app.UseSwaggerVersioned( );
```

### XML Documentation

Swagger automatically includes XML comments from your assembly. Enable XML documentation in your project file:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

Then document your endpoints:

```csharp
/// <summary>
/// Retrieves all active users from the system
/// </summary>
/// <returns>A list of user objects</returns>
/// <response code="200">Returns the list of users</response>
/// <response code="401">If the user is not authenticated</response>
[HttpGet]
[ProducesResponseType( StatusCodes.Status200OK )]
[ProducesResponseType( StatusCodes.Status401Unauthorized )]
public IActionResult GetUsers( ) {
    return Ok( users );
}
```

### Accessing Swagger UI

After configuration, Swagger UI is available at:
- `https://localhost:5001/swagger`

Each API version gets its own endpoint:
- `https://localhost:5001/swagger/v1/swagger.json`
- `https://localhost:5001/swagger/v2/swagger.json`

## AutoMapper Integration

### Configuration

```csharp
services.AddTypeMapping( conf => {
    conf.CreateMap<UserEntity, UserDto>( );
    conf.CreateMap<CreateUserRequest, UserEntity>( );
    conf.CreateMap<UpdateUserRequest, UserEntity>( );
} );
```

### Built-in Pagination Mapping

The library automatically configures mapping for pagination types:

```csharp
IPaginated<UserEntity> paginatedEntities = repository.GetPaginated( );
IPaginated<UserDto> paginatedDtos = paginatedEntities.MapTo<IPaginated<UserDto>>( );
```

### Using AutoMapper Profiles

```csharp
public class UserMappingProfile : Profile {

    public UserMappingProfile( ) {
        CreateMap<UserEntity, UserDto>( )
            .ForMember( dest => dest.FullName, opt => opt.MapFrom( src => $"{src.FirstName} {src.LastName}" ) )
            .ForMember( dest => dest.IsActive, opt => opt.MapFrom( src => src.Status == UserStatus.Active ) );

        CreateMap<CreateUserRequest, UserEntity>( )
            .ForMember( dest => dest.Id, opt => opt.Ignore( ) )
            .ForMember( dest => dest.CreatedAt, opt => opt.MapFrom( _ => DateTime.UtcNow ) );
    }
}

services.AddTypeMapping( );
```

Profiles are automatically discovered and registered from all application assemblies.

## Type Mapping Extensions

The library provides convenient extension methods for object mapping available throughout your application:

### Synchronous Mapping

```csharp
var user = userEntity.MapTo<UserDto>( );

var users = userEntities.Select( e => e.MapTo<UserDto>( ) ).ToList( );
```

### Asynchronous Mapping

For async operations that return mapped objects:

```csharp
public async Task<UserDto> GetUserAsync( int id ) {
    return await repository.GetByIdAsync( id ).MapToAsync<UserEntity, UserDto>( );
}

public async ValueTask<OrderDto> GetOrderAsync( int id ) {
    return await repository.GetOrderAsync( id ).MapToAsync<OrderEntity, OrderDto>( );
}
```

### Exception Handling

If `AddTypeMapping` hasn't been called, mapping methods throw `TypeMappingNotConfiguredException`:

```csharp
try {
    var dto = entity.MapTo<UserDto>( );
}
catch ( TypeMappingNotConfiguredException ex ) {
    logger.LogError( ex, "AutoMapper not configured" );
}
```

## Real-World Examples

### Domain-Driven Design Integration

```csharp
public class OrdersController : ControllerBase {
    private readonly IOrderRepository _repository;

    public OrdersController( IOrderRepository repository ) {
        _repository = repository;
    }

    /// <summary>
    /// Creates a new order
    /// </summary>
    [HttpPost]
    [ApiVersion( "1.0" )]
    public async Task<IActionResult> CreateOrder( CreateOrderRequest request ) {
        var orderEntity = request.MapTo<OrderEntity>( );

        await _repository.AddAsync( orderEntity );

        return CreatedAtAction(
            nameof( GetOrder ),
            new { id = orderEntity.Id },
            orderEntity.MapTo<OrderDto>( )
        );
    }

    /// <summary>
    /// Retrieves paginated orders
    /// </summary>
    [HttpGet]
    [ApiVersion( "1.0" )]
    public async Task<IActionResult> GetOrders( [FromQuery] int page = 1, [FromQuery] int pageSize = 20 ) {
        var paginatedOrders = await _repository.GetPaginatedAsync( page, pageSize );

        var result = paginatedOrders.MapTo<IPaginated<OrderDto>>( );

        return Ok( result );
    }
}
```

### Service Layer Integration

```csharp
public class UserService : IUserService {
    private readonly IUserRepository _repository;

    public UserService( IUserRepository repository ) {
        _repository = repository;
    }

    public async Task<UserDto> GetUserByEmailAsync( string email ) {
        var user = await _repository.GetByEmailAsync( email );

        return user.MapTo<UserDto>( );
    }

    public async Task<IPaginated<UserDto>> SearchUsersAsync( string searchTerm, int page, int pageSize ) {
        var users = await _repository.SearchPaginatedAsync( searchTerm, page, pageSize );

        return users.MapTo<IPaginated<UserDto>>( );
    }
}
```

### CQRS Pattern

```csharp
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto> {
    private readonly IUserRepository _repository;

    public GetUserQueryHandler( IUserRepository repository ) {
        _repository = repository;
    }

    public async Task<QueryResult<UserDto>> HandleAsync( GetUserQuery query, CancellationToken cancellationToken ) {
        var user = await _repository.GetByIdAsync( query.UserId, cancellationToken );

        if ( user == null )
            return QueryResult<UserDto>.NotFound( );

        return QueryResult<UserDto>.Success( user.MapTo<UserDto>( ) );
    }
}
```

## Configuration Reference

### Advanced Swagger Configuration Methods

| Method | Parameters | Description |
|--------|------------|-------------|
| `UseTitle(string)` | title | API title displayed in Swagger UI |
| `UseDescription(string)` | description | API description and purpose |
| `UseContact(string, string, string)` | name, email, url | Contact information |
| `UseBearerAuthorization()` | - | Enable JWT Bearer authentication |
| `UseBasicAuthorization()` | - | Enable Basic authentication |
| `UseApiKeyAuthorization()` | - | Enable API Key authentication |
| `UseAdvancedFeatures()` | - | Enable all advanced features with defaults |

#### Advanced Feature Configuration

| Method | Parameters | Description |
|--------|------------|-------------|
| `UseTreeView(bool, string)` | enableHierarchy, tagSeparator | Hierarchical endpoint organization |
| `UseSearch(bool, SearchFields)` | enableRealTime, searchFields | Real-time search configuration |
| `UseTheme(SwaggerTheme, bool)` | defaultTheme, allowUserToggle | Theme and appearance settings |
| `UseCache(bool, int, bool)` | enablePersistence, expirationMinutes, enableHistory | Persistent cache configuration |
| `UseAuthentication(bool, bool, bool)` | enableDropdown, validateTokens, requireAuth | Advanced authentication settings |
| `UseUI(bool, bool, bool, bool)` | enableKeyboardShortcuts, enableDirectExecution, enableJsonBeautify, enableModelCollapse | UI/UX enhancements |
| `UsePerformance(bool, bool, bool)` | enableTiming, enableStatusColors, enableProgressIndicators | Performance monitoring |

#### Configuration Examples

**Minimal Configuration:**
```csharp
settings.UseTitle("My API").UseBearerAuthorization();
```

**Production-Ready Configuration:**
```csharp
settings.UseTitle("Production API")
       .UseDescription("Secure API with advanced features")
       .UseContact("API Team", "api@company.com", "https://docs.company.com")
       .UseAuthentication(enableDropdown: true, validateTokens: true, requireAuth: true)
       .UseAdvancedFeatures();
```

**Custom Feature Selection:**
```csharp
settings.UseTitle("Custom API")
       .UseTreeView(enableHierarchy: true, tagSeparator: "::")
       .UseSearch(enableRealTime: false)  // Disable real-time search
       .UseTheme(SwaggerTheme.Dark, allowUserToggle: false)  // Force dark theme
       .UseCache(enablePersistence: false)  // Disable cache
       .UseUI(enableDirectExecution: false);  // Keep "Try it out" button
```

### Legacy SwaggerSettings Properties (Still Supported)

| Property | Type | Description | Required |
|----------|------|-------------|----------|
| `Title` | string | API title displayed in Swagger UI | Yes |
| `Description` | string | API description and purpose | Yes |
| `DeprecatedDescription` | string | Message shown for deprecated versions | No (default: "This version of API is deprecated!") |
| `ContactName` | string | Contact person or team name | No |
| `ContactEmail` | string | Contact email address | No |
| `ContactUrl` | string | Documentation or support URL | No |
| `Options` | SwaggerGenOptions | Access to underlying Swagger configuration | No |

### Versioning Configuration

The `AddVersioning` method configures:
- Default API version (parameter-specified)
- URL segment reader: `/api/v1/...`
- Header reader: `X-API-Version: 1.0`
- Media type reader: `application/json;v=1.0`
- API explorer with version substitution
- Version reporting in response headers

## Dependencies

- **Asp.Versioning.Mvc** (8.1.0): API versioning framework
- **Asp.Versioning.Mvc.ApiExplorer** (8.1.0): API explorer for versioned endpoints
- **AutoMapper** (13.0.1): Object-to-object mapping
- **Swashbuckle.AspNetCore** (6.6.2): Swagger/OpenAPI implementation
- **Swashbuckle.AspNetCore.Annotations** (6.6.2): Swagger annotations support
- **Myth.DependencyInjection**: Type discovery and assembly scanning
- **Myth.Repository**: Pagination interfaces

## Architecture Benefits

This library promotes clean architecture by:

1. **Separation of Concerns**: DTOs for API contracts, entities for domain logic
2. **Versioning Strategy**: Graceful API evolution without breaking clients
3. **Documentation**: Automatic API documentation synchronized with code
4. **Type Safety**: Compile-time checked mappings between layers
5. **Consistency**: Standardized configuration across microservices
6. **Testability**: Easy to mock and test with dependency injection

## Best Practices

1. **Always version your APIs** from the start, even if you only have v1
2. **Use XML comments** extensively for comprehensive Swagger documentation
3. **Create dedicated DTOs** for each API version to maintain backward compatibility
4. **Organize AutoMapper profiles** by domain aggregate or bounded context
5. **Configure pagination mappings** for all collection endpoints
6. **Use semantic versioning** (1.0, 1.1, 2.0) for API versions
7. **Document breaking changes** in deprecated version descriptions
8. **Test mappings** with unit tests to catch configuration errors early

## License

Licensed under the Apache License, Version 2.0. See LICENSE file for details.