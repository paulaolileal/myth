using Microsoft.Extensions.Logging;
using Myth.Settings;

namespace Myth.Morph;

/// <summary>
/// Mapping executor for IMorphableFrom pattern where the destination object defines how to be created from the source.
/// This executor handles the "From" direction of object transformation.
/// </summary>
/// <typeparam name="TDestination">The destination type for the mapping.</typeparam>
public class FromMappingExecutor<TDestination> : BaseMappingExecutor<TDestination>, IFromMappingApplier {

	/// <summary>
	/// Initializes a new instance of the FromMappingExecutor class.
	/// </summary>
	/// <param name="logger">Optional logger for diagnostic information.</param>
	/// <param name="typeResolver">The type resolver for handling inheritance and proxies.</param>
	/// <param name="nullBehavior">Behavior when a source property value is null during auto-mapping.</param>
	public FromMappingExecutor( ILogger? logger, TypeResolver typeResolver, NullPropertyBehavior nullBehavior = NullPropertyBehavior.AssignDefault )
		: base( logger, typeResolver, nullBehavior ) {
		Logger?.LogDebug( "Initialized FromMappingExecutor for destination type {DestinationType}", typeof( TDestination ).Name );
	}

	/// <summary>
	/// Applies mappings from a source instance to a destination that implements IMorphableFrom.
	/// </summary>
	/// <param name="source">The source instance to map from.</param>
	/// <param name="destination">The destination instance to map to.</param>
	/// <param name="serviceProvider">The service provider for dependency resolution.</param>
	/// <param name="manuallyMappedProps">Set of property names that have been manually mapped.</param>
	/// <param name="ignoredProperties">Set of property names to ignore during mapping.</param>
	public void ApplyMapping( object source, TDestination destination, IServiceProvider serviceProvider,
		HashSet<string> manuallyMappedProps, HashSet<string> ignoredProperties ) {
		if ( source == null )
			throw new ArgumentNullException( nameof( source ) );

		if ( destination == null )
			throw new ArgumentNullException( nameof( destination ) );

		var actualSourceType = TypeResolver.GetActualType( source.GetType( ) );
		var actualDestType = TypeResolver.GetActualType( destination.GetType( ) );

		Logger?.LogDebug( "Applying FromMapping from {SourceType} to {DestinationType}",
			actualSourceType.Name, actualDestType.Name );
		Logger?.LogTrace( "Destination implements IMorphableFrom<{SourceType}>", actualSourceType.Name );

		AutoMapProperties( source, destination, serviceProvider, manuallyMappedProps, ignoredProperties );

		Logger?.LogDebug( "Completed FromMapping from {SourceType} to {DestinationType}",
			actualSourceType.Name, actualDestType.Name );
	}

	/// <summary>
	/// Explicit implementation of IFromMappingApplier, allowing Schema&lt;T&gt; to invoke
	/// this executor without reflection when the destination type is only known at runtime.
	/// </summary>
	void IFromMappingApplier.ApplyMapping(
		object source,
		object destination,
		IServiceProvider serviceProvider,
		HashSet<string> manuallyMappedProps,
		HashSet<string> ignoredProperties )
		=> ApplyMapping( source, (TDestination)destination, serviceProvider, manuallyMappedProps, ignoredProperties );
}
