<img  style="float: right;" src="myth-guard-logo.png" alt="drawing" width="250"/>

# Myth.Guard

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Guard?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Guard/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Guard?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Guard/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](README.md)

A powerful, fluent .NET validation library designed for enterprise applications. Built with clean architecture principles, Myth.Guard provides declarative validation with context-awareness, async service integration, and automatic ASP.NET Core middleware.

## Why Myth.Guard?

Most validation libraries force you to choose between attribute-based validation (inflexible) or imperative validation code (verbose and scattered). Myth.Guard offers a third way: **declarative, fluent validation that lives with your entities**, promoting Domain-Driven Design while keeping validation logic maintainable and testable.

## Key Features

- **Declarative Fluent API**: Write readable validation rules with chainable methods
- **Multi-Validation**: Validate multiple values simultaneously with parallel execution (similar to Task.WhenAll)
- **Standalone Validation**: Use Guard.For() for independent field validation outside model context
- **Context-Aware Validation**: Different rules for Create, Update, Delete operations on the same entity
- **Async Service Integration**: Access dependency injection for database or API validation
- **Global Exception Handler**: Configure custom exception mappings with status codes and response formats
- **Automatic Error Handling**: ASP.NET Core middleware with structured JSON responses
- **100+ Built-in Rules**: Comprehensive validation for strings, numbers, collections, dates, booleans, enums
- **Nullable Type Support**: Full support for nullable value types with dedicated rules
- **Custom Rules**: Easy extensibility with `Respect()` and `RespectAsync()` methods
- **Conditional Validation**: Field-level and entity-level conditional rules
- **Stop on Failure**: Optimize validation by stopping after critical failures
- **HTTP Status Customization**: Configure default status codes globally and override per validation error

## Installation

```bash
dotnet add package Myth.Guard
```

## Quick Start

### 1. Define Validation on Your Entity

```csharp
public class CreateUserDto : IValidatable<CreateUserDto>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public List<string> Tags { get; set; }

    public void Validate( ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context = null )
    {
        builder.For( Name, x => x.NotEmpty().MinimumLength( 2 ).MaximumLength( 100 ) );
        builder.For( Email, x => x.NotEmpty().Email() );
        builder.For( Age, x => x.GreaterThan( 0 ).LessThan( 150 ) );
        builder.For( Tags, x => x.NotEmpty().CountBetween( 1, 10 ) );
    }
}
```

### 2. Configure Services and Middleware

```csharp
var builder = WebApplication.CreateBuilder( args );

// Basic configuration
builder.Services.AddGuard();

// Advanced configuration with default validation status code
builder.Services.AddGuard( config => config
    .UseDefaultStatusCode( 422 ) // UnprocessableEntity for validation errors
    .AutoGuardCommonExceptions()
);

var app = builder.Build();

app.UseGuard(); // Adds automatic validation exception handling

app.MapControllers();
app.Run();
```

### 3. Use in Controllers

```csharp
public class UserController : ControllerBase
{
    private readonly IValidator _validator;

    public UserController( IValidator validator )
    {
        _validator = validator;
    }

    [HttpPost( "users" )]
    public async Task<IActionResult> CreateUser( [FromBody] CreateUserDto request )
    {
        // Validate and throw ValidationException on failure
        await _validator.ValidateAsync( request, ValidationContextKey.Create );

        // Or validate and check result without throwing
        var result = await _validator.ValidateAndReturnAsync( request, ValidationContextKey.Create );

        if ( !result.IsValid )
            return BadRequest( new { errors = result.Errors } );

        // Process user creation...
        return Ok( new { message = "User created successfully" } );
    }
}
```

### Automatic Error Response

With `app.UseGuard()` middleware, validation exceptions are automatically formatted:

```json
{
    "code": "MULTIPLE_ERRORS",
    "errors": [
        {
            "field": "email",
            "message": "Email is required",
            "code": "VIOLATION"
        },
        {
            "field": "age",
            "message": "Value must be greater than 0",
            "code": "VIOLATION"
        }
    ]
}
```

## Validation Rules Reference

### String Rules

```csharp
builder.For( Email, x => x
    .NotEmpty()
    .Email()
    .MaximumLength( 254 ) );

builder.For( Name, x => x
    .NotEmpty()
    .MinimumLength( 2 )
    .MaximumLength( 100 )
    .OnlyLetters() );

builder.For( Password, x => x
    .NotEmpty()
    .MinimumLength( 8 )
    .Matches( new Regex( @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$" ) )
    .WithMessage( "Password must contain uppercase, lowercase, and digit" ) );

builder.For( PhoneNumber, x => x
    .NotEmpty()
    .Matches( new Regex( @"^\+\d{1,3}\d{10,14}$" ) ) );
```

**Available String Rules:**
- `NotEmpty()` - Not null, empty, or whitespace
- `MinimumLength(int)`, `MaximumLength(int)`, `LengthBetween(int, int)` - Length validation
- `Email()`, `Url()` - Format validation
- `OnlyLetters()`, `OnlyNumbers()`, `Alphanumeric()` - Character type validation
- `StartsWith(string)`, `EndsWith(string)`, `Contains(string)` - Substring checks
- `Matches(Regex)` - Regex pattern matching
- `EqualsTo(string)`, `BeOneOf(params string[])` - Enumeration checks
- `AvailableCharacters(params char[])`, `ForbiddenCharacters(params char[])` - Character whitelist/blacklist
- `NoSymbols(char[]?)` - Symbol validation

### Numeric Rules

```csharp
builder.For( Age, x => x
    .GreaterThan( 0 )
    .LessThan( 150 ) );

builder.For( Salary, x => x
    .GreaterOrEquals( 0 )
    .LessThan( 1000000m ) );

builder.For( Score, x => x
    .Between( 0, 100 )
    .When( score => score.HasValue ) );

builder.For( Quantity, x => x
    .Positive()
    .NotZero() );
```

