<img  style="float: right;" src="myth-rest-logo.png" alt="drawing" width="250"/>

# Myth.Rest

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Rest?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Rest/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Rest?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Rest/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A modern, fluent REST client for .NET with enterprise-grade resilience patterns. Built for developers who value clean code, type safety, and minimal boilerplate.

## 🎯 Why Myth.Rest?

**Calling external APIs with HttpClient is verbose and error-prone.** Manual JSON serialization, no retry logic, forgetting to dispose clients, no circuit breakers, complex authentication setup. **Myth.Rest provides a fluent, type-safe API client** with built-in retries, circuit breakers, pooled HttpClient management, and automatic JSON handling. Call APIs in one fluent chain, resilience patterns included.

### The Problem

`HttpClient` boilerplate everywhere. Manual `JsonSerializer.Deserialize`. No retry logic (network failures crash). Memory leaks from improper disposal. Authentication scattered. File uploads complex.

### The Solution

**Fluent API**: `await _rest.Post("api/users").Body(user).ExecuteAsync<UserDto>()`. **Resilience built-in**: Retry with exponential backoff, circuit breakers. **Type-safe**: Automatic JSON serialization/deserialization. **HttpClient pooling**: Proper resource management. **Files**: Upload/download with minimal code. **Auth**: Bearer tokens, certificates, custom headers.

### Key Benefits

**90% less code** vs raw HttpClient. **Resilient**: Automatic retry and circuit breaker. **Type-safe**: No manual JSON. **Production-ready**: Pooled clients, proper disposal.

## Why Myth.Rest?

- **Fluent API**: Intuitive, chainable interface that reads like natural language
- **Enterprise Resilience**: Built-in retry policies with exponential backoff and circuit breakers
- **Type Safety**: Strong typing with automatic JSON serialization/deserialization
- **Zero Configuration**: Works out of the box with sensible defaults
- **Flexible Architecture**: Object pooling for high-performance scenarios
- **DI-First Design**: Seamless ASP.NET Core integration with named configurations
- **Comprehensive File Support**: Upload and download files with minimal code
- **Smart Error Handling**: Conditional error handling with fallback support

## Installation

```bash
dotnet add package Myth.Rest
```

## Quick Start

### Simple GET Request

```csharp
var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://api.github.com")
        .WithHeader("User-Agent", "Myth.Rest"))
    .DoGet("users/octocat")
    .OnResult(result => result.UseTypeForSuccess<User>())
    .OnError(error => error.ThrowForNonSuccess())
    .BuildAsync();

var user = response.GetAs<User>();
```

### POST Request with Body

```csharp
var newUser = new CreateUserRequest {
    Name = "John Doe",
    Email = "john@example.com"
};

var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://api.example.com")
        .WithBearerAuthorization(token))
    .DoPost("users", newUser)
    .OnResult(result => result
        .UseTypeFor<User>(HttpStatusCode.Created)
        .UseTypeFor<ValidationError>(HttpStatusCode.BadRequest))
    .OnError(error => error.ThrowForNonSuccess())
    .BuildAsync();

var user = response.GetAs<User>();
```

## Dependency Injection

### Basic Registration

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRest(config => config
    .WithBaseUrl("https://api.example.com")
    .WithBearerAuthorization(builder.Configuration["ApiToken"])
    .WithRetry());

var app = builder.Build();
```

### Using in Services

```csharp
public class UserService
{
    private readonly IRestRequest _restClient;

    public UserService(IRestRequest restClient)
    {
        _restClient = restClient;
    }

    public async Task<User> GetUserAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _restClient
            .DoGet($"users/{id}")
            .OnResult(r => r.UseTypeForSuccess<User>())
            .OnError(e => e.ThrowForNonSuccess())
            .BuildAsync(cancellationToken);

