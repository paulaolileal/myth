using System.Net;
using FluentAssertions;
using Myth.Models;

namespace Myth.Flow.Actions.Test;

public class CommandResultTests {

	[Fact]
	public void Success_ShouldCreateSuccessResult( ) {
		var result = CommandResult.Success( );

		result.IsSuccess.Should( ).BeTrue( );
		result.IsFailure.Should( ).BeFalse( );
		result.ErrorMessage.Should( ).BeNull( );
		result.Exception.Should( ).BeNull( );
	}

	[Fact]
	public void Success_ShouldHaveOkStatusCode( ) {
		var result = CommandResult.Success( );

		result.StatusCode.Should( ).Be( HttpStatusCode.OK );
	}

	[Fact]
	public void Success_WithMetadata_ShouldIncludeMetadata( ) {
		var metadata = new Dictionary<string, object> { [ "key" ] = "value" };

		var result = CommandResult.Success( metadata );

		result.IsSuccess.Should( ).BeTrue( );
		result.Metadata.Should( ).ContainKey( "key" );
		result.Metadata![ "key" ].Should( ).Be( "value" );
	}

	[Fact]
	public void Failure_ShouldCreateFailureResult( ) {
		var result = CommandResult.Failure( "Error occurred" );

		result.IsSuccess.Should( ).BeFalse( );
		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).Be( "Error occurred" );
	}

	[Fact]
	public void Failure_ShouldDefaultToBadRequestStatusCode( ) {
		var result = CommandResult.Failure( "Error occurred" );

		result.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );
	}

	[Fact]
	public void Failure_WithExplicitStatusCode_ShouldUseProvidedCode( ) {
		var result = CommandResult.Failure( "Payment required", HttpStatusCode.PaymentRequired );

		result.IsFailure.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( HttpStatusCode.PaymentRequired );
		result.ErrorMessage.Should( ).Be( "Payment required" );
	}

	[Fact]
	public void Failure_WithException_ShouldIncludeException( ) {
		var exception = new InvalidOperationException( "Test error" );

		var result = CommandResult.Failure( "Error occurred", exception );

		result.IsFailure.Should( ).BeTrue( );
		result.Exception.Should( ).Be( exception );
		result.ErrorMessage.Should( ).Be( "Error occurred" );
	}

	[Fact]
	public void Failure_WithMetadata_ShouldIncludeMetadata( ) {
		var metadata = new Dictionary<string, object> { [ "errorCode" ] = 500 };

		var result = CommandResult.Failure( "Error", metadata: metadata );

		result.IsFailure.Should( ).BeTrue( );
		result.Metadata.Should( ).ContainKey( "errorCode" );
	}

	[Theory]
	[InlineData( HttpStatusCode.NotFound )]
	[InlineData( HttpStatusCode.Forbidden )]
	[InlineData( HttpStatusCode.Unauthorized )]
	[InlineData( HttpStatusCode.PaymentRequired )]
	[InlineData( HttpStatusCode.Conflict )]
	[InlineData( HttpStatusCode.UnprocessableEntity )]
	public void SemanticFactories_ShouldProduceExpectedStatusCode( HttpStatusCode expectedCode ) {
		CommandResult result = expectedCode switch {
			HttpStatusCode.NotFound => CommandResult.NotFound( "not found" ),
			HttpStatusCode.Forbidden => CommandResult.Forbidden( ),
			HttpStatusCode.Unauthorized => CommandResult.Unauthorized( ),
			HttpStatusCode.PaymentRequired => CommandResult.PaymentRequired( "payment required" ),
			HttpStatusCode.Conflict => CommandResult.Conflict( "conflict" ),
			HttpStatusCode.UnprocessableEntity => CommandResult.UnprocessableEntity( "unprocessable" ),
			_ => throw new ArgumentOutOfRangeException( )
		};

		result.IsFailure.Should( ).BeTrue( );
		result.StatusCode.Should( ).Be( expectedCode );
	}

	[Fact]
	public void NotFound_ShouldHaveCorrectMessageAndStatusCode( ) {
		var result = CommandResult.NotFound( "Resource not found" );

		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).Be( "Resource not found" );
		result.StatusCode.Should( ).Be( HttpStatusCode.NotFound );
	}

	[Fact]
	public void Forbidden_ShouldHaveDefaultMessageWhenNoneProvided( ) {
		var result = CommandResult.Forbidden( );

		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).NotBeNullOrWhiteSpace( );
		result.StatusCode.Should( ).Be( HttpStatusCode.Forbidden );
	}

	[Fact]
	public void PaymentRequired_ShouldHaveCorrectStatusCode( ) {
		var result = CommandResult.PaymentRequired( "Insufficient credits" );

		result.StatusCode.Should( ).Be( HttpStatusCode.PaymentRequired );
		result.ErrorMessage.Should( ).Be( "Insufficient credits" );
	}

	[Fact]
	public void Conflict_ShouldHaveCorrectStatusCode( ) {
		var result = CommandResult.Conflict( "Resource already exists" );

		result.StatusCode.Should( ).Be( HttpStatusCode.Conflict );
	}
}