**Available Numeric Rules** (int, long, decimal, double, float, etc.):
- `GreaterThan(T)`, `GreaterOrEquals(T)` - Minimum value validation
- `LessThan(T)`, `LessOrEquals(T)` - Maximum value validation
- `Between(T min, T max)` - Range validation (inclusive)
- `Positive()`, `Negative()` - Sign validation
- `Zero()`, `NotZero()` - Zero value checks

### Collection Rules

```csharp
builder.For( Tags, x => x
    .NotEmpty()
    .CountBetween( 1, 10 )
    .All( tag => !string.IsNullOrWhiteSpace( tag ) )
    .Distinct() );

builder.For( UserRoles, x => x
    .NotEmpty()
    .Any( role => role == "Admin" || role == "User" )
    .None( role => role == "Banned" ) );

builder.For( Products, x => x
    .DistinctBy( p => p.Sku ) );
```

**Available Collection Rules:**
- `NotEmpty()` - Collection not null and has elements
- `CountBetween(int, int)`, `CountGreaterThan(int)`, `CountLessThan(int)` - Size validation
- `All<T>(Func<T, bool>)` - All elements match condition
- `Any<T>(Func<T, bool>)` - At least one matches
- `None<T>(Func<T, bool>)` - No elements match
- `Distinct<T>()` - No duplicates (using default equality)
- `DistinctBy<T, TKey>(Func<T, TKey>)` - No duplicates by key

### DateTime and DateOnly Rules

```csharp
builder.For( BirthDate, x => x
    .Past()
    .After( new DateTime( 1900, 1, 1 ) ) );

builder.For( ScheduledDate, x => x
    .Future()
    .Before( DateTime.Now.AddYears( 1 ) ) );

builder.For( AppointmentDate, x => x
    .Between( DateTime.Today, DateTime.Today.AddDays( 30 ) ) );

builder.For( EventDate, x => x.Today() );
```

**Available DateTime/DateOnly Rules:**
- `Past()`, `Future()`, `Today()` - Temporal validation
- `After(DateTime)`, `Before(DateTime)` - Comparison (exclusive)
- `AfterOrEquals(DateTime)`, `BeforeOrEquals(DateTime)` - Comparison (inclusive)
- `Between(DateTime, DateTime)` - Date range (inclusive)

### Boolean and Enum Rules

```csharp
builder.For( IsActive, x => x.IsTrue() );
builder.For( IsDeleted, x => x.IsFalse() );

builder.For( Role, x => x.BeInEnum<UserRole>() );
builder.For( Status, x => x.BeOneOf( Status.Active, Status.Pending ) );
```

### Constant Rules

Validate values and names against `Myth.Commons.ValueObjects.Constant<TConstant, TValue>` types:

```csharp
// Define your constants
public class Status : Constant<Status, string> {
    public static readonly Status Active = new( "Active", "A" );
    public static readonly Status Inactive = new( "Inactive", "I" );
    public static readonly Status Pending = new( "Pending", "P" );

    public Status( string name, string value ) : base( name, value ) { }
}

public class Priority : Constant<Priority, int> {
    public static readonly Priority Low = new( "Low", 1 );
    public static readonly Priority Medium = new( "Medium", 5 );
    public static readonly Priority High = new( "High", 10 );

    public Priority( string name, int value ) : base( name, value ) { }
}

// Validate constant values and names
builder.For( StatusCode, x => x
    .NotEmpty()
    .ExistsInConstant<Status, string>() );

builder.For( StatusName, x => x
    .NotEmpty()
    .NameExistsInConstant<Status, string>() );

builder.For( PriorityLevel, x => x
    .ExistsInConstant<Priority, int>() );

builder.For( PriorityName, x => x
    .NotEmpty()
    .NameExistsInConstant<Priority, int>() );
```

**Available Constant Rules:**
- `ExistsInConstant<TConstant, TValue>()` - Validates that a value exists in the constant definition
- `NameExistsInConstant<TConstant, TValue>()` - Validates that a name exists in the constant definition

**Error Messages:**
- Value error: `"Value 'X' is not valid. Valid options are: A: Active | I: Inactive | P: Pending"`
- Name error: `"Name 'Unknown' is not valid. Valid options are: 1: Low | 5: Medium | 10: High"`

### Generic Rules (All Types)

```csharp
builder.For( UserId, x => x
    .NotNull()
    .NotDefault() );

builder.For( Email, x => x
    .NotNull()
    .NotEqualsTo( "admin@example.com" ) );
```

**Available Generic Rules:**
- `NotNull()`, `BeNull()` - Null checks
- `EqualsTo(T)`, `NotEqualsTo(T)` - Value comparison
- `BeDefault()`, `NotDefault()` - Default value checks
- `Respect(Func<T, bool>)` - Custom sync validation
- `RespectAsync(Func<T, CancellationToken, IServiceProvider, Task<bool>>)` - Custom async validation
- `Respect<TEntity>(Func<T, TEntity, bool>)` - Custom sync validation with entity access
- `RespectAsync<TEntity>(Func<T, TEntity, CancellationToken, IServiceProvider, Task<bool>>)` - Custom async validation with entity access

### Nullable Type Support

All numeric, DateTime, and boolean rules have nullable versions:

```csharp
builder.For( OptionalAge, x => x
    .GreaterThan( 18 )
    .When( age => age.HasValue ) );

builder.For( OptionalDate, x => x
    .Future()
    .When( date => date.HasValue ) );

builder.For( OptionalFlag, x => x.IsTrue() );
```

## Context-Aware Validation

Define different validation rules for different operations:

