using System.Net;

namespace Myth.Models;

/// <summary>
/// Represents the result of a pipeline operation, including success, value, error message, exception, and HTTP status code.
/// </summary>
public readonly struct Result<T> {

	/// <summary>
	/// Gets a value indicating whether the operation was successful.
	/// </summary>
	public bool IsSuccess { get; }

	/// <summary>
	/// Gets the value returned by the operation, or <c>null</c> if failed.
	/// </summary>
	public T? Value { get; }

	/// <summary>
	/// Gets the error message if the operation failed, or <c>null</c> if successful.
	/// </summary>
	public string? ErrorMessage { get; }

	/// <summary>
	/// Gets the exception associated with a failed operation, or <c>null</c> if successful.
	/// </summary>
	public Exception? Exception { get; }

	/// <summary>
	/// Gets the HTTP status code associated with this result, if any.
	/// Set on failure to propagate the semantic status code from handlers through the pipeline.
	/// </summary>
	public HttpStatusCode? StatusCode { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="Result{T}"/> with the specified parameters.
	/// </summary>
	private Result( bool isSuccess, T? value, string? errorMessage, Exception? exception, HttpStatusCode? statusCode = null ) {
		IsSuccess = isSuccess;
		Value = value;
		ErrorMessage = errorMessage;
		Exception = exception;
		StatusCode = statusCode;
	}

	/// <summary>
	/// Creates a successful <see cref="Result{T}"/> with the specified value.
	/// </summary>
	/// <param name="value">The value returned by the operation.</param>
	/// <returns>A successful <see cref="Result{T}"/>.</returns>
	public static Result<T> Success( T value ) =>
		new( true, value, null, null );

	/// <summary>
	/// Creates a failed <see cref="Result{T}"/> with the specified error message, optional exception, and optional HTTP status code.
	/// </summary>
	/// <param name="errorMessage">The error message describing the failure.</param>
	/// <param name="exception">The exception associated with the failure, or <c>null</c>.</param>
	/// <param name="statusCode">The HTTP status code associated with the failure, or <c>null</c>.</param>
	/// <returns>A failed <see cref="Result{T}"/>.</returns>
	public static Result<T> Failure( string errorMessage, Exception? exception = null, HttpStatusCode? statusCode = null ) =>
		new( false, default, errorMessage, exception, statusCode );

	/// <summary>
	/// Gets a value indicating whether the operation failed.
	/// </summary>
	public bool IsFailure => !IsSuccess;
}
