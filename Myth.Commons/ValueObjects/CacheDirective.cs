using System.Runtime.CompilerServices;

namespace Myth.ValueObjects;

/// <summary>
/// Type-safe constants for HTTP Cache-Control directives with excellent developer experience.
/// Can be used as controller parameters with FromHeader attribute.
/// </summary>
public class CacheDirective( string name, string value ) : Constant<CacheDirective, string>( name, value ) {

	// Basic directives
	public static readonly CacheDirective Public = CreateWithCallerName( "public" );
	public static readonly CacheDirective Private = CreateWithCallerName( "private" );
	public static readonly CacheDirective NoCache = CreateWithCallerName( "no-cache" );
	public static readonly CacheDirective NoStore = CreateWithCallerName( "no-store" );
	public static readonly CacheDirective MustRevalidate = CreateWithCallerName( "must-revalidate" );
	public static readonly CacheDirective Immutable = CreateWithCallerName( "immutable" );

	// Factory methods for parameterized directives
	public static CacheDirective MaxAge( int seconds ) => new CacheDirective( $"MaxAge({seconds})", $"max-age={seconds}" );
	public static CacheDirective MaxAge( TimeSpan duration ) => MaxAge( ( int )duration.TotalSeconds );
	public static CacheDirective SMaxAge( int seconds ) => new CacheDirective( $"SMaxAge({seconds})", $"s-maxage={seconds}" );
	public static CacheDirective SMaxAge( TimeSpan duration ) => SMaxAge( ( int )duration.TotalSeconds );
}
