using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Myth.Interfaces;

namespace Myth.Models;

/// <summary>
/// Default implementation of cache manager
/// Provides public access to cache operations with consistent key generation
/// </summary>
internal sealed class CacheManager( ICacheProvider cacheProvider, ILogger<CacheManager> logger ) : ICacheManager {
	private readonly ICacheProvider _cacheProvider = cacheProvider;
	private readonly ILogger<CacheManager> _logger = logger;

	/// <summary>
	/// Gets a cached value by key
	/// </summary>
	public async Task<T?> GetAsync<T>( string key, CancellationToken cancellationToken = default ) {
		var result = await _cacheProvider.GetAsync<T>( key, cancellationToken );
		return result.HasValue ? result.Value : default;
	}

	/// <summary>
	/// Sets a value in cache with specified time-to-live
	/// </summary>
	public async Task SetAsync<T>( string key, T value, TimeSpan ttl, bool slidingExpiration = false, CancellationToken cancellationToken = default ) {
		await _cacheProvider.SetAsync( key, value, ttl, slidingExpiration, cancellationToken );
	}

	/// <summary>
	/// Invalidates (removes) a specific cache entry by key
	/// </summary>
	public async Task InvalidateAsync( string key, CancellationToken cancellationToken = default ) {
		_logger.LogInformation( "Invalidating cache for key: {Key}", key );
		await _cacheProvider.RemoveAsync( key, cancellationToken );
	}

	/// <summary>
	/// Invalidates (removes) all cache entries matching a pattern
	/// </summary>
	public async Task InvalidateByPatternAsync( string pattern, CancellationToken cancellationToken = default ) {
		_logger.LogInformation( "Invalidating cache by pattern: {Pattern}", pattern );
		await _cacheProvider.RemoveByPatternAsync( pattern, cancellationToken );
	}

	/// <summary>
	/// Invalidates cache entries for a specific query type
	/// If query instance is provided, invalidates only that specific query
	/// If query is null, invalidates all queries of that type using pattern matching
	/// </summary>
	public async Task InvalidateByTypeAsync<TQuery>( TQuery? query = default, CancellationToken cancellationToken = default ) {
		// Check if query has a non-default value (works for both reference and value types)
		if ( !EqualityComparer<TQuery>.Default.Equals( query, default ) ) {
			// Invalidate specific query instance
			var key = GenerateKey( query! );
			_logger.LogInformation( "Invalidating cache for query type {QueryType} with key: {Key}",
				typeof( TQuery ).Name, key );
			await _cacheProvider.RemoveAsync( key, cancellationToken );
		} else {
			// Invalidate all queries of this type
			var pattern = $"{typeof( TQuery ).Name}:*";
			_logger.LogInformation( "Invalidating all cache entries for query type {QueryType} using pattern: {Pattern}",
				typeof( TQuery ).Name, pattern );
			await _cacheProvider.RemoveByPatternAsync( pattern, cancellationToken );
		}
	}

	/// <summary>
	/// Generates a cache key for a query using the same algorithm as the Dispatcher
	/// This ensures consistency between cached queries and manual cache operations
	/// </summary>
	public string GenerateKey<TQuery>( TQuery query ) {
		return CacheKeyGenerator.Generate( query );
	}
}

/// <summary>
/// Utility class for generating consistent cache keys
/// Used by both Dispatcher and CacheManager to ensure key compatibility
/// </summary>
internal static class CacheKeyGenerator {

	/// <summary>
	/// Generates a cache key for a query using JSON serialization for parameter consistency
	/// </summary>
	/// <typeparam name="TQuery">The type of query</typeparam>
	/// <param name="query">The query instance</param>
	/// <returns>A cache key string in the format "TypeName:Hash" where Hash is derived from JSON serialization</returns>
	public static string Generate<TQuery>( TQuery query ) {
		var typeName = typeof( TQuery ).Name;

		try {
			// Deterministic serialization to ensure consistency
			var jsonOptions = new JsonSerializerOptions {
				WriteIndented = false,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
				IncludeFields = false
			};

			var serialized = JsonSerializer.Serialize( query, jsonOptions );
			var hash = ComputeStableHash( serialized );

			return $"{typeName}:{hash}";
		} catch {
			// Fallback to GetHashCode() in case of serialization error
			var hashCode = query?.GetHashCode( ) ?? 0;
			return $"{typeName}:{hashCode}";
		}
	}

	/// <summary>
	/// Computes a stable hash from a string using SHA256
	/// </summary>
	/// <param name="input">The input string to hash</param>
	/// <returns>A 16-character hexadecimal hash string</returns>
	private static string ComputeStableHash( string input ) {
		using var sha256 = SHA256.Create( );
		var bytes = Encoding.UTF8.GetBytes( input );
		var hashBytes = sha256.ComputeHash( bytes );
		return Convert.ToHexString( hashBytes )[ ..16 ]; // First 16 characters for performance
	}
}
