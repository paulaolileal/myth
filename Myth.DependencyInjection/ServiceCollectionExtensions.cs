using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Myth.DependencyInjection {

    public static class ServiceCollectionExtensions {
        private static AssemblyService _assemblyServices = new( );

        public static IEnumerable<Assembly> GetAssemblies( ) => _assemblyServices.GetAssemblies( );

        public static IEnumerable<Type> GetTypes( ) => _assemblyServices.GetTypes( );

        public static void AddCustomAssemblies( IEnumerable<Assembly> customAssemblies ) =>
           _assemblyServices = new AssemblyService( customAssemblies );

        public static IServiceCollection AddServicesFromType<TType>( this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient, string interfaceName = "", params string[ ] filterNamespaces ) {
            var servicesDescriptors = _assemblyServices.GetServiceDescriptors<TType>( serviceLifetime, interfaceName, filterNamespaces );

            foreach ( var serviceDescriptor in servicesDescriptors )
                services.Add( serviceDescriptor );

            return services;
        }
    }
}