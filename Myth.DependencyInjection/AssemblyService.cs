using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using System.Reflection;

namespace Myth.DependencyInjection {

    public class AssemblyService {
        private readonly IEnumerable<Assembly> _customAssemblies;
        private readonly IEnumerable<Assembly> _localAssemblies;
        private readonly IEnumerable<Type> _localTypes;

        private readonly IEnumerable<string> _ignoredNamespaces = new List<string> { "Myth", "Harpy" };

        public AssemblyService( ) {
            _customAssemblies = new List<Assembly>( );
            _localTypes = LoadTypes( );
            _localAssemblies = LoadAssemblies( );
        }

        public AssemblyService( IEnumerable<Assembly> customAssemblies ) : this( ) {
            _customAssemblies = customAssemblies;
        }

        public IEnumerable<Type> GetTypesAssignableFrom<TType>( params string[ ] filterNamespaces ) {
            var result = _localTypes.Where( t => typeof( TType ).IsAssignableFrom( t ) );

            if ( filterNamespaces.Any( ) )
                result = result.Where( t => t.Namespace is not null && !t.Namespace.ContainsAny( filterNamespaces ) );

            return result;
        }

        public IEnumerable<ServiceDescriptor> GetServiceDescriptors<TType>( ServiceLifetime serviceLifetime, string interfaceName = "", params string[ ] filterNamespaces ) {
            var typeList = GetTypesAssignableFrom<TType>( filterNamespaces );

            var result = typeList.Select( type => {
                var @interface = interfaceName;

                if ( string.IsNullOrEmpty( interfaceName ) )
                    @interface = type.Name;

                var interfaceType = type
                     .GetInterfaces( )
                     .FirstOrDefault( i => i.Name
                        .ToLower( )
                        .Contains( @interface.ToLower( ) ) );

                return new ServiceDescriptor( interfaceType, type, serviceLifetime );
            } );

            return result;
        }

        public IEnumerable<Assembly> GetAssemblies( ) => _localAssemblies;

        public IEnumerable<Type> GetTypes( ) => _localTypes;

        private List<Type> LoadTypes( ) {
            var current = AppDomain.CurrentDomain
                .GetAssemblies( )
                .Where( x => !x.IsDynamic );

            if ( _customAssemblies != null && _customAssemblies.Any( ) )
                current = current.Concat( _customAssemblies );

            var assembliesFromFiles = LoadAssembliesFromFiles( current );

            var assemblies = current
                .Concat( assembliesFromFiles )
                .Where( a => !a.IsDynamic )
                .Distinct( )
                .OrderBy( x => x.FullName )
                .ToList( );

            var frameworkAssemblies = LoadFrameworkAssemblies( assemblies, _ignoredNamespaces );

            var frameworkTypes = LoadFrameworkTypes( frameworkAssemblies );

            var types = LoadTypes( assemblies, frameworkTypes, _ignoredNamespaces );

            return types
                .Distinct( )
                .ToList( );
        }

        private List<Assembly> LoadAssemblies( ) =>
            _localTypes
                .Select( x => x.Assembly )
                .Distinct( )
                .ToList( );

        private static List<Type> LoadTypes( IEnumerable<Assembly> assemblies, IEnumerable<Type> frameworkTypes, IEnumerable<string> ignoreNames ) {
            var types = new List<Type>( );
            foreach ( var assembly in assemblies ) {
                try {
                    foreach ( var type in assembly.GetExportedTypes( ) ) {
                        if ( !type.IsAbstract && !type.IsInterface &&
                            frameworkTypes.Any( x => x.IsAssignableFrom( type ) ) &&
                            ( type.Namespace is not null && !type.Namespace.StartsWithAny( ignoreNames ) ) )
                            types.Add( type );
                    }
                } catch ( Exception ) { continue; }
            }

            return types;
        }

        private static IEnumerable<Type> LoadFrameworkTypes( IEnumerable<Assembly> frameworkAssemblies ) =>
            frameworkAssemblies.SelectMany( assembly => assembly.GetExportedTypes( ) );

        private IEnumerable<Assembly> LoadFrameworkAssemblies( List<Assembly> assemblies, IEnumerable<string> ignoreNames ) =>
            assemblies.Where( assembly => assembly.FullName.StartsWithAny( ignoreNames ) );

        private static List<Assembly> LoadAssembliesFromFiles( IEnumerable<Assembly> current ) {
            var localFiles = Directory.GetFiles( AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.TopDirectoryOnly );

            var files = localFiles
                .Except( current.Select( x => x.Location ) );

            var localAssembliesFromFiles = new List<Assembly>( );
            foreach ( var item in files )
                try {
                    localAssembliesFromFiles.Add( Assembly.LoadFrom( item ) );
                } catch ( Exception ) { continue; }

            return localAssembliesFromFiles;
        }
    }
}