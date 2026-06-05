---
name: myth-rest
description: Use when you need to make external HTTP calls. Rest.Create() builds requests fluently with .DoGet()/.DoPost()/.DoPut()/.DoDelete(), auto JSON serialization, retry policies (fixed, exponential backoff, jitter), circuit breaker, Bearer/Basic/certificate auth, file upload/download, and IHttpClientFactory integration. Supports named configurations for multiple API clients.
---

# Myth.Rest - Fluent REST Client with Resilience Patterns

## Overview

Myth.Rest is a modern, fluent REST client for .NET that provides enterprise-grade resilience patterns. It eliminates HttpClient boilerplate with a chainable API, automatic JSON handling, built-in retry policies, circuit breakers, and proper resource management through object pooling.

**Key Features:**
- **Fluent API**: Chainable, readable interface for building HTTP requests
- **Resilience Patterns**: Built-in retry policies (exponential backoff, jitter) and circuit breakers
- **Type Safety**: Automatic JSON serialization/deserialization with strong typing
- **Object Pooling**: High-performance HTTP client pooling for reduced allocations
- **File Operations**: Simple upload/download with stream, byte array, and IFormFile support
- **Authentication**: Bearer tokens, Basic auth, certificates (PEM, PFX, Store), custom headers
- **Smart Error Handling**: Conditional error handling with fallback responses
- **DI-First Design**: Seamless integration with ASP.NET Core dependency injection
- **Named Configurations**: Factory pattern for managing multiple API clients

**Dependencies:**
- .NET 10.0+
- Myth.Commons (base types)
- ASP.NET Core 10.0+

---

## Installation

```bash
dotnet add package Myth.Rest
```

---

## Core Concepts

### 1. Fluent API Pattern

Build HTTP requests using a fluent, chainable interface:
```
Rest.Create() → Configure() → DoXXX() → OnResult() → OnError() → BuildAsync()
```

### 2. Resilience Patterns

**Retry Policies:**
- Fixed delay
- Exponential backoff
- Exponential backoff with jitter (recommended for production)
- Random delay

**Circuit Breaker:**
- Protects against cascading failures
- States: Closed (normal), Open (failures exceeded), HalfOpen (testing recovery)

### 3. Type Mapping

Map HTTP status codes to specific types:
- Different types for different status codes (201 → User, 400 → ValidationError)
- Conditional mapping based on response body content
- Fallback responses for error scenarios

### 4. Resource Management

**Object Pooling:**
- `RestBuilder` instances are pooled automatically
- Reduces allocations in high-throughput scenarios
- Transparent to the consumer

**HttpClient Management:**
- Proper disposal through HttpClientHandler
- Integration with IHttpClientFactory
- Supports pre-configured HttpClient injection

---

## API Reference

### Rest (Entry Point)

```csharp
public static class Rest {
    // Create new REST client instance
    static IRestBuilder Create();
}
```

### IRestRequest

```csharp
public interface IRestRequest {
    // HTTP Methods
    IRestPostProcessing DoGet(string url);
    IRestPostProcessing DoPost<TBody>(string url, TBody? body = default);
    IRestPostProcessing DoPut<TBody>(string url, TBody? body = default);
    IRestPostProcessing DoPatch<TBody>(string url, TBody? body = default);
    IRestPostProcessing DoDelete(string url);

    // File Operations
    IRestPostProcessing DoDownload(string url);
    IRestPostProcessing DoUpload<T>(string url, T body, string contentType, Action<RestUploadSettings>? settings = null);
    IRestPostProcessing DoUpload(string url, Stream stream, string contentType, Action<RestUploadSettings>? settings = null);
    IRestPostProcessing DoUpload(string url, IFormFile file, Action<RestUploadSettings>? settings = null);
    IRestPostProcessing DoUpload(string url, HttpContent content, Action<RestUploadSettings>? settings = null);

    // Configuration
    IRestRequest Configure(Action<ConfigurationBuilder> configurationBuilder);
}
```

### ConfigurationBuilder

