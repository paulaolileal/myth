using System.Net;

namespace Myth.Models;

/// <summary>
/// Represents query execution result
/// </summary>
/// <typeparam name="TData">Data type</typeparam>
public readonly struct QueryResult<TData> {

	/// <summary>
	/// Indicates whether the query execution was successful
	/// </summary>
	public bool IsSuccess { get; }

	/// <summary>
	/// The data returned by the query
	/// </summary>
	public TData? Data { get; }

	/// <summary>
	/// The error message if the query failed
	/// </summary>
	public string? ErrorMessage { get; }

	/// <summary>
	/// The exception that caused the failure, if any
	/// </summary>
	public Exception? Exception { get; }

	/// <summary>
	/// Indicates whether the result was retrieved from cache
	/// </summary>
	public bool FromCache { get; }

	/// <summary>
	/// Additional metadata associated with the query execution
	/// </summary>
	public Dictionary<string, object>? Metadata { get; }

	/// <summary>
	/// The HTTP status code associated with this result.
	/// Defaults to <see cref="HttpStatusCode.OK"/> on success and
	/// <see cref="HttpStatusCode.BadRequest"/> on failure.
	/// </summary>
	public HttpStatusCode StatusCode { get; }

	private QueryResult( bool isSuccess, TData? data, string? errorMessage, Exception? exception, bool fromCache, Dictionary<string, object>? metadata, HttpStatusCode statusCode ) {
		IsSuccess = isSuccess;
		Data = data;
		ErrorMessage = errorMessage;
		Exception = exception;
		FromCache = fromCache;
		Metadata = metadata;
		StatusCode = statusCode;
	}

	/// <summary>
	/// Creates a successful query result with data
	/// </summary>
	/// <param name="data">The data returned by the query</param>
	/// <param name="fromCache">Indicates if the data was retrieved from cache</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A successful QueryResult with data and HTTP 200 OK</returns>
	public static QueryResult<TData> Success( TData data, bool fromCache = false, Dictionary<string, object>? metadata = null ) =>
		new( true, data, null, null, fromCache, metadata, HttpStatusCode.OK );

	/// <summary>
	/// Creates a failed query result with HTTP 400 Bad Request
	/// </summary>
	/// <param name="errorMessage">The error message describing the failure</param>
	/// <param name="exception">The exception that caused the failure</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A failed QueryResult with HTTP 400 Bad Request</returns>
	public static QueryResult<TData> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, exception, false, metadata, HttpStatusCode.BadRequest );

	/// <summary>
	/// Creates a failed query result with an explicit HTTP status code
	/// </summary>
	/// <param name="errorMessage">The error message describing the failure</param>
	/// <param name="statusCode">The HTTP status code to associate with this failure</param>
	/// <param name="exception">The exception that caused the failure</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A failed QueryResult with the specified HTTP status code</returns>
	public static QueryResult<TData> Failure( string errorMessage, HttpStatusCode statusCode, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, exception, false, metadata, statusCode );

	/// <summary>
	/// Creates a result representing a resource that was not found (HTTP 404 Not Found).
	/// Prefer this over <c>Success(null!)</c> when the entity does not exist — the type
	/// should express absence explicitly.
	/// </summary>
	/// <param name="errorMessage">Optional message describing what was not found</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A QueryResult with HTTP 404 Not Found and no data</returns>
	public static QueryResult<TData> NotFound( string errorMessage = "Not found", Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, false, metadata, HttpStatusCode.NotFound );

	/// <summary>
	/// Creates a result representing an access denial (HTTP 403 Forbidden)
	/// </summary>
	/// <param name="errorMessage">Optional message describing the access restriction</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A QueryResult with HTTP 403 Forbidden and no data</returns>
	public static QueryResult<TData> Forbidden( string errorMessage = "Access denied", Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, false, metadata, HttpStatusCode.Forbidden );

	/// <summary>
	/// Creates a result representing an unauthenticated request (HTTP 401 Unauthorized)
	/// </summary>
	/// <param name="errorMessage">Optional message describing the authentication requirement</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A QueryResult with HTTP 401 Unauthorized and no data</returns>
	public static QueryResult<TData> Unauthorized( string errorMessage = "Unauthorized", Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, false, metadata, HttpStatusCode.Unauthorized );

	/// <summary>
	/// Indicates whether the query execution failed
	/// </summary>
	public bool IsFailure => !IsSuccess;
}
