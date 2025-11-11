namespace Myth.Exceptions;

public class DifferentResponseTypeException( Type informedType, Type expectedType ) : Exception( $"The type informed {informedType.Name} and the type expected {expectedType.Name} are not the same!" ) {
	public Type InformedType { get; private set; } = informedType;
	public Type ExpectedType { get; private set; } = expectedType;
}
