using FluentAssertions;
using Myth.Testing.Test.Services;

namespace Myth.Testing.Test;

/// <summary>
/// Test class using collection fixture
/// </summary>
/// <remarks>
/// Initialize with collection fixture
/// </remarks>
/// <param name="fixture">The shared fixture</param>
[Collection( "Shared Service Collection" )]
public class CollectionBasedTests( SharedServiceFixture fixture ) {
	private readonly SharedServiceFixture _fixture = fixture;

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
