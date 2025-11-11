namespace Myth.Flow.Actions.Test.Models;

/// <summary>
/// Test service for pipeline step testing
/// </summary>
public class TestService {

	/// <summary>
	/// Process some data
	/// </summary>
	/// <param name="input">Input data</param>
	/// <returns>Processed data</returns>
	public string ProcessData( string input ) {
		return $"Processed: {input}";
	}

	/// <summary>
	/// Validate some input
	/// </summary>
	/// <param name="input">Input to validate</param>
	/// <returns>True if valid</returns>
	public bool ValidateInput( string input ) {
		return !string.IsNullOrEmpty( input );
	}

	/// <summary>
	/// Log some message
	/// </summary>
	/// <param name="message">Message to log</param>
	public void LogMessage( string message ) {
		// Log implementation would go here
	}
}
