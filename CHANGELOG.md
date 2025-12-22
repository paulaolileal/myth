# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Myth.Guard

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

