using Microsoft.Extensions.Logging;

namespace Myth.Flow.Resilience;

/// <summary>
/// Circuit breaker policy for protecting resources
/// </summary>
public sealed class CircuitBreakerPolicy {
	private readonly int _failureThreshold;
	private readonly TimeSpan _openDuration;
	private readonly ILogger? _logger;

	private int _failureCount;
	private DateTimeOffset _lastFailureTime;
	private CircuitState _state = CircuitState.Closed;
	private readonly object _lock = new( );

	public CircuitBreakerPolicy( int failureThreshold, TimeSpan openDuration, ILogger? logger = null ) {
		_failureThreshold = failureThreshold;
		_openDuration = openDuration;
		_logger = logger;
	}

	public async Task<T> ExecuteAsync<T>(
		Func<Task<T>> operation,
		CancellationToken cancellationToken = default ) {
		lock ( _lock ) {
			if ( _state == CircuitState.Open ) {
				if ( DateTimeOffset.UtcNow - _lastFailureTime >= _openDuration ) {
					_state = CircuitState.HalfOpen;
					_logger?.LogInformation( "Circuit breaker transitioning to HalfOpen state" );
				} else {
					throw new InvalidOperationException( "Circuit breaker is open" );
				}
			}
		}

		try {
			var result = await operation( );

			lock ( _lock ) {
				if ( _state == CircuitState.HalfOpen ) {
					_state = CircuitState.Closed;
					_failureCount = 0;
					_logger?.LogInformation( "Circuit breaker closed" );
				}
			}

			return result;
		} catch ( Exception ex ) {
			lock ( _lock ) {
				_failureCount++;
				_lastFailureTime = DateTimeOffset.UtcNow;

				if ( _failureCount >= _failureThreshold ) {
					_state = CircuitState.Open;
					_logger?.LogWarning( ex,
						"Circuit breaker opened after {FailureCount} failures",
						_failureCount );
				}
			}

			throw;
		}
	}

	public CircuitState State {
		get {
			lock ( _lock ) {
				return _state;
			}
		}
	}
}