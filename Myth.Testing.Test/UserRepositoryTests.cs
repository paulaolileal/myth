using Bogus;
using FluentAssertions;
using Myth.Testing.Repositories;
using Myth.Testing.Test.Models;
using Myth.Testing.Test.Repositories;

namespace Myth.Testing.Test {

	/// <summary>
	/// Example test class demonstrating the use of BaseDatabaseTests with Entity Framework
	/// </summary>
	public class UserRepositoryTests : BaseDatabaseTests<UserDbContext> {
		private readonly UserRepository _repository;
		private readonly Faker<User> _userFaker;

		/// <summary>
		/// Initialize test with UserRepository and DbContext
		/// </summary>
		public UserRepositoryTests( ) {
			// Add services to the test container
			AddService<UserRepository, UserRepository>( );

			_userFaker = new Faker<User>( )
				.RuleFor( u => u.CreatedAt, f => f.Date.Past( ) )
				.RuleFor( u => u.UpdatedAt, f => f.Date.Recent( ) )
				.RuleFor( u => u.Name, f => f.Name.FullName( ) )
				.RuleFor( u => u.Email, f => f.Internet.Email( ) );

			// Get the repository instance
			_repository = GetRequiredService<UserRepository>( );
		}

		/// <summary>
		/// Test creating a user with database
		/// </summary>
		[Fact]
		public async Task CreateAsync_WithValidUser_ShouldCreateUserInDatabase( ) {
			// Arrange
			var userEntity = _userFaker.Generate( );

			// Act
			var result = await _repository.CreateAsync( userEntity );

			// Assert
			result.Should( ).NotBeNull( );
			result.Id.Should( ).NotBeEmpty( );

			// Verify in database
			var context = GetContext( );
			var dbUser = await context.Users.FindAsync( result.Id );
			dbUser.Should( ).NotBeNull( );
			dbUser!.Email.Should( ).Be( userEntity.Email );
		}

		/// <summary>
		/// Test getting user by ID
		/// </summary>
		[Fact]
		public async Task GetByIdAsync_WithExistingId_ShouldReturnUser( ) {
			// Arrange
			var userEntity = _userFaker.Generate( );

			var created = await _repository.CreateAsync( userEntity );

			// Act
			var result = await _repository.GetByIdAsync( created.Id );

			// Assert
			result.Should( ).NotBeNull( );
			result!.Id.Should( ).Be( created.Id );
			result.Email.Should( ).Be( created.Email );
		}

		/// <summary>
		/// Test getting user by non-existing ID
		/// </summary>
		[Fact]
		public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull( ) {
			// Act
			var result = await _repository.GetByIdAsync( Guid.NewGuid( ) );

			// Assert
			result.Should( ).BeNull( );
		}

		/// <summary>
		/// Test getting user by email
		/// </summary>
		[Fact]
		public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser( ) {
			// Arrange
			var userEntity = _userFaker.Generate( );
			userEntity.Email = "test@example.com";

			await _repository.CreateAsync( userEntity );

			// Act
			var result = await _repository.GetByEmailAsync( "test@example.com" );

			// Assert
			result.Should( ).NotBeNull( );
			result!.Email.Should( ).Be( "test@example.com" );
		}

		/// <summary>
		/// Test getting all users
		/// </summary>
		[Fact]
		public async Task GetAllAsync_WithMultipleUsers_ShouldReturnAllUsers( ) {
			// Arrange
			var users = _userFaker.Generate( 3 );
			foreach ( var user in users )
				await _repository.CreateAsync( user );

			// Act
			var result = await _repository.GetAllAsync( );

			// Assert
			result.Should( ).HaveCount( 3 );
			result.Should( ).Contain( u => users.Any( created => created.Email == u.Email ) );
		}

		/// <summary>
		/// Test updating user
		/// </summary>
		[Fact]
		public async Task UpdateAsync_WithValidUser_ShouldUpdateUserInDatabase( ) {
			// Arrange
			var userEntity = _userFaker.Generate( );

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
		}

		/// <summary>
		/// Test deleting user
		/// </summary>
		[Fact]
		public async Task DeleteAsync_WithExistingId_ShouldDeleteUserFromDatabase( ) {
			// Arrange
			var userEntity = _userFaker.Generate( );

			var created = await _repository.CreateAsync( userEntity );

			// Act
			var result = await _repository.DeleteAsync( created.Id );

			// Assert
			result.Should( ).BeTrue( );

			// Verify deletion
			var dbUser = await _repository.GetByIdAsync( created.Id );
			dbUser.Should( ).BeNull( );
		}

		/// <summary>
		/// Test error handling scenario
		/// </summary>
		[Fact]
		public async Task CreateAsync_WithNullUser_ShouldThrowArgumentNullException( ) {
			// Act & Assert
			var task = ( ) => _repository.CreateAsync( null! );

			await task.Should( ).ThrowAsync<ArgumentNullException>( );

			// Verify no users were created
			var allUsers = await _repository.GetAllAsync( );
			allUsers.Should( ).BeEmpty( );
		}
	}
}