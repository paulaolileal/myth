using FluentAssertions;
using Myth.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Commons.Test {
	public class EnumerableTests {

		public static IEnumerable<object[ ]> Data( ) {
			yield return new object[ ] { new List<string> { "a", "b", "c", "d" } };
			yield return new object[ ] { new List<string> { "a", "1", "c", "2" } };
			yield return new object[ ] { new List<string> { "_", "1", "", "A" } };
		}

		[Theory]
		[MemberData( nameof( Data ) )]
		public void To_string_with_separator_should_return_string( IEnumerable<string> list ) {
			// Act
			var result = list.ToStringWithSeparator( );

			// Assert
			result.Should( ).NotBeEmpty( );
		}

		[Fact]
		public void To_string_with_separator_should_return_empty_on_empty_param() {
			// Arrange
			var list = new List<string>();

			// Act
			var result = list.ToStringWithSeparator( );

			// Assert
			result.Should( ).BeEmpty( );
		}
	}
}
