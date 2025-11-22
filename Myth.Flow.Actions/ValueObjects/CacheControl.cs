using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Myth.Models;
using Myth.ValueObjects;

namespace Myth.Flow.Actions.ValueObjects;

/// <summary>
/// Represents HTTP Cache-Control directives with type-safe constants and parsing capabilities
/// </summary>
public sealed class CacheControl : Constant<CacheControl, string> {

	#region Type-Safe Constants

	/// <summary>
	/// no-cache directive - forces caches to submit the request to the origin server for validation before releasing a cached copy
	/// </summary>
	public static readonly CacheControl NoCache = CreateWithCallerName( "no-cache" );

	/// <summary>
	/// no-store directive - instructs caches not to store either the immediate request or any response to it
	/// </summary>
	public static readonly CacheControl NoStore = CreateWithCallerName( "no-store" );

	/// <summary>
	/// public directive - indicates that the response may be cached by any cache
	/// </summary>
	public static readonly CacheControl Public = CreateWithCallerName( "public" );

	/// <summary>
	/// private directive - indicates that the response is intended for a single user and must not be stored by a shared cache
	/// </summary>
	public static readonly CacheControl Private = CreateWithCallerName( "private" );

	/// <summary>
	/// must-revalidate directive - indicates that once a resource becomes stale, caches must not use their stale copy without successful validation on the origin server
	/// </summary>
	public static readonly CacheControl MustRevalidate = CreateWithCallerName( "must-revalidate" );

	/// <summary>
	/// proxy-revalidate directive - similar to must-revalidate, but only applies to proxy caches
	/// </summary>
	public static readonly CacheControl ProxyRevalidate = CreateWithCallerName( "proxy-revalidate" );

	/// <summary>
	/// no-transform directive - indicates that an intermediary should not transform the payload
	/// </summary>
	public static readonly CacheControl NoTransform = CreateWithCallerName( "no-transform" );

	/// <summary>
	/// immutable directive - indicates that the response body will not change over time
	/// </summary>
	public static readonly CacheControl Immutable = CreateWithCallerName( "immutable" );

	/// <summary>
	/// max-age directive - indicates the maximum amount of time a resource will be considered fresh
	/// </summary>
	public static readonly CacheControl MaxAge = CreateWithCallerName( "max-age" );

	/// <summary>
	/// s-maxage directive - overrides max-age for shared caches only
	/// </summary>
	public static readonly CacheControl SMaxAge = CreateWithCallerName( "s-maxage" );

	#endregion

	private readonly bool _isValid;
	private readonly string? _originalValue;

	/// <summary>
	/// Constructor for Constant pattern - matches base class signature
	/// </summary>
	public CacheControl( string name, string value )
		: base( name, value ) {
		_isValid = true;
		_originalValue = value;
	}

	/// <summary>
	/// Constructor for parsed values with validation
	/// </summary>
	private CacheControl( string name, string value, bool isValid, string? originalValue )
		: base( name, value ) {
		_isValid = isValid;
		_originalValue = originalValue ?? value;
	}

	/// <summary>
	/// Gets whether this cache control directive is valid according to RFC 7234
	/// </summary>
	public bool IsValid => _isValid;

	/// <summary>
	/// Gets the original header value before parsing
	/// </summary>
	public string? OriginalValue => _originalValue;

	#region Factory Methods

	/// <summary>
	/// Creates a CacheControl from header value with parsing and validation
	/// </summary>
	/// <param name="headerValue">The Cache-Control header value</param>
	/// <returns>A CacheControl instance, marked as invalid if parsing fails</returns>
	public static CacheControl Parse( string? headerValue ) {
		if ( string.IsNullOrWhiteSpace( headerValue ) ) {
			return new CacheControl( "invalid", string.Empty, false, headerValue );
		}

		try {
			var cleanValue = headerValue.Trim( );

			// Validate basic syntax
			if ( IsValidCacheControlSyntax( cleanValue ) ) {
				return new CacheControl( "parsed", cleanValue.ToLowerInvariant( ), true, headerValue );
			}

			return new CacheControl( "invalid", headerValue, false, headerValue );
		} catch {
			return new CacheControl( "invalid", headerValue, false, headerValue );
		}
	}

	/// <summary>
	/// Creates a CacheControl with max-age directive
	/// </summary>
	/// <param name="maxAge">The max-age value in seconds</param>
	/// <returns>A CacheControl with max-age directive</returns>
	public static CacheControl WithMaxAge( int maxAge ) {
		return new CacheControl( "max-age", $"max-age={maxAge}", true, $"max-age={maxAge}" );
	}

	/// <summary>
	/// Creates a CacheControl with max-age directive
	/// </summary>
	/// <param name="maxAge">The max-age duration</param>
	/// <returns>A CacheControl with max-age directive</returns>
	public static CacheControl WithMaxAge( TimeSpan maxAge ) {
		return WithMaxAge( ( int )maxAge.TotalSeconds );
	}

	/// <summary>
	/// Creates a public cache control with max-age
	/// </summary>
	/// <param name="maxAge">The max-age duration</param>
	/// <returns>A CacheControl with public and max-age directives</returns>
	public static CacheControl PublicWithMaxAge( TimeSpan maxAge ) {
		var seconds = ( int )maxAge.TotalSeconds;
		return new CacheControl( "public-max-age", $"public, max-age={seconds}", true, $"public, max-age={seconds}" );
	}

