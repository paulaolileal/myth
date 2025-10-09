/// <summary>
/// Thrown when message broker operations fail
/// </summary>
public sealed class MessageBrokerException : Exception {

    public MessageBrokerException( string message ) : base( message ) {
    }

    public MessageBrokerException( string message, Exception innerException )
		: base( message, innerException ) { }
}