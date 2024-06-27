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
				.Where( x => !x.IsDynamic &&
							  x.GetName( ).Name!.Contains( baseApplicationNamespace! ) );

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
					if ( !assembly.IsDynamic &&
						 assembly.GetName( ).Name!.Contains( baseApplicationNamespace! ) )
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
			.SelectMany( x => x.GetTypes( ) )
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
}