using FluentAssertions;
using Myth.Guard;
using Myth.Guard.Test;
using Myth.Validation;

namespace Test;

/// <summary>
/// Tests for multi-validation functionality using Validate.All() and extension methods
/// </summary>
public class MultiValidationTests : BaseTestFixture {

	#region Basic Multi-Validation Tests

	[Fact]
	public async Task ValidateAll_AllValid_ShouldBeSuccessful( ) {
		// Arrange
		var email = "test@example.com";
		var age = 25;
		var name = "John Doe";

		// Act - Traditional approach
		var result = await Myth.Validation.Validate.AllAsync( [
			Sentry.For( email, "Email" ).NotEmpty( ).Email( ),
			Sentry.For( age, "Age" ).GreaterThan( 0 ).LessThan( 150 ),
			Sentry.For( name, "Name" ).NotEmpty( ).MinimumLength( 2 )
		] );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
		result.ErrorCount.Should( ).Be( 0 );
		result.FieldsWithErrorsCount.Should( ).Be( 0 );
	}

	[Fact]
	public async Task ValidateAll_SomeInvalid_ShouldReturnAggregatedErrors( ) {
		// Arrange
		var email = "invalid-email";
		var age = -5;
		var name = "";

		// Act
		var result = await Myth.Validation.Validate.AllAsync( [
			Sentry.For( email, "Email" ).NotEmpty( ).Email( ),
			Sentry.For( age, "Age" ).GreaterThan( 0 ).LessThan( 150 ),
			Sentry.For( name, "Name" ).NotEmpty( ).MinimumLength( 2 )
		] );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.ErrorCount.Should( ).Be( 4 ); // Multiple errors can occur per field
		result.FieldsWithErrorsCount.Should( ).Be( 3 );

		// Check specific field errors
		result.HasErrorsForField( "Email" ).Should( ).BeTrue( );
		result.HasErrorsForField( "Age" ).Should( ).BeTrue( );
		result.HasErrorsForField( "Name" ).Should( ).BeTrue( );

		result.GetErrorsForField( "Email" ).Should( ).HaveCountGreaterThan( 0 );
		result.GetErrorsForField( "Age" ).Should( ).HaveCount( 1 );
		result.GetErrorsForField( "Name" ).Should( ).HaveCountGreaterThan( 0 );
	}

	[Fact]
	public async Task ValidateAll_Empty_ShouldBeSuccessful( ) {
		// Act
		var result = await Myth.Validation.Validate.AllAsync( [ ] );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
	}

	#endregion

	#region Fluent Builder API Tests

	[Fact]
	public async Task ValidateAllBuilder_AllValid_ShouldBeSuccessful( ) {
		// Arrange
		var email = "test@example.com";
		var age = 25;
		var name = "John Doe";

		// Act - Using fluent builder API
		var result = await Myth.Validation.Validate.All( )
			.Add( Sentry.For( email, "Email" ).NotEmpty( ).Email( ) )
			.Add( Sentry.For( age, "Age" ).GreaterThan( 0 ).LessThan( 150 ) )
			.Add( Sentry.For( name, "Name" ).NotEmpty( ).MinimumLength( 2 ) )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task ValidateAllBuilder_WithExtensionMethods_ShouldWork( ) {
		// Arrange
		var email = "test@example.com";
		var age = 25;
		var name = "John Doe";
		var score = 85.5m;

		// Act - Using extension methods for common scenarios
		var result = await Myth.Validation.Validate.All( )
			.ValidateEmail( email )
			.ValidateRange( age, "Age", 0, 150 )
			.ValidateRequired( name, "Name" )
			.ValidateRange( score, "Score", 0m, 100m )
			.ValidateString( name, "Name", s => s.MinimumLength( 2 ).MaximumLength( 100 ) )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task ValidateAllBuilder_InvalidData_ShouldReturnErrors( ) {
		// Arrange
		var email = "invalid";
		var age = -1;
		var name = "";

		// Act
		var result = await Myth.Validation.Validate.All( )
			.ValidateEmail( email )
			.ValidateRange( age, "Age", 0, 150 )
			.ValidateRequired( name, "Name" )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.ErrorCount.Should( ).Be( 3 );

		result.HasErrorsForField( "Email" ).Should( ).BeTrue( );
		result.HasErrorsForField( "Age" ).Should( ).BeTrue( );
		result.HasErrorsForField( "Name" ).Should( ).BeTrue( );
	}

	#endregion

	#region Extension Array Method Tests

	[Fact]
	public async Task ValidateAllArray_AllValid_ShouldBeSuccessful( ) {
		// Arrange & Act - Using extension method on array
		var validations = new IStandaloneValidationBuilder[ ] {
			Sentry.For( "test@example.com", "Email" ).NotEmpty( ).Email( ),
			Sentry.For( 25, "Age" ).GreaterThan( 0 ).LessThan( 150 ),
			Sentry.For( "John", "Name" ).NotEmpty( ).MinimumLength( 2 )
		};
		var result = await validations.ValidateAllAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task ValidateAllArrayAndThrow_Invalid_ShouldThrow( ) {
		// Arrange & Act & Assert
		var validations = new IStandaloneValidationBuilder[ ] {
			Sentry.For( "invalid-email", "Email" ).Email( ),
			Sentry.For( -1, "Age" ).GreaterThan( 0 )
		};
		var exception = await Assert.ThrowsAsync<Myth.Exceptions.ValidationException>( async ( ) =>
			await validations.ValidateAllAndThrowAsync( )
		);

		exception.ValidationResult.IsValid.Should( ).BeFalse( );
		exception.ValidationResult.Errors.Should( ).HaveCount( 2 );
	}

	#endregion

	#region Complex Validation Scenarios

	[Fact]
	public async Task ValidateAll_ComplexUserScenario_ShouldWork( ) {
		// Arrange - Simulate user registration form
		var firstName = "John";
		var lastName = "Doe";
		var email = "john.doe@example.com";
		var password = "SecurePass123!";
		var age = 25;
		var phone = "+1234567890";

		// Act
		var result = await Myth.Validation.Validate.All( )
			.ValidateRequired( firstName, "FirstName" )
			.ValidateRequired( lastName, "LastName" )
			.ValidateEmail( email )
			.ValidateString( password, "Password", p => p
				.NotEmpty( )
				.MinimumLength( 8 )
				.Matches( @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]" )
				.WithMessage( "Password must contain uppercase, lowercase, number and special character" )
			)
			.ValidateRange( age, "Age", 18, 120 )
			.ValidateString( phone, "Phone", p => p
				.NotEmpty( )
				.Matches( @"^\+?[1-9]\d{1,14}$" )
				.WithMessage( "Please enter a valid phone number" )
			)
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task ValidateAll_WithCustomValidations_ShouldWork( ) {
		// Arrange
		var username = "john_doe";
		var confirmPassword = "password123";
		var password = "different_password";

		// Act
		var result = await Myth.Validation.Validate.All( )
			.ValidateValue( username, "Username", u => u
				.NotEmpty( )
				.MinimumLength( 3 )
				.Matches( @"^[a-zA-Z0-9_]+$" )
				.WithMessage( "Username can only contain letters, numbers and underscores" )
			)
			.ValidateValue( password, "Password", p => p.NotEmpty( ).MinimumLength( 8 ) )
			.ValidateValue( confirmPassword, "ConfirmPassword", c => c
				.NotEmpty( )
				.Respect( x => x == password )
				.WithMessage( "Passwords do not match" )
			)
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.HasErrorsForField( "ConfirmPassword" ).Should( ).BeTrue( );
		result.GetErrorsForField( "ConfirmPassword" ).First( ).Message.Should( ).Be( "Passwords do not match" );
	}

	#endregion

	#region Async Validation Tests

	[Fact]
	public async Task ValidateAll_WithAsyncValidation_ShouldWork( ) {
		// Arrange
		var email = "test@example.com";
		var username = "john_doe";

		// Act
		var result = await Myth.Validation.Validate.All( )
			.ValidateValue( email, "Email", e => e
				.NotEmpty( )
				.Email( )
				.RespectAsync( async ( email, ct, sp ) => {
					// Simulate async email availability check
					await Task.Delay( 10, ct );
					return !email.Contains( "taken" );
				} )
				.WithMessage( "Email is already taken" )
			)
			.ValidateValue( username, "Username", u => u
				.NotEmpty( )
				.RespectAsync( async ( user, ct, sp ) => {
					// Simulate async username availability check
					await Task.Delay( 10, ct );
					return !user.Contains( "admin" );
				} )
				.WithMessage( "Username is not available" )
			)
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task ValidateAll_WithFailingAsyncValidation_ShouldReturnErrors( ) {
		// Arrange
		var email = "taken@example.com";
		var username = "admin_user";

		// Act
		var result = await Myth.Validation.Validate.All( )
			.ValidateValue( email, "Email", e => e
				.Email( )
				.RespectAsync( async ( email, ct, sp ) => {
					await Task.Delay( 1, ct );
					return !email.Contains( "taken" );
				} )
				.WithMessage( "Email is already taken" )
			)
			.ValidateValue( username, "Username", u => u
				.NotEmpty( )
				.RespectAsync( async ( user, ct, sp ) => {
					await Task.Delay( 1, ct );
					return !user.Contains( "admin" );
				} )
				.WithMessage( "Username is not available" )
			)
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.ErrorCount.Should( ).Be( 2 );
		result.GetErrorsForField( "Email" ).First( ).Message.Should( ).Be( "Email is already taken" );
		result.GetErrorsForField( "Username" ).First( ).Message.Should( ).Be( "Username is not available" );
	}

	#endregion

	#region Enum and Constant Validation Tests

	// TODO: Fix enum validation tests after resolving generic constraint issues
	// [Fact]
	// public async Task ValidateAll_WithEnumAndConstants_ShouldWork( ) {
	// 	// Arrange
	// 	var status = TestStatus.Active;
	// 	var priority = 1; // Low priority from TestPriorityLevels

	// 	// Act
	// 	var result = await Validate.All( )
	// 		.ValidateValue( status, "Status", s => s.IsValidEnumValue( ).WithOptions( ) )
	// 		.ValidateValue( priority, "Priority", p => p.ExistsInConstant<TestPriorityLevels, int>( ).WithOptions( ) )
	// 		.ValidateAsync( );

	// 	// Assert
	// 	result.IsValid.Should( ).BeTrue( );
	// }

	// [Fact]
	// public async Task ValidateAll_WithInvalidEnumAndConstants_ShouldReturnOptionsInErrors( ) {
	// 	// Arrange
	// 	var status = ( TestStatus )999;
	// 	var priority = 999; // Invalid priority

	// 	// Act
	// 	var result = await Validate.All( )
	// 		.ValidateValue( status, "Status", s => s.IsValidEnumValue( ).WithOptions( ) )
	// 		.ValidateValue( priority, "Priority", p => p.ExistsInConstant<TestPriorityLevels, int>( ).WithOptions( ) )
	// 		.ValidateAsync( );

	// 	// Assert
	// 	result.IsValid.Should( ).BeFalse( );
	// 	result.ErrorCount.Should( ).Be( 2 );

	// 	var statusError = result.GetErrorsForField( "Status" ).First( );
	// 	statusError.Options.Should( ).NotBeNull( );
	// 	statusError.Options.Should( ).Contain( "10: Active" );

	// 	var priorityError = result.GetErrorsForField( "Priority" ).First( );
	// 	priorityError.Options.Should( ).NotBeNull( );
	// 	priorityError.Options.Should( ).Contain( "1: Low" );
	// }

	#endregion

	#region Error Grouping and Analysis Tests

	[Fact]
	public async Task MultiValidationResult_ErrorsByField_ShouldGroupCorrectly( ) {
		// Arrange & Act
		var result = await Myth.Validation.Validate.All( )
			.ValidateString( "", "Name", s => s.NotEmpty( ).MinimumLength( 2 ) ) // 2 errors for same field
			.ValidateString( "short", "Name", s => s.MinimumLength( 10 ) ) // 1 more error for same field
			.ValidateRange( -1, "Age", 0, 150 ) // 1 error for different field
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.ErrorCount.Should( ).Be( 4 ); // Total errors (2 for "" + 1 for "short" + 1 for -1)
		result.FieldsWithErrorsCount.Should( ).Be( 2 ); // Unique fields with errors

		result.ErrorsByField.Should( ).ContainKey( "Name" );
		result.ErrorsByField.Should( ).ContainKey( "Age" );

		result.GetErrorsForField( "Name" ).Should( ).HaveCount( 3 ); // Multiple validation calls on same field
		result.GetErrorsForField( "Age" ).Should( ).HaveCount( 1 );
		result.GetErrorsForField( "NonExistent" ).Should( ).BeEmpty( );
	}

	[Fact]
	public async Task MultiValidationResult_ErrorMessage_ShouldConcatenateAllMessages( ) {
		// Arrange & Act
		var result = await Myth.Validation.Validate.All( )
			.ValidateString( "", "Name", s => s.NotEmpty( ).WithMessage( "Name is required" ) )
			.ValidateRange( -1, "Age", 0, 150 )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.ErrorMessage.Should( ).Contain( "Name is required" );
		result.ErrorMessage.Should( ).Contain( Environment.NewLine );
	}

	#endregion

	#region Validate.AllAndThrowAsync Tests

	[Fact]
	public async Task ValidateAllAndThrow_Valid_ShouldNotThrow( ) {
		// Arrange & Act & Assert (should not throw)
		await Myth.Validation.Validate.AllAndThrowAsync( [
			Sentry.For( "test@example.com", "Email" ).Email( ),
			Sentry.For( 25, "Age" ).GreaterThan( 0 )
		] );
		// Test passes if no exception is thrown
	}

	[Fact]
	public async Task ValidateAllAndThrow_Invalid_ShouldThrowWithAllErrors( ) {
		// Arrange & Act & Assert
		var exception = await Assert.ThrowsAsync<Myth.Exceptions.ValidationException>( async ( ) =>
			await Myth.Validation.Validate.AllAndThrowAsync( [
				Sentry.For( "invalid-email", "Email" ).Email( ),
				Sentry.For( -1, "Age" ).GreaterThan( 0 )
			] )
		);

		exception.ValidationResult.IsValid.Should( ).BeFalse( );
		exception.ValidationResult.Errors.Should( ).HaveCount( 2 );
	}

	[Fact]
	public async Task ValidateAllBuilder_AndThrow_Invalid_ShouldThrow( ) {
		// Arrange & Act & Assert
		var exception = await Assert.ThrowsAsync<Myth.Exceptions.ValidationException>( async ( ) =>
			await Myth.Validation.Validate.All( )
				.ValidateEmail( "invalid" )
				.ValidateRange( -1, "Age", 0, 150 )
				.ValidateAndThrowAsync( )
		);

		exception.ValidationResult.Errors.Should( ).HaveCount( 2 );
	}

	#endregion
}
