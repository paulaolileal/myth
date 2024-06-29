namespace Myth.Rest;

public static class Rest {

	/// <summary>
	/// Init a content request
	/// </summary>
	/// <returns>Content request builder</returns>
	public static RestBuilder Create( ) => new( );

	/// <summary>
	/// Init a file request
	/// </summary>
	/// <returns>File request builder</returns>
	public static RestFileBuilder File( ) => new( );
}