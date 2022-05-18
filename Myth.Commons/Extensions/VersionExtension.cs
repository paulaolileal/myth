using System.Reflection;

namespace Myth.Extensions {

    public static class AppVersion {

        public static string GetCurrent( ) => Assembly
            .GetCallingAssembly( )
            .GetName( )
            .Version
            .ToString( );
    }
}