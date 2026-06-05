using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Myth.Repositories;
using Myth.Testing.Test.Models;
using Myth.Testing.Test.Repositories;

namespace Myth.Testing.Test.Mocks;

/// <summary>
/// Tests for ModelMockAsync base class using UserMockAsync implementation
/// </summary>
public class ModelMockAsyncTests : BaseDatabaseTests<UserDbContext> {
	private readonly Faker _faker;
	private UserMockAsync _userMockAsync = null!;

	/// <summary>
	/// Initialize tests with database context
	/// </summary>
	public ModelMockAsyncTests( ) {
		_faker = new Faker( "en_US" );
	}

	/// <summary>
	/// Helper method to get UserMockAsync instance
	/// </summary>
	private UserMockAsync GetUserMockAsync( ) {
		var context = GetContext( );
		return new UserMockAsync( context, _faker );
	}

	/// <summary>
	/// Test generating a single user asynchronously
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithoutAmount_ShouldReturnSingleUser( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );

		// Act
		var users = await _userMockAsync.GenerateAsync( 1 );
		var user = users.First( );

		// Assert
		user.Should( ).NotBeNull( );
		user.Should( ).BeOfType<User>( );
		user.Id.Should( ).NotBeEmpty( );
		user.Name.Should( ).NotBeNullOrEmpty( );
		user.Email.Should( ).NotBeNullOrEmpty( );
		user.CreatedAt.Should( ).BeBefore( DateTime.UtcNow );
	}

	/// <summary>
	/// Test generating multiple users asynchronously
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithAmount_ShouldReturnSpecifiedNumberOfUsers( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );
		int expectedCount = 5;

		// Act
		var users = await _userMockAsync.GenerateAsync( expectedCount );

		// Assert
		users.Should( ).NotBeNull( );
		users.Should( ).HaveCount( expectedCount );
		users.Should( ).AllBeOfType<User>( );
		users.Select( u => u.Id ).Should( ).OnlyHaveUniqueItems( );
	}

	/// <summary>
	/// Test generating users with metadata
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithMetadata_ShouldApplyMetadataValues( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );
		var metadata = new Dictionary<string, object> {
			{ "Name", "Jane Smith" },
			{ "Email", "jane.smith@example.com" }
		};

		// Act
		var user = await _userMockAsync.GenerateAsync( metadata );

		// Assert
		user.Should( ).NotBeNull( );
		user.Name.Should( ).Be( "Jane Smith" );
		user.Email.Should( ).Be( "jane.smith@example.com" );
	}

	/// <summary>
	/// Test generating and saving to database
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithSaveToDatabaseMetadata_ShouldPersistToDatabase( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );
		var metadata = new Dictionary<string, object> {
			{ "SaveToDatabase", true }
		};

		// Act
		var users = await _userMockAsync.GenerateAsync( 3, metadata );

		// Assert
		users.Should( ).HaveCount( 3 );

		var context = GetContext( );
		var savedUsers = await context.Users.ToListAsync( );
		savedUsers.Should( ).HaveCount( 3 );

		foreach ( var user in users ) {
			savedUsers.Should( ).Contain( u => u.Email == user.Email );
		}
	}

	/// <summary>
	/// Test generating without saving to database
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithoutSaveToDatabaseMetadata_ShouldNotPersistToDatabase( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );

		// Act
		var users = await _userMockAsync.GenerateAsync( 3 );

		// Assert
		users.Should( ).HaveCount( 3 );

		var context = GetContext( );
		var savedUsers = await context.Users.ToListAsync( );
		savedUsers.Should( ).BeEmpty( );
	}

	/// <summary>
	/// Test context access in async mock
	/// </summary>
	[Fact]
	public async Task GenerateAsync_ShouldHaveAccessToDbContext( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );
		var metadata = new Dictionary<string, object> {
			{ "SaveToDatabase", true }
		};

		// Act
		await _userMockAsync.GenerateAsync( 2, metadata );

		// Assert
		var context = GetContext( );
		var users = await context.Users.ToListAsync( );
		users.Should( ).HaveCount( 2 );
	}

	/// <summary>
	/// Test generating users with unique emails
	/// </summary>
	[Fact]
	public async Task GenerateAsync_ShouldCreateUsersWithUniqueEmails( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );

		// Act
		var users = await _userMockAsync.GenerateAsync( 10 );

		// Assert
		var emails = users.Select( u => u.Email ).ToList( );
		emails.Should( ).OnlyHaveUniqueItems( );
	}

	/// <summary>
	/// Test generating users with unique IDs
	/// </summary>
	[Fact]
	public async Task GenerateAsync_ShouldCreateUsersWithUniqueIds( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );

		// Act
		var users = await _userMockAsync.GenerateAsync( 20 );

		// Assert
		var ids = users.Select( u => u.Id ).ToList( );
		ids.Should( ).OnlyHaveUniqueItems( );
		ids.Should( ).AllSatisfy( id => id.Should( ).NotBeEmpty( ) );
	}

	/// <summary>
	/// Test that generated users have valid data
	/// </summary>
	[Fact]
	public async Task GenerateAsync_ShouldCreateUsersWithValidData( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );

		// Act
		var users = await _userMockAsync.GenerateAsync( 5 );

		// Assert
		users.Should( ).AllSatisfy( u => {
			u.Id.Should( ).NotBeEmpty( );
			u.Name.Should( ).NotBeNullOrEmpty( );
			u.Email.Should( ).Contain( "@" );
			u.CreatedAt.Should( ).BeBefore( DateTime.UtcNow );
		} );
	}

	/// <summary>
	/// Test generating zero users
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithZeroAmount_ShouldReturnEmptyCollection( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );

		// Act
		var users = await _userMockAsync.GenerateAsync( 0 );

		// Assert
		users.Should( ).NotBeNull( );
		users.Should( ).BeEmpty( );
	}

	/// <summary>
	/// Test generating and saving large number of users
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithLargeAmountAndSave_ShouldHandleEfficiently( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );
		var metadata = new Dictionary<string, object> {
			{ "SaveToDatabase", true }
		};

		// Act
		var users = await _userMockAsync.GenerateAsync( 100, metadata );

		// Assert
		users.Should( ).HaveCount( 100 );

		var context = GetContext( );
		var savedUsers = await context.Users.ToListAsync( );
		savedUsers.Should( ).HaveCount( 100 );
	}

	/// <summary>
	/// Test partial metadata application
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithPartialMetadata_ShouldOnlyOverrideSpecifiedFields( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );
		var metadata = new Dictionary<string, object> {
			{ "Name", "Fixed Name" }
		};

		// Act
		var users = await _userMockAsync.GenerateAsync( 3, metadata );

		// Assert
		users.Should( ).AllSatisfy( u => u.Name.Should( ).Be( "Fixed Name" ) );
		users.Select( u => u.Email ).Should( ).OnlyHaveUniqueItems( );
		users.Select( u => u.Id ).Should( ).OnlyHaveUniqueItems( );
	}

	/// <summary>
	/// Test multiple generate calls with same context
	/// </summary>
	[Fact]
	public async Task GenerateAsync_MultipleCalls_ShouldWorkWithSameContext( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );
		var metadata = new Dictionary<string, object> {
			{ "SaveToDatabase", true }
		};

		// Act
		var firstBatch = await _userMockAsync.GenerateAsync( 3, metadata );
		var secondBatch = await _userMockAsync.GenerateAsync( 2, metadata );

		// Assert
		var context = GetContext( );
		var allUsers = await context.Users.ToListAsync( );
		allUsers.Should( ).HaveCount( 5 );
	}

	/// <summary>
	/// Test that SaveChangesAsync is called correctly
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithSaveToDatabase_ShouldCallSaveChangesAsync( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );
		var metadata = new Dictionary<string, object> {
			{ "SaveToDatabase", true },
			{ "Email", "test@example.com" }
		};

		// Act
		await _userMockAsync.GenerateAsync( 1, metadata );

		// Assert
		var context = GetContext( );
		var user = await context.Users.FirstOrDefaultAsync( u => u.Email == "test@example.com" );
		user.Should( ).NotBeNull( );
		user!.Email.Should( ).Be( "test@example.com" );
	}

	/// <summary>
	/// Test concurrent generation
	/// </summary>
	[Fact]
	public async Task GenerateAsync_ConcurrentCalls_ShouldHandleCorrectly( ) {
		// Arrange
		var tasks = Enumerable.Range( 0, 5 ).Select( async i => {
			var mockAsync = GetUserMockAsync( );
			var metadata = new Dictionary<string, object> {
				{ "SaveToDatabase", true }
			};
			return await mockAsync.GenerateAsync( 2, metadata );
		} );

		// Act
		var results = await Task.WhenAll( tasks );

		// Assert
		var context = GetContext( );
		var allUsers = await context.Users.ToListAsync( );
		allUsers.Should( ).HaveCount( 10 );
		allUsers.Select( u => u.Id ).Should( ).OnlyHaveUniqueItems( );
	}

	/// <summary>
	/// Test null metadata handling
	/// </summary>
	[Fact]
	public async Task GenerateAsync_WithNullMetadata_ShouldGenerateRandomData( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );

		// Act
		var users = await _userMockAsync.GenerateAsync( 3, null );

		// Assert
		users.Should( ).HaveCount( 3 );
		users.Select( u => u.Name ).Should( ).OnlyHaveUniqueItems( );
		users.Select( u => u.Email ).Should( ).OnlyHaveUniqueItems( );
	}

	/// <summary>
	/// Test that DbSet collection is accessible
	/// </summary>
	[Fact]
	public async Task GenerateAsync_ShouldUseDbSetCollection( ) {
		// Arrange
		_userMockAsync = GetUserMockAsync( );
		var metadata = new Dictionary<string, object> {
			{ "SaveToDatabase", true }
		};

		// Act
		await _userMockAsync.GenerateAsync( 5, metadata );

		// Assert
		var context = GetContext( );
		var usersFromDbSet = await context.Users.ToListAsync( );
		usersFromDbSet.Should( ).HaveCount( 5 );
	}
}
