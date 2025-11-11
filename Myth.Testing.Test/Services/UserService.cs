using Myth.Testing.Test.Models;

namespace Myth.Testing.Test.Services;

/// <summary>
/// Example service for demonstrating testing patterns
/// </summary>
public class UserService {
	private readonly List<User> _users = new( );

	/// <summary>
	/// Create a new user
	/// </summary>
	/// <param name="user">The user to create</param>
	/// <returns>The created user</returns>
	public async Task<User> CreateUserAsync( User user ) {
		if ( user is null )
			throw new ArgumentNullException( nameof( user ) );

		if ( string.IsNullOrWhiteSpace( user.Email ) )
			throw new ArgumentException( "Email is required", nameof( user ) );

		if ( _users.Any( u => u.Email == user.Email ) )
			throw new InvalidOperationException( "User with this email already exists" );

		user.Id = Guid.NewGuid( );
		user.CreatedAt = DateTime.UtcNow;

		_users.Add( user );

		// Simulate async operation
		await Task.Delay( 10 );

		return user;
	}

	/// <summary>
	/// Get user by email
	/// </summary>
	/// <param name="email">The user email</param>
	/// <returns>The user if found, null otherwise</returns>
	public async Task<User?> GetUserByEmailAsync( string email ) {
		if ( string.IsNullOrWhiteSpace( email ) )
			return null;

		// Simulate async operation
		await Task.Delay( 5 );

		return _users.FirstOrDefault( u => u.Email.Equals( email, StringComparison.OrdinalIgnoreCase ) );
	}

	/// <summary>
	/// Get all users
	/// </summary>
	/// <returns>All users</returns>
	public async Task<IEnumerable<User>> GetAllUsersAsync( ) {
		// Simulate async operation
		await Task.Delay( 5 );

		return _users.ToList( );
	}

	/// <summary>
	/// Update user
	/// </summary>
	/// <param name="user">The user to update</param>
	/// <returns>The updated user</returns>
	public async Task<User> UpdateUserAsync( User user ) {
		if ( user is null )
			throw new ArgumentNullException( nameof( user ) );

		var existingUser = _users.FirstOrDefault( u => u.Id == user.Id );
		if ( existingUser is null )
			throw new InvalidOperationException( "User not found" );

		existingUser.Name = user.Name;
		existingUser.Email = user.Email;
		existingUser.UpdatedAt = DateTime.UtcNow;

		// Simulate async operation
		await Task.Delay( 10 );

		return existingUser;
	}

	/// <summary>
	/// Delete user
	/// </summary>
	/// <param name="id">The user ID</param>
	/// <returns>True if deleted, false if not found</returns>
	public async Task<bool> DeleteUserAsync( Guid id ) {
		var user = _users.FirstOrDefault( u => u.Id == id );
		if ( user is null )
			return false;

		_users.Remove( user );

		// Simulate async operation
		await Task.Delay( 10 );

		return true;
	}
}