```csharp
public class UserDto : IValidatable<UserDto>
{
    public string Email { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public string Password { get; set; }

    public void Validate( ValidationBuilder<UserDto> builder, ValidationContextKey? context = null )
    {
        // Global rules (apply to all contexts)
        builder.For( Email, x => x.NotEmpty().Email() );
        builder.For( Age, x => x.GreaterThan( 0 ).LessThan( 150 ) );

        // Create-specific rules
        builder.InContext( ValidationContextKey.Create, b =>
        {
            b.For( Email, x => x
                .RespectAsync( async ( email, ct, sp ) =>
                {
                    var userService = sp.GetRequiredService<IUserService>();
                    return await userService.IsEmailAvailableAsync( email, ct );
                } )
                .WithMessage( "Email already exists" )
                .WithCode( "EMAIL_EXISTS" )
                .WithStatusCode( HttpStatusCode.Conflict ) );

            b.For( Password, x => x
                .NotEmpty()
                .MinimumLength( 8 )
                .Matches( new Regex( @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$" ) ) );

            b.For( IsActive, x => x.IsTrue() );
        } );

        // Update-specific rules
        builder.InContext( ValidationContextKey.Update, b =>
        {
            b.For( Age, x => x.GreaterOrEquals( 18 ) );
            // Password is optional on update
        } );

        // Delete-specific rules
        builder.InContext( ValidationContextKey.Delete, b =>
        {
            b.For( IsActive, x => x.IsFalse()
                .WithMessage( "Cannot delete active user" ) );
        } );
    }
}

// Usage with different contexts
await validator.ValidateAsync( user, ValidationContextKey.Create );
await validator.ValidateAsync( user, ValidationContextKey.Update );
await validator.ValidateAsync( user, ValidationContextKey.Delete );
```

### Pre-defined Contexts

```csharp
ValidationContextKey.Default     // Default context
ValidationContextKey.Create      // For creation operations
ValidationContextKey.Update      // For update operations
ValidationContextKey.Delete      // For deletion operations
ValidationContextKey.GetByField  // For field-based queries
ValidationContextKey.GetAll      // For listing operations
ValidationContextKey.Search      // For search operations
ValidationContextKey.Activate    // For activation operations
ValidationContextKey.Deactivate  // For deactivation operations

// Custom contexts
ValidationContextKey.Custom( "BulkImport" )
```

## Conditional Validation

Execute rules based on conditions:

### Field-Level Conditions

```csharp
builder.For( PhoneNumber, x => x
    .NotEmpty()
    .When( phone => !string.IsNullOrEmpty( phone ) ) // Only validate if not empty
    .Matches( new Regex( @"^\+\d{1,3}\d{10,14}$" ) ) );

builder.For( Password, x => x
    .NotEmpty()
    .Unless( pwd => IsExternalUser ) // Skip for external users
    .MinimumLength( 8 ) );
```

### Entity-Level Conditions

```csharp
builder.For( PhoneNumber, x => x
    .NotEmpty()
    .When<string, UserDto>( user => user.PhoneType == PhoneType.Required )
    .Unless<string, UserDto>( user => user.IsVerified ) );

builder.For( Salary, x => x
    .GreaterThan( 0 )
    .When<decimal, EmployeeDto>( emp => emp.EmploymentType == EmploymentType.FullTime ) );
```

## Cross-Property Validation with Entity Access

Myth.Guard provides powerful cross-property validation capabilities, allowing validation rules to access the entire entity being validated, not just the individual property value. This enables complex business rules that span multiple properties.

### Basic Entity Access

Use `Respect<TEntity>()` and `RespectAsync<TEntity>()` to access the parent object:

```csharp
public class LoginDto : IValidatable<LoginDto>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public bool IsActive { get; set; }

    public void Validate( ValidationBuilder<LoginDto> builder, ValidationContextKey? context = null )
    {
        // Email validation with access to the entire LoginDto object
        builder.For( Email, x => x
            .NotEmpty()
            .Email()
            .Respect<LoginDto>( ( email, login ) => !login.IsActive || !string.IsNullOrEmpty( email ) )
            .WithMessage( "Email is required for active users" )
            .WithCode( "EMAIL_REQUIRED_FOR_ACTIVE" ) );

        // Password validation with async entity access and external service
        builder.For( Password, x => x
            .NotEmpty()
            .MinimumLength( 6 )
            .RespectAsync<LoginDto>( async ( password, login, ct, sp ) => {
                var userService = sp.GetRequiredService<IUserService>();
                return await userService.ValidateCredentialsAsync( login.Email, password, ct );
            } )
            .WithMessage( "Invalid email and password combination" )
            .WithCode( "INVALID_CREDENTIALS" ) );
    }
}
```

### Advanced Cross-Property Rules

Create complex validation logic that considers multiple properties:

```csharp
public class UserProfileDto : IValidatable<UserProfileDto>
{
    public string Name { get; set; }
    public string Role { get; set; }
    public int Age { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }

    public void Validate( ValidationBuilder<UserProfileDto> builder, ValidationContextKey? context = null )
    {
        // Name length requirements vary by role
        builder.For( Name, x => x
            .NotEmpty()
            .Respect<UserProfileDto>( ( name, profile ) => {
                return profile.Role switch {
                    "Admin" => name.Length >= 5,
                    "Manager" => name.Length >= 4,
                    "User" => name.Length >= 3,
                    _ => name.Length >= 2
                };
            } )
            .WithMessage( "Name length requirement not met for role" )
            .WithCode( "NAME_LENGTH_ROLE_MISMATCH" ) );

        // Age requirements based on role
        builder.For( Age, x => x
            .GreaterThan( 0 )
            .Respect<UserProfileDto>( ( age, profile ) => {
                return profile.Role switch {
                    "Admin" => age >= 25,
                    "Manager" => age >= 21,
                    "User" => age >= 18,
                    _ => age >= 16
                };
            } )
            .WithMessage( "Age requirement not met for role" )
            .WithCode( "AGE_ROLE_MISMATCH" ) );

        // Salary validation with role and activity status
        builder.For( Salary, x => x
            .GreaterOrEquals( 0 )
            .Respect<UserProfileDto>( ( salary, profile ) => {
                if ( !profile.IsActive ) return true; // Inactive users can have any salary

                return profile.Role switch {
                    "Admin" => salary >= 80000,
                    "Manager" => salary >= 60000,
                    "User" => salary >= 30000,
                    _ => salary >= 20000
                };
            } )
            .WithMessage( "Salary below minimum for active user role" )
            .WithCode( "SALARY_BELOW_MINIMUM" ) );
    }
}
```

