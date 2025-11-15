namespace Myth.Exceptions;

/// <summary>
/// Exception thrown when a constant is not found
/// </summary>
public class ConstantNotFoundException : Exception {
	public ConstantNotFoundException( string message ) : base( message ) { }
}
