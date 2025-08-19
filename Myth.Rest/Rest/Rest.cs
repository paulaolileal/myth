using Myth.Rest.Interfaces;

namespace Myth.Rest;

/// <summary>
/// Static entry point for REST operations
/// </summary>
public static class Rest {

	/// <summary>
	/// Create a new REST request builder with fluent interface
	/// </summary>
	/// <returns>REST configuration interface</returns>
	public static IRestBuilder Create( ) => new RestBuilder( );
}