---
name: myth-guard
description: Use when you need fluent, context-aware validation. Entities implement IValidatable<T> with ValidationBuilder<T> rules scoped to ValidationContextKey (Create, Update, Delete, etc.). Use Sentry.For() for standalone field validation, Validate.All() for parallel multi-value validation. IValidator.ValidateAsync() throws on failure; UseGuard() middleware returns RFC 9457 Problem Details responses.
---

# SKILL.md - Myth.Guard

**Version:** 1.0
**Target Framework:** .NET 8.0
**License:** Apache 2.0

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
- [Validation Rules](#validation-rules)
- [Usage Examples](#usage-examples)
- [Advanced Features](#advanced-features)
- [Best Practices](#best-practices)

---

## Overview

Myth.Guard is a declarative, fluent, and context-aware validation library for .NET designed for enterprise applications following DDD, Clean Architecture, and SOLID principles. It provides structured data validation with automatic ASP.NET Core integration.

### Key Features

- **Declarative Validation**: Define validation rules on entities via `IValidatable<T>`
- **Context-Aware**: Different rules for different operations (Create, Update, Delete, etc.)
- **100+ Validation Rules**: Comprehensive rules for all common types
- **Async Service Access**: Full DI integration for database/API validation
- **Fluent API**: Chainable, intuitive syntax
- **Standalone Validation (Sentry)**: Validate single values outside entity context using `Sentry.For()`
- **Multi-Validation**: Parallel validation of multiple values using `Validate.All()`
- **Automatic Middleware**: Structured error responses using RFC 9457 Problem Details format
- **Custom Messages**: Static or dynamic error messages
- **HTTP Status Codes**: Configurable per rule
- **Conditional Rules**: `When()` and `Unless()` modifiers
- **Cross-Property Validation**: Access multiple properties in validation
- **Stop on Failure**: Fine control over validation flow
- **Nullable Support**: Full support for nullable types
- **Constant Validation**: Validate against `Constant<T,V>` types

---

## Installation

```bash
dotnet add package Myth.Guard
```

### Dependencies
- .NET 8.0 or higher
- Myth.Commons
- Microsoft.AspNetCore.Http.Abstractions
- System.ComponentModel.Annotations

---

## Namespace Reference

All types from Myth.Guard live in the `Myth.Guard` assembly. The namespaces are:

| Type | Namespace | Notes |
|------|-----------|-------|
| `IValidatable<T>` | `Myth.Interfaces` | |
| `IValidator` | `Myth.Interfaces` | |
| `ValidationBuilder<T>` | `Myth.Guard` | |
| `FluentRuleBuilder<T>` | `Myth.Guard` | |
| `FluentRuleBuilderExtensions` | `Myth.Guard` | All rule extension methods |
| `ValidationContextKey` | `Myth.Guard` | |
| `Sentry` | `Myth.Guard` | Standalone field validation |
| `Validate` | `Myth.Guard` | Multi-value parallel validation |
| `ValidationResult` | `Myth.Guard` | |
| `ValidationError` | `Myth.Guard` | |
| `ValidationException` | **`Myth.Exceptions`** | ⚠️ NOT `Myth.Guard.Exceptions` — add `using Myth.Exceptions;` |

> **Common import mistake:** `ValidationException` is in `Myth.Exceptions`, even though it is part of the Myth.Guard assembly. Always add `using Myth.Exceptions;` when throwing or catching it.

---

## Common Validation Pitfalls

### NotDefault() vs NotEmpty() for Guid and value types

`NotEmpty()` is a **string rule** — it validates that a string is not null/empty/whitespace. When called on a `Guid`, it **compiles but does not validate `Guid.Empty`**, producing a silent runtime bug.

Use `NotDefault()` for Guid, enum, int, and other value types where "empty" means the default value:

```csharp
// ❌ WRONG — compiles but Guid.Empty passes validation
builder.For(x.OrganizationId, r => r.NotEmpty());

// ✅ CORRECT — rejects Guid.Empty
builder.For(x.OrganizationId, r => r.NotDefault());
```

| Type | Correct rule | Why |
|------|-------------|-----|
| `string` | `NotEmpty()` | Checks null/empty/whitespace |
| `Guid` | `NotDefault()` | `Guid.Empty` is `default(Guid)` |
| `int`, `long`, etc. | `NotDefault()` | `0` is `default(int)` |
| `bool` | `IsTrue()` or `IsFalse()` | Context-dependent |
| `DateTime` | `NotDefault()` | `DateTime.MinValue` is `default` |
| Collection | `NotEmpty()` (collection rule) | Checks empty collection |

### MaxLength / MinLength vs MaximumLength / MinimumLength

The canonical method names are `MaximumLength(int)` and `MinimumLength(int)`. The shorter aliases `MaxLength(int)` and `MinLength(int)` also exist and are identical. Use whichever is more natural:

```csharp
// Both are equivalent:
builder.For(x.Name, r => r.MaximumLength(100));
builder.For(x.Name, r => r.MaxLength(100));      // alias
```

> **Don't use `.Must(v => v.Length <= 100)`** — it works but produces a generic error message without the max-length value. The built-in rule generates a structured message like _"Name must have at most 100 characters"_.

---

## Core Concepts

### 1. IValidatable<T>

Entities implement this interface to define their validation rules:

```csharp
public class CreateUserDto : IValidatable<CreateUserDto> {
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }

    public void Validate(ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context = null) {
        builder.For(Name, x => x.NotEmpty().MinimumLength(2));
        builder.For(Email, x => x.NotEmpty().Email());
        builder.For(Age, x => x.GreaterThan(0).LessThan(150));
    }
}
```

### 2. Validation Contexts

Different rules for different operations:

```csharp
public void Validate(ValidationBuilder<User> builder, ValidationContextKey? context = null) {
    // Global rules (always apply)
    builder.For(Email, x => x.NotEmpty().Email());

    // Create-specific rules
    builder.InContext(ValidationContextKey.Create, b => {
        b.For(Email, x => x
            .RespectAsync(async (email, ct, sp) => {
                var service = sp.GetRequiredService<IUserService>();
                return await service.IsEmailAvailableAsync(email, ct);
            })
            .WithMessage("Email already exists")
            .WithStatusCode(HttpStatusCode.Conflict));
    });

    // Update-specific rules
    builder.InContext(ValidationContextKey.Update, b => {
        b.For(Id, x => x.NotNull());
    });
}
```

> **Global rules always execute.** Rules defined outside any `InContext` block run for every call to `ValidateAsync()`, regardless of which `ValidationContextKey` is passed. `InContext` is *additive*: it layers extra rules on top of the globals when the matching context is active. Passing `ValidationContextKey.Create` does **not** suppress global rules — it runs global rules first, then the Create-specific rules. This means you only need to put truly operation-specific logic inside `InContext`; shared constraints (required fields, format rules) belong in global rules.

### 3. Predefined Contexts

```csharp
ValidationContextKey.Default
ValidationContextKey.Create
ValidationContextKey.Update
ValidationContextKey.Delete
ValidationContextKey.GetByField
ValidationContextKey.GetAll
ValidationContextKey.Search
ValidationContextKey.Activate
ValidationContextKey.Deactivate
ValidationContextKey.Custom("MyContext")
```

---

## Quick Start

### 1. Setup (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Guard
builder.Services.AddGuard();

var app = builder.BuildApp();

// Add middleware for automatic exception handling
app.UseGuard();

app.Run();
```

### 2. Define Entity Validation

```csharp
public class CreateUserDto : IValidatable<CreateUserDto> {
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }

    public void Validate(ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context = null) {
        builder.For(Name, x => x
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100));

        builder.For(Email, x => x
            .NotEmpty()
            .Email()
            .MaximumLength(254));

        builder.For(Age, x => x
            .GreaterThan(0)
            .LessThan(150));
    }
}
```

### 3. Use in Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase {
    private readonly IValidator _validator;

    public UsersController(IValidator validator) {
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserDto dto) {
        // Validate and throw ValidationException if invalid
        await _validator.ValidateAsync(dto, ValidationContextKey.Create);

        // If we get here, dto is valid
        var user = await _userService.CreateAsync(dto);
        return Ok(user);
    }

    // Or validate without throwing
    [HttpPost("safe")]
    public async Task<IActionResult> CreateUserSafe(CreateUserDto dto) {
        var result = await _validator.ValidateAndReturnAsync(dto, ValidationContextKey.Create);
        if (!result.IsValid) {
            return BadRequest(result.Errors);
        }

        var user = await _userService.CreateAsync(dto);
        return Ok(user);
    }
}
```

---

## API Reference

### IValidator

**Namespace:** `Myth.Interfaces`

```csharp
public interface IValidator {
    // Validate and throw ValidationException on failure
    Task ValidateAsync<T>(
        T entity,
        ValidationContextKey? context = null,
        CancellationToken cancellationToken = default) where T : class;

    // Validate and return result without throwing
    Task<ValidationResult> ValidateAndReturnAsync<T>(
        T entity,
        ValidationContextKey? context = null,
        CancellationToken cancellationToken = default) where T : class;
}
```

### ValidationResult

```csharp
public sealed class ValidationResult {
    public IReadOnlyList<ValidationError> Errors { get; }
    public bool IsValid { get; }
    public HttpStatusCode StatusCode { get; }
}
```

### ValidationError

```csharp
public sealed class ValidationError {
    public string Field { get; init; }
    public string Message { get; init; }
    public HttpStatusCode StatusCode { get; init; }
    public IReadOnlyList<string>? Options { get; init; }
}
```

### ValidationException

```csharp
public sealed class ValidationException : Exception {
    public ValidationResult ValidationResult { get; }
}
```

---

## Validation Rules

### String Rules

```csharp
builder.For(Name, x => x
    .NotEmpty()
    .MinimumLength(3)
    .MaximumLength(100)
    .LengthBetween(3, 100)
    .EqualsTo("expected", ignoreCase: false)
    .StartsWith("prefix")
    .EndsWith("suffix")
    .Contains("substring")
    .Matches(regex)
    .Email()
    .Url()
    .OnlyLetters()
    .OnlyNumbers()
    .Alphanumeric()
    .NoSymbols()
    .AvailableCharacters('a', 'b', 'c')
    .ForbiddenCharacters('@', '#')
    .BeOneOf("option1", "option2", "option3"));
```

### Numeric Rules

Supported types: `int`, `long`, `decimal`, `double`, `float`, `short`, `byte`, and nullable versions.

```csharp
builder.For(Age, x => x
    .GreaterThan(18)
    .GreaterOrEquals(18)
    .LessThan(100)
    .LessOrEquals(99)
    .Between(18, 99)
    .Positive()
    .Negative()
    .Zero()
    .NotZero());
```

### Collection Rules

```csharp
builder.For(Tags, x => x
    .NotEmpty()
    .CountGreaterThan(1)
    .CountLessThan(10)
    .CountBetween(1, 10)
    .All(tag => tag.Length > 0)
    .Any(tag => tag.StartsWith("important"))
    .None(tag => tag.Contains("banned"))
    .Distinct()
    .DistinctBy(tag => tag.ToLower()));
```

### DateTime Rules

```csharp
builder.For(BirthDate, x => x
    .After(DateTime.Parse("1900-01-01"))
    .AfterOrEquals(minimumDate)
    .Before(DateTime.UtcNow)
    .BeforeOrEquals(DateTime.UtcNow)
    .Between(minDate, maxDate)
    .Today()
    .Past()
    .Future());
```

### DateOnly Rules

```csharp
builder.For(StartDate, x => x
    .After(DateOnly.FromDateTime(DateTime.UtcNow))
    .Before(endDate)
    .Between(minDate, maxDate)
    .Today()
    .Past()
    .Future());
```

### Boolean Rules

```csharp
builder.For(IsActive, x => x
    .IsTrue()
    .IsFalse()
    .Not());
```

### Enum Rules

```csharp
builder.For(Status, x => x
    .BeInEnum()
    .BeNotInEnum()
    .Only(Status.Active, Status.Pending)
    .IsValidEnumValue());
```

### Generic Rules

```csharp
builder.For(Value, x => x
    .NotNull()
    .BeNull()
    .NotDefault()
    .BeDefault()
    .EqualsTo(expectedValue)
    .NotEqualsTo(forbiddenValue)
    .Respect(val => val.IsValid())
    .RespectAsync(async (val, ct, sp) => {
        var service = sp.GetRequiredService<IMyService>();
        return await service.ValidateAsync(val, ct);
    }));
```

### Constant Validation

```csharp
public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Pending = CreateWithCallerName("P");
    public static readonly OrderStatus Confirmed = CreateWithCallerName("C");
    private OrderStatus(string name, string value) : base(name, value) { }
}

builder.For(StatusCode, x => x
    .ExistsInConstant<OrderStatus, string>()
    .WithOptions(OptionsType.ValueAndName));

builder.For(StatusName, x => x
    .NameExistsInConstant<OrderStatus, string>());
```

---

## Rule Modifiers

All validation rules support these modifiers:

```csharp
builder.For(Email, x => x
    .Email()
    .WithMessage("Invalid email format")
    .WithMessage(email => $"{email} is not valid")
    .WithStatusCode(HttpStatusCode.BadRequest)
    .WithStatusCode(422)
    .SetStopOnFailure()
    .When(email => email.Contains("@"))
    .Unless(email => email.StartsWith("test"))
    .WhenEntity(entity => entity.IsProduction)
    .UnlessEntity(entity => entity.IsTest)
    .WithOptions("option1", "option2")
    .WithOptions(OptionsType.ValueAndName));
```

---

## Usage Examples

### Example 1: Complete User Validation

```csharp
public class CreateUserDto : IValidatable<CreateUserDto> {
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public int Age { get; set; }
    public List<string> Roles { get; set; }
    public bool AcceptedTerms { get; set; }

    public void Validate(ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context = null) {
        // Name validation
        builder.For(Name, x => x
            .NotEmpty()
            .WithMessage("Name is required")
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters")
            .MaximumLength(100)
            .OnlyLetters()
            .WithMessage("Name can only contain letters"));

        // Email validation
        builder.For(Email, x => x
            .NotEmpty()
            .Email()
            .MaximumLength(254));

        // Phone validation (optional)
        builder.For(Phone, x => x
            .Matches(new Regex(@"^\+?[\d\s-]{10,}$"))
            .WithMessage("Invalid phone format")
            .When(phone => !string.IsNullOrEmpty(phone)));

        // Age validation
        builder.For(Age, x => x
            .GreaterOrEquals(18)
            .WithMessage("Must be 18 or older")
            .LessThan(150));

        // Roles validation
        builder.For(Roles, x => x
            .NotEmpty()
            .CountBetween(1, 5)
            .WithMessage("Must have between 1 and 5 roles")
            .All(role => !string.IsNullOrWhiteSpace(role))
            .Distinct());

        // Terms acceptance
        builder.For(AcceptedTerms, x => x
            .IsTrue()
            .WithMessage("You must accept the terms and conditions"));

        // Context-specific validation
        builder.InContext(ValidationContextKey.Create, b => {
            b.For(Email, x => x
                .RespectAsync(async (email, ct, sp) => {
                    var userService = sp.GetRequiredService<IUserService>();
                    return await userService.IsEmailAvailableAsync(email, ct);
                })
                .WithMessage("Email already exists")
                .WithStatusCode(HttpStatusCode.Conflict));
        });
    }
}
```

### Example 2: Cross-Property Validation

```csharp
public class PasswordChangeDto : IValidatable<PasswordChangeDto> {
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }

    public void Validate(ValidationBuilder<PasswordChangeDto> builder, ValidationContextKey? context = null) {
        builder.For(OldPassword, x => x
            .NotEmpty()
            .MinimumLength(8));

        builder.For(NewPassword, x => x
            .NotEmpty()
            .MinimumLength(8)
            .Matches(new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)"))
            .WithMessage("Password must contain uppercase, lowercase, and number")
            .Respect<PasswordChangeDto>((newPwd, dto) => newPwd != dto.OldPassword)
            .WithMessage("New password must be different from old password"));

        builder.For(ConfirmPassword, x => x
            .NotEmpty()
            .Respect<PasswordChangeDto>((confirm, dto) => confirm == dto.NewPassword)
            .WithMessage("Passwords do not match"));

        // Async validation with service access
        builder.For(OldPassword, x => x
            .RespectAsync<PasswordChangeDto>(async (oldPwd, dto, ct, sp) => {
                var authService = sp.GetRequiredService<IAuthService>();
                return await authService.ValidatePasswordAsync(dto.UserId, oldPwd, ct);
            })
            .WithMessage("Current password is incorrect")
            .WithStatusCode(HttpStatusCode.Unauthorized));
    }
}
```

### Example 3: Conditional Validation

```csharp
public class ShippingAddressDto : IValidatable<ShippingAddressDto> {
    public bool SameAsBilling { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }

    public void Validate(ValidationBuilder<ShippingAddressDto> builder, ValidationContextKey? context = null) {
        // These fields are required only if SameAsBilling is false
        builder.For(Street, x => x
            .NotEmpty()
            .Unless(s => SameAsBilling));

        builder.For(City, x => x
            .NotEmpty()
            .Unless(c => SameAsBilling));

        builder.For(ZipCode, x => x
            .NotEmpty()
            .Matches(new Regex(@"^\d{5}(-\d{4})?$"))
            .When(z => !SameAsBilling));

        builder.For(Country, x => x
            .NotEmpty()
            .WhenEntity(entity => !entity.SameAsBilling));
    }
}
```

### Example 4: Standalone Validation with Sentry

**Use Case:** Validate individual values outside of entity context (e.g., in services, API endpoints, utilities).

**Sentry** provides standalone validation for single values with full access to service provider for async database/API checks.

```csharp
public class UserService {
    private readonly IServiceProvider _serviceProvider;

    public async Task<Result> ValidateEmailAsync(string email) {
        // Use Sentry.For() for standalone validation
        var validation = Sentry.For(email, "Email")
            .NotEmpty()
            .Email()
            .MaximumLength(254)
            .RespectAsync(async (e, ct, sp) => {
                var service = sp.GetRequiredService<IUserRepository>();
                return !await service.AnyAsync(u => u.Email == e, ct);
            })
            .WithMessage("Email already exists")
            .WithStatusCode(HttpStatusCode.Conflict);

        var result = await validation.ValidateAsync(_serviceProvider);

        return result.IsValid
            ? Result.Success()
            : Result.Failure(result.Errors.First().Message);
    }

    public async Task<Result> ValidatePasswordStrengthAsync(string password) {
        var result = await Sentry.For(password, "Password")
            .NotEmpty()
            .MinimumLength(8)
            .Matches(new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])"))
            .WithMessage("Password must contain uppercase, lowercase, number, and special character")
            .ValidateAsync(_serviceProvider);

        return result.IsValid
            ? Result.Success()
            : Result.Failure(result.Errors.First().Message);
    }
}
```

### Example 5: Multi-Validation

```csharp
public async Task<ValidationResult> ValidateRegistrationAsync(
    string email,
    string password,
    int age,
    List<string> interests) {

    var result = await Validate.All()
        .ValidateEmail(email, "Email")
        .ValidateValue(password, "Password", x => x
            .NotEmpty()
            .MinimumLength(8)
            .Matches(new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")))
        .ValidateRange(age, "Age", 18, 120)
        .ValidateValue(interests, "Interests", x => x
            .NotEmpty()
            .CountBetween(1, 10))
        .ValidateAsync(_serviceProvider);

    return result;
}
```

### Example 6: RFC 9457 Problem Details Error Format

**IMPORTANT:** Myth.Guard uses the **RFC 9457 Problem Details** standard for structured error responses, not custom error codes.

When validation fails, the middleware automatically returns errors in RFC 9457 format:

```json
{
  "type": "https://tools.ietf.org/html/rfc9457",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": [
    {
      "field": "Email",
      "message": "Email is required",
      "statusCode": 400
    },
    {
      "field": "Age",
      "message": "Must be 18 or older",
      "statusCode": 400
    }
  ]
}
```

**Configuring Status Codes:**
```csharp
builder.For(Email, x => x
    .Email()
    .WithMessage("Invalid email format")
    .WithStatusCode(HttpStatusCode.BadRequest)); // 400

builder.For(Email, x => x
    .RespectAsync(async (email, ct, sp) => {
        var exists = await _repository.AnyAsync(u => u.Email == email, ct);
        return !exists;
    })
    .WithMessage("Email already exists")
    .WithStatusCode(HttpStatusCode.Conflict)); // 409

builder.For(UserId, x => x
    .NotNull()
    .WithMessage("User ID is required")
    .WithStatusCode(422)); // Unprocessable Entity
```

**Content-Type:**
```
Content-Type: application/problem+json
```

**Benefits of RFC 9457:**
- ✅ Industry standard format
- ✅ Machine-readable and parseable
- ✅ Consistent across all APIs
- ✅ Supports HTTP status codes per field
- ✅ Compatible with API gateways and proxies

---

### Example 8: Custom Validation Context

```csharp
public class ProductDto : IValidatable<ProductDto> {
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsPublished { get; set; }

    public void Validate(ValidationBuilder<ProductDto> builder, ValidationContextKey? context = null) {
        // Global rules
        builder.For(Name, x => x.NotEmpty().MaximumLength(200));
        builder.For(Price, x => x.GreaterThan(0));

        // Create context
        builder.InContext(ValidationContextKey.Create, b => {
            b.For(Stock, x => x.GreaterOrEquals(0));
        });

        // Update context
        builder.InContext(ValidationContextKey.Update, b => {
            // Stock can't be reduced if product is published
            b.For(Stock, x => x
                .Respect<ProductDto>((stock, dto) => {
                    if (dto.IsPublished && stock < dto.CurrentStock)
                        return false;
                    return true;
                })
                .WithMessage("Cannot reduce stock of published product")
                .When(s => IsPublished));
        });

        // Custom publish context
        builder.InContext(ValidationContextKey.Custom("Publish"), b => {
            b.For(Stock, x => x
                .GreaterThan(0)
                .WithMessage("Cannot publish product with zero stock"));
            b.For(Price, x => x
                .GreaterThan(0)
                .WithMessage("Cannot publish product without price"));
        });
    }
}

// Usage
await _validator.ValidateAsync(product, ValidationContextKey.Create);
await _validator.ValidateAsync(product, ValidationContextKey.Update);
await _validator.ValidateAsync(product, ValidationContextKey.Custom("Publish"));
```

---

## Advanced Features

### 1. Exception Mapping

Configure automatic exception handling:

```csharp
builder.Services.AddGuard(config => config
    .AutoGuardCommonExceptions(includeStackTrace: false)
    .UseDefaultStatusCode(HttpStatusCode.BadRequest)
    .Guard<ArgumentNullException>()
        .WithStatusCode(HttpStatusCode.UnprocessableEntity)
        .WithErrorCode("INVALID_ARGUMENT")
        .WithResponse(ex => new {
            Message = ex.Message,
            Parameter = ex.ParamName
        })
        .And()
    .Guard<InvalidOperationException>()
        .WithStatusCode(HttpStatusCode.Conflict)
        .OnBeforeResponse((ex, context) => {
            // Log or modify context
            _logger.LogWarning(ex, "Invalid operation");
        })
        .And());
```

### 2. Nullable Type Validation

```csharp
public class OptionalFieldsDto : IValidatable<OptionalFieldsDto> {
    public int? Age { get; set; }
    public DateTime? BirthDate { get; set; }
    public bool? IsActive { get; set; }

    public void Validate(ValidationBuilder<OptionalFieldsDto> builder, ValidationContextKey? context = null) {
        // Validates only if not null
        builder.For(Age, x => x
            .GreaterThan(0)
            .LessThan(150));

        builder.For(BirthDate, x => x
            .Past()
            .After(DateTime.Parse("1900-01-01")));

        builder.For(IsActive, x => x
            .IsTrue());
    }
}
```

### 3. Dynamic Error Messages

```csharp
builder.For(Age, x => x
    .GreaterThan(18)
    .WithMessage(age => $"Age {age} is below minimum of 18"));

builder.For(Email, x => x
    .Email()
    .WithMessage(email => $"'{email}' is not a valid email address"));
```

### 4. Options in Errors

```csharp
public class StatusDto : IValidatable<StatusDto> {
    public string Status { get; set; }

    public void Validate(ValidationBuilder<StatusDto> builder, ValidationContextKey? context = null) {
        builder.For(Status, x => x
            .ExistsInConstant<OrderStatus, string>()
            .WithOptions(OptionsType.ValueAndName));
        // Error will include: "Options: P: Pending | C: Confirmed | S: Shipped"
    }
}
```

---

## Async Business Rule Validation

`RespectAsync` provides full DI access inside validation rules, making it the right tool for database-backed business rules: entity existence, unique-constraint checks, plan limits, permissions.

### Entity existence check

```csharp
public class CreateProjectCommand : IValidatable<CreateProjectCommand> {
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;

    public void Validate(ValidationBuilder<CreateProjectCommand> builder, ValidationContextKey? context = null) {
        builder.For(WorkspaceId, r => r
            .NotDefault()                                 // Guid.Empty is invalid
            .RespectAsync(async (id, ct, sp) => {
                var repo = sp.GetRequiredService<IWorkspaceRepository>();
                return await repo.AnyAsync(w => w.Id == id, ct);
            })
            .WithMessage("Workspace not found")
            .WithStatusCode(HttpStatusCode.NotFound));

        builder.For(Name, r => r
            .NotEmpty()
            .MaxLength(100));
    }
}
```

### Unique constraint check (Create context only)

```csharp
builder.InContext(ValidationContextKey.Create, b => {
    b.For(Email, r => r
        .NotEmpty()
        .Email()
        .RespectAsync(async (email, ct, sp) => {
            var repo = sp.GetRequiredService<IUserRepository>();
            return !await repo.AnyAsync(u => u.Email == email, ct);
        })
        .WithMessage("Email is already registered")
        .WithStatusCode(HttpStatusCode.Conflict));
});
```

### Plan limit / credit check (cross-property with entity access)

```csharp
builder.For(Count, r => r
    .GreaterThan(0)
    .RespectAsync<CreateItemsCommand>(async (count, command, ct, sp) => {
        var repo = sp.GetRequiredService<IUserRepository>();
        var user = await repo.FirstOrDefaultAsync(u => u.Id == command.UserId, ct);
        return user?.RemainingCredits >= count;
    })
    .WithMessage("Insufficient credits")
    .WithStatusCode(HttpStatusCode.PaymentRequired));
```

> **Note:** The overload `RespectAsync<TEntity>(Func<TValue, TEntity, ct, sp, Task<bool>>)` gives access to the full command/entity being validated, not just the field value. Use it when the validation requires context from other fields.

---

## Best Practices

### 1. Keep Validation Rules Close to Domain

**✅ DO:**
```csharp
public class User : IValidatable<User> {
    public void Validate(ValidationBuilder<User> builder, ValidationContextKey? context = null) {
        // Validation is part of the entity
    }
}
```

**❌ DON'T:**
```csharp
// Separate validator class (harder to maintain)
public class UserValidator {
    public void Validate(User user) { }
}
```

### 2. Use Context-Specific Rules

**✅ DO:**
```csharp
builder.InContext(ValidationContextKey.Create, b => {
    b.For(Email, x => x.RespectAsync(...));  // Check email uniqueness only on create
});

builder.InContext(ValidationContextKey.Update, b => {
    b.For(Id, x => x.NotNull());  // ID required only on update
});
```

### 3. Use Async for Database/API Checks

**✅ DO:**
```csharp
builder.For(Email, x => x
    .RespectAsync(async (email, ct, sp) => {
        var repo = sp.GetRequiredService<IUserRepository>();
        return !await repo.AnyAsync(u => u.Email == email, ct);
    }));
```

**❌ DON'T:**
```csharp
builder.For(Email, x => x
    .Respect(email => {
        return !_repo.AnyAsync(u => u.Email == email).Result;  // Blocking!
    }));
```

### 4. Provide Meaningful Error Messages

**✅ DO:**
```csharp
builder.For(Age, x => x
    .GreaterOrEquals(18)
    .WithMessage("You must be at least 18 years old to register"));
```

**❌ DON'T:**
```csharp
builder.For(Age, x => x
    .GreaterOrEquals(18));  // Generic message
```

### 5. Use Stop on Failure for Expensive Operations

**✅ DO:**
```csharp
builder.For(Email, x => x
    .NotEmpty()
    .SetStopOnFailure()  // Don't check format if empty
    .Email()
    .SetStopOnFailure()  // Don't check uniqueness if invalid format
    .RespectAsync(async (email, ct, sp) => {
        // Expensive database check
    }));
```

---

## Troubleshooting

### Issue 1: Validation Not Running

**Problem:** Validation rules not executing.

**Checklist:**
1. Is `IValidatable<T>` implemented?
2. Is `builder.Services.AddGuard()` called?
3. Is `_validator.ValidateAsync()` called?

### Issue 2: Async Rules Not Executing

**Problem:** `RespectAsync` not running.

**Cause:** Forgot to await or pass service provider.

**Solution:**
```csharp
// Ensure service provider is available
await _validator.ValidateAsync(dto, context, cancellationToken);
```

### Issue 3: Context Rules Not Applying

**Problem:** Context-specific rules ignored.

**Solution:**
```csharp
// Always pass context
await _validator.ValidateAsync(dto, ValidationContextKey.Create);
```

### Issue 4: Middleware Not Catching Exceptions

**Problem:** ValidationException not handled.

**Solution:**
```csharp
// Ensure middleware is registered
app.UseGuard();
```

---

## Summary

Myth.Guard provides:

- ✅ **100+ Validation Rules**: Comprehensive coverage
- ✅ **Context-Aware**: Different rules per operation
- ✅ **Async Service Access**: Database/API validation
- ✅ **Fluent API**: Intuitive, chainable syntax
- ✅ **Automatic Middleware**: Structured error responses
- ✅ **Nullable Support**: Full nullable type handling
- ✅ **Conditional Rules**: `When`/`Unless` modifiers
- ✅ **Cross-Property**: Access entity in validation
- ✅ **Standalone**: Validate single values
- ✅ **Multi-Validation**: Parallel validation

---

## Additional Resources

- **Repository**: https://gitlab.com/dotnet-myth/myth
- **License**: Apache 2.0
- **Target Framework**: .NET 8.0
- **NuGet Package**: Myth.Guard

---

*This documentation is maintained for AI agents and developers. For questions or contributions, please refer to the repository.*
