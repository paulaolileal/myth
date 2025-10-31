using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Testing.Fixtures;
using Xunit;

namespace Myth.Testing.Examples {

	/// <summary>
	/// Example fixture for sharing services across multiple test classes
	/// </summary>
	public class SharedServiceFixture : TestFixture {

		/// <summary>
		/// Configure shared services
		/// </summary>
		/// <param name="services">Service collection</param>
		protected override void ConfigureServices( IServiceCollection services ) {
			// Add shared services that are expensive to create
			services.AddSingleton<UserService>( );
			services.AddSingleton<ExpensiveService>( );

			base.ConfigureServices( services );
		}
	}

	/// <summary>
	/// Example of expensive service that should be shared
	/// </summary>
	public class ExpensiveService {
		private readonly DateTime _createdAt;

		/// <summary>
		/// Initialize expensive service
		/// </summary>
		public ExpensiveService( ) {
			// Simulate expensive initialization
			_createdAt = DateTime.UtcNow;
			Thread.Sleep( 100 ); // Simulate slow initialization
		}

		/// <summary>
		/// Get service creation time
		/// </summary>
		public DateTime CreatedAt => _createdAt;

		/// <summary>
		/// Expensive operation
		/// </summary>
		/// <param name="value">Input value</param>
		/// <returns>Processed value</returns>
		public async Task<string> ProcessAsync( string value ) {
			// Simulate expensive processing
			await Task.Delay( 50 );

			return $"Processed: {value}";
		}
	}

	/// <summary>
	/// First test class using shared fixture
	/// </summary>
	public class FirstSharedServiceTests : IClassFixture<SharedServiceFixture> {
		private readonly SharedServiceFixture _fixture;

		/// <summary>
		/// Initialize with shared fixture
		/// </summary>
		/// <param name="fixture">The shared fixture</param>
		public FirstSharedServiceTests( SharedServiceFixture fixture ) {
			_fixture = fixture;
		}

		/// <summary>
		/// Test using shared UserService
		/// </summary>
		[Fact]
		public async Task UserService_FromSharedFixture_ShouldWork( ) {
			// Arrange
			var userService = _fixture.GetRequiredService<UserService>( );
			var user = new User {
				Name = _fixture.Faker.Name.FullName( ),
				Email = _fixture.Faker.Internet.Email( )
			};

			// Act
			var result = await userService.CreateUserAsync( user );

			// Assert
			result.Should( ).NotBeNull( );
			result.Id.Should( ).NotBeEmpty( );
		}

		/// <summary>
		/// Test using shared ExpensiveService
		/// </summary>
		[Fact]
		public async Task ExpensiveService_FromSharedFixture_ShouldReuseInstance( ) {
			// Arrange
			var expensiveService = _fixture.GetRequiredService<ExpensiveService>( );

			// Act
			var result = await expensiveService.ProcessAsync( "test value" );

			// Assert
			result.Should( ).Be( "Processed: test value" );
			expensiveService.CreatedAt.Should( ).BeBefore( DateTime.UtcNow );
		}
	}

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

	/// <summary>
	/// Example of collection fixture for sharing across multiple test classes
	/// </summary>
	[CollectionDefinition( "Shared Service Collection" )]
	public class SharedServiceCollection : ICollectionFixture<SharedServiceFixture> {
		// This class has no code, and is never created. Its purpose is simply
		// to be the place to apply [CollectionDefinition] and all the
		// ICollectionFixture<> interfaces.
	}

	/// <summary>
	/// Test class using collection fixture
	/// </summary>
	[Collection( "Shared Service Collection" )]
	public class CollectionBasedTests {
		private readonly SharedServiceFixture _fixture;

		/// <summary>
		/// Initialize with collection fixture
		/// </summary>
		/// <param name="fixture">The shared fixture</param>
		public CollectionBasedTests( SharedServiceFixture fixture ) {
			_fixture = fixture;
		}

		/// <summary>
		/// Test using collection-shared services
		/// </summary>
		[Fact]
		public void ExpensiveService_FromCollection_ShouldBeShared( ) {
			// Arrange
			var expensiveService = _fixture.GetRequiredService<ExpensiveService>( );

			// Act & Assert
			expensiveService.Should( ).NotBeNull( );
		}
	}
}