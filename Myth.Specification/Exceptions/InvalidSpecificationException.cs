namespace Myth.Exceptions;

[Serializable]
public sealed class InvalidSpecificationException( string message ) : Exception( message ) {
}
