using Myth.ValueObjects;

namespace Myth.Models;

/// <summary>
/// Metadata about cache behavior for HTTP header generation
/// </summary>
public sealed class CacheMetadata {

	/// <summary>
	/// The cache policy that defines Cache-Control directives
	/// </summary>
	public CachePolicy? Policy { get; set; }

	/// <summary>
	/// When this cached item expires (for Expires header)
	/// </summary>
	public DateTime? ExpiresAt { get; set; }

	/// <summary>
	/// ETag value for this response
	/// </summary>
	public string? ETag { get; set; }

	/// <summary>
	/// Last modified timestamp
	/// </summary>
	public DateTime? LastModified { get; set; }

	/// <summary>
	/// Headers that this response varies by
	/// </summary>
	public string[ ]? VaryHeaders { get; set; }

	/// <summary>
	/// Age of the cached content in seconds
	/// </summary>
	public TimeSpan? Age { get; set; }

	/// <summary>
	/// Whether this response came from cache
	/// </summary>
	public bool FromCache { get; set; }

	/// <summary>
	/// Whether HTTP headers should be generated from this metadata
	/// </summary>
	public bool ShouldGenerateHeaders => Policy != null || !string.IsNullOrEmpty( ETag );

	/// <summary>
	/// Converts cache metadata to HTTP headers dictionary
	/// </summary>
	public Dictionary<string, string> ToHeaders( ) {
		var headers = new Dictionary<string, string>( );

		if ( Policy != null )
			headers[ "Cache-Control" ] = Policy.ToHeaderValue( );

		if ( ExpiresAt.HasValue )
			headers[ "Expires" ] = ExpiresAt.Value.ToString( "R" );

		if ( !string.IsNullOrEmpty( ETag ) )
			headers[ "ETag" ] = ETag;

		if ( LastModified.HasValue )
			headers[ "Last-Modified" ] = LastModified.Value.ToString( "R" );

		if ( VaryHeaders != null && VaryHeaders.Length > 0 )
			headers[ "Vary" ] = string.Join( ", ", VaryHeaders );

		if ( Age.HasValue )
			headers[ "Age" ] = ( ( int )Age.Value.TotalSeconds ).ToString( );

		return headers;
	}

	/// <summary>
	/// Creates metadata for a public cache
	/// </summary>
	public static CacheMetadata Public( TimeSpan duration, string? etag = null ) =>
		new( ) {
			Policy = CachePolicy.Public( duration ),
			ExpiresAt = DateTime.UtcNow.Add( duration ),
			ETag = etag
		};

	/// <summary>
	/// Creates metadata for a private cache
	/// </summary>
	public static CacheMetadata Private( TimeSpan duration, string? etag = null ) =>
		new( ) {
			Policy = CachePolicy.Private( duration ),
			ExpiresAt = DateTime.UtcNow.Add( duration ),
			ETag = etag
		};

	/// <summary>
	/// Creates metadata that disables caching
	/// </summary>
	public static CacheMetadata NoCache( ) =>
		new( ) {
			Policy = CachePolicy.NoCache
		};
}
