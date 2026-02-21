namespace Myth.Interfaces;

/// <summary>
/// Public interface for cache management operations
/// Provides controlled access to cache invalidation and management
/// </summary>
public interface ICacheManager {

	/// <summary>
	/// Gets a cached value by key
	/// </summary>
	/// <typeparam name="T">The type of value to retrieve</typeparam>
	/// <param name="key">The cache key</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>The cached value if found, otherwise null</returns>
	Task<T?> GetAsync<T>( string key, CancellationToken cancellationToken = default );

	/// <summary>
	/// Sets a value in cache with specified time-to-live
	/// </summary>
	/// <typeparam name="T">The type of value to cache</typeparam>
	/// <param name="key">The cache key</param>
	/// <param name="value">The value to cache</param>
	/// <param name="ttl">Time-to-live for the cached value</param>
	/// <param name="slidingExpiration">Whether to use sliding expiration (resets TTL on access)</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task representing the asynchronous operation</returns>
	Task SetAsync<T>( string key, T value, TimeSpan ttl, bool slidingExpiration = false, CancellationToken cancellationToken = default );

	/// <summary>
	/// Invalidates (removes) a specific cache entry by key
	/// </summary>
	/// <param name="key">The cache key to invalidate</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task representing the asynchronous operation</returns>
	Task InvalidateAsync( string key, CancellationToken cancellationToken = default );

	/// <summary>
	/// Invalidates (removes) all cache entries matching a pattern
	/// Pattern format depends on cache provider (e.g., "User:*" for Redis)
	/// Note: MemoryCache has limited pattern support
	/// </summary>
	/// <param name="pattern">The pattern to match cache keys</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task representing the asynchronous operation</returns>
	Task InvalidateByPatternAsync( string pattern, CancellationToken cancellationToken = default );

	/// <summary>
	/// Invalidates cache entries for a specific query type
	/// If query instance is provided, invalidates only that specific query
	/// If query is null, invalidates all queries of that type using pattern matching
	/// </summary>
	/// <typeparam name="TQuery">The query type to invalidate cache for</typeparam>
	/// <param name="query">Optional query instance for specific invalidation. If null, invalidates all queries of this type</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task representing the asynchronous operation</returns>
	Task InvalidateByTypeAsync<TQuery>( TQuery? query = default, CancellationToken cancellationToken = default );

	/// <summary>
	/// Generates a cache key for a query using the same algorithm as the Dispatcher
	/// This ensures consistency between cached queries and manual cache operations
	/// </summary>
	/// <typeparam name="TQuery">The type of query</typeparam>
	/// <param name="query">The query instance</param>
	/// <returns>A cache key string in the format "TypeName:Hash"</returns>
	string GenerateKey<TQuery>( TQuery query );
}
