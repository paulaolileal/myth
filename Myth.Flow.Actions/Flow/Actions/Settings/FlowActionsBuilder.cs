namespace Myth.Flow.Actions.Settings;

/// <summary>
/// Fluent builder for Flow.Actions specific configuration.
/// Telemetry and retry policies are inherited from Flow configuration and cannot be overridden.
/// </summary>
public sealed class FlowActionsBuilder {
	private readonly FlowActionsConfiguration _configuration = new( );

	/// <summary>
	/// Configures in-memory message broker
	/// </summary>
	public FlowActionsBuilder UseInMemory( Action<InMemoryBrokerOptions>? configure = null ) {
		_configuration.BrokerType = MessageBrokerType.InMemory;

		if ( configure is not null )
			_configuration.BrokerConfigurationFactory = ( ) => {
				var options = new InMemoryBrokerOptions( );
				configure?.Invoke( options );
				return options;
			};

		return this;
	}

	/// <summary>
	/// Configures Kafka message broker
	/// </summary>
	public FlowActionsBuilder UseKafka( Action<KafkaOptions> configure ) {
		_configuration.BrokerType = MessageBrokerType.Kafka;
		_configuration.BrokerConfigurationFactory = ( ) => {
			var options = new KafkaOptions {
				BootstrapServers = "localhost:9092",
				GroupId = "flow-actions"
			};
			configure( options );

			// Validar propriedades obrigatórias
			if ( string.IsNullOrWhiteSpace( options.BootstrapServers ) )
				throw new ArgumentException( "BootstrapServers is required for Kafka configuration" );

			if ( string.IsNullOrWhiteSpace( options.GroupId ) )
				throw new ArgumentException( "GroupId is required for Kafka configuration" );

			return options;
		};
		return this;
	}

	/// <summary>
	/// Configures RabbitMQ message broker
	/// </summary>
	public FlowActionsBuilder UseRabbitMQ( Action<RabbitMQOptions> configure ) {
		_configuration.BrokerType = MessageBrokerType.RabbitMQ;
		_configuration.BrokerConfigurationFactory = ( ) => {
			var options = new RabbitMQOptions {
				HostName = "localhost",
				UserName = "guest",
				Password = "guest"
			};
			configure( options );

			// Validar propriedades obrigatórias
			if ( string.IsNullOrWhiteSpace( options.HostName ) )
				throw new ArgumentException( "HostName is required for RabbitMQ configuration" );

			if ( string.IsNullOrWhiteSpace( options.UserName ) )
				throw new ArgumentException( "UserName is required for RabbitMQ configuration" );

			if ( string.IsNullOrWhiteSpace( options.Password ) )
				throw new ArgumentException( "Password is required for RabbitMQ configuration" );

			return options;
		};

		return this;
	}

	/// <summary>
	/// Configures caching for query results
	/// </summary>
	/// <param name="configure">Optional configuration action for cache settings</param>
	public FlowActionsBuilder UseCaching( Action<CacheConfiguration>? configure = null ) {
		_configuration.CachingEnabled = true;
		_configuration.CacheConfiguration = configure;
		return this;
	}

	/// <summary>
	/// Configures dead letter queue for failed message handling
	/// </summary>
	/// <param name="enabled">Whether to enable dead letter queue</param>
	public FlowActionsBuilder UseDeadLetterQueue( bool enabled = true ) {
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
	/// Configures automatic subscription of event handlers to the EventBus.
	/// When enabled (default), all discovered event handlers are automatically subscribed to handle their respective events.
	/// When disabled, handlers must be manually subscribed using EventBus.Subscribe&lt;TEvent, THandler&gt;().
	/// </summary>
	/// <param name="enabled">True to automatically subscribe handlers (default), false to require manual subscription</param>
	/// <returns>The builder instance for method chaining</returns>
	public FlowActionsBuilder AutoSubscribeEventHandlers( bool enabled = true ) {
		_configuration.AutoSubscribeEventHandlers = enabled;
		return this;
	}

	/// <summary>
	/// Builds the configuration
	/// </summary>
	internal FlowActionsConfiguration Build( ) => _configuration;
}
