namespace Myth.Flow.Actions.Settings;

/// <summary>
/// Configuration for Flow.Actions specific features (CQRS, Events, Message Brokers, Caching)
/// </summary>
public sealed class FlowActionsConfiguration {

	// Message Broker Configuration
	internal MessageBrokerType BrokerType { get; set; } = MessageBrokerType.InMemory;

	internal Func<object>? BrokerConfigurationFactory { get; set; }

	// Caching Configuration
	internal bool CachingEnabled { get; set; }

	internal Action<CacheConfiguration>? CacheConfiguration { get; set; }

	// Dead Letter Queue Configuration
	internal bool DeadLetterQueueEnabled { get; set; }

	// Handler Discovery Configuration
	internal bool AutoSubscribeEventHandlers { get; set; } = true;

	internal List<System.Reflection.Assembly> AssembliesToScan { get; set; } = new( );
}