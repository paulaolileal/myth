using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Myth.Repositories;
using Myth.Testing.Test.Models;
using Myth.Testing.Test.Repositories;

namespace Myth.Testing.Test;

/// <summary>
/// Test class demonstrating the use of BaseMongoDbTests with MongoDB
/// </summary>
public class BaseMongoDbTestsTests : BaseMongoDbTests<UserDbContext> {
	private readonly Faker<User> _userFaker;

	/// <summary>
	/// Initialize test with MongoDB context
	/// </summary>
	public BaseMongoDbTestsTests( ) {
		_userFaker = new Faker<User>( )
			.RuleFor( u => u.Id, f => Guid.NewGuid( ) )
			.RuleFor( u => u.CreatedAt, f => f.Date.Past( ) )
			.RuleFor( u => u.UpdatedAt, f => f.Date.Recent( ) )
			.RuleFor( u => u.Name, f => f.Name.FullName( ) )
			.RuleFor( u => u.Email, f => f.Internet.Email( ) );
	}

	/// <summary>
	/// Test database initialization and context retrieval
	/// </summary>
	[Fact]
	public void GetContext_ShouldReturnValidContext( ) {
		// Act
		var context = GetContext( );

		// Assert
		context.Should( ).NotBeNull( );
		context.Should( ).BeOfType<UserDbContext>( );
		context.Database.Should( ).NotBeNull( );
	}

	/// <summary>
	/// Test async context retrieval
	/// </summary>
	[Fact]
	public async Task GetContextAsync_ShouldReturnValidContext( ) {
		// Act
		var context = await GetContextAsync( );

		// Assert
		context.Should( ).NotBeNull( );
		context.Should( ).BeOfType<UserDbContext>( );
		context.Database.Should( ).NotBeNull( );
	}

	/// <summary>
	/// Test database name is unique per test instance
	/// </summary>
	[Fact]
	public void DatabaseName_ShouldBeUnique( ) {
		// Assert
		DatabaseName.Should( ).NotBeNullOrEmpty( );
		DatabaseName.Should( ).StartWith( "TestDB_" );
		DatabaseName.Should( ).HaveLength( 39 ); // "TestDB_" (7) + GUID without hyphens (32)
	}

	/// <summary>
	/// Test adding and retrieving data from MongoDB
	/// </summary>
	[Fact]
	public async Task SaveChangesAsync_WithValidData_ShouldPersistToDatabase( ) {
		// Arrange
		var context = await GetContextAsync( );
		var user = _userFaker.Generate( );

		context.Users.Add( user );

		// Act
		await SaveChangesAsync( );

		// Assert
		var retrievedContext = await GetContextAsync( );
		var savedUser = await retrievedContext.Users.FindAsync( user.Id );

		savedUser.Should( ).NotBeNull( );
		savedUser!.Email.Should( ).Be( user.Email );
		savedUser.Name.Should( ).Be( user.Name );
	}

	/// <summary>
	/// Test multiple entities can be saved
	/// </summary>
	[Fact]
	public async Task SaveChangesAsync_WithMultipleEntities_ShouldPersistAll( ) {
		// Arrange
		var context = await GetContextAsync( );
		var users = _userFaker.Generate( 5 );

		context.Users.AddRange( users );

		// Act
		await SaveChangesAsync( );

		// Assert
		var retrievedContext = await GetContextAsync( );
		var savedUsers = await retrievedContext.Users.ToListAsync( );

		savedUsers.Should( ).HaveCount( 5 );
		foreach ( var user in users ) {
			savedUsers.Should( ).Contain( u => u.Email == user.Email );
		}
	}

	/// <summary>
	/// Test context change tracker is cleared
	/// </summary>
	[Fact]
	public async Task GetContext_ShouldClearChangeTracker( ) {
		// Arrange
		var context = await GetContextAsync( );
		var user = _userFaker.Generate( );
		context.Users.Add( user );
		await SaveChangesAsync( );

		// Act
		var freshContext = GetContext( );

		// Assert
		freshContext.ChangeTracker.Entries( ).Should( ).BeEmpty( );
	}

	/// <summary>
	/// Test MongoDB client is available
	/// </summary>
	[Fact]
	public void GetRequiredService_ShouldReturnMongoClient( ) {
		// Act
		var client = GetRequiredService<IMongoClient>( );

		// Assert
		client.Should( ).NotBeNull( );
		client.Should( ).BeAssignableTo<IMongoClient>( );
	}

	/// <summary>
	/// Test database collections are created
	/// </summary>
	[Fact]
	public async Task InitializeDatabaseAsync_ShouldCreateCollections( ) {
		// Arrange
		var context = await GetContextAsync( );
		var client = GetRequiredService<IMongoClient>( );
		var database = client.GetDatabase( DatabaseName );

		// Act
		var collections = await ( await database.ListCollectionNamesAsync( ) ).ToListAsync( );

		// Assert
		collections.Should( ).NotBeEmpty( );
		collections.Should( ).Contain( "Users" );
	}

