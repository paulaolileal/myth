using Myth.Extensions;
using Myth.Models;
using System.Net;

namespace Myth.Builders;

public class ResultBuilder {
	private readonly ResultMappingList _resultMapping;
	private bool _shouldMap = true;
	public bool ShouldMap => _shouldMap;

	public ResultBuilder() {
		_resultMapping = [];
	}

	/// <summary>
	/// Clear all type mappings
	/// </summary>
	public void Clear() => _resultMapping.Clear();

	internal dynamic TryGet(HttpStatusCode statusCode, dynamic content, out Type? type) =>
		_resultMapping.TryGet(statusCode, content, out type);

	/// <summary>
	/// Set to ignore mapping types
	/// </summary>
	/// <returns></returns>
	public ResultBuilder DoNotMap() {
		_shouldMap = false;
		return this;
	}

	/// <summary>
	/// Set the same type for all success status codes
	/// </summary>
	/// <typeparam name="TResult">The type</typeparam>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseTypeForSuccess<TResult>(Func<dynamic, bool>? condition = null) =>
		UseTypeForSuccess(typeof(TResult), condition);

	/// <summary>
	/// Set the same type for all success status code
	/// </summary>
	/// <param name="type">The type</param>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseTypeForSuccess(Type type, Func<dynamic, bool>? condition = null) {
		foreach (var statusCode in Enum.GetValues<HttpStatusCode>()) {
			if (statusCode.IsSuccess())
				UseTypeFor(statusCode, type, condition);
		}

		return this;
	}

	/// <summary>
	/// Set the same type for all non success status codes
	/// </summary>
	/// <typeparam name="TResult">The type</typeparam>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseTypeForNonSuccess<TResult>(Func<dynamic, bool>? condition = null) =>
		UseTypeForNonSuccess(typeof(TResult), condition);

	/// <summary>
	/// Set the same type for all non success status code
	/// </summary>
	/// <param name="type">The type</param>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseTypeForNonSuccess(Type type, Func<dynamic, bool>? condition = null) {
		foreach (var statusCode in Enum.GetValues<HttpStatusCode>()) {
			if (!statusCode.IsSuccess())
				UseTypeFor(statusCode, type, condition);
		}

		return this;
	}

	/// <summary>
	/// Set a type for a specific status code
	/// </summary>
	/// <typeparam name="TResult">The type</typeparam>
	/// <param name="statusCode">A status code</param>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseTypeFor<TResult>(HttpStatusCode statusCode, Func<dynamic, bool>? condition = null) =>
		UseTypeFor(statusCode, typeof(TResult), condition);

	/// <summary>
	/// Set a type for a specific status code
	/// </summary>
	/// <param name="statusCode">The type</param>
	/// <param name="type">The type</param>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseTypeFor(HttpStatusCode statusCode, Type type, Func<dynamic, bool>? condition = null) {
		_resultMapping.Add(statusCode, type, condition);
		return this;
	}

	/// <summary>
	/// Set a empty type for a specifict status code
	/// </summary>
	/// <param name="statusCode">A status code</param>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseEmptyFor(HttpStatusCode statusCode, Func<dynamic, bool>? condition = null) {
		_resultMapping.Add(statusCode, typeof(string), condition);
		return this;
	}

	/// <summary>
	/// Set a type for many status codes
	/// </summary>
	/// <param name="statusCodes">A list of status codes</param>
	/// <param name="type">The type</param>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseTypeFor(IEnumerable<HttpStatusCode> statusCodes, Type type, Func<dynamic, bool>? condition = null) {
		foreach (var statusCode in statusCodes)
			_resultMapping.Add(statusCode, type, condition);

		return this;
	}

	/// <summary>
	/// Set a type for many status codes
	/// </summary>
	/// <typeparam name="TResult">The type</typeparam>
	/// <param name="statusCodes">A list of status codes</param>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseTypeFor<TResult>(IEnumerable<HttpStatusCode> statusCodes, Func<dynamic, bool>? condition = null) =>
		UseTypeFor(statusCodes, typeof(TResult), condition);

	/// <summary>
	/// Set the same type for all status codes
	/// </summary>
	/// <typeparam name="TResult">The type</typeparam>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseTypeForAll<TResult>(Func<dynamic, bool>? condition = null) =>
		UseTypeForAll(typeof(TResult), condition);

	/// <summary>
	/// Set the same type for all status codes
	/// </summary>
	/// <param name="type">The type</param>
	/// <param name="condition">A condition to check before the mapping</param>
	/// <returns>This object</returns>
	public ResultBuilder UseTypeForAll(Type type, Func<dynamic, bool>? condition = null) {
		foreach (var statusCode in Enum.GetValues<HttpStatusCode>())
			_resultMapping.Add(statusCode, type, condition);

		return this;
	}
}