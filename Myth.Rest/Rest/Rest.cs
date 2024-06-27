namespace Myth.Rest;

public static class Rest {

	public static RestBuilder Create( ) => new( );

	public static RestFileBuilder File( ) => new( );
}