        return response.GetAs<User>();
    }
}
```

### Factory Pattern (Multiple APIs)

```csharp
builder.Services.AddRestFactory()
    .AddRestConfiguration("github", config => config
        .WithBaseUrl("https://api.github.com")
        .WithHeader("User-Agent", "MyApp"))
    .AddRestConfiguration("stripe", config => config
        .WithBaseUrl("https://api.stripe.com")
        .WithBearerAuthorization(stripeKey));
```

```csharp
public class MultiApiService
{
    private readonly IRestFactory _factory;

    public MultiApiService(IRestFactory factory)
    {
        _factory = factory;
    }

    public async Task<Repository> GetGitHubRepoAsync(string owner, string repo)
    {
        var response = await _factory
            .Create("github")
            .DoGet($"repos/{owner}/{repo}")
            .OnResult(r => r.UseTypeForSuccess<Repository>())
            .BuildAsync();

        return response.GetAs<Repository>();
    }

    public async Task<Charge> CreateStripeChargeAsync(ChargeRequest charge)
    {
        var response = await _factory
            .Create("stripe")
            .DoPost("charges", charge)
            .OnResult(r => r.UseTypeForSuccess<Charge>())
            .BuildAsync();

        return response.GetAs<Charge>();
    }
}
```

## Configuration

### Basic Configuration

```csharp
.Configure(config => config
    .WithBaseUrl("https://api.example.com")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithContentType("application/json")
    .WithBearerAuthorization("your-token")
    .WithHeader("X-Custom-Header", "value")
    .WithBodySerialization(CaseStrategy.CamelCase)
    .WithBodyDeserialization(CaseStrategy.SnakeCase))
```

### Authorization Methods

```csharp
// Bearer Token
.WithBearerAuthorization("your-token")

// Basic Authentication
.WithBasicAuthorization("username", "password")

// Basic Authentication (pre-encoded)
.WithBasicAuthorization("base64EncodedToken")

// Custom Authorization
.WithAuthorization("CustomScheme", "token")
```

### Using HttpClientFactory

```csharp
builder.Services.AddHttpClient("MyApi", client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
});

builder.Services.AddRest(config => config
    .WithHttpClientFactory(serviceProvider.GetRequiredService<IHttpClientFactory>(), "MyApi"));
```

### Type Converters

For APIs that return interfaces, use type converters:

```csharp
.Configure(config => config
    .WithTypeConverter<IUser, UserDto>()
    .WithTypeConverter<IProduct, ProductDto>())
```

### Logging

```csharp
.Configure(config => config
    .WithLogging(logger, logRequests: true, logResponses: true))
```

## Retry Policies

### Default Retry (Recommended)

```csharp
.WithRetry() // 3 attempts, exponential backoff with jitter, server errors only
```

### Custom Retry Strategies

#### Exponential Backoff with Jitter (Production)

```csharp
.WithRetry(retry => retry
    .WithMaxAttempts(5)
    .UseExponentialBackoffWithJitter(
        baseDelay: TimeSpan.FromSeconds(1),
        multiplier: 2.0,
        maxDelay: TimeSpan.FromSeconds(30),
        jitterRange: TimeSpan.FromMilliseconds(100))
    .ForServerErrors()
    .ForExceptions(typeof(TaskCanceledException), typeof(HttpRequestException)))
```

#### Exponential Backoff

```csharp
.WithRetry(retry => retry
    .WithMaxAttempts(3)
    .UseExponentialBackoff(TimeSpan.FromSeconds(1), multiplier: 2.0)
    .ForServerErrors())
```

#### Fixed Delay

```csharp
.WithRetry(retry => retry
    .WithMaxAttempts(3)
    .UseFixedDelay(TimeSpan.FromSeconds(2))
    .ForStatusCodes(HttpStatusCode.ServiceUnavailable))
