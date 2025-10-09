/// <summary>
/// Thrown when cache operations fail
/// </summary>
public sealed class CacheException : Exception {

	public CacheException( string message ) : base( message ) {
	}

	public CacheException( string message, Exception innerException )
		: base( message, innerException ) { }
}