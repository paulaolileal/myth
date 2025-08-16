namespace Myth.Rest;

public static class Rest {

	/// <summary>
	/// Init a unified REST request builder
	/// </summary>
	/// <returns>Unified REST request builder</returns>
	public static RestBuilder Create( ) => new( );
}