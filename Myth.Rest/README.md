# Myth.Rest

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Rest?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Rest/) [![NuGet Version](<https://img.shields.io/nuget/vpre/Myth.Rest?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200)>)](https://www.nuget.org/packages/Myth.Rest/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A powerful .NET library for consuming REST APIs with a fluent, chainable interface. Built with enterprise-grade features including advanced retry policies, circuit breakers, dependency injection, and comprehensive error handling.

# ⭐ Features

- **Fluent Interface**: Simple, chainable API design
- **Advanced Retry Policies**: Exponential backoff, jitter, custom strategies
- **Circuit Breaker**: Prevent cascading failures in distributed systems
- **Dependency Injection**: Full ASP.NET Core DI integration
- **Factory Pattern**: Manage multiple API configurations
- **File Operations**: Built-in support for uploads and downloads
- **Logging Integration**: Structured logging with Microsoft.Extensions.Logging
- **Type Safety**: Strong typing with automatic serialization/deserialization
- **Fallback Support**: Graceful degradation with custom fallback responses
- **Exception-Oriented**: Clear error handling and custom exceptions

# 📦 Installation

```bash
dotnet add package Myth.Rest
```

# 🚀 Quick Start

## Basic Usage

```csharp
var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://api.example.com")
        .WithBearerAuthorization("your-token")
        .WithRetry())
    .DoGet("users")
    .OnResult(result => result
        .UseTypeForSuccess<List<User>>())
    .OnError(error => error
        .ThrowForNonSuccess())
    .BuildAsync();

var users = response.GetAs<List<User>>();
```

## Dependency Injection Setup

### Program.cs (Minimal API)

```csharp
builder.Services.AddRest(config => config
    .WithBaseUrl("https://api.example.com")
    .WithBearerAuthorization("token")
    .WithRetry());

// Or use the factory pattern for multiple configurations
builder.Services.AddRestFactory()
    .AddRestConfiguration("api1", config => config
        .WithBaseUrl("https://api1.example.com")
        .WithBearerAuthorization("token1"))
    .AddRestConfiguration("api2", config => config
        .WithBaseUrl("https://api2.example.com")
        .WithBasicAuthorization("user", "pass"));
```

### Using in Controllers/Services

```csharp
public class UserService
{
    private readonly IRestRequest _restClient;
    private readonly IRestFactory _restFactory;

    public UserService(IRestRequest restClient, IRestFactory restFactory)
    {
        _restClient = restClient;
        _restFactory = restFactory;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        var response = await _restClient
            .DoGet("users")
            .OnResult(r => r.UseTypeForSuccess<List<User>>())
            .OnError(e => e.ThrowForNonSuccess())
            .BuildAsync();

        return response.GetAs<List<User>>();
    }

    public async Task<Product[]> GetProductsFromApi2Async()
    {
        var response = await _restFactory
            .Create("api2") // Uses named configuration
            .DoGet("products")
            .OnResult(r => r.UseTypeForSuccess<Product[]>())
            .BuildAsync();

        return response.GetAs<Product[]>();
    }
}
```

# 🔧 Configuration

## Basic Configuration

```csharp
.Configure(config => config
    .WithBaseUrl("https://api.example.com")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithContentType("application/json")
    .WithBearerAuthorization("your-bearer-token")
    .WithHeader("X-Custom-Header", "value")
    .WithBodySerialization(CaseStrategy.CamelCase)
    .WithBodyDeserialization(CaseStrategy.SnakeCase))
```

## Advanced Configuration

### Custom HttpClient

```csharp
.Configure(config => config
    .WithClient(customHttpClient)
    // or
    .WithHttpClientFactory(httpClientFactory, "named-client"))
```

### Type Converters

```csharp
.Configure(config => config
    .WithTypeConverter<IUserRepository, UserRepository>()) // Interface to concrete type mapping
```

### Logging Integration

```csharp
.Configure(config => config
    .WithLogging(logger, logRequests: true, logResponses: true))
```

## Authorization Methods

- `.WithAuthorization(scheme, token)`: Custom authorization header
- `.WithBearerAuthorization(token)`: Bearer token authentication
- `.WithBasicAuthorization(username, password)`: Basic authentication (auto-encoded)
- `.WithBasicAuthorization(encodedToken)`: Basic authentication with pre-encoded token

# 🔄 Retry Policies

The library provides enterprise-grade retry mechanisms following industry standards.

## Smart Default (Recommended)

```csharp
.WithRetry() // 3 attempts, exponential backoff with jitter, server errors only
```

## Custom Retry Strategies

### Exponential Backoff with Jitter (Recommended)

```csharp
.WithRetry(retry => retry
    .WithMaxAttempts(5)
    .UseExponentialBackoffWithJitter(
        baseDelay: TimeSpan.FromSeconds(1),
        multiplier: 2.0,
        maxDelay: TimeSpan.FromSeconds(30),
        jitterRange: TimeSpan.FromMilliseconds(100))
    .ForServerErrors()
    .ForExceptions(typeof(TaskCanceledException)))
```

### Other Strategies

**Exponential Backoff**

```csharp
.UseExponentialBackoff(TimeSpan.FromSeconds(1), multiplier: 2.0)
```

**Fixed Delay**

```csharp
.UseFixedDelay(TimeSpan.FromSeconds(2))
```

**Random Delay**

```csharp
.UseRandom(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
```

### Retry Configuration Options

```csharp
.WithRetry(retry => retry
    .WithMaxAttempts(3)
    .ForServerErrors()                    // 5xx and 429 status codes
    .ForStatusCodes(HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable)
    .ForExceptions(typeof(HttpRequestException), typeof(TaskCanceledException)))
```

# ⚡ Circuit Breaker

Prevent cascading failures in distributed systems:

```csharp
var circuitBreaker = new CircuitBreaker(
    failureThreshold: 5,
    timeout: TimeSpan.FromMinutes(1),
    halfOpenRetryTimeout: TimeSpan.FromSeconds(30));

.Configure(config => config
    .WithCircuitBreaker(circuitBreaker))
```

The circuit breaker has three states:

- **Closed**: Normal operation
- **Open**: Failures exceeded threshold, requests are blocked
- **Half-Open**: Testing if service recovered

# 🎯 HTTP Operations

## GET Requests

```csharp
.DoGet("users/{id}")
.DoGet("products?category=electronics")
```

## POST/PUT/PATCH Requests

```csharp
.DoPost("users", newUser)
.DoPut("users/123", updatedUser)
.DoPatch("users/123", partialUpdate)
```

## DELETE Requests

```csharp
.DoDelete("users/123")
```

# 📁 File Operations

## Downloads

```csharp
var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://api.example.com")
        .WithRetry())
    .DoDownload("files/document.pdf")
    .OnError(error => error.ThrowForNonSuccess())
    .BuildAsync();

// Save to file
await response.SaveToFileAsync("./downloads", "document.pdf", replaceExisting: true);

// Or get as stream
var stream = response.ToStream();
```

## Uploads

### Upload from Stream

```csharp
.DoUpload("files/upload", fileStream, "application/pdf")
```

### Upload from byte array

```csharp
.DoUpload("files/upload", fileBytes, "image/jpeg")
```

### Upload from IFormFile (ASP.NET Core)

```csharp
.DoUpload("files/upload", formFile)
```

### Upload with custom HTTP method

```csharp
.DoUpload("files/upload", file, settings => settings.UsePutAsMethod())
// Available: UsePostAsMethod(), UsePutAsMethod(), UsePatchAsMethod()
```

# ✅ Result Handling

## Type Mapping by Status Code

```csharp
.OnResult(result => result
    .UseTypeForSuccess<User>()                    // 2xx status codes
    .UseTypeFor<ErrorResponse>(HttpStatusCode.BadRequest)
    .UseTypeFor<List<ValidationError>>(HttpStatusCode.UnprocessableEntity)
    .UseEmptyFor(HttpStatusCode.NoContent))       // Empty response for 204
```

## Conditional Type Mapping

```csharp
.OnResult(result => result
    .UseTypeFor<SuccessResponse>(
        HttpStatusCode.OK,
        body => body.success == true)
    .UseTypeFor<ErrorResponse>(
        HttpStatusCode.OK,
        body => body.success == false))
```

## Multiple Status Codes

```csharp
.OnResult(result => result
    .UseTypeFor<ErrorResponse>(new[] {
        HttpStatusCode.BadRequest,
        HttpStatusCode.Conflict,
        HttpStatusCode.UnprocessableEntity
    }))
```

# ❌ Error Handling

## Basic Error Handling

```csharp
.OnError(error => error
    .ThrowForNonSuccess()                        // Throw for any non-2xx status
    .ThrowFor(HttpStatusCode.Unauthorized)       // Throw for specific status
    .NotThrowFor(HttpStatusCode.NotFound))       // Don't throw for 404
```

## Conditional Error Handling

```csharp
.OnError(error => error
    .ThrowFor(HttpStatusCode.BadRequest,
        body => body.errorCode == "VALIDATION_ERROR"))
```

## Fallback Responses

```csharp
.OnError(error => error
    .UseFallback(HttpStatusCode.ServiceUnavailable, new { message = "Service temporarily unavailable" })
    .UseFallback(HttpStatusCode.NotFound, "{}"))
```

# 🏗️ Advanced Patterns

## Repository Pattern

```csharp
public class UserRepository
{
    private readonly IRestRequest _client;

    public UserRepository(IRestRequest client)
    {
        _client = client;
    }

    public async Task<User> GetUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _client
            .DoGet($"users/{id}")
            .OnResult(r => r.UseTypeForSuccess<User>())
            .OnError(e => e
                .ThrowForNonSuccess()
                .UseFallback(HttpStatusCode.NotFound, new User { Id = id, Name = "Unknown" }))
            .BuildAsync(cancellationToken);

        return response.GetAs<User>();
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _client
            .DoPost("users", request)
            .OnResult(r => r
                .UseTypeFor<User>(HttpStatusCode.Created)
                .UseTypeFor<ValidationErrorResponse>(HttpStatusCode.BadRequest))
            .OnError(e => e.ThrowForNonSuccess())
            .BuildAsync(cancellationToken);

        return response.GetAs<User>();
    }
}
```

## Multi-API Factory Pattern

```csharp
public class ApiService
{
    private readonly IRestFactory _restFactory;

    public ApiService(IRestFactory restFactory)
    {
        _restFactory = restFactory;
    }

    public async Task<UserProfile> GetUserProfileAsync(int userId)
    {
        // Get user from API 1
        var userResponse = await _restFactory
            .Create("userApi")
            .DoGet($"users/{userId}")
            .OnResult(r => r.UseTypeForSuccess<User>())
            .BuildAsync();

        // Get preferences from API 2
        var preferencesResponse = await _restFactory
            .Create("preferencesApi")
            .DoGet($"preferences/{userId}")
            .OnResult(r => r.UseTypeForSuccess<UserPreferences>())
            .OnError(e => e.UseFallback(HttpStatusCode.NotFound, new UserPreferences()))
            .BuildAsync();

        return new UserProfile
        {
            User = userResponse.GetAs<User>(),
            Preferences = preferencesResponse.GetAs<UserPreferences>()
        };
    }
}
```

## APIs that Always Return 200 OK

For APIs that return business logic errors as 200 OK:

```csharp
var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://legacy-api.com")
        .WithRetry(retry => retry
            .WithMaxAttempts(2)
            .UseFixedDelay(TimeSpan.FromSeconds(1))))
    .DoGet("users")
    .OnResult(result => result
        .UseTypeFor<List<User>>(HttpStatusCode.OK, body => body.success == true))
    .OnError(error => error
        .ThrowFor(HttpStatusCode.OK, body => body.success == false)
        .ThrowForNonSuccess())
    .BuildAsync();
```

# 🔧 Enterprise Scenarios

## E-commerce with Different Retry Strategies

```csharp
// Critical operations - Conservative retry
services.AddRestConfiguration("orders", config => config
    .WithBaseUrl("https://orders-api.com")
    .WithRetry(retry => retry
        .WithMaxAttempts(2)
        .UseExponentialBackoff(TimeSpan.FromSeconds(2))
        .ForStatusCodes(HttpStatusCode.ServiceUnavailable, HttpStatusCode.TooManyRequests)));

// Read operations - Aggressive retry
services.AddRestConfiguration("catalog", config => config
    .WithBaseUrl("https://catalog-api.com")
    .WithRetry(retry => retry
        .WithMaxAttempts(5)
        .UseExponentialBackoffWithJitter(TimeSpan.FromSeconds(1))
        .ForServerErrors()
        .ForExceptions(typeof(TaskCanceledException))));
```

## Microservices Communication

```csharp
services.AddRestFactory()
    .AddRestConfiguration("userService", config => config
        .WithBaseUrl("https://user-service:8080")
        .WithCircuitBreaker(new CircuitBreaker(5, TimeSpan.FromMinutes(1)))
        .WithRetry())
    .AddRestConfiguration("orderService", config => config
        .WithBaseUrl("https://order-service:8080")
        .WithCircuitBreaker(new CircuitBreaker(3, TimeSpan.FromMinutes(2)))
        .WithRetry(retry => retry
            .WithMaxAttempts(2)
            .UseFixedDelay(TimeSpan.FromSeconds(3))));
```

# 📊 Response Information

Every response contains comprehensive metadata:

```csharp
var response = await Rest.Create()...BuildAsync();

Console.WriteLine($"Status: {response.StatusCode}");
Console.WriteLine($"URL: {response.Url}");
Console.WriteLine($"Method: {response.Method}");
Console.WriteLine($"Elapsed Time: {response.ElapsedTime}");
Console.WriteLine($"Retries Made: {response.RetriesMade}");
Console.WriteLine($"Fallback Used: {response.FallbackUsed}");
Console.WriteLine($"Is Success: {response.IsSuccessStatusCode()}");

// Get typed result
var user = response.GetAs<User>();

// Get raw content
var jsonString = response.ToString();
var bytes = response.ToByteArray();
var stream = response.ToStream();
```

# ⚠️ Exception Types

The library provides specific exceptions for different scenarios:

- `NonSuccessException`: Thrown for HTTP error status codes
- `NotMappedResultTypeException`: When no type mapping is found for a status code
- `DifferentResponseTypeException`: When trying to cast to wrong type
- `ParsingTypeException`: When JSON deserialization fails
- `FileAlreadyExistsOnDownloadException`: When download file already exists
- `NoActionMadeException`: When no HTTP action was defined
- `CircuitBreakerOpenException`: When circuit breaker is open

# 🎛️ Diagnostics & Monitoring

The library includes built-in diagnostics using .NET's Activity API:

```csharp
// Activities are automatically created with tags:
// - http.url
// - http.method
// - Operation timing
```

Integration with OpenTelemetry and other observability tools is seamless.

# 📋 Best Practices

1. **Use Dependency Injection**: Register REST clients as services for better testability
2. **Configure Retry Policies**: Always use retry policies for production scenarios
3. **Implement Circuit Breakers**: Prevent cascade failures in microservices
4. **Handle Errors Gracefully**: Use fallbacks for non-critical operations
5. **Use Typed Responses**: Leverage strong typing for better code maintainability
6. **Configure Timeouts**: Set appropriate timeouts for your scenarios
7. **Log Requests/Responses**: Enable logging for debugging and monitoring
8. **Use Named Configurations**: Use factory pattern for multiple API integrations

# 📄 License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.
