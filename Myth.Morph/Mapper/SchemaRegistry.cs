using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.ServiceProvider;
using Myth.Settings;

namespace Myth.Morph;

/// <summary>
/// Central registry for managing object transformation schemas and mappings.
/// This class handles the registration and execution of type mappings, supporting both
/// generic mappings and instance-based mappings through the IMorphable interface.
/// Includes inheritance fallback support for handling derived classes and type hierarchies.
/// </summary>
public class SchemaRegistry {
	private readonly IServiceProvider _sp;
	private readonly ILogger<SchemaRegistry>? _logger;
	private readonly MorphSettings _settings;
	private readonly Dictionary<(Type, Type), object> _builders = [ ];
	private readonly Dictionary<Type, Type> _genericInterfaceToConcrete = [ ];
	private readonly List<Action<Type, Type>> _genericRegisters = [ ];
	private readonly HashSet<(Type, Type)> _instanceBasedMappings = [ ];
	private readonly TypeResolver _typeResolver;

	private readonly ConcurrentDictionary<(Type, Type), (Type, Type)?> _mappingFallbackCache = new( );

	/// <summary>
	/// Initializes a new instance of the SchemaRegistry with the specified service provider.
	/// </summary>
	/// <param name="sp">The service provider for dependency injection and logger resolution.</param>
	/// <param name="settings">The MorphSettings configuration for inheritance fallback and other options.</param>
	public SchemaRegistry( IServiceProvider sp, MorphSettings? settings = null ) {
		_sp = sp;
		_settings = settings ?? new MorphSettings( );
		var serviceProvider = MythServiceProvider.GetOrFallback( sp );
		_logger = serviceProvider?.GetService<ILogger<SchemaRegistry>>( );

		_typeResolver = new TypeResolver(
			_logger as ILogger<TypeResolver>,
			_settings.MaxInheritanceDepth,
			_settings.IncludeInterfacesInFallback,
			_settings.ResolveToBaseType );

		_logger?.LogDebug( "SchemaRegistry initialized with inheritance fallback: {InheritanceFallback}, maxDepth: {MaxDepth}, resolveToBaseType: {ResolveToBaseType}",
			_settings.EnableInheritanceFallback, _settings.MaxInheritanceDepth, _settings.ResolveToBaseType );
	}

	/// <summary>
	/// Registers an instance-based mapping between source and destination types.
	/// This type of mapping uses the IMorphable interface implemented by the source type
	/// to define custom transformation logic.
	/// </summary>
	/// <param name="sourceType">The source type that implements IMorphable&lt;TDestination&gt;.</param>
	/// <param name="destinationType">The destination type to map to.</param>
	public void RegisterInstanceBasedMapping( Type sourceType, Type destinationType ) {
		_instanceBasedMappings.Add( (sourceType, destinationType) );
		_logger?.LogTrace( "Registered instance-based mapping: {SourceType} -> {DestinationType}", sourceType.Name, destinationType.Name );
	}

	/// <summary>
	/// Registers a generic type mapping between an interface and its concrete implementation.
	/// This enables automatic resolution of interface types to concrete types during mapping.
	/// </summary>
	/// <param name="ifaceGeneric">The generic interface type definition.</param>
	/// <param name="concreteGeneric">The concrete type definition that implements the interface.</param>
	public void RegisterGenericMapping( Type ifaceGeneric, Type concreteGeneric ) {
		_genericInterfaceToConcrete[ ifaceGeneric ] = concreteGeneric;
		_logger?.LogTrace( "Registered generic mapping: {Interface} -> {Concrete}", ifaceGeneric.Name, concreteGeneric.Name );
	}

	/// <summary>
	/// Transforms a source object to the specified destination type using registered mappings.
	/// </summary>
	/// <typeparam name="TSource">The type of the source object.</typeparam>
	/// <typeparam name="TDestination">The type of the destination object.</typeparam>
	/// <param name="source">The source object to transform.</param>
	/// <returns>The transformed destination object.</returns>
	/// <exception cref="BinderNotFoundException">Thrown when no mapping is found for the specified types.</exception>
	public TDestination Morph<TSource, TDestination>( TSource source ) {
		var sourceType = typeof( TSource );
		var destinationType = typeof( TDestination );

		_logger?.LogDebug( "Starting transformation from {SourceType} to {DestinationType}", sourceType.Name, destinationType.Name );

		// If source is null, return default
		if ( source == null ) {
			_logger?.LogDebug( "Source is null, returning default value for {DestinationType}", destinationType.Name );
			return default!;
		}

		// Use the actual type of the object (resolving inheritance) if different from the generic type
		var runtimeSourceType = source.GetType( );
		var actualSourceType = _typeResolver.GetActualType( runtimeSourceType );

		if ( actualSourceType != sourceType ) {
			_logger?.LogTrace( "Using actual source type {ActualType} instead of generic type {GenericType} (runtime type: {RuntimeType})",
				actualSourceType.Name, sourceType.Name, runtimeSourceType.Name );
		}

		if ( actualSourceType != runtimeSourceType && _typeResolver.HasBaseType( runtimeSourceType ) ) {
			_logger?.LogDebug( "Resolved derived type {DerivedType} to base type {ActualType}",
				runtimeSourceType.Name, actualSourceType.Name );
		}

		// Check if it's an instance-based mapping with inheritance fallback support
		if ( TryGetInstanceBasedMapping( actualSourceType, destinationType ) ) {
			// Check for IMorphableTo<TDestination> pattern (source defines how to transform to destination)
			if ( source is IMorphableTo<TDestination> mapToInstance ) {
				_logger?.LogDebug( "Using IMorphableTo instance-based mapping for {SourceType} -> {DestinationType}", actualSourceType.Name, destinationType.Name );
				return MapFromMorphableTo( mapToInstance, destinationType );
			}
		}

		// Check for IMorphableFrom<TSource> pattern (destination defines how to create from source)
		if ( TryGetMorphableFromMapping( actualSourceType, destinationType ) ) {
			_logger?.LogDebug( "Using IMorphableFrom instance-based mapping for {SourceType} -> {DestinationType}", actualSourceType.Name, destinationType.Name );
			return MapFromMorphableFrom<TSource, TDestination>( source, actualSourceType, destinationType );
		}

		// Determine the concrete destination type
		var concreteDestinationType = ResolveConcreteDestinationType( destinationType );
		if ( concreteDestinationType != destinationType ) {
			_logger?.LogTrace( "Resolved concrete destination type {ConcreteType} for interface {InterfaceType}", concreteDestinationType.Name, destinationType.Name );
		}

		// Check if it's a generic type mapping
		if ( IsGenericMapping( actualSourceType, destinationType ) ) {
			_logger?.LogDebug( "Using generic type mapping for {SourceType} -> {DestinationType}", actualSourceType.Name, destinationType.Name );
			return MapGenericTypes<TSource, TDestination>( source, actualSourceType, destinationType, concreteDestinationType );
		}

		// First try to find a direct builder for the original types
		if ( !TryGetBuilder( actualSourceType, destinationType, out var builderObj ) ) {
			// If not found, try with the concrete type
			if ( !TryGetBuilder( actualSourceType, concreteDestinationType, out builderObj ) ) {
				// Try to register dynamically
				_logger?.LogTrace( "No existing builder found, attempting dynamic registration" );
				foreach ( var register in _genericRegisters ) {
					register( actualSourceType, destinationType );
					register( actualSourceType, concreteDestinationType );
				}

				// Try again after registering
				if ( !TryGetBuilder( actualSourceType, destinationType, out builderObj ) &&
					!TryGetBuilder( actualSourceType, concreteDestinationType, out builderObj ) ) {
					var errorMessage = $"No mapping registered from {actualSourceType} to {destinationType}";
					_logger?.LogError( errorMessage );
					throw new BinderNotFoundException( errorMessage );
				}
			}
		}

		// Create destination instance using the concrete type
		var dest = CreateInstance( concreteDestinationType );
		_logger?.LogTrace( "Created destination instance of type {Type}", concreteDestinationType.Name );

		// Apply mapping using reflection if necessary
		if ( actualSourceType != sourceType || concreteDestinationType != destinationType ) {
			_logger?.LogTrace( "Applying dynamic mapping due to type differences" );
			return ApplyMappingDynamically<TDestination>(
				source,
				actualSourceType,
				dest,
				concreteDestinationType,
				destinationType );
		}

		_logger?.LogDebug( "Transformation completed successfully from {SourceType} to {DestinationType}", sourceType.Name, destinationType.Name );
		return ( TDestination )dest;
	}

