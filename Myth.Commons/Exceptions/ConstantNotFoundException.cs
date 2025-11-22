namespace Myth.Exceptions;

/// <summary>
/// Exception thrown when a constant is not found
/// </summary>
public class ConstantNotFoundException( string message ) : Exception( message ) {
}
