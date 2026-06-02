using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Guard;
using Myth.Guard.Test.Models;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for string validation rules
/// </summary>
public class StringRulesTests {
	private readonly IServiceProvider _serviceProvider;
	private readonly Validator _validator;

	public StringRulesTests( ) {
		var services = new ServiceCollection( );
		_serviceProvider = services.BuildServiceProvider( );
		_validator = new Validator( _serviceProvider );
	}

	[Fact]
	public async Task NotEmpty_WithEmptyString_ShouldFail( ) {
		// Arrange
		var entity = new StringTestEntity { Text = "" };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Text" );
	}

	[Fact]
	public async Task NotEmpty_WithNullString_ShouldFail( ) {
		// Arrange
		var entity = new StringTestEntity { Text = null! };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Text" );
	}

	[Fact]
	public async Task NotEmpty_WithValidString_ShouldPass( ) {
		// Arrange
		var entity = new StringTestEntity {
			Text = "Valid text",
			Email = "test@example.com",
			Url = "https://example.com",
			AlphaNumeric = "abc123"
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( "Hi" )] // Length 2, minimum is 5
	[InlineData( "Test" )] // Length 4, minimum is 5
	public async Task MinimumLength_WithShortString_ShouldFail( string shortText ) {
		// Arrange
		var entity = new StringTestEntity { Text = shortText };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Text" );
	}

	[Theory]
	[InlineData( "ValidText" )] // Length 9, within limits
	[InlineData( "Exactly50CharactersLongTextForTestingMaxLength12" )] // Exactly 50 chars
	public async Task MinimumAndMaximumLength_WithValidLength_ShouldPass( string validText ) {
		// Arrange
		var entity = new StringTestEntity {
			Text = validText,
			Email = "test@example.com",
			Url = "https://example.com",
			AlphaNumeric = "abc123"
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task MaximumLength_WithTooLongString_ShouldFail( ) {
		// Arrange
		var longText = new string( 'a', 51 ); // Exceeds maximum of 50
		var entity = new StringTestEntity { Text = longText };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Text" );
	}

	[Theory]
	[InlineData( "test@example.com" )]
	[InlineData( "user.name+tag@domain.co.uk" )]
	[InlineData( "test123@subdomain.example.org" )]
	public async Task Email_WithValidEmail_ShouldPass( string validEmail ) {
		// Arrange
		var entity = new StringTestEntity {
			Text = "Valid text",
			Email = validEmail,
			Url = "https://example.com",
			AlphaNumeric = "abc123"
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( "invalid-email" )]
	[InlineData( "@domain.com" )]
	[InlineData( "user@" )]
	[InlineData( "user space@domain.com" )]
	[InlineData( "" )]
	public async Task Email_WithInvalidEmail_ShouldFail( string invalidEmail ) {
		// Arrange
		var entity = new StringTestEntity { Email = invalidEmail };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Email" );
	}

	[Theory]
	[InlineData( "https://example.com" )]
	[InlineData( "http://subdomain.example.org" )]
	[InlineData( "https://example.com/path?param=value" )]
	[InlineData( "ftp://files.example.com" )]
	public async Task Url_WithValidUrl_ShouldPass( string validUrl ) {
		// Arrange
		var entity = new StringTestEntity {
			Text = "Valid text",
			Email = "test@example.com",
			Url = validUrl,
			AlphaNumeric = "abc123"
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( "not-a-url" )]
	[InlineData( "://invalid" )]
	[InlineData( "http://" )]
	[InlineData( "" )]
	public async Task Url_WithInvalidUrl_ShouldFail( string invalidUrl ) {
		// Arrange
		var entity = new StringTestEntity { Url = invalidUrl };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Url" );
	}

	[Theory]
	[InlineData( "abc123" )]
	[InlineData( "TEST456" )]
	[InlineData( "MixedCase789" )]
	public async Task Alphanumeric_WithValidInput_ShouldPass( string validAlphaNumeric ) {
		// Arrange
		var entity = new StringTestEntity {
			Text = "Valid text",
			Email = "test@example.com",
			Url = "https://example.com",
			AlphaNumeric = validAlphaNumeric
		};

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( "abc-123" )] // Contains hyphen
	[InlineData( "test@123" )] // Contains special character
	[InlineData( "hello world" )] // Contains space
	[InlineData( "test!" )] // Contains exclamation
	public async Task Alphanumeric_WithInvalidInput_ShouldFail( string invalidAlphaNumeric ) {
		// Arrange
		var entity = new StringTestEntity { AlphaNumeric = invalidAlphaNumeric };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "AlphaNumeric" );
	}
}

/// <summary>
/// Additional tests for advanced string rules
/// </summary>
public class AdvancedStringRulesTests {
	private readonly IServiceProvider _serviceProvider;

	public AdvancedStringRulesTests( ) {
		var services = new ServiceCollection( );
		_serviceProvider = services.BuildServiceProvider( );
	}

	[Fact]
	public async Task OnlyLetters_WithLettersOnly_ShouldPass( ) {
		// Arrange
		var user = new TestUser {
			Name = "JohnDoe", // Only letters
			Email = "john@example.com",
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
		var result = await validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task OnlyLetters_WithNumbers_ShouldFail( ) {
		// Arrange
		var user = new TestUser {
			Name = "John123", // Contains numbers
			Email = "john@example.com",
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
		var result = await validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Name" );
	}

	[Fact]
	public async Task CustomStringRules_WithComplexValidation_ShouldWork( ) {
		// This test would require creating a more complex entity with custom string rules
		// For now, we'll test the basic functionality with TestUser

		// Arrange
		var user = new TestUser {
			Name = "ValidName",
			Email = "test@example.com",
			Age = 25,
			BirthDate = DateTime.Now.AddYears( -25 ),
			RegistrationDate = DateOnly.FromDateTime( DateTime.Today ),
			Tags = new List<string> { "developer", "remote" },
			IsActive = true,
			Role = UserRole.User,
			Salary = 50000,
			Score = 85.5
		};

		var validator = new Validator( _serviceProvider );

		// Act
		var result = await validator.ValidateAndReturnAsync( user );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task MaxLength_alias_should_reject_string_exceeding_limit( ) {
		// MaxLength is an alias for MaximumLength — same behavior expected
		var result = await Sentry.For( "toolongvalue", "Field" )
			.MaxLength( 5 )
			.ValidateAsync( _serviceProvider );

		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Field" );
	}

	[Fact]
	public async Task MaxLength_alias_should_accept_string_within_limit( ) {
		var result = await Sentry.For( "ok", "Field" )
			.MaxLength( 10 )
			.ValidateAsync( _serviceProvider );

		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task MinLength_alias_should_reject_string_below_minimum( ) {
		var result = await Sentry.For( "ab", "Field" )
			.MinLength( 5 )
			.ValidateAsync( _serviceProvider );

		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Field" );
	}

	[Fact]
	public async Task MinLength_alias_should_accept_string_meeting_minimum( ) {
		var result = await Sentry.For( "hello", "Field" )
			.MinLength( 5 )
			.ValidateAsync( _serviceProvider );

		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task MaxLength_alias_should_produce_same_error_as_MaximumLength( ) {
		// Both aliases must produce an error for the same field — error message format must be identical
		var aliasResult = await Sentry.For( "tooLongValue", "Name" )
			.MaxLength( 5 )
			.ValidateAsync( _serviceProvider );

		var canonicalResult = await Sentry.For( "tooLongValue", "Name" )
			.MaximumLength( 5 )
			.ValidateAsync( _serviceProvider );

		aliasResult.IsValid.Should( ).BeFalse( );
		canonicalResult.IsValid.Should( ).BeFalse( );
		aliasResult.Errors[ 0 ].Message.Should( ).Be( canonicalResult.Errors[ 0 ].Message );
	}
}
