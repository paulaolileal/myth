using Myth.ValueObjects;

namespace Myth.Interfaces;

/// <summary>
/// Provides fluent configuration for query caching with HTTP header support
/// </summary>
public interface ICacheConfig {

	/// <summary>
	/// Enables caching for the query with optional cache key and fluent HTTP configuration
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
	/// Enables caching with fluent HTTP configuration
	/// Usage: .UseCache(cache => cache.Public(TimeSpan.FromMinutes(15)).WithETag(x => x.Id))
	/// </summary>
	/// <param name="configure">Function to configure cache behavior</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig UseCache( Action<ICacheConfig> configure );

	/// <summary>
	/// Enables caching based on user-provided Cache-Control directive
	/// Usage: .UseCache(cacheDirective) where cacheDirective comes from [FromHeader("Cache-Control")]
	/// </summary>
	/// <param name="directive">Cache directive from user request header</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig UseCache( CacheDirective directive );

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

	/// <summary>
	/// Sets a public cache policy with max-age
	/// Usage: .Public(TimeSpan.FromMinutes(15))
	/// </summary>
	/// <param name="maxAge">How long the response can be cached</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig Public( TimeSpan maxAge );

	/// <summary>
	/// Sets a private cache policy with max-age
	/// Usage: .Private(TimeSpan.FromMinutes(5))
	/// </summary>
	/// <param name="maxAge">How long the response can be cached</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig Private( TimeSpan maxAge );

	/// <summary>
	/// Disables all caching with no-cache directive
	/// Usage: .NoCache()
	/// </summary>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig NoCache( );

	/// <summary>
	/// Sets an immutable cache policy for static content
	/// Usage: .Immutable(TimeSpan.FromDays(365))
	/// </summary>
	/// <param name="maxAge">How long the response can be cached</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig Immutable( TimeSpan maxAge );

	/// <summary>
	/// Sets a custom cache policy
	/// Usage: .WithPolicy(CachePolicy.Public(TimeSpan.FromHours(1)))
	/// </summary>
	/// <param name="policy">The cache policy to use</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig WithPolicy( CachePolicy policy );

	/// <summary>
	/// Configures ETag generation for the cached response
	/// Usage: .WithETag(result => $"\"{result.Id}-{result.UpdatedAt.Ticks}\"")
	/// </summary>
	/// <param name="generator">Function to generate ETag from result</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig WithETag( Func<object, string> generator );

	/// <summary>
	/// Sets headers that this response varies by
	/// Usage: .WithVary("Accept-Language", "Authorization")
	/// </summary>
	/// <param name="headers">Header names to include in Vary header</param>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig WithVary( params string[ ] headers );

	/// <summary>
	/// Disables HTTP header generation
	/// Usage: .WithoutHeaders()
	/// </summary>
	/// <returns>Cache configuration builder for method chaining</returns>
	ICacheConfig WithoutHeaders( );
}
