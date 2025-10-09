namespace Myth.Flow.Resilience;

/// <summary>
/// Circuit breaker state
/// </summary>
public enum CircuitState {
	Closed,
	Open,
	HalfOpen
}