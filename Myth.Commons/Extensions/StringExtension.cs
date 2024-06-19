using System.Text.RegularExpressions;

namespace Myth.Extensions {

	public static class StringExtension {

		public static string Remove( this string value, string text ) => value.Replace( text, string.Empty );

		public static string Minify( this string text ) => Regex.Replace( text, @"\s+", "" );

		public static string ToFirstLower( this string text ) {
			if ( !string.IsNullOrEmpty( text ) && text.Any( ) )
				return string.Concat(
					text.First( ).ToString( ).ToLowerInvariant( ),
					text.AsSpan( 1 ) );

			return string.Empty;
		}

		public static string ToFirstUpper( this string text ) {
			if ( !string.IsNullOrEmpty( text ) && text.Any( ) )
				return string.Concat(
					text.First( ).ToString( ).ToUpperInvariant( ),
					text.AsSpan( 1 ) );

			return string.Empty;
		}

		public static string GetStringBetween( this string text, char startCharacter, char? endCharacter = null ) {
			if ( string.IsNullOrEmpty( text ) )
				return string.Empty;

			endCharacter ??= startCharacter;

			return string.Concat(
				text
					.Substring( text.IndexOf( startCharacter ) + 1 )
					.TakeWhile( ( c ) => c != endCharacter ) );
		}

		public static string? GetWordThatContains( this string text, string word ) {
			if ( string.IsNullOrEmpty( text ) || string.IsNullOrEmpty( word ) )
				return string.Empty;

			var foundedWord = text
				.Split( " ", StringSplitOptions.RemoveEmptyEntries )
				.FirstOrDefault( x => x.Contains( word, StringComparison.InvariantCultureIgnoreCase ) );

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
}