	/// <summary>
	/// Determines if the mapping between source and destination types should use generic mapping logic.
	/// </summary>
	/// <param name="sourceType">The source type to check.</param>
	/// <param name="destinationType">The destination type to check.</param>
	/// <returns>True if generic mapping should be used, false otherwise.</returns>
	private bool IsGenericMapping( Type sourceType, Type destinationType ) {
		// Check if both are generic types
		if ( !sourceType.IsGenericType || !destinationType.IsGenericType )
			return false;

		var srcGenericDef = sourceType.GetGenericTypeDefinition( );
		var dstGenericDef = destinationType.GetGenericTypeDefinition( );

		// Check if we have a registered generic mapping
		var hasMapping = _genericInterfaceToConcrete.ContainsKey( dstGenericDef );
		_logger?.LogTrace( "Generic mapping check for {SourceType} -> {DestinationType}: {HasMapping}", srcGenericDef.Name, dstGenericDef.Name, hasMapping );
		return hasMapping;
	}

	/// <summary>
	/// Performs generic type mapping between compatible generic types.
	/// </summary>
	/// <typeparam name="TSource">The source generic type.</typeparam>
	/// <typeparam name="TDestination">The destination generic type.</typeparam>
	/// <param name="source">The source object instance.</param>
	/// <param name="actualSourceType">The actual runtime type of the source.</param>
	/// <param name="destinationType">The destination type.</param>
	/// <param name="concreteDestinationType">The concrete implementation type of the destination.</param>
	/// <returns>The mapped destination object.</returns>
	private TDestination MapGenericTypes<TSource, TDestination>(
		TSource source,
		Type actualSourceType,
		Type destinationType,
		Type concreteDestinationType ) {
		_logger?.LogDebug( "Performing generic type mapping from {SourceType} to {ConcreteDestType}", actualSourceType.Name, concreteDestinationType.Name );

		// Create an instance of the concrete type
		var dest = CreateInstance( concreteDestinationType );

		// Map properties using reflection
		MapPropertiesGeneric( source, dest, actualSourceType, concreteDestinationType );

		return ( TDestination )dest;
	}

	/// <summary>
	/// Maps properties between generic types using reflection-based property matching.
	/// </summary>
	/// <param name="source">The source object.</param>
	/// <param name="dest">The destination object.</param>
	/// <param name="sourceType">The source type.</param>
	/// <param name="destType">The destination type.</param>
	private void MapPropertiesGeneric( object source, object dest, Type sourceType, Type destType ) {
		var srcProperties = sourceType
			.GetProperties( BindingFlags.Public | BindingFlags.Instance )
			.Where( p => p.CanRead )
			.ToArray( );

		var destProperties = destType
			.GetProperties( BindingFlags.Public | BindingFlags.Instance )
			.Where( p => p.CanWrite )
			.ToArray( );

		_logger?.LogTrace( "Mapping properties: {SourceCount} source properties, {DestCount} destination properties", srcProperties.Length, destProperties.Length );

		var mappedCount = 0;
		var errorCount = 0;

		foreach ( var destProp in destProperties ) {
			var srcProp = srcProperties.FirstOrDefault( p => p.Name == destProp.Name );

			if ( srcProp == null ) {
				continue;
			}

			try {
				var srcValue = srcProp.GetValue( source );

				if ( srcValue == null ) {
					continue;
				}

				var mappedValue = MapPropertyValue( srcValue, srcProp.PropertyType, destProp.PropertyType );
				destProp.SetValue( dest, mappedValue );
				mappedCount++;

				_logger?.LogTrace( "Mapped property {PropertyName}: {SourceType} -> {DestType}", destProp.Name, srcProp.PropertyType.Name, destProp.PropertyType.Name );
			} catch ( Exception ex ) {
				errorCount++;
				_logger?.LogWarning( ex, "Error mapping property {PropertyName}", destProp.Name );
			}
		}

		_logger?.LogDebug( "Property mapping completed. Successful: {MappedCount}, Errors: {ErrorCount}", mappedCount, errorCount );
	}

	/// <summary>
	/// Maps a single property value from source type to destination type, handling type conversions and collections.
	/// </summary>
	/// <param name="value">The value to map.</param>
	/// <param name="sourceType">The source property type.</param>
	/// <param name="destType">The destination property type.</param>
	/// <returns>The mapped value.</returns>
	private object? MapPropertyValue( object value, Type sourceType, Type destType ) {
		// If types are equal or compatible, return directly
		if ( destType.IsAssignableFrom( sourceType ) ) {
			_logger?.LogTrace( "Direct assignment for compatible types {SourceType} -> {DestType}", sourceType.Name, destType.Name );
			return value;
		}

		// If it's a generic collection, map the elements
		if ( IsGenericCollection( sourceType ) && IsGenericCollection( destType ) ) {
			_logger?.LogTrace( "Mapping generic collection from {SourceType} to {DestType}", sourceType.Name, destType.Name );
			return MapGenericCollection( value, sourceType, destType );
		}

