using FluentAssertions;
using Myth.Guard.Test.Models;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for boolean and enum validation rules
/// </summary>
public class BooleanAndEnumRulesTests : BaseTestFixture {
	private readonly Validator _validator;

	public BooleanAndEnumRulesTests( ) {
		_validator = new Validator( ServiceProvider );
	}

	[Fact]
	public async Task Boolean_WithValidValues_ShouldPass( ) {
		// Arrange
		var entity = new BooleanTestEntity {
			IsActive = true, 
			IsDeleted = false, 
			IsOptional = true 
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task Boolean_IsTrue_WithFalseValue_ShouldFail( ) {
		// Arrange
		var entity = new BooleanTestEntity {
			IsActive = false,
			IsDeleted = false,
			IsOptional = true
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "IsActive" );
	}

	[Fact]
	public async Task Boolean_IsFalse_WithTrueValue_ShouldFail( ) {
		// Arrange
		var entity = new BooleanTestEntity {
			IsActive = true,
			IsDeleted = true, 
			IsOptional = true
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "IsDeleted" );
	}

	[Fact]
	public async Task Boolean_NotNull_WithNullValue_ShouldFail( ) {
		// Arrange
		var entity = new BooleanTestEntity {
			IsActive = true,
			IsDeleted = false,
			IsOptional = null 
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "IsOptional" );
	}

	[Theory]
	[InlineData( true )]
	[InlineData( false )]
	public async Task Boolean_NotNull_WithValidValues_ShouldPass( bool validValue ) {
		// Arrange
		var entity = new BooleanTestEntity {
			IsActive = true,
			IsDeleted = false,
			IsOptional = validValue 
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task TestUser_IsActive_WithTrueValue_ShouldPass( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = ["test"],
			IsActive = true, 
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task TestUser_IsActive_WithFalseValue_InCreateContext_ShouldFail( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = ["test"],
			IsActive = false, 
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user, ValidationContextKey.Create );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "IsActive" );
	}

	[Theory]
	[InlineData( UserRole.Admin )]
	[InlineData( UserRole.User )]
	[InlineData( UserRole.Guest )]
	public async Task Enum_BeInEnum_WithValidValues_ShouldPass( UserRole validRole ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = ["test"],
			IsActive = true,
			Role = validRole,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task Enum_BeInEnum_WithInvalidValue_ShouldFail( ) {
		// This test demonstrates what happens with an invalid enum value
		// In practice, invalid enum values would typically be caught at compile time
		// or during JSON deserialization, but we can simulate it for testing

		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = ["test"],
			IsActive = true,
			Role = ( UserRole )999, // Invalid enum value
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Role" );
	}

	[Theory]
	[InlineData( PhoneType.Optional )]
	[InlineData( PhoneType.Required )]
	public async Task Enum_PhoneType_WithValidValues_ShouldPass( PhoneType validPhoneType ) {
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
			PhoneType = validPhoneType,
			PhoneNumber = validPhoneType == PhoneType.Required ? "123-456-7890" : null,
			IsVerified = validPhoneType == PhoneType.Required ? false : true, // For conditional logic
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task ComplexBoolean_WithMultipleConditions_ShouldValidateCorrectly( ) {
		// Test the complex conditional logic in TestUser
		// PhoneNumber should be required when PhoneType is Required AND IsVerified is false

		// Arrange 
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
			PhoneNumber = null, // Should be required
			IsVerified = false,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result1 = await _validator.ValidateAndReturnAsync( user1 );

		// Assert
		result1.IsValid.Should( ).BeFalse( );
		result1.Errors.Should( ).Contain( e => e.Field == "PhoneNumber" );

		// Arrange
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
			IsVerified = true, // Unless condition is met
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result2 = await _validator.ValidateAndReturnAsync( user2 );

		// Assert
		result2.IsValid.Should( ).BeTrue( );

		// Arrange 
		var user3 = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			PhoneType = PhoneType.Optional,
			PhoneNumber = null,
			IsVerified = false,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result3 = await _validator.ValidateAndReturnAsync( user3 );

		// Assert
		result3.IsValid.Should( ).BeTrue( );
	}
}