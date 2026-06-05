using System.Net;
using Myth.Interfaces;

namespace Myth.Models;

/// <summary>
/// Represents command execution result
/// </summary>
public readonly struct CommandResult : IResultStatusCode {

	/// <summary>
	/// Indicates whether the command execution was successful
	/// </summary>
	public bool IsSuccess { get; }

	/// <summary>
	/// The error message if the command failed
	/// </summary>
	public string? ErrorMessage { get; }

	/// <summary>
	/// The exception that caused the failure, if any
	/// </summary>
	public Exception? Exception { get; }

	/// <summary>
	/// Additional metadata associated with the command execution
	/// </summary>
	public Dictionary<string, object>? Metadata { get; }

	/// <summary>
	/// The HTTP status code associated with this result.
	/// Defaults to <see cref="HttpStatusCode.OK"/> on success and
	/// <see cref="HttpStatusCode.BadRequest"/> on failure.
	/// </summary>
	public HttpStatusCode StatusCode { get; }

	private CommandResult( bool isSuccess, string? errorMessage, Exception? exception, Dictionary<string, object>? metadata, HttpStatusCode statusCode ) {
		IsSuccess = isSuccess;
		ErrorMessage = errorMessage;
		Exception = exception;
		Metadata = metadata;
		StatusCode = statusCode;
	}

	/// <summary>
	/// Creates a successful command result with HTTP 200 OK
	/// </summary>
	public static CommandResult Success( Dictionary<string, object>? metadata = null ) =>
		new( true, null, null, metadata, HttpStatusCode.OK );

	/// <summary>
	/// Creates a successful command result with HTTP 204 No Content,
	/// for commands that complete successfully but produce no response body.
	/// </summary>
	public static CommandResult NoContent( Dictionary<string, object>? metadata = null ) =>
		new( true, null, null, metadata, HttpStatusCode.NoContent );

	/// <summary>
	/// Creates a failed command result with HTTP 400 Bad Request
	/// </summary>
	public static CommandResult Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, errorMessage, exception, metadata, HttpStatusCode.BadRequest );

	/// <summary>
	/// Creates a failed command result with an explicit HTTP status code
	/// </summary>
	public static CommandResult Failure( string errorMessage, HttpStatusCode statusCode, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, errorMessage, exception, metadata, statusCode );

	/// <summary>
	/// Creates a failed command result with HTTP 404 Not Found
	/// </summary>
	public static CommandResult NotFound( string errorMessage, Dictionary<string, object>? metadata = null ) =>
		new( false, errorMessage, null, metadata, HttpStatusCode.NotFound );

	/// <summary>
	/// Creates a failed command result with HTTP 403 Forbidden
	/// </summary>
	public static CommandResult Forbidden( string errorMessage = "Access denied", Dictionary<string, object>? metadata = null ) =>
		new( false, errorMessage, null, metadata, HttpStatusCode.Forbidden );

	/// <summary>
	/// Creates a failed command result with HTTP 401 Unauthorized
	/// </summary>
	public static CommandResult Unauthorized( string errorMessage = "Unauthorized", Dictionary<string, object>? metadata = null ) =>
		new( false, errorMessage, null, metadata, HttpStatusCode.Unauthorized );

	/// <summary>
	/// Creates a failed command result with HTTP 402 Payment Required
	/// </summary>
	public static CommandResult PaymentRequired( string errorMessage, Dictionary<string, object>? metadata = null ) =>
		new( false, errorMessage, null, metadata, HttpStatusCode.PaymentRequired );

	/// <summary>
	/// Creates a failed command result with HTTP 409 Conflict
	/// </summary>
	public static CommandResult Conflict( string errorMessage, Dictionary<string, object>? metadata = null ) =>
		new( false, errorMessage, null, metadata, HttpStatusCode.Conflict );

	/// <summary>
	/// Creates a failed command result with HTTP 422 Unprocessable Entity
	/// </summary>
	public static CommandResult UnprocessableEntity( string errorMessage, Dictionary<string, object>? metadata = null ) =>
		new( false, errorMessage, null, metadata, HttpStatusCode.UnprocessableEntity );

	/// <summary>
	/// Indicates whether the command execution failed
	/// </summary>
	public bool IsFailure => !IsSuccess;
}

