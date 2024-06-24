namespace Myth.Exceptions {

	[Serializable]
	public sealed class InvalidSpecificationException : Exception {

		public InvalidSpecificationException( string message )
			: base( message ) {
		}
	}
}