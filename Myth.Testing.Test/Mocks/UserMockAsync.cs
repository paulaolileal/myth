using Bogus;
using Microsoft.EntityFrameworkCore;
using Myth.Mocks;
using Myth.Testing.Test.Models;
using Myth.Testing.Test.Repositories;

namespace Myth.Testing.Test.Mocks;

/// <summary>
/// Async mock generator for User entities with database context
/// </summary>
/// <remarks>
/// Initialize UserMockAsync with context and Faker instance
/// </remarks>
/// <param name="context">Database context</param>
/// <param name="faker">Faker instance for data generation</param>
public class UserMockAsync( UserDbContext context, Faker faker ) : ModelMockAsync<UserDbContext, User>( context, faker ) {

	/// <summary>
	/// Generate multiple User instances asynchronously
	/// </summary>
	/// <param name="amount">Number of users to generate</param>
	/// <param name="metadata">Optional metadata for customization</param>
	/// <returns>Task containing collection of generated users</returns>
	public override async Task<IEnumerable<User>> GenerateAsync( int amount, IDictionary<string, object>? metadata = null ) {
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

			if ( metadata.ContainsKey( "SaveToDatabase" ) && ( bool )metadata[ "SaveToDatabase" ] ) {
				var users = userFaker.Generate( amount );
				await _collection.AddRangeAsync( users );
				await SaveChangesAsync( );
				return users;
			}
		}

		return await Task.FromResult( userFaker.Generate( amount ) );
	}
}
