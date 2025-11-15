using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Guard;
using Myth.Interfaces;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for Constant validation rules (ExistsInConstant and NameExistsInConstant)
/// </summary>
public partial class ConstantValidationRulesTests : BaseTestFixture {

	#region ExistsInConstant Tests

	[Fact]
	public async Task ExistsInConstant_WithValidStringValue_ShouldPass( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto { StatusCode = "A" }; // Valid status code

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert
		var statusCodeErrors = result.Errors.Where( e => e.Field == "StatusCode" ).ToList( );
		statusCodeErrors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task ExistsInConstant_WithInvalidStringValue_ShouldFail( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto { StatusCode = "X" }; // Invalid status code

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert
		var statusCodeErrors = result.Errors.Where( e => e.Field == "StatusCode" ).ToList( );
		statusCodeErrors.Should( ).HaveCount( 1 );
		statusCodeErrors[ 0 ].Message.Should( ).Contain( "Value 'X' is not valid" );
		statusCodeErrors[ 0 ].Message.Should( ).Contain( $"Valid options are: {TestStatus.GetOptions( )}" );
		statusCodeErrors[ 0 ].Code.Should( ).Be( "VIOLATION" );
	}

	[Fact]
	public async Task ExistsInConstant_WithValidIntValue_ShouldPass( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto { PriorityLevel = 5 }; // Valid priority level

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert
		var priorityErrors = result.Errors.Where( e => e.Field == "PriorityLevel" ).ToList( );
		priorityErrors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task ExistsInConstant_WithInvalidIntValue_ShouldFail( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto { PriorityLevel = 999 }; // Invalid priority level

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert
		var priorityErrors = result.Errors.Where( e => e.Field == "PriorityLevel" ).ToList( );
		priorityErrors.Should( ).HaveCount( 1 );
		priorityErrors[ 0 ].Message.Should( ).Contain( "Value '999' is not valid" );
		priorityErrors[ 0 ].Message.Should( ).Contain( $"Valid options are: {TestPriority.GetOptions( )}" );
		priorityErrors[ 0 ].Code.Should( ).Be( "VIOLATION" );
	}

	[Fact]
	public async Task ExistsInConstant_WithNullValue_ShouldPass( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );

		var dto = new SimpleNullableDto {
			NullableStatusCode = null // Null should be handled gracefully
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert - Should not fail on null, let NotEmpty handle null validation
		result.IsValid.Should( ).BeTrue( );
	}

	#endregion

	#region NameExistsInConstant Tests

	[Fact]
	public async Task NameExistsInConstant_WithValidStringName_ShouldPass( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto { StatusName = "Active" }; // Valid status name

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert
		var statusNameErrors = result.Errors.Where( e => e.Field == "StatusName" ).ToList( );
		statusNameErrors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task NameExistsInConstant_WithInvalidStringName_ShouldFail( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto { StatusName = "Unknown" }; // Invalid status name

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert
		var statusNameErrors = result.Errors.Where( e => e.Field == "StatusName" ).ToList( );
		statusNameErrors.Should( ).HaveCount( 1 );
		statusNameErrors[ 0 ].Message.Should( ).Contain( "Name 'Unknown' is not valid" );
		statusNameErrors[ 0 ].Message.Should( ).Contain( $"Valid options are: {TestStatus.GetOptions( )}" );
		statusNameErrors[ 0 ].Code.Should( ).Be( "VIOLATION" );
	}

	[Fact]
	public async Task NameExistsInConstant_WithValidIntConstantName_ShouldPass( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto { PriorityName = "Medium" }; // Valid priority name

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert
		var priorityNameErrors = result.Errors.Where( e => e.Field == "PriorityName" ).ToList( );
		priorityNameErrors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task NameExistsInConstant_WithInvalidIntConstantName_ShouldFail( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto { PriorityName = "Critical" }; // Invalid priority name

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert
		var priorityNameErrors = result.Errors.Where( e => e.Field == "PriorityName" ).ToList( );
		priorityNameErrors.Should( ).HaveCount( 1 );
		priorityNameErrors[ 0 ].Message.Should( ).Contain( "Name 'Critical' is not valid" );
		priorityNameErrors[ 0 ].Message.Should( ).Contain( $"Valid options are: {TestPriority.GetOptions( )}" );
		priorityNameErrors[ 0 ].Code.Should( ).Be( "VIOLATION" );
	}

	[Fact]
	public async Task NameExistsInConstant_WithEmptyString_ShouldPass( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );

		var dto = new SimpleNullableDto {
			NullableStatusName = string.Empty // Empty should be handled gracefully
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert - Should not fail on empty, let NotEmpty handle empty validation
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task NameExistsInConstant_WithWhitespaceString_ShouldPass( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );

		var dto = new SimpleNullableDto {
			NullableStatusName = "   " // Whitespace should be handled gracefully
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert - Should not fail on whitespace, let NotEmpty handle whitespace validation
		result.IsValid.Should( ).BeTrue( );
	}

	#endregion

	#region Integration Tests

	[Fact]
	public async Task CompleteDto_WithAllValidValues_ShouldPass( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto {
			StatusCode = "A",
			StatusName = "Active",
			PriorityLevel = 5,
			PriorityName = "Medium"
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert
		result.IsValid.Should( ).BeTrue( );
		result.Errors.Should( ).BeEmpty( );
	}

	[Fact]
	public async Task CompleteDto_WithAllInvalidValues_ShouldFailWithMultipleErrors( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto {
			StatusCode = "X",
			StatusName = "Unknown",
			PriorityLevel = 999,
			PriorityName = "Critical"
		};

		// Act
		var result = await validator.ValidateAndReturnAsync( dto );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).HaveCount( 4 );

		// Check specific error messages contain GetOptions() output
		var statusCodeError = result.Errors.FirstOrDefault( e => e.Field == "StatusCode" );
		statusCodeError.Should( ).NotBeNull( );
		statusCodeError!.Message.Should( ).Contain( $"Valid options are: {TestStatus.GetOptions( )}" );

		var priorityLevelError = result.Errors.FirstOrDefault( e => e.Field == "PriorityLevel" );
		priorityLevelError.Should( ).NotBeNull( );
		priorityLevelError!.Message.Should( ).Contain( $"Valid options are: {TestPriority.GetOptions( )}" );
	}

	[Fact]
	public async Task Validator_WithConstantValidationException_ShouldThrowValidationException( ) {
		// Arrange
		var validator = ServiceProvider.GetRequiredService<IValidator>( );
		var dto = new TestDto {
			StatusCode = "X" // Invalid
		};

		// Act & Assert
		var exception = await Assert.ThrowsAsync<ValidationException>(
			( ) => validator.ValidateAsync( dto ) );

		exception.ValidationResult.Errors.Should( ).HaveCount( 4 ); // 1 for StatusCode, 3 for empty fields
		exception.ValidationResult.Errors.Any( e => e.Field == "StatusCode" ).Should( ).BeTrue( );
	}

	#endregion

	#region Custom Message Tests

	[Fact]
	public async Task ConstantValidation_WithCustomMessage_ShouldUseCustomMessage( ) {
		// Arrange
		var customDto = new CustomMessageDto { StatusCode = "X" };
		var validator = ServiceProvider.GetRequiredService<IValidator>( );

		// Act
		var result = await validator.ValidateAndReturnAsync( customDto );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		var statusError = result.Errors.FirstOrDefault( e => e.Field == "StatusCode" );
		statusError.Should( ).NotBeNull( );
		statusError!.Message.Should( ).Be( "Please select a valid status" );
		statusError.Code.Should( ).Be( "INVALID_STATUS" );
	}

	#endregion

}
