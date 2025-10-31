using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Testing.Builders;
using Myth.Testing.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Testing.Examples {
	/// <summary>
	/// Example test class demonstrating the use of BaseTests with xUnit
	/// </summary>
	public class UserServiceTests : BaseTests {
		private readonly UserService _userService;

		/// <summary>
		/// Initialize test with UserService
		/// </summary>
		public UserServiceTests( ) {
			// Add services to the test container
			AddService<UserService>( new UserService( ) );

			// Get the service instance
			_userService = GetRequiredService<UserService>( );
		}

		/// <summary>
		/// Test creating a valid user
		/// </summary>
		[Fact]
		public async Task CreateUserAsync_WithValidUser_ShouldCreateUser( ) {
			// Arrange
			var user = new UserBuilder( _faker )
				.WithName( "John Doe" )
				.WithEmail( "john.doe@example.com" )
				.Build( );

			// Act
			var result = await _userService.CreateUserAsync( user );

			// Assert
			result.Should( ).NotBeNull( );
			result.Id.Should( ).NotBeEmpty( );
			result.Name.Should( ).Be( "John Doe" );
			result.Email.Should( ).Be( "john.doe@example.com" );
			result.CreatedAt.Should( ).BeCloseTo( DateTime.UtcNow, TimeSpan.FromSeconds( 5 ) );
		}

		/// <summary>
		/// Test creating a user with null parameter
		/// </summary>
		[Fact]
		public async Task CreateUserAsync_WithNullUser_ShouldThrowArgumentNullException( ) {
			// Act & Assert
			await TestExtensions.AssertThrowsAsync<ArgumentNullException>(
				( ) => _userService.CreateUserAsync( null! )
			);
		}

		/// <summary>
		/// Test creating a user with empty email
		/// </summary>
		[Fact]
		public async Task CreateUserAsync_WithEmptyEmail_ShouldThrowArgumentException( ) {
			// Arrange
			var user = new UserBuilder( _faker )
				.WithEmail( string.Empty )
				.Build( );

			// Act & Assert
			var exception = await TestExtensions.AssertThrowsAsync<ArgumentException>(
				( ) => _userService.CreateUserAsync( user )
			);

			exception.Message.Should( ).Contain( "Email is required" );
		}

		/// <summary>
		/// Test creating duplicate users
		/// </summary>
		[Fact]
		public async Task CreateUserAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException( ) {
			// Arrange
			var email = _faker.Internet.Email( );
			var user1 = new UserBuilder( _faker ).WithEmail( email ).Build( );
			var user2 = new UserBuilder( _faker ).WithEmail( email ).Build( );

			await _userService.CreateUserAsync( user1 );

			// Act & Assert
			var exception = await TestExtensions.AssertThrowsAsync<InvalidOperationException>(
				( ) => _userService.CreateUserAsync( user2 )
			);

			exception.Message.Should( ).Contain( "already exists" );
		}

		/// <summary>
		/// Test getting user by email
		/// </summary>
		[Fact]
		public async Task GetUserByEmailAsync_WithExistingEmail_ShouldReturnUser( ) {
			// Arrange
			var user = new UserBuilder( _faker ).Build( );
			await _userService.CreateUserAsync( user );

			// Act
			var result = await _userService.GetUserByEmailAsync( user.Email );

			// Assert
			result.Should( ).NotBeNull( );
			result!.Email.Should( ).Be( user.Email );
			result.Name.Should( ).Be( user.Name );
		}

		/// <summary>
		/// Test getting user by non-existing email
		/// </summary>
		[Fact]
		public async Task GetUserByEmailAsync_WithNonExistingEmail_ShouldReturnNull( ) {
			// Act
			var result = await _userService.GetUserByEmailAsync( "nonexisting@example.com" );

			// Assert
			result.Should( ).BeNull( );
		}

		/// <summary>
		/// Test getting all users
		/// </summary>
		[Fact]
		public async Task GetAllUsersAsync_WithMultipleUsers_ShouldReturnAllUsers( ) {
			// Arrange
			var users = new UserBuilder( _faker ).BuildList( 3 );
			foreach ( var user in users )
				await _userService.CreateUserAsync( user );

			// Act
			var result = await _userService.GetAllUsersAsync( );

			// Assert
			result.Should( ).HaveCount( 3 );
			result.Should( ).Contain( u => users.Any( created => created.Email == u.Email ) );
		}

		/// <summary>
		/// Test with timeout to demonstrate async testing patterns
		/// </summary>
		[Fact]
		public async Task CreateUserAsync_WithTimeout_ShouldCompleteWithinTimeLimit( ) {
			// Arrange
			var user = new UserBuilder( _faker ).Build( );

			// Act & Assert
			await TestExtensions.WithTimeoutAsync(
				( ) => _userService.CreateUserAsync( user ),
				TimeSpan.FromSeconds( 1 )
			);
		}

		/// <summary>
		/// Test using scoped services
		/// </summary>
		[Fact]
		public async Task CreateUserAsync_WithScopedService_ShouldWorkCorrectly( ) {
			// Arrange
			using var scope = CreateScope( );
			var scopedUserService = scope.ServiceProvider.GetRequiredService<UserService>( );
			var user = new UserBuilder( _faker ).Build( );

			// Act
			var result = await scopedUserService.CreateUserAsync( user );

			// Assert
			result.Should( ).NotBeNull( );
			result.Id.Should( ).NotBeEmpty( );
		}
	}

	/// <summary>
	/// Example builder for User objects
	/// </summary>
	public class UserBuilder : TestDataBuilder<User, UserBuilder> {
		/// <summary>
		/// Initialize UserBuilder with Faker
		/// </summary>
		/// <param name="faker">The Faker instance</param>
		public UserBuilder( Bogus.Faker faker ) : base( faker ) { }

		/// <summary>
		/// Set the user name
		/// </summary>
		/// <param name="name">The user name</param>
		/// <returns>The builder for fluent chaining</returns>
		public UserBuilder WithName( string name ) {
			return With( nameof( User.Name ), name );
		}

		/// <summary>
		/// Set the user email
		/// </summary>
		/// <param name="email">The user email</param>
		/// <returns>The builder for fluent chaining</returns>
		public UserBuilder WithEmail( string email ) {
			return With( nameof( User.Email ), email );
		}

		/// <summary>
		/// Set the user ID
		/// </summary>
		/// <param name="id">The user ID</param>
		/// <returns>The builder for fluent chaining</returns>
		public UserBuilder WithId( Guid id ) {
			return With( nameof( User.Id ), id );
		}

		/// <summary>
		/// Build the User object
		/// </summary>
		/// <returns>A User object with generated or overridden values</returns>
		public override User Build( ) {
			return new User {
				Id = GetOverrideOrDefault( nameof( User.Id ), Guid.Empty ),
				Name = GetOverrideOrGenerate( nameof( User.Name ), f => f.Name.FullName( ) ),
				Email = GetOverrideOrGenerate( nameof( User.Email ), f => f.Internet.Email( ) ),
				CreatedAt = GetOverrideOrDefault( nameof( User.CreatedAt ), DateTime.MinValue ),
				UpdatedAt = GetOverrideOrDefault<DateTime?>( nameof( User.UpdatedAt ), null )
			};
		}
	}
}