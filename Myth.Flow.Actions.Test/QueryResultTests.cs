using FluentAssertions;
using Myth.Models;

namespace Myth.Flow.Actions.Test;

public class QueryResultTests {

	[Fact]
	public void Success_ShouldCreateSuccessResult( ) {
		// Arrange & Act
		var result = QueryResult<string>.Success( "data" );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.Data.Should( ).Be( "data" );
		result.FromCache.Should( ).BeFalse( );
	}

	[Fact]
	public void Success_WithFromCache_ShouldIndicateCacheHit( ) {
		// Arrange & Act
		var result = QueryResult<string>.Success( "cached", fromCache: true );

		// Assert
		result.IsSuccess.Should( ).BeTrue( );
		result.FromCache.Should( ).BeTrue( );
	}

	[Fact]
	public void Failure_ShouldCreateFailureResult( ) {
		// Arrange & Act
		var result = QueryResult<string>.Failure( "Not found" );

		// Assert
		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).Be( "Not found" );
		result.FromCache.Should( ).BeFalse( );
	}
}
