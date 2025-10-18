namespace Myth.Models;

/// <summary>
/// Cache configuration for queries
/// </summary>
public sealed class CacheOptions {

	/// <summary>
	/// Indicates whether caching is enabled for the query
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	/// Custom cache key to use instead of auto-generated key
	/// </summary>
	public string? CacheKey { get; set; }

	/// <summary>
	/// Time-to-live for the cached value. Default is 5 minutes
	/// </summary>
	public TimeSpan Ttl { get; set; } = TimeSpan.FromMinutes( 5 );

	/// <summary>
	/// Indicates whether to use sliding expiration (resets TTL on each access)
	/// </summary>
	public bool SlidingExpiration { get; set; }

	/// <summary>
	/// Custom function to generate cache keys from query objects
	/// </summary>
	public Func<object, string>? KeyGenerator { get; set; }
}