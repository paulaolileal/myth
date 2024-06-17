namespace Myth.Exceptions {

	public class DifferentResponseTypeException : Exception {
		public Type InformedType { get; private set; }
		public Type ExpectedType { get; private set; }

		public DifferentResponseTypeException( Type informedType, Type expectedType )
			: base( $"The type informed {informedType.Name} and the type expected {expectedType.Name} are not the same!" ) {
			InformedType = informedType;
			ExpectedType = expectedType;
		}
	}
}