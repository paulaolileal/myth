namespace Myth.Guard.Test.Models;

/// <summary>
/// Interface for testing async validation with service provider
/// </summary>
public interface ITestUserService {

	Task<bool> IsEmailAvailableAsync( string email, CancellationToken cancellationToken = default );

	Task<bool> IsUsernameAvailableAsync( string username, CancellationToken cancellationToken = default );
}

/// <summary>
/// Mock implementation of user service for testing
/// </summary>
public class TestUserService : ITestUserService {
	private readonly HashSet<string> _existingEmails = new( ) { "taken@example.com", "admin@test.com" };
	private readonly HashSet<string> _existingUsernames = new( ) { "admin", "testuser" };

	public Task<bool> IsEmailAvailableAsync( string email, CancellationToken cancellationToken = default ) {
		return Task.FromResult( !_existingEmails.Contains( email.ToLowerInvariant( ) ) );
	}

	public Task<bool> IsUsernameAvailableAsync( string username, CancellationToken cancellationToken = default ) {
		return Task.FromResult( !_existingUsernames.Contains( username.ToLowerInvariant( ) ) );
	}
}