using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Interfaces;
using Myth.Morph;
using Myth.Settings;

namespace Myth.Extensions;

/// <summary>
/// Provides extension methods for configuring the Myth Morph mapping services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions {

	/// <summary>
	/// Adds Myth Morph mapping services to the specified <see cref="IServiceCollection"/>.
	/// </summary>
	/// <remarks>
	/// This method registers the necessary services for Morph, including generic type mappings,
	/// instance-based mapping profiles, and automatic mappings for generic types with identical definitions. If no
	/// assemblies are specified in <paramref name="settings"/>, the method uses all assemblies loaded in the current
	/// application domain. Automatically initializes the global service provider using the centralized Myth system.
	/// </remarks>
	/// <param name="services">The <see cref="IServiceCollection"/> to which the Morph services will be added.</param>
	/// <param name="settings">An optional delegate to configure <see cref="MorphSettings"/>. If not provided, default settings are used.</param>
	/// <returns>The same <see cref="IServiceCollection"/> instance, allowing for method chaining.</returns>
	public static IServiceCollection AddMorph( this IServiceCollection services, Action<MorphSettings>? settings = null ) {
		var morphSettings = new MorphSettings( );
		settings?.Invoke( morphSettings );

		var assemblies = morphSettings.Assemblies;
		if ( assemblies == null || assemblies.Count == 0 )
			assemblies = AppDomain.CurrentDomain.GetAssemblies( ).ToList( );

		services.AddSingleton( morphSettings );

		services.AddSingleton( sp => {
			var logger = sp.GetService<ILogger<SchemaRegistry>>( );
			logger?.LogInformation( "Initializing SchemaRegistry with {AssemblyCount} assemblies", assemblies.Count );

			var registry = new SchemaRegistry( sp, morphSettings );

			// Register manually defined generic mappings
			foreach ( var (iface, concrete) in morphSettings.GenericMappings ) {
				logger?.LogDebug( "Registering generic mapping: {Interface} -> {Concrete}", iface.Name, concrete.Name );
				registry.RegisterGenericMapping( iface, concrete );
			}

			// Register profiles based on IMorphableTo<TDestination> and IMorphableFrom<TSource> interfaces
			RegisterInstanceBasedMorphProfiles( registry, assemblies, logger );

			// Register automatic mapping for equal generic types
			registry.RegisterGenericEqualTypesMapping( );
			logger?.LogDebug( "Registered automatic mapping for equal generic types" );

			logger?.LogInformation( "SchemaRegistry initialization completed successfully" );
			return registry;
		} );

		return services;
	}

	/// <summary>
	/// Registers instance-based profiles from the specified assemblies into the provided bind registry.
	/// </summary>
	/// <remarks>
	/// This method iterates through all non-abstract, non-interface types in the provided assemblies and
	/// attempts to register instance-based profiles for each type into the specified registry. If an assembly cannot be
	/// fully loaded, only the successfully loaded types are processed. Any errors encountered during the registration of
	/// individual profiles are logged for debugging purposes.
	/// </remarks>
	/// <param name="registry">The <see cref="SchemaRegistry"/> where the profiles will be registered.</param>
	/// <param name="assemblies">A list of assemblies to scan for types containing instance-based profiles.</param>
	/// <param name="logger">The logger instance for recording registration activities.</param>
	private static void RegisterInstanceBasedMorphProfiles( SchemaRegistry registry, List<Assembly> assemblies, ILogger? logger ) {
		logger?.LogDebug( "Starting instance-based profile registration across {AssemblyCount} assemblies", assemblies.Count );

		// Filter out known problematic system assemblies before processing
		var filteredAssemblies = assemblies
			.Where( assembly => !IsSystemAssembly( assembly ) )
			.ToList( );

		if ( filteredAssemblies.Count < assemblies.Count ) {
			logger?.LogDebug( "Filtered out {FilteredCount} system assemblies, processing {RemainingCount} assemblies",
				assemblies.Count - filteredAssemblies.Count, filteredAssemblies.Count );
		}

		var allTypes = filteredAssemblies
			.SelectMany( assembly => GetTypesFromAssembly( assembly, logger ) )
			.Where( x =>
				x != null &&
				!x.IsAbstract &&
				!x.IsInterface );

		var typeArray = allTypes.ToArray( );
		logger?.LogDebug( "Found {TypeCount} eligible types for profile registration", typeArray.Length );

		var successCount = 0;
		var errorCount = 0;

		foreach ( var type in typeArray ) {
			try {
				RegisterMorphProfiles( type, registry, logger );
				successCount++;
			} catch ( Exception ex ) {
				errorCount++;
				logger?.LogError( ex, "Error registering profiles for type {TypeName}", type.Name );
			}
		}

		logger?.LogInformation( "Instance-based profile registration completed. Success: {SuccessCount}, Errors: {ErrorCount}", successCount, errorCount );
	}

	/// <summary>
	/// Safely loads types from an assembly, handling ReflectionTypeLoadException
	/// </summary>
	/// <param name="assembly">The assembly to load types from</param>
	/// <param name="logger">The logger instance for recording loading activities</param>
	/// <returns>Collection of successfully loaded types</returns>
	private static IEnumerable<Type> GetTypesFromAssembly( Assembly assembly, ILogger? logger ) {
		try {
			var types = assembly.GetTypes( );
			logger?.LogTrace( "Successfully loaded {TypeCount} types from assembly {AssemblyName}", types.Length, assembly.GetName( ).Name );
			return types;
		} catch ( ReflectionTypeLoadException ex ) {
			// Return only the types that were successfully loaded
			var loadedTypes = ex.Types.Where( t => t != null ).Cast<Type>( ).ToArray( );
			logger?.LogDebug( "Partial type loading from assembly {AssemblyName}. Loaded {LoadedCount} out of {TotalCount} types",
				assembly.GetName( ).Name, loadedTypes.Length, ex.Types.Length );
			return loadedTypes;
		} catch ( Exception ex ) {
			logger?.LogWarning( ex, "Failed to load types from assembly {AssemblyName}", assembly.GetName( ).Name );
			return [ ];
		}
	}

	/// <summary>
	/// Determines if an assembly is a system/framework assembly that should be excluded
	/// </summary>
	/// <param name="assembly">The assembly to check</param>
	/// <returns>True if the assembly is a system assembly, false otherwise</returns>
	private static bool IsSystemAssembly( Assembly assembly ) {
		var name = assembly.GetName( ).Name ?? string.Empty;

		// Exclude known problematic assemblies
		var excludedPrefixes = new[ ] {
			"Microsoft.Build",
			"Microsoft.CodeAnalysis",
			"Microsoft.VisualStudio",
			"NuGet.",
			"Newtonsoft.Json.Schema",
			"System.",
			"mscorlib",
			"netstandard"
		};

		return excludedPrefixes.Any( prefix => name.StartsWith( prefix, StringComparison.OrdinalIgnoreCase ) );
	}

	/// <summary>
	/// Registers instance-based mapping profiles for a given type.
	/// </summary>
	/// <remarks>
	/// This method identifies interfaces implemented by the type that match the
	/// generic definition of <see cref="IMorphableTo{T}"/> or <see cref="IMorphableFrom{T}"/>.
	/// For each matching interface, it registers appropriate mappings in the registry.
	/// </remarks>
	/// <param name="type">The type to analyze for instance-based mapping profiles.</param>
	/// <param name="registry">The registry where the mappings will be registered.</param>
	/// <param name="logger">The logger instance for recording registration activities.</param>
	private static void RegisterMorphProfiles( Type type, SchemaRegistry registry, ILogger? logger ) {
		var interfacesFound = 0;
		var mappingsRegistered = 0;

		foreach ( var iface in type.GetInterfaces( ) ) {
			if ( !iface.IsGenericType )
				continue;

			var genericDef = iface.GetGenericTypeDefinition( );

			// Check for IMorphableTo<TDestination> - source type defines how to transform to destination
			if ( genericDef == typeof( IMorphableTo<> ) ) {
				interfacesFound++;
				var destinationType = iface.GenericTypeArguments[ 0 ];

				try {
					// Register a mapping for types that implement IMorphableTo<TDestination>
					registry.RegisterInstanceBasedMapping( type, destinationType );
					mappingsRegistered++;

					logger?.LogDebug( "IMorphableTo profile registered: {SourceType} -> {DestinationType}", type.Name, destinationType.Name );
				} catch ( Exception ex ) {
					logger?.LogError( ex, "Error registering IMorphableTo profile for {SourceType} -> {DestinationType}", type.Name, destinationType.Name );
				}
			}
			// IMorphableFrom<TSource> mappings are handled dynamically when needed
			// No need to register them upfront as they're discovered at runtime
			else if ( genericDef == typeof( IMorphableFrom<> ) ) {
				interfacesFound++;
				var sourceType = iface.GenericTypeArguments[ 0 ];
				logger?.LogTrace( "Found IMorphableFrom interface: {DestinationType} can be created from {SourceType}", type.Name, sourceType.Name );
			}
		}

		if ( interfacesFound > 0 ) {
			logger?.LogTrace( "Processed {InterfaceCount} morph interfaces for type {TypeName}, registered {MappingCount} mappings",
				interfacesFound, type.Name, mappingsRegistered );
		}
	}
}
