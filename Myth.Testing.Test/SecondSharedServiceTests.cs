using FluentAssertions;
using Myth.Testing.Test.Models;
using Myth.Testing.Test.Services;

namespace Myth.Testing.Test {

	/// <summary>
	/// Second test class using the same shared fixture
	/// </summary>
	public class SecondSharedServiceTests : IClassFixture<SharedServiceFixture> {
		private readonly SharedServiceFixture _fixture;

		/// <summary>
		/// Initialize with shared fixture
		/// </summary>
		/// <param name="fixture">The shared fixture</param>
		public SecondSharedServiceTests( SharedServiceFixture fixture ) {
			_fixture = fixture;
		}

		/// <summary>
		/// Test using the same shared ExpensiveService instance
		/// </summary>
		[Fact]
		public void ExpensiveService_ShouldBeSharedBetweenTestClasses( ) {
			// Arrange
			var expensiveService = _fixture.GetRequiredService<ExpensiveService>( );

			// Act & Assert
			// This service should be the same instance as in FirstSharedServiceTests
			expensiveService.Should( ).NotBeNull( );
			expensiveService.CreatedAt.Should( ).BeBefore( DateTime.UtcNow );
		}

		/// <summary>
		/// Test demonstrating shared Faker instance
		/// </summary>
		[Fact]
		public void Faker_ShouldBeAvailableFromFixture( ) {
			// Act
			var name = _fixture.Faker.Name.FullName( );
			var email = _fixture.Faker.Internet.Email( );

			// Assert
			name.Should( ).NotBeNullOrEmpty( );
			email.Should( ).NotBeNullOrEmpty( );
			email.Should( ).Contain( "@" );
		}

		/// <summary>
		/// Test generating multiple users with shared services
		/// </summary>
		[Fact]
		public async Task CreateMultipleUsers_WithSharedServices_ShouldWork( ) {
			// Arrange
			var userService = _fixture.GetRequiredService<UserService>( );
			var users = new List<User>( );

			for ( int i = 0; i < 3; i++ ) {
				users.Add( new User {
					Name = _fixture.Faker.Name.FullName( ),
					Email = _fixture.Faker.Internet.Email( )
				} );
			}

			// Act
			var results = new List<User>( );
			foreach ( var user in users ) {
				var result = await userService.CreateUserAsync( user );
				results.Add( result );
			}

			// Assert
			results.Should( ).HaveCount( 3 );
			results.Should( ).OnlyContain( u => u.Id != Guid.Empty );
			results.Should( ).OnlyContain( u => !string.IsNullOrEmpty( u.Email ) );
		}
	}
}