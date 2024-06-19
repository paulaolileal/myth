using FluentAssertions;
using Myth.Constants;
using Myth.Exceptions;
using Myth.Extensions;

namespace Myth.Commons.Test {

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
				.IgnoreNull( ) );

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
				.SetCaseAs( CaseStrategy.SnakeCase ) );

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
	}
}