	/// <summary>
	/// Test updating existing data
	/// </summary>
	[Fact]
	public async Task SaveChangesAsync_WithUpdatedData_ShouldPersistChanges( ) {
		// Arrange
		var context = await GetContextAsync( );
		var user = _userFaker.Generate( );
		context.Users.Add( user );
		await SaveChangesAsync( );

		// Act
		var updateContext = await GetContextAsync( );
		var userToUpdate = await updateContext.Users.FindAsync( user.Id );
		userToUpdate!.Name = "Updated Name";
		userToUpdate.Email = "updated@example.com";
		await SaveChangesAsync( );

		// Assert
		var verifyContext = await GetContextAsync( );
		var updatedUser = await verifyContext.Users.FindAsync( user.Id );
		updatedUser.Should( ).NotBeNull( );
		updatedUser!.Name.Should( ).Be( "Updated Name" );
		updatedUser.Email.Should( ).Be( "updated@example.com" );
	}

	/// <summary>
	/// Test deleting data
	/// </summary>
	[Fact]
	public async Task SaveChangesAsync_WithDeletedData_ShouldRemoveFromDatabase( ) {
		// Arrange
		var context = await GetContextAsync( );
		var user = _userFaker.Generate( );
		context.Users.Add( user );
		await SaveChangesAsync( );

		// Act
		var deleteContext = await GetContextAsync( );
		var userToDelete = await deleteContext.Users.FindAsync( user.Id );
		deleteContext.Users.Remove( userToDelete! );
		await SaveChangesAsync( );

		// Assert
		var verifyContext = await GetContextAsync( );
		var deletedUser = await verifyContext.Users.FindAsync( user.Id );
		deletedUser.Should( ).BeNull( );
	}

	/// <summary>
	/// Test retry mechanism on save failures
	/// </summary>
	[Fact]
	public async Task SaveChangesAsync_ShouldHandleTransientFailures( ) {
		// Arrange
		var context = await GetContextAsync( );
		var user = _userFaker.Generate( );
		context.Users.Add( user );

		// Act
		var act = async ( ) => await SaveChangesAsync( );

		// Assert
		await act.Should( ).NotThrowAsync( );
	}

	/// <summary>
	/// Test database cleanup after disposal
	/// </summary>
	[Fact]
	public async Task CleanupDatabaseAsync_ShouldRemoveCollections( ) {
		// Arrange
		var context = await GetContextAsync( );
		var user = _userFaker.Generate( );
		context.Users.Add( user );
		await SaveChangesAsync( );

		var client = GetRequiredService<IMongoClient>( );
		var database = client.GetDatabase( DatabaseName );

		var collectionsBeforeCleanup = await ( await database.ListCollectionNamesAsync( ) ).ToListAsync( );
		collectionsBeforeCleanup.Should( ).NotBeEmpty( );

		// Act
		await CleanupDatabaseAsync( );

		// Assert
		var collectionsAfterCleanup = await ( await database.ListCollectionNamesAsync( ) ).ToListAsync( );
		collectionsAfterCleanup.Should( ).BeEmpty( );
	}

	/// <summary>
	/// Test concurrent access to database
	/// </summary>
	[Fact]
	public async Task GetContextAsync_WithConcurrentAccess_ShouldHandleCorrectly( ) {
		// Arrange
		var users = _userFaker.Generate( 5 );

		// Act
		var context = await GetContextAsync( );

		await context.Users.AddRangeAsync( users );

		await SaveChangesAsync( );

		// Assert
		var verifyContext = await GetContextAsync( );

		var usersInDb = await verifyContext.Users.ToListAsync( );

		usersInDb.Should( ).HaveCount( 5 );

		usersInDb.Select( x => x.Id ).Should( ).OnlyHaveUniqueItems( );
	}

	/// <summary>
	/// Test disposal does not throw exceptions
	/// </summary>
	[Fact]
	public async Task DisposeAsync_ShouldNotThrowException( ) {
		// Arrange
		var context = await GetContextAsync( );
		var user = _userFaker.Generate( );
		context.Users.Add( user );
		await SaveChangesAsync( );

		// Act
		var act = async ( ) => await DisposeAsync( );

		// Assert
		await act.Should( ).NotThrowAsync( );
	}

	/// <summary>
	/// Test synchronous disposal
	/// </summary>
	[Fact]
	public void Dispose_ShouldNotThrowException( ) {
		// Arrange
		var context = GetContext( );

		// Act
		var act = ( ) => Dispose( );

		// Assert
		act.Should( ).NotThrow( );
	}
}