### Entity Access with Async Services

Combine entity access with external service validation:

```csharp
public class OrderDto : IValidatable<OrderDto>
{
    public int CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string CustomerType { get; set; }
    public string ShippingAddress { get; set; }

    public void Validate( ValidationBuilder<OrderDto> builder, ValidationContextKey? context = null )
    {
        builder.For( Amount, x => x
            .GreaterThan( 0 )
            .RespectAsync<OrderDto>( async ( amount, order, ct, sp ) => {
                var customerService = sp.GetRequiredService<ICustomerService>();
                var customer = await customerService.GetByIdAsync( order.CustomerId, ct );

                if ( customer == null ) return false;

                // Different limits for different customer types
                var maxAmount = order.CustomerType switch {
                    "Premium" => 50000m,
                    "Gold" => 25000m,
                    "Silver" => 10000m,
                    _ => 5000m
                };

                return amount <= maxAmount;
            } )
            .WithMessage( "Order amount exceeds limit for customer type" )
            .WithCode( "AMOUNT_EXCEEDS_CUSTOMER_LIMIT" ) );

        // Address validation based on customer location
        builder.For( ShippingAddress, x => x
            .NotEmpty()
            .RespectAsync<OrderDto>( async ( address, order, ct, sp ) => {
                var customerService = sp.GetRequiredService<ICustomerService>();
                var addressService = sp.GetRequiredService<IAddressService>();

                var customer = await customerService.GetByIdAsync( order.CustomerId, ct );
                if ( customer == null ) return false;

                return await addressService.IsValidForCustomerAsync( address, customer.Country, ct );
            } )
            .WithMessage( "Shipping address not valid for customer location" )
            .WithCode( "INVALID_SHIPPING_ADDRESS" ) );
    }
}
```

### Backward Compatibility

The new entity access methods are fully backward compatible. All existing `Respect()` and `RespectAsync()` methods continue to work exactly as before:

```csharp
// Existing syntax still works
builder.For( Email, x => x
    .Respect( email => !string.IsNullOrEmpty( email ) && email.Contains( "@" ) ) );

builder.For( UserId, x => x
    .RespectAsync( async ( id, ct, sp ) => {
        var userService = sp.GetRequiredService<IUserService>();
        return await userService.ExistsAsync( id, ct );
    } ) );

// New entity access syntax
builder.For( Email, x => x
    .Respect<UserDto>( ( email, user ) => user.IsActive || string.IsNullOrEmpty( email ) ) );

builder.For( UserId, x => x
    .RespectAsync<UserDto>( async ( id, user, ct, sp ) => {
        var userService = sp.GetRequiredService<IUserService>();
        return await userService.HasPermissionAsync( id, user.Role, ct );
    } ) );
```

### Type Safety

The generic constraint `where TEntity : class` ensures type safety and provides full IntelliSense support:

```csharp
// Compile-time type checking
builder.For( Email, x => x
    .Respect<LoginDto>( ( email, login ) => {
        // 'login' is strongly typed as LoginDto
        return login.IsActive && !string.IsNullOrEmpty( email );
    } ) );

// Compiler error if wrong type is used
builder.For( Email, x => x
    .Respect<WrongType>( ( email, wrong ) => true ) ); // ❌ Compilation error
```

### Performance Considerations

- **Minimal overhead**: Entity access adds only a single cast operation
- **No reflection**: Uses compile-time generics for optimal performance
- **Lazy evaluation**: Rules only execute when validation runs
- **Service sharing**: Single service provider instance shared across all rules

## Async Validation with Service Provider

Access dependency injection for database or API validation:

```csharp
public class CreateOrderDto : IValidatable<CreateOrderDto>
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string CustomerEmail { get; set; }

    public void Validate( ValidationBuilder<CreateOrderDto> builder, ValidationContextKey? context = null )
    {
        builder.For( ProductId, x => x
            .GreaterThan( 0 )
            .RespectAsync( async ( productId, ct, sp ) =>
            {
                var productService = sp.GetRequiredService<IProductService>();
                return await productService.ExistsAsync( productId, ct );
            } )
            .WithMessage( "Product does not exist" )
            .WithCode( "PRODUCT_NOT_FOUND" ) );

        builder.For( Quantity, x => x
            .GreaterThan( 0 )
            .RespectAsync( async ( quantity, ct, sp ) =>
            {
                var productService = sp.GetRequiredService<IProductService>();
                var stock = await productService.GetStockAsync( ProductId, ct );
                return quantity <= stock;
            } )
            .WithMessage( "Insufficient stock" )
            .WithCode( "INSUFFICIENT_STOCK" ) );

        builder.For( CustomerEmail, x => x
            .NotEmpty()
            .Email()
            .RespectAsync( async ( email, ct, sp ) =>
            {
                var customerService = sp.GetRequiredService<ICustomerService>();
                return await customerService.IsActiveCustomerAsync( email, ct );
            } )
            .WithMessage( "Customer not found or inactive" )
            .WithCode( "CUSTOMER_INACTIVE" )
            .WithStatusCode( HttpStatusCode.NotFound ) );
    }
}
```

## Advanced Features

### Custom Error Messages

```csharp
// Static message
builder.For( Age, x => x
    .GreaterThan( 18 )
    .WithMessage( "User must be at least 18 years old" ) );

// Dynamic message using field value
builder.For( Age, x => x
    .GreaterThan( 18 )
    .WithMessage( age => $"User must be at least 18 years old, but is {age}" ) );

// Custom error code
builder.For( Email, x => x
    .Email()
    .WithCode( "INVALID_EMAIL_FORMAT" ) );

// Custom HTTP status code
builder.For( UserId, x => x
    .RespectAsync( async ( id, ct, sp ) =>
    {
        var userService = sp.GetRequiredService<IUserService>();
        return await userService.ExistsAsync( id, ct );
    } )
    .WithMessage( "User not found" )
    .WithCode( "USER_NOT_FOUND" )
    .WithStatusCode( HttpStatusCode.NotFound ) ); // Returns 404 instead of 400
```

