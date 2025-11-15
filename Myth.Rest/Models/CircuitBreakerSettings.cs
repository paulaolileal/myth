namespace Myth.Models;

public class CircuitBreakerSettings {
	internal int FailureThreshold { get; private set; } = 5;
	internal TimeSpan Timeout { get; private set; } = TimeSpan.FromMinutes( 1 );
	internal TimeSpan HalfOpenRetryTimeout { get; private set; } = TimeSpan.FromSeconds( 30 );

	/// <summary>
	/// Set the failure threshold. Default is 5
	/// </summary>
	/// <param name="failureThreshold"></param>
	/// <returns>The instance of settings</returns>
	public CircuitBreakerSettings UseFailureThreshold( int failureThreshold ) {
		FailureThreshold = failureThreshold > 0 ? failureThreshold : 5;
		return this;
	}

	/// <summary>
	/// Set the timeout. Default is 1 minute
	/// </summary>
	/// <param name="timeout">Value for timeout</param>
	/// <returns>The instance of settings</returns>
	public CircuitBreakerSettings UseTimeout( TimeSpan timeout ) {
		Timeout = timeout == default ? TimeSpan.FromMinutes( 1 ) : timeout;
		return this;
	}

	/// <summary>
	/// Set the half-open retry timeout. Default is 30 seconds
	/// </summary>
	/// <param name="halfOpenRetryTimeout">Value for half-open timeout</param>
	/// <returns>The instance</returns>
	public CircuitBreakerSettings UseHalfOpenRetryTimeout( TimeSpan halfOpenRetryTimeout ) {
		HalfOpenRetryTimeout = halfOpenRetryTimeout == default ? TimeSpan.FromSeconds( 30 ) : halfOpenRetryTimeout;
		return this;
	}
}
