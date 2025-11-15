using FluentAssertions;
using Myth.Testing.Test.Services;

namespace Myth.Testing.Test;

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
