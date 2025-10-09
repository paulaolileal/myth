namespace Myth.Models;

/// <summary>
/// Represents command execution result
/// </summary>
public readonly struct CommandResult {
	public bool IsSuccess { get; }
	public string? ErrorMessage { get; }
	public Exception? Exception { get; }
	public Dictionary<string, object>? Metadata { get; }

	private CommandResult( bool isSuccess, string? errorMessage, Exception? exception, Dictionary<string, object>? metadata ) {
		IsSuccess = isSuccess;
		ErrorMessage = errorMessage;
		Exception = exception;
		Metadata = metadata;
	}

	public static CommandResult Success( Dictionary<string, object>? metadata = null ) =>
		new( true, null, null, metadata );

	public static CommandResult Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, errorMessage, exception, metadata );

	public bool IsFailure => !IsSuccess;
}

/// <summary>
/// Represents command execution result with typed response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public readonly struct CommandResult<TResponse> {
	public bool IsSuccess { get; }
	public TResponse? Data { get; }
	public string? ErrorMessage { get; }
	public Exception? Exception { get; }
	public Dictionary<string, object>? Metadata { get; }

	private CommandResult( bool isSuccess, TResponse? data, string? errorMessage, Exception? exception, Dictionary<string, object>? metadata ) {
		IsSuccess = isSuccess;
		Data = data;
		ErrorMessage = errorMessage;
		Exception = exception;
		Metadata = metadata;
	}

	public static CommandResult<TResponse> Success( TResponse data, Dictionary<string, object>? metadata = null ) =>
		new( true, data, null, null, metadata );

	public static CommandResult<TResponse> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, exception, metadata );

	public bool IsFailure => !IsSuccess;
}