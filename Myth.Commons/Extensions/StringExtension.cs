using System.Text.RegularExpressions;

namespace Myth.Extensions {

	public static class StringExtension {

		public static string Remove( this string value, string text ) => value.Replace( text, string.Empty );

		public static string ToCamelCase( this string text ) {
			if ( !string.IsNullOrEmpty(text) && text.Any( ) )
				return char.ToLowerInvariant( text.First( ) ) + text.Substring( 1 );

			return string.Empty;
		}

		public static string ToFirstUpper( this string text ) {
			if ( !string.IsNullOrEmpty( text ) && text.Any( ) )
				return char.ToUpperInvariant( text.First( ) ) + text.Substring( 1 );

			return string.Empty;
		}

		public static string Minify( this string text ) => Regex.Replace( text, @"\s+", "" );

		public static string GetStringBetween( this string text, char startCharacter, char? endCharacter = null ) {
			if ( string.IsNullOrEmpty( text ) )
				return string.Empty;

			endCharacter ??= startCharacter;

			return string.Concat(
				text
					.Substring( text.IndexOf( startCharacter ) + 1 )
					.TakeWhile( ( c ) => c != endCharacter ) );
		}

		public static string? GetWordThatContains( this string text, string word, bool removeWord = false ) {
			var foundedWord = text
				.Split( " ", StringSplitOptions.RemoveEmptyEntries )
				.FirstOrDefault( x => x.Contains( word, StringComparison.InvariantCultureIgnoreCase ) );

			if ( removeWord && !string.IsNullOrEmpty( foundedWord ) )
				foundedWord = foundedWord.Replace( word, "", StringComparison.InvariantCultureIgnoreCase );

			return foundedWord;
		}

		public static string GetWordBefore( this string text, string word ) {
			var split = text.Split( " ", StringSplitOptions.RemoveEmptyEntries ).ToList( );
			var index = split.IndexOf( word );

			if ( index > 0 && index - 1 >= 0 && split.Any( ) ) {
				return split.ElementAtOrDefault( index - 1 )!;
			}

			return string.Empty;
		}

		public static string? GetWordAfter( this string text, string word ) {
			var split = text.Split( " ", StringSplitOptions.RemoveEmptyEntries ).ToList( );
			var index = split.IndexOf( word );

			if ( index > 0 && index + 1 < split.Count ) {
				return split.ElementAtOrDefault( index + 1 );
			}

			return string.Empty;
		}

		public static bool ContainsAnyOf( this string s, IEnumerable<string> substrings ) {
			if ( string.IsNullOrEmpty( s ) || substrings == null )
				return false;

			return substrings.Any( substring => s.Contains( substring, StringComparison.CurrentCultureIgnoreCase ) );
		}

		public static bool StartsWithAnyOf( this string s, IEnumerable<string> substrings ) {
			if ( string.IsNullOrEmpty( s ) || substrings == null )
				return false;

			return substrings.Any( substring => s.StartsWith( substring, StringComparison.CurrentCultureIgnoreCase ) );
		}
	}
}