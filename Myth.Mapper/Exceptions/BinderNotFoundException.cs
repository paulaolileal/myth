namespace Myth.Exceptions {

	public class BinderNotFoundException : Exception {

		public BinderNotFoundException( ) {
		}

		public BinderNotFoundException( string? message ) : base( message ) {
		}
	}
}