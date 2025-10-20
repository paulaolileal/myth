# Myth.Guard

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Guard?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Guard/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Guard?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Guard/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

A powerful .NET library for building maintainable and type-safe validation systems with a fluent, declarative API. Built with enterprise-grade features including context-aware validation, async service provider integration, automatic ASP.NET Core middleware, and comprehensive error handling.

# ⭐ Features

- **Fluent Interface**: Declarative, chainable API for readable validation code
- **Type Safety**: Strong typing with comprehensive rule coverage for all .NET types
- **Context-Aware Validation**: Different validation rules per operation (Create, Update, Delete, etc.)
- **Async Service Integration**: Access dependency injection container in validation rules
- **ASP.NET Core Middleware**: Automatic validation exception handling with structured responses
- **400+ Validation Rules**: Extensive built-in rules for strings, numbers, collections, dates, and more
- **Custom Rules**: Easy extensibility with `Respect()` and `RespectAsync()` methods
- **Conditional Validation**: Execute rules based on entity or field conditions
- **Error Aggregation**: Collect all validation errors before failing
- **HTTP Status Customization**: Configure HTTP status codes per validation rule

# 📦 Installation

```bash
dotnet add package Myth.Guard
```

# 🚀 Quick Start

## Basic Usage

```csharp
public class CreateUserDto : IValidatable<CreateUserDto>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public List<string> Tags { get; set; }

    public void Validate(ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context = null)
    {
        builder.For(Name, x => x.NotEmpty().MinimumLength(2).MaximumLength(100));
        builder.For(Email, x => x.NotEmpty().Email());
        builder.For(Age, x => x.GreaterThan(0).LessThan(150));
        builder.For(Tags, x => x.NotEmpty().CountBetween(1, 10));
    }
}

// Validate and throw exception on failure
await validator.ValidateAsync(dto);

// Or validate and return result
var result = await validator.ValidateAndReturnAsync(dto);
if (!result.IsValid)
{
    // Handle validation errors
}
```

## Dependency Injection Setup

### Program.cs (Minimal API)

```csharp
builder.Services.AddGuard();

// Register your services for async validation
builder.Services.AddScoped<IUserService, UserService>();
```

### Add Middleware

```csharp
var app = builder.Build();

app.UseGuard(); // Adds automatic validation exception handling

app.MapControllers();
```

### Using in Controllers/Services

```csharp
public class UserController : ControllerBase
{
    private readonly IValidator _validator;

    public UserController(IValidator validator)
    {
        _validator = validator;
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        // Validate and throw ValidationException on failure
        await _validator.ValidateAsync(request, ValidationContextKey.Create);

        // Or validate and check result without throwing
        var result = await _validator.ValidateAndReturnAsync(request, ValidationContextKey.Create);
        if (!result.IsValid)
        {
            return BadRequest(new { errors = result.Errors });
        }

        // Process user creation...
        return Ok(new { message = "User created successfully" });
    }
}
```

# 🔧 Configuration

## Basic Configuration

```csharp
// Simple setup - no additional configuration needed
builder.Services.AddGuard();
app.UseGuard();
```

## Error Response Format

The middleware automatically formats validation errors into structured JSON responses:

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
            "message": "Value must be greater than 18",
            "code": "VIOLATION"
        }
    ]
}
```

# 📋 Validation Rules

## String Rules

```csharp
builder.For(Email, x => x
    .NotEmpty()
    .Email()
    .MaximumLength(254));

builder.For(Name, x => x
    .NotEmpty()
    .MinimumLength(2)
    .MaximumLength(100)
    .OnlyLetters());

