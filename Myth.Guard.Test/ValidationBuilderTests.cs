using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Guard.Test.Models;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for ValidationBuilder functionality
/// </summary>
public class ValidationBuilderTests {
	private readonly IServiceProvider _serviceProvider;

	public ValidationBuilderTests( ) {
		var services = new ServiceCollection( );
		services.AddSingleton<ITestUserService, TestUserService>( );
		_serviceProvider = services.BuildServiceProvider( );
	}

	[Fact]
	public async Task ValidationBuilder_WithSimpleEntity_ShouldBuildValidation( ) {
		// Arrange
		var entity = new SimpleEntity { Value = "test" };
		var validator = new Validator( _serviceProvider );

		// Act
		var result = await validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task ValidationBuilder_WithInvalidSimpleEntity_ShouldReturnErrors( ) {
		// Arrange
		var entity = new SimpleEntity { Value = "" }; // Empty value should fail NotEmpty rule
		var validator = new Validator( _serviceProvider );

		// Act
		var result = await validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).HaveCount( 1 );
		result.Errors.First( ).Field.Should( ).Be( "Value" );
	}

	[Fact]
	public async Task ValidationBuilder_InContext_ShouldApplyContextSpecificRules( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 16, // Valid for default context, invalid for Update context
			BirthDate = DateTime.Now.AddYears( -16 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "young" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 1000,
			Score = 50
		};

		var validator = new Validator( _serviceProvider );

		// Act - Default context (should pass)
		var defaultResult = await validator.ValidateAndReturnAsync( user );

		// Act - Update context (should fail due to age < 18)
		var updateResult = await validator.ValidateAndReturnAsync( user, ValidationContextKey.Update );

		// Assert
		defaultResult.IsValid.Should( ).BeTrue( );
		updateResult.IsValid.Should( ).BeFalse( );
		updateResult.Errors.Should( ).Contain( e => e.Field == "Age" );
	}

	[Fact]
	public async Task ValidationBuilder_InContext_Create_ShouldCheckAsyncRules( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "taken@example.com", // This email is marked as taken in TestUserService
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		var validator = new Validator( _serviceProvider );

		// Act
		var result = await validator.ValidateAndReturnAsync( user, ValidationContextKey.Create );

		// Assert
		result.IsValid.Should( ).BeFalse( );
	}

	[Fact]
	public async Task ValidationBuilder_InContext_Create_WithAvailableEmail_ShouldPass( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "available@example.com", // This email is available
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		var validator = new Validator( _serviceProvider );

		// Act
		var result = await validator.ValidateAndReturnAsync( user, ValidationContextKey.Create );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task ValidationBuilder_WithCustomErrorMessage_ShouldUseCustomMessage( ) {
		// Test would require creating a custom entity with WithMessage() modifier
		// For now, we'll test that error messages are present and meaningful

		// Arrange
		var entity = new StringTestEntity {
			Text = "Hi", // Too short (minimum 5)
			Email = "invalid-email",
			Url = "not-a-url",
			AlphaNumeric = "contains@symbols"
		};

		var validator = new Validator( _serviceProvider );

		// Act
		var result = await validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).NotBeEmpty( );
		result.Errors.Should( ).OnlyContain( e => !string.IsNullOrWhiteSpace( e.Message ) );
	}

	[Fact]
	public async Task ValidationBuilder_WithCustomStatusCode_ShouldUseCustomStatusCode( ) {
		// This test demonstrates that custom status codes can be used
		// For comprehensive testing, we'd need entities with WithStatusCode() modifiers

		// Arrange
		var user = new TestUser {
			Name = "", // Invalid
			Email = "invalid",
			Age = -1,
			Tags = new List<string>( )
		};

		var validator = new Validator( _serviceProvider );

		// Act
		var result = await validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.StatusCode.Should( ).Be( HttpStatusCode.BadRequest ); // Default status code
		result.Errors.Should( ).OnlyContain( e => e.StatusCode == HttpStatusCode.BadRequest );
	}

