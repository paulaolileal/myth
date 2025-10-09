using Microsoft.Extensions.Logging;
using Myth.Flow.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

/// <summary>
/// RabbitMQ message broker implementation
/// </summary>
internal sealed class RabbitMQBroker : IMessageBroker, IDisposable {
	private readonly RabbitMQOptions _options;
	private readonly ILogger<RabbitMQBroker> _logger;
	private IConnection? _connection;
	private IChannel? _channel;
	private AsyncEventingBasicConsumer? _consumer;

	public RabbitMQBroker( RabbitMQOptions options, ILogger<RabbitMQBroker> logger ) {
		_options = options;
		_logger = logger;
		InitializeConnectionAsync( ).GetAwaiter( ).GetResult( );
	}

	private async Task InitializeConnectionAsync( ) {
		var factory = new ConnectionFactory {
			HostName = _options.HostName,
			Port = _options.Port,
			UserName = _options.UserName,
			Password = _options.Password,
			VirtualHost = _options.VirtualHost,
			RequestedConnectionTimeout = TimeSpan.FromMilliseconds( _options.ConnectionTimeout ),
			AutomaticRecoveryEnabled = true,
			NetworkRecoveryInterval = TimeSpan.FromSeconds( 10 )
		};

		_connection = await factory.CreateConnectionAsync( );
		_channel = await _connection.CreateChannelAsync( );

		await _channel.ExchangeDeclareAsync(
			exchange: _options.ExchangeName,
			type: _options.ExchangeType,
			durable: _options.ExchangeDurable );

		_logger.LogInformation( "RabbitMQ connection established to {HostName}", _options.HostName );
	}

	public async Task PublishAsync<TEvent>( TEvent @event, CancellationToken cancellationToken = default )
		where TEvent : IEvent {
		try {
			if ( _channel == null )
				throw new MessageBrokerException( "RabbitMQ channel not initialized" );

			var routingKey = GetRoutingKey<TEvent>( );
			var body = Encoding.UTF8.GetBytes( JsonSerializer.Serialize( @event ) );

			var properties = new BasicProperties( );
			properties.Persistent = true;
			properties.ContentType = "application/json";
			properties.MessageId = @event.EventId;
			properties.Timestamp = new AmqpTimestamp( DateTimeOffset.UtcNow.ToUnixTimeSeconds( ) );
			properties.Headers = new Dictionary<string, object?>
			{
				{ "event-type", typeof(TEvent).FullName ?? typeof(TEvent).Name },
				{ "occurred-at", @event.OccurredAt.ToString("O") }
			};

			await _channel.BasicPublishAsync(
				exchange: _options.ExchangeName,
				routingKey: routingKey,
				mandatory: false,
				basicProperties: properties,
				body: body,
				cancellationToken: cancellationToken );

			_logger.LogInformation( "Published event {EventType} to RabbitMQ exchange {Exchange} with routing key {RoutingKey}",
				typeof( TEvent ).Name, _options.ExchangeName, routingKey );
		} catch ( Exception ex ) {
			_logger.LogError( ex, "Error publishing event {EventType} to RabbitMQ", typeof( TEvent ).Name );
			throw new MessageBrokerException( $"Failed to publish event to RabbitMQ: {ex.Message}", ex );
		}
	}

	public async Task StartAsync( CancellationToken cancellationToken = default ) {
		if ( _channel == null )
			throw new MessageBrokerException( "RabbitMQ channel not initialized" );

		_logger.LogInformation( "Starting RabbitMQ message broker consumer" );

		var queueName = ( await _channel.QueueDeclareAsync(
			queue: "",
			durable: _options.QueueDurable,
			exclusive: false,
			autoDelete: _options.QueueAutoDelete ) ).QueueName;

		await _channel.QueueBindAsync( queueName, _options.ExchangeName, "flow.actions.#" );
		await _channel.BasicQosAsync( 0, _options.PrefetchCount, false );

		_consumer = new AsyncEventingBasicConsumer( _channel );
		_consumer.ReceivedAsync += OnMessageReceived;

		await _channel.BasicConsumeAsync(
			queue: queueName,
			autoAck: false,
			consumer: _consumer );
	}

	public Task StopAsync( CancellationToken cancellationToken = default ) {
		_logger.LogInformation( "Stopping RabbitMQ message broker consumer" );
		_channel?.CloseAsync( );
		_connection?.CloseAsync( );
		return Task.CompletedTask;
	}

	private Task OnMessageReceived( object? sender, BasicDeliverEventArgs args ) {
		try {
			var body = Encoding.UTF8.GetString( args.Body.ToArray( ) );
			_logger.LogDebug( "Received message from RabbitMQ with routing key {RoutingKey}", args.RoutingKey );

			_channel?.BasicAckAsync( args.DeliveryTag, false );
		} catch ( Exception ex ) {
			_logger.LogError( ex, "Error processing message from RabbitMQ" );
			_channel?.BasicNackAsync( args.DeliveryTag, false, true );
		}

		return Task.CompletedTask;
	}

	private static string GetRoutingKey<TEvent>( ) where TEvent : IEvent {
		var eventType = typeof( TEvent );
		var routingKey = eventType.Name.ToLowerInvariant( ).Replace( "event", "" );
		return $"flow.actions.{routingKey}";
	}

	public void Dispose( ) {
		_channel?.Dispose( );
		_connection?.Dispose( );
	}
}