		// Try using the default mapping system
		try {
			var mapToMethod = typeof( MorphExtensions )
				.GetMethod( nameof( MorphExtensions.To ), [ typeof( object ), typeof( IServiceProvider ) ] )?
				.MakeGenericMethod( destType );

			var result = mapToMethod?.Invoke( null, [ value, _sp ] );
			_logger?.LogTrace( "Successfully mapped {SourceType} to {DestType} using MorphExtensions", sourceType.Name, destType.Name );
			return result;
		} catch ( Exception ex ) {
			_logger?.LogDebug( ex, "Failed to map {SourceType} to {DestType}, attempting fallback", sourceType.Name, destType.Name );
			// If it fails, return the original value if compatible
			return destType.IsAssignableFrom( value.GetType( ) ) ? value : null;
		}
	}

	/// <summary>
	/// Determines if a type represents a generic collection.
	/// </summary>
	/// <param name="type">The type to check.</param>
	/// <returns>True if the type is a generic collection, false otherwise.</returns>
	private static bool IsGenericCollection( Type type ) {
		return type.IsGenericType &&
			   ( type.GetGenericTypeDefinition( ) == typeof( IEnumerable<> ) ||
				type.GetInterfaces( ).Any( i => i.IsGenericType && i.GetGenericTypeDefinition( ) == typeof( IEnumerable<> ) ) );
	}

	/// <summary>
	/// Maps elements from a source generic collection to a destination generic collection.
	/// </summary>
	/// <param name="sourceCollection">The source collection to map from.</param>
	/// <param name="sourceType">The type of the source collection.</param>
	/// <param name="destType">The type of the destination collection.</param>
	/// <returns>The mapped destination collection.</returns>
	private object? MapGenericCollection( object sourceCollection, Type sourceType, Type destType ) {
		var sourceElementType = GetGenericArgumentType( sourceType );
		var destElementType = GetGenericArgumentType( destType );

		if ( sourceElementType == null || destElementType == null ) {
			_logger?.LogWarning( "Unable to determine element types for collection mapping from {SourceType} to {DestType}", sourceType.Name, destType.Name );
			return null;
		}

		_logger?.LogTrace( "Mapping collection elements from {SourceElementType} to {DestElementType}", sourceElementType.Name, destElementType.Name );

		var enumerable = ( System.Collections.IEnumerable )sourceCollection;
		var mappedItems = new List<object?>( );

		var itemCount = 0;
		var errorCount = 0;

		foreach ( var item in enumerable ) {
			itemCount++;

			if ( item == null ) {
				mappedItems.Add( null );
				continue;
			}

			try {
				var mappedItem = MapPropertyValue( item, sourceElementType, destElementType );
				mappedItems.Add( mappedItem );
			} catch ( Exception ex ) {
				errorCount++;
				_logger?.LogWarning( ex, "Error mapping collection item {ItemIndex}", itemCount - 1 );
				mappedItems.Add( null );
			}
		}

		_logger?.LogTrace( "Collection mapping completed. Items: {ItemCount}, Errors: {ErrorCount}", itemCount, errorCount );

		// Create the appropriate destination collection
		return CreateGenericCollection( destType, destElementType, mappedItems );
	}

	/// <summary>
	/// Extracts the generic argument type from a generic type.
	/// </summary>
	/// <param name="type">The generic type to extract from.</param>
	/// <returns>The first generic argument type, or null if not found.</returns>
	private Type? GetGenericArgumentType( Type type ) {
		if ( type.IsGenericType ) {
			var args = type.GetGenericArguments( );
			return args.Length > 0 ? args[ 0 ] : null;
		}

		// Search in implemented interfaces
		var genericInterface = type
			.GetInterfaces( )
			.FirstOrDefault( i =>
				i.IsGenericType &&
				i.GetGenericTypeDefinition( ) == typeof( IEnumerable<> ) );

		return genericInterface?.GetGenericArguments( ).FirstOrDefault( );
	}

	/// <summary>
	/// Creates a generic collection of the specified type with the provided items.
	/// </summary>
	/// <param name="collectionType">The type of collection to create.</param>
	/// <param name="elementType">The type of elements in the collection.</param>
	/// <param name="items">The items to populate the collection with.</param>
	/// <returns>The created collection instance.</returns>
	private object? CreateGenericCollection( Type collectionType, Type elementType, List<object?> items ) {
		try {
			// If it's an array
			if ( collectionType.IsArray ) {
				var array = Array.CreateInstance( elementType, items.Count );
				for ( int i = 0; i < items.Count; i++ )
					array.SetValue( items[ i ], i );
				return array;
			}

			// If it's a generic interface (IEnumerable<T>, ICollection<T>, etc.), create a List<T>
			if ( collectionType.IsInterface && collectionType.IsGenericType ) {
				var listType = typeof( List<> ).MakeGenericType( elementType );
				var list = ( IList )Activator.CreateInstance( listType )!;
				foreach ( var item in items )
					list.Add( item );
				return list;
			}

			// For concrete types, try to create direct instance
			var instance = CreateInstance( collectionType );
			if ( instance is IList concreteList ) {
				foreach ( var item in items )
					concreteList.Add( item );
				return instance;
			}
		} catch ( Exception ex ) {
			_logger?.LogError( ex, "Error creating collection of type {CollectionType}", collectionType.Name );
		}

		// Fallback: return a List<T>
		var fallbackListType = typeof( List<> ).MakeGenericType( elementType );
		var fallbackList = ( IList )Activator.CreateInstance( fallbackListType )!;
		foreach ( var item in items )
			fallbackList.Add( item );
		return fallbackList;
	}

	/// <summary>
	/// Performs instance-based mapping using the IMorphableTo interface.
	/// </summary>
	/// <typeparam name="TDestination">The destination type.</typeparam>
	/// <param name="source">The source object that implements IMorphableTo.</param>
	/// <param name="destinationType">The destination type to map to.</param>
	/// <returns>The mapped destination object.</returns>
	private TDestination MapFromMorphableTo<TDestination>( IMorphableTo<TDestination> source, Type destinationType ) {
		_logger?.LogDebug( "Starting IMorphableTo mapping for {SourceType} -> {DestinationType}", source.GetType( ).Name, destinationType.Name );

		// Resolve concrete type if necessary
		var concreteDestinationType = ResolveConcreteDestinationType( destinationType );

		// Create destination instance
		var dest = ( TDestination )CreateInstance( concreteDestinationType );

		// Create builder specific for instances
		var instanceBuilder = new Schema<TDestination>( );

		// Call the instance's MorphTo method to configure the builder
		source.MorphTo( instanceBuilder );

		// Apply the mapping
		var applyMethod = typeof( Schema<TDestination> )
			.GetMethod( "ApplyFromInstanceAsync", BindingFlags.Instance | BindingFlags.NonPublic );

		if ( applyMethod != null ) {
			var genericApplyMethod = applyMethod.MakeGenericMethod( source.GetType( ) );
			var task = ( Task )genericApplyMethod.Invoke( instanceBuilder, [ source, dest, _sp ] )!;
			task.GetAwaiter( ).GetResult( );
		}

		_logger?.LogDebug( "IMorphableTo mapping completed for {SourceType} -> {DestinationType}", source.GetType( ).Name, destinationType.Name );
		return dest;
	}

