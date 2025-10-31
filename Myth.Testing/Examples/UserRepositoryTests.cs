using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Testing.Builders;
using Myth.Testing.Extensions;
using Myth.Testing.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Testing.Examples {
	/// <summary>
	/// Example test class demonstrating the use of BaseDatabaseTests with Entity Framework
	/// </summary>
	public class UserRepositoryTests : BaseDatabaseTests<UserDbContext> {
		private readonly UserRepository _repository;

		/// <summary>
		/// Initialize test with UserRepository and DbContext
		/// </summary>
		public UserRepositoryTests( ) {
			// Add services to the test container
			AddService<UserRepository, UserRepository>( );

			// Get the repository instance
			_repository = GetRequiredService<UserRepository>( );
		}

		/// <summary>
		/// Test creating a user with database
		/// </summary>
		[Fact]
		public async Task CreateAsync_WithValidUser_ShouldCreateUserInDatabase( ) {
			// Arrange
			await InitializeDatabaseAsync( );

			var userEntity = new UserEntityBuilder( _faker )
				.WithName( "John Doe" )
				.WithEmail( "john.doe@example.com" )
				.Build( );

			// Act
			var result = await _repository.CreateAsync( userEntity );

			// Assert
			result.Should( ).NotBeNull( );
			result.Id.Should( ).NotBeEmpty( );
			result.Name.Should( ).Be( "John Doe" );
			result.Email.Should( ).Be( "john.doe@example.com" );
			result.CreatedAt.Should( ).BeCloseTo( DateTime.UtcNow, TimeSpan.FromSeconds( 5 ) );

			// Verify in database
			var context = GetContext( );
			var dbUser = await context.Users.FindAsync( result.Id );
			dbUser.Should( ).NotBeNull( );
			dbUser!.Email.Should( ).Be( "john.doe@example.com" );

			await CleanupDatabaseAsync( );
		}

		/// <summary>
		/// Test getting user by ID
		/// </summary>
		[Fact]
		public async Task GetByIdAsync_WithExistingId_ShouldReturnUser( ) {
			// Arrange
			await InitializeDatabaseAsync( );

			var userEntity = new UserEntityBuilder( _faker ).Build( );
			var created = await _repository.CreateAsync( userEntity );

			// Act
			var result = await _repository.GetByIdAsync( created.Id );

			// Assert
			result.Should( ).NotBeNull( );
			result!.Id.Should( ).Be( created.Id );
			result.Email.Should( ).Be( created.Email );

			await CleanupDatabaseAsync( );
		}

		/// <summary>
		/// Test getting user by non-existing ID
		/// </summary>
		[Fact]
		public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull( ) {
			// Arrange
			await InitializeDatabaseAsync( );

			// Act
			var result = await _repository.GetByIdAsync( Guid.NewGuid( ) );

			// Assert
			result.Should( ).BeNull( );

			await CleanupDatabaseAsync( );
		}

		/// <summary>
		/// Test getting user by email
		/// </summary>
		[Fact]
		public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser( ) {
			// Arrange
			await InitializeDatabaseAsync( );

			var userEntity = new UserEntityBuilder( _faker )
				.WithEmail( "test@example.com" )
				.Build( );
			await _repository.CreateAsync( userEntity );

			// Act
			var result = await _repository.GetByEmailAsync( "test@example.com" );

			// Assert
			result.Should( ).NotBeNull( );
			result!.Email.Should( ).Be( "test@example.com" );

			await CleanupDatabaseAsync( );
		}

		/// <summary>
		/// Test getting all users
		/// </summary>
		[Fact]
		public async Task GetAllAsync_WithMultipleUsers_ShouldReturnAllUsers( ) {
			// Arrange
			await InitializeDatabaseAsync( );

			var users = new UserEntityBuilder( _faker ).BuildList( 3 );
			foreach ( var user in users )
				await _repository.CreateAsync( user );

			// Act
			var result = await _repository.GetAllAsync( );

			// Assert
			result.Should( ).HaveCount( 3 );
			result.Should( ).Contain( u => users.Any( created => created.Email == u.Email ) );

			await CleanupDatabaseAsync( );
		}

		/// <summary>
		/// Test updating user
		/// </summary>
		[Fact]
		public async Task UpdateAsync_WithValidUser_ShouldUpdateUserInDatabase( ) {
			// Arrange
			await InitializeDatabaseAsync( );

			var userEntity = new UserEntityBuilder( _faker ).Build( );
			var created = await _repository.CreateAsync( userEntity );

			created.Name = "Updated Name";
			created.Email = "updated@example.com";

			// Act
			var result = await _repository.UpdateAsync( created );

			// Assert
			result.Should( ).NotBeNull( );
			result.Name.Should( ).Be( "Updated Name" );
			result.Email.Should( ).Be( "updated@example.com" );
			result.UpdatedAt.Should( ).BeCloseTo( DateTime.UtcNow, TimeSpan.FromSeconds( 5 ) );

			// Verify in database
			var dbUser = await _repository.GetByIdAsync( created.Id );
			dbUser!.Name.Should( ).Be( "Updated Name" );
			dbUser.Email.Should( ).Be( "updated@example.com" );

			await CleanupDatabaseAsync( );
		}

		/// <summary>
		/// Test deleting user
		/// </summary>
		[Fact]
		public async Task DeleteAsync_WithExistingId_ShouldDeleteUserFromDatabase( ) {
			// Arrange
			await InitializeDatabaseAsync( );

			var userEntity = new UserEntityBuilder( _faker ).Build( );
			var created = await _repository.CreateAsync( userEntity );

			// Act
			var result = await _repository.DeleteAsync( created.Id );

			// Assert
			result.Should( ).BeTrue( );

			// Verify deletion
			var dbUser = await _repository.GetByIdAsync( created.Id );
			dbUser.Should( ).BeNull( );

			await CleanupDatabaseAsync( );
		}

		/// <summary>
		/// Test with async patterns and timeout
		/// </summary>
		[Fact]
		public async Task CreateAsync_WithTimeout_ShouldCompleteWithinTimeLimit( ) {
			// Arrange
			await InitializeDatabaseAsync( );

			var userEntity = new UserEntityBuilder( _faker ).Build( );

			// Act & Assert
			await TestExtensions.WithTimeoutAsync(
				( ) => _repository.CreateAsync( userEntity ),
				TimeSpan.FromSeconds( 5 )
			);

			await CleanupDatabaseAsync( );
		}

		/// <summary>
		/// Test error handling scenario
		/// </summary>
		[Fact]
		public async Task CreateAsync_WithNullUser_ShouldThrowArgumentNullException( ) {
			// Arrange
			await InitializeDatabaseAsync( );

			// Act & Assert
			await TestExtensions.AssertThrowsAsync<ArgumentNullException>(
				( ) => _repository.CreateAsync( null! )
			);

			// Verify no users were created
			var allUsers = await _repository.GetAllAsync( );
			allUsers.Should( ).BeEmpty( );

			await CleanupDatabaseAsync( );
		}
	}

	/// <summary>
	/// Example builder for UserEntity objects
	/// </summary>
	public class UserEntityBuilder : TestDataBuilder<UserEntity, UserEntityBuilder> {
		/// <summary>
		/// Initialize UserEntityBuilder with Faker
		/// </summary>
		/// <param name="faker">The Faker instance</param>
		public UserEntityBuilder( Bogus.Faker faker ) : base( faker ) { }

		/// <summary>
		/// Set the user name
		/// </summary>
		/// <param name="name">The user name</param>
		/// <returns>The builder for fluent chaining</returns>
		public UserEntityBuilder WithName( string name ) {
			return With( nameof( UserEntity.Name ), name );
		}

		/// <summary>
		/// Set the user email
		/// </summary>
		/// <param name="email">The user email</param>
		/// <returns>The builder for fluent chaining</returns>
		public UserEntityBuilder WithEmail( string email ) {
			return With( nameof( UserEntity.Email ), email );
		}

		/// <summary>
		/// Set the user ID
		/// </summary>
		/// <param name="id">The user ID</param>
		/// <returns>The builder for fluent chaining</returns>
		public UserEntityBuilder WithId( Guid id ) {
			return With( nameof( UserEntity.Id ), id );
		}

		/// <summary>
		/// Build the UserEntity object
		/// </summary>
		/// <returns>A UserEntity object with generated or overridden values</returns>
		public override UserEntity Build( ) {
			return new UserEntity {
				Id = GetOverrideOrDefault( nameof( UserEntity.Id ), Guid.Empty ),
				Name = GetOverrideOrGenerate( nameof( UserEntity.Name ), f => f.Name.FullName( ) ),
				Email = GetOverrideOrGenerate( nameof( UserEntity.Email ), f => f.Internet.Email( ) ),
				CreatedAt = GetOverrideOrDefault( nameof( UserEntity.CreatedAt ), DateTime.MinValue ),
				UpdatedAt = GetOverrideOrDefault<DateTime?>( nameof( UserEntity.UpdatedAt ), null )
			};
		}
	}
}