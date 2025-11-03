# Myth.Guard

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Guard?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Guard/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Guard?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Guard/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](README.md)

A powerful, fluent .NET validation library designed for enterprise applications. Built with clean architecture principles, Myth.Guard provides declarative validation with context-awareness, async service integration, and automatic ASP.NET Core middleware.

## Why Myth.Guard?

Most validation libraries force you to choose between attribute-based validation (inflexible) or imperative validation code (verbose and scattered). Myth.Guard offers a third way: **declarative, fluent validation that lives with your entities**, promoting Domain-Driven Design while keeping validation logic maintainable and testable.

## Key Features

- **Declarative Fluent API**: Write readable validation rules with chainable methods
- **Context-Aware Validation**: Different rules for Create, Update, Delete operations on the same entity
- **Async Service Integration**: Access dependency injection for database or API validation
- **Automatic Error Handling**: ASP.NET Core middleware with structured JSON responses
- **100+ Built-in Rules**: Comprehensive validation for strings, numbers, collections, dates, booleans, enums
- **Nullable Type Support**: Full support for nullable value types with dedicated rules
- **Custom Rules**: Easy extensibility with `Respect()` and `RespectAsync()` methods
- **Conditional Validation**: Field-level and entity-level conditional rules
- **Stop on Failure**: Optimize validation by stopping after critical failures
- **HTTP Status Customization**: Configure appropriate status codes per validation error

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

builder.Services.AddGuard();

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