	/// <summary>
	/// Performs instance-based mapping using the IMorphableFrom interface.
	/// </summary>
	/// <typeparam name="TSource">The source type.</typeparam>
	/// <typeparam name="TDestination">The destination type.</typeparam>
	/// <param name="source">The source object.</param>
	/// <param name="sourceType">The actual runtime source type (may be a proxy).</param>
	/// <param name="destinationType">The destination type to map to.</param>
	/// <returns>The mapped destination object.</returns>
	private TDestination MapFromMorphableFrom<TSource, TDestination>( TSource source, Type sourceType, Type destinationType ) {
		var actualSourceType = _typeResolver.GetActualType( sourceType );
		_logger?.LogDebug( "Starting IMorphableFrom mapping for {SourceType} -> {DestinationType} (actual source: {ActualSource})",
			sourceType.Name, destinationType.Name, actualSourceType.Name );

		// Resolve concrete type if necessary
		var concreteDestinationType = ResolveConcreteDestinationType( destinationType );

		// Create destination instance
		var dest = ( TDestination )CreateInstance( concreteDestinationType );

		// Try direct cast first (fast path for non-proxy scenarios)
		if ( dest is IMorphableFrom<TSource> morphableFromDest ) {
			_logger?.LogDebug( "Using direct IMorphableFrom<{TSource}> cast (fast path)", typeof( TSource ).Name );

			// Create schema for source type
			var sourceSchema = new Schema<TSource>( );

			// Call the destination's MorphFrom method to configure the schema
			morphableFromDest.MorphFrom( sourceSchema );

			// Apply the mapping from source to destination using the configured schema
			sourceSchema.ApplyFromSourceToDestination( source, dest, _sp );
		} else {
			// Fallback for proxy scenarios - check if destination implements IMorphableFrom for the actual source type
			_logger?.LogTrace( "Direct cast failed, checking for IMorphableFrom with proxy support" );

			var morphableFromInterface = _typeResolver.FindGenericInterface( concreteDestinationType, typeof( IMorphableFrom<> ) );

			if ( morphableFromInterface != null ) {
				var sourceTypeArg = morphableFromInterface.GetGenericArguments( )[ 0 ];
				_logger?.LogTrace( "Found IMorphableFrom<{SourceTypeArg}> on destination type {DestType}",
					sourceTypeArg.Name, concreteDestinationType.Name );

				// Check if the actual source type is compatible with the interface's source type
				if ( sourceTypeArg.IsAssignableFrom( actualSourceType ) || sourceTypeArg == actualSourceType ) {
					_logger?.LogDebug( "Source type {ActualSource} is compatible with IMorphableFrom<{SourceTypeArg}>",
						actualSourceType.Name, sourceTypeArg.Name );

					// Use reflection to call MorphFrom and ApplyFromSourceToDestination with the correct types
					ApplyMorphableFromMapping( source, dest, sourceTypeArg, actualSourceType );
				} else {
					_logger?.LogWarning( "Source type {ActualSource} is not compatible with IMorphableFrom<{SourceTypeArg}>",
						actualSourceType.Name, sourceTypeArg.Name );
				}
			} else {
				_logger?.LogTrace( "Destination type {DestType} does not implement IMorphableFrom<>",
					concreteDestinationType.Name );
			}
		}

		_logger?.LogDebug( "IMorphableFrom mapping completed for {SourceType} -> {DestinationType}",
			sourceType.Name, destinationType.Name );
		return dest;
	}

	/// <summary>
	/// Applies the IMorphableFrom mapping using reflection to handle proxy types.
	/// </summary>
	private void ApplyMorphableFromMapping<TDestination>( object source, TDestination dest, Type interfaceSourceType, Type actualSourceType ) {
		_logger?.LogTrace( "Applying IMorphableFrom mapping with interface source type {InterfaceSource} and actual source {ActualSource}",
			interfaceSourceType.Name, actualSourceType.Name );

		try {
			// Create Schema<InterfaceSourceType>
			var schemaType = typeof( Schema<> ).MakeGenericType( interfaceSourceType );
			var schema = Activator.CreateInstance( schemaType )!;

			// Get the MorphFrom method and invoke it
			var morphFromMethod = dest!.GetType( ).GetMethod( nameof( IMorphableFrom<object>.MorphFrom ) );
			if ( morphFromMethod != null ) {
				_logger?.LogTrace( "Invoking MorphFrom method on destination" );
				morphFromMethod.Invoke( dest, [ schema ] );

				// Apply the mapping using ApplyFromSourceToDestination
				var applyMethod = schemaType.GetMethod( nameof( Schema<object>.ApplyFromSourceToDestination ) );
				if ( applyMethod != null ) {
					_logger?.LogTrace( "Invoking ApplyFromSourceToDestination" );
					applyMethod.Invoke( schema, [ source, dest, _sp ] );
				} else {
					_logger?.LogError( "ApplyFromSourceToDestination method not found on Schema<{Type}>", interfaceSourceType.Name );
				}
			} else {
				_logger?.LogError( "MorphFrom method not found on destination type {DestType}", dest.GetType( ).Name );
			}
		} catch ( Exception ex ) {
			_logger?.LogError( ex, "Error applying IMorphableFrom mapping" );
			throw;
		}
	}

