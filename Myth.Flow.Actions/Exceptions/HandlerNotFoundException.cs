/// <summary>
/// Thrown when no handler is found for a request
/// </summary>
public sealed class HandlerNotFoundException : InvalidOperationException {

	public HandlerNotFoundException( string message ) : base( message ) {
	}

	public HandlerNotFoundException( string message, Exception innerException )
		: base( message, innerException ) { }
}