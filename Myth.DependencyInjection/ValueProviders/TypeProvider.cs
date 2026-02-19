using System.Reflection;

namespace Myth.ValueProviders;

public static class TypeProvider {

	/// <summary>
	/// The first part of your application namespace
	/// </summary>
	public static string? BaseApplicationNamespace => Assembly
		.GetCallingAssembly( )
		.GetName( ).Name?
		.Split( "." )
		.FirstOrDefault( );

	/// <summary>
	/// All assemblies from your application
	/// </summary>
	public static IEnumerable<Assembly> ApplicationAssemblies {
		get {
			var baseApplicationNamespace = BaseApplicationNamespace;

			var loadedAssembies = AppDomain.CurrentDomain
				.GetAssemblies( )
				.Where( x => !x.IsDynamic )
				.Where( x => !IsSystemAssembly( x ) );

			var localFiles = Directory.GetFiles(
				AppDomain.CurrentDomain.BaseDirectory,
				"*.dll",
				SearchOption.TopDirectoryOnly );

			var files = localFiles
				.Except( loadedAssembies.Select( x => x.Location ) );

			var assembliesFromFile = new List<Assembly>( );
			foreach ( var item in files ) {
				try {
					var assembly = Assembly.LoadFrom( item );
					if ( !assembly.IsDynamic && !IsSystemAssembly( assembly ) )
						assembliesFromFile.Add( assembly );
				} catch ( Exception ) { continue; }
			}

			return loadedAssembies
				.Concat( assembliesFromFile )
				.ToList( );
		}
	}

	/// <summary>
	/// All types exported by your application
	/// </summary>
	public static IEnumerable<Type> ApplicationTypes =>
		ApplicationAssemblies
			.SelectMany( assembly => GetTypesFromAssembly( assembly ) )
			.Where( x => !x.IsAbstract && !x.IsInterface )
			.ToList( );

	/// <summary>
	/// Get types derivaded from another type
	/// </summary>
	/// <typeparam name="TType">Base type - can be interface</typeparam>
	/// <returns>All types derived</returns>
	public static IEnumerable<Type> GetTypesAssignableFrom<TType>( ) =>
		ApplicationTypes
			.Where( x => typeof( TType ).IsAssignableFrom( x ) )
			.ToList( );

	/// <summary>
	/// Safely loads types from an assembly, handling ReflectionTypeLoadException
	/// </summary>
	/// <param name="assembly">The assembly to load types from</param>
	/// <returns>Collection of successfully loaded types</returns>
	private static IEnumerable<Type> GetTypesFromAssembly( Assembly assembly ) {
		try {
			return assembly.GetTypes( );
		} catch ( ReflectionTypeLoadException ex ) {
			// Return only the types that were successfully loaded
			return ex.Types.Where( t => t != null )!;
		} catch ( Exception ) {
			// For any other exception, return empty collection
			return [ ];
		}
	}

	/// <summary>
	/// Determines if an assembly is a system/framework assembly that should be excluded
	/// </summary>
	/// <param name="assembly">The assembly to check</param>
	/// <returns>True if the assembly is a system assembly, false otherwise</returns>
	private static bool IsSystemAssembly( Assembly assembly ) {
		var name = assembly.GetName( ).Name ?? string.Empty;

		// Exclude known problematic assemblies
		var excludedPrefixes = new[] {
			"Microsoft.Build",
			"Microsoft.CodeAnalysis",
			"Microsoft.VisualStudio",
			"NuGet.",
			"Newtonsoft.Json.Schema",
			"System.",
			"mscorlib",
			"netstandard"
		};

		return excludedPrefixes.Any( prefix => name.StartsWith( prefix, StringComparison.OrdinalIgnoreCase ) );
	}
}