	/// <summary>
	/// Maps from a source object to a destination type using the IMorphableFrom pattern asynchronously.
	/// This method supports async bindings and service provider access during mapping configuration.
	/// </summary>
	/// <typeparam name="TSource">The type of the source object.</typeparam>
	/// <typeparam name="TDestination">The type of the destination object.</typeparam>
	/// <param name="source">The source object to map from.</param>
	/// <param name="sourceType">The actual runtime source type (may be a proxy).</param>
	/// <param name="destinationType">The runtime type of the destination.</param>
	/// <returns>A task that returns the mapped destination instance.</returns>
	private async Task<TDestination> MapFromMorphableFromAsync<TSource, TDestination>( TSource source, Type sourceType, Type destinationType ) {
		var actualSourceType = _typeResolver.GetActualType( sourceType );
		_logger?.LogDebug( "Starting async IMorphableFrom mapping from {SourceType} to {DestinationType} (actual source: {ActualSource})",
			sourceType.Name, destinationType.Name, actualSourceType.Name );

		var concreteDestinationType = ResolveConcreteDestinationType( destinationType );

		// Create destination instance
		var dest = ( TDestination )CreateInstance( concreteDestinationType );

		// Try direct cast first (fast path for non-proxy scenarios)
		if ( dest is IMorphableFrom<TSource> morphableFromDest ) {
			_logger?.LogDebug( "Using direct IMorphableFrom<{TSource}> cast (fast path)", typeof( TSource ).Name );

			// Create schema for source type
			var sourceSchema = new Schema<TSource>( );

			// Call the destination's MorphFrom method to configure the schema
			morphableFromDest.MorphFrom( sourceSchema );

			// Apply the mapping from source to destination using the configured schema (including async mappings)
			await sourceSchema.ApplyFromSourceToDestinationAsync( source, dest, _sp );
		} else {
			// Fallback for proxy scenarios
			_logger?.LogTrace( "Direct cast failed, checking for IMorphableFrom with proxy support" );

			var morphableFromInterface = _typeResolver.FindGenericInterface( concreteDestinationType, typeof( IMorphableFrom<> ) );

			if ( morphableFromInterface != null ) {
				var sourceTypeArg = morphableFromInterface.GetGenericArguments( )[ 0 ];
				_logger?.LogTrace( "Found IMorphableFrom<{SourceTypeArg}> on destination type {DestType}",
					sourceTypeArg.Name, concreteDestinationType.Name );

				// Check if the actual source type is compatible with the interface's source type
				if ( sourceTypeArg.IsAssignableFrom( actualSourceType ) || sourceTypeArg == actualSourceType ) {
					_logger?.LogDebug( "Source type {ActualSource} is compatible with IMorphableFrom<{SourceTypeArg}>",
						actualSourceType.Name, sourceTypeArg.Name );

					// Use reflection to call MorphFrom and ApplyFromSourceToDestinationAsync with the correct types
					await ApplyMorphableFromMappingAsync( source, dest, sourceTypeArg, actualSourceType );
				} else {
					_logger?.LogWarning( "Source type {ActualSource} is not compatible with IMorphableFrom<{SourceTypeArg}>",
						actualSourceType.Name, sourceTypeArg.Name );
				}
			} else {
				_logger?.LogTrace( "Destination type {DestType} does not implement IMorphableFrom<>",
					concreteDestinationType.Name );
			}
		}

		_logger?.LogDebug( "Async IMorphableFrom mapping completed for {SourceType} -> {DestinationType}",
			sourceType.Name, destinationType.Name );
		return dest;
	}

	/// <summary>
	/// Applies the IMorphableFrom mapping asynchronously using reflection to handle proxy types.
	/// </summary>
	private async Task ApplyMorphableFromMappingAsync<TDestination>( object source, TDestination dest, Type interfaceSourceType, Type actualSourceType ) {
		_logger?.LogTrace( "Applying async IMorphableFrom mapping with interface source type {InterfaceSource} and actual source {ActualSource}",
			interfaceSourceType.Name, actualSourceType.Name );

		try {
			// Create Schema<InterfaceSourceType>
			var schemaType = typeof( Schema<> ).MakeGenericType( interfaceSourceType );
			var schema = Activator.CreateInstance( schemaType )!;

			// Get the MorphFrom method and invoke it
			var morphFromMethod = dest!.GetType( ).GetMethod( nameof( IMorphableFrom<object>.MorphFrom ) );
			if ( morphFromMethod != null ) {
				_logger?.LogTrace( "Invoking MorphFrom method on destination" );
				morphFromMethod.Invoke( dest, [ schema ] );

				// Apply the mapping using ApplyFromSourceToDestinationAsync
				var applyMethod = schemaType.GetMethod( nameof( Schema<object>.ApplyFromSourceToDestinationAsync ) );
				if ( applyMethod != null ) {
					_logger?.LogTrace( "Invoking ApplyFromSourceToDestinationAsync" );
					var task = ( Task? )applyMethod.Invoke( schema, [ source, dest, _sp ] );
					if ( task != null ) {
						await task;
					}
				} else {
					_logger?.LogError( "ApplyFromSourceToDestinationAsync method not found on Schema<{Type}>", interfaceSourceType.Name );
				}
			} else {
				_logger?.LogError( "MorphFrom method not found on destination type {DestType}", dest.GetType( ).Name );
			}
		} catch ( Exception ex ) {
			_logger?.LogError( ex, "Error applying async IMorphableFrom mapping" );
			throw;
		}
	}

	/// <summary>
	/// Resolves the concrete implementation type for a given destination type.
	/// If the destination is an interface, attempts to find a concrete implementation.
	/// </summary>
	/// <param name="destinationType">The destination type to resolve.</param>
	/// <returns>The concrete type to use for instantiation.</returns>
	private Type ResolveConcreteDestinationType( Type destinationType ) {
		// If it's a generic interface, try to resolve to concrete
		if ( destinationType.IsInterface &&
			 TryResolveGenericConcrete( destinationType, out var concrete ) ) {
			_logger?.LogTrace( "Resolved generic interface {Interface} to concrete type {Concrete}", destinationType.Name, concrete.Name );
			return concrete;
		}

		// If it's a non-generic interface, look for registered implementation in DI
		if ( destinationType.IsInterface ) {
			var serviceProvider = MythServiceProvider.GetOrFallback( _sp );
			var serviceImpl = serviceProvider?.GetService( destinationType );
			if ( serviceImpl != null ) {
				_logger?.LogTrace( "Resolved interface {Interface} to service implementation {Implementation}", destinationType.Name, serviceImpl.GetType( ).Name );
				return serviceImpl.GetType( );
			}
		}

		return destinationType;
	}

	/// <summary>
	/// Attempts to resolve a generic interface to its concrete implementation.
	/// </summary>
	/// <param name="iface">The generic interface type.</param>
	/// <param name="concrete">The resolved concrete type, if successful.</param>
	/// <returns>True if resolution was successful, false otherwise.</returns>
	public bool TryResolveGenericConcrete( Type iface, out Type concrete ) {
		if ( iface.IsGenericType ) {
			var genericDef = iface.GetGenericTypeDefinition( );

			if ( _genericInterfaceToConcrete.TryGetValue( genericDef, out var concreteDef ) ) {
				var args = iface.GetGenericArguments( );
				concrete = concreteDef.MakeGenericType( args );
				return true;
			}
		}
		concrete = null!;
		return false;
	}