```csharp
public class ConfigurationBuilder {
    // Base Configuration
    ConfigurationBuilder WithBaseUrl(string baseUrl);
    ConfigurationBuilder WithTimeout(TimeSpan timeout);
    ConfigurationBuilder WithClient(HttpClient httpClient);
    ConfigurationBuilder WithHttpClientFactory(IHttpClientFactory factory, string name = "default");

    // Authentication
    ConfigurationBuilder WithAuthorization(string scheme, string token);
    ConfigurationBuilder WithBearerAuthorization(string token);
    ConfigurationBuilder WithBasicAuthorization(string username, string password);
    ConfigurationBuilder WithBasicAuthorization(string encodedToken);

    // Headers and Content
    ConfigurationBuilder WithContentType(string contentType);
    ConfigurationBuilder WithHeader(string key, string value);

    // Serialization
    ConfigurationBuilder WithBodySerialization(CaseStrategy serializationCaseStrategy);
    ConfigurationBuilder WithBodyDeserialization(CaseStrategy deserializationCaseStrategy);
    ConfigurationBuilder WithTypeConverter<TInterface, TType>();

    // Retry Policies
    ConfigurationBuilder WithRetry(); // Default: 3 attempts, exponential backoff with jitter, server errors
    ConfigurationBuilder WithRetry(Action<RetryPolicy> configure);
    ConfigurationBuilder WithRetry(int amount, TimeSpan timeBetweenRetries, params HttpStatusCode[] statusCodes);

    // Circuit Breaker
    ConfigurationBuilder WithCircuitBreaker(ICircuitBreaker circuitBreaker);
    ConfigurationBuilder WithCircuitBreaker(Action<CircuitBreakerSettings>? options);

    // Certificates (mTLS)
    ConfigurationBuilder WithCertificate(string certificatePath, string keyPath, string? keyPassword = null); // PEM
    ConfigurationBuilder WithCertificate(string pfxPath, string password); // PFX file
    ConfigurationBuilder WithCertificate(byte[] pfxData, string password); // PFX bytes
    ConfigurationBuilder WithCertificate(X509Certificate2 certificate); // X509Certificate2
    ConfigurationBuilder WithCertificateFromStore(string thumbprint, StoreLocation storeLocation = StoreLocation.CurrentUser);
    ConfigurationBuilder WithCertificateFromStoreBySubject(string subjectName, StoreLocation storeLocation = StoreLocation.CurrentUser);
    ConfigurationBuilder WithCertificate(Action<CertificateOptions> configure); // Advanced

    // Logging
    ConfigurationBuilder WithLogging(ILogger logger, bool logRequests = true, bool logResponses = true);
}
```

### RetryPolicy

```csharp
public class RetryPolicy {
    // Retry Strategies
    RetryPolicy UseFixedDelay(TimeSpan delay);
    RetryPolicy UseExponentialBackoff(TimeSpan baseDelay, double multiplier = 2.0, TimeSpan? maxDelay = null);
    RetryPolicy UseExponentialBackoffWithJitter(TimeSpan baseDelay, double multiplier = 2.0, TimeSpan? maxDelay = null, TimeSpan? jitterRange = null);
    RetryPolicy UseRandom(TimeSpan minDelay, TimeSpan maxDelay);

    // Configuration
    RetryPolicy WithMaxAttempts(int maxAttempts);
    RetryPolicy ForStatusCodes(params HttpStatusCode[] statusCodes);
    RetryPolicy ForExceptions(params Type[] exceptionTypes);
    RetryPolicy ForServerErrors(); // 500, 502, 503, 504, 429

    // Calculate delay for attempt
    TimeSpan CalculateDelay(int attemptNumber);

    // Properties
    int AmountRetries { get; }
    RetryStrategy Strategy { get; }
    TimeSpan MinDelay { get; }
    TimeSpan MaxDelay { get; }
}
```

### CircuitBreakerSettings

```csharp
public class CircuitBreakerSettings {
    CircuitBreakerSettings UseFailureThreshold(int failureThreshold); // Default: 5
    CircuitBreakerSettings UseTimeout(TimeSpan timeout); // Default: 1 minute
    CircuitBreakerSettings UseHalfOpenRetryTimeout(TimeSpan halfOpenRetryTimeout); // Default: 30 seconds
}
```

