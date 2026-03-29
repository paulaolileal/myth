using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Myth.Extensions;

namespace Myth.Morph;

/// <summary>
/// Cache of compiled generic-invocation delegates.
/// Eliminates <c>MakeGenericMethod</c> and <c>MethodInfo.Invoke</c> overhead
/// in hot mapping paths by compiling delegates once per type pair.
/// </summary>
internal static class MorphDelegateCache {
	/// <summary>
	/// Delegates for SchemaRegistry.Morph&lt;TSource, TDestination&gt;().
	/// Used by MorphExtensions.To&lt;T&gt;() to avoid MakeGenericMethod per call.
	/// </summary>
	private static readonly ConcurrentDictionary<(Type Source, Type Destination), Func<SchemaRegistry, object, object>>
		_morphDelegates = new( );

	/// <summary>
	/// Delegates for MorphExtensions.To&lt;TDestination&gt;().
	/// Used by BaseMappingExecutor.MapValue() for nested type mapping.
	/// </summary>
	private static readonly ConcurrentDictionary<Type, Func<object, IServiceProvider, object?>>
		_nestedMappers = new( );

	private static readonly MethodInfo _morphMethod =
		typeof( SchemaRegistry ).GetMethod( nameof( SchemaRegistry.Morph ) )!;

	private static readonly MethodInfo _toMethod =
		typeof( MorphExtensions ).GetMethod( nameof( MorphExtensions.To ), [ typeof( object ), typeof( IServiceProvider ) ] )!;

	/// <summary>
	/// Returns a compiled delegate for <c>SchemaRegistry.Morph&lt;TSource, TDestination&gt;(source)</c>.
	/// The delegate is compiled once per source/destination pair and cached.
	/// </summary>
	internal static Func<SchemaRegistry, object, object> GetMorphDelegate( Type sourceType, Type destinationType )
		=> _morphDelegates.GetOrAdd( (sourceType, destinationType), BuildMorphDelegate );

	/// <summary>
	/// Returns a compiled delegate for <c>MorphExtensions.To&lt;TDestination&gt;(source, sp)</c>.
	/// Used for nested mapping of complex-type properties inside AutoMapProperties.
	/// </summary>
	internal static Func<object, IServiceProvider, object?> GetNestedMapper( Type destinationType )
		=> _nestedMappers.GetOrAdd( destinationType, BuildNestedMapper );

	private static Func<SchemaRegistry, object, object> BuildMorphDelegate( (Type Source, Type Destination) key ) {
		var genericMethod = _morphMethod.MakeGenericMethod( key.Source, key.Destination );

		var registryParam = Expression.Parameter( typeof( SchemaRegistry ), "registry" );
		var sourceParam = Expression.Parameter( typeof( object ), "source" );

		var typedSource = Expression.Convert( sourceParam, key.Source );
		var call = Expression.Call( registryParam, genericMethod, typedSource );
		var boxed = Expression.Convert( call, typeof( object ) );

		return Expression
			.Lambda<Func<SchemaRegistry, object, object>>( boxed, registryParam, sourceParam )
			.Compile( );
	}

	private static Func<object, IServiceProvider, object?> BuildNestedMapper( Type destinationType ) {
		var genericMethod = _toMethod.MakeGenericMethod( destinationType );

		var sourceParam = Expression.Parameter( typeof( object ), "source" );
		var spParam = Expression.Parameter( typeof( IServiceProvider ), "sp" );

		var call = Expression.Call( null, genericMethod, sourceParam, spParam );
		var boxed = Expression.Convert( call, typeof( object ) );

		return Expression
			.Lambda<Func<object, IServiceProvider, object?>>( boxed, sourceParam, spParam )
			.Compile( );
	}

	/// <summary>Clears all cached delegates. For testing purposes only.</summary>
	internal static void Clear( ) {
		_morphDelegates.Clear( );
		_nestedMappers.Clear( );
	}
}
