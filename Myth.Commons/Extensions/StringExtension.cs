using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Myth.Extensions {

    public static class StringExtension {

        public static string ToCamelCase( this string text ) {
            if ( text.Any( ) )
                return Char.ToLowerInvariant( text.First( ) ) + text.Substring( 1 );
            return text;
        }

        public static string ToFirstUpper( this string text ) {
            if ( text.Any( ) )
                return Char.ToUpperInvariant( text.First( ) ) + text.Substring( 1 );
            return text;
        }

        public static string Minify( this string text ) {
            return Regex.Replace( text, @"\s+", "" );
        }

        public static string GetStringBetween( this string text, char startCharacter, char? endCharacter = null ) {
            if ( string.IsNullOrEmpty( text ) )
                return string.Empty;

            if ( endCharacter == null )
                endCharacter = startCharacter;

            return string.Concat( text.Substring( text.IndexOf( startCharacter ) + 1 ).TakeWhile( ( c ) => c != endCharacter ) );
        }

        public static string GetWordThatContains( this string text, string word, bool removeWord = false ) {
            var founded = text
                .Split( " ", StringSplitOptions.RemoveEmptyEntries )
                .FirstOrDefault( x => x.Contains( word, StringComparison.InvariantCultureIgnoreCase ) );

            if ( removeWord )
                founded = founded.Replace( word, "", StringComparison.InvariantCultureIgnoreCase );

            return founded;
        }

        public static string GetWordBefore( this string text, string word ) {
            var split = text.Replace( "(", " (" ).Split( " ", StringSplitOptions.RemoveEmptyEntries ).ToList( );
            var index = split.IndexOf( word );
            if ( index > 0 && index - 1 >= 0 ) {
                return split.ElementAtOrDefault( index - 1 );
            }
            return string.Empty;
        }

        public static string GetWordAfter( this string text, string word ) {
            var split = text.Replace( "(", " (" ).Split( " ", StringSplitOptions.RemoveEmptyEntries ).ToList( );
            var index = split.IndexOf( word );
            if ( index > 0 && index + 1 < split.Count ) {
                return split.ElementAtOrDefault( index + 1 );
            }
            return "";
        }
    }
}