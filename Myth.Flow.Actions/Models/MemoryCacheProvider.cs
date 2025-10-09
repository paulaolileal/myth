using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Myth.Flow.Interfaces;

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

	public Task<CacheValue<T>> GetAsync<T>( string key, CancellationToken cancellationToken = default ) {
		if ( _cache.TryGetValue( key, out T? value ) && value != null ) {
			_logger.LogDebug( "Cache hit for key: {Key}", key );
			return Task.FromResult( CacheValue<T>.Hit( value ) );
		}

		_logger.LogDebug( "Cache miss for key: {Key}", key );
		return Task.FromResult( CacheValue<T>.Miss( ) );
	}

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

	public Task RemoveAsync( string key, CancellationToken cancellationToken = default ) {
		_cache.Remove( key );
		_logger.LogDebug( "Removed cache for key: {Key}", key );
		return Task.CompletedTask;
	}

	public Task RemoveByPatternAsync( string pattern, CancellationToken cancellationToken = default ) {
		_logger.LogWarning( "Pattern-based removal not supported in MemoryCache" );
		return Task.CompletedTask;
	}
}