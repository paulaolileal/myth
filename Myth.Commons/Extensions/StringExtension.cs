using System.Text.RegularExpressions;

namespace Myth.Extensions;

public static class StringExtension {

	/// <summary>
	/// Removes a word from string
	/// </summary>
	/// <param name="value">The original text</param>
	/// <param name="text">The text to removes</param>
	/// <returns>A text without the word</returns>
	public static string Remove( this string value, string text ) => value.Replace( text, string.Empty );

	/// <summary>
	/// Minify a text removing all empty spaces
	/// </summary>
	/// <param name="text">The original text</param>
	/// <returns>A text in a single line</returns>
	public static string Minify( this string text ) => Regex.Replace( text, @"\s+", "" );

	/// <summary>
	/// Changes the first letter of string into lowecase
	/// </summary>
	/// <param name="text">The original text</param>
	/// <returns>A text with the first letter as lowercase</returns>
	public static string ToFirstLower( this string text ) {
		if ( !string.IsNullOrEmpty( text ) && text.Any( ) )
			return string.Concat(
				text.First( ).ToString( ).ToLowerInvariant( ),
				text.AsSpan( 1 ) );

		return string.Empty;
	}

	/// <summary>
	/// Changes the first letter of string into upercase
	/// </summary>
	/// <param name="text">The original text</param>
	/// <returns>A text with the first letter as upercase</returns>
	public static string ToFirstUpper( this string text ) {
		if ( !string.IsNullOrEmpty( text ) && text.Any( ) )
			return string.Concat(
				text.First( ).ToString( ).ToUpperInvariant( ),
				text.AsSpan( 1 ) );

		return string.Empty;
	}

	/// <summary>
	/// Searchs for a given word in the text
	/// </summary>
	/// <param name="text">The original text</param>
	/// <param name="startCharacter">Initial character</param>
	/// <param name="endCharacter">Final character</param>
	/// <returns>A string found</returns>
	public static string GetStringBetween( this string text, char startCharacter, char? endCharacter = null ) {
		if ( string.IsNullOrEmpty( text ) )
			return string.Empty;

		endCharacter ??= startCharacter;

		return string.Concat(
			text
				[ ( text.IndexOf( startCharacter ) + 1 ).. ]
				.TakeWhile( ( c ) => c != endCharacter ) );
	}

	/// <summary>
	/// Searchs for a word that contains a substring
	/// </summary>
	/// <param name="text">The original text</param>
	/// <param name="word">The word to look for</param>
	/// <returns>A word found</returns>
	public static string? GetWordThatContains( this string text, string word ) {
		if ( string.IsNullOrEmpty( text ) || string.IsNullOrEmpty( word ) )
			return string.Empty;

		var foundedWord = text
			.Split( " ", StringSplitOptions.RemoveEmptyEntries )
			.FirstOrDefault( x => x.Contains( word, StringComparison.InvariantCultureIgnoreCase ) );

		return foundedWord;
	}

	/// <summary>
	/// Get the word right before a word
	/// </summary>
	/// <param name="text">The original text</param>
	/// <param name="word">The word to search</param>
	/// <returns>A word</returns>
	public static string GetWordBefore( this string text, string word ) {
		var split = text.Split( " ", StringSplitOptions.RemoveEmptyEntries ).ToList( );
		var index = split.IndexOf( word );

		if ( index > 0 && index - 1 >= 0 && split.Any( ) )
			return split.ElementAtOrDefault( index - 1 )!;

		return string.Empty;
	}

	/// <summary>
	/// Get the word right after a word
	/// </summary>
	/// <param name="text">The original text</param>
	/// <param name="word">The word to search</param>
	/// <returns>A word</returns>
	public static string? GetWordAfter( this string text, string word ) {
		var split = text.Split( " ", StringSplitOptions.RemoveEmptyEntries ).ToList( );
		var index = split.IndexOf( word );

		if ( index > 0 && index + 1 < split.Count )
			return split.ElementAtOrDefault( index + 1 );

		return string.Empty;
	}

	/// <summary>
	/// Search in text if contains any of words in list
	/// </summary>
	/// <param name="text">The original text</param>
	/// <param name="substrings">A list of strings</param>
	/// <returns>The result of search</returns>
	public static bool ContainsAnyOf( this string text, params string[ ] substrings ) {
		if ( string.IsNullOrEmpty( text ) ||
			 substrings is null ||
			 !substrings.Any( ) )
			return false;

		return substrings.Any( substring => text
			.Contains(
				substring,
				StringComparison.CurrentCultureIgnoreCase ) );
	}

	/// <summary>
	/// Search if the text starts with any characters in a list
	/// </summary>
	/// <param name="text">The original text</param>
	/// <param name="substrings">A list of charaters</param>
	/// <returns>The result of search</returns>
	public static bool StartsWithAnyOf( this string text, params string[ ] substrings ) {
		if ( string.IsNullOrEmpty( text ) ||
			 substrings is null ||
			 !substrings.Any( ) )
			return false;

		return substrings.Any( substring => text
			.StartsWith(
				substring,
				StringComparison.CurrentCultureIgnoreCase ) );
	}
}