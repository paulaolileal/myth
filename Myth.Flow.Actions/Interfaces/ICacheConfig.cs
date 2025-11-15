namespace Myth.Interfaces;

/// <summary>
/// Provides fluent configuration for query caching
/// </summary>
public interface ICacheConfig {

	/// <summary>
	/// Enables caching for the query with optional cache key
	/// </summary>
	/// <param name="key">The cache key to use. If null, auto-generates key based on query type and properties</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig UseCache( string? key = null );

	/// <summary>
	/// Enables caching with specific key and TTL
	/// </summary>
	/// <param name="key">The cache key to use</param>
	/// <param name="ttl">Time-to-live for the cached result</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig UseCache( string key, TimeSpan ttl );

	/// <summary>
	/// Enables caching with a custom key generator function
	/// </summary>
	/// <typeparam name="TQuery">The type of query for the key generator</typeparam>
	/// <param name="keyGenerator">Function to generate cache key from query instance</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig UseCache<TQuery>( Func<TQuery, string> keyGenerator );

	/// <summary>
	/// Sets the cache key for the query result
	/// </summary>
	/// <param name="key">The cache key to use</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig WithKey( string key );

	/// <summary>
	/// Sets the time-to-live for the cached result
	/// </summary>
	/// <param name="ttl">Time-to-live duration</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig WithTtl( TimeSpan ttl );

	/// <summary>
	/// Enables sliding expiration for the cached result
	/// </summary>
	/// <param name="sliding">Whether to use sliding expiration. Default is true</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig WithSlidingExpiration( bool sliding = true );
}
