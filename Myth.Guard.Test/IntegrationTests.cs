using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.DependencyInjection;
using Myth.Guard.Test.Models;
using Myth.Interfaces;
using Myth.Models;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Integration tests for the complete Myth.Guard validation system
/// </summary>
public class IntegrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IValidator _validator;

    public IntegrationTests()
    {
        var services = new ServiceCollection();
        services.AddGuard();
        services.AddSingleton<ITestUserService, TestUserService>();
        _serviceProvider = services.BuildServiceProvider();
        _validator = _serviceProvider.GetRequiredService<IValidator>();
    }

    [Fact]
    public async Task CompleteValidationFlow_WithValidEntity_ShouldSucceed()
    {
        // Arrange
        var user = new TestUser
        {
            Name = "IntegrationTestUser",
            Email = "integration@example.com",
            Age = 28,
            BirthDate = new DateTime(1995, 5, 15),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
            Tags = new List<string> { "developer", "senior", "remote" },
            IsActive = true,
            Role = UserRole.User,
            PhoneType = PhoneType.Optional,
            PhoneNumber = null,
            IsVerified = true,
            Salary = 75000m,
            Score = 92.5
        };

        // Act & Assert - Global validation
        var globalResult = await _validator.ValidateAndReturnAsync(user);
        globalResult.IsValid.Should().BeTrue();

        // Act & Assert - Create context validation
        var createResult = await _validator.ValidateAndReturnAsync(user, ValidationContextKey.Create);
        createResult.IsValid.Should().BeTrue();

        // Act & Assert - Update context validation
        var updateResult = await _validator.ValidateAndReturnAsync(user, ValidationContextKey.Update);
        updateResult.IsValid.Should().BeTrue();

        // Act & Assert - Custom context validation
        var customResult = await _validator.ValidateAndReturnAsync(user, ValidationContextKey.Custom("CustomContext"));
        customResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteValidationFlow_WithInvalidEntity_ShouldReturnAllErrors()
    {
        // Arrange - Create an entity that violates multiple rules
        var user = new TestUser
        {
            Name = "A", // Too short (< 2 chars), but let's make it just 1 char to test minimum length
            Email = "invalid-email-format", // Invalid email
            Age = -5, // Negative age
            BirthDate = DateTime.Now.AddYears(5), // Future birth date
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)), // Future registration
            Tags = new List<string>(), // Empty tags
            IsActive = true,
            Role = (UserRole)999, // Invalid enum value
            PhoneType = PhoneType.Required,
            PhoneNumber = null, // Required but null
            IsVerified = false,
            Salary = -1000m, // Negative salary
            Score = 150.0 // Score > 100
        };

        // Act
        var result = await _validator.ValidateAndReturnAsync(user);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.Errors.Count.Should().BeGreaterThan(5); // Should have multiple validation errors

        // Verify specific error fields
        var errorFields = result.Errors.Select(e => e.Field).ToList();
        errorFields.Should().Contain("Name");
        errorFields.Should().Contain("Email");
        errorFields.Should().Contain("Age");
        errorFields.Should().Contain("BirthDate");
        errorFields.Should().Contain("RegistrationDate");
        errorFields.Should().Contain("Tags");
        errorFields.Should().Contain("Role");
        errorFields.Should().Contain("PhoneNumber");
        errorFields.Should().Contain("Salary");
        errorFields.Should().Contain("Score");
    }

    [Fact]
    public async Task ContextSpecificValidation_ShouldApplyDifferentRules()
    {
        // Arrange - User valid for general use but has context-specific issues
        var user = new TestUser
        {
            Name = "ContextTestUser",
            Email = "taken@example.com", // This email is "taken" according to TestUserService
            Age = 16, // Valid globally, but invalid for Update context (< 18)
            BirthDate = DateTime.Now.AddYears(-16),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
            Tags = new List<string> { "young" },
            IsActive = false, // Invalid for Create context (should be true)
            Role = UserRole.User,
            Salary = 5000m,
            Score = 75.0
        };

        // Act - Default context (should pass global rules despite age being 16)
        var defaultResult = await _validator.ValidateAndReturnAsync(user);

        // Act - Create context (should fail on email existence and IsActive)
        var createResult = await _validator.ValidateAndReturnAsync(user, ValidationContextKey.Create);

        // Act - Update context (should fail on age < 18)
        var updateResult = await _validator.ValidateAndReturnAsync(user, ValidationContextKey.Update);

        // Assert - Default context passes
        defaultResult.IsValid.Should().BeTrue();

        // Assert - Create context fails
        createResult.IsValid.Should().BeFalse();
        createResult.Errors.Should().Contain(e => e.Field == "Email" && e.Code == "EMAIL_EXISTS");
        createResult.Errors.Should().Contain(e => e.Field == "IsActive");

        // Assert - Update context fails
        updateResult.IsValid.Should().BeFalse();
        updateResult.Errors.Should().Contain(e => e.Field == "Age");
    }

    [Fact]
    public async Task AsyncValidationRules_ShouldAccessServices()
    {
        // Arrange
        var userWithTakenEmail = new TestUser
        {
            Name = "AsyncTestUser",
            Email = "taken@example.com", // This email is marked as taken in TestUserService
            Age = 25,
            BirthDate = DateTime.Now.AddYears(-25),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
            Tags = new List<string> { "test" },
            IsActive = true,
            Role = UserRole.User,
            Salary = 50000m,
            Score = 85.0
        };

        var userWithAvailableEmail = new TestUser
        {
            Name = "AsyncTestUser2",
            Email = "available@example.com", // This email is available
            Age = 25,
            BirthDate = DateTime.Now.AddYears(-25),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
            Tags = new List<string> { "test" },
            IsActive = true,
            Role = UserRole.User,
            Salary = 50000m,
            Score = 85.0
        };

        // Act
        var takenEmailResult = await _validator.ValidateAndReturnAsync(userWithTakenEmail, ValidationContextKey.Create);
        var availableEmailResult = await _validator.ValidateAndReturnAsync(userWithAvailableEmail, ValidationContextKey.Create);

        // Assert
        takenEmailResult.IsValid.Should().BeFalse();
        takenEmailResult.Errors.Should().Contain(e => e.Field == "Email" && e.Code == "EMAIL_EXISTS");

        availableEmailResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ConditionalValidationRules_ShouldApplyBasedOnConditions()
    {
        // Test case 1: PhoneType.Required, IsVerified = false -> PhoneNumber should be required
        var user1 = new TestUser
        {
            Name = "ConditionalUser1",
            Email = "conditional1@example.com",
            Age = 25,
            BirthDate = DateTime.Now.AddYears(-25),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
            Tags = new List<string> { "test" },
            IsActive = true,
            Role = UserRole.User,
            PhoneType = PhoneType.Required,
            PhoneNumber = null, // Should fail validation
            IsVerified = false,
            Salary = 50000m,
            Score = 85.0
        };

        // Test case 2: PhoneType.Required, IsVerified = true -> PhoneNumber not required (Unless condition)
        var user2 = new TestUser
        {
            Name = "ConditionalUser2",
            Email = "conditional2@example.com",
            Age = 25,
            BirthDate = DateTime.Now.AddYears(-25),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
            Tags = new List<string> { "test" },
            IsActive = true,
            Role = UserRole.User,
            PhoneType = PhoneType.Required,
            PhoneNumber = null, // Should pass validation due to Unless condition
            IsVerified = true,
            Salary = 50000m,
            Score = 85.0
        };

        // Test case 3: PhoneType.Optional -> PhoneNumber not required (When condition not met)
        var user3 = new TestUser
        {
            Name = "ConditionalUser3",
            Email = "conditional3@example.com",
            Age = 25,
            BirthDate = DateTime.Now.AddYears(-25),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
            Tags = new List<string> { "test" },
            IsActive = true,
            Role = UserRole.User,
            PhoneType = PhoneType.Optional,
            PhoneNumber = null, // Should pass validation
            IsVerified = false,
            Salary = 50000m,
            Score = 85.0
        };

        // Act
        var result1 = await _validator.ValidateAndReturnAsync(user1);
        var result2 = await _validator.ValidateAndReturnAsync(user2);
        var result3 = await _validator.ValidateAndReturnAsync(user3);

        // Assert
        result1.IsValid.Should().BeFalse();
        result1.Errors.Should().Contain(e => e.Field == "PhoneNumber");

        result2.IsValid.Should().BeTrue();

        result3.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task MultipleValidationTypes_ShouldAllWork()
    {
        // Arrange - Entity that tests all validation rule types
        var stringEntity = new StringTestEntity
        {
            Text = "ValidText123",
            Email = "valid@example.com",
            Url = "https://example.com",
            AlphaNumeric = "abc123"
        };

        var numericEntity = new NumericTestEntity
        {
            IntValue = 50,
            DecimalValue = 500.50m,
            DoubleValue = 0.75,
            LongValue = 1000L
        };

        var collectionEntity = new CollectionTestEntity
        {
            Items = new List<string> { "item1", "item2", "item3" },
            Tags = new[] { "tag1", "tag2" },
            Numbers = new[] { 1, 2, 3 }
        };

        var dateTimeEntity = new DateTimeTestEntity
        {
            CreatedAt = DateTime.Now.AddDays(-5),
            UpdatedAt = DateTime.Now.AddDays(2),
            EventDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
        };

        var booleanEntity = new BooleanTestEntity
        {
            IsActive = true,
            IsDeleted = false,
            IsOptional = true
        };

        // Act
        var stringResult = await _validator.ValidateAndReturnAsync(stringEntity);
        var numericResult = await _validator.ValidateAndReturnAsync(numericEntity);
        var collectionResult = await _validator.ValidateAndReturnAsync(collectionEntity);
        var dateTimeResult = await _validator.ValidateAndReturnAsync(dateTimeEntity);
        var booleanResult = await _validator.ValidateAndReturnAsync(booleanEntity);

        // Assert
        stringResult.IsValid.Should().BeTrue();
        numericResult.IsValid.Should().BeTrue();
        collectionResult.IsValid.Should().BeTrue();
        dateTimeResult.IsValid.Should().BeTrue();
        booleanResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CancellationToken_ShouldBeRespected()
    {
        // Arrange
        var user = new TestUser
        {
            Name = "CancellationTestUser",
            Email = "cancellation@example.com",
            Age = 25,
            BirthDate = DateTime.Now.AddYears(-25),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
            Tags = new List<string> { "test" },
            IsActive = true,
            Role = UserRole.User,
            Salary = 50000m,
            Score = 85.0
        };

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var act = async () => await _validator.ValidateAsync(user, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ValidationWithServiceProvider_ShouldInjectDependencies()
    {
        // This test verifies that the service provider is correctly passed to validation rules
        // and that dependencies can be resolved during async validation

        // Arrange
        var user = new TestUser
        {
            Name = "ServiceProviderTest",
            Email = "service@example.com", // Available email
            Age = 25,
            BirthDate = DateTime.Now.AddYears(-25),
            RegistrationDate = DateOnly.FromDateTime(DateTime.Today),
            Tags = new List<string> { "test" },
            IsActive = true,
            Role = UserRole.User,
            Salary = 50000m,
            Score = 85.0
        };

        // Act
        var result = await _validator.ValidateAndReturnAsync(user, ValidationContextKey.Create);

        // Assert
        result.IsValid.Should().BeTrue();

        // Verify that ITestUserService was accessible during validation
        // (if it wasn't, the async rule would have failed or thrown an exception)
        var userService = _serviceProvider.GetService<ITestUserService>();
        userService.Should().NotBeNull();
    }
}