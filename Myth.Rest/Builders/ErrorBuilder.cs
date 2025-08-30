using Myth.Extensions;
using Myth.Models;
using System.Net;

namespace Myth.Builders;

public class ErrorBuilder {
	private readonly ExceptionMappingList _exceptionMapping;
	public bool _throwForNonMappedResult;
	public string? _fallbackResponse;
	public HttpStatusCode? _fallbackStatusCode;

	public bool _useFallback =>
		!string.IsNullOrEmpty(_fallbackResponse)
		&& _fallbackStatusCode is not null;

	public ErrorBuilder() {
		_exceptionMapping = [];
		_throwForNonMappedResult = true;
	}

	/// <summary>
	/// Clear all maping exceptions
	/// </summary>
	public void Clear() => _exceptionMapping.Clear();

	internal bool TryGet(HttpStatusCode statusCode, dynamic content) =>
		_exceptionMapping.TryGet(statusCode, content);

	/// <summary>
	/// Throws a exception for all status that is not a success
	/// </summary>
	/// <param name="condition">A condition to check before the throw</param>
	/// <returns>This object</returns>
	public ErrorBuilder ThrowForNonSuccess(Func<dynamic, bool>? condition = null) {
		foreach (var statusCode in Enum.GetValues<HttpStatusCode>())
			if (!statusCode.IsSuccess())
				ThrowFor(statusCode, condition);

		return this;
	}

	/// <summary>
	/// Throws a exception for a specific status code
	/// </summary>
	/// <param name="statusCode">The status code</param>
	/// <param name="condition">A condition to check before the throw</param>
	/// <returns>This object</returns>
	public ErrorBuilder ThrowFor(HttpStatusCode statusCode, Func<dynamic, bool>? condition = null) {
		_exceptionMapping.Add(statusCode, condition);
		return this;
	}

	/// <summary>
	/// Throws a exception for a spec ific status code
	/// </summary>
	/// <param name="statusCodes"></param>
	/// <param name="condition">A condition to check before the throw</param>
	/// <returns>This object</returns>
	public ErrorBuilder ThrowFor(List<HttpStatusCode> statusCodes, Func<dynamic, bool>? condition = null) {
		foreach (var statusCode in statusCodes)
			ThrowFor(statusCode, condition);

		return this;
	}

	/// <summary>
	/// Throws a exception for all status code
	/// </summary>
	/// <param name="condition">A condition to check before the throw</param>
	/// <returns>This object</returns>
	public ErrorBuilder ThrowForAll(Func<dynamic, bool>? condition = null) {
		foreach (var statusCode in Enum.GetValues<HttpStatusCode>())
			_exceptionMapping.Add(statusCode, condition);

		return this;
	}

	/// <summary>
	/// Defines that not throw a exception if a status code is not mapped
	/// </summary>
	/// <returns>This object</returns>
	public ErrorBuilder NotThrowForNonMappedResult() {
		_throwForNonMappedResult = false;

		return this;
	}

	/// <summary>
	/// Not throws a exception for specific status code
	/// </summary>
	/// <param name="statusCode"></param>
	/// <returns>This object</returns>
	public ErrorBuilder NotThrowFor(HttpStatusCode statusCode) {
		_exceptionMapping.Remove(statusCode);
		return this;
	}

	/// <summary>
	/// Sets a fallback response for a specific status code using a generic object, serializing it to JSON.
	/// </summary>
	/// <typeparam name="T">The type of the fallback response object.</typeparam>
	/// <param name="statusCode">The HTTP status code to associate with the fallback response.</param>
	/// <param name="fallbackResponse">The fallback response object to use if the status code is returned.</param>
	/// <returns>This object.</returns>
	public ErrorBuilder UseFallback<T>(HttpStatusCode statusCode, T fallbackResponse) {
		if (fallbackResponse is not null) {
			_fallbackStatusCode = statusCode;
			_fallbackResponse = fallbackResponse.ToJson();
		}

		return this;
	}

	/// <summary>
	/// Sets a fallback response for a specific status code using a string response.
	/// </summary>
	/// <param name="statusCode">The HTTP status code to associate with the fallback response.</param>
	/// <param name="fallbackResponse">The fallback response string to use if the status code is returned.</param>
	/// <returns>This object.</returns>
	public ErrorBuilder UseFallback(HttpStatusCode statusCode, string fallbackResponse) {
		if (!string.IsNullOrEmpty(fallbackResponse)) {
			_fallbackStatusCode = statusCode;
			_fallbackResponse = fallbackResponse;
		}

		return this;
	}
}