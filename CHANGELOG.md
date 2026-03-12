# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Myth.Flow.Actions

#### ✨ Added

- **ICacheManager Interface**
  - Added public `ICacheManager` interface for manual cache management and invalidation
  - Provides controlled access to cache operations without exposing internal `ICacheProvider`
  - Methods:
    - `GetAsync<T>(string key)` - retrieves cached value by key
    - `SetAsync<T>(string key, T value, TimeSpan ttl)` - sets cache value with TTL
    - `InvalidateAsync(string key)` - invalidates specific cache entry
    - `InvalidateByPatternAsync(string pattern)` - invalidates cache entries matching pattern (e.g., "User:*")
    - `InvalidateByTypeAsync<TQuery>(TQuery? query)` - invalidates cache for query type (specific instance or all)
    - `GenerateKey<TQuery>(TQuery query)` - generates cache key compatible with Dispatcher
  - Automatically registered in DI when caching is enabled
  - Use case: Invalidate cached queries after commands that modify data
  - Example: `await cacheManager.InvalidateByTypeAsync(new GetUserQuery { Id = userId })`

- **CacheKeyGenerator Utility**
  - Extracted cache key generation logic from Dispatcher into reusable `CacheKeyGenerator` class
  - Ensures consistent cache key generation between Dispatcher and CacheManager
  - Uses deterministic JSON serialization with SHA256 hashing
  - Format: `{TypeName}:{Hash}` (e.g., "GetUserQuery:A1B2C3D4E5F6G7H8")

#### 🔄 Changed

- **Dispatcher Cache Key Generation**
  - Refactored `GenerateCacheKey()` method to use shared `CacheKeyGenerator.Generate()`
  - Removed duplicate SHA256 hashing logic from Dispatcher
  - Fixed obsolete `JsonSerializerOptions.IgnoreNullValues` usage, replaced with `DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`

#### 📝 Documentation

- Added comprehensive cache management section to README.md and README.pt-br.md
- Documented all `ICacheManager` methods with usage examples
- Added note about limited pattern support in MemoryCache vs Redis

### Myth.DependencyInjection

#### 🐛 Fixed

- **ReflectionTypeLoadException in TypeProvider**
  - Fixed random `ReflectionTypeLoadException` when calling `services.AddRepositories()` or other auto-registration methods
  - Added proper exception handling for `ReflectionTypeLoadException` in `GetTypesFromAssembly()` method
  - When assembly type loading fails, only successfully loaded types are returned instead of throwing exception
  - Added filtering to exclude known problematic system assemblies (Microsoft.Build, Microsoft.CodeAnalysis, etc.)
  - Prevents loading types from assemblies with missing dependencies like `Microsoft.Build` version conflicts
  - Impact: Resolves issue where `AddRepositories()` would fail with "Method 'ImportMetadata' does not have an implementation" error

### Myth.Morph

#### 🐛 Fixed

- **ReflectionTypeLoadException in SchemaRegistry**
  - Fixed verbose warning logs when registering morph profiles with problematic assemblies
  - Added `GetTypesFromAssembly()` method with proper exception handling for `ReflectionTypeLoadException`
  - Added `IsSystemAssembly()` filter to exclude known problematic system assemblies (Microsoft.Build, Microsoft.CodeAnalysis, etc.)
  - When assembly type loading fails, only successfully loaded types are used for profile registration
  - Reduced log level from `Warning` to `Debug` for partial type loading to minimize noise
  - Impact: Eliminates verbose exception logs during `AddMorph()` initialization while maintaining functionality

### Myth.Guard

#### ✨ Added

- **Native Nullable Struct Support (`NotDefault` for `T?`)**
  - `NotDefault()` on `FluentRuleBuilder<T?>` (where `T : struct`) now validates the unwrapped value against the struct's default
  - If the value is `null`, validation passes (use `NotNull()` to reject null values separately)
  - If the value is present but equals the default (e.g., `Guid.Empty` for `Guid?`, `0` for `int?`), validation fails
  - Eliminates the need to manually unwrap `.Value` before calling `NotDefault()`, which caused incorrect field name capture via `CallerArgumentExpression`
  - Example: `builder.For(UserId, rules => rules.NotDefault().WithMessage("UserId is required"))`

- **Manual Validation Failure Methods**
  - Added `Sentry.Fail()` methods for manually throwing validation exceptions
  - Overloads: `Fail(string message)`, `Fail(string field, string message)`, `Fail(ValidationError error)`, `Fail(IEnumerable<ValidationError> errors)`
  - Allows manual validation failure with custom messages and status codes
  - Supports throwing multiple validation errors at once

