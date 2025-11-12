using FluentAssertions;
using Myth.Commons.Test.Models;
using Myth.Constants;
using Myth.Exceptions;
using Myth.Extensions;

namespace Myth.Commons.Test;

public class JsonTests {

	[Fact]
	public void To_json_should_return_string_from_object( ) {
		// Arrange
		var testObject = new {
			prop1 = "Ok",
			prop2 = true,
			prop3 = -1,
			prop4 = new {
				prop5 = "Yes"
			}
		};

		// Act
		var result = testObject.ToJson( x => x
			.Minify( )
			.IgnoreNull( )
		 );

		// Assert
		result.Should( ).NotBeEmpty( );
		result.Should( ).Be( "{\"prop1\":\"Ok\",\"prop2\":true,\"prop3\":-1,\"prop4\":{\"prop5\":\"Yes\"}}" );
	}

	[Fact]
	public void To_json_snake_case_should_return_string_from_object( ) {
		// Arrange
		var testObject = new {
			prop_one = "Ok",
			prop_two = true
		};

		// Act
		var result = testObject.ToJson( x => x
			.Minify( )
			.UseCaseStrategy( CaseStrategy.SnakeCase )
		 );

		// Assert
		result.Should( ).NotBeEmpty( );
		result.Should( ).Be( "{\"prop_one\":\"Ok\",\"prop_two\":true}" );
	}

	[Fact]
	public void From_json_should_create_type_from_string( ) {
		// Arrange
		var json = "{\"prop1\":\"Ok\",\"prop2\":true,\"prop3\":-1,\"prop4\":{\"prop5\":\"Yes\"}}";

		// Act
		var result = json.FromJson<object>( );

		// Assert
		result.Should( ).NotBeNull( );
	}

	[Fact]
	public void From_json_should_throw_exception_on_non_json( ) {
		// Arrange
		var json = "+-=";

		// Act
		var action = ( ) => json.FromJson<object>( );

		// Assert
		action.Should( ).Throw<JsonParsingException>( );
	}

	[Fact]
	public void Json_should_use_global_settings( ) {
		// Arrange
		JsonExtensions.Configure( conf => conf
			.Minify( )
			.UseInterfaceConverter<ITestModel, TestModel>( ) );

		var testObject = new TestModel {
			Id = 1,
			Name = "test",
		};

		var jsonObject = testObject.ToJson( );

		// Act
		var result = jsonObject.FromJson<ITestModel>( );

		// Assert
		result.Should( ).NotBeNull( );
		result!.Id.Should( ).Be( testObject.Id );
		result.Name.Should( ).Be( testObject.Name );
	}

	[Theory]
	[InlineData( "{\"test\":\"value\"}" )] // Object JSON
	[InlineData( "[1,2,3]" )] // Array JSON
	[InlineData( "\"hello\"" )] // String JSON
	[InlineData( "null" )] // null JSON
	[InlineData( "true" )] // Boolean JSON
	[InlineData( "false" )] // Boolean JSON
	[InlineData( "42" )] // Number JSON
	[InlineData( "3.14" )] // Decimal JSON
	public void IsValidJson_should_return_true_for_valid_json( string json ) {
		// Act
		var result = json.IsValidJson( );

		// Assert
		result.Should( ).BeTrue( );
	}

	[Theory]
	[InlineData( "" )] // Empty string
	[InlineData( "   " )] // Whitespace only
	[InlineData( null )] // null
	[InlineData( "<html>" )] // HTML
	[InlineData( "plain text" )] // Plain text
	[InlineData( "{invalid}" )] // Invalid JSON structure
	[InlineData( "{ test }" )] // Malformed JSON
	public void IsValidJson_should_return_false_for_invalid_json( string? json ) {
		// Act
		var result = json.IsValidJson( );

		// Assert
		result.Should( ).BeFalse( );
	}

	[Fact]
	public void SafeFromJson_should_return_object_for_valid_json( ) {
		// Arrange
		var json = "{\"id\":1,\"name\":\"test\"}";

		// Act
		var result = json.SafeFromJson<TestModel>( );

		// Assert
		result.Should( ).NotBeNull( );
		result!.Id.Should( ).Be( 1 );
		result.Name.Should( ).Be( "test" );
	}

