using System.Collections;
using System.Collections.Immutable;

namespace Myth.Morph.Generator.Helpers;

/// <summary>
/// An immutable array wrapper that implements structural equality.
/// Required for Roslyn incremental generator models: the incremental pipeline
/// uses equality to skip re-generation when inputs have not changed.
/// </summary>
/// <typeparam name="T">Element type. Must implement <see cref="IEquatable{T}"/>.</typeparam>
/// <remarks>Initializes the array with the provided immutable array.</remarks>
internal readonly struct EquatableArray<T>( ImmutableArray<T> items ) : IEquatable<EquatableArray<T>>, IEnumerable<T>
	where T : IEquatable<T> {

	private readonly ImmutableArray<T> _items = items;

	/// <summary>Number of elements in the array.</summary>
	public int Count => _items.IsDefaultOrEmpty ? 0 : _items.Length;

	/// <summary>Returns <c>true</c> when the array has no elements.</summary>
	public bool IsEmpty => _items.IsDefaultOrEmpty;

	/// <summary>Gets the element at the given index.</summary>
	public T this[ int index ] => _items[ index ];

	/// <inheritdoc/>
	public bool Equals( EquatableArray<T> other ) {
		if ( _items.IsDefaultOrEmpty && other._items.IsDefaultOrEmpty )
			return true;

		if ( _items.IsDefaultOrEmpty || other._items.IsDefaultOrEmpty )
			return false;

		if ( _items.Length != other._items.Length )
			return false;

		for ( var i = 0; i < _items.Length; i++ ) {
			if ( !_items[ i ].Equals( other._items[ i ] ) )
				return false;
		}

		return true;
	}

	/// <inheritdoc/>
	public override bool Equals( object? obj )
		=> obj is EquatableArray<T> other && Equals( other );

	/// <inheritdoc/>
	public override int GetHashCode( ) {
		if ( _items.IsDefaultOrEmpty )
			return 0;

		var hash = 17;
		foreach ( var item in _items )
			hash = hash * 31 + item.GetHashCode( );
		return hash;
	}

	/// <inheritdoc/>
	public ImmutableArray<T>.Enumerator GetEnumerator( ) => _items.GetEnumerator( );

	IEnumerator<T> IEnumerable<T>.GetEnumerator( ) => ( ( IEnumerable<T> )_items ).GetEnumerator( );

	IEnumerator IEnumerable.GetEnumerator( ) => ( ( IEnumerable )_items ).GetEnumerator( );

	public static bool operator ==( EquatableArray<T> left, EquatableArray<T> right ) => left.Equals( right );
	public static bool operator !=( EquatableArray<T> left, EquatableArray<T> right ) => !left.Equals( right );
}
