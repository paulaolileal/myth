using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Myth.Interfaces;

namespace Myth.Models;

/// <summary>
/// In-memory cache provider implementation
/// </summary>
internal sealed class MemoryCacheProvider : ICacheProvider {
	private readonly IMemoryCache _cache;
	private readonly ILogger<MemoryCacheProvider> _logger;

	public MemoryCacheProvider( IMemoryCache cache, ILogger<MemoryCacheProvider> logger ) {
		_cache = cache;
		_logger = logger;
	}

	/// <summary>
	/// Retrieves a value from the memory cache
	/// </summary>
	/// <typeparam name="T">The type of value to retrieve</typeparam>
	/// <param name="key">The cache key</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A CacheValue indicating whether the value was found</returns>
	public Task<CacheValue<T>> GetAsync<T>( string key, CancellationToken cancellationToken = default ) {
		if ( _cache.TryGetValue( key, out T? value ) && value != null ) {
			_logger.LogDebug( "Cache hit for key: {Key}", key );
			return Task.FromResult( CacheValue<T>.Hit( value ) );
		}

		_logger.LogDebug( "Cache miss for key: {Key}", key );
		return Task.FromResult( CacheValue<T>.Miss( ) );
	}

	/// <summary>
	/// Stores a value in the memory cache with the specified expiration policy
	/// </summary>
	/// <typeparam name="T">The type of value to store</typeparam>
	/// <param name="key">The cache key</param>
	/// <param name="value">The value to cache</param>
	/// <param name="ttl">Time-to-live for the cached value</param>
	/// <param name="slidingExpiration">Whether to use sliding expiration (resets TTL on access)</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task representing the asynchronous operation</returns>
	public Task SetAsync<T>( string key, T value, TimeSpan ttl, bool slidingExpiration = false, CancellationToken cancellationToken = default ) {
		var options = new MemoryCacheEntryOptions( );

		if ( slidingExpiration )
			options.SetSlidingExpiration( ttl );
		else
			options.SetAbsoluteExpiration( ttl );

		_cache.Set( key, value, options );

		_logger.LogDebug( "Cached value for key: {Key} with TTL: {Ttl}", key, ttl );

		return Task.CompletedTask;
	}

	/// <summary>
	/// Removes a value from the memory cache
	/// </summary>
	/// <param name="key">The cache key to remove</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task representing the asynchronous operation</returns>
	public Task RemoveAsync( string key, CancellationToken cancellationToken = default ) {
		_cache.Remove( key );
		_logger.LogDebug( "Removed cache for key: {Key}", key );
		return Task.CompletedTask;
	}

	/// <summary>
	/// Removes cached values matching a pattern. Not supported in MemoryCache implementation
	/// </summary>
	/// <param name="pattern">The pattern to match cache keys against</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>A task representing the asynchronous operation</returns>
	public Task RemoveByPatternAsync( string pattern, CancellationToken cancellationToken = default ) {
		_logger.LogWarning( "Pattern-based removal not supported in MemoryCache" );
		return Task.CompletedTask;
	}
}
