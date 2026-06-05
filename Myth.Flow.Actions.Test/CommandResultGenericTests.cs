using System.Net;
using FluentAssertions;
using Myth.Models;

namespace Myth.Flow.Actions.Test;

public class CommandResultGenericTests {

	[Fact]
	public void Success_ShouldCreateSuccessResultWithData( ) {
		var result = CommandResult<int>.Success( 42 );

		result.IsSuccess.Should( ).BeTrue( );
		result.IsFailure.Should( ).BeFalse( );
		result.Data.Should( ).Be( 42 );
		result.ErrorMessage.Should( ).BeNull( );
	}

	[Fact]
	public void Success_ShouldHaveOkStatusCode( ) {
		var result = CommandResult<string>.Success( "response" );

		result.StatusCode.Should( ).Be( HttpStatusCode.OK );
	}

	[Fact]
	public void Success_WithMetadata_ShouldIncludeDataAndMetadata( ) {
		var metadata = new Dictionary<string, object> { [ "source" ] = "test" };

		var result = CommandResult<string>.Success( "data", metadata );

		result.IsSuccess.Should( ).BeTrue( );
		result.Data.Should( ).Be( "data" );
		result.Metadata.Should( ).ContainKey( "source" );
	}

	[Fact]
	public void Failure_ShouldCreateFailureResult( ) {
		var result = CommandResult<int>.Failure( "Error" );

		result.IsFailure.Should( ).BeTrue( );
		result.IsSuccess.Should( ).BeFalse( );
		result.Data.Should( ).Be( 0 );
		result.ErrorMessage.Should( ).Be( "Error" );
	}

	[Fact]
	public void Failure_ShouldDefaultToBadRequestStatusCode( ) {
		var result = CommandResult<int>.Failure( "Error" );

		result.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );
	}

	[Fact]
	public void Failure_WithExplicitStatusCode_ShouldUseProvidedCode( ) {
		var result = CommandResult<int>.Failure( "Conflict", HttpStatusCode.Conflict );

		result.IsFailure.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( HttpStatusCode.Conflict );
	}

	[Fact]
	public void Failure_WithException_ShouldIncludeException( ) {
		var exception = new ArgumentException( "Invalid argument" );

		var result = CommandResult<string>.Failure( "Failed", exception );

		result.IsFailure.Should( ).BeTrue( );
		result.Exception.Should( ).Be( exception );
		result.Data.Should( ).BeNull( );
	}

	[Theory]
	[InlineData( HttpStatusCode.NotFound )]
	[InlineData( HttpStatusCode.Forbidden )]
	[InlineData( HttpStatusCode.Unauthorized )]
	[InlineData( HttpStatusCode.PaymentRequired )]
	[InlineData( HttpStatusCode.Conflict )]
	[InlineData( HttpStatusCode.UnprocessableEntity )]
	public void SemanticFactories_ShouldProduceExpectedStatusCode( HttpStatusCode expectedCode ) {
		CommandResult<int> result = expectedCode switch {
			HttpStatusCode.NotFound => CommandResult<int>.NotFound( "not found" ),
			HttpStatusCode.Forbidden => CommandResult<int>.Forbidden( ),
			HttpStatusCode.Unauthorized => CommandResult<int>.Unauthorized( ),
			HttpStatusCode.PaymentRequired => CommandResult<int>.PaymentRequired( "payment required" ),
			HttpStatusCode.Conflict => CommandResult<int>.Conflict( "conflict" ),
			HttpStatusCode.UnprocessableEntity => CommandResult<int>.UnprocessableEntity( "unprocessable" ),
			_ => throw new ArgumentOutOfRangeException( )
		};

		result.IsFailure.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( expectedCode );
		result.Data.Should( ).Be( default );
	}

	[Fact]
	public void Success_WithComplexType_ShouldStoreCorrectly( ) {
		var data = new { Id = 1, Name = "Test" };

		var result = CommandResult<object>.Success( data );

		result.IsSuccess.Should( ).BeTrue( );
		result.Data.Should( ).Be( data );
		result.StatusCode.Should( ).Be( HttpStatusCode.OK );
	}
}
