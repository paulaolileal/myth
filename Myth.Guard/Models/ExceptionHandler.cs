using Microsoft.AspNetCore.Http;

namespace Myth.Models; 

/// <summary>
/// Defines how a specific exception type should be handled by the Guard middleware
/// </summary>
public sealed class ExceptionHandler {

	/// <summary>
	/// The HTTP status code to return for this exception
	/// </summary>
	public Func<Exception, int> StatusCodeResolver { get; set; } = _ => 500;

	/// <summary>
	/// The error code to include in the response
	/// </summary>
	public Func<Exception, string>? ErrorCodeResolver { get; set; }

	/// <summary>
	/// Builds the response object from the exception
	/// </summary>
	public Func<Exception, object> ResponseBuilder { get; set; } = ex => new { error = ex.Message };

	/// <summary>
	/// Optional callback executed before writing the response (for logging, telemetry, etc.)
	/// </summary>
	public Action<Exception, HttpContext>? OnBeforeResponse { get; set; }

	/// <summary>
	/// The exception type this handler is for
	/// </summary>
	public Type ExceptionType { get; set; } = typeof( Exception );
}
