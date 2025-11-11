namespace Myth.Models;

/// <summary>
/// Represents command execution result
/// </summary>
public readonly struct CommandResult {

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

	private CommandResult( bool isSuccess, string? errorMessage, Exception? exception, Dictionary<string, object>? metadata ) {
		IsSuccess = isSuccess;
		ErrorMessage = errorMessage;
		Exception = exception;
		Metadata = metadata;
	}

	/// <summary>
	/// Creates a successful command result
	/// </summary>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A successful CommandResult</returns>
	public static CommandResult Success( Dictionary<string, object>? metadata = null ) =>
		new( true, null, null, metadata );

	/// <summary>
	/// Creates a failed command result
	/// </summary>
	/// <param name="errorMessage">The error message describing the failure</param>
	/// <param name="exception">The exception that caused the failure</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A failed CommandResult</returns>
	public static CommandResult Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, errorMessage, exception, metadata );

	/// <summary>
	/// Indicates whether the command execution failed
	/// </summary>
	public bool IsFailure => !IsSuccess;
}

/// <summary>
/// Represents command execution result with typed response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public readonly struct CommandResult<TResponse> {

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

	private CommandResult( bool isSuccess, TResponse? data, string? errorMessage, Exception? exception, Dictionary<string, object>? metadata ) {
		IsSuccess = isSuccess;
		Data = data;
		ErrorMessage = errorMessage;
		Exception = exception;
		Metadata = metadata;
	}

	/// <summary>
	/// Creates a successful command result with data
	/// </summary>
	/// <param name="data">The response data from the command</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A successful CommandResult with data</returns>
	public static CommandResult<TResponse> Success( TResponse data, Dictionary<string, object>? metadata = null ) =>
		new( true, data, null, null, metadata );

	/// <summary>
	/// Creates a failed command result
	/// </summary>
	/// <param name="errorMessage">The error message describing the failure</param>
	/// <param name="exception">The exception that caused the failure</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A failed CommandResult</returns>
	public static CommandResult<TResponse> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, exception, metadata );

	/// <summary>
	/// Indicates whether the command execution failed
	/// </summary>
	public bool IsFailure => !IsSuccess;
}
