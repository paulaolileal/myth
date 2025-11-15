using Myth.Builder;
using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Guard.Test.Models;

/// <summary>
/// Test model representing a user entity for validation testing
/// </summary>
public class TestUser : IValidatable<TestUser> {
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public int Age { get; set; }
	public DateTime BirthDate { get; set; }
	public DateOnly RegistrationDate { get; set; }
	public IEnumerable<string> Tags { get; set; } = new List<string>( );
	public bool IsActive { get; set; }
	public UserRole Role { get; set; }
	public string? PhoneNumber { get; set; }
	public PhoneType PhoneType { get; set; }
	public bool IsVerified { get; set; }
	public decimal Salary { get; set; }
	public double Score { get; set; }

	public void Validate( ValidationBuilder<TestUser> builder, ValidationContextKey? context = null ) {
		// Global rules
		builder.For( Name, x => x
			.NotEmpty( )
			.MinimumLength( 2 )
			.MaximumLength( 100 )
			.OnlyLetters( ) );

		builder.For( Email, x => x
			.NotEmpty( )
			.Email( ) );

		builder.For( Age, x => x
			.GreaterThan( 0 )
			.LessThan( 150 ) );

		builder.For( BirthDate, x => x
			.Past( )
			.After( new DateTime( 1900, 1, 1 ) ) );

		builder.For( RegistrationDate, x => x
			.BeforeOrEquals( DateOnly.FromDateTime( DateTime.Today ) ) );

		builder.For( Tags, x => x
			.NotEmpty( )
			.CountBetween( 1, 10 )
			.All( ( string tag ) => !string.IsNullOrWhiteSpace( tag ) ) );

		builder.For( Role, x => x.BeInEnum( ) );

		builder.For( Salary, x => x
			.GreaterOrEquals( 0 )
			.LessThan( 1000000 ) );

		builder.For( Score, x => x
			.Between( 0, 100 ) );

		// Context-specific rules
		builder.InContext( ValidationContextKey.Create, b => {
			b.For( Email, x => x
				.RespectAsync( async ( email, ct, sp ) => {
					var userService = sp.GetService( typeof( ITestUserService ) ) as ITestUserService;
					return userService == null || await userService.IsEmailAvailableAsync( email, ct );
				} )
				.WithMessage( "Email already exists" )
				.WithCode( "EMAIL_EXISTS" ) );

			b.For( IsActive, x => x.IsTrue( ) );
		} );

		builder.InContext( ValidationContextKey.Update, b => {
			b.For( Age, x => x.GreaterOrEquals( 18 ) );
		} );

		// Conditional rules
		builder.For( PhoneNumber, x => x
			.NotEmpty( )
			.When<string, TestUser>( user => user.PhoneType == PhoneType.Required )
			.Unless<string, TestUser>( user => user.IsVerified ) );
	}
}

public enum UserRole {
	Admin,
	User,
	Guest
}

public enum PhoneType {
	Optional,
	Required
}