```

#### Random Delay

```csharp
.WithRetry(retry => retry
    .WithMaxAttempts(3)
    .UseRandom(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
    .ForServerErrors())
```

### Retry Configuration Options

```csharp
.WithRetry(retry => retry
    .WithMaxAttempts(3)
    .ForServerErrors() // 500, 502, 503, 504, 429
    .ForStatusCodes(HttpStatusCode.RequestTimeout, HttpStatusCode.TooManyRequests)
    .ForExceptions(typeof(TaskCanceledException)))
```

## Circuit Breaker

Prevent cascading failures in distributed systems:

```csharp
.WithCircuitBreaker(options => options
    .UseFailureThreshold(5)
    .UseTimeout(TimeSpan.FromMinutes(1))
    .UseHalfOpenRetryTimeout(TimeSpan.FromSeconds(30)))
```

**Circuit Breaker States**:
- **Closed**: Normal operation
- **Open**: Failure threshold exceeded, requests are blocked
- **Half-Open**: Testing if service recovered

## HTTP Operations

### GET

```csharp
.DoGet("users")
.DoGet("users/123")
.DoGet("products?category=electronics&sort=price")
```

### POST

```csharp
.DoPost("users", newUser)
.DoPost("orders", orderRequest)
```

### PUT

```csharp
.DoPut("users/123", updatedUser)
```

### PATCH

```csharp
.DoPatch("users/123", partialUpdate)
```

### DELETE

```csharp
.DoDelete("users/123")
```

## File Operations

### Download Files

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

// Or get as bytes
var bytes = response.ToByteArray();
```

### Upload Files

#### From Stream

```csharp
await using var fileStream = File.OpenRead("document.pdf");

var response = await Rest
    .Create()
    .Configure(config => config.WithBaseUrl("https://api.example.com"))
    .DoUpload("files/upload", fileStream, "application/pdf")
    .OnResult(r => r.UseTypeForSuccess<UploadResult>())
    .BuildAsync();
```

#### From Byte Array

```csharp
var fileBytes = File.ReadAllBytes("image.jpg");

.DoUpload("files/upload", fileBytes, "image/jpeg")
```

#### From IFormFile (ASP.NET Core)

```csharp
[HttpPost("upload")]
public async Task<IActionResult> Upload(IFormFile file)
{
    var response = await _restClient
        .DoUpload("files/upload", file)
        .OnResult(r => r.UseTypeForSuccess<UploadResult>())
        .BuildAsync();

    return Ok(response.GetAs<UploadResult>());
}
```

#### Custom HTTP Method

```csharp
.DoUpload("files/upload", file, settings => settings.UsePutAsMethod())
// Available: UsePostAsMethod(), UsePutAsMethod(), UsePatchAsMethod()
```

## Result Handling

### Type Mapping by Status Code

```csharp
.OnResult(result => result
    .UseTypeForSuccess<User>() // All 2xx codes
    .UseTypeFor<ErrorResponse>(HttpStatusCode.BadRequest)
    .UseTypeFor<ValidationErrors>(HttpStatusCode.UnprocessableEntity)
    .UseEmptyFor(HttpStatusCode.NoContent))
```

### Conditional Type Mapping

For APIs that return different structures with the same status code:

```csharp
.OnResult(result => result
    .UseTypeFor<SuccessResponse>(
        HttpStatusCode.OK,
        body => body.status == "success")
    .UseTypeFor<ErrorResponse>(
        HttpStatusCode.OK,
        body => body.status == "error"))
```

### Multiple Status Codes

```csharp
.OnResult(result => result
    .UseTypeFor<ErrorResponse>(new[] {
        HttpStatusCode.BadRequest,
        HttpStatusCode.Conflict,
        HttpStatusCode.UnprocessableEntity
    }))
```

### All Status Codes

```csharp
.OnResult(result => result.UseTypeForAll<ApiResponse>())
```

### No Type Mapping

```csharp
.OnResult(result => result.DoNotMap())
```

## Error Handling

### Basic Error Handling

```csharp
.OnError(error => error
    .ThrowForNonSuccess() // Throw for any non-2xx status
    .ThrowFor(HttpStatusCode.Unauthorized)
    .NotThrowFor(HttpStatusCode.NotFound))
```

### Conditional Error Handling

```csharp
.OnError(error => error
    .ThrowFor(HttpStatusCode.BadRequest,
        body => body.errorCode == "VALIDATION_FAILED"))
```

### Fallback Responses

Provide default responses for specific error scenarios:

```csharp
.OnError(error => error
    .UseFallback(HttpStatusCode.ServiceUnavailable, new {
        message = "Service temporarily unavailable"
    })
    .UseFallback(HttpStatusCode.NotFound, new User {
        Id = 0,
        Name = "Unknown"
    }))
```

### Don't Throw for Unmapped Results

```csharp
.OnError(error => error
    .ThrowForNonSuccess()
    .NotThrowForNonMappedResult())
```

## Client Certificates (mTLS)

### PEM Certificate with Key

```csharp
.WithCertificate(
    certificatePath: "client-cert.pem",
    keyPath: "client-key.pem",
    keyPassword: "optional-password")
```

### PFX Certificate

```csharp
// From file
.WithCertificate(pfxPath: "client-cert.pfx", password: "password")

// From bytes
var pfxData = File.ReadAllBytes("client-cert.pfx");
.WithCertificate(pfxData: pfxData, password: "password")
```

### From Certificate Store

```csharp
// By thumbprint
.WithCertificateFromStore(
    thumbprint: "A1B2C3D4E5F6...",
    storeLocation: StoreLocation.CurrentUser)

// By subject name
.WithCertificateFromStoreBySubject(
    subjectName: "CN=MyCert",
    storeLocation: StoreLocation.LocalMachine)
```

### X509Certificate2 Instance

```csharp
var certificate = new X509Certificate2("client-cert.pfx", "password");
.WithCertificate(certificate)
```

### Advanced Certificate Configuration

```csharp
.WithCertificate(options =>
{
    options.Type = CertificateType.PemWithKey;
    options.CertificatePath = "client-cert.pem";
    options.KeyPath = "client-key.pem";
    options.ValidateServerCertificate = false; // For development only
    options.ServerCertificateValidationCallback = (sender, cert, chain, errors) =>
    {
        // Custom validation logic
        return true;
    };
})
```

## Advanced Patterns

### Repository Pattern

```csharp
public class UserRepository : IUserRepository
{
    private readonly IRestRequest _client;

    public UserRepository(IRestRequest client)
    {
        _client = client;
    }

    public async Task<User> GetByIdAsync(int id, CancellationToken cancellationToken = default)
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

    public async Task<User> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
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

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _client
            .DoPut($"users/{id}", request)
            .OnResult(r => r.UseEmptyFor(HttpStatusCode.NoContent))
            .OnError(e => e.ThrowForNonSuccess())
            .BuildAsync(cancellationToken);

        return response.IsSuccessStatusCode();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _client
            .DoDelete($"users/{id}")
            .OnResult(r => r.UseEmptyFor(HttpStatusCode.NoContent))
            .OnError(e => e
                .ThrowForNonSuccess()
                .NotThrowFor(HttpStatusCode.NotFound))
            .BuildAsync(cancellationToken);

        return response.IsSuccessStatusCode();
    }
}
```

### Working with Legacy APIs

Some APIs return 200 OK for all responses and use body fields to indicate errors:

```csharp
var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://legacy-api.com")
        .WithRetry())
    .DoGet("users")
    .OnResult(result => result
        .UseTypeFor<List<User>>(
            HttpStatusCode.OK,
            body => body.success == true))
    .OnError(error => error
        .ThrowFor(
            HttpStatusCode.OK,
            body => body.success == false)
        .ThrowForNonSuccess())
    .BuildAsync();
```

### Microservices Communication

```csharp
builder.Services.AddRestFactory()
    .AddRestConfiguration("user-service", config => config
        .WithBaseUrl("http://user-service:8080")
        .WithCircuitBreaker(options => options
            .UseFailureThreshold(5)
            .UseTimeout(TimeSpan.FromMinutes(1)))
        .WithRetry(retry => retry
            .WithMaxAttempts(3)
            .UseExponentialBackoffWithJitter(TimeSpan.FromSeconds(1))
            .ForServerErrors()))
    .AddRestConfiguration("order-service", config => config
        .WithBaseUrl("http://order-service:8080")
        .WithCircuitBreaker(options => options
            .UseFailureThreshold(3)
            .UseTimeout(TimeSpan.FromMinutes(2)))
        .WithRetry(retry => retry
            .WithMaxAttempts(2)
            .UseFixedDelay(TimeSpan.FromSeconds(2))));
```

### Resilient E-Commerce Service

```csharp
public class ProductService
{
    private readonly IRestFactory _factory;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IRestFactory factory, ILogger<ProductService> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    public async Task<Product> GetProductAsync(string productId)
    {
        try
        {
            var response = await _factory
                .Create("catalog")
                .DoGet($"products/{productId}")
                .OnResult(r => r.UseTypeForSuccess<Product>())
                .OnError(e => e
                    .ThrowForNonSuccess()
                    .UseFallback(HttpStatusCode.ServiceUnavailable, new Product
                    {
                        Id = productId,
                        Name = "Product Unavailable",
                        Available = false
                    }))
                .BuildAsync();

            _logger.LogInformation(
                "Retrieved product {ProductId} in {ElapsedTime}ms with {Retries} retries",
                productId,
                response.ElapsedTime.TotalMilliseconds,
                response.RetriesMade);

            return response.GetAs<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve product {ProductId}", productId);
            throw;
        }
    }
}
```

## Response Metadata

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

// Access results
var user = response.GetAs<User>(); // Typed result
var json = response.ToString(); // JSON string
var bytes = response.ToByteArray(); // Byte array
var stream = response.ToStream(); // Stream
dynamic dyn = response.DynamicResult; // Dynamic object
```

## Exception Types

```csharp
try
{
    var response = await Rest.Create()...BuildAsync();
}
catch (NonSuccessException ex)
{
    // HTTP error status codes
    Console.WriteLine($"Status: {ex.Response.StatusCode}");
    Console.WriteLine($"Content: {ex.Response}");
}
catch (NotMappedResultTypeException ex)
{
    // No type mapping found for status code
    Console.WriteLine($"Unmapped status: {ex.StatusCode}");
}
catch (DifferentResponseTypeException ex)
{
    // Attempting to cast to wrong type
    Console.WriteLine($"Expected: {ex.ExpectedType}, Actual: {ex.ActualType}");
}
catch (ParsingTypeException ex)
{
    // JSON deserialization failed
    Console.WriteLine($"Failed to parse: {ex.Content}");
}
catch (FileAlreadyExsistsOnDownloadException ex)
{
    // File exists during download
    Console.WriteLine($"File exists: {ex.FilePath}");
}
catch (NoActionMadeException)
{
    // No HTTP action was defined
}
catch (CircuitBreakerOpenException ex)
{
    // Circuit breaker is open
    Console.WriteLine("Service circuit breaker is open");
}
```

## Best Practices

1. **Always Use Dependency Injection**: Register REST clients as services for better testability and configuration management

2. **Configure Retry Policies**: Use retry policies in production for transient failure resilience

3. **Implement Circuit Breakers**: Prevent cascade failures in distributed systems

4. **Use Named Configurations**: Leverage the factory pattern when integrating with multiple APIs

5. **Handle Errors Gracefully**: Use fallbacks for non-critical operations

6. **Leverage Strong Typing**: Always map responses to strongly typed models

7. **Configure Timeouts**: Set appropriate timeouts based on your API characteristics

8. **Enable Logging**: Use structured logging for debugging and monitoring

9. **Secure Sensitive Headers**: Use specific authorization methods instead of manual headers for tokens

10. **Use CancellationTokens**: Always pass cancellation tokens to support request cancellation

## Performance

Myth.Rest uses `ObjectPool<RestBuilder>` internally to minimize allocations and improve performance in high-throughput scenarios. The pooling is transparent and automatic.

## License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.