### Stop on Failure

Stop validating a field after the first failure:

```csharp
builder.For( Password, x => x
    .NotEmpty()
    .SetStopOnFailure() // Don't check other rules if empty
    .MinimumLength( 8 )
    .Matches( new Regex( @"[A-Z]" ) )
    .Matches( new Regex( @"[a-z]" ) )
    .Matches( new Regex( @"\d" ) ) );
```

### Complex Business Rules

```csharp
public class OrderDto : IValidatable<OrderDto>
{
    public decimal Amount { get; set; }
    public string CustomerType { get; set; }
    public List<OrderItem> Items { get; set; }
    public string CouponCode { get; set; }

    public void Validate( ValidationBuilder<OrderDto> builder, ValidationContextKey? context = null )
    {
        builder.For( Amount, x => x
            .GreaterThan( 0 )
            .When<decimal, OrderDto>( order => order.Items?.Any() == true ) );

        // Premium customers can have higher order amounts
        builder.For( Amount, x => x
            .LessThan( 10000 )
            .Unless<decimal, OrderDto>( order => order.CustomerType == "Premium" ) );

        builder.For( Items, x => x
            .NotEmpty()
            .CountBetween( 1, 50 )
            .All( item => item.Quantity > 0 )
            .All( item => item.Price > 0 ) );

        // Coupon validation
        builder.For( CouponCode, x => x
            .RespectAsync( async ( coupon, ct, sp ) =>
            {
                if ( string.IsNullOrEmpty( coupon ) ) return true; // Optional

                var couponService = sp.GetRequiredService<ICouponService>();
                var isValid = await couponService.IsValidAsync( coupon, ct );
                var isApplicable = await couponService.IsApplicableToOrderAsync( coupon, Amount, ct );

                return isValid && isApplicable;
            } )
            .WithMessage( "Invalid or inapplicable coupon code" )
            .WithCode( "INVALID_COUPON" ) );
    }
}
```

## Error Handling

### Validation Result

```csharp
var result = await validator.ValidateAndReturnAsync( dto );

Console.WriteLine( $"Is Valid: {result.IsValid}" );
Console.WriteLine( $"Status Code: {result.StatusCode}" );

if ( !result.IsValid )
{
    foreach ( var error in result.Errors )
    {
        Console.WriteLine( $"Field: {error.Field}" );
        Console.WriteLine( $"Message: {error.Message}" );
        Console.WriteLine( $"Code: {error.Code}" );
        Console.WriteLine( $"Status: {error.StatusCode}" );
    }
}
```

### Exception Handling

```csharp
try
{
    await validator.ValidateAsync( dto, ValidationContextKey.Create );
}
catch ( ValidationException ex )
{
    var errors = ex.ValidationResult.Errors;
    var statusCode = ex.ValidationResult.StatusCode;

    return BadRequest( new
    {
        message = "Validation failed",
        errors = errors.Select( e => new
        {
            field = e.Field,
            message = e.Message,
            code = e.Code
        } )
    } );
}
```

### Middleware Error Response

When using `app.UseGuard()`, validation exceptions are automatically caught and formatted:

```json
{
    "code": "MULTIPLE_ERRORS",
    "errors": [
        {
            "field": "email",
            "message": "Email already exists",
            "code": "EMAIL_EXISTS"
        },
        {
            "field": "password",
            "message": "Password must contain at least 8 characters",
            "code": "VIOLATION"
        }
    ]
}
```

**HTTP Status Code**: The highest status code from all validation errors (e.g., if one error has `409 Conflict`, the response will be `409`).

### Status Code Configuration

Configure default and custom status codes for validation errors:

```csharp
// Configure default status code globally
builder.Services.AddGuard( config => config
    .UseDefaultStatusCode( 422 ) // UnprocessableEntity
    // or
    .UseDefaultStatusCode( HttpStatusCode.UnprocessableEntity )
);

// Override per validation rule
public void Validate( ValidationBuilder<UserDto> builder, ValidationContextKey? context = null )
{
    builder.For( Email, x => x
        .NotEmpty()
        .Email()
        .RespectAsync( async ( email, ct, sp ) =>
        {
            var userService = sp.GetRequiredService<IUserService>();
            return await userService.IsEmailAvailableAsync( email, ct );
        } )
        .WithMessage( "Email already exists" )
        .WithCode( "EMAIL_EXISTS" )
        .WithStatusCode( HttpStatusCode.Conflict ) // 409 - Override global default
    );

    builder.For( Age, x => x
        .GreaterThan( 0 )
        .LessThan( 150 )
        // Uses global default status code (422) when no WithStatusCode() is specified
    );
}
```

**Precedence Order**:
1. **Custom status code** (`.WithStatusCode()`) - highest priority
2. **Global default** (`.UseDefaultStatusCode()`) - medium priority
3. **BadRequest (400)** - fallback when no configuration provided

## Multi-Validation

Myth.Guard provides powerful multi-validation capabilities for validating multiple values simultaneously with parallel execution, similar to `Task.WhenAll`. This is perfect when you need to validate multiple independent values without the overhead of separate validation calls.

### Parallel Validation with Validate.AllAsync()

Validate multiple values in parallel for optimal performance:

