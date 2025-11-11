using Myth.Interfaces;

namespace Myth.Models;

/// <summary>
/// Circuit breaker implementation
/// </summary>
public class CircuitBreaker : ICircuitBreaker {
	private readonly int _failureThreshold;
	private readonly TimeSpan _timeout;
	private readonly TimeSpan _halfOpenRetryTimeout;
	private readonly object _lock = new( );

	private CircuitBreakerState _state = CircuitBreakerState.Closed;
	private int _failureCount = 0;
	private DateTime _lastFailureTime = DateTime.MinValue;
	private DateTime _nextAttemptTime = DateTime.MinValue;

	/// <summary>
	/// Create a new circuit breaker
	/// </summary>
	/// <param name="failureThreshold">Number of failures before opening</param>
	/// <param name="timeout">Timeout before attempting half-open</param>
	/// <param name="halfOpenRetryTimeout">Timeout for half-open retry attempts</param>
	public CircuitBreaker( int failureThreshold = 5, TimeSpan timeout = default, TimeSpan halfOpenRetryTimeout = default ) {
		_failureThreshold = failureThreshold > 0 ? failureThreshold : 5;
		_timeout = timeout == default ? TimeSpan.FromMinutes( 1 ) : timeout;
		_halfOpenRetryTimeout = halfOpenRetryTimeout == default ? TimeSpan.FromSeconds( 30 ) : halfOpenRetryTimeout;
	}

	/// <summary>
	/// Current state of the circuit breaker
	/// </summary>
	public CircuitBreakerState State {
		get {
			lock ( _lock ) {
				return _state;
			}
		}
	}

	/// <summary>
	/// Execute an operation through the circuit breaker
	/// </summary>
	public async Task<T> ExecuteAsync<T>( Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default ) {
		if ( !CanExecute( ) )
			throw new CircuitBreakerOpenException( $"Circuit breaker is {_state}. Next attempt allowed at: {_nextAttemptTime}" );

		try {
			var result = await operation( cancellationToken );

			RecordSuccess( );

			return result;
		} catch ( Exception ex ) {
			RecordFailure( ex );
			throw;
		}
	}

	/// <summary>
	/// Check if the circuit breaker allows execution
	/// </summary>
	public bool CanExecute( ) {
		lock ( _lock ) {
			switch ( _state ) {
				case CircuitBreakerState.Closed:
					return true;

				case CircuitBreakerState.Open:
					if ( DateTime.UtcNow >= _nextAttemptTime ) {
						_state = CircuitBreakerState.HalfOpen;

						return true;
					}
					return false;

				case CircuitBreakerState.HalfOpen:
					return true;

				default:
					return false;
			}
		}
	}

	/// <summary>
	/// Record a successful execution
	/// </summary>
	public void RecordSuccess( ) {
		lock ( _lock ) {
			_failureCount = 0;
			_lastFailureTime = DateTime.MinValue;
			_state = CircuitBreakerState.Closed;
		}
	}

	/// <summary>
	/// Record a failed execution
	/// </summary>
	public void RecordFailure( Exception exception ) {
		lock ( _lock ) {
			_failureCount++;
			_lastFailureTime = DateTime.UtcNow;

			if ( _state == CircuitBreakerState.HalfOpen ) {
				// In half-open state, any failure immediately opens the circuit
				_state = CircuitBreakerState.Open;

				_nextAttemptTime = DateTime.UtcNow.Add( _timeout );
			} else if ( _failureCount >= _failureThreshold ) {
				// Threshold reached, open the circuit
				_state = CircuitBreakerState.Open;

				_nextAttemptTime = DateTime.UtcNow.Add( _timeout );
			}
		}
	}

	/// <summary>
	/// Reset the circuit breaker to closed state
	/// </summary>
	public void Reset( ) {
		lock ( _lock ) {
			_failureCount = 0;
			_lastFailureTime = DateTime.MinValue;
			_nextAttemptTime = DateTime.MinValue;
			_state = CircuitBreakerState.Closed;
		}
	}
}