	/// <summary>
	/// Attempts to find an existing builder for the specified source and destination types.
	/// Includes inheritance fallback support when enabled in settings.
	/// </summary>
	/// <param name="sourceType">The source type.</param>
	/// <param name="destinationType">The destination type.</param>
	/// <param name="builderObj">The found builder object, if successful.</param>
	/// <returns>True if a builder was found, false otherwise.</returns>
	private bool TryGetBuilder( Type sourceType, Type destinationType, out object? builderObj ) {
		// Look for exact mapping
		if ( _builders.TryGetValue( (sourceType, destinationType), out builderObj ) ) {
			_logger?.LogTrace( "Found exact builder for {SourceType} -> {DestinationType}", sourceType.Name, destinationType.Name );
			return true;
		}

		// Try inheritance fallback if enabled
		if ( _settings.EnableInheritanceFallback ) {
			if ( TryGetBuilderWithInheritanceFallback( sourceType, destinationType, out builderObj ) ) {
				return true;
			}

			// Look for compatible mappings (inheritance/implementation) - original behavior but only if inheritance is enabled
			foreach ( var ((src, dst), builder) in _builders ) {
				if ( src.IsAssignableFrom( sourceType ) && dst.IsAssignableFrom( destinationType ) ) {
					builderObj = builder;
					_logger?.LogTrace( "Found compatible builder for {SourceType} -> {DestinationType} (via {BuilderSource} -> {BuilderDest})",
						sourceType.Name, destinationType.Name, src.Name, dst.Name );
					return true;
				}
			}
		}

		builderObj = null;
		return false;
	}

	/// <summary>
	/// Checks if an instance-based mapping exists for the specified types, including inheritance fallback support.
	/// </summary>
	/// <param name="sourceType">The source type to check.</param>
	/// <param name="destinationType">The destination type to check.</param>
	/// <returns>True if an instance-based mapping exists (directly or through inheritance), false otherwise.</returns>
	private bool TryGetInstanceBasedMapping( Type sourceType, Type destinationType ) {
		var actualSourceType = _typeResolver.GetActualType( sourceType );
		var actualDestType = _typeResolver.GetActualType( destinationType );

		_logger?.LogTrace( "Checking instance-based mapping for {SourceType} -> {DestinationType} (actual: {ActualSource} -> {ActualDest})",
			sourceType.Name, destinationType.Name, actualSourceType.Name, actualDestType.Name );

		// Check direct mapping first
		if ( _instanceBasedMappings.Contains( (actualSourceType, actualDestType) ) ) {
			_logger?.LogDebug( "Found direct instance-based mapping for {SourceType} -> {DestinationType}",
				actualSourceType.Name, actualDestType.Name );
			return true;
		}

		// If inheritance fallback is enabled, check the inheritance hierarchy
		if ( _settings.EnableInheritanceFallback ) {
			_logger?.LogTrace( "Checking instance-based mapping with inheritance fallback for {SourceType}", actualSourceType.Name );
			var hierarchy = _typeResolver.GetInheritanceHierarchy( actualSourceType );

			foreach ( var baseType in hierarchy ) {
				if ( _instanceBasedMappings.Contains( (baseType, actualDestType) ) ) {
					_logger?.LogDebug( "Found inheritance instance-based mapping for {SourceType} -> {DestinationType} via base type {BaseType}",
						actualSourceType.Name, actualDestType.Name, baseType.Name );
					return true;
				}
			}

			_logger?.LogTrace( "No instance-based mapping found in hierarchy for {SourceType} -> {DestinationType}",
				actualSourceType.Name, actualDestType.Name );
		}

		return false;
	}

	/// <summary>
	/// Checks if the destination type implements IMorphableFrom for the given source type.
	/// </summary>
	/// <param name="sourceType">The source type.</param>
	/// <param name="destinationType">The destination type.</param>
	/// <returns>True if the destination type can be created from the source type using IMorphableFrom.</returns>
	private bool TryGetMorphableFromMapping( Type sourceType, Type destinationType ) {
		// Resolve actual types (handle proxies)
		var actualSourceType = _typeResolver.GetActualType( sourceType );
		var actualDestType = _typeResolver.GetActualType( destinationType );

		_logger?.LogTrace( "Checking IMorphableFrom mapping for {SourceType} -> {DestinationType} (actual: {ActualSource} -> {ActualDest})",
			sourceType.Name, destinationType.Name, actualSourceType.Name, actualDestType.Name );

		// Check if destination type implements IMorphableFrom<actualSourceType>
		var morphableFromInterface = actualDestType
			.GetInterfaces( )
			.FirstOrDefault( i => i.IsGenericType &&
							   i.GetGenericTypeDefinition( ) == typeof( IMorphableFrom<> ) &&
							   _typeResolver.GetActualType( i.GetGenericArguments( )[ 0 ] ) == actualSourceType );

		if ( morphableFromInterface != null ) {
			_logger?.LogDebug( "Found direct IMorphableFrom mapping for {SourceType} -> {DestinationType}",
				actualSourceType.Name, actualDestType.Name );
			return true;
		}

		// If inheritance fallback is enabled, check the inheritance hierarchy of the source type
		if ( _settings.EnableInheritanceFallback ) {
			_logger?.LogTrace( "Checking IMorphableFrom with inheritance fallback for {SourceType}", actualSourceType.Name );
			var sourceHierarchy = _typeResolver.GetInheritanceHierarchy( actualSourceType );

			_logger?.LogTrace( "Source type {SourceType} has {HierarchyCount} types in hierarchy",
				actualSourceType.Name, sourceHierarchy.Count );

			foreach ( var baseSourceType in sourceHierarchy ) {
				var hierarchyInterface = actualDestType
					.GetInterfaces( )
					.FirstOrDefault( i => i.IsGenericType &&
									   i.GetGenericTypeDefinition( ) == typeof( IMorphableFrom<> ) &&
									   _typeResolver.GetActualType( i.GetGenericArguments( )[ 0 ] ) == baseSourceType );

				if ( hierarchyInterface != null ) {
					_logger?.LogDebug( "Found IMorphableFrom inheritance mapping for {SourceType} -> {DestinationType} via base type {BaseSourceType}",
						actualSourceType.Name, actualDestType.Name, baseSourceType.Name );
					return true;
				}
			}

			_logger?.LogTrace( "No IMorphableFrom mapping found in hierarchy for {SourceType} -> {DestinationType}",
				actualSourceType.Name, actualDestType.Name );
		}

		return false;
	}

