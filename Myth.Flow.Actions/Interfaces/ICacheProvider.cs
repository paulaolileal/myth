using Myth.Models;

namespace Myth.Interfaces;

/// <summary>
/// Abstraction for cache provider
/// </summary>
public interface ICacheProvider {

	/// <summary>
	/// Gets cached value
	/// </summary>
	Task<CacheValue<T>> GetAsync<T>( string key, CancellationToken cancellationToken = default );

	/// <summary>
	/// Sets cached value
	/// </summary>
	Task SetAsync<T>( string key, T value, TimeSpan ttl, bool slidingExpiration = false, CancellationToken cancellationToken = default );

	/// <summary>
	/// Removes cached value
	/// </summary>
	Task RemoveAsync( string key, CancellationToken cancellationToken = default );

	/// <summary>
	/// Removes cached values by pattern
	/// </summary>
	Task RemoveByPatternAsync( string pattern, CancellationToken cancellationToken = default );
}