builder.For(Password, x => x
    .NotEmpty()
    .MinimumLength(8)
    .Matches(new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$"))
    .WithMessage("Password must contain uppercase, lowercase, and digit"));

builder.For(PhoneNumber, x => x
    .NotEmpty()
    .Matches(new Regex(@"^\+\d{1,3}\d{10,14}$"))
    .WithMessage("Invalid phone number format"));
```

**Available String Rules:**
- `NotEmpty()`, `MinimumLength(int)`, `MaximumLength(int)`, `LengthBetween(int, int)`
- `Email()`, `Url()` - Format validation
- `OnlyLetters()`, `OnlyNumbers()`, `Alphanumeric()` - Character type validation
- `StartsWith(string)`, `EndsWith(string)`, `Contains(string)` - Substring checks
- `Matches(Regex)` - Regex pattern matching
- `EqualsTo(string)`, `BeOneOf(params string[])` - Enumeration checks

## Numeric Rules

```csharp
builder.For(Age, x => x
    .GreaterThan(0)
    .LessThan(150));

builder.For(Salary, x => x
    .GreaterOrEquals(0)
    .LessThan(1000000m));

builder.For(Score, x => x
    .Between(0, 100)
    .When(score => score.HasValue));

builder.For(Quantity, x => x
    .Positive()
    .NotZero());
```

**Available Numeric Rules (int, long, decimal, double, etc.):**
- `GreaterThan(T)`, `GreaterOrEquals(T)`, `LessThan(T)`, `LessOrEquals(T)`
- `Between(T min, T max)` - Range validation
- `Positive()`, `Negative()`, `Zero()`, `NotZero()`

## Collection Rules

```csharp
builder.For(Tags, x => x
    .NotEmpty()
    .CountBetween(1, 10)
    .All(tag => !string.IsNullOrWhiteSpace(tag))
    .Distinct());

builder.For(UserRoles, x => x
    .NotEmpty()
    .Any(role => role == "Admin" || role == "User")
    .None(role => role == "Banned"));
```

**Available Collection Rules:**
- `NotEmpty()` - Collection not empty
- `CountBetween(int min, int max)`, `CountGreaterThan(int)`, `CountLessThan(int)`
- `All<T>(Func<T, bool> predicate)` - All elements match condition
- `Any<T>(Func<T, bool> predicate)` - At least one matches
- `None<T>(Func<T, bool> predicate)` - No elements match
- `Distinct<T>()`, `DistinctBy<T, TKey>(Func<T, TKey> keySelector)` - Duplicate detection

## DateTime Rules

```csharp
builder.For(BirthDate, x => x
    .Past()
    .After(new DateTime(1900, 1, 1)));

builder.For(ScheduledDate, x => x
    .Future()
    .Before(DateTime.Now.AddYears(1)));

builder.For(AppointmentDate, x => x
    .Between(DateTime.Today, DateTime.Today.AddDays(30)));
```

**Available DateTime Rules:**
- `Past()`, `Future()`, `Today()`
- `After(DateTime)`, `Before(DateTime)`, `Between(DateTime, DateTime)`
- `AfterOrEquals(DateTime)`, `BeforeOrEquals(DateTime)`

## Boolean and Enum Rules

```csharp
builder.For(IsActive, x => x.IsTrue());
builder.For(IsDeleted, x => x.IsFalse());

builder.For(Role, x => x.BeInEnum<UserRole>());
builder.For(Status, x => x.BeOneOf(Status.Active, Status.Pending));
```

## Generic Rules (All Types)

```csharp
builder.For(UserId, x => x
    .NotNull()
    .NotDefault());

builder.For(Email, x => x
    .NotNull()
    .NotEqualsTo("admin@example.com"));
```

**Available Generic Rules:**
- `NotNull()`, `BeNull()`
- `EqualsTo(T)`, `NotEqualsTo(T)`
- `BeDefault()`, `NotDefault()`
- `Respect(Func<T, bool> predicate)` - Custom sync validation
- `RespectAsync(Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate)` - Custom async validation

# 🎯 Context-Aware Validation

One of the most powerful features is context-specific validation rules:

```csharp
public class UserDto : IValidatable<UserDto>
{
    public string Email { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public string Password { get; set; }

    public void Validate(ValidationBuilder<UserDto> builder, ValidationContextKey? context = null)
    {
        // Global rules (apply to all contexts)
        builder.For(Email, x => x.NotEmpty().Email());
        builder.For(Age, x => x.GreaterThan(0).LessThan(150));

        // Create-specific rules
        builder.InContext(ValidationContextKey.Create, b =>
        {
            b.For(Email, x => x
                .RespectAsync(async (email, ct, sp) =>
                {
                    var userService = sp.GetRequiredService<IUserService>();
                    return await userService.IsEmailAvailableAsync(email, ct);
                })
                .WithMessage("Email already exists")
                .WithCode("EMAIL_EXISTS")
                .WithStatusCode(HttpStatusCode.Conflict));

            b.For(Password, x => x
                .NotEmpty()
                .MinimumLength(8)
                .Matches(new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$")));

            b.For(IsActive, x => x.IsTrue());
        });

        // Update-specific rules
        builder.InContext(ValidationContextKey.Update, b =>
        {
            b.For(Age, x => x.GreaterOrEquals(18));
            // Password is optional on update
        });

        // Delete-specific rules
        builder.InContext(ValidationContextKey.Delete, b =>
        {
            b.For(IsActive, x => x.IsFalse()
                .WithMessage("Cannot delete active user"));
        });
    }
}

// Usage with different contexts
await validator.ValidateAsync(user, ValidationContextKey.Create);
await validator.ValidateAsync(user, ValidationContextKey.Update);
await validator.ValidateAsync(user, ValidationContextKey.Delete);
```

## Pre-defined Contexts

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
ValidationContextKey.Custom("MyCustomContext")
```

# 🔄 Conditional Validation

Execute validation rules based on conditions:

## Field-Level Conditions

```csharp
builder.For(PhoneNumber, x => x
    .NotEmpty()
    .When(phone => !string.IsNullOrEmpty(phone)) // Only validate if not empty
    .Matches(new Regex(@"^\+\d{1,3}\d{10,14}$")));

builder.For(Password, x => x
    .NotEmpty()
    .Unless(pwd => IsExternalUser) // Skip for external users
    .MinimumLength(8));
```

## Entity-Level Conditions

```csharp
builder.For(PhoneNumber, x => x
    .NotEmpty()
    .When<string, UserDto>(user => user.PhoneType == PhoneType.Required)
    .Unless<string, UserDto>(user => user.IsVerified));

builder.For(Salary, x => x
    .GreaterThan(0)
    .When<decimal, EmployeeDto>(emp => emp.EmploymentType == EmploymentType.FullTime));
```

# 🔁 Async Validation with Service Provider

Access dependency injection container for database or API validation:

```csharp
public class CreateOrderDto : IValidatable<CreateOrderDto>
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string CustomerEmail { get; set; }

    public void Validate(ValidationBuilder<CreateOrderDto> builder, ValidationContextKey? context = null)
    {
        builder.For(ProductId, x => x
            .GreaterThan(0)
            .RespectAsync(async (productId, ct, sp) =>
            {
                var productService = sp.GetRequiredService<IProductService>();
                return await productService.ExistsAsync(productId, ct);
            })
            .WithMessage("Product does not exist")
            .WithCode("PRODUCT_NOT_FOUND"));

        builder.For(Quantity, x => x
            .GreaterThan(0)
            .RespectAsync(async (quantity, ct, sp) =>
            {
                var productService = sp.GetRequiredService<IProductService>();
                var stock = await productService.GetStockAsync(ProductId, ct);
                return quantity <= stock;
            })
            .WithMessage("Insufficient stock")
            .WithCode("INSUFFICIENT_STOCK"));

        builder.For(CustomerEmail, x => x
            .NotEmpty()
            .Email()
            .RespectAsync(async (email, ct, sp) =>
            {
                var customerService = sp.GetRequiredService<ICustomerService>();
                return await customerService.IsActiveCustomerAsync(email, ct);
            })
            .WithMessage("Customer not found or inactive")
            .WithCode("CUSTOMER_INACTIVE")
            .WithStatusCode(HttpStatusCode.NotFound));
    }
}
```

# 🎨 Advanced Features

## Custom Error Messages

```csharp
// Static message
builder.For(Age, x => x
    .GreaterThan(18)
    .WithMessage("User must be at least 18 years old"));

// Dynamic message using field value
builder.For(Age, x => x
    .GreaterThan(18)
    .WithMessage(age => $"User must be at least 18 years old, but is {age}"));

// Custom error code
builder.For(Email, x => x
    .Email()
    .WithCode("INVALID_EMAIL_FORMAT"));

// Custom HTTP status code
builder.For(UserId, x => x
    .RespectAsync(async (id, ct, sp) =>
    {
        var userService = sp.GetRequiredService<IUserService>();
        return await userService.ExistsAsync(id, ct);
    })
    .WithMessage("User not found")
    .WithCode("USER_NOT_FOUND")
    .WithStatusCode(HttpStatusCode.NotFound)); // Returns 404 instead of 400
```

## Stop on Failure

Stop validating a field after the first failure:

```csharp
builder.For(Password, x => x
    .NotEmpty()
    .SetStopOnFailure() // Don't check other rules if empty
    .MinimumLength(8)
    .Matches(new Regex(@"[A-Z]"))
    .Matches(new Regex(@"[a-z]"))
    .Matches(new Regex(@"\d")));
```

## Complex Business Rules

```csharp
public class OrderDto : IValidatable<OrderDto>
{
    public decimal Amount { get; set; }
    public string CustomerType { get; set; }
    public List<OrderItem> Items { get; set; }
    public string CouponCode { get; set; }

    public void Validate(ValidationBuilder<OrderDto> builder, ValidationContextKey? context = null)
    {
        builder.For(Amount, x => x
            .GreaterThan(0)
            .When<decimal, OrderDto>(order => order.Items?.Any() == true));

        // Premium customers can have higher order amounts
        builder.For(Amount, x => x
            .LessThan(10000)
            .Unless<decimal, OrderDto>(order => order.CustomerType == "Premium"));

        builder.For(Items, x => x
            .NotEmpty()
            .CountBetween(1, 50)
            .All(item => item.Quantity > 0)
            .All(item => item.Price > 0));

        // Coupon validation
        builder.For(CouponCode, x => x
            .RespectAsync(async (coupon, ct, sp) =>
            {
                if (string.IsNullOrEmpty(coupon)) return true; // Optional

                var couponService = sp.GetRequiredService<ICouponService>();
                var isValid = await couponService.IsValidAsync(coupon, ct);
                var isApplicable = await couponService.IsApplicableToOrderAsync(coupon, Amount, ct);
                return isValid && isApplicable;
            })
            .WithMessage("Invalid or inapplicable coupon code")
            .WithCode("INVALID_COUPON"));
    }
}
```

# ❌ Error Handling

## Validation Result

```csharp
var result = await validator.ValidateAndReturnAsync(dto);

Console.WriteLine($"Is Valid: {result.IsValid}");
Console.WriteLine($"Status Code: {result.StatusCode}");

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Field: {error.Field}");
        Console.WriteLine($"Message: {error.Message}");
        Console.WriteLine($"Code: {error.Code}");
        Console.WriteLine($"Status: {error.StatusCode}");
    }
}
```

## Exception Handling

```csharp
try
{
    await validator.ValidateAsync(dto, ValidationContextKey.Create);
}
catch (ValidationException ex)
{
    var errors = ex.ValidationResult.Errors;
    var statusCode = ex.ValidationResult.StatusCode;

    // Log errors or transform to custom response
    return BadRequest(new
    {
        message = "Validation failed",
        errors = errors.Select(e => new
        {
            field = e.Field,
            message = e.Message,
            code = e.Code
        })
    });
}
```

## Middleware Error Response

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

HTTP Status Code: The highest status code from all validation errors (e.g., if one error has `409 Conflict`, the response will be `409`).

# 🧪 Testing

The validation design makes testing straightforward:

```csharp
[Fact]
public async Task CreateUser_WithInvalidEmail_ShouldFail()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<IUserService>(sp => mockUserService.Object);
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
        () => validator.ValidateAsync(dto, ValidationContextKey.Create));

    exception.ValidationResult.Errors.Should().HaveCount(1);
    exception.ValidationResult.Errors.First().Field.Should().Be("Email");
    exception.ValidationResult.Errors.First().Code.Should().Be("VIOLATION");
}

[Fact]
public async Task CreateUser_WithExistingEmail_ShouldReturnConflict()
{
    // Arrange
    mockUserService.Setup(x => x.IsEmailAvailableAsync("existing@test.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

    var dto = new CreateUserDto
    {
        Name = "John Doe",
        Email = "existing@test.com",
        Age = 25
    };

    // Act
    var result = await validator.ValidateAndReturnAsync(dto, ValidationContextKey.Create);

    // Assert
    result.IsValid.Should().BeFalse();
    result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    result.Errors.Should().ContainSingle(e => e.Code == "EMAIL_EXISTS");
}
```

## Testing Custom Rules

```csharp
[Fact]
public async Task ValidateOrderAmount_WithPremiumCustomer_ShouldAllowHigherAmount()
{
    // Arrange
    var order = new OrderDto
    {
        Amount = 15000, // Above normal limit
        CustomerType = "Premium",
        Items = new List<OrderItem> { new() { Quantity = 1, Price = 15000 } }
    };

    // Act
    var result = await validator.ValidateAndReturnAsync(order);

    // Assert
    result.IsValid.Should().BeTrue();
}
```

# 📋 Best Practices

1. **Use Context-Aware Validation**: Leverage `ValidationContextKey` for operation-specific rules
2. **Configure Dependency Injection**: Always use DI for service access in async validation
3. **Add Middleware**: Use `app.UseGuard()` for automatic exception handling
4. **Separate Concerns**: Keep validation rules focused and business-logic free
5. **Use Async Rules Sparingly**: Only for database/API checks that require external services
6. **Handle Errors Gracefully**: Always check `IsValid` before accessing validated data
7. **Use Custom Status Codes**: Set appropriate HTTP status codes for different validation failures
8. **Test Validation Logic**: Test both positive and negative validation scenarios
9. **Stop on Critical Failures**: Use `SetStopOnFailure()` for rules that prevent further validation
10. **Use Meaningful Error Messages**: Provide clear, actionable error messages for users

# 🏗️ Advanced Patterns

## Multi-Step Validation Pipeline

```csharp
public class ComplexOrderDto : IValidatable<ComplexOrderDto>
{
    public CustomerInfo Customer { get; set; }
    public List<OrderItem> Items { get; set; }
    public PaymentInfo Payment { get; set; }
    public ShippingInfo Shipping { get; set; }

    public void Validate(ValidationBuilder<ComplexOrderDto> builder, ValidationContextKey? context = null)
    {
        // Customer validation
        builder.For(Customer, x => x.NotNull());
        builder.For(Customer?.Email, x => x
            .NotEmpty()
            .Email()
            .RespectAsync(async (email, ct, sp) =>
            {
                var customerService = sp.GetRequiredService<ICustomerService>();
                return await customerService.IsActiveAsync(email, ct);
            })
            .WithMessage("Customer not found or inactive"));

        // Items validation
        builder.For(Items, x => x
            .NotEmpty()
            .CountBetween(1, 100)
            .All(item => item.Quantity > 0)
            .All(item => item.Price > 0));

        // Payment validation
        builder.For(Payment?.CardNumber, x => x
            .NotEmpty()
            .Matches(new Regex(@"^\d{13,19}$"))
            .RespectAsync(async (cardNumber, ct, sp) =>
            {
                var paymentService = sp.GetRequiredService<IPaymentService>();
                return await paymentService.ValidateCardAsync(cardNumber, ct);
            })
            .WithMessage("Invalid payment card"));

        // Shipping validation
        builder.For(Shipping?.Address, x => x
            .NotEmpty()
            .MinimumLength(10)
            .RespectAsync(async (address, ct, sp) =>
            {
                var shippingService = sp.GetRequiredService<IShippingService>();
                return await shippingService.CanDeliverToAsync(address, ct);
            })
            .WithMessage("Delivery not available to this address"));

        // Cross-field validation
        builder.InContext(ValidationContextKey.Create, b =>
        {
            // Total amount must match items
            b.For(Payment?.Amount, x => x
                .EqualsTo(Items?.Sum(i => i.Price * i.Quantity))
                .WithMessage("Payment amount doesn't match order total"));

            // Express shipping requires premium customer
            b.For(Shipping?.Type, x => x
                .Respect(type => type != "Express" || Customer?.Type == "Premium")
                .WithMessage("Express shipping only available for premium customers"));
        });
    }
}
```

## Repository Pattern Integration

```csharp
public class UserRepository
{
    private readonly IValidator _validator;
    private readonly IDbContext _context;

    public UserRepository(IValidator validator, IDbContext context)
    {
        _validator = validator;
        _context = context;
    }

    public async Task<User> CreateAsync(CreateUserDto dto)
    {
        // Validate before creation
        await _validator.ValidateAsync(dto, ValidationContextKey.Create);

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Age = dto.Age
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new NotFoundException("User not found");

        // Validate before update
        await _validator.ValidateAsync(dto, ValidationContextKey.Update);

        user.Name = dto.Name;
        user.Age = dto.Age;
        // Email typically not updated

        await _context.SaveChangesAsync();
        return user;
    }
}
```

## CQRS Command Validation

```csharp
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IValidator _validator;
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler(IValidator validator, IOrderRepository repository)
    {
        _validator = validator;
        _repository = repository;
    }

    public async Task<CommandResult> HandleAsync(CreateOrderCommand command)
    {
        try
        {
            // Validate command
            await _validator.ValidateAsync(command.OrderData, ValidationContextKey.Create);

            // Process order
            var order = await _repository.CreateAsync(command.OrderData);

            return CommandResult.Success();
        }
        catch (ValidationException ex)
        {
            return CommandResult.Failure(ex.ValidationResult.Errors);
        }
    }
}
```

# 📊 Performance Considerations

1. **Async Rules**: Use `RespectAsync()` only when necessary (database/API calls)
2. **Stop on Failure**: Use `SetStopOnFailure()` for expensive validation rules
3. **Context Filtering**: Use specific contexts to avoid unnecessary rule execution
4. **Service Caching**: Cache expensive service calls in async validation rules
5. **Reflection Overhead**: The library uses reflection to extract field values - minimal performance impact for typical use cases

# 📄 License

This project is licensed under the Apache License 2.0 - see the LICENSE file for details.

# 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

# 📧 Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/paulaolileal/myth).