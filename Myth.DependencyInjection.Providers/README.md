# Myth.DependencyInjection.Providers

[![NuGet Version](https://img.shields.io/nuget/v/Myth.DependencyInjection.Providers?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.DependencyInjection.Providers/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.DependencyInjection.Providers?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.DependencyInjection.Providers/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A comprehensive .NET library that provides pre-configured dependency injection setup for enterprise ASP.NET Core applications. Simplifies the integration of API versioning, advanced Swagger/OpenAPI documentation, AutoMapper, health checks, observability, database connections, HashiCorp Vault, and more with production-ready defaults.

## Why Myth.DependencyInjection.Providers?

Modern ASP.NET Core applications require consistent setup across multiple projects for versioning, API documentation, and object mapping. This library eliminates boilerplate configuration by providing:

- Production-ready API versioning with multiple reader support (URL, header, media type)
- **Advanced Swagger/OpenAPI documentation** with hierarchical navigation, real-time search, and modern UI
- Versioned documentation with automatic endpoint generation and enhanced developer experience
- AutoMapper integration with built-in pagination support
- Extension methods for simplified object mapping throughout your application
- Minimal configuration with sensible defaults and powerful customization options

## Features

### 🌐 **API Documentation & Versioning**
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

### 🗄️ **Database Integration**
- **MongoDB**: Pre-configured MongoDB client and database services with Vault token replacement
- **Connection Management**: Singleton client pattern with scoped database access
- **Configuration**: Simple connection string and database name configuration

### 🔍 **Health Checks & Monitoring**
- **Multi-Database Support**: SQL Server, PostgreSQL, MongoDB, and Redis health checks
- **Custom Health Checks**: Internet connectivity and custom dependency validation
- **Metrics Endpoint**: Comprehensive health status reporting with detailed diagnostics
- **Environment Information**: Runtime environment detection and reporting

### 📊 **Observability & Telemetry**
- **OpenTelemetry Integration**: Full tracing and metrics with Prometheus export
- **Custom Metrics**: Auto-discovery and registration of application metrics
- **Instrumentation**: ASP.NET Core, HTTP, SQL, and Runtime instrumentation out-of-the-box
- **Performance Monitoring**: Request timing, resource usage, and operational metrics
- **Metrics Controller**: RESTful endpoint for accessing telemetry data

### 🔧 **Object Mapping & Transformations**
- **AutoMapper Integration**: Simplified configuration with pagination type mappings and global access
- **Type Mapping Extensions**: Static extension methods for convenient object transformations
- **Profile Auto-Discovery**: Automatic registration of mapping profiles from application assemblies
- **Global Access**: Static mapper provider for use throughout the application

### 🔐 **Security & Configuration**
- **HashiCorp Vault Integration**: Kubernetes authentication with automatic token refresh
- **Secret Management**: KV engine secret retrieval with 12-hour token management
- **Local Fallback**: Development-friendly local secret configuration
- **CORS Policies**: Pre-configured CORS policies with customizable settings

### 🏗️ **Infrastructure & Developer Experience**
- **Fluent APIs**: Intuitive configuration with method chaining
- **Minimal Boilerplate**: Reduce configuration code by up to 80%
- **Production Ready**: Battle-tested defaults with enterprise-grade features
- **100% Backward Compatibility**: Seamless migration from existing setups
- **Type Safety**: Compile-time checked configurations and mappings

## Installation

```bash
dotnet add package Myth.DependencyInjection.Providers
```

## Quick Start

### Complete Enterprise Setup Example

```csharp
using Myth.Documentations;
using Myth.HealthChecks.Extensions;
using Myth.Instrumentations;
using Myth.Mappings;
using Myth.Databases.Mongo;
using Myth.Vault.Extensions;
using Myth.Policies;
using Myth.Versionings;

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers( );

// API Versioning
builder.Services.AddVersioning( 1.0 );

// Documentation with advanced features and authentication
builder.Services.AddDocs( settings => {
    settings.UseTitle( "Enterprise API" )
           .UseDescription( "Comprehensive API with enterprise features" )
           .UseContact( "API Team", "api@mycompany.com", "https://mycompany.com/api" )
           .UseAdvancedFeatures( )
           .UseBearerAuthorization( )
           .RequireAuthenticationIf( env => env.IsProduction( ) ); // Auto-protect in production
} );

// Object Mapping
builder.Services.AddTypeMapping( conf => {
    conf.CreateMap<UserEntity, UserDto>( );
    conf.CreateMap<OrderEntity, OrderDto>( );
} );

// Database Integration
builder.Services.AddMongoDB( settings => {
    settings.ConnectionStringKey = "MongoDB";
    settings.DatabaseName = "MyApplicationDB";
} );

// Health Checks
builder.Services.AddHealthCheck( settings => {
    settings.AddSqlServer( "DefaultConnection" )
           .AddMongoDB( "MongoDB" )
           .AddRedis( "Redis" )
           .AddInternetAccess( );
} );

// Observability & Telemetry
builder.Services.AddTelemetry( settings => {
    settings.ApplicationName = "MyApp"
           .EnableMetrics( true )
           .EnableTracing( true );
} );

builder.Services.AddCollectibleMetrics( );

// HashiCorp Vault Integration
builder.Services.AddHashicorpVault( settings => {
    settings.VaultUrl = builder.Configuration[ "Vault:Url" ];
    settings.Namespace = builder.Configuration[ "Vault:Namespace" ];
    settings.RoleName = builder.Configuration[ "Vault:RoleName" ];
} );

// CORS Policies
builder.Services.AddDefaultCors( );

var app = builder.Build( );

// Middleware Pipeline
if ( app.Environment.IsDevelopment( ) ) {
    app.UseDeveloperExceptionPage( );
}

app.UseDefaultCors( );
app.UseDocs( );
app.UseMetrics( ); // Prometheus endpoint
app.UseAuthorization( );

// Add metrics controller
app.MapControllers( );
app.AddMetricsController( );

app.Run( );
```

### Minimal Setup (Backward Compatible)

```csharp
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers( );
builder.Services.AddVersioning( 1.0 );
builder.Services.AddDocs( settings => {
    settings.UseTitle( "My API" ).UseBearerAuthorization( );
} );
builder.Services.AddTypeMapping( );

var app = builder.Build( );

app.UseDocs( );
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
           )

           // Integrated authentication protection
           .RequireAuthenticationIf( env => env.IsProduction( ) ); // Auto-protect in production
} );

app.UseDocs( ); // Authentication is automatically applied when configured
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

### Integrated Swagger Authentication

The library now provides **integrated authentication protection** for Swagger UI, eliminating the need for separate middleware configuration. Authentication requirements are configured directly in the `AddDocs()` method and automatically applied by `UseDocs()`.

#### Basic Authentication Protection

```csharp
// Require authentication in production environments only
builder.Services.AddDocs( settings => {
    settings.UseTitle( "Protected API" )
           .UseBearerAuthorization( )
           .RequireAuthenticationIf( env => env.IsProduction( ) ); // Auto-detect environment
} );

var app = builder.Build( );
app.UseDocs( ); // Authentication automatically applied when configured
```

#### Always Require Authentication

```csharp
// Always require authentication (useful for public APIs)
builder.Services.AddDocs( settings => {
    settings.UseTitle( "Secure API" )
           .UseBearerAuthorization( )
           .RequireAuthentication( true ); // Always require auth
} );
```

#### Custom Authentication Logic

```csharp
// Custom authentication validation
builder.Services.AddDocs( settings => {
    settings.UseTitle( "Custom Auth API" )
           .RequireCustomAuthentication( async ( context ) => {
               // Custom validation logic
               var apiKey = context.Request.Headers[ "X-API-Key" ].FirstOrDefault( );
               return await ValidateApiKeyAsync( apiKey );
           } );
} );
```

#### Authentication with Bypass Paths

```csharp
// Allow certain paths to bypass authentication
builder.Services.AddDocs( settings => {
    settings.UseTitle( "API with Health Checks" )
           .RequireAuthentication( true )
           .WithAuthenticationBypass( "/swagger/health", "/swagger/status" );
} );
```

#### Multiple Authentication Schemes

```csharp
// Support multiple authentication methods
builder.Services.AddDocs( settings => {
    settings.UseTitle( "Multi-Auth API" )
           .UseBearerAuthorization( )
           .RequireAuthentication(
               requireAuthentication: true,
               validateTokens: true,
               AuthorizationType.Bearer,
               AuthorizationType.Basic,
               AuthorizationType.ApiKey
           );
} );
```

**Key Benefits:**
- **Automatic Integration**: No separate middleware registration needed
- **Environment Awareness**: Different requirements per environment
- **Flexible Validation**: Support for custom authentication logic
- **Multiple Schemes**: Bearer, Basic, API Key, and custom methods
- **Bypass Options**: Allow specific paths to remain public
- **Error Handling**: Configurable unauthorized responses

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

## New Enterprise Modules

### Database Integration

#### MongoDB Support

```csharp
// Add MongoDB with Vault token replacement
builder.Services.AddMongoDB( settings => {
    settings.ConnectionStringKey = "MongoDB";
    settings.DatabaseName = "MyApplicationDB";
} );
```

Features:
- Singleton MongoClient pattern for optimal performance
- Scoped IMongoDatabase for request isolation
- Automatic Vault token replacement in connection strings
- Simple configuration with connection string keys

### Health Checks & Monitoring

#### Multi-Database Health Checks

```csharp
builder.Services.AddHealthCheck( settings => {
    settings.AddSqlServer( "DefaultConnection" )
           .AddPostgreSQL( "PostgreSQLConnection" )
           .AddMongoDB( "MongoDB" )
           .AddRedis( "RedisCache" )
           .AddInternetAccess( );
} );

app.UseMetrics( ); // Enable Prometheus endpoint at /metrics
```

#### Metrics Controller

The library provides a ready-to-use controller for health and environment monitoring:

```csharp
app.AddMetricsController( ); // Adds endpoints to existing controllers

// Available endpoints:
// GET /Metrics/HealthCheck      - Comprehensive health status
// GET /Metrics/Environment      - Runtime environment information
// GET /Metrics/Prometheus       - Redirects to /metrics endpoint
```

### Observability & Telemetry

#### OpenTelemetry Integration

```csharp
// Configure telemetry with Prometheus export
builder.Services.AddTelemetry( settings => {
    settings.ApplicationName = "MyApplication"
           .EnableMetrics( true )
           .EnableTracing( true );
} );

// Auto-discover and register custom metrics
builder.Services.AddCollectibleMetrics( );
```

#### Custom Metrics

Create custom metrics by implementing `ICustomMetric`:

```csharp
public class OrderProcessingMetric : ICustomMetric {
    private static readonly Counter<int> OrdersProcessed =
        Meter.CreateCounter<int>( "orders_processed_total", "Number of orders processed" );

    public void RecordOrderProcessed( ) {
        OrdersProcessed.Add( 1 );
    }
}
```

Features:
- OpenTelemetry integration with Prometheus export
- ASP.NET Core, HTTP, SQL, and Runtime instrumentation
- Auto-discovery of custom metrics
- Console export for development
- Production-ready Prometheus endpoint

### HashiCorp Vault Integration

#### Kubernetes Authentication

```csharp
// Production Vault configuration
builder.Services.AddHashicorpVault( settings => {
    settings.VaultUrl = "https://vault.company.com"
           .Namespace = "app/production"
           .RoleName = "myapp-role";
} );

// Development local fallback
builder.Services.AddLocalVault( );
```

#### Secret Retrieval

```csharp
public class OrderService {
    private readonly IVaultProvider _vault;

    public OrderService( IVaultProvider vault ) {
        _vault = vault;
    }

    public async Task ProcessOrderAsync( ) {
        var apiKey = await _vault.GetSecretAsync( "payment-gateway/api-key" );
        // Use the secret...
    }
}
```

Features:
- Kubernetes service account authentication
- Automatic 12-hour token refresh
- KV engine secret retrieval
- Local development fallback
- Token management and error handling

### CORS Policy Management

#### Default CORS Configuration

```csharp
// Add permissive CORS policy for development
builder.Services.AddDefaultCors( );

var app = builder.Build( );

// Enable CORS middleware
app.UseDefaultCors( );
```

The default policy allows:
- Any origin (`AllowAnyOrigin`)
- Any HTTP method (`AllowAnyMethod`)
- Any headers (`AllowAnyHeader`)

### Configuration Examples

#### Microservice Setup

```csharp
var builder = WebApplication.CreateBuilder( args );

// Essential services
builder.Services.AddControllers( );
builder.Services.AddVersioning( 1.0 );

// Documentation
builder.Services.AddDocs( s => s.UseTitle( "Orders API" ).UseAdvancedFeatures( ) );

// Data & Mapping
builder.Services.AddMongoDB( s => s.ConnectionStringKey = "MongoDB" );
builder.Services.AddTypeMapping( );

// Observability
builder.Services.AddHealthCheck( s => s.AddMongoDB( "MongoDB" ).AddInternetAccess( ) );
builder.Services.AddTelemetry( s => s.ApplicationName = "OrdersAPI" );

// Security
builder.Services.AddHashicorpVault( s => s.LoadFromConfiguration( builder.Configuration ) );

var app = builder.Build( );

app.UseDocs( );
app.UseMetrics( );
app.MapControllers( );
app.AddMetricsController( );
app.Run( );
```

#### API Gateway Configuration

```csharp
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers( );
builder.Services.AddVersioning( 2.0 );

// Enhanced documentation for public API with integrated authentication
builder.Services.AddDocs( settings => {
    settings.UseTitle( "Public API Gateway" )
           .UseDescription( "Unified API for all microservices" )
           .UseContact( "API Team", "api@company.com", "https://docs.company.com" )
           .UseAuthentication( enableDropdown: true, requireAuth: true )
           .UseAdvancedFeatures( )
           .RequireAuthentication( true ); // Always require authentication for public API gateway
} );

// Comprehensive health monitoring
builder.Services.AddHealthCheck( settings => {
    settings.AddSqlServer( "UsersDB" )
           .AddMongoDB( "OrdersDB" )
           .AddMongoDB( "InventoryDB" )
           .AddRedis( "SessionCache" )
           .AddRedis( "DataCache" )
           .AddInternetAccess( );
} );

// Full observability stack
builder.Services.AddTelemetry( settings => {
    settings.ApplicationName = "APIGateway"
           .EnableMetrics( true )
           .EnableTracing( true );
} );

builder.Services.AddCollectibleMetrics( );

var app = builder.Build( );

app.UseDocs( ); // Authentication automatically applied based on configuration
app.UseMetrics( );
app.MapControllers( );
app.AddMetricsController( );
app.Run( );
```

## Dependencies

### Core Dependencies
- **Asp.Versioning.Mvc** (8.1.0): API versioning framework
- **Asp.Versioning.Mvc.ApiExplorer** (8.1.0): API explorer for versioned endpoints
- **AutoMapper** (13.0.1): Object-to-object mapping
- **Swashbuckle.AspNetCore** (6.6.2): Swagger/OpenAPI implementation
- **Swashbuckle.AspNetCore.Annotations** (6.6.2): Swagger annotations support

### Health Checks
- **AspNetCore.HealthChecks.SqlServer** (8.0.2): SQL Server health checks
- **AspNetCore.HealthChecks.NpgSql** (8.0.2): PostgreSQL health checks
- **AspNetCore.HealthChecks.MongoDb** (8.0.1): MongoDB health checks
- **AspNetCore.HealthChecks.Redis** (8.0.1): Redis health checks

### Observability & Telemetry
- **OpenTelemetry.Api** (1.8.1): OpenTelemetry API
- **OpenTelemetry.Api.ProviderBuilderExtensions** (1.8.1): Provider builder extensions
- **OpenTelemetry.Exporter.Console** (1.8.1): Console exporter for development
- **OpenTelemetry.Exporter.OpenTelemetryProtocol** (1.8.1): OTLP exporter
- **OpenTelemetry.Exporter.Prometheus.AspNetCore** (1.5.0-rc.1): Prometheus metrics export
- **OpenTelemetry.Extensions.Hosting** (1.8.1): Hosting integration
- **OpenTelemetry.Instrumentation.AspNetCore** (1.8.1): ASP.NET Core instrumentation
- **OpenTelemetry.Instrumentation.Http** (1.8.1): HTTP client instrumentation
- **OpenTelemetry.Instrumentation.Process** (0.5.0-beta.5): Process metrics
- **OpenTelemetry.Instrumentation.Runtime** (1.8.1): .NET runtime metrics
- **OpenTelemetry.Instrumentation.SqlClient** (1.8.0-beta.1): SQL Client instrumentation
- **Npgsql.OpenTelemetry** (8.0.6): PostgreSQL telemetry
- **prometheus-net.AspNetCore** (8.2.1): Prometheus .NET integration

### Database Integration
- **MongoDB.Driver**: MongoDB .NET driver (automatically referenced)
- **Npgsql**: PostgreSQL .NET driver for health checks

### Internal Dependencies
- **Myth.DependencyInjection**: Type discovery and assembly scanning
- **Myth.Repository**: Pagination interfaces and repository patterns
- **Myth.Rest**: HTTP communication and REST client functionality

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