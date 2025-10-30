using FluentAssertions;
using Myth.Guard.Test.Models;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for numeric validation rules
/// </summary>
public class NumericRulesTests : BaseTestFixture {
	private readonly Validator _validator;

	public NumericRulesTests( ) {
		_validator = new Validator( ServiceProvider );
	}

	[Fact]
	public async Task IntValue_WithValidRange_ShouldPass( ) {
		// Arrange
		var entity = new NumericTestEntity {
			IntValue = 50,														// Valid: between 0 and 100
			DecimalValue = 100.50m,												// Valid: positive and <= 1000.99
			DoubleValue = 0.5,													// Valid: between 0.0 and 1.0
			LongValue = 500L													// Valid: not zero and >= -1000
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( -5 )]															// Less than 0
	[InlineData( 0 )]															// Equal to 0 (should be greater than 0)
	[InlineData( 150 )]															// Greater than 100
	public async Task IntValue_GreaterThan_WithInvalidValues_ShouldFail( int invalidValue ) {
		// Arrange
		var entity = new NumericTestEntity { IntValue = invalidValue };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "IntValue" );
	}

	[Theory]
	[InlineData( 1 )]
	[InlineData( 50 )]
	[InlineData( 99 )]
	public async Task IntValue_LessThan_WithValidValues_ShouldPass( int validValue ) {
		// Arrange
		var entity = new NumericTestEntity {
			IntValue = validValue,
			DecimalValue = 100m,
			DoubleValue = 0.5,
			LongValue = 100L
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( 0.1 )]
	[InlineData( 500.99 )]
	[InlineData( 1000.99 )]
	public async Task DecimalValue_Positive_WithValidValues_ShouldPass( decimal validValue ) {
		// Arrange
		var entity = new NumericTestEntity {
			IntValue = 50,
			DecimalValue = validValue,
			DoubleValue = 0.5,
			LongValue = 100L
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( -0.1 )]														// Negative
	[InlineData( 0 )]															// Zero (should be positive)
	[InlineData( 1001.00 )]														// Greater than 1000.99
	public async Task DecimalValue_Positive_WithInvalidValues_ShouldFail( decimal invalidValue ) {
		// Arrange
		var entity = new NumericTestEntity { DecimalValue = invalidValue };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "DecimalValue" );
	}

	[Theory]
	[InlineData( 0.0 )]
	[InlineData( 0.5 )]
	[InlineData( 1.0 )]
	public async Task DoubleValue_Between_WithValidValues_ShouldPass( double validValue ) {
		// Arrange
		var entity = new NumericTestEntity {
			IntValue = 50,
			DecimalValue = 100m,
			DoubleValue = validValue,
			LongValue = 100L
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( -0.1 )]														// Less than 0.0
	[InlineData( 1.1 )]															// Greater than 1.0
	[InlineData( 2.0 )]															// Greater than 1.0
	public async Task DoubleValue_Between_WithInvalidValues_ShouldFail( double invalidValue ) {
		// Arrange
		var entity = new NumericTestEntity { DoubleValue = invalidValue };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "DoubleValue" );
	}

	[Theory]
	[InlineData( -1000L )]														// Minimum valid value
	[InlineData( -500L )]
	[InlineData( 100L )]
	[InlineData( 1000L )]
	public async Task LongValue_NotZeroAndGreaterOrEquals_WithValidValues_ShouldPass( long validValue ) {
		// Arrange
		var entity = new NumericTestEntity {
			IntValue = 50,
			DecimalValue = 100m,
			DoubleValue = 0.5,
			LongValue = validValue
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( 0L )]															// Zero (should be not zero)
	[InlineData( -1001L )]														// Less than -1000
	public async Task LongValue_NotZeroAndGreaterOrEquals_WithInvalidValues_ShouldFail( long invalidValue ) {
		// Arrange
		var entity = new NumericTestEntity { LongValue = invalidValue };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "LongValue" );
	}

	[Fact]
	public async Task TestUser_Age_WithValidAge_ShouldPass( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,															// Valid: > 0 and < 150
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000m,													// Valid: >= 0 and < 1000000
			Score = 85.5														// Valid: between 0 and 100
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( -1 )]															// Negative age
	[InlineData( 0 )]															// Zero age
	[InlineData( 200 )]															// Too old
	public async Task TestUser_Age_WithInvalidAge_ShouldFail( int invalidAge ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = invalidAge,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000m,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Age" );
	}

	[Theory]
	[InlineData( -100 )]														// Negative salary
	[InlineData( 1000000 )]														// Equal to limit (should be less than)
	[InlineData( 1500000 )]														// Over limit
	public async Task TestUser_Salary_WithInvalidSalary_ShouldFail( decimal invalidSalary ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = invalidSalary,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Salary" );
	}

	[Theory]
	[InlineData( -10.5 )]														// Below range
	[InlineData( 150.0 )]														// Above range
	public async Task TestUser_Score_WithInvalidScore_ShouldFail( double invalidScore ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000m,
			Score = invalidScore
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Score" );
	}

	[Theory]
	[InlineData( 0 )]															// Minimum valid
	[InlineData( 50000 )]														// Middle range
	[InlineData( 999999.99 )]													// Maximum valid
	public async Task TestUser_Salary_WithValidSalary_ShouldPass( decimal validSalary ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = validSalary,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( 0.0 )]															// Minimum valid
	[InlineData( 50.5 )]														// Middle range
	[InlineData( 100.0 )]														// Maximum valid
	public async Task TestUser_Score_WithValidScore_ShouldPass( double validScore ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000m,
			Score = validScore
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}
}