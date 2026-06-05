using System.Net;

namespace Myth.Interfaces;

/// <summary>
/// Exposes the HTTP status code and success state from a command or query result,
/// allowing the pipeline to propagate them without reflection or type casting.
/// </summary>
public interface IResultStatusCode {

	/// <summary>
	/// The HTTP status code associated with this result.
	/// </summary>
	HttpStatusCode StatusCode { get; }

	/// <summary>
	/// Indicates whether the operation was successful.
	/// </summary>
	bool IsSuccess { get; }
}
