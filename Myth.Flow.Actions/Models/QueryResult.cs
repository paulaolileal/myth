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

	private QueryResult( bool isSuccess, TData? data, string? errorMessage, Exception? exception, bool fromCache, Dictionary<string, object>? metadata ) {
		IsSuccess = isSuccess;
		Data = data;
		ErrorMessage = errorMessage;
		Exception = exception;
		FromCache = fromCache;
		Metadata = metadata;
	}

	/// <summary>
	/// Creates a successful query result with data
	/// </summary>
	/// <param name="data">The data returned by the query</param>
	/// <param name="fromCache">Indicates if the data was retrieved from cache</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A successful QueryResult with data</returns>
	public static QueryResult<TData> Success( TData data, bool fromCache = false, Dictionary<string, object>? metadata = null ) =>
		new( true, data, null, null, fromCache, metadata );

	/// <summary>
	/// Creates a failed query result
	/// </summary>
	/// <param name="errorMessage">The error message describing the failure</param>
	/// <param name="exception">The exception that caused the failure</param>
	/// <param name="metadata">Optional metadata to include with the result</param>
	/// <returns>A failed QueryResult</returns>
	public static QueryResult<TData> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null ) =>
		new( false, default, errorMessage, exception, false, metadata );

	/// <summary>
	/// Indicates whether the query execution failed
	/// </summary>
	public bool IsFailure => !IsSuccess;
}
