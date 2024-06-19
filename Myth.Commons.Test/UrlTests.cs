using FluentAssertions;
using Myth.Extensions;

namespace Myth.Commons.Test {

	public class UrlTests {

		[Theory]
		[InlineData( "https://localhost:5001?teste=@ok" )]
		[InlineData( true )]
		public void Encode_should_encode_an_url( object value ) {
			// Act
			var result = value.EncodeAsUrl( );

			// Assert
			result?.ToString( ).Should( ).NotContainAll( ":", "/", "?", "@" );
		}
	}
}