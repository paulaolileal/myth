using Bogus;
using Myth.Mocks;
using Myth.Testing.Test.Models;

namespace Myth.Testing.Test.Mocks;

/// <summary>
/// Mock generator for User entities
/// </summary>
/// <remarks>
/// Initialize UserMock with Faker instance
/// </remarks>
/// <param name="faker">Faker instance for data generation</param>
public class UserMock( Faker faker ) : ModelMock<User>( faker ) {

	/// <summary>
	/// Generate multiple User instances
	/// </summary>
	/// <param name="amount">Number of users to generate</param>
	/// <param name="metadata">Optional metadata for customization</param>
	/// <returns>Collection of generated users</returns>
	public override IEnumerable<User> Generate( int amount, IDictionary<string, object>? metadata = null ) {
		var userFaker = new Faker<User>( )
			.RuleFor( u => u.Id, f => Guid.NewGuid( ) )
			.RuleFor( u => u.Name, f => f.Name.FullName( ) )
			.RuleFor( u => u.Email, f => f.Internet.Email( ) )
			.RuleFor( u => u.CreatedAt, f => f.Date.Past( ) )
			.RuleFor( u => u.UpdatedAt, f => f.Date.Recent( ) );

		if ( metadata != null ) {
			if ( metadata.ContainsKey( "Name" ) )
				userFaker.RuleFor( u => u.Name, metadata[ "Name" ].ToString( )! );

			if ( metadata.ContainsKey( "Email" ) )
				userFaker.RuleFor( u => u.Email, metadata[ "Email" ].ToString( )! );
		}

		return userFaker.Generate( amount );
	}
}
