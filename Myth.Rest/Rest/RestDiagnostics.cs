using System.Diagnostics;

namespace Myth.Rest;

/// <summary>
/// Diagnostic information for REST operations
/// </summary>
public class RestDiagnostics {
	private static readonly ActivitySource _activitySource = new( "Myth.Rest" );

	/// <summary>
	/// Create a new activity for REST request
	/// </summary>
	public static Activity? StartActivity( string operationName, string? url = null, string? method = null ) {
		var activity = _activitySource.StartActivity( operationName );

		if ( activity != null ) {
			if ( !string.IsNullOrEmpty( url ) ) {
				activity.SetTag( "http.url", url );
			}
			if ( !string.IsNullOrEmpty( method ) ) {
				activity.SetTag( "http.method", method );
			}
		}

		return activity;
	}
}
