using FluentAssertions;
using Myth.Testing.Test.Models;
using Myth.Testing.Test.Services;

namespace Myth.Testing.Test;

/// <summary>
/// First test class using shared fixture
/// </summary>
/// <remarks>
/// Initialize with shared fixture
/// </remarks>
/// <param name="fixture">The shared fixture</param>
public class FirstSharedServiceTests( SharedServiceFixture fixture ) : IClassFixture<SharedServiceFixture> {
	private readonly SharedServiceFixture _fixture = fixture;

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