### IRestPostProcessing

```csharp
public interface IRestPostProcessing {
    // Result Configuration
    IRestResultHandler OnResult(Action<ResultBuilder> resultSettings);

    // Error Configuration
    IRestErrorHandler OnError(Action<ErrorBuilder> errorSettings);

    // Build and Execute
    Task<RestResponse> BuildAsync(CancellationToken cancellationToken = default);
    Task<RestResponse> BuildAsync<TResult>(CancellationToken cancellationToken = default);
}
```

### ResultBuilder

```csharp
public class ResultBuilder {
    // Type Mapping
    ResultBuilder UseTypeForSuccess<T>(); // All 2xx codes
    ResultBuilder UseTypeFor<T>(HttpStatusCode statusCode);
    ResultBuilder UseTypeFor<T>(HttpStatusCode statusCode, Func<dynamic, bool> condition);
    ResultBuilder UseTypeFor<T>(HttpStatusCode[] statusCodes);
    ResultBuilder UseTypeForAll<T>(); // All status codes

    // Special Mappings
    ResultBuilder UseEmptyFor(HttpStatusCode statusCode);
    ResultBuilder DoNotMap();
}
```

### ErrorBuilder

```csharp
public class ErrorBuilder {
    // Throw Configuration
    ErrorBuilder ThrowForNonSuccess(); // Throw for any non-2xx
    ErrorBuilder ThrowFor(HttpStatusCode statusCode);
    ErrorBuilder ThrowFor(HttpStatusCode statusCode, Func<dynamic, bool> condition);
    ErrorBuilder NotThrowFor(HttpStatusCode statusCode);
    ErrorBuilder NotThrowForNonMappedResult();

    // Fallback Responses
    ErrorBuilder UseFallback(HttpStatusCode statusCode, object fallbackValue);
}
```

### RestResponse

```csharp
public class RestResponse {
    // Status
    HttpStatusCode StatusCode { get; }
    bool IsSuccessStatusCode();

    // Request Info
    string Url { get; }
    string Method { get; }

    // Performance Metrics
    TimeSpan ElapsedTime { get; }
    int RetriesMade { get; }
    bool FallbackUsed { get; }

    // Result Access
    T GetAs<T>();
    string ToString();
    byte[] ToByteArray();
    Stream ToStream();
    dynamic DynamicResult { get; }

    // File Operations
    Task SaveToFileAsync(string directory, string fileName, bool replaceExisting = false);
}
```

### IRestFactory

```csharp
public interface IRestFactory {
    // Create REST client with named configuration
    IRestRequest Create(string configurationName);

    // Create REST client with custom configuration
    IRestRequest Create(Action<ConfigurationBuilder> configurationBuilder);
}
```

### ServiceCollectionExtensions

```csharp
public static class ServiceCollectionExtensions {
    // Single configuration
    static IServiceCollection AddRest(this IServiceCollection services, Action<ConfigurationBuilder> configure);

    // Factory pattern for multiple APIs
    static IRestFactoryBuilder AddRestFactory(this IServiceCollection services);
}

public interface IRestFactoryBuilder {
    IRestFactoryBuilder AddRestConfiguration(string name, Action<ConfigurationBuilder> configure);
}
```

---

## Usage Examples

### 1. Simple GET Request

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

### 2. POST with Authentication

```csharp
var newUser = new CreateUserRequest {
    Name = "John Doe",
    Email = "john@example.com"
};

var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://api.example.com")
        .WithBearerAuthorization(token)
        .WithRetry()) // Default retry policy
    .DoPost("users", newUser)
    .OnResult(result => result
        .UseTypeFor<User>(HttpStatusCode.Created)
        .UseTypeFor<ValidationError>(HttpStatusCode.BadRequest))
    .OnError(error => error.ThrowForNonSuccess())
    .BuildAsync();

var user = response.GetAs<User>();
```

