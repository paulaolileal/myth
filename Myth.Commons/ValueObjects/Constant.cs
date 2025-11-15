using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Myth.Extensions;

namespace Myth.ValueObjects;

/// <summary>
/// A base for creating constants highly typed with excellent developer experience.
/// Provides simplified syntax without repetitive generics while maintaining full functionality.
/// </summary>
/// <typeparam name="TSelf">The derived type (CRTP pattern for static methods)</typeparam>
/// <typeparam name="TValue">The value type</typeparam>
public abstract class Constant<TSelf, TValue> : IEquatable<Constant<TSelf, TValue>>, IComparable<Constant<TSelf, TValue>>
	where TSelf : Constant<TSelf, TValue>
	where TValue : IEquatable<TValue>, IComparable<TValue> {

	private static readonly ConcurrentDictionary<Type, List<Constant<TSelf, TValue>>> _instancesByType = new( );
	private static readonly ConcurrentDictionary<Type, bool> _initializedTypes = new( );

	public string Name { get; }
	public TValue Value { get; }

	/// <summary>
	/// Initializes a new instance of the Constant class
	/// </summary>
	/// <param name="name">Name of the constant</param>
	/// <param name="value">Value of the constant</param>
	protected Constant( string name, TValue value ) {
		Name = name ?? throw new ArgumentNullException( nameof( name ) );
		Value = value ?? throw new ArgumentNullException( nameof( value ) );

		RegisterInstance( this );
	}

	/// <summary>
	/// Creates a new constant instance using the calling member name
	/// </summary>
	/// <param name="value">Value of the constant</param>
	/// <param name="memberName">The name of the calling member (automatically provided by CallerMemberName)</param>
	protected static TSelf CreateWithCallerName( TValue value, [CallerMemberName] string memberName = "" ) {
		return ( TSelf )Activator.CreateInstance( typeof( TSelf ), memberName, value )!;
	}

	/// <summary>
	/// Registers an instance for the type registry
	/// </summary>
	private void RegisterInstance( Constant<TSelf, TValue> instance ) {
		var type = instance.GetType( );

		_instancesByType.AddOrUpdate( type,
			[ instance ],
			( _, existing ) => {
				if ( !existing.Any( x => x.Value.Equals( instance.Value ) ) ) {
					existing.Add( instance );
				}
				return existing;
			} );
	}

	/// <summary>
	/// Ensures all static instances of the given type are loaded
	/// </summary>
	private static void EnsureInitialized( ) {
		var type = typeof( TSelf );

		if ( _initializedTypes.ContainsKey( type ) )
			return;

		// Force static constructor execution to register all instances
		RuntimeHelpers.RunClassConstructor( type.TypeHandle );

		// Also initialize static readonly fields
		var fields = type.GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
			.Where( f => f.IsInitOnly && typeof( TSelf ).IsAssignableFrom( f.FieldType ) );

		foreach ( var field in fields ) {
			_ = field.GetValue( null ); // This will trigger field initialization
		}

		_initializedTypes.TryAdd( type, true );
	}

	/// <summary>
	/// Gets all instances of this constant type
	/// Usage: OrderStatus.GetAll()
	/// </summary>
	public static IReadOnlyList<TSelf> GetAll( ) {
		EnsureInitialized( );

		var type = typeof( TSelf );
		return _instancesByType.TryGetValue( type, out var instances )
			? instances.Cast<TSelf>( ).ToList( ).AsReadOnly( )
			: new List<TSelf>( ).AsReadOnly( );
	}

	/// <summary>
	/// Finds a constant by its value
	/// Usage: OrderStatus.FromValue("A")
	/// </summary>
	public static TSelf FromValue( TValue value ) {
		EnsureInitialized( );

		var instance = GetAll( ).FirstOrDefault( x => x.Value.Equals( value ) );
		return instance ?? throw new ConstantNotFoundException( $"No {typeof( TSelf ).Name} with value '{value}' found" );
	}

	/// <summary>
	/// Finds a constant by its name
	/// Usage: OrderStatus.FromName("Active")
	/// </summary>
	public static TSelf FromName( string name ) {
		EnsureInitialized( );

		var instance = GetAll( ).FirstOrDefault( x => x.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) );
		return instance ?? throw new ConstantNotFoundException( $"No {typeof( TSelf ).Name} with name '{name}' found" );
	}

	/// <summary>
	/// Tries to find a constant by its value
	/// Usage: OrderStatus.TryFromValue("A", out var result)
	/// </summary>
	public static bool TryFromValue( TValue value, out TSelf? result ) {
		EnsureInitialized( );

		result = GetAll( ).FirstOrDefault( x => x.Value.Equals( value ) );
		return result != null;
	}

	/// <summary>
	/// Tries to find a constant by its name (case insensitive)
	/// Usage: OrderStatus.TryFromName("Active", out var result)
	/// </summary>
	public static bool TryFromName( string name, out TSelf? result ) {
		EnsureInitialized( );

		result = GetAll( ).FirstOrDefault( x => x.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) );
		return result != null;
	}

	/// <summary>
	/// Gets a formatted string with all available options
	/// Usage: OrderStatus.GetOptions()
	/// </summary>
	public static string GetOptions( ) {
		EnsureInitialized( );

		return GetAll( )
			.OrderBy( x => x.Value )
			.Select( x => $"{x.Value}: {x.Name}" )
			.ToStringWithSeparator( " | " );
	}

	/// <summary>
	/// Allows easy iteration in foreach and switch expressions
	/// Usage: foreach(var status in OrderStatus.All) { ... }
	/// </summary>
	public static IEnumerable<TSelf> All => GetAll( );

	/// <summary>
	/// Gets all values for easy switch expressions
	/// Usage: status.Value switch { OrderStatus.Values.Pending => ..., _ => ... }
	/// </summary>
	public static class Values {
		static Values( ) {
			// Force initialization of the parent type
			_ = GetAll( );
		}

		/// <summary>
		/// Gets all values as an enumerable for pattern matching
		/// </summary>
		public static IEnumerable<TValue> All => GetAll( ).Select( x => x.Value );
	}

	// Implicit conversion for great DX and Entity Framework compatibility
	public static implicit operator TValue( Constant<TSelf, TValue> constant ) => constant.Value;

	// IEquatable implementation
	public bool Equals( Constant<TSelf, TValue>? other ) {
		return other != null &&
			   GetType( ) == other.GetType( ) &&
			   Value.Equals( other.Value );
	}

	public override bool Equals( object? obj ) => Equals( obj as Constant<TSelf, TValue> );
	public override int GetHashCode( ) => HashCode.Combine( GetType( ), Value );
	public override string ToString( ) => Name;

	// IComparable implementation for switch support
	public int CompareTo( Constant<TSelf, TValue>? other ) {
		if ( other == null )
			return 1;
		return Value.CompareTo( other.Value );
	}

	// Operators
	public static bool operator ==( Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right ) =>
		EqualityComparer<Constant<TSelf, TValue>>.Default.Equals( left, right );

	public static bool operator !=( Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right ) => !( left == right );

	public static bool operator <( Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right ) =>
		left?.CompareTo( right ) < 0;

	public static bool operator >( Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right ) =>
		left?.CompareTo( right ) > 0;

	public static bool operator <=( Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right ) =>
		left?.CompareTo( right ) <= 0;

	public static bool operator >=( Constant<TSelf, TValue>? left, Constant<TSelf, TValue>? right ) =>
		left?.CompareTo( right ) >= 0;
}
