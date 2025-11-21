using Microsoft.Extensions.Logging;

namespace Myth.Flow.Resilience;

/// <summary>
/// Retry policy for failed operations
/// </summary>
public sealed class RetryPolicy( int maxAttempts, int baseBackoffMs, bool exponentialBackoff, ILogger? logger = null ) {
	private readonly int _maxAttempts = maxAttempts;
	private readonly int _baseBackoffMs = baseBackoffMs;
	private readonly bool _exponentialBackoff = exponentialBackoff;
	private readonly ILogger? _logger = logger;

	/// <summary>
	/// Executes an operation with retry logic based on the configured policy
	/// </summary>
	/// <typeparam name="T">The return type of the operation</typeparam>
	/// <param name="operation">The async operation to execute with retry</param>
	/// <param name="cancellationToken">Token to cancel the operation</param>
	/// <returns>The result of the successful operation</returns>
	/// <exception cref="Exception">Throws the last exception if all retry attempts fail</exception>
	public async Task<T> ExecuteAsync<T>(
		Func<Task<T>> operation,
		CancellationToken cancellationToken = default ) {
		var attempt = 0;

		while ( true ) {
			try {
				return await operation( );
			} catch ( Exception ex ) when ( attempt < _maxAttempts - 1 ) {
				attempt++;
				var delay = CalculateDelay( attempt );

				_logger?.LogWarning( ex,
					"Operation failed (attempt {Attempt}/{MaxAttempts}). Retrying in {Delay}ms...",
					attempt, _maxAttempts, delay );

				await Task.Delay( delay, cancellationToken );
			}
		}
	}

	/// <summary>
	/// Calculates the delay before the next retry attempt
	/// </summary>
	/// <param name="attempt">The current attempt number (1-indexed)</param>
	/// <returns>The delay in milliseconds</returns>
	private int CalculateDelay( int attempt ) {
		if ( !_exponentialBackoff )
			return _baseBackoffMs;

		return _baseBackoffMs * ( int )Math.Pow( 2, attempt - 1 );
	}
}