### 3. Dependency Injection (Single API)

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRest(config => config
    .WithBaseUrl("https://api.example.com")
    .WithBearerAuthorization(builder.Configuration["ApiToken"])
    .WithRetry(retry => retry
        .WithMaxAttempts(5)
        .UseExponentialBackoffWithJitter(TimeSpan.FromSeconds(1))
        .ForServerErrors())
    .WithCircuitBreaker(options => options
        .UseFailureThreshold(10)
        .UseTimeout(TimeSpan.FromMinutes(2))));

var app = builder.Build();

// Service
public class UserService {
    private readonly IRestRequest _restClient;

    public UserService(IRestRequest restClient) {
        _restClient = restClient;
    }

    public async Task<User> GetUserAsync(int id, CancellationToken cancellationToken) {
        var response = await _restClient
            .DoGet($"users/{id}")
            .OnResult(r => r.UseTypeForSuccess<User>())
            .OnError(e => e.ThrowForNonSuccess())
            .BuildAsync(cancellationToken);

        return response.GetAs<User>();
    }
}
```

### 4. Factory Pattern (Multiple APIs)

```csharp
// Program.cs
builder.Services.AddRestFactory()
    .AddRestConfiguration("github", config => config
        .WithBaseUrl("https://api.github.com")
        .WithHeader("User-Agent", "MyApp")
        .WithRetry())
    .AddRestConfiguration("stripe", config => config
        .WithBaseUrl("https://api.stripe.com")
        .WithBearerAuthorization(stripeKey)
        .WithRetry());

// Service
public class MultiApiService {
    private readonly IRestFactory _factory;

    public MultiApiService(IRestFactory factory) {
        _factory = factory;
    }

    public async Task<Repository> GetGitHubRepoAsync(string owner, string repo) {
        var response = await _factory
            .Create("github")
            .DoGet($"repos/{owner}/{repo}")
            .OnResult(r => r.UseTypeForSuccess<Repository>())
            .BuildAsync();

        return response.GetAs<Repository>();
    }

    public async Task<Charge> CreateStripeChargeAsync(ChargeRequest charge) {
        var response = await _factory
            .Create("stripe")
            .DoPost("charges", charge)
            .OnResult(r => r.UseTypeForSuccess<Charge>())
            .BuildAsync();

        return response.GetAs<Charge>();
    }
}
```

### 5. Advanced Retry Configuration

```csharp
.Configure(config => config
    .WithBaseUrl("https://api.example.com")
    .WithRetry(retry => retry
        .WithMaxAttempts(5)
        .UseExponentialBackoffWithJitter(
            baseDelay: TimeSpan.FromSeconds(1),
            multiplier: 2.0,
            maxDelay: TimeSpan.FromSeconds(30),
            jitterRange: TimeSpan.FromMilliseconds(100))
        .ForServerErrors() // 500, 502, 503, 504, 429
        .ForStatusCodes(HttpStatusCode.RequestTimeout)
        .ForExceptions(typeof(TaskCanceledException), typeof(HttpRequestException))))
```

### 6. Circuit Breaker Configuration

```csharp
.Configure(config => config
    .WithBaseUrl("https://api.example.com")
    .WithCircuitBreaker(options => options
        .UseFailureThreshold(5) // Open after 5 failures
        .UseTimeout(TimeSpan.FromMinutes(1)) // Stay open for 1 minute
        .UseHalfOpenRetryTimeout(TimeSpan.FromSeconds(30)))) // Test recovery after 30s
```

### 7. File Download

```csharp
var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://api.example.com")
        .WithBearerAuthorization(token))
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

### 8. File Upload (Multiple Methods)

```csharp
// From Stream
await using var fileStream = File.OpenRead("document.pdf");
var response = await Rest
    .Create()
    .Configure(config => config.WithBaseUrl("https://api.example.com"))
    .DoUpload("files/upload", fileStream, "application/pdf")
    .OnResult(r => r.UseTypeForSuccess<UploadResult>())
    .BuildAsync();

// From Byte Array
var fileBytes = File.ReadAllBytes("image.jpg");
await Rest.Create()
    .Configure(config => config.WithBaseUrl("https://api.example.com"))
    .DoUpload("files/upload", fileBytes, "image/jpeg")
    .BuildAsync();

// From IFormFile (ASP.NET Core)
[HttpPost("upload")]
public async Task<IActionResult> Upload(IFormFile file) {
    var response = await _restClient
        .DoUpload("files/upload", file)
        .OnResult(r => r.UseTypeForSuccess<UploadResult>())
        .BuildAsync();

    return Ok(response.GetAs<UploadResult>());
}

// Custom HTTP Method
.DoUpload("files/upload", file, settings => settings.UsePutAsMethod())
```

