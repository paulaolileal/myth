using FluentAssertions;
using Myth.Extensions;

namespace Myth.Commons.Test;

public class TextTests {

	[Fact]
	public void Remove_should_remove_word_from_string( ) {
		// Arrange
		var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		// Act
		var result = text.Remove( "dolor" );

		// Assert
		result.Should( ).NotContain( "dolor" );
	}

	[Fact]
	public void Minify_should_remove_any_spaces( ) {
		// Arrange
		var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit,	 \nsed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		// Act
		var result = text.Minify( );

		// Assert
		result.Should( ).NotContainAll( " ", "\t", "\n", "\r" );
	}

	[Fact]
	public void To_first_lower_should_turn_first_letter_lower( ) {
		// Arrange
		var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		// Act
		var result = text.ToFirstLower( );

		// Assert
		result.First( ).ToString( ).Should( ).BeLowerCased( );
	}

	[Fact]
	public void To_first_upper_should_turn_first_letter_upper( ) {
		// Arrange
		var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		// Act
		var result = text.ToFirstUpper( );

		// Assert
		result.First( ).ToString( ).Should( ).BeUpperCased( );
	}

	[Fact]
	public void Get_string_between_should_get_a_word( ) {
		// Arrange
		var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		// Act
		var result = text.GetStringBetween( 'c', 'r' );

		// Assert
		result.Should( ).Be( "onsectetu" );
	}

	[Fact]
	public void Get_word_contains_should_get_a_word( ) {
		// Arrange
		var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		// Act
		var result = text.GetWordThatContains( "sect" );

		// Assert
		result.Should( ).Be( "consectetur" );
	}

	[Fact]
	public void Get_word_before_should_get_a_word( ) {
		// Arrange
		var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		// Act
		var result = text.GetWordBefore( "sit" );

		// Assert
		result.Should( ).Be( "dolor" );
	}

	[Fact]
	public void Get_word_after_should_get_a_word( ) {
		// Arrange
		var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		// Act
		var result = text.GetWordAfter( "consectetur" );

		// Assert
		result.Should( ).Be( "adipiscing" );
	}

	[Fact]
	public void Contains_any_of_should_get_a_bool( ) {
		// Arrange
		var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		// Act
		var result = text.ContainsAnyOf( "consectetur" );

		// Assert
		result.Should( ).BeTrue( );
	}

	[Fact]
	public void Starts_with_any_of_should_get_a_bool( ) {
		// Arrange
		var text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		// Act
		var result = text.StartsWithAnyOf( "Lorem" );

		// Assert
		result.Should( ).BeTrue( );
	}
}