```csharp
// Basic parallel validation
var result = await Validate.AllAsync([
    Guard.For(email, "Email").NotEmpty().Email(),
    Guard.For(age, "Age").GreaterThan(0).LessThan(150),
    Guard.For(name, "Name").NotEmpty().MinimumLength(2)
]);

if (!result.IsValid)
{
    Console.WriteLine($"Found {result.ErrorCount} errors across {result.FieldsWithErrorsCount} fields");
    foreach (var error in result.Errors)
        Console.WriteLine($"{error.Field}: {error.Message}");
}

// Validate and throw on failure
await Validate.AllAndThrowAsync([
    Guard.For(username, "Username").NotEmpty().Alphanumeric(),
    Guard.For(password, "Password").NotEmpty().MinimumLength(8)
]);
```

### Fluent Builder API

Use the fluent builder for more readable multi-validation scenarios:

```csharp
var result = await Validate.All()
    .Add(Guard.For(email, "Email").NotEmpty().Email())
    .Add(Guard.For(age, "Age").GreaterThan(0).LessThan(150))
    .Add(Guard.For(name, "Name").NotEmpty().MinimumLength(2))
    .ValidateAsync();

// Or using extension methods for common scenarios
var result = await Validate.All()
    .ValidateEmail(email)
    .ValidateRange(age, "Age", 0, 150)
    .ValidateRequired(name, "Name")
    .ValidateString(password, "Password", p => p
        .NotEmpty()
        .MinimumLength(8)
        .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$")
        .WithMessage("Password must contain uppercase, lowercase and digit"))
    .ValidateAsync();

// Fluent with exception throwing
await Validate.All()
    .ValidateEmail(userEmail)
    .ValidateRange(userAge, "Age", 18, 65)
    .ValidateAndThrowAsync();
```

### Array Extension Methods

Convenient extension methods for collections of validations:

```csharp
var validations = new[] {
    Guard.For("test@example.com", "Email").Email(),
    Guard.For(25, "Age").GreaterThan(0),
    Guard.For("John Doe", "Name").NotEmpty()
};

// Validate all with result
var result = await validations.ValidateAllAsync();

// Validate all and throw on failure
await validations.ValidateAllAndThrowAsync();
```

### Multi-Validation with Async Rules

Combine parallel execution with async service validation:

```csharp
var result = await Validate.All()
    .ValidateValue(email, "Email", e => e
        .NotEmpty()
        .Email()
        .RespectAsync(async (email, ct, sp) => {
            var userService = sp.GetRequiredService<IUserService>();
            return await userService.IsEmailAvailableAsync(email, ct);
        })
        .WithMessage("Email already exists"))
    .ValidateValue(username, "Username", u => u
        .NotEmpty()
        .RespectAsync(async (user, ct, sp) => {
            var userService = sp.GetRequiredService<IUserService>();
            return await userService.IsUsernameAvailableAsync(user, ct);
        })
        .WithMessage("Username already taken"))
    .ValidateAsync();
```

### Complex Multi-Validation Scenarios

Handle complex validation scenarios with multiple fields and business rules:

```csharp
// User registration validation
var result = await Validate.All()
    .ValidateRequired(firstName, "FirstName")
    .ValidateRequired(lastName, "LastName")
    .ValidateEmail(email)
    .ValidateString(password, "Password", p => p
        .NotEmpty()
        .MinimumLength(8)
        .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
        .WithMessage("Password must contain uppercase, lowercase, number and special character"))
    .ValidateRange(age, "Age", 18, 120)
    .ValidateString(phone, "Phone", p => p
        .NotEmpty()
        .Matches(@"^\+?[1-9]\d{1,14}$")
        .WithMessage("Please enter a valid phone number"))
    .ValidateAsync();

if (!result.IsValid)
{
    // Group errors by field for better UX
    foreach (var fieldErrors in result.ErrorsByField)
    {
        Console.WriteLine($"{fieldErrors.Key}: {string.Join(", ", fieldErrors.Value.Select(e => e.Message))}");
    }
}
```

### MultiValidationResult Features

The `MultiValidationResult` provides rich functionality for working with aggregated validation results:

```csharp
var result = await Validate.AllAsync(validations);

// Basic validation status
Console.WriteLine($"Is Valid: {result.IsValid}");
Console.WriteLine($"Total Errors: {result.ErrorCount}");
Console.WriteLine($"Fields with Errors: {result.FieldsWithErrorsCount}");

// Access all error messages
Console.WriteLine($"All Errors: {result.ErrorMessage}");

// Check specific fields
if (result.HasErrorsForField("Email"))
{
    var emailErrors = result.GetErrorsForField("Email");
    Console.WriteLine($"Email has {emailErrors.Count} errors");
}

// Group errors by field
foreach (var field in result.ErrorsByField)
{
    Console.WriteLine($"{field.Key}: {field.Value.Count} errors");
    foreach (var error in field.Value)
        Console.WriteLine($"  - {error.Message} ({error.Code})");
}

// Get first error for quick feedback
var firstError = result.FirstError;
if (firstError != null)
    Console.WriteLine($"First error: {firstError.Field} - {firstError.Message}");
```

### Standalone Validation with Guard.For()

Use `Guard.For()` for standalone validation outside of model contexts:

```csharp
// Simple field validation
var emailResult = await Guard.For(email, "Email")
    .NotEmpty()
    .Email()
    .ValidateAsync();

if (!emailResult.IsValid)
    Console.WriteLine($"Email error: {emailResult.FirstError?.Message}");

// Validate and throw
await Guard.For(age, "Age")
    .GreaterThan(0)
    .LessThan(150)
    .ValidateAndThrowAsync();

// Async validation with services
var usernameResult = await Guard.For(username, "Username")
    .NotEmpty()
    .MinimumLength(3)
    .RespectAsync(async (user, ct, sp) => {
        var userService = sp.GetService<IUserService>();
        return await userService?.IsUsernameAvailableAsync(user, ct) ?? true;
    })
    .ValidateAsync(serviceProvider);
```

### Performance Benefits

Multi-validation provides significant performance benefits:

1. **Parallel Execution**: All validations run simultaneously using `Task.WhenAll`
2. **Single Service Provider Resolution**: DI services resolved once and shared
3. **Batched Error Collection**: All errors collected in single pass
4. **Reduced Memory Allocation**: Optimized for multiple validation scenarios

