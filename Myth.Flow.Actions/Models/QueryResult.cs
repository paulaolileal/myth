namespace Myth.Models;

/// <summary>
/// Represents query execution result
/// </summary>
/// <typeparam name="TData">Data type</typeparam>
public readonly struct QueryResult<TData> {
	public bool IsSuccess { get; }
	public TData? Data { get; }
	public string? ErrorMessage { get; }
	public Exception? Exception { get; }
	public bool FromCache { get; }
	public Dictionary<string, object>? Metadata { get; }

	private QueryResult( bool isSuccess, TData? data, string? errorMessage, Exception? exception, bool fromCache, Dictionary<string, object>? metadata ) {
		IsSuccess = isSuccess;
		Data = data;
		ErrorMessage = errorMessage;
		Exception = exception;
		FromCache = fromCache;
		Metadata = metadata;
	}

	public static QueryResult<TData> Success( TData data, bool fromCache = false, Dictionary<string, object>? metadata = null ) =>
		new( true, data, null, null, fromCache, metadata );

	public static QueryResult<TData> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, exception, false, metadata );

	public bool IsFailure => !IsSuccess;
}