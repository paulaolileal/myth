using System.Reflection;

namespace Myth.Utils {

    public static class DirectoryPaths {

        public static string GetPublishDirectory( ) => Path.GetDirectoryName( Assembly.GetEntryAssembly( ).Location );
    }
}