```csharp
// Instead of multiple sequential calls (slower)
var emailResult = await Guard.For(email, "Email").Email().ValidateAsync();
var ageResult = await Guard.For(age, "Age").GreaterThan(0).ValidateAsync();
var nameResult = await Guard.For(name, "Name").NotEmpty().ValidateAsync();

// Use parallel multi-validation (faster)
var result = await Validate.AllAsync([
    Guard.For(email, "Email").Email(),
    Guard.For(age, "Age").GreaterThan(0),
    Guard.For(name, "Name").NotEmpty()
]);
```

## Global Exception Handling

Myth.Guard now includes a powerful **Global Exception Handler** that allows you to map any exception type to custom HTTP responses with appropriate status codes and error formats.

### Opt-In Behavior

By default, `UseGuard()` **only handles `ValidationException`** automatically. Other exceptions are **not intercepted** unless you explicitly configure handlers for them. This ensures backward compatibility and gives you full control.

### Quick Setup

Configure exception mappings when adding Guard services:

```csharp
builder.Services.AddGuard( options => {
    options.AutoMapCommonExceptions( );
} );
```

**Important:** Without calling `AutoMapCommonExceptions()` or configuring custom handlers, only `ValidationException` will be handled by the middleware.

The `AutoMapCommonExceptions()` method automatically configures sensible defaults for common .NET exceptions:

- `ArgumentNullException` → 400 Bad Request
- `ArgumentException` → 400 Bad Request
- `InvalidOperationException` → 409 Conflict
- `UnauthorizedAccessException` → 403 Forbidden
- `NotImplementedException` → 501 Not Implemented
- `TimeoutException` → 408 Request Timeout
- Default handler → 500 Internal Server Error (with formatted stack trace in development)

### Custom Exception Mappings

Map your own exception types with fluent configuration:

```csharp
builder.Services.AddGuard( options => {
    // Map specific exception types
    options
        .MapException<NotFoundException>( )
        .WithStatusCode( 404 )
        .WithErrorCode( "NOT_FOUND" )
        .WithResponse( ex => new {
            error = ex.Message,
            resourceType = ex.ResourceType
        } );

    options
        .MapException<BusinessRuleException>( )
        .WithStatusCode( 422 )
        .WithErrorCode( "BUSINESS_RULE_VIOLATION" )
        .WithResponse( ex => new {
            error = ex.Message,
            rule = ex.RuleName,
            details = ex.Details
        } )
        .OnBeforeResponse( ( ex, ctx ) => {
            _logger.LogWarning( ex, "Business rule violation: {Rule}", ex.RuleName );
        } );

    // Configure default handler for unmapped exceptions
    options
        .MapDefaultException( )
        .WithStatusCode( 500 )
        .WithErrorCode( "INTERNAL_ERROR" )
        .WithResponse( ex => new {
            error = _env.IsDevelopment( ) ? ex.Message : "An internal error occurred",
            trace = _env.IsDevelopment( ) ? ex.StackTrace : null
        } )
        .OnBeforeResponse( ( ex, ctx ) => {
            _logger.LogError( ex, "Unhandled exception" );
        } );
} );
```

### API Reference

#### `MapException<TException>()`

Creates a mapping for a specific exception type.

**Chainable Methods:**

- `.WithStatusCode( int statusCode )` - Sets HTTP status code (e.g., 404, 500)
- `.WithStatusCode( HttpStatusCode statusCode )` - Sets HTTP status code using enum (e.g., HttpStatusCode.NotFound)
- `.WithStatusCode( Func<TException, int> resolver )` - Dynamic status code resolver
- `.WithStatusCode( Func<TException, HttpStatusCode> resolver )` - Dynamic status code resolver with enum
- `.WithErrorCode( string code )` - Sets error code string
- `.WithErrorCode( Func<TException, string> resolver )` - Dynamic error code resolver
- `.WithResponse( Func<TException, object> builder )` - Builds response object
- `.OnBeforeResponse( Action<TException, HttpContext> callback )` - Executes before writing response (for logging, telemetry, etc.)

#### `MapDefaultException()`

Configures the fallback handler for unmapped exceptions. Uses the same fluent API as `MapException<TException>()`.

#### `AutoMapCommonExceptions( bool includeStackTrace = true )`

Automatically configures handlers for common .NET exceptions with sensible defaults. In development mode, includes formatted stack traces for the default handler.

### Exception Resolution

The middleware uses **inheritance-aware resolution** to find the best matching handler:

1. **Exact match**: Looks for handler registered for the exact exception type
2. **Inheritance match**: Searches for handlers of base types, prioritizing the most specific match
3. **Default handler**: Falls back to the default handler if no match found
4. **Built-in fallback**: Returns generic 500 error if no handlers configured

### Stack Trace Formatting

When `AutoMapCommonExceptions()` is used with stack traces enabled (default in development), stack traces are automatically formatted for readability:

**Before:**
```
at MyApp.Services.UserService.GetUser(Int32 id) in C:\Projects\MyApp\Services\UserService.cs:line 42
at MyApp.Controllers.UserController.Get(Int32 id) in C:\Projects\MyApp\Controllers\UserController.cs:line 28
```

**After:**
```
  at MyApp.Services.UserService.GetUser(Int32 id) in C:\Projects\MyApp\Services\UserService.cs:line 42
  at MyApp.Controllers.UserController.Get(Int32 id) in C:\Projects\MyApp\Controllers\UserController.cs:line 28
```

### Complete Example

