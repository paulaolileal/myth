using Microsoft.Extensions.Logging;

namespace Myth.Flow.Resilience {

	/// <summary>
	/// Retry policy for failed operations
	/// </summary>
	public sealed class RetryPolicy {
		private readonly int _maxAttempts;
		private readonly int _baseBackoffMs;
		private readonly bool _exponentialBackoff;
		private readonly ILogger? _logger;

		public RetryPolicy( int maxAttempts, int baseBackoffMs, bool exponentialBackoff, ILogger? logger = null ) {
			_maxAttempts = maxAttempts;
			_baseBackoffMs = baseBackoffMs;
			_exponentialBackoff = exponentialBackoff;
			_logger = logger;
		}

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

		private int CalculateDelay( int attempt ) {
			if ( !_exponentialBackoff )
				return _baseBackoffMs;

			return _baseBackoffMs * ( int )Math.Pow( 2, attempt - 1 );
		}
	}
}