using System.Net;
using Myth.Interfaces;

namespace Myth.Models;

/// <summary>
/// Represents query execution result
/// </summary>
/// <typeparam name="TData">Data type</typeparam>
public readonly struct QueryResult<TData> : IResultStatusCode {

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
	/// Creates a successful query result with data and HTTP 200 OK
	/// </summary>
	public static QueryResult<TData> Success( TData data, bool fromCache = false, Dictionary<string, object>? metadata = null ) =>
		new( true, data, null, null, fromCache, metadata, HttpStatusCode.OK );

	/// <summary>
	/// Creates a successful query result with HTTP 204 No Content,
	/// for queries that complete successfully but produce no response body
	/// (e.g. an optional resource that is intentionally absent).
	/// </summary>
	public static QueryResult<TData> NoContent( Dictionary<string, object>? metadata = null ) =>
		new( true, default, null, null, false, metadata, HttpStatusCode.NoContent );

	/// <summary>
	/// Creates a failed query result with HTTP 400 Bad Request
	/// </summary>
	public static QueryResult<TData> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, exception, false, metadata, HttpStatusCode.BadRequest );

	/// <summary>
	/// Creates a failed query result with an explicit HTTP status code
	/// </summary>
	public static QueryResult<TData> Failure( string errorMessage, HttpStatusCode statusCode, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, exception, false, metadata, statusCode );

	/// <summary>
	/// Creates a result representing a resource that was not found (HTTP 404 Not Found).
	/// Prefer this over <c>Success(null!)</c> when the entity does not exist.
	/// </summary>
	public static QueryResult<TData> NotFound( string errorMessage = "Not found", Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, false, metadata, HttpStatusCode.NotFound );

	/// <summary>
	/// Creates a result representing an access denial (HTTP 403 Forbidden)
	/// </summary>
	public static QueryResult<TData> Forbidden( string errorMessage = "Access denied", Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, false, metadata, HttpStatusCode.Forbidden );

	/// <summary>
	/// Creates a result representing an unauthenticated request (HTTP 401 Unauthorized)
	/// </summary>
	public static QueryResult<TData> Unauthorized( string errorMessage = "Unauthorized", Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, false, metadata, HttpStatusCode.Unauthorized );

	/// <summary>
	/// Creates a result representing a payment requirement (HTTP 402 Payment Required),
	/// for premium or paid-tier content that the caller has not unlocked.
	/// </summary>
	public static QueryResult<TData> PaymentRequired( string errorMessage, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, false, metadata, HttpStatusCode.PaymentRequired );

	/// <summary>
	/// Creates a result representing a state conflict detected as a pre-condition for the query
	/// (HTTP 409 Conflict). Use when the query cannot be answered because the current state
	/// is inconsistent — e.g., the resource exists in a state that makes the query invalid.
	/// </summary>
	public static QueryResult<TData> Conflict( string errorMessage, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, false, metadata, HttpStatusCode.Conflict );

	/// <summary>
	/// Indicates whether the query execution failed
	/// </summary>
	public bool IsFailure => !IsSuccess;
}