### 9. mTLS with Certificates

```csharp
// PEM Certificate with Key
.Configure(config => config
    .WithCertificate(
        certificatePath: "client-cert.pem",
        keyPath: "client-key.pem",
        keyPassword: "optional-password"))

// PFX from file
.Configure(config => config
    .WithCertificate(pfxPath: "client-cert.pfx", password: "password"))

// PFX from bytes
var pfxData = File.ReadAllBytes("client-cert.pfx");
.Configure(config => config
    .WithCertificate(pfxData: pfxData, password: "password"))

// From Certificate Store by thumbprint
.Configure(config => config
    .WithCertificateFromStore(
        thumbprint: "A1B2C3D4E5F6...",
        storeLocation: StoreLocation.CurrentUser))

// From Certificate Store by subject name
.Configure(config => config
    .WithCertificateFromStoreBySubject(
        subjectName: "CN=MyCert",
        storeLocation: StoreLocation.LocalMachine))

// X509Certificate2 instance
var certificate = new X509Certificate2("client-cert.pfx", "password");
.Configure(config => config.WithCertificate(certificate))
```

### 10. Conditional Type Mapping

```csharp
// APIs that return different structures with same status code
.OnResult(result => result
    .UseTypeFor<SuccessResponse>(
        HttpStatusCode.OK,
        body => body.status == "success")
    .UseTypeFor<ErrorResponse>(
        HttpStatusCode.OK,
        body => body.status == "error"))
```

### 11. Error Handling with Fallbacks

```csharp
var response = await _restClient
    .DoGet($"users/{id}")
    .OnResult(r => r.UseTypeForSuccess<User>())
    .OnError(error => error
        .ThrowForNonSuccess()
        .UseFallback(HttpStatusCode.NotFound, new User {
            Id = id,
            Name = "Unknown"
        })
        .UseFallback(HttpStatusCode.ServiceUnavailable, new User {
            Id = id,
            Name = "Service Unavailable"
        }))
    .BuildAsync();

var user = response.GetAs<User>();
```

### 12. Legacy API Support (Body-Based Errors)

```csharp
// Some legacy APIs return 200 OK for everything and use body fields
var response = await Rest
    .Create()
    .Configure(config => config.WithBaseUrl("https://legacy-api.com"))
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

### 13. Repository Pattern

```csharp
public class UserRepository : IUserRepository {
    private readonly IRestRequest _client;

    public UserRepository(IRestRequest client) {
        _client = client;
    }

    public async Task<User> GetByIdAsync(int id, CancellationToken cancellationToken) {
        var response = await _client
            .DoGet($"users/{id}")
            .OnResult(r => r.UseTypeForSuccess<User>())
            .OnError(e => e
                .ThrowForNonSuccess()
                .UseFallback(HttpStatusCode.NotFound, new User {
                    Id = id,
                    Name = "Unknown"
                }))
            .BuildAsync(cancellationToken);

        return response.GetAs<User>();
    }

    public async Task<User> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken) {
        var response = await _client
            .DoPost("users", request)
            .OnResult(r => r
                .UseTypeFor<User>(HttpStatusCode.Created)
                .UseTypeFor<ValidationErrorResponse>(HttpStatusCode.BadRequest))
            .OnError(e => e.ThrowForNonSuccess())
            .BuildAsync(cancellationToken);

        return response.GetAs<User>();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken) {
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

### 14. Microservices Communication

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
            .UseTimeout(TimeSpan.FromMinutes(2))));
```

### 15. Response Metadata Access

```csharp
var response = await _restClient.DoGet("users").BuildAsync();

