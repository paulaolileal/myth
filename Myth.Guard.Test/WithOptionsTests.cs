using FluentAssertions;
using Myth.Enums;
using Myth.Guard.Test.Models;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for WithOptions functionality in validation rules
/// </summary>
public class WithOptionsTests : BaseTestFixture {
	private readonly Validator _validator;

	public WithOptionsTests( ) {
		_validator = new Validator( ServiceProvider );
	}

	[Fact]
	public async Task WithOptions_EnumValidation_ShouldIncludeOptionsInError( ) {
		// Arrange
		var entity = new TestEntityWithOptions {
			Status = ( TestStatus )999 // Invalid enum value
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		var error = result.Errors.First( );
		error.Options.Should( ).NotBeNull( );
		error.Options.Should( ).Contain( "10: Active" );
		error.Options.Should( ).Contain( "20: Inactive" );
		error.Options.Should( ).Contain( "30: Pending" );
		error.Options.Should( ).Contain( "40: Suspended" );
	}

	[Theory]
	[InlineData( OptionsType.OnlyValue, "10", "20", "30", "40" )]
	[InlineData( OptionsType.OnlyName, "Active", "Inactive", "Pending", "Suspended" )]
	[InlineData( OptionsType.ValueAndName, "10: Active", "20: Inactive", "30: Pending", "40: Suspended" )]
	public async Task WithOptions_DifferentOptionsTypes_ShouldFormatCorrectly( OptionsType optionsType, params string[ ] expectedOptions ) {
		// Arrange
		var entity = new TestEntityWithCustomOptionsType {
			Status = ( TestStatus )999, // Invalid enum value
			OptionsType = optionsType
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		var error = result.Errors.First( );
		error.Options.Should( ).NotBeNull( );

		foreach ( var expectedOption in expectedOptions ) {
			error.Options.Should( ).Contain( expectedOption );
		}
	}

	[Fact]
	public async Task WithOptions_ManualOptions_ShouldUseProvidedOptions( ) {
		// Arrange
		var entity = new TestEntityWithManualOptions {
			Category = "invalid"
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		var error = result.Errors.First( );
		error.Options.Should( ).NotBeNull( );
		error.Options.Should( ).Contain( "electronics: Electronics" );
		error.Options.Should( ).Contain( "clothing: Clothing" );
		error.Options.Should( ).Contain( "books: Books" );
		error.Options.Should( ).Contain( "home: Home & Garden" );
	}

	[Fact]
	public async Task WithOptions_ConstantValidation_ShouldIncludeConstantOptions( ) {
		// Arrange
		var entity = new TestEntityWithConstantOptions {
			Priority = 999 // Invalid priority
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		var error = result.Errors.First( );
		error.Options.Should( ).NotBeNull( );
		error.Options.Should( ).Contain( "1: Low" );
		error.Options.Should( ).Contain( "3: Medium" );
		error.Options.Should( ).Contain( "5: High" );
		error.Options.Should( ).Contain( "10: Critical" );
	}

	[Fact]
	public async Task WithoutOptions_ShouldNotIncludeOptionsInError( ) {
		// Arrange
		var entity = new TestEntityWithoutOptions {
			Status = ( TestStatus )999 // Invalid enum value
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		var error = result.Errors.First( );
		error.Options.Should( ).BeNull( );
	}

	[Fact]
	public async Task WithOptions_ValidValue_ShouldNotIncludeOptions( ) {
		// Arrange
		var entity = new TestEntityWithOptions {
			Status = TestStatus.Active // Valid enum value
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}
}
