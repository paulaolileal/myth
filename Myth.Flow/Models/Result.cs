namespace Myth.Models {

	/// <summary>
	/// Represents the result of a pipeline operation, including success, value, error message, and exception.
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
		/// Initializes a new instance of <see cref="Result{T}"/> with the specified parameters.
		/// </summary>
		/// <param name="isSuccess">Indicates if the operation was successful.</param>
		/// <param name="value">The value returned by the operation.</param>
		/// <param name="errorMessage">The error message if failed.</param>
		/// <param name="exception">The exception if failed.</param>
		private Result( bool isSuccess, T? value, string? errorMessage, Exception? exception ) {
			IsSuccess = isSuccess;
			Value = value;
			ErrorMessage = errorMessage;
			Exception = exception;
		}

		/// <summary>
		/// Creates a successful <see cref="Result{T}"/> with the specified value.
		/// </summary>
		/// <param name="value">The value returned by the operation.</param>
		/// <returns>A successful <see cref="Result{T}"/>.</returns>
		public static Result<T> Success( T value ) =>
			new( true, value, null, null );

		/// <summary>
		/// Creates a failed <see cref="Result{T}"/> with the specified error message and optional exception.
		/// </summary>
		/// <param name="errorMessage">The error message describing the failure.</param>
		/// <param name="exception">The exception associated with the failure, or <c>null</c>.</param>
		/// <returns>A failed <see cref="Result{T}"/>.</returns>
		public static Result<T> Failure( string errorMessage, Exception? exception = null ) =>
			new( false, default, errorMessage, exception );

		/// <summary>
		/// Gets a value indicating whether the operation failed.
		/// </summary>
		public bool IsFailure => !IsSuccess;
	}
}