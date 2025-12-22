using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Builder;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Guard.Test.Models;
using Myth.Interfaces;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for Respect and RespectAsync methods with entity access functionality
/// </summary>
public class RespectWithEntityAccessTests : BaseTestFixture {

	protected override void ConfigureServices( IServiceCollection services ) {
		services.AddSingleton<ILoginService, TestLoginService>( );
		services.AddSingleton<IUserRepository, TestUserRepository>( );
	}

	#region Respect<TEntity> Tests

	[Fact]
	public async Task Respect_WithEntityAccess_WhenValidationPasses_ShouldSucceed( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var loginDto = new LoginDto {
			Email = "admin@test.com",
			Password = "admin123",
			IsActive = true
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( loginDto );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task Respect_WithEntityAccess_WhenValidationFails_ShouldFail( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var loginDto = new LoginDto {
			Email = "", // Empty email for active user should fail
			Password = "password123",
			IsActive = true // This will cause validation to fail because email is empty
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( loginDto );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		var emailError = result.Errors.FirstOrDefault( e => e.Field == "Email" );
		emailError.Should( ).NotBeNull( );
		emailError!.Message.Should( ).Be( "Email is required for active users" );
	}

	[Fact]
	public async Task Respect_WithEntityAccess_WhenEntityIsWrongType_ShouldHandleGracefully( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var simpleEntity = new SimpleEntity { Value = "Test" };

		// Act & Assert - Should not throw casting exception
		var exception = await Record.ExceptionAsync( async ( ) => {
			await validator.ValidateAndReturnAsync( simpleEntity );
		} );

		exception.Should( ).BeNull( );
	}

	#endregion

	#region RespectAsync<TEntity> Tests

	[Fact]
	public async Task RespectAsync_WithEntityAccess_WhenCredentialsValid_ShouldSucceed( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var loginDto = new LoginDto {
			Email = "user@test.com",
			Password = "correct_password",
			IsActive = true
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( loginDto );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task RespectAsync_WithEntityAccess_WhenCredentialsInvalid_ShouldFail( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var loginDto = new LoginDto {
			Email = "user@test.com",
			Password = "wrong_password",
			IsActive = true
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( loginDto );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		var passwordError = result.Errors.FirstOrDefault( e => e.Field == "Password" );
		passwordError.Should( ).NotBeNull( );
		passwordError!.Message.Should( ).Be( "Invalid email and password combination" );
	}

	[Fact]
	public async Task RespectAsync_WithEntityAccess_WhenServiceThrows_ShouldHandleException( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var loginDto = new LoginDto {
			Email = "error@test.com", // This will trigger service exception
			Password = "password123",
			IsActive = true
		};

		// Act & Assert
		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			( ) => validator.ValidateAsync( loginDto ) );

		exception.Message.Should( ).Be( "Service error occurred" );
	}

	#endregion

	#region Integration Tests

	[Fact]
	public async Task CompleteValidation_WithMultipleEntityRules_ShouldValidateCorrectly( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var userProfile = new UserProfileDto {
			Email = "admin@test.com",
			Name = "Admin User",
			Role = "Admin",
			IsActive = true,
			MinimumAge = 21
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( userProfile );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task CompleteValidation_WithFailingEntityRules_ShouldReturnAllErrors( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var userProfile = new UserProfileDto {
			Email = "",
			Name = "AB", // Too short for User role (requires 3+)
			Role = "User",
			IsActive = true, // Email required for active users
			MinimumAge = 16 // Below minimum for User role (requires 18+)
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( userProfile );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).HaveCountGreaterThan( 1 );

		var emailError = result.Errors.FirstOrDefault( e => e.Field == "Email" );
		emailError.Should( ).NotBeNull( );

		var nameError = result.Errors.FirstOrDefault( e => e.Field == "Name" );
		nameError.Should( ).NotBeNull( );

		var ageError = result.Errors.FirstOrDefault( e => e.Field == "MinimumAge" );
		ageError.Should( ).NotBeNull( );
	}

	#endregion

	#region Backwards Compatibility Tests

	[Fact]
	public async Task ExistingRespectMethods_ShouldContinueToWork( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var simpleDto = new SimpleValidationDto {
			Value = 50, // Valid value between 0 and 100
			AsyncValue = "valid_test" // Valid value that starts with "valid"
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( simpleDto );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task ExistingRespectAsyncMethods_ShouldContinueToWork( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var simpleDto = new SimpleValidationDto { AsyncValue = "valid_async" };

		// Act
		var result = await validator.ValidateAndReturnAsync( simpleDto );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	#endregion
}

#region Test Models

/// <summary>
/// Test model for login validation with entity access
/// </summary>
public class LoginDto : IValidatable<LoginDto> {
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public bool IsActive { get; set; }

	public void Validate( ValidationBuilder<LoginDto> builder, ValidationContextKey? context = null ) {
		// Email validation with entity access - more specific validation first
		builder.For( Email, x => x
			.Respect<LoginDto>( ( email, login ) => !login.IsActive || !string.IsNullOrEmpty( email ) )
			.WithMessage( "Email is required for active users" ) );

		// Basic email validation
		builder.For( Email, x => x
			.Email( ) );

		// Password validation with async entity access and external service
		builder.For( Password, x => x
			.NotEmpty( )
			.MinimumLength( 6 )
			.RespectAsync<LoginDto>( async ( password, login, ct, sp ) => {
				var loginService = sp.GetRequiredService<ILoginService>( );
				return await loginService.ValidateCredentialsAsync( login.Email, password, ct );
			} )
			.WithMessage( "Invalid email and password combination" ) );
	}
}

/// <summary>
/// Test model for user profile validation with multiple entity rules
/// </summary>
public class UserProfileDto : IValidatable<UserProfileDto> {
	public string Email { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
	public bool IsActive { get; set; }
	public int MinimumAge { get; set; }

	public void Validate( ValidationBuilder<UserProfileDto> builder, ValidationContextKey? context = null ) {
		builder.For( Email, x => x
			.NotEmpty( ).When<string, UserProfileDto>( u => u.IsActive )
			.Email( ) );

		builder.For( Name, x => x
			.NotEmpty( )
			.Respect<UserProfileDto>( ( name, profile ) => {
				if ( profile.Role == "Admin" )
					return name.Length >= 5;
				return name.Length >= 3;
			} )
			.WithMessage( "Name length requirement not met for role" ) );

		builder.For( MinimumAge, x => x
			.GreaterThan( 0 )
			.Respect<UserProfileDto>( ( age, profile ) => {
				return profile.Role switch {
					"Admin" => age >= 21,
					"User" => age >= 18,
					_ => age >= 13
				};
			} )
			.WithMessage( "Age requirement not met for role" ) );
	}
}

/// <summary>
/// Simple test model for backwards compatibility testing
/// </summary>
public class SimpleValidationDto : IValidatable<SimpleValidationDto> {
	public int Value { get; set; }
	public string AsyncValue { get; set; } = string.Empty;

	public void Validate( ValidationBuilder<SimpleValidationDto> builder, ValidationContextKey? context = null ) {
		// Test existing Respect method
		builder.For( Value, x => x
			.Respect( value => value >= 0 && value <= 100 )
			.WithMessage( "Value must be between 0 and 100" ) );

		// Test existing RespectAsync method
		builder.For( AsyncValue, x => x
			.RespectAsync( async ( value, ct, sp ) => {
				await Task.Delay( 1, ct ); // Simulate async work
				return !string.IsNullOrEmpty( value ) && value.StartsWith( "valid" );
			} )
			.WithMessage( "Async value must start with 'valid'" ) );
	}
}

#endregion

#region Test Services

public interface ILoginService {
	Task<bool> ValidateCredentialsAsync( string email, string password, CancellationToken cancellationToken );
}

public class TestLoginService : ILoginService {
	public async Task<bool> ValidateCredentialsAsync( string email, string password, CancellationToken cancellationToken ) {
		await Task.Delay( 10, cancellationToken ); // Simulate async work

		// Simulate service exception for specific email
		if ( email == "error@test.com" ) {
			throw new InvalidOperationException( "Service error occurred" );
		}

		// Simulate credential validation
		return (email, password) switch {
			("user@test.com", "correct_password" ) => true,
			("admin@test.com", "admin123" ) => true,
			_ => false
		};
	}
}

public interface IUserRepository {
	Task<bool> AnyAsync( Func<object, bool> predicate, CancellationToken cancellationToken );
}

public class TestUserRepository : IUserRepository {
	public async Task<bool> AnyAsync( Func<object, bool> predicate, CancellationToken cancellationToken ) {
		await Task.Delay( 5, cancellationToken ); // Simulate database query
		return predicate( new { Email = "existing@test.com", Password = "hashed_password" } );
	}
}

#endregion
