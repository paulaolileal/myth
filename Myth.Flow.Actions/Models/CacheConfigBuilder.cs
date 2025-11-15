using Myth.Interfaces;

namespace Myth.Models;

/// <summary>
/// Internal implementation of cache configuration builder
/// </summary>
internal class CacheConfigBuilder : ICacheConfig {

	/// <summary>
	/// Gets whether caching is enabled for this query
	/// </summary>
	public bool Enabled { get; private set; }

	/// <summary>
	/// Gets the cache key to use for storing/retrieving the result
	/// </summary>
	public string? Key { get; private set; }

	/// <summary>
	/// Gets the time-to-live for the cached result
	/// </summary>
	public TimeSpan? Ttl { get; private set; }

	/// <summary>
	/// Gets whether sliding expiration is enabled
	/// </summary>
	public bool SlidingExpiration { get; private set; }

	/// <summary>
	/// Gets the key generator function for custom cache key generation
	/// </summary>
	public Func<object, string>? KeyGenerator { get; private set; }

	/// <summary>
	/// Enables caching for the query with optional cache key
	/// </summary>
	/// <param name="key">The cache key to use. If null, auto-generates key based on query type and properties</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig UseCache( string? key = null ) {
		Enabled = true;
		Key = key;
		return this;
	}

	/// <summary>
	/// Enables caching with specific key and TTL
	/// </summary>
	/// <param name="key">The cache key to use</param>
	/// <param name="ttl">Time-to-live for the cached result</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig UseCache( string key, TimeSpan ttl ) {
		Enabled = true;
		Key = key;
		Ttl = ttl;
		return this;
	}

	/// <summary>
	/// Enables caching with a custom key generator function
	/// </summary>
	/// <typeparam name="TQuery">The type of query for the key generator</typeparam>
	/// <param name="keyGenerator">Function to generate cache key from query instance</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig UseCache<TQuery>( Func<TQuery, string> keyGenerator ) {
		Enabled = true;
		KeyGenerator = query => keyGenerator( ( TQuery )query );
		return this;
	}

	/// <summary>
	/// Sets the cache key for the query result
	/// </summary>
	/// <param name="key">The cache key to use</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig WithKey( string key ) {
		Key = key;
		return this;
	}

	/// <summary>
	/// Sets the time-to-live for the cached result
	/// </summary>
	/// <param name="ttl">Time-to-live duration</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig WithTtl( TimeSpan ttl ) {
		Ttl = ttl;
		return this;
	}

	/// <summary>
	/// Enables sliding expiration for the cached result
	/// </summary>
	/// <param name="sliding">Whether to use sliding expiration. Default is true</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig WithSlidingExpiration( bool sliding = true ) {
		SlidingExpiration = sliding;
		return this;
	}

	/// <summary>
	/// Converts this cache configuration to CacheOptions for dispatcher
	/// </summary>
	/// <returns>CacheOptions instance or null if caching is disabled</returns>
	public CacheOptions? ToCacheOptions( ) {
		if ( !Enabled )
			return null;

		return new CacheOptions {
			Enabled = true,
			CacheKey = Key,
			KeyGenerator = KeyGenerator,
			Ttl = Ttl ?? TimeSpan.FromMinutes( 5 ),
			SlidingExpiration = SlidingExpiration
		};
	}
}
