using Myth.Models;
using Myth.ValueObjects;

namespace Myth.Extensions;

/// <summary>
/// Extension methods for converting CacheDirective to CacheOptions
/// </summary>
public static class CacheDirectiveExtensions {

	/// <summary>
	/// Converts a CacheDirective to CacheOptions for internal caching logic
	/// </summary>
	/// <param name="directive">The cache directive from user header</param>
	/// <returns>CacheOptions configured according to the directive</returns>
	public static CacheOptions ToCacheOptions( this CacheDirective directive ) {
		if ( directive == null )
			return new CacheOptions { Enabled = false };

		var options = new CacheOptions {
			Enabled = true,
			GenerateHeaders = true
		};

		// Handle different directive types
		switch ( directive.Value.ToLowerInvariant( ) ) {
			case "no-cache":
				options.Enabled = false;
				options.Policy = CachePolicy.NoCache;
				break;

			case "no-store":
				options.Enabled = false;
				options.Policy = CachePolicy.NoStore;
				break;

			case "public":
				options.Policy = CachePolicy.Public( TimeSpan.FromMinutes( 5 ) ); // Default TTL
				options.Ttl = TimeSpan.FromMinutes( 5 );
				break;

			case "private":
				options.Policy = CachePolicy.Private( TimeSpan.FromMinutes( 5 ) ); // Default TTL
				options.Ttl = TimeSpan.FromMinutes( 5 );
				break;

			default:
				// Handle max-age=xxx directives
				if ( directive.Value.StartsWith( "max-age=" ) ) {
					var ageStr = directive.Value.Substring( 8 );
					if ( int.TryParse( ageStr, out var seconds ) ) {
						var duration = TimeSpan.FromSeconds( seconds );
						options.Policy = CachePolicy.Public( duration );
						options.Ttl = duration;
					}
				}
				// Handle private max-age
				else if ( directive.Value.StartsWith( "private, max-age=" ) ) {
					var ageStr = directive.Value.Substring( 17 );
					if ( int.TryParse( ageStr, out var seconds ) ) {
						var duration = TimeSpan.FromSeconds( seconds );
						options.Policy = CachePolicy.Private( duration );
						options.Ttl = duration;
					}
				} else {
					// Unknown directive, use default caching
					options.Policy = CachePolicy.Public( TimeSpan.FromMinutes( 5 ) );
					options.Ttl = TimeSpan.FromMinutes( 5 );
				}
				break;
		}

		return options;
	}

	/// <summary>
	/// Parses a Cache-Control header string into a CacheDirective
	/// </summary>
	/// <param name="headerValue">The Cache-Control header value</param>
	/// <returns>A CacheDirective representing the header</returns>
	public static CacheDirective ParseCacheControl( string? headerValue ) {
		if ( string.IsNullOrWhiteSpace( headerValue ) )
			return CacheDirective.Public; // Default

		var value = headerValue.Trim( ).ToLowerInvariant( );

		// Handle common single directives
		return value switch {
			"no-cache" => CacheDirective.NoCache,
			"no-store" => CacheDirective.NoStore,
			"public" => CacheDirective.Public,
			"private" => CacheDirective.Private,
			"must-revalidate" => CacheDirective.MustRevalidate,
			"immutable" => CacheDirective.Immutable,
			_ => ParseComplexDirective( headerValue )
		};
	}

	/// <summary>
	/// Parses complex cache directives like max-age=xxx
	/// </summary>
	private static CacheDirective ParseComplexDirective( string headerValue ) {
		var value = headerValue.ToLowerInvariant( );

		// Extract max-age value
		if ( value.Contains( "max-age=" ) ) {
			var maxAgeMatch = System.Text.RegularExpressions.Regex.Match( value, @"max-age=(\d+)" );
			if ( maxAgeMatch.Success && int.TryParse( maxAgeMatch.Groups[ 1 ].Value, out var seconds ) ) {
				return CacheDirective.MaxAge( seconds );
			}
		}

		// Extract s-maxage value
		if ( value.Contains( "s-maxage=" ) ) {
			var sMaxAgeMatch = System.Text.RegularExpressions.Regex.Match( value, @"s-maxage=(\d+)" );
			if ( sMaxAgeMatch.Success && int.TryParse( sMaxAgeMatch.Groups[ 1 ].Value, out var seconds ) ) {
				return CacheDirective.SMaxAge( seconds );
			}
		}

		// Return a custom directive for unknown values
		return new CacheDirective( "Custom", headerValue );
	}
}