	[Fact]
	public void SafeFromJson_should_return_null_for_html_content( ) {
		// Arrange
		var htmlContent = "<html><body>Error 404</body></html>";

		// Act
		var result = htmlContent.SafeFromJson<TestModel>( );

		// Assert
		result.Should( ).BeNull( );
	}

	[Fact]
	public void SafeFromJson_should_return_null_for_empty_content( ) {
		// Arrange
		var content = "";

		// Act
		var result = content.SafeFromJson<TestModel>( );

		// Assert
		result.Should( ).BeNull( );
	}

	[Fact]
	public void SafeFromJson_should_return_null_for_plain_text( ) {
		// Arrange
		var content = "This is plain text, not JSON";

		// Act
		var result = content.SafeFromJson<TestModel>( );

		// Assert
		result.Should( ).BeNull( );
	}

	[Fact]
	public void FromJsonOrThrow_should_return_object_for_valid_json( ) {
		// Arrange
		var json = "{\"id\":1,\"name\":\"test\"}";

		// Act
		var result = json.FromJsonOrThrow<TestModel>( System.Net.HttpStatusCode.OK, "application/json" );

		// Assert
		result.Should( ).NotBeNull( );
		result!.Id.Should( ).Be( 1 );
		result.Name.Should( ).Be( "test" );
	}

	[Fact]
	public void FromJsonOrThrow_should_throw_InvalidJsonResponseException_for_html_content( ) {
		// Arrange
		var htmlContent = "<html><body>Error 404</body></html>";

		// Act
		var action = ( ) => htmlContent.FromJsonOrThrow<TestModel>( System.Net.HttpStatusCode.NotFound, "text/html" );

		// Assert
		var exception = action.Should( ).Throw<InvalidJsonResponseException>( ).Which;
		exception.StatusCode.Should( ).Be( System.Net.HttpStatusCode.NotFound );
		exception.RawContent.Should( ).Be( htmlContent );
		exception.ContentType.Should( ).Be( "text/html" );
		exception.Message.Should( ).Contain( "404 NotFound" );
		exception.Message.Should( ).Contain( "text/html" );
		exception.Message.Should( ).Contain( "<html><body>Error 404</body></html>" );
	}

	[Fact]
	public void FromJsonOrThrow_should_return_default_for_empty_204_content( ) {
		// Arrange
		var content = "";

		// Act
		var result = content.FromJsonOrThrow<TestModel>( System.Net.HttpStatusCode.NoContent );

		// Assert
		result.Should( ).BeNull( );
	}

	[Fact]
	public void FromJsonOrThrow_should_return_empty_object_for_empty_dynamic_content( ) {
		// Arrange
		var content = "";

		// Act & Assert - should not throw
		var result = content.FromJsonOrThrow<dynamic>( System.Net.HttpStatusCode.NoContent );

		( ( object )result ).Should( ).NotBeNull( );
	}

	[Fact]
	public void FromJsonOrThrow_should_throw_InvalidJsonResponseException_for_empty_error_content( ) {
		// Arrange
		var content = "";

		// Act
		var action = ( ) => content.FromJsonOrThrow<TestModel>( System.Net.HttpStatusCode.BadRequest );

		// Assert
		var exception = action.Should( ).Throw<InvalidJsonResponseException>( ).Which;
		exception.StatusCode.Should( ).Be( System.Net.HttpStatusCode.BadRequest );
		exception.RawContent.Should( ).Be( "" );
		exception.ContentType.Should( ).BeNull( );
		exception.Message.Should( ).Contain( "400 BadRequest" );
	}

	[Fact]
	public void FromJsonOrThrow_should_throw_InvalidJsonResponseException_for_malformed_json( ) {
		// Arrange
		var malformedJson = "{\"id\":1,\"name\":}"; // Missing value after colon

		// Act
		var action = ( ) => malformedJson.FromJsonOrThrow<TestModel>( System.Net.HttpStatusCode.OK, "application/json" );

		// Assert
		var exception = action.Should( ).Throw<InvalidJsonResponseException>( ).Which;
		exception.StatusCode.Should( ).Be( System.Net.HttpStatusCode.OK );
		exception.RawContent.Should( ).Be( malformedJson );
		exception.ContentType.Should( ).Be( "application/json" );
		exception.Message.Should( ).Contain( "200 OK" );
	}
}
