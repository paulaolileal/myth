namespace Myth.Exceptions {

    public class StatusMapException : Exception {

        public StatusMapException( )
            : base( $"A exception map and result map cannot be at the same time on status code!" ) {
        }
    }
}