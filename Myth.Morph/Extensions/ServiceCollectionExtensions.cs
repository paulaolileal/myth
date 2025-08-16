using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Interfaces;
using Myth.Morph;
using Myth.Settings;
using System.Reflection;

namespace Myth.Extensions {

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
		/// application domain.
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

			services.AddSingleton( sp => {
				var logger = sp.GetService<ILogger<SchemaRegistry>>( );
				logger?.LogInformation( "Initializing SchemaRegistry with {AssemblyCount} assemblies", assemblies.Count );

				var registry = new SchemaRegistry( sp );

				// Register manually defined generic mappings
				foreach ( var (iface, concrete) in morphSettings.GenericMappings ) {
					logger?.LogDebug( "Registering generic mapping: {Interface} -> {Concrete}", iface.Name, concrete.Name );
					registry.RegisterGenericMapping( iface, concrete );
				}

				// Register profiles based on IMorphable<TDestination> interface
				RegisterInstanceBasedMapToProfiles( registry, assemblies, logger );

				// Register automatic mapping for equal generic types
				registry.RegisterGenericEqualTypesMapping( );
				logger?.LogDebug( "Registered automatic mapping for equal generic types" );

				logger?.LogInformation( "SchemaRegistry initialization completed successfully" );
				return registry;
			} );

			DefaultProvider.EnsureProvider( services.BuildServiceProvider( ) );

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
		private static void RegisterInstanceBasedMapToProfiles( SchemaRegistry registry, List<Assembly> assemblies, ILogger? logger ) {
			logger?.LogDebug( "Starting instance-based profile registration across {AssemblyCount} assemblies", assemblies.Count );

			var allTypes = assemblies
				.SelectMany( assembly => {
					try {
						var types = assembly.GetTypes( );
						logger?.LogTrace( "Successfully loaded {TypeCount} types from assembly {AssemblyName}", types.Length, assembly.GetName( ).Name );
						return types;
					} catch ( ReflectionTypeLoadException ex ) {
						var loadedTypes = ex.Types.Where( t => t != null ).ToArray( )!;
						logger?.LogWarning( ex, "Partial type loading from assembly {AssemblyName}. Loaded {LoadedCount} out of {TotalCount} types",
							assembly.GetName( ).Name, loadedTypes.Length, ex.Types.Length );
						return loadedTypes;
					} catch ( Exception ex ) {
						logger?.LogError( ex, "Failed to load types from assembly {AssemblyName}", assembly.GetName( ).Name );
						return [ ];
					}
				} )
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
					RegisterInstanceBasedProfiles( type, registry, logger );
					successCount++;
				} catch ( Exception ex ) {
					errorCount++;
					logger?.LogError( ex, "Error registering profiles for type {TypeName}", type.Name );
				}
			}

			logger?.LogInformation( "Instance-based profile registration completed. Success: {SuccessCount}, Errors: {ErrorCount}", successCount, errorCount );
		}

		/// <summary>
		/// Registers instance-based mapping profiles for a given source type.
		/// </summary>
		/// <remarks>
		/// This method identifies interfaces implemented by <paramref name="sourceType"/> that match the
		/// generic definition of <see cref="IMorphable{T}"/>. For each matching interface, it registers a mapping between
		/// <paramref name="sourceType"/> and the destination type specified by the generic argument of <see
		/// cref="IMorphable{T}"/>.
		/// </remarks>
		/// <param name="sourceType">The type to analyze for instance-based mapping profiles.</param>
		/// <param name="registry">The registry where the mappings will be registered.</param>
		/// <param name="logger">The logger instance for recording registration activities.</param>
		private static void RegisterInstanceBasedProfiles( Type sourceType, SchemaRegistry registry, ILogger? logger ) {
			var interfacesFound = 0;
			var mappingsRegistered = 0;

			foreach ( var iface in sourceType.GetInterfaces( ) ) {
				if ( !iface.IsGenericType )
					continue;

				var genericDef = iface.GetGenericTypeDefinition( );
				if ( genericDef != typeof( IMorphable<> ) )
					continue;

				interfacesFound++;
				var destinationType = iface.GenericTypeArguments[ 0 ];

				try {
					// Register a special mapping for types that implement IMorphable<TDestination>
					registry.RegisterInstanceBasedMapping( sourceType, destinationType );
					mappingsRegistered++;

					logger?.LogDebug( "Instance-based profile registered: {SourceType} -> {DestinationType}", sourceType.Name, destinationType.Name );
				} catch ( Exception ex ) {
					logger?.LogError( ex, "Error registering instance-based profile for {SourceType} -> {DestinationType}", sourceType.Name, destinationType.Name );
				}
			}

			if ( interfacesFound > 0 ) {
				logger?.LogTrace( "Processed {InterfaceCount} IMorphable interfaces for type {TypeName}, registered {MappingCount} mappings",
					interfacesFound, sourceType.Name, mappingsRegistered );
			}
		}
	}
}