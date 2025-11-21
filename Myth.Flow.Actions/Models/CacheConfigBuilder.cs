using Myth.Extensions;
using Myth.Flow.Actions.ValueObjects;
using Myth.Interfaces;

namespace Myth.Models;

/// <summary>
/// Internal implementation of cache configuration builder with HTTP support
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
	/// Gets the HTTP cache policy for generating headers
	/// </summary>
	public CachePolicy? Policy { get; private set; }

	/// <summary>
	/// Gets the ETag generator function
	/// </summary>
	public Func<object, string>? ETagGenerator { get; private set; }

	/// <summary>
	/// Gets the headers for the Vary header
	/// </summary>
	public string[ ]? VaryHeaders { get; private set; }

	/// <summary>
	/// Gets whether HTTP headers should be generated
	/// </summary>
	public bool GenerateHeaders { get; private set; } = true;

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
	/// Enables caching with fluent HTTP configuration
	/// </summary>
	/// <param name="configure">Function to configure cache behavior</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig UseCache( Action<ICacheConfig> configure ) {
		Enabled = true;
		configure( this );
		return this;
	}

	/// <summary>
	/// Enables caching based on user-provided Cache-Control directive
	/// </summary>
	/// <param name="cacheControl">Cache control from user request header</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig UseCache( CacheControl cacheControl ) {
		if ( cacheControl == null || !cacheControl.IsValid ) {
			Enabled = false;
			return this;
		}

		// Convert the user's cache control to cache configuration
		var userOptions = cacheControl.ToCacheOptions( );

		if ( userOptions == null ) {
			Enabled = false;
			return this;
		}

		Enabled = userOptions.Enabled;
		Policy = userOptions.Policy;
		Ttl = userOptions.Ttl;
		GenerateHeaders = userOptions.GenerateHeaders;

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
	/// Sets a public cache policy with max-age
	/// </summary>
	/// <param name="maxAge">How long the response can be cached</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig Public( TimeSpan maxAge ) {
		Enabled = true;
		Policy = CachePolicy.Public( maxAge );
		Ttl = maxAge;
		return this;
	}

	/// <summary>
	/// Sets a private cache policy with max-age
	/// </summary>
	/// <param name="maxAge">How long the response can be cached</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig Private( TimeSpan maxAge ) {
		Enabled = true;
		Policy = CachePolicy.Private( maxAge );
		Ttl = maxAge;
		return this;
	}

	/// <summary>
	/// Disables all caching with no-cache directive
	/// </summary>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig NoCache( ) {
		Enabled = true;
		Policy = CachePolicy.NoCache;
		return this;
	}

	/// <summary>
	/// Sets an immutable cache policy for static content
	/// </summary>
	/// <param name="maxAge">How long the response can be cached</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig Immutable( TimeSpan maxAge ) {
		Enabled = true;
		Policy = CachePolicy.Immutable( maxAge );
		Ttl = maxAge;
		return this;
	}

	/// <summary>
	/// Sets a custom cache policy
	/// </summary>
	/// <param name="policy">The cache policy to use</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig WithPolicy( CachePolicy policy ) {
		Enabled = true;
		Policy = policy;
		if ( policy.MaxAge.HasValue )
			Ttl = policy.MaxAge.Value;
		return this;
	}

	/// <summary>
	/// Configures ETag generation for the cached response
	/// </summary>
	/// <param name="generator">Function to generate ETag from result</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig WithETag( Func<object, string> generator ) {
		Enabled = true; // Enable cache when ETag is configured
		ETagGenerator = generator;
		return this;
	}

	/// <summary>
	/// Sets headers that this response varies by
	/// </summary>
	/// <param name="headers">Header names to include in Vary header</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig WithVary( params string[ ] headers ) {
		Enabled = true; // Enable cache when Vary is configured
		VaryHeaders = headers;
		return this;
	}

	/// <summary>
	/// Disables HTTP header generation
	/// </summary>
	/// <returns>Cache configuration builder for method chaining</returns>
	public ICacheConfig WithoutHeaders( ) {
		GenerateHeaders = false;
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
			SlidingExpiration = SlidingExpiration,
			Policy = Policy,
			GenerateHeaders = GenerateHeaders,
			ETagGenerator = ETagGenerator,
			VaryHeaders = VaryHeaders
		};
	}
}
