namespace Myth.Exceptions {

    public class MapContentException : Exception {
        public Type TypeInformed { get; private set; }
        public string ContentExpected { get; private set; }

        public MapContentException( Type typeInformed, string contentExpected, Exception innerException )
            : base( $"The type informed is different from the type expected by the request!", innerException ) {
            TypeInformed = typeInformed;
            ContentExpected = contentExpected;
        }
    }
}