using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;

public class MockUserRepository( bool shouldFailOnCreate = false ) : IUserRepository {
	private readonly List<string> _existingEmails = new( );
	private readonly bool _shouldFailOnCreate = shouldFailOnCreate;

	public void AddExistingEmail( string email ) => _existingEmails.Add( email );

	public Task<bool> EmailExistsAsync( string email ) =>
		Task.FromResult( _existingEmails.Contains( email ) );

	public Task<User> CreateUserAsync( User user ) {
		if ( _shouldFailOnCreate ) {
			throw new InvalidOperationException( "Database error" );
		}
		return Task.FromResult( user );
	}
}
