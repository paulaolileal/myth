using System.Runtime.Serialization;

namespace Myth.Exceptions {

    [Serializable]
    public sealed class InvalidSpecificationException : Exception {

        private InvalidSpecificationException( SerializationInfo info, StreamingContext context )
            : base( info, context ) {
        }

        public InvalidSpecificationException( string message )
            : base( message ) {
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context ) {
            base.GetObjectData( info, context );
        }
    }
}