```csharp
// Program.cs
using System.Net;

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddGuard( options => {
    // Auto-map common exceptions
    options.AutoMapCommonExceptions( );

    // Custom domain exceptions using enum
    options
        .MapException<EntityNotFoundException>( )
        .WithStatusCode( HttpStatusCode.NotFound )
        .WithErrorCode( "ENTITY_NOT_FOUND" )
        .WithResponse( ex => new {
            error = $"{ex.EntityType} with ID {ex.EntityId} not found"
        } );

    // Or using int status code
    options
        .MapException<DuplicateEntityException>( )
        .WithStatusCode( 409 )
        .WithErrorCode( "DUPLICATE_ENTITY" )
        .WithResponse( ex => new {
            error = ex.Message,
            conflictingField = ex.FieldName,
            existingId = ex.ExistingEntityId
        } );

    // Dynamic status code using enum
    options
        .MapException<BusinessRuleException>( )
        .WithStatusCode( ex => ex.IsCritical ? HttpStatusCode.Forbidden : HttpStatusCode.UnprocessableEntity )
        .WithErrorCode( "BUSINESS_RULE_VIOLATION" )
        .WithResponse( ex => new {
            error = ex.Message,
            rule = ex.RuleName
        } );
} );

var app = builder.Build( );

// Enable global exception handling
app.UseGuard( );

app.MapControllers( );
app.Run( );
```

```csharp
// Controller usage - no try/catch needed!
[ApiController]
[Route( "api/[controller]" )]
public class UsersController : ControllerBase {

    [HttpGet( "{id}" )]
    public async Task<UserDto> GetUser( int id ) {
        // Throws EntityNotFoundException if not found
        // Automatically handled by Guard middleware
        return await _userService.GetByIdAsync( id );
    }
}
```

### Backward Compatibility

**ValidationException** continues to work exactly as before. The middleware automatically detects and handles it with the existing structured error format:

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

No changes required to existing validation code!

## Testing

The validation design makes testing straightforward:

```csharp
[Fact]
public async Task CreateUser_WithInvalidEmail_ShouldFail()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<IUserService>( sp => mockUserService.Object );
    services.AddGuard();

    var serviceProvider = services.BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator>();

    var dto = new CreateUserDto
    {
        Name = "John Doe",
        Email = "invalid-email",
        Age = 25
    };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ValidationException>(
        () => validator.ValidateAsync( dto, ValidationContextKey.Create ) );

    exception.ValidationResult.Errors.Should().HaveCount( 1 );
    exception.ValidationResult.Errors.First().Field.Should().Be( "Email" );
    exception.ValidationResult.Errors.First().Code.Should().Be( "VIOLATION" );
}

[Fact]
public async Task CreateUser_WithExistingEmail_ShouldReturnConflict()
{
    // Arrange
    mockUserService.Setup( x => x.IsEmailAvailableAsync( "existing@test.com", It.IsAny<CancellationToken>() ) )
               .ReturnsAsync( false );

    var dto = new CreateUserDto
    {
        Name = "John Doe",
        Email = "existing@test.com",
        Age = 25
    };

    // Act
    var result = await validator.ValidateAndReturnAsync( dto, ValidationContextKey.Create );

    // Assert
    result.IsValid.Should().BeFalse();
    result.StatusCode.Should().Be( HttpStatusCode.Conflict );
    result.Errors.Should().ContainSingle( e => e.Code == "EMAIL_EXISTS" );
}
```

## Best Practices

1. **Use Context-Aware Validation**: Leverage `ValidationContextKey` for operation-specific rules
2. **Keep Validation Close to Entities**: Implement `IValidatable<T>` on DTOs for better maintainability
3. **Add Middleware**: Use `app.UseGuard()` for automatic exception handling
4. **Async Rules Sparingly**: Only for database/API checks requiring external services
5. **Meaningful Error Messages**: Provide clear, actionable messages for users
6. **Use Custom Status Codes**: Set appropriate HTTP codes for different validation failures
7. **Stop on Critical Failures**: Use `SetStopOnFailure()` for rules preventing further validation
8. **Test Validation Logic**: Test both positive and negative scenarios
9. **Separate Concerns**: Keep validation focused, avoid business logic in validators
10. **DDD Integration**: Use validation as part of your domain model's invariants

## Architecture Patterns

### Repository Pattern Integration

```csharp
public class UserRepository
{
    private readonly IValidator _validator;
    private readonly IDbContext _context;

    public UserRepository( IValidator validator, IDbContext context )
    {
        _validator = validator;
        _context = context;
    }

    public async Task<User> CreateAsync( CreateUserDto dto )
    {
        await _validator.ValidateAsync( dto, ValidationContextKey.Create );

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Age = dto.Age
        };

        _context.Users.Add( user );
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> UpdateAsync( int id, UpdateUserDto dto )
    {
        var user = await _context.Users.FindAsync( id );

        if ( user == null )
            throw new NotFoundException( "User not found" );

        await _validator.ValidateAsync( dto, ValidationContextKey.Update );

        user.Name = dto.Name;
        user.Age = dto.Age;

        await _context.SaveChangesAsync();

        return user;
    }
}
```

### CQRS Command Validation

```csharp
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IValidator _validator;
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler( IValidator validator, IOrderRepository repository )
    {
        _validator = validator;
        _repository = repository;
    }

    public async Task<CommandResult> HandleAsync( CreateOrderCommand command )
    {
        try
        {
            await _validator.ValidateAsync( command.OrderData, ValidationContextKey.Create );

            var order = await _repository.CreateAsync( command.OrderData );

            return CommandResult.Success();
        }
        catch ( ValidationException ex )
        {
            return CommandResult.Failure( ex.ValidationResult.Errors );
        }
    }
}
```

## Performance Considerations

1. **Async Rules**: Use `RespectAsync()` only when necessary (database/API calls)
2. **Stop on Failure**: Use `SetStopOnFailure()` for expensive validation rules
3. **Context Filtering**: Use specific contexts to avoid unnecessary rule execution
4. **Service Caching**: Cache expensive service calls in async validation rules
5. **Reflection Overhead**: Minimal performance impact for typical use cases

## License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or contributions, please visit the [GitLab repository](https://gitlab.com/dotnet-myth/myth).
