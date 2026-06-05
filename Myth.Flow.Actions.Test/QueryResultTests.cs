using System.Net;
using FluentAssertions;
using Myth.Models;

namespace Myth.Flow.Actions.Test;

public class QueryResultTests {

	[Fact]
	public void Success_ShouldCreateSuccessResult( ) {
		var result = QueryResult<string>.Success( "data" );

		result.IsSuccess.Should( ).BeTrue( );
		result.Data.Should( ).Be( "data" );
		result.FromCache.Should( ).BeFalse( );
	}

	[Fact]
	public void Success_ShouldHaveOkStatusCode( ) {
		var result = QueryResult<string>.Success( "data" );

		result.StatusCode.Should( ).Be( HttpStatusCode.OK );
	}

	[Fact]
	public void Success_WithFromCache_ShouldIndicateCacheHit( ) {
		var result = QueryResult<string>.Success( "cached", fromCache: true );

		result.IsSuccess.Should( ).BeTrue( );
		result.FromCache.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( HttpStatusCode.OK );
	}

	[Fact]
	public void Failure_ShouldCreateFailureResult( ) {
		var result = QueryResult<string>.Failure( "Query failed" );

		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).Be( "Query failed" );
		result.FromCache.Should( ).BeFalse( );
	}

	[Fact]
	public void Failure_ShouldDefaultToBadRequestStatusCode( ) {
		var result = QueryResult<string>.Failure( "Error" );

		result.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );
	}

	[Fact]
	public void Failure_WithExplicitStatusCode_ShouldUseProvidedCode( ) {
		var result = QueryResult<string>.Failure( "Service unavailable", HttpStatusCode.ServiceUnavailable );

		result.IsFailure.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( HttpStatusCode.ServiceUnavailable );
	}

	[Fact]
	public void NotFound_ShouldHaveNotFoundStatusCodeAndNoData( ) {
		var result = QueryResult<string>.NotFound( "Project not found" );

		result.IsFailure.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( HttpStatusCode.NotFound );
		result.ErrorMessage.Should( ).Be( "Project not found" );
		result.Data.Should( ).BeNull( );
		result.FromCache.Should( ).BeFalse( );
	}

	[Fact]
	public void NotFound_WithDefaultMessage_ShouldHaveDefaultMessage( ) {
		var result = QueryResult<string>.NotFound( );

		result.StatusCode.Should( ).Be( HttpStatusCode.NotFound );
		result.ErrorMessage.Should( ).NotBeNullOrWhiteSpace( );
	}

	[Fact]
	public void Forbidden_ShouldHaveForbiddenStatusCodeAndNoData( ) {
		var result = QueryResult<string>.Forbidden( );

		result.IsFailure.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( HttpStatusCode.Forbidden );
		result.Data.Should( ).BeNull( );
	}

	[Fact]
	public void Unauthorized_ShouldHaveUnauthorizedStatusCodeAndNoData( ) {
		var result = QueryResult<int>.Unauthorized( );

		result.IsFailure.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( HttpStatusCode.Unauthorized );
		result.Data.Should( ).Be( default );
	}

	[Theory]
	[InlineData( HttpStatusCode.NotFound )]
	[InlineData( HttpStatusCode.Forbidden )]
	[InlineData( HttpStatusCode.Unauthorized )]
	public void SemanticFactories_ShouldProduceExpectedStatusCodeAndBeFailures( HttpStatusCode expectedCode ) {
		QueryResult<string> result = expectedCode switch {
			HttpStatusCode.NotFound => QueryResult<string>.NotFound( "not found" ),
			HttpStatusCode.Forbidden => QueryResult<string>.Forbidden( ),
			HttpStatusCode.Unauthorized => QueryResult<string>.Unauthorized( ),
			_ => throw new ArgumentOutOfRangeException( )
		};

		result.IsFailure.Should( ).BeTrue( );
		result.IsSuccess.Should( ).BeFalse( );
		result.StatusCode.Should( ).Be( expectedCode );
		result.Data.Should( ).BeNull( );
	}

	[Fact]
	public void NotFound_ShouldBePreferredOverSuccessWithNull( ) {
		// NotFound() is explicit about the absent state — Success(null!) lies about success
		var notFoundResult = QueryResult<string>.NotFound( "Entity not found" );
		var dishonestResult = QueryResult<string>.Success( null! );

		notFoundResult.IsFailure.Should( ).BeTrue( );
		notFoundResult.StatusCode.Should( ).Be( HttpStatusCode.NotFound );

		dishonestResult.IsSuccess.Should( ).BeTrue( ); // misleading — data is null
	}
}