Console.WriteLine($"Status: {response.StatusCode}");
Console.WriteLine($"URL: {response.Url}");
Console.WriteLine($"Method: {response.Method}");
Console.WriteLine($"Elapsed Time: {response.ElapsedTime}");
Console.WriteLine($"Retries Made: {response.RetriesMade}");
Console.WriteLine($"Fallback Used: {response.FallbackUsed}");
Console.WriteLine($"Is Success: {response.IsSuccessStatusCode()}");
```

---

## Best Practices

### 1. Dependency Injection

**✅ DO:**
- Always use DI for REST clients in production applications
- Use named configurations (factory pattern) for multiple APIs
- Register with appropriate lifetime (typically Scoped for web apps)

**❌ DON'T:**
- Create REST clients with `Rest.Create()` in production code (OK for testing)
- Share single IRestRequest across multiple unrelated APIs

### 2. Retry Policies

**✅ DO:**
- Always configure retry policies for production
- Use exponential backoff with jitter to avoid thundering herd
- Retry only on transient failures (server errors, timeouts)
- Set reasonable max attempts (3-5)

**❌ DON'T:**
- Retry on client errors (400, 401, 403, 404)
- Use fixed delay in production (causes synchronized retries)
- Set unlimited retries

### 3. Circuit Breakers

**✅ DO:**
- Use circuit breakers for external service communication
- Configure based on service SLA and expected failure rates
- Monitor circuit breaker state for alerting
- Use in microservices architectures

**❌ DON'T:**
- Use circuit breakers for internal services in monoliths
- Set thresholds too low (causes unnecessary circuit opens)
- Forget to configure HalfOpen retry timeout

### 4. Error Handling

**✅ DO:**
- Map different status codes to different types
- Use fallbacks for non-critical operations
- Handle specific error scenarios gracefully
- Log errors with request metadata

**❌ DON'T:**
- Swallow exceptions silently
- Use generic catch-all error handling
- Ignore non-success status codes

### 5. Type Safety

**✅ DO:**
- Always map responses to strongly typed models
- Use different types for different status codes
- Validate deserialization results

**❌ DON'T:**
- Use `dynamic` or `object` in production code
- Assume successful deserialization without validation

### 6. Performance

**✅ DO:**
- Reuse HttpClient through DI (automatic in Myth.Rest)
- Set appropriate timeouts
- Use object pooling (automatic in Myth.Rest)
- Pass CancellationTokens

**❌ DON'T:**
- Create new HttpClient for each request manually
- Use infinite timeouts
- Block async calls with `.Result` or `.Wait()`

---

## Exception Reference

```csharp
// Non-success HTTP status codes
catch (NonSuccessException ex) {
    Console.WriteLine($"Status: {ex.Response.StatusCode}");
    Console.WriteLine($"Content: {ex.Response}");
}

// No type mapping found for status code
catch (NotMappedResultTypeException ex) {
    Console.WriteLine($"Unmapped status: {ex.StatusCode}");
}

// Attempting to cast to wrong type
catch (DifferentResponseTypeException ex) {
    Console.WriteLine($"Expected: {ex.ExpectedType}, Actual: {ex.ActualType}");
}

// JSON deserialization failed
catch (ParsingTypeException ex) {
    Console.WriteLine($"Failed to parse: {ex.Content}");
}

// File exists during download
catch (FileAlreadyExsistsOnDownloadException ex) {
    Console.WriteLine($"File exists: {ex.FilePath}");
}

// No HTTP action was defined
catch (NoActionMadeException) {
    Console.WriteLine("No HTTP method was called");
}

// Circuit breaker is open
catch (CircuitBreakerOpenException) {
    Console.WriteLine("Service circuit breaker is open");
}
```

---

## Summary

Myth.Rest eliminates HttpClient boilerplate while providing enterprise-grade features:

- **90% Less Code**: Fluent API reduces HTTP client code dramatically
- **Production-Ready**: Built-in retry policies and circuit breakers
- **Type-Safe**: Automatic JSON handling with strong typing
- **High Performance**: Object pooling and proper resource management
- **Flexible**: Supports simple use cases to complex microservices communication

Use Myth.Rest to build resilient, maintainable HTTP clients with minimal code.