/// <summary>
/// Represents command execution result with typed response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public readonly struct CommandResult<TResponse> : IResultStatusCode {

	/// <summary>
	/// Indicates whether the command execution was successful
	/// </summary>
	public bool IsSuccess { get; }

	/// <summary>
	/// The data returned by the successful command execution
	/// </summary>
	public TResponse? Data { get; }

	/// <summary>
	/// The error message if the command failed
	/// </summary>
	public string? ErrorMessage { get; }

	/// <summary>
	/// The exception that caused the failure, if any
	/// </summary>
	public Exception? Exception { get; }

	/// <summary>
	/// Additional metadata associated with the command execution
	/// </summary>
	public Dictionary<string, object>? Metadata { get; }

	/// <summary>
	/// The HTTP status code associated with this result.
	/// Defaults to <see cref="HttpStatusCode.OK"/> on success and
	/// <see cref="HttpStatusCode.BadRequest"/> on failure.
	/// </summary>
	public HttpStatusCode StatusCode { get; }

	private CommandResult( bool isSuccess, TResponse? data, string? errorMessage, Exception? exception, Dictionary<string, object>? metadata, HttpStatusCode statusCode ) {
		IsSuccess = isSuccess;
		Data = data;
		ErrorMessage = errorMessage;
		Exception = exception;
		Metadata = metadata;
		StatusCode = statusCode;
	}

	/// <summary>
	/// Creates a successful command result with data and HTTP 200 OK
	/// </summary>
	public static CommandResult<TResponse> Success( TResponse data, Dictionary<string, object>? metadata = null ) =>
		new( true, data, null, null, metadata, HttpStatusCode.OK );

	/// <summary>
	/// Creates a successful command result with HTTP 204 No Content,
	/// for commands that complete successfully but produce no response body.
	/// </summary>
	public static CommandResult<TResponse> NoContent( Dictionary<string, object>? metadata = null ) =>
		new( true, default, null, null, metadata, HttpStatusCode.NoContent );

	/// <summary>
	/// Creates a failed command result with HTTP 400 Bad Request
	/// </summary>
	public static CommandResult<TResponse> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, exception, metadata, HttpStatusCode.BadRequest );

	/// <summary>
	/// Creates a failed command result with an explicit HTTP status code
	/// </summary>
	public static CommandResult<TResponse> Failure( string errorMessage, HttpStatusCode statusCode, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, exception, metadata, statusCode );

	/// <summary>
	/// Creates a failed command result with HTTP 404 Not Found
	/// </summary>
	public static CommandResult<TResponse> NotFound( string errorMessage, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, metadata, HttpStatusCode.NotFound );

	/// <summary>
	/// Creates a failed command result with HTTP 403 Forbidden
	/// </summary>
	public static CommandResult<TResponse> Forbidden( string errorMessage = "Access denied", Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, metadata, HttpStatusCode.Forbidden );

	/// <summary>
	/// Creates a failed command result with HTTP 401 Unauthorized
	/// </summary>
	public static CommandResult<TResponse> Unauthorized( string errorMessage = "Unauthorized", Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, metadata, HttpStatusCode.Unauthorized );

	/// <summary>
	/// Creates a failed command result with HTTP 402 Payment Required
	/// </summary>
	public static CommandResult<TResponse> PaymentRequired( string errorMessage, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, metadata, HttpStatusCode.PaymentRequired );

	/// <summary>
	/// Creates a failed command result with HTTP 409 Conflict
	/// </summary>
	public static CommandResult<TResponse> Conflict( string errorMessage, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, metadata, HttpStatusCode.Conflict );

	/// <summary>
	/// Creates a failed command result with HTTP 422 Unprocessable Entity
	/// </summary>
	public static CommandResult<TResponse> UnprocessableEntity( string errorMessage, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, null, metadata, HttpStatusCode.UnprocessableEntity );

	/// <summary>
	/// Indicates whether the command execution failed
	/// </summary>
	public bool IsFailure => !IsSuccess;
}
