using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Exceptions;
using Myth.Guard.Test.Models;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for the main Validator class
/// </summary>
public class ValidatorTests {
	private readonly IServiceProvider _serviceProvider;
	private readonly Validator _sut; // System Under Test

	public ValidatorTests( ) {
		var services = new ServiceCollection( );
		services.AddSingleton<ITestUserService, TestUserService>( );
		_serviceProvider = services.BuildServiceProvider( );
		_sut = new Validator( _serviceProvider );
	}

	[Fact]
	public async Task ValidateAsync_WithValidEntity_ShouldNotThrow( ) {
		// Arrange
		var user = new TestUser {
			Name = "John",
			Email = "john@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "developer", "remote" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var act = async ( ) => await _sut.ValidateAsync( user );

		// Assert
		await act.Should( ).NotThrowAsync( );
	}

	[Fact]
	public async Task ValidateAsync_WithInvalidEntity_ShouldThrowValidationException( ) {
		// Arrange
		var user = new TestUser {
			Name = "", // Invalid: empty name
			Email = "invalid-email", // Invalid: not a valid email
			Age = -5, // Invalid: negative age
			BirthDate = DateTime.Now.AddYears( 5 ), // Invalid: future birth date
			Tags = new List<string>( ), // Invalid: empty tags
			Salary = -1000, // Invalid: negative salary
			Score = 150 // Invalid: score > 100
		};

		// Act
		var act = async ( ) => await _sut.ValidateAsync( user );

		// Assert
		var exception = await act.Should( ).ThrowAsync<ValidationException>( );
		exception.Which.ValidationResult.Should( ).NotBeNull( );
		exception.Which.ValidationResult.IsValid.Should( ).BeFalse( );
		exception.Which.ValidationResult.Errors.Should( ).NotBeEmpty( );
		exception.Which.ValidationResult.Errors.Should( ).HaveCountGreaterThan( 5 );
	}

	[Fact]
	public async Task ValidateAndReturnAsync_WithValidEntity_ShouldReturnSuccessResult( ) {
		// Arrange
		var user = new TestUser {
			Name = "Jane",
			Email = "jane@example.com",
			Age = 30,
			BirthDate = DateTime.Now.AddYears( -30 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "manager" },
			IsActive = true,
			Role = UserRole.Admin,
			Salary = 75000,
			Score = 92.3
		};

		// Act
		var result = await _sut.ValidateAndReturnAsync( user );

		// Assert
		result.Should( ).NotBeNull( );
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
		result.StatusCode.Should( ).Be( HttpStatusCode.OK );
	}

	[Fact]
	public async Task ValidateAndReturnAsync_WithInvalidEntity_ShouldReturnFailureResult( ) {
		// Arrange
		var user = new TestUser {
			Name = "A", // Too short
			Email = "notanemail",
			Age = 200, // Too old
			Tags = new List<string>( )
		};

		// Act
		var result = await _sut.ValidateAndReturnAsync( user );

		// Assert
		result.Should( ).NotBeNull( );
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).NotBeEmpty( );
		result.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );

		// Check specific error fields
		var fieldNames = result.Errors.Select( e => e.Field ).ToList( );
		fieldNames.Should( ).Contain( "Name" );
		fieldNames.Should( ).Contain( "Email" );
		fieldNames.Should( ).Contain( "Age" );
		fieldNames.Should( ).Contain( "Tags" );
	}

	[Fact]
	public async Task ValidateAsync_WithContextSpecificRules_ShouldApplyCorrectRules( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 16, // Valid for general context, invalid for Update context
			BirthDate = DateTime.Now.AddYears( -16 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "young" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 1000,
			Score = 50
		};

		// Act & Assert - Should pass with default context
		var act1 = async ( ) => await _sut.ValidateAsync( user );
		await act1.Should( ).NotThrowAsync( );

		// Act & Assert - Should fail with Update context (age < 18)
		var act2 = async ( ) => await _sut.ValidateAsync( user, ValidationContextKey.Update );
		await act2.Should( ).ThrowAsync<ValidationException>( );
	}

	[Fact]
	public async Task ValidateAsync_WithCreateContext_ShouldCheckEmailAvailability( ) {
		// Arrange
		var user = new TestUser {
			Name = "NewUser",
			Email = "taken@example.com", // This email is "taken" in TestUserService
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "new" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 40000,
			Score = 70
		};

		// Act
		var act = async ( ) => await _sut.ValidateAsync( user, ValidationContextKey.Create );

		// Assert
		var exception = await act.Should( ).ThrowAsync<ValidationException>( );
		exception.Which.ValidationResult.Errors
			.Should( ).Contain( e => e.Field == "Email" && e.Code == "EMAIL_EXISTS" );
	}

	[Fact]
	public async Task ValidateAsync_WithConditionalRules_ShouldApplyWhenConditionMet( ) {
		// Arrange
		var user = new TestUser {
			Name = "ConditionalTest",
			Email = "conditional@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			PhoneType = PhoneType.Required,
			PhoneNumber = null, // Should be required when PhoneType is Required
			IsVerified = false,
			Salary = 30000,
			Score = 60
		};

		// Act
		var act = async ( ) => await _sut.ValidateAsync( user );

		// Assert
		var exception = await act.Should( ).ThrowAsync<ValidationException>( );
		exception.Which.ValidationResult.Errors
			.Should( ).Contain( e => e.Field == "PhoneNumber" );
	}

	[Fact]
	public async Task ValidateAsync_WithConditionalRules_ShouldSkipWhenUnlessConditionMet( ) {
		// Arrange
		var user = new TestUser {
			Name = "ConditionalTest",
			Email = "conditional@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			PhoneType = PhoneType.Required,
			PhoneNumber = null, // Should NOT be required when IsVerified is true (Unless condition)
			IsVerified = true,
			Salary = 30000,
			Score = 60
		};

		// Act
		var act = async ( ) => await _sut.ValidateAsync( user );

		// Assert
		await act.Should( ).NotThrowAsync( );
	}

	[Fact]
	public async Task ValidateAsync_WithNullEntity_ShouldThrowArgumentNullException( ) {
		// Act
		var act = async ( ) => await _sut.ValidateAsync<TestUser>( null! );

		// Assert
		await act.Should( ).ThrowAsync<ArgumentNullException>( );
	}

	[Fact]
	public async Task ValidateAsync_WithCancellation_ShouldRespectCancellationToken( ) {
		// Arrange
		var user = new TestUser {
			Name = "CancelTest",
			Email = "cancel@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 30000,
			Score = 60
		};

		using var cts = new CancellationTokenSource( );
		cts.Cancel( );

		// Act
		var act = async ( ) => await _sut.ValidateAsync( user, cancellationToken: cts.Token );

		// Assert
		await act.Should( ).ThrowAsync<OperationCanceledException>( );
	}
}
