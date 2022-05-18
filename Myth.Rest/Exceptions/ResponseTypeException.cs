namespace Myth.Exceptions {

    public class ResponseTypeException : Exception {
        public Type? InformedType { get; private set; }
        public Type? ExpectedType { get; private set; }

        protected ResponseTypeException( ) {
        }

        public ResponseTypeException( Type informedType, Type? expectedType )
            : base( $"The type informed {informedType} and the type expected {expectedType} are not the same!" ) {
            InformedType = informedType;
            ExpectedType = expectedType;
        }
    }
}