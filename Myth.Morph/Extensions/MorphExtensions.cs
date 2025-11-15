using Microsoft.Extensions.Logging;
using Myth.Exceptions;
using Myth.Morph;
using Myth.ServiceProvider;

namespace Myth.Extensions;

/// <summary>
/// Provides extension methods for object transformation and mapping using the Morph system.
/// These extensions enable seamless conversion between different types through a configured binding registry.
/// </summary>
public static class MorphExtensions {

	/// <summary>
	/// Converts an object to the specified destination type using a configured binding registry.
	/// </summary>
	/// <remarks>
	/// This method relies on a binding registry to perform the conversion. Ensure that the dependency
	/// injection container is properly configured by calling <c>ServiceCollectionExtensions.AddMorph()</c> during
	/// application setup. Uses the centralized Myth service provider management for seamless integration.
	/// </remarks>
	/// <typeparam name="TDestination">The type to which the source object will be converted.</typeparam>
	/// <param name="source">The object to be converted. If <paramref name="source"/> is <see langword="null"/>, the method returns the default
	/// value of <typeparamref name="TDestination"/>.</param>
	/// <param name="sp">An optional <see cref="IServiceProvider"/> instance used to resolve dependencies. If not provided, the global
	/// service provider is used.</param>
	/// <returns>An instance of <typeparamref name="TDestination"/> representing the converted object.</returns>
	/// <exception cref="InvalidMorphConfigurationException">Thrown if no service provider is available or if the required <see cref="SchemaRegistry"/> is
	/// not registered in the dependency injection container.</exception>
	public static TDestination To<TDestination>( this object source, IServiceProvider? sp = null ) {
		var serviceProvider = MythServiceProvider.GetOrFallback( sp );
		var logger = GetLogger( serviceProvider );

		if ( source is null ) {
			logger?.LogDebug( "Source object is null, returning default value for type {DestinationType}", typeof( TDestination ).Name );
			return default!;
		}

		var srcType = source.GetType( );
		var destType = typeof( TDestination );

		logger?.LogDebug( "Converting object from {SourceType} to {DestinationType}", srcType.Name, destType.Name );

		var registry = ( SchemaRegistry? )serviceProvider.GetService( typeof( SchemaRegistry ) );
		if ( registry is null ) {
			var errorMessage = $"{nameof( SchemaRegistry )} not found in DI. Verify that {nameof( ServiceCollectionExtensions.AddMorph )}() was called correctly.";
			logger?.LogError( errorMessage );
			throw new InvalidMorphConfigurationException( errorMessage );
		}

		logger?.LogTrace( "Retrieved SchemaRegistry from service provider" );

		var method = typeof( SchemaRegistry )
			.GetMethod( nameof( SchemaRegistry.Morph ) )!
			.MakeGenericMethod( srcType, destType );

		try {
			var result = ( TDestination )method.Invoke( registry, [ source ] )!;
			logger?.LogDebug( "Successfully converted {SourceType} to {DestinationType}", srcType.Name, destType.Name );
			return result;
		} catch ( Exception ex ) {
			logger?.LogError( ex, "Failed to convert {SourceType} to {DestinationType}", srcType.Name, destType.Name );
			throw;
		}
	}

	/// <summary>
	/// Converts each element in the source list to the specified destination type.
	/// </summary>
	/// <typeparam name="TDestination">The type to which each element in the source list will be converted.</typeparam>
	/// <param name="sourceList">The list of objects to be converted. Cannot be null.</param>
	/// <param name="sp">An optional <see cref="IServiceProvider"/> used to assist in the conversion process. If not provided, the global
	/// service provider is used.</param>
	/// <returns>A list of objects of type <typeparamref name="TDestination"/>. Returns an empty list if <paramref
	/// name="sourceList"/> is null or contains no convertible elements.</returns>
	public static IEnumerable<TDestination> To<TDestination>( this IEnumerable<object> sourceList, IServiceProvider? sp = null ) {
		var serviceProvider = MythServiceProvider.GetOrFallback( sp );
		var logger = GetLogger( serviceProvider );

		if ( sourceList is null ) {
			logger?.LogDebug( "Source list is null, returning empty collection" );
			return [ ];
		}

		var sourceArray = sourceList.Where( s => s != null ).ToArray( );
		logger?.LogDebug( "Converting {Count} items to {DestinationType}", sourceArray.Length, typeof( TDestination ).Name );

		return sourceArray
			.Select( s => s.To<TDestination>( serviceProvider ) )
			.AsEnumerable( );
	}

	/// <summary>
	/// Converts a sequence of source objects to a sequence of destination objects using a mapping registry.
	/// </summary>
	/// <remarks>This method uses a <see cref="SchemaRegistry"/> to perform the mapping between source and
	/// destination types. Ensure that the <see cref="SchemaRegistry"/> is properly registered in the dependency injection
	/// container. Uses the centralized Myth service provider management for seamless integration.</remarks>
	/// <typeparam name="TSource">The type of the source objects in the input sequence.</typeparam>
	/// <typeparam name="TDestination">The type of the destination objects in the output sequence.</typeparam>
	/// <param name="sourceList">The sequence of source objects to be converted. Cannot be null.</param>
	/// <param name="sp">An optional <see cref="IServiceProvider"/> used to resolve dependencies for the mapping process. If not provided,
	/// the global service provider will be used.</param>
	/// <returns>A sequence of destination objects mapped from the source objects. Returns an empty sequence if <paramref
	/// name="sourceList"/> is null.</returns>
	/// <exception cref="InvalidMorphConfigurationException">Thrown if no service provider is available or if the required <see cref="SchemaRegistry"/> is
	/// not found in the dependency injection container.</exception>
	public static IEnumerable<TDestination> To<TSource, TDestination>( this IEnumerable<TSource> sourceList, IServiceProvider? sp = null ) {
		var serviceProvider = MythServiceProvider.GetOrFallback( sp );
		var logger = GetLogger( serviceProvider );

		if ( sourceList is null ) {
			logger?.LogDebug( "Source list is null, returning empty collection" );
			return [ ];
		}

		var registry = ( SchemaRegistry )serviceProvider.GetService( typeof( SchemaRegistry ) )!;
		if ( registry is null ) {
			var errorMessage = $"{nameof( SchemaRegistry )} can't be found in the DI.";
			logger?.LogError( errorMessage );
			throw new InvalidMorphConfigurationException( errorMessage );
		}

		var sourceArray = sourceList.Where( item => item != null ).ToArray( );
		logger?.LogDebug( "Converting {Count} items from {SourceType} to {DestinationType}", sourceArray.Length, typeof( TSource ).Name, typeof( TDestination ).Name );

		var result = new List<TDestination>( );
		foreach ( var item in sourceArray ) {
			try {
				var mapped = registry.Morph<TSource, TDestination>( item );
				result.Add( mapped );
			} catch ( Exception ex ) {
				logger?.LogWarning( ex, "Failed to map item of type {SourceType} to {DestinationType}, skipping", typeof( TSource ).Name, typeof( TDestination ).Name );
			}
		}

		logger?.LogDebug( "Successfully converted {ProcessedCount} out of {TotalCount} items", result.Count, sourceArray.Length );
		return result.AsEnumerable( );
	}

	/// <summary>
	/// Converts the specified source object to the specified destination type asynchronously.
	/// </summary>
	/// <remarks>This method wraps the synchronous conversion logic provided by <c>To&lt;TDestination&gt;</c> in
	/// an asynchronous operation. It is useful for scenarios where asynchronous execution is required or
	/// preferred. Uses the centralized Myth service provider management for seamless integration.</remarks>
	/// <typeparam name="TDestination">The type to which the source object will be converted.</typeparam>
	/// <param name="source">The object to be converted. Cannot be <see langword="null"/>.</param>
	/// <param name="sp">An optional <see cref="IServiceProvider"/> instance that may be used during the conversion process. If not provided,
	/// the global service provider is used.</param>
	/// <returns>A task representing the asynchronous operation. The result contains the converted object of type <typeparamref
	/// name="TDestination"/>.</returns>
	public static async Task<TDestination> ToAsync<TDestination>( this object source, IServiceProvider? sp = null ) {
		var serviceProvider = MythServiceProvider.GetOrFallback( sp );
		var logger = GetLogger( serviceProvider );
		logger?.LogTrace( "Starting async conversion from {SourceType} to {DestinationType}", source?.GetType( ).Name ?? "null", typeof( TDestination ).Name );

		return await Task.FromResult( source.To<TDestination>( serviceProvider ) );
	}

	/// <summary>
	/// Converts a collection of objects to a list of asynchronous results of the specified type.
	/// </summary>
	/// <remarks>This method filters out null objects from the source collection before performing the conversion.
	/// Each object in the collection is converted asynchronously using the <c>ToAsync&lt;TDestination&gt;</c>
	/// method. Uses the centralized Myth service provider management for seamless integration.</remarks>
	/// <typeparam name="TDestination">The type to which each object in the source collection will be converted.</typeparam>
	/// <param name="sourceList">The collection of objects to be converted. Objects in the collection must not be null.</param>
	/// <param name="sp">An optional <see cref="IServiceProvider"/> used to resolve dependencies during the conversion process. If not provided,
	/// the global service provider is used.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains a list of <typeparamref
	/// name="TDestination"/> objects.</returns>
	public static async Task<IEnumerable<TDestination>> ToAsync<TDestination>( this IEnumerable<object> sourceList, IServiceProvider? sp = null ) {
		var serviceProvider = MythServiceProvider.GetOrFallback( sp );
		var logger = GetLogger( serviceProvider );

		if ( sourceList is null ) {
			logger?.LogDebug( "Source list is null, returning empty collection" );
			return [ ];
		}

		var validItems = sourceList.Where( s => s != null ).ToArray( );
		logger?.LogDebug( "Starting async conversion of {Count} items to {DestinationType}", validItems.Length, typeof( TDestination ).Name );

		var tasks = validItems.Select( s => s.ToAsync<TDestination>( serviceProvider ) );
		var result = await Task.WhenAll( tasks );

		logger?.LogDebug( "Completed async conversion of {Count} items", result.Length );
		return result.AsEnumerable( );
	}

	/// <summary>
	/// Determines whether the specified source object can be bound to the specified destination type.
	/// </summary>
	/// <remarks>This method checks whether a binding exists between the type of the source object and the
	/// specified destination type. It uses a <see cref="SchemaRegistry"/> resolved from the global service provider
	/// or the provided service provider. If no binding registry is available or an error occurs
	/// during the check, the method returns <see langword="false"/>.</remarks>
	/// <typeparam name="TDestination">The type of the destination to check for binding compatibility.</typeparam>
	/// <param name="source">The source object to evaluate for binding compatibility. Cannot be <see langword="null"/>.</param>
	/// <param name="sp">An optional <see cref="IServiceProvider"/> instance used to resolve binding dependencies. If not provided,
	/// the global service provider is used.</param>
	/// <returns><see langword="true"/> if the source object can be bound to the specified destination type; otherwise, <see
	/// langword="false"/>.</returns>
	public static bool CanBindTo<TDestination>( this object source, IServiceProvider? sp = null ) {
		if ( source is null ) {
			return false;
		}

		try {
			var serviceProvider = MythServiceProvider.GetOrFallback( sp );
			var logger = GetLogger( serviceProvider );

			var registry = ( SchemaRegistry? )serviceProvider.GetService( typeof( SchemaRegistry ) );
			if ( registry is null ) {
				logger?.LogTrace( "SchemaRegistry not available, cannot check binding for {SourceType} to {DestinationType}", source.GetType( ).Name, typeof( TDestination ).Name );
				return false;
			}

			var canBind = registry.HasMapping( source.GetType( ), typeof( TDestination ) );
			logger?.LogTrace( "Binding check for {SourceType} to {DestinationType}: {CanBind}", source.GetType( ).Name, typeof( TDestination ).Name, canBind );
			return canBind;
		} catch ( Exception ex ) {
			return false;
		}
	}

	/// <summary>
	/// Determines whether the specified source object can be bound to the specified destination type.
	/// </summary>
	/// <remarks>This method checks for binding compatibility by querying a <see cref="SchemaRegistry"/> service. If
	/// the <paramref name="sp"/> parameter is not provided, the method attempts to use the global service provider. If no
	/// <see cref="SchemaRegistry"/> is available or an error occurs during the check, the method returns <see
	/// langword="false"/>.</remarks>
	/// <typeparam name="TSource">The type of the source object.</typeparam>
	/// <typeparam name="TDestination">The type of the destination object.</typeparam>
	/// <param name="source">The source object to check for binding compatibility. Cannot be <see langword="null"/>.</param>
	/// <param name="sp">An optional <see cref="IServiceProvider"/> instance used to resolve binding dependencies. If not provided,
	/// the global service provider is used.</param>
	/// <returns><see langword="true"/> if the source object can be bound to the destination type; otherwise, <see
	/// langword="false"/>.</returns>
	public static bool CanBindTo<TSource, TDestination>( this TSource source, IServiceProvider? sp = null ) {
		if ( source is null ) {
			return false;
		}

		try {
			var serviceProvider = MythServiceProvider.GetOrFallback( sp );
			var logger = GetLogger( serviceProvider );

			var registry = ( SchemaRegistry? )serviceProvider.GetService( typeof( SchemaRegistry ) );
			if ( registry is null ) {
				logger?.LogTrace( "SchemaRegistry not available, cannot check binding for {SourceType} to {DestinationType}", typeof( TSource ).Name, typeof( TDestination ).Name );
				return false;
			}

			var canBind = registry.HasMapping( typeof( TSource ), typeof( TDestination ) );
			logger?.LogTrace( "Binding check for {SourceType} to {DestinationType}: {CanBind}", typeof( TSource ).Name, typeof( TDestination ).Name, canBind );
			return canBind;
		} catch ( Exception ex ) {
			return false;
		}
	}

	/// <summary>
	/// Gets a logger instance from the service provider if available.
	/// </summary>
	/// <param name="sp">The service provider to resolve the logger from.</param>
	/// <returns>An ILogger instance or null if not available.</returns>
	private static ILogger? GetLogger( IServiceProvider? sp = null ) {
		try {
			var serviceProvider = MythServiceProvider.GetOrFallback( sp );
			return serviceProvider?.GetService( typeof( ILogger<> ).MakeGenericType( typeof( MorphExtensions ) ) ) as ILogger;
		} catch {
			return null;
		}
	}
}
