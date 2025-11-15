using Microsoft.Extensions.Logging;
using Myth.Interfaces;

namespace Myth.Morph;

/// <summary>
/// Mapping executor for IMorphableTo pattern where the source object defines how to transform to the destination.
/// This executor handles the "To" direction of object transformation.
/// </summary>
/// <typeparam name="TDestination">The destination type for the mapping.</typeparam>
public class ToMappingExecutor<TDestination> : BaseMappingExecutor<TDestination> {

	/// <summary>
	/// Initializes a new instance of the ToMappingExecutor class.
	/// </summary>
	/// <param name="logger">Optional logger for diagnostic information.</param>
	/// <param name="typeResolver">The type resolver for handling inheritance and proxies.</param>
	public ToMappingExecutor( ILogger? logger, TypeResolver typeResolver )
		: base( logger, typeResolver ) {
		Logger?.LogDebug( "Initialized ToMappingExecutor for destination type {DestinationType}", typeof( TDestination ).Name );
	}

	/// <summary>
	/// Applies mappings from a source instance that implements IMorphableTo to a destination instance.
	/// </summary>
	/// <typeparam name="TSource">The source type that implements IMorphableTo.</typeparam>
	/// <param name="source">The source instance to map from.</param>
	/// <param name="destination">The destination instance to map to.</param>
	/// <param name="serviceProvider">The service provider for dependency resolution.</param>
	/// <param name="manuallyMappedProps">Set of property names that have been manually mapped.</param>
	/// <param name="ignoredProperties">Set of property names to ignore during mapping.</param>
	public void ApplyMapping<TSource>( TSource source, TDestination destination, IServiceProvider serviceProvider,
		HashSet<string> manuallyMappedProps, HashSet<string> ignoredProperties )
		where TSource : IMorphableTo<TDestination> {
		if ( source == null )
			throw new ArgumentNullException( nameof( source ) );

		if ( destination == null )
			throw new ArgumentNullException( nameof( destination ) );

		var actualSourceType = TypeResolver.GetActualType( source.GetType( ) );
		var actualDestType = TypeResolver.GetActualType( destination.GetType( ) );

		Logger?.LogDebug( "Applying ToMapping from {SourceType} to {DestinationType}",
			actualSourceType.Name, actualDestType.Name );
		Logger?.LogTrace( "Source implements IMorphableTo<{DestinationType}>", typeof( TDestination ).Name );

		AutoMapProperties( source, destination, serviceProvider, manuallyMappedProps, ignoredProperties );

		Logger?.LogDebug( "Completed ToMapping from {SourceType} to {DestinationType}",
			actualSourceType.Name, actualDestType.Name );
	}
}
