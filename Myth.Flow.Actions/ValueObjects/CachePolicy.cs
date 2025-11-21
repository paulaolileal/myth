using Myth.Extensions;

namespace Myth.Flow.Actions.ValueObjects;

/// <summary>
/// Represents a complete HTTP cache policy with multiple directives.
/// Provides simple factory methods for common caching scenarios.
/// </summary>
public class CachePolicy {
	private readonly List<CacheControl> _directives = new( );

	/// <summary>
	/// All cache directives in this policy
	/// </summary>
	public IReadOnlyList<CacheControl> Directives => _directives.AsReadOnly( );

	/// <summary>
	/// The max-age value if specified
	/// </summary>
	public TimeSpan? MaxAge { get; private set; }

	/// <summary>
	/// Whether this is a public cache policy
	/// </summary>
	public bool IsPublic => _directives.Any( d => d.Value == "public" );

	/// <summary>
	/// Whether this is a private cache policy
	/// </summary>
	public bool IsPrivate => _directives.Any( d => d.Value == "private" );

	/// <summary>
	/// Whether caching is disabled
	/// </summary>
	public bool IsNoCache => _directives.Any( d => d.Value == "no-cache" );

	/// <summary>
	/// Adds a directive to this policy
	/// </summary>
	public CachePolicy Add( CacheControl directive ) {
		if ( directive == null )
			throw new ArgumentNullException( nameof( directive ) );

		_directives.Add( directive );

		// Track max-age if it's being set
		if ( directive.Value.StartsWith( "max-age=" ) ) {
			var ageStr = directive.Value.Substring( 8 );
			if ( int.TryParse( ageStr, out var seconds ) ) {
				MaxAge = TimeSpan.FromSeconds( seconds );
			}
		}

		return this;
	}

	/// <summary>
	/// Sets max-age directive
	/// </summary>
	public CachePolicy WithMaxAge( TimeSpan duration ) {
		MaxAge = duration;
		var maxAgeValue = $"max-age={( int )duration.TotalSeconds}";
		_directives.Add( new CacheControl( "max-age", maxAgeValue ) );
		return this;
	}

	/// <summary>
	/// Converts policy to Cache-Control header value
	/// </summary>
	public string ToHeaderValue( ) {
		return _directives.Select( d => d.Value ).ToStringWithSeparator( ", " );
	}

	/// <summary>
	/// Policy that disables all caching
	/// </summary>
	public static CachePolicy NoCache => new CachePolicy( ).Add( CacheControl.NoCache );

	/// <summary>
	/// Policy that prevents storing in any cache
	/// </summary>
	public static CachePolicy NoStore => new CachePolicy( ).Add( CacheControl.NoStore );

	/// <summary>
	/// Public cache for the specified duration
	/// Usage: CachePolicy.Public( TimeSpan.FromMinutes( 15 ) )
	/// </summary>
	public static CachePolicy Public( TimeSpan duration ) =>
		new CachePolicy( ).Add( CacheControl.Public ).WithMaxAge( duration );

	/// <summary>
	/// Private cache for the specified duration
	/// Usage: CachePolicy.Private( TimeSpan.FromMinutes( 5 ) )
	/// </summary>
	public static CachePolicy Private( TimeSpan duration ) =>
		new CachePolicy( ).Add( CacheControl.Private ).WithMaxAge( duration );

	/// <summary>
	/// Immutable public cache for long-term storage
	/// Usage: CachePolicy.Immutable( TimeSpan.FromDays( 365 ) )
	/// </summary>
	public static CachePolicy Immutable( TimeSpan duration ) =>
		new CachePolicy( )
			.Add( CacheControl.Public )
			.Add( CacheControl.Immutable )
			.WithMaxAge( duration );

	public override string ToString( ) => ToHeaderValue( );
}
