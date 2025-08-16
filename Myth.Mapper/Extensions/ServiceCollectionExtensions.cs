using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces;
using Myth.Morph;
using Myth.Settings;
using System.Diagnostics;
using System.Reflection;

namespace Myth.Extensions {

	public static class ServiceCollectionExtensions {

		/// <summary>
		/// Adds Myth Morph mapping services to the specified <see cref="IServiceCollection"/>.
		/// </summary>
		/// <remarks>This method registers the necessary services for Morph, including generic type mappings,
		/// instance-based mapping profiles, and automatic mappings for generic types with identical definitions. If no
		/// assemblies are specified in <paramref name="settings"/>, the method uses all assemblies loaded in the current
		/// application domain.</remarks>
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
				var registry = new SchemaRegistry( sp );

				// Registers generic mapping manually defined 
				foreach ( var (iface, concrete) in morphSettings.GenericMappings )
					registry.RegisterGenericMapping( iface, concrete );

				/// Registers profiles based on interface <see cref="IMorphable{TDestination}"/>
				RegisterInstanceBasedMapToProfiles( registry, assemblies );

				// Registers automatic mapping from equals generic
				registry.RegisterGenericEqualTypesMapping( );

				return registry;
			} );

			DefaultProvider.EnsureProvider( services.BuildServiceProvider( ) );

			return services;
		}

		/// <summary>
		/// Registers instance-based profiles from the specified assemblies into the provided bind registry.
		/// </summary>
		/// <remarks>This method iterates through all non-abstract, non-interface types in the provided assemblies and
		/// attempts to register instance-based profiles for each type into the specified registry. If an assembly cannot be
		/// fully loaded, only the successfully loaded types are processed. Any errors encountered during the registration of
		/// individual profiles are logged for debugging purposes.</remarks>
		/// <param name="registry">The <see cref="SchemaRegistry"/> where the profiles will be registered.</param>
		/// <param name="assemblies">A list of assemblies to scan for types containing instance-based profiles.</param>
		private static void RegisterInstanceBasedMapToProfiles( SchemaRegistry registry, List<Assembly> assemblies ) {
			var allTypes = assemblies
				.SelectMany( assembly => {
					try {
						return assembly.GetTypes( );
					} catch ( ReflectionTypeLoadException ex ) {
						return
							ex.Types
								.Where( t => t != null )
								.ToArray( )!;
					} catch {
						return [ ];
					}
				} )
				.Where( x =>
					x != null &&
					!x.IsAbstract &&
					!x.IsInterface );

			foreach ( var type in allTypes ) {
				try {
					RegisterInstanceBasedProfiles( type, registry );
				} catch ( Exception ex ) {
					Debug.WriteLine( $"[Morph] Error Erro ao registrar profiles do tipo {type.Name}: {ex.Message}" );
				}
			}
		}

		/// <summary>
		/// Registers instance-based mapping profiles for a given source type.
		/// </summary>
		/// <remarks>This method identifies interfaces implemented by <paramref name="sourceType"/> that match the
		/// generic definition of <see cref="IMorphable{T}"/>. For each matching interface, it registers a mapping between
		/// <paramref name="sourceType"/> and the destination type specified by the generic argument of <see
		/// cref="IMorphable{T}"/>.</remarks>
		/// <param name="sourceType">The type to analyze for instance-based mapping profiles.</param>
		/// <param name="registry">The registry where the mappings will be registered.</param>
		private static void RegisterInstanceBasedProfiles( Type sourceType, SchemaRegistry registry ) {
			foreach ( var iface in sourceType.GetInterfaces( ) ) {
				if ( !iface.IsGenericType )
					continue;

				var genericDef = iface.GetGenericTypeDefinition( );
				if ( genericDef != typeof( IMorphable<> ) )
					continue;

				var destinationType = iface.GenericTypeArguments[ 0 ];

				try {
					/// Registers a special mapping for types that implement <see cref="IMorphable{TDestination}"/>
					registry.RegisterInstanceBasedMapping( sourceType, destinationType );

					Debug.WriteLine( $"[Morph] Instance-based Profile registrado: {sourceType.Name} -> {destinationType.Name}" );
				} catch ( Exception ex ) {
					Debug.WriteLine( $"[Morph] Erro ao registrar instance-based profile {sourceType.Name}: {ex.Message}" );
				}
			}
		}
	}
}