	/// <summary>
	/// Creates a private cache control with max-age
	/// </summary>
	/// <param name="maxAge">The max-age duration</param>
	/// <returns>A CacheControl with private and max-age directives</returns>
	public static CacheControl PrivateWithMaxAge( TimeSpan maxAge ) {
		var seconds = ( int )maxAge.TotalSeconds;
		return new CacheControl( "private-max-age", $"private, max-age={seconds}", true, $"private, max-age={seconds}" );
	}

	private static bool IsValidCacheControlSyntax( string value ) {
		if ( string.IsNullOrWhiteSpace( value ) )
			return false;

		var validDirectives = new HashSet<string> {
			"no-cache", "no-store", "public", "private", "must-revalidate",
			"proxy-revalidate", "no-transform", "immutable"
		};

		var parts = value.Split( ',' ).Select( p => p.Trim( ).ToLowerInvariant( ) );

		foreach ( var part in parts ) {
			if ( validDirectives.Contains( part ) )
				continue;

			// Check max-age=number or s-maxage=number
			if ( part.StartsWith( "max-age=" ) || part.StartsWith( "s-maxage=" ) ) {
				var numberPart = part.Split( '=' )[ 1 ];
				if ( int.TryParse( numberPart, out var number ) && number >= 0 )
					continue;
			}

			// Check stale-while-revalidate=number
			if ( part.StartsWith( "stale-while-revalidate=" ) ) {
				var numberPart = part.Split( '=' )[ 1 ];
				if ( int.TryParse( numberPart, out var number ) && number >= 0 )
					continue;
			}

			// Check stale-if-error=number
			if ( part.StartsWith( "stale-if-error=" ) ) {
				var numberPart = part.Split( '=' )[ 1 ];
				if ( int.TryParse( numberPart, out var number ) && number >= 0 )
					continue;
			}

			return false;
		}

		return true;
	}

	#endregion

	#region Conversion Methods

	/// <summary>
	/// Converts to CacheOptions for dispatcher usage
	/// </summary>
	/// <returns>CacheOptions instance or null if invalid or no-store directive present</returns>
	public CacheOptions? ToCacheOptions( ) {
		if ( !_isValid )
			return null;

		var value = Value.ToLowerInvariant( );

		// no-store means don't cache at all
		if ( value.Contains( "no-store" ) ) {
			return null;
		}

		var policy = ParseToCachePolicy( );
		var ttl = ExtractTtl( ) ?? TimeSpan.FromMinutes( 5 );

		return new CacheOptions {
			Enabled = true,
			Policy = policy,
			Ttl = ttl,
			GenerateHeaders = policy != null
		};
	}

	private CachePolicy? ParseToCachePolicy( ) {
		if ( !_isValid )
			return null;

		var value = Value.ToLowerInvariant( );

		// no-cache directive
		if ( value.Contains( "no-cache" ) ) {
			return CachePolicy.NoCache;
		}

		// public with max-age
		if ( value.Contains( "public" ) && value.Contains( "max-age=" ) ) {
			var maxAge = ExtractTtl( ) ?? TimeSpan.FromMinutes( 5 );
			return CachePolicy.Public( maxAge );
		}

		// private with max-age
		if ( value.Contains( "private" ) && value.Contains( "max-age=" ) ) {
			var maxAge = ExtractTtl( ) ?? TimeSpan.FromMinutes( 5 );
			return CachePolicy.Private( maxAge );
		}

		// immutable
		if ( value.Contains( "immutable" ) ) {
			var maxAge = ExtractTtl( ) ?? TimeSpan.FromDays( 365 );
			return CachePolicy.Immutable( maxAge );
		}

		// just max-age (defaults to private)
		if ( value.Contains( "max-age=" ) ) {
			var maxAge = ExtractTtl( ) ?? TimeSpan.FromMinutes( 5 );
			return CachePolicy.Private( maxAge );
		}

		// Default for just "public"
		if ( value == "public" ) {
			return CachePolicy.Public( TimeSpan.FromMinutes( 5 ) );
		}

		// Default for just "private"
		if ( value == "private" ) {
			return CachePolicy.Private( TimeSpan.FromMinutes( 5 ) );
		}

		return null;
	}

	private TimeSpan? ExtractTtl( ) {
		if ( !_isValid )
			return null;

		// Extract max-age
		var maxAgeMatch = Regex.Match( Value, @"max-age=(\d+)" );
		if ( maxAgeMatch.Success && int.TryParse( maxAgeMatch.Groups[ 1 ].Value, out var seconds ) ) {
			return TimeSpan.FromSeconds( seconds );
		}

		// Extract s-maxage (shared cache max-age)
		var sMaxAgeMatch = Regex.Match( Value, @"s-maxage=(\d+)" );
		if ( sMaxAgeMatch.Success && int.TryParse( sMaxAgeMatch.Groups[ 1 ].Value, out var sSeconds ) ) {
			return TimeSpan.FromSeconds( sSeconds );
		}

		return null;
	}

	#endregion

	/// <summary>
	/// Implicit conversion to string for HTTP header usage
	/// </summary>
	public static implicit operator string( CacheControl cacheControl ) {
		return cacheControl?.Value ?? string.Empty;
	}

	/// <summary>
	/// Returns a string representation of this cache control directive
	/// </summary>
	public override string ToString( ) {
		return _isValid ? Value : $"[Invalid: {_originalValue}]";
	}
}
