namespace Myth.Models;

/// <summary>
/// Represents cached value with metadata
/// </summary>
public readonly struct CacheValue<T> {
	public bool HasValue { get; }
	public T Value { get; }

	private CacheValue( bool hasValue, T value ) {
		HasValue = hasValue;
		Value = value;
	}

	public static CacheValue<T> Hit( T value ) => new( true, value );

	public static CacheValue<T> Miss( ) => new( false, default! );
}