- **Dictionary Validation Support**
  - Added `IDictionaryRules<TKey, TValue>` interface with comprehensive dictionary validation rules
  - Added `Sentry.For<TKey, TValue>(IDictionary<TKey, TValue>? value)` for standalone dictionary validation
  - Added dictionary validation support in `IValidatable<T>` entities via `builder.For(dictionary, r => ...)`
  - Dictionary validation rules:
    - `NotEmpty()` - validates dictionary is not null and has entries
    - `CountGreaterThan(min)` - validates entry count exceeds minimum
    - `CountLessThan(max)` - validates entry count is below maximum
    - `CountBetween(min, max)` - validates entry count is within range
    - `ContainsKey(key)` - validates specific key exists
    - `NotContainsKey(key)` - validates specific key does not exist
    - `ContainsValue(value)` - validates specific value exists
    - `AllKeys(predicate)` - validates all keys satisfy condition
    - `AllValues(predicate)` - validates all values satisfy condition
    - `AnyKey(predicate)` - validates at least one key satisfies condition
    - `AnyValue(predicate)` - validates at least one value satisfies condition
    - `NoKeys(predicate)` - validates no keys satisfy condition
    - `NoValues(predicate)` - validates no values satisfy condition

#### 🚨 BREAKING CHANGES

- **Migrated to RFC 9457 (Problem Details for HTTP APIs)**
  - Removed `Code` property from `ValidationError` class
  - Removed `WithCode()` method from all validation builders (`FluentRuleBuilder`, `StandaloneValidationBuilder`)
  - Removed `code` parameter from `ValidationError` constructor
  - Removed `code` parameter from `MultiValidationResult.Failure()` method
  - Removed `code` parameter from `StandaloneValidationResult.Failure()` method
  - Deleted obsolete models: `ErrorDetail` and `ValidationErrorResponse`

#### ✨ Added

- **RFC 9457 Support**
  - Middleware now returns `ValidationProblemDetails` (native Microsoft type)
  - Response format follows RFC 9457 standard with `type`, `title`, `status`, `instance`, `errors`, and `traceId`
  - Content-Type changed to `application/problem+json` for validation errors
  - Added `traceId` field for distributed tracing correlation
  - Added `options` in Extensions for enum/constant validation errors
  - Error documentation at `docs/errors/validation.md`

- **Documentation**
  - Created comprehensive error documentation (`docs/errors/validation.md`)
  - Added RFC 9457 examples to README.md and README.pt-br.md
  - Removed all references to `WithCode()` from documentation

#### 🔄 Changed

- **Error Response Format**
  - Old format:
    ```json
    {
      "code": "MULTIPLE_ERRORS",
      "errors": [
        {
          "field": "email",
          "message": "Email is required",
          "code": "VIOLATION"
        }
      ]
    }
    ```
  - New format (RFC 9457):
    ```json
    {
      "type": "https://github.com/paulaolileal/myth/blob/main/docs/errors/validation.md",
      "title": "One or more validation errors occurred",
      "status": 400,
      "instance": "/api/users",
      "traceId": "00-abc123...",
      "errors": {
        "email": ["Email is required"]
      }
    }
    ```

- **Middleware**
  - `GuardExceptionMiddleware` now uses `ValidationProblemDetails` from `Microsoft.AspNetCore.Mvc`
  - Errors grouped by field name with array of messages
  - Status codes preserved (uses highest status code among all errors)
  - Options for enum/constant validation moved to `Extensions` dictionary

#### 📝 Migration Guide

**Before:**
```csharp
builder.For(Email, x => x
    .Email()
    .WithMessage("Email already exists")
    .WithCode("EMAIL_EXISTS")  // ❌ No longer available
    .WithStatusCode(HttpStatusCode.Conflict));
```

**After:**
```csharp
builder.For(Email, x => x
    .Email()
    .WithMessage("Email already exists")
    .WithStatusCode(HttpStatusCode.Conflict)); // ✅ Simplified
```

**Error Response Changes:**
- `ValidationError.Code` property removed - no direct replacement
- Error categorization now done via HTTP status codes and field names
- For custom error identification, use `WithStatusCode()` to set specific HTTP codes (400, 409, 422, 404, etc.)

**Benefits:**
- Industry-standard error format (RFC 9457)
- Better tooling support (Swagger, Postman, HTTP clients)
- Simplified API (less cognitive load)
- Native Microsoft types (better interoperability)
- Built-in traceId for observability

