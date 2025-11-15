using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Guard.Test.Models;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for DateTime and DateOnly validation rules
/// </summary>
public class DateTimeRulesTests {
	private readonly IServiceProvider _serviceProvider;
	private readonly Validator _validator;

	public DateTimeRulesTests( ) {
		var services = new ServiceCollection( );
		_serviceProvider = services.BuildServiceProvider( );
		_validator = new Validator( _serviceProvider );
	}

	[Fact]
	public async Task DateTime_WithValidDates_ShouldPass( ) {
		// Arrange
		var entity = new DateTimeTestEntity {
			CreatedAt = DateTime.Now.AddDays( -1 ), // Past date, after 2020
			UpdatedAt = DateTime.Now.AddDays( 1 ), // Future date
			EventDate = DateOnly.FromDateTime( DateTime.Today.AddDays( 30 ) ) // Future event within a year
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task DateTime_Past_WithFutureDate_ShouldFail( ) {
		// Arrange
		var entity = new DateTimeTestEntity {
			CreatedAt = DateTime.Now.AddDays( 1 ), // Future date (should be past)
			UpdatedAt = DateTime.Now.AddDays( 1 ),
			EventDate = DateOnly.FromDateTime( DateTime.Today.AddDays( 30 ) )
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "CreatedAt" );
	}

	[Fact]
	public async Task DateTime_After_WithTooOldDate_ShouldFail( ) {
		// Arrange
		var entity = new DateTimeTestEntity {
			CreatedAt = new DateTime( 1999, 1, 1 ), // Before 2020 (should be after 2020-01-01)
			UpdatedAt = DateTime.Now.AddDays( 1 ),
			EventDate = DateOnly.FromDateTime( DateTime.Today.AddDays( 30 ) )
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "CreatedAt" );
	}

	[Fact]
	public async Task DateTime_Future_WithPastDate_ShouldFail( ) {
		// Arrange
		var entity = new DateTimeTestEntity {
			CreatedAt = DateTime.Now.AddDays( -1 ),
			UpdatedAt = DateTime.Now.AddDays( -1 ), // Past date (should be future)
			EventDate = DateOnly.FromDateTime( DateTime.Today.AddDays( 30 ) )
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "UpdatedAt" );
	}

	[Fact]
	public async Task DateTime_Future_WithNullValue_ShouldPassWhenCondition( ) {
		// Arrange (UpdatedAt is null, so the When condition should make the rule not apply)
		var entity = new DateTimeTestEntity {
			CreatedAt = DateTime.Now.AddDays( -1 ),
			UpdatedAt = null, // Null value
			EventDate = DateOnly.FromDateTime( DateTime.Today.AddDays( 30 ) )
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( ); // Should pass because When condition is not met
	}

	[Fact]
	public async Task DateOnly_Between_WithValidDate_ShouldPass( ) {
		// Arrange
		var entity = new DateTimeTestEntity {
			CreatedAt = DateTime.Now.AddDays( -1 ),
			UpdatedAt = DateTime.Now.AddDays( 1 ),
			EventDate = DateOnly.FromDateTime( DateTime.Today.AddDays( 180 ) ) // 6 months from today, within range
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task DateOnly_Between_WithPastDate_ShouldFail( ) {
		// Arrange
		var entity = new DateTimeTestEntity {
			CreatedAt = DateTime.Now.AddDays( -1 ),
			UpdatedAt = DateTime.Now.AddDays( 1 ),
			EventDate = DateOnly.FromDateTime( DateTime.Today.AddDays( -1 ) ) // Yesterday (before today)
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "EventDate" );
	}

	[Fact]
	public async Task DateOnly_Between_WithTooFarFutureDate_ShouldFail( ) {
		// Arrange
		var entity = new DateTimeTestEntity {
			CreatedAt = DateTime.Now.AddDays( -1 ),
			UpdatedAt = DateTime.Now.AddDays( 1 ),
			EventDate = DateOnly.FromDateTime( DateTime.Today.AddYears( 2 ) ) // 2 years from today (beyond 1 year limit)
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "EventDate" );
	}

	[Fact]
	public async Task TestUser_BirthDate_WithValidPastDate_ShouldPass( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = new DateTime( 1998, 5, 15 ), // Valid past date after 1900
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
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
	public async Task TestUser_BirthDate_WithFutureDate_ShouldFail( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( 5 ), // Future birth date
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "BirthDate" );
	}

	[Fact]
	public async Task TestUser_BirthDate_WithTooOldDate_ShouldFail( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = new DateTime( 1899, 1, 1 ), // Before 1900
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "BirthDate" );
	}

	[Fact]
	public async Task TestUser_RegistrationDate_WithValidDate_ShouldPass( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ), // Today
			Tags = new List<string> { "test" },
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
	public async Task TestUser_RegistrationDate_WithFutureDate_ShouldFail( ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today.AddDays( 1 ) ), // Tomorrow
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "RegistrationDate" );
	}

	[Theory]
	[InlineData( -365 )] // 1 year ago
	[InlineData( -30 )]  // 1 month ago
	[InlineData( -1 )]   // Yesterday
	[InlineData( 0 )]    // Today
	public async Task TestUser_RegistrationDate_WithValidPastOrTodayDates_ShouldPass( int daysFromToday ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today.AddDays( daysFromToday ) ),
			Tags = new List<string> { "test" },
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

	[Theory]
	[InlineData( 1 )]   // Tomorrow
	[InlineData( 7 )]   // Next week
	[InlineData( 30 )]  // Next month
	public async Task TestUser_RegistrationDate_WithFutureDates_ShouldFail( int daysFromToday ) {
		// Arrange
		var user = new TestUser {
			Name = "TestUser",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today.AddDays( daysFromToday ) ),
			Tags = new List<string> { "test" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "RegistrationDate" );
	}
}
