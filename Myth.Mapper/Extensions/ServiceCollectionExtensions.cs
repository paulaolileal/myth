using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces;
using Myth.Mapper;
using System.Reflection;

namespace Myth.Extensions {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddMapper( this IServiceCollection services, params Assembly[ ] assemblies ) {
			if ( assemblies == null || assemblies.Length == 0 )
				assemblies = AppDomain.CurrentDomain.GetAssemblies( );

			services.AddSingleton<MapRegistry>( sp =>
			{
				var registry = new MapRegistry( sp );

				var allTypes = assemblies.SelectMany( x => x.GetTypes( ) )
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
								.GetMethod( nameof( ServiceCollectionExtensions.BuildProfileWrapper ), BindingFlags.NonPublic | BindingFlags.Static )!
								.MakeGenericMethod( source, dest )
								.Invoke( null, new object[ ] { instance } )!;

							var registerMethod = typeof( MapRegistry ).GetMethod( nameof( MapRegistry.Register ) )!
								.MakeGenericMethod( source, dest );

							registerMethod.Invoke( registry, new[ ] { wrapperDelegate } );
						}
					}
				}

				return registry;
			} );

			DefaultProvider.ServiceProvider = services.BuildServiceProvider( );
			return services;
		}

		private static Action<MappingBuilder<TSource, TDestination>> BuildProfileWrapper<TSource, TDestination>(IMapTo<TSource, TDestination> profile ) {
			return builder => profile.MapTo( builder );
		}
	}


	internal static class DefaultProvider {
		public static IServiceProvider? ServiceProvider;
	}

}