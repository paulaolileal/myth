namespace Myth.Testing.Test.Services {

	/// <summary>
	/// Example of expensive service that should be shared
	/// </summary>
	public class ExpensiveService {
		private readonly DateTime _createdAt;

		/// <summary>
		/// Initialize expensive service
		/// </summary>
		public ExpensiveService( ) {
			// Simulate expensive initialization
			_createdAt = DateTime.UtcNow;
			Thread.Sleep( 100 ); // Simulate slow initialization
		}

		/// <summary>
		/// Get service creation time
		/// </summary>
		public DateTime CreatedAt => _createdAt;

		/// <summary>
		/// Expensive operation
		/// </summary>
		/// <param name="value">Input value</param>
		/// <returns>Processed value</returns>
		public async Task<string> ProcessAsync( string value ) {
			// Simulate expensive processing
			await Task.Delay( 50 );

			return $"Processed: {value}";
		}
	}
}