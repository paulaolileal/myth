using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces;
using Myth.Mapper;
using System.Reflection;

namespace Myth.Extensions {

	public static class ServiceCollectionExtensions {

		public static IServiceCollection AddMapper( this IServiceCollection services, Action<MapperSettings>? settings = null ) {
			var mapperSettings = new MapperSettings( );
			settings?.Invoke( mapperSettings );

			var assemblies = mapperSettings.Assemblies;
			if ( assemblies == null || !assemblies.Any( ) )
				assemblies = AppDomain.CurrentDomain.GetAssemblies( ).ToList( );

			services.AddSingleton<MapRegistry>( sp => {
				var registry = new MapRegistry( sp );

				// 1️⃣ Registra mapeamentos genéricos definidos manualmente
				foreach ( var (iface, concrete) in mapperSettings.GenericMappings )
					registry.RegisterGenericMapping( iface, concrete );

				// 2️⃣ Registra perfis com nova interface IMapTo<TDestination>
				RegisterInstanceBasedMapToProfiles( registry, assemblies );

				// 3️⃣ Registra o mapeamento automático para tipos genéricos iguais
				registry.RegisterGenericEqualTypesMapping( );

				return registry;
			} );

			DefaultProvider.EnsureProvider( services.BuildServiceProvider( ) );
			return services;
		}

		private static void RegisterInstanceBasedMapToProfiles( MapRegistry registry, List<Assembly> assemblies ) {
			var allTypes = assemblies
				.SelectMany( assembly => {
					try {
						return assembly.GetTypes( );
					} catch ( ReflectionTypeLoadException ex ) {
						return ex.Types.Where( t => t != null ).ToArray( )!;
					} catch {
						return Array.Empty<Type>( );
					}
				} )
				.Where( x => x != null && !x.IsAbstract && !x.IsInterface );

			foreach ( var type in allTypes ) {
				try {
					RegisterInstanceBasedProfiles( type, registry );
				} catch ( Exception ex ) {
					System.Diagnostics.Debug.WriteLine( $"[Mapper] Erro ao registrar profiles do tipo {type.Name}: {ex.Message}" );
				}
			}
		}

		private static void RegisterInstanceBasedProfiles( Type sourceType, MapRegistry registry ) {
			foreach ( var iface in sourceType.GetInterfaces( ) ) {
				if ( !iface.IsGenericType )
					continue;

				var genericDef = iface.GetGenericTypeDefinition( );
				if ( genericDef != typeof( IMapTo<> ) )
					continue;

				var destinationType = iface.GenericTypeArguments[ 0 ];

				try {
					// Registra um mapeamento especial para tipos que implementam IMapTo<TDestination>
					registry.RegisterInstanceBasedMapping( sourceType, destinationType );

					System.Diagnostics.Debug.WriteLine( $"[Mapper] Instance-based Profile registrado: {sourceType.Name} -> {destinationType.Name}" );
				} catch ( Exception ex ) {
					System.Diagnostics.Debug.WriteLine( $"[Mapper] Erro ao registrar instance-based profile {sourceType.Name}: {ex.Message}" );
				}
			}
		}
	}

	internal static class DefaultProvider {
		private static IServiceProvider? _serviceProvider;

		public static IServiceProvider? ServiceProvider {
			get => _serviceProvider;
			set => _serviceProvider = value;
		}

		public static void EnsureProvider( IServiceProvider? sp ) {
			if ( _serviceProvider == null && sp != null ) {
				ServiceProvider = sp;
			}
		}
	}

	public class MapperSettings {

		/// <summary>
		/// Assemblies para procurar mapeamentos via IMapTo
		/// </summary>
		public List<Assembly> Assemblies { get; set; } = new( );

		/// <summary>
		/// Registros de mapeamentos genéricos (ex: interface -> implementação concreta)
		/// </summary>
		public List<(Type iface, Type concrete)> GenericMappings { get; set; } = new( );

		/// <summary>
		/// Adiciona um assembly para procurar perfis de mapeamento
		/// </summary>
		public MapperSettings AddAssembly( Assembly assembly ) {
			if ( !Assemblies.Contains( assembly ) )
				Assemblies.Add( assembly );
			return this;
		}

		/// <summary>
		/// Adiciona um mapeamento genérico (ex: typeof(IPaginated&lt;&gt;), typeof(Paginated&lt;&gt;))
		/// </summary>
		public MapperSettings AddGenericMapping( Type ifaceGeneric, Type concreteGeneric ) {
			GenericMappings.Add( (ifaceGeneric, concreteGeneric) );
			return this;
		}

		/// <summary>
		/// Adiciona mapeamento genérico de forma type-safe
		/// </summary>
		public MapperSettings AddGenericMapping<TInterface, TConcrete>( )
			where TInterface : class
			where TConcrete : class, TInterface {
			var ifaceType = typeof( TInterface );
			var concreteType = typeof( TConcrete );

			// Verifica se são tipos genéricos
			if ( !ifaceType.IsGenericTypeDefinition || !concreteType.IsGenericTypeDefinition ) {
				throw new ArgumentException( "Ambos os tipos devem ser definições de tipos genéricos (ex: typeof(IList<>))" );
			}

			GenericMappings.Add( (ifaceType, concreteType) );
			return this;
		}
	}
}