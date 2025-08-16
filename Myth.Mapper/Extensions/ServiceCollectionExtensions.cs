using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces;
using Myth.Morph;
using Myth.Settings;
using System.Diagnostics;
using System.Reflection;

namespace Myth.Extensions {

	public static class ServiceCollectionExtensions {

		public static IServiceCollection AddMorph( this IServiceCollection services, Action<MorphSettings>? settings = null ) {
			var morphSettings = new MorphSettings( );
			settings?.Invoke( morphSettings );

			var assemblies = morphSettings.Assemblies;
			if ( assemblies == null || assemblies.Count == 0 )
				assemblies = AppDomain.CurrentDomain.GetAssemblies( ).ToList( );

			services.AddSingleton( sp => {
				var registry = new BindRegistry( sp );

				// Registra mapeamentos genéricos definidos manualmente
				foreach ( var (iface, concrete) in morphSettings.GenericMappings )
					registry.RegisterGenericMapping( iface, concrete );

				// Registra perfis com nova interface IMapTo<TDestination>
				RegisterInstanceBasedMapToProfiles( registry, assemblies );

				// Registra o mapeamento automático para tipos genéricos iguais
				registry.RegisterGenericEqualTypesMapping( );

				return registry;
			} );

			DefaultProvider.EnsureProvider( services.BuildServiceProvider( ) );
			return services;
		}

		private static void RegisterInstanceBasedMapToProfiles( BindRegistry registry, List<Assembly> assemblies ) {
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
					Debug.WriteLine( $"[Morph] Erro ao registrar profiles do tipo {type.Name}: {ex.Message}" );
				}
			}
		}

		private static void RegisterInstanceBasedProfiles( Type sourceType, BindRegistry registry ) {
			foreach ( var iface in sourceType.GetInterfaces( ) ) {
				if ( !iface.IsGenericType )
					continue;

				var genericDef = iface.GetGenericTypeDefinition( );
				if ( genericDef != typeof( IMorphTo<> ) )
					continue;

				var destinationType = iface.GenericTypeArguments[ 0 ];

				try {
					// Registra um mapeamento especial para tipos que implementam IMapTo<TDestination>
					registry.RegisterInstanceBasedMapping( sourceType, destinationType );

					Debug.WriteLine( $"[Morph] Instance-based Profile registrado: {sourceType.Name} -> {destinationType.Name}" );
				} catch ( Exception ex ) {
					Debug.WriteLine( $"[Morph] Erro ao registrar instance-based profile {sourceType.Name}: {ex.Message}" );
				}
			}
		}
	}
}