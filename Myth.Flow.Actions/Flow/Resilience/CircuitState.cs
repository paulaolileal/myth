namespace Myth.Flow.Resilience;

/// <summary>
/// Circuit breaker state
/// </summary>
public enum CircuitState {

	/// <summary>
	/// Circuit is closed and operations are allowed
	/// </summary>
	Closed,

	/// <summary>
	/// Circuit is open and operations are blocked due to failures
	/// </summary>
	Open,

	/// <summary>
	/// Circuit is testing if the protected resource has recovered
	/// </summary>
	HalfOpen
}
