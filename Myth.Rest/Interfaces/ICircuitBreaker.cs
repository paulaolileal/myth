namespace Myth.Interfaces {

	/// Circuit breaker states
	/// </summary>
	public enum CircuitBreakerState {
		Closed,
		Open,
		HalfOpen
	}

	/// <summary>
	/// Interface for circuit breaker pattern implementation
	/// </summary>
	public interface ICircuitBreaker {

		/// <summary>
		/// Current state of the circuit breaker
		/// </summary>
		CircuitBreakerState State { get; }

		/// <summary>
		/// Execute an operation through the circuit breaker
		/// </summary>
		/// <typeparam name="T">Return type</typeparam>
		/// <param name="operation">The operation to execute</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Result of the operation</returns>
		Task<T> ExecuteAsync<T>( Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default );

		/// <summary>
		/// Check if the circuit breaker allows execution
		/// </summary>
		/// <returns>True if execution is allowed</returns>
		bool CanExecute( );

		/// <summary>
		/// Record a successful execution
		/// </summary>
		void RecordSuccess( );

		/// <summary>
		/// Record a failed execution
		/// </summary>
		/// <param name="exception">The exception that occurred</param>
		void RecordFailure( Exception exception );

		/// <summary>
		/// Reset the circuit breaker to closed state
		/// </summary>
		void Reset( );
	}

	/// <summary>
	/// Exception thrown when circuit breaker is open
	/// </summary>
	public class CircuitBreakerOpenException : Exception {

		public CircuitBreakerOpenException( string message ) : base( message ) {
		}

		public CircuitBreakerOpenException( string message, Exception innerException ) : base( message, innerException ) {
		}
	}
}