	/// <summary>
	/// Attempts to find a builder using inheritance fallback by searching through the type hierarchy.
	/// </summary>
	/// <param name="sourceType">The source type to search for.</param>
	/// <param name="destinationType">The destination type.</param>
	/// <param name="builderObj">The found builder object, if successful.</param>
	/// <returns>True if a builder was found through inheritance fallback, false otherwise.</returns>
	private bool TryGetBuilderWithInheritanceFallback( Type sourceType, Type destinationType, out object? builderObj ) {
		builderObj = null;

		var actualSourceType = _typeResolver.GetActualType( sourceType );
		var actualDestType = _typeResolver.GetActualType( destinationType );

		// Check cache first
		var cacheKey = (actualSourceType, actualDestType);
		if ( _mappingFallbackCache.TryGetValue( cacheKey, out var cachedMapping ) ) {
			if ( cachedMapping.HasValue ) {
				_builders.TryGetValue( cachedMapping.Value, out builderObj );
				_logger?.LogTrace( "Found cached inheritance mapping for {SourceType} -> {DestinationType} via {CachedSource} -> {CachedDest}",
					actualSourceType.Name, actualDestType.Name, cachedMapping.Value.Item1.Name, cachedMapping.Value.Item2.Name );
				return builderObj != null;
			}
			_logger?.LogTrace( "Cache indicates no mapping exists for {SourceType} -> {DestinationType}",
				actualSourceType.Name, actualDestType.Name );
			return false;
		}

		_logger?.LogTrace( "Searching inheritance hierarchy for builder: {SourceType} -> {DestinationType}",
			actualSourceType.Name, actualDestType.Name );

		// Get inheritance hierarchy for source type using TypeResolver
		var hierarchy = _typeResolver.GetInheritanceHierarchy( actualSourceType );

		// Search through hierarchy for compatible mappings
		foreach ( var baseType in hierarchy ) {
			if ( _builders.TryGetValue( (baseType, actualDestType), out builderObj ) ) {
				_logger?.LogDebug( "Found inheritance fallback builder for {SourceType} -> {DestinationType} via base type {BaseType}",
					actualSourceType.Name, actualDestType.Name, baseType.Name );

				// Cache this successful mapping
				_mappingFallbackCache[ cacheKey ] = (baseType, actualDestType);
				return true;
			}
		}

		_logger?.LogTrace( "No inheritance fallback builder found for {SourceType} -> {DestinationType}",
			actualSourceType.Name, actualDestType.Name );

		// Cache as not found
		_mappingFallbackCache[ cacheKey ] = null;
		return false;
	}

	/// <summary>
	/// Applies mapping dynamically when types don't match exactly, using reflection.
	/// </summary>
	/// <typeparam name="TDestination">The expected destination type.</typeparam>
	/// <param name="source">The source object.</param>
	/// <param name="actualSourceType">The actual source type.</param>
	/// <param name="dest">The destination object.</param>
	/// <param name="concreteDestType">The concrete destination type.</param>
	/// <param name="destinationType">The interface destination type.</param>
	/// <returns>The mapped destination object.</returns>
	private TDestination ApplyMappingDynamically<TDestination>(
		object source,
		Type actualSourceType,
		object dest,
		Type concreteDestType,
		Type destinationType ) {
		_logger?.LogDebug( "Applying dynamic mapping from {ActualSourceType} to {ConcreteDestType}", actualSourceType.Name, concreteDestType.Name );

		// Find or create a compatible builder
		var compatibleBuilder = GetOrCreateCompatibleBuilder( actualSourceType, concreteDestType );

		if ( compatibleBuilder != null ) {
			// Invoke ApplyAsync dynamically
			var applyMethod = compatibleBuilder.GetType( ).GetMethod( "ApplyAsync" );
			if ( applyMethod != null ) {
				var applyTask = ( Task )applyMethod.Invoke( compatibleBuilder, [ source, dest, _sp ] )!;
				applyTask.GetAwaiter( ).GetResult( );
			}
		}

		_logger?.LogDebug( "Dynamic mapping completed" );
		return ( TDestination )dest;
	}

	/// <summary>
	/// Gets an existing compatible builder or creates a new one dynamically.
	/// </summary>
	/// <param name="sourceType">The source type.</param>
	/// <param name="destType">The destination type.</param>
	/// <returns>A compatible builder object, or null if none can be created.</returns>
	private object? GetOrCreateCompatibleBuilder( Type sourceType, Type destType ) {
		// First check if one already exists
		if ( _builders.TryGetValue( (sourceType, destType), out var existing ) ) {
			return existing;
		}

		// Try to create a dynamic mapping if types are compatible
		if ( CanCreateDynamicMapping( sourceType, destType ) ) {
			_logger?.LogTrace( "Creating dynamic mapping for {SourceType} -> {DestType}", sourceType.Name, destType.Name );
			CreateDynamicMapping( sourceType, destType );
			return _builders.GetValueOrDefault( (sourceType, destType) );
		}

		return null;
	}

	/// <summary>
	/// Determines if a dynamic mapping can be created between the specified types.
	/// </summary>
	/// <param name="sourceType">The source type.</param>
	/// <param name="destType">The destination type.</param>
	/// <returns>True if dynamic mapping is possible, false otherwise.</returns>
	private bool CanCreateDynamicMapping( Type sourceType, Type destType ) {
		// Can create automatic mapping if:
		// 1. Both are classes or structs (not interfaces)
		// 2. Or if we can resolve the concrete destination type
		if ( !sourceType.IsInterface && !destType.IsInterface )
			return true;

		if ( destType.IsInterface && TryResolveGenericConcrete( destType, out _ ) )
			return true;

		return false;
	}

	/// <summary>
	/// Creates a dynamic mapping between the specified types.
	/// </summary>
	/// <param name="sourceType">The source type.</param>
	/// <param name="destType">The destination type.</param>
	private void CreateDynamicMapping( Type sourceType, Type destType ) {
		// Resolve concrete type if necessary
		var concreteDestType = ResolveConcreteDestinationType( destType );

		// Create a generic builder dynamically
		var builderType = typeof( Schema<> ).MakeGenericType( sourceType, concreteDestType );
		var builder = Activator.CreateInstance( builderType )!;

		// Add basic automatic mapping (will be done by AutoMap)
		_builders[ (sourceType, destType) ] = builder;

		// If the concrete type is different from the interface type, register both
		if ( concreteDestType != destType ) {
			_builders[ (sourceType, concreteDestType) ] = builder;
		}

		_logger?.LogTrace( "Created dynamic builder for {SourceType} -> {DestType}", sourceType.Name, destType.Name );
	}

