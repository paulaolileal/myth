namespace Myth.Flow.Actions.Settings;

/// <summary>
/// Fluent builder for Flow.Actions configuration
/// </summary>
public sealed class FlowActionsBuilder {
	private readonly FlowActionsConfiguration _configuration = new( );

	/// <summary>
	/// Configures in-memory message broker
	/// </summary>
	public FlowActionsBuilder UseInMemory( ) {
		_configuration.BrokerType = MessageBrokerType.InMemory;
		return this;
	}

	/// <summary>
	/// Configures Kafka message broker
	/// </summary>
	public FlowActionsBuilder UseKafka( Action<KafkaOptions> configure ) {
		_configuration.BrokerType = MessageBrokerType.Kafka;
		_configuration.BrokerConfiguration = options => configure( ( KafkaOptions )options );
		return this;
	}

	/// <summary>
	/// Configures RabbitMQ message broker
	/// </summary>
	public FlowActionsBuilder UseRabbitMQ( Action<RabbitMQOptions> configure ) {
		_configuration.BrokerType = MessageBrokerType.RabbitMQ;
		_configuration.BrokerConfiguration = options => configure( ( RabbitMQOptions )options );
		return this;
	}

	/// <summary>
	/// Enables telemetry
	/// </summary>
	public FlowActionsBuilder EnableTelemetry( bool enabled = true ) {
		_configuration.TelemetryEnabled = enabled;
		return this;
	}

	/// <summary>
	/// Enables caching
	/// </summary>
	public FlowActionsBuilder EnableCaching( Action<CacheConfiguration>? configure = null ) {
		_configuration.CachingEnabled = true;
		_configuration.CacheConfiguration = configure;
		return this;
	}

	/// <summary>
	/// Configures retry policy
	/// </summary>
	public FlowActionsBuilder EnableRetry( Action<RetryConfiguration> configure ) {
		configure( _configuration.RetryConfig );
		return this;
	}

	/// <summary>
	/// Enables dead letter queue
	/// </summary>
	public FlowActionsBuilder EnableDeadLetterQueue( bool enabled = true ) {
		_configuration.DeadLetterQueueEnabled = enabled;
		return this;
	}

	/// <summary>
	/// Scans assemblies for handlers
	/// </summary>
	public FlowActionsBuilder ScanAssemblies( params System.Reflection.Assembly[ ] assemblies ) {
		_configuration.AssembliesToScan.AddRange( assemblies );
		return this;
	}

	/// <summary>
	/// Builds the configuration
	/// </summary>
	internal FlowActionsConfiguration Build( ) => _configuration;
}