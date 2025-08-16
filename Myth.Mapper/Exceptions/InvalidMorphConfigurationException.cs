namespace Myth.Exceptions {

	public class InvalidMorphConfigurationException : Exception {

		public InvalidMorphConfigurationException( ) {
		}

		public InvalidMorphConfigurationException( string? message ) : base( message ) {
		}
	}
}