using FluentAssertions;
using Myth.Guard;
using Myth.Guard.Test.Models;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for Guard.For() standalone validation functionality
/// </summary>
public class SentryForTests : BaseTestFixture {

	#region String Validation Tests

	[Fact]
	public async Task For_String_NotEmpty_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = "valid string";

		// Act
		var result = await Sentry.For( value ).NotEmpty( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task For_String_NotEmpty_Invalid_ShouldFail( ) {
		// Arrange
		var value = "";

		// Act
		var result = await Sentry.For( value ).NotEmpty( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).HaveCount( 1 );
		result.FirstError!.Field.Should( ).Be( "Value" );
		result.FirstError!.Message.Should( ).Be( "Value cannot be empty" );
	}

	[Fact]
	public async Task For_String_CustomPropertyName_ShouldUseCorrectPropertyName( ) {
		// Arrange
		var value = "";

		// Act
		var result = await Sentry.For( value, "Email" ).NotEmpty( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Field.Should( ).Be( "Email" );
	}

	[Fact]
	public async Task For_String_WithMessage_ShouldUseCustomMessage( ) {
		// Arrange
		var value = "";

		// Act
		var result = await Sentry.For( value )
			.NotEmpty( )
			.WithMessage( "Custom error message" )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Be( "Custom error message" );
	}

	[Fact]
	public async Task For_String_WithCode_ShouldUseCustomCode( ) {
		// Arrange
		var value = "";

		// Act
		var result = await Sentry.For( value )
			.NotEmpty( )
			.WithStatusCode( 400 )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.StatusCode.Should( ).Be( System.Net.HttpStatusCode.BadRequest );
	}

	[Fact]
	public async Task For_String_WithStatusCode_ShouldUseCustomStatusCode( ) {
		// Arrange
		var value = "";

		// Act
		var result = await Sentry.For( value )
			.NotEmpty( )
			.WithStatusCode( 422 )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.StatusCode.Should( ).Be( System.Net.HttpStatusCode.UnprocessableEntity );
	}

	[Fact]
	public async Task For_String_MinimumLength_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = "valid";

		// Act
		var result = await Sentry.For( value ).MinimumLength( 3 ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_String_MinimumLength_Invalid_ShouldFail( ) {
		// Arrange
		var value = "hi";

		// Act
		var result = await Sentry.For( value ).MinimumLength( 3 ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Contain( "3" );
	}

	[Fact]
	public async Task For_String_Email_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = "test@example.com";

		// Act
		var result = await Sentry.For( value ).Email( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_String_Email_Invalid_ShouldFail( ) {
		// Arrange
		var value = "not-an-email";

		// Act
		var result = await Sentry.For( value ).Email( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Contain( "email" );
	}

	[Fact]
	public async Task For_String_Matches_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = "test123";

		// Act
		var result = await Sentry.For( value ).Matches( @"^[a-z]+\d+$" ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_String_Matches_Invalid_ShouldFail( ) {
		// Arrange
		var value = "Test123";

		// Act
		var result = await Sentry.For( value ).Matches( @"^[a-z]+\d+$" ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Contain( "pattern" );
	}

	#endregion

	#region Numeric Validation Tests

	[Fact]
	public async Task For_Int_GreaterThan_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = 10;

		// Act
		var result = await Sentry.For( value ).GreaterThan( 5 ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Int_GreaterThan_Invalid_ShouldFail( ) {
		// Arrange
		var value = 3;

		// Act
		var result = await Sentry.For( value ).GreaterThan( 5 ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Contain( "greater than 5" );
	}

	[Fact]
	public async Task For_Int_Between_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = 5;

		// Act
		var result = await Sentry.For( value ).Between( 1, 10 ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Int_Between_Invalid_ShouldFail( ) {
		// Arrange
		var value = 15;

		// Act
		var result = await Sentry.For( value ).Between( 1, 10 ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Contain( "between" );
	}

	[Fact]
	public async Task For_Decimal_LessThan_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = 4.5m;

		// Act
		var result = await Sentry.For( value ).LessThan( 5.0m ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Decimal_LessThan_Invalid_ShouldFail( ) {
		// Arrange
		var value = 6.5m;

		// Act
		var result = await Sentry.For( value ).LessThan( 5.0m ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Contain( "less than 5" );
	}

	#endregion

	#region Null Validation Tests

	[Fact]
	public async Task For_Object_NotNull_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = "not null";

		// Act
		var result = await Sentry.For( value ).NotNull( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Object_NotNull_Invalid_ShouldFail( ) {
		// Arrange
		string? value = null;

		// Act
		var result = await Sentry.For( value ).NotNull( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Be( "Value cannot be null" );
	}

	[Fact]
	public async Task For_Object_Null_Valid_ShouldBeSuccessful( ) {
		// Arrange
		string? value = null;

		// Act
		var result = await Sentry.For( value ).Null( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Object_Null_Invalid_ShouldFail( ) {
		// Arrange
		var value = "not null";

		// Act
		var result = await Sentry.For( value ).Null( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Be( "Value must be null" );
	}

	#endregion

	#region Custom Validation Tests

	[Fact]
	public async Task For_Respect_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = 10;

		// Act
		var result = await Sentry.For( value )
			.Respect( x => x % 2 == 0 )
			.WithMessage( "Value must be even" )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Respect_Invalid_ShouldFail( ) {
		// Arrange
		var value = 11;

		// Act
		var result = await Sentry.For( value )
			.Respect( x => x % 2 == 0 )
			.WithMessage( "Value must be even" )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Be( "Value must be even" );
	}

	[Fact]
	public async Task For_RespectAsync_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = "test@example.com";

		// Act
		var result = await Sentry.For( value )
			.RespectAsync( async ( email, ct, sp ) => {
				// Simulate async validation (e.g., checking if email exists in database)
				await Task.Delay( 1, ct );
				return email.Contains( "@" );
			} )
			.WithMessage( "Email format is invalid" )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_RespectAsync_Invalid_ShouldFail( ) {
		// Arrange
		var value = "invalid-email";

		// Act
		var result = await Sentry.For( value )
			.RespectAsync( async ( email, ct, sp ) => {
				await Task.Delay( 1, ct );
				return email.Contains( "@" );
			} )
			.WithMessage( "Email format is invalid" )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Be( "Email format is invalid" );
	}

	#endregion

	#region Enum Validation Tests

	[Fact]
	public async Task For_Enum_IsValidEnumValue_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = TestStatus.Active;

		// Act
		var result = await Sentry.For( value ).IsValidEnumValue( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Enum_IsValidEnumValue_Invalid_ShouldFail( ) {
		// Arrange
		var value = ( TestStatus )999;

		// Act
		var result = await Sentry.For( value ).IsValidEnumValue( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Contain( "valid" ).And.Contain( "enum" );
	}

	[Fact]
	public async Task For_Enum_IsValidEnumValue_WithOptions_ShouldIncludeOptions( ) {
		// Arrange
		var value = ( TestStatus )999;

		// Act
		var result = await Sentry.For( value )
			.IsValidEnumValue( )
			.WithOptions( )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Options.Should( ).NotBeNull( );
		result.FirstError!.Options.Should( ).Contain( "10: Active" );
		result.FirstError!.Options.Should( ).Contain( "20: Inactive" );
	}

	#endregion

	#region Constant Validation Tests

	[Fact]
	public async Task For_Constant_ExistsInConstant_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = 1; // Low priority

		// Act
		var result = await Sentry.For( value )
			.ExistsInConstant<TestPriorityLevels, int>( )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Constant_ExistsInConstant_Invalid_ShouldFail( ) {
		// Arrange
		var value = 999; // Invalid priority

		// Act
		var result = await Sentry.For( value )
			.ExistsInConstant<TestPriorityLevels, int>( )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Contain( "not valid" );
	}

	[Fact]
	public async Task For_Constant_ExistsInConstant_WithOptions_ShouldIncludeOptions( ) {
		// Arrange
		var value = 999; // Invalid priority

		// Act
		var result = await Sentry.For( value )
			.ExistsInConstant<TestPriorityLevels, int>( )
			.WithOptions( )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Options.Should( ).NotBeNull( );
		result.FirstError!.Options.Should( ).Contain( "1: Low" );
		result.FirstError!.Options.Should( ).Contain( "3: Medium" );
		result.FirstError!.Options.Should( ).Contain( "5: High" );
		result.FirstError!.Options.Should( ).Contain( "10: Critical" );
	}

	#endregion

	#region Collection Validation Tests

	[Fact]
	public async Task For_Collection_NotEmpty_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = new[ ] { 1, 2, 3 };

		// Act
		var result = await Sentry.For( value ).NotEmpty( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Collection_NotEmpty_Invalid_ShouldFail( ) {
		// Arrange
		var value = Array.Empty<int>( );

		// Act
		var result = await Sentry.For( value ).NotEmpty( ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Contain( "empty" );
	}

	[Fact]
	public async Task For_Collection_CountBetween_Valid_ShouldBeSuccessful( ) {
		// Arrange
		var value = new[ ] { 1, 2, 3 };

		// Act
		var result = await Sentry.For( value ).CountBetween( 1, 5 ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Collection_CountBetween_Invalid_ShouldFail( ) {
		// Arrange
		var value = new[ ] { 1, 2, 3, 4, 5, 6 };

		// Act
		var result = await Sentry.For( value ).CountBetween( 1, 3 ).ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Message.Should( ).Contain( "between" );
	}

	#endregion

	#region Multiple Rules Tests

	[Fact]
	public async Task For_MultipleRules_AllValid_ShouldBeSuccessful( ) {
		// Arrange
		var value = "test@example.com";

		// Act
		var result = await Sentry.For( value )
			.NotNull( )
			.NotEmpty( )
			.MinimumLength( 5 )
			.Email( )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task For_MultipleRules_OneInvalid_ShouldFail( ) {
		// Arrange
		var value = "ab"; // Too short

		// Act
		var result = await Sentry.For( value )
			.NotNull( )
			.NotEmpty( )
			.MinimumLength( 5 )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).HaveCount( 1 );
		result.FirstError!.Message.Should( ).Contain( "5" );
	}

	[Fact]
	public async Task For_MultipleRules_MultipleInvalid_ShouldReturnAllErrors( ) {
		// Arrange
		var value = "";

		// Act
		var result = await Sentry.For( value )
			.NotEmpty( )
			.MinimumLength( 5 )
			.Email( )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).HaveCount( 3 ); // All three rules should fail
	}

	#endregion

	#region ValidateAndThrowAsync Tests

	[Fact]
	public async Task ValidateAndThrowAsync_Valid_ShouldNotThrow( ) {
		// Arrange
		var value = "valid@example.com";

		// Act & Assert
		await Sentry.For( value ).Email( ).ValidateAndThrowAsync( );
		// Should complete without throwing
	}

	[Fact]
	public async Task ValidateAndThrowAsync_Invalid_ShouldThrowValidationException( ) {
		// Arrange
		var value = "invalid-email";

		// Act & Assert
		var exception = await Assert.ThrowsAsync<Myth.Exceptions.ValidationException>(
			async ( ) => await Sentry.For( value ).Email( ).ValidateAndThrowAsync( )
		);

		exception.ValidationResult.IsValid.Should( ).BeFalse( );
		exception.ValidationResult.Errors.Should( ).HaveCount( 1 );
	}

	#endregion

	#region WithOptions Tests

	[Fact]
	public async Task For_WithCustomOptions_ShouldUseProvidedOptions( ) {
		// Arrange
		var value = "invalid";
		var customOptions = new[ ] { "option1", "option2", "option3" };

		// Act
		var result = await Sentry.For( value )
			.Respect( x => false ) // Always fail
			.WithOptions( customOptions )
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.FirstError!.Options.Should( ).NotBeNull( );
		result.FirstError!.Options.Should( ).BeEquivalentTo( customOptions );
	}

	#endregion

	#region Conditional Validation Tests

	[Fact]
	public async Task For_When_ConditionTrue_ShouldApplyRule( ) {
		// Arrange
		var value = "";

		// Act
		var result = await Sentry.For( value )
			.NotEmpty( )
			.When( x => true ) // Always apply
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
	}

	[Fact]
	public async Task For_When_ConditionFalse_ShouldSkipRule( ) {
		// Arrange
		var value = "";

		// Act
		var result = await Sentry.For( value )
			.NotEmpty( )
			.When( x => false ) // Never apply
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task For_Unless_ConditionFalse_ShouldApplyRule( ) {
		// Arrange
		var value = "";

		// Act
		var result = await Sentry.For( value )
			.NotEmpty( )
			.Unless( x => false ) // Apply unless false (so apply)
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeFalse( );
	}

	[Fact]
	public async Task For_Unless_ConditionTrue_ShouldSkipRule( ) {
		// Arrange
		var value = "";

		// Act
		var result = await Sentry.For( value )
			.NotEmpty( )
			.Unless( x => true ) // Don't apply if true (so skip)
			.ValidateAsync( );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	#endregion
}
