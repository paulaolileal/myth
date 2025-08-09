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

				// 2️⃣ Registra perfis IMapTo<TSrc, TDest>
				var allTypes = assemblies
					.SelectMany( x => x.GetTypes( ) )
					.Where( x => !x.IsAbstract && !x.IsInterface );

				foreach ( var type in allTypes ) {
					foreach ( var iface in type.GetInterfaces( ) ) {
						if ( !type.IsClass || !iface.IsGenericType )
							continue;

						if ( iface.GetGenericTypeDefinition( ) == typeof( IMapTo<,> ) ) {
							var source = iface.GenericTypeArguments[ 0 ];
							var dest = iface.GenericTypeArguments[ 1 ];

							var instance = Activator.CreateInstance( type )!;

							var wrapperDelegate = typeof( ServiceCollectionExtensions )
								.GetMethod( nameof( BuildProfileWrapper ), BindingFlags.NonPublic | BindingFlags.Static )!
								.MakeGenericMethod( source, dest )
								.Invoke( null, new[ ] { instance } )!;

							var registerMethod = typeof( MapRegistry )
								.GetMethod( nameof( MapRegistry.Register ) )!
								.MakeGenericMethod( source, dest );

							registerMethod.Invoke( registry, new[ ] { wrapperDelegate } );
						}
					}
				}

				// 3️⃣ Registra o mapeamento automático para tipos genéricos iguais
				registry.RegisterGenericEqualTypesMapping( );

				return registry;
			} );

			// 4️⃣ Garante que o DefaultProvider está apontando para o container atual
			DefaultProvider.ServiceProvider = services.BuildServiceProvider( );

			return services;
		}

		private static Action<MappingBuilder<TSource, TDest>> BuildProfileWrapper<TSource, TDest>( IMapTo<TSource, TDest> profile ) {
			return builder => profile.MapTo( builder );
		}
	}

	internal static class DefaultProvider {
		public static IServiceProvider? ServiceProvider;
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
	}
}