using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Myth.Settings;

namespace Myth.Morph;

/// <summary>
/// Produces <see cref="IFromMappingApplier"/> instances without per-call reflection overhead.
/// Builds and caches a compiled constructor delegate for each destination type on first access,
/// replacing the <c>MakeGenericType + Activator.CreateInstance + GetMethod + Invoke</c> pattern
/// in <see cref="Schema{TDestination}"/>.
/// </summary>
internal static class FromMappingExecutorFactory {
	private static readonly ConcurrentDictionary<Type, Func<ILogger?, TypeResolver, NullPropertyBehavior, IFromMappingApplier>>
		_factories = new( );

	/// <summary>
	/// Returns an <see cref="IFromMappingApplier"/> for the given destination type.
	/// The constructor delegate is compiled on first access per type and cached.
	/// </summary>
	internal static IFromMappingApplier Create(
		Type destinationType,
		ILogger? logger,
		TypeResolver typeResolver,
		NullPropertyBehavior nullBehavior ) {
		var factory = _factories.GetOrAdd( destinationType, BuildFactory );
		return factory( logger, typeResolver, nullBehavior );
	}

	private static Func<ILogger?, TypeResolver, NullPropertyBehavior, IFromMappingApplier> BuildFactory( Type destinationType ) {
		var executorType = typeof( FromMappingExecutor<> ).MakeGenericType( destinationType );

		var ctor = executorType.GetConstructor( [ typeof( ILogger ), typeof( TypeResolver ), typeof( NullPropertyBehavior ) ] )
			?? throw new InvalidOperationException(
				$"Could not find expected constructor on {executorType.Name}. " +
				$"Expected (ILogger?, TypeResolver, NullPropertyBehavior)." );

		var loggerParam = Expression.Parameter( typeof( ILogger ), "logger" );
		var resolverParam = Expression.Parameter( typeof( TypeResolver ), "resolver" );
		var behaviorParam = Expression.Parameter( typeof( NullPropertyBehavior ), "behavior" );

		var newExpr = Expression.New( ctor, loggerParam, resolverParam, behaviorParam );
		var cast = Expression.Convert( newExpr, typeof( IFromMappingApplier ) );

		return Expression
			.Lambda<Func<ILogger?, TypeResolver, NullPropertyBehavior, IFromMappingApplier>>(
				cast, loggerParam, resolverParam, behaviorParam )
			.Compile( );
	}

	/// <summary>Clears all cached factories. For testing purposes only.</summary>
	internal static void Clear( ) => _factories.Clear( );
}
