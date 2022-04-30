using Microsoft.Extensions.DependencyInjection;
using Myth.Rest;

namespace Myth.DependencyInjection {

    public static class RestContainer {

        public static IServiceCollection AddRestFactory( this IServiceCollection services, string key ) => AddRestFactory( services, key );

        public static IServiceCollection AddRestFactory( this IServiceCollection services, string key, RestBuilder builder = null ) {
            var factory = GetFactory( services );

            if ( builder == null )
                builder = new RestBuilder( );

            factory.Add( key, builder );

            services.AddScoped<RestFactory>( x => factory );

            return services;
        }

        public static IServiceCollection AddRestFactory( this IServiceCollection services, string key, HttpClient client ) {
            var factory = GetFactory( services );

            var builder = new RestBuilder( client );

            factory.Add( key, builder );

            services.AddScoped<RestFactory>( x => factory );

            return services;
        }

        private static RestFactory GetFactory( IServiceCollection services ) {
            var serviceProvider = services.BuildServiceProvider( );

            var factory = serviceProvider.GetService<RestFactory>( );
            if ( factory == null ) {
                factory = new RestFactory( );
            } else {
                var service = services.FirstOrDefault( x => x.ServiceType == typeof( RestFactory ) );
                services.Remove( service );
            }

            return factory;
        }
    }
}