	/// <summary>
	/// Checks if a mapping exists between the specified source and destination types.
	/// Includes inheritance fallback support when enabled in settings.
	/// </summary>
	/// <param name="sourceType">The source type to check.</param>
	/// <param name="destinationType">The destination type to check.</param>
	/// <returns>True if a mapping exists, false otherwise.</returns>
	public bool HasMapping( Type sourceType, Type destinationType ) {
		// Check if it's an instance-based mapping with inheritance fallback support
		if ( TryGetInstanceBasedMapping( sourceType, destinationType ) ) {
			_logger?.LogTrace( "Found instance-based mapping for {SourceType} -> {DestType}", sourceType.Name, destinationType.Name );
			return true;
		}

		// Check direct mapping
		if ( _builders.ContainsKey( (sourceType, destinationType) ) ) {
			_logger?.LogTrace( "Found direct mapping for {SourceType} -> {DestType}", sourceType.Name, destinationType.Name );
			return true;
		}

		// Check inheritance fallback if enabled
		if ( _settings.EnableInheritanceFallback ) {
			if ( TryGetBuilderWithInheritanceFallback( sourceType, destinationType, out _ ) ) {
				_logger?.LogTrace( "Found inheritance fallback mapping for {SourceType} -> {DestType}", sourceType.Name, destinationType.Name );
				return true;
			}
		}

		// Check if it's a generic mapping
		if ( IsGenericMapping( sourceType, destinationType ) ) {
			_logger?.LogTrace( "Found generic mapping for {SourceType} -> {DestType}", sourceType.Name, destinationType.Name );
			return true;
		}

		// Check if we can resolve the concrete type and have mapping for it
		var concreteDestType = ResolveConcreteDestinationType( destinationType );
		if ( concreteDestType != destinationType && _builders.ContainsKey( (sourceType, concreteDestType) ) ) {
			_logger?.LogTrace( "Found concrete type mapping for {SourceType} -> {ConcreteDestType}", sourceType.Name, concreteDestType.Name );
			return true;
		}

		// Check concrete destination type with inheritance fallback
		if ( _settings.EnableInheritanceFallback && concreteDestType != destinationType ) {
			if ( TryGetBuilderWithInheritanceFallback( sourceType, concreteDestType, out _ ) ) {
				_logger?.LogTrace( "Found inheritance fallback mapping for {SourceType} -> {ConcreteDestType}", sourceType.Name, concreteDestType.Name );
				return true;
			}
		}

		_logger?.LogTrace( "No mapping found for {SourceType} -> {DestType}", sourceType.Name, destinationType.Name );
		return false;
	}

	/// <summary>
	/// Registers automatic mappings for generic types with identical definitions.
	/// This enables automatic mapping between generic types like List&lt;SourceItem&gt; to List&lt;DestItem&gt;.
	/// </summary>
	/// <param name="memberMatchRule">Optional rule for matching members between types.</param>
	public void RegisterGenericEqualTypesMapping( Func<string, string, bool>? memberMatchRule = null ) {
		_logger?.LogDebug( "Registering generic equal types mapping" );

		_genericRegisters.Add( ( sourceType, destType ) => {
			// Resolve concrete types if necessary
			var concreteDestType = ResolveConcreteDestinationType( destType );

			// If mapping already exists, don't register again
			if ( _builders.ContainsKey( (sourceType, destType) ) ||
				_builders.ContainsKey( (sourceType, concreteDestType) ) )
				return;

			// Check if they are compatible generic types
			if ( !IsCompatibleGenericMapping( sourceType, concreteDestType ) )
				return;

			_logger?.LogTrace( "Creating generic equal types mapping for {SourceType} -> {ConcreteDestType}", sourceType.Name, concreteDestType.Name );

			// Create builder dynamically via reflection
			var builderType = typeof( Schema<> ).MakeGenericType( sourceType, concreteDestType );
			var builder = Activator.CreateInstance( builderType )!;

			// Register for both types (interface and concrete)
			_builders[ (sourceType, destType) ] = builder;
			if ( destType != concreteDestType ) {
				_builders[ (sourceType, concreteDestType) ] = builder;
			}
		} );
	}

	/// <summary>
	/// Determines if two generic types are compatible for automatic mapping.
	/// </summary>
	/// <param name="sourceType">The source generic type.</param>
	/// <param name="destType">The destination generic type.</param>
	/// <returns>True if the types are compatible, false otherwise.</returns>
	private bool IsCompatibleGenericMapping( Type sourceType, Type destType ) {
		// Both must be generic
		if ( !sourceType.IsGenericType || !destType.IsGenericType )
			return false;

		var srcGenericDef = sourceType.GetGenericTypeDefinition( );
		var dstGenericDef = destType.GetGenericTypeDefinition( );

		// Must have the same generic definition
		if ( srcGenericDef != dstGenericDef )
			return false;

		var srcArgs = sourceType.GetGenericArguments( );
		var dstArgs = destType.GetGenericArguments( );

		// Must have the same number of generic arguments
		var isCompatible = srcArgs.Length == dstArgs.Length;

		_logger?.LogTrace( "Generic compatibility check for {SourceGeneric} vs {DestGeneric}: {IsCompatible}", srcGenericDef.Name, dstGenericDef.Name, isCompatible );
		return isCompatible;
	}

	/// <summary>
	/// Creates an instance of the specified type, handling dependency injection and fallback scenarios.
	/// </summary>
	/// <param name="type">The type to create an instance of.</param>
	/// <returns>An instance of the specified type.</returns>
	/// <exception cref="BindException">Thrown when the type cannot be instantiated.</exception>
	private object CreateInstance( Type type ) {
		_logger?.LogTrace( "Creating instance of type {TypeName}", type.Name );

		// 1 - Try via DI first
		var serviceProvider = MythServiceProvider.GetOrFallback( _sp );
		var fromSp = serviceProvider?.GetService( type );
		if ( fromSp != null ) {
			_logger?.LogTrace( "Created instance via service provider for {TypeName}", type.Name );
			return fromSp;
		}

		// 2 - Try parameterless constructor
		var ctor = type.GetConstructor( Type.EmptyTypes );
		if ( ctor != null ) {
			var instance = ctor.Invoke( null );
			_logger?.LogTrace( "Created instance via parameterless constructor for {TypeName}", type.Name );
			return instance;
		}

		// 3 - Get constructor with fewest parameters and try to resolve via DI
		var ctorWithParams = type
			.GetConstructors( )
			.OrderBy( c => c.GetParameters( ).Length )
			.FirstOrDefault( );

		if ( ctorWithParams is null ) {
			var errorMessage = $"Type {type} has no accessible constructor.";
			_logger?.LogError( errorMessage );
			throw new BindException( errorMessage );
		}

		var parameters = ctorWithParams.GetParameters( );
		_logger?.LogTrace( "Attempting to create instance with {ParameterCount} constructor parameters", parameters.Length );

		var args = parameters
			.Select( p => {
				// Try to resolve via DI first
				var serviceValue = serviceProvider?.GetService( p.ParameterType );
				if ( serviceValue != null ) {
					_logger?.LogTrace( "Resolved parameter {ParameterName} of type {ParameterType} via DI", p.Name, p.ParameterType.Name );
					return serviceValue;
				}

				// Otherwise use default value
				var defaultValue = p.HasDefaultValue ? p.DefaultValue : GetDefault( p.ParameterType );
				_logger?.LogTrace( "Using default value for parameter {ParameterName} of type {ParameterType}", p.Name, p.ParameterType.Name );
				return defaultValue;
			} )
			.ToArray( );

		var result = ctorWithParams.Invoke( args );
		_logger?.LogTrace( "Successfully created instance of {TypeName} with constructor parameters", type.Name );
		return result;
	}

	/// <summary>
	/// Gets the default value for a given type.
	/// </summary>
	/// <param name="type">The type to get the default value for.</param>
	/// <returns>The default value for the type.</returns>
	private static object? GetDefault( Type type ) =>
		type.IsValueType
		? Activator.CreateInstance( type )
		: null;
}
