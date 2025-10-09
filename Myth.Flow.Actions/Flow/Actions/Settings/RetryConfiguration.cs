namespace Myth.Flow.Actions.Settings;

/// <summary>
/// Retry configuration
/// </summary>
public sealed class RetryConfiguration {
	public int MaxAttempts { get; set; } = 3;
	public int BackoffMs { get; set; } = 1000;
	public bool ExponentialBackoff { get; set; } = true;
}