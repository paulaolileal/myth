namespace Myth.Models;

/// <summary>
/// Represents cached value with metadata
/// </summary>
public readonly struct CacheValue<T> {

	/// <summary>
	/// Indicates whether a value was found in the cache
	/// </summary>
	public bool HasValue { get; }

	/// <summary>
	/// The cached value, or default if not found
	/// </summary>
	public T Value { get; }

	private CacheValue( bool hasValue, T value ) {
		HasValue = hasValue;
		Value = value;
	}

	/// <summary>
	/// Creates a cache hit result with the cached value
	/// </summary>
	/// <param name="value">The value retrieved from cache</param>
	/// <returns>A CacheValue indicating a cache hit</returns>
	public static CacheValue<T> Hit( T value ) => new( true, value );

	/// <summary>
	/// Creates a cache miss result
	/// </summary>
	/// <returns>A CacheValue indicating a cache miss</returns>
	public static CacheValue<T> Miss( ) => new( false, default! );
}