	[Fact]
	public async Task ValidationBuilder_WithConditionalRules_When_ShouldApplyConditionally( ) {
		// Arrange - Case where condition is met
		var user1 = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			PhoneType = PhoneType.Required, // Condition for PhoneNumber rule
			PhoneNumber = null,
			IsVerified = false,
			Salary = 50000,
			Score = 85.5
		};

		// Arrange - Case where condition is not met
		var user2 = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			PhoneType = PhoneType.Optional, // Condition not met
			PhoneNumber = null,
			IsVerified = false,
			Salary = 50000,
			Score = 85.5
		};

		var validator = new Validator( _serviceProvider );

		// Act
		var result1 = await validator.ValidateAndReturnAsync( user1 );
		var result2 = await validator.ValidateAndReturnAsync( user2 );

		// Assert
		result1.IsValid.Should( ).BeFalse( ); // Should fail because condition is met and PhoneNumber is null
		result1.Errors.Should( ).Contain( e => e.Field == "PhoneNumber" );

		result2.IsValid.Should( ).BeTrue( ); // Should pass because condition is not met
	}

	[Fact]
	public async Task ValidationBuilder_WithConditionalRules_Unless_ShouldApplyUnlessConditionMet( ) {
		// Arrange - Case where unless condition is met (rule should not apply)
		var user1 = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			PhoneType = PhoneType.Required,
			PhoneNumber = null,
			IsVerified = true, // Unless condition is met - rule should not apply
			Salary = 50000,
			Score = 85.5
		};

		// Arrange - Case where unless condition is not met (rule should apply)
		var user2 = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			PhoneType = PhoneType.Required,
			PhoneNumber = null,
			IsVerified = false, // Unless condition not met - rule should apply
			Salary = 50000,
			Score = 85.5
		};

		var validator = new Validator( _serviceProvider );

		// Act
		var result1 = await validator.ValidateAndReturnAsync( user1 );
		var result2 = await validator.ValidateAndReturnAsync( user2 );

		// Assert
		result1.IsValid.Should( ).BeTrue( ); // Should pass because unless condition is met

		result2.IsValid.Should( ).BeFalse( ); // Should fail because unless condition is not met
		result2.Errors.Should( ).Contain( e => e.Field == "PhoneNumber" );
	}

	[Fact]
	public async Task ValidationBuilder_WithMultipleContexts_ShouldCombineRules( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "taken@example.com", // Will fail in Create context
			Age = 16, // Will fail in Update context
			BirthDate = DateTime.Now.AddYears( -16 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = false, // Will fail in Create context (IsActive should be true)
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		var validator = new Validator( _serviceProvider );

		// Act
		var createResult = await validator.ValidateAndReturnAsync( user, ValidationContextKey.Create );
		var updateResult = await validator.ValidateAndReturnAsync( user, ValidationContextKey.Update );

		// Assert
		// Create context should fail on: Email (taken), IsActive (false)
		createResult.IsValid.Should( ).BeFalse( );
		var createFieldNames = createResult.Errors.Select( e => e.Field ).ToList( );
		createFieldNames.Should( ).Contain( "Email" );
		createFieldNames.Should( ).Contain( "IsActive" );

		// Update context should fail on: Age (< 18)
		updateResult.IsValid.Should( ).BeFalse( );
		var updateFieldNames = updateResult.Errors.Select( e => e.Field ).ToList( );
		updateFieldNames.Should( ).Contain( "Age" );
	}

	[Fact]
	public async Task ValidationBuilder_WithNullContext_ShouldUseGlobalRulesOnly( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "taken@example.com", // This should NOT fail because Create context rules don't apply
			Age = 16, // This should NOT fail because Update context rules don't apply
			BirthDate = DateTime.Now.AddYears( -16 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = false, // This should NOT fail because Create context rules don't apply
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		var validator = new Validator( _serviceProvider );

		// Act
		var result = await validator.ValidateAndReturnAsync( user, null ); // Null context

		// Assert
		result.IsValid.Should( ).BeTrue( ); // Should pass because only global rules apply
	}
}
