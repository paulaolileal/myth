/// <summary>
/// Configuration for Flow.Actions
/// </summary>
public sealed class FlowActionsConfiguration {
	internal MessageBrokerType BrokerType { get; set; } = MessageBrokerType.InMemory;
	internal Action<object>? BrokerConfiguration { get; set; }
	internal bool TelemetryEnabled { get; set; } = true;
	internal bool CachingEnabled { get; set; }
	internal Action<CacheConfiguration>? CacheConfiguration { get; set; }
	internal RetryConfiguration RetryConfig { get; set; } = new( );
	internal bool DeadLetterQueueEnabled { get; set; }
	internal List<System.Reflection.Assembly> AssembliesToScan { get; set; } = new( );
}