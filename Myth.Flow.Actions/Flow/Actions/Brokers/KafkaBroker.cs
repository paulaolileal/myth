using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Myth.Flow.Actions.Settings;
using Myth.Interfaces;
using System.Text.Json;

namespace Myth.Flow.Actions.Brokers;

/// <summary>
/// Kafka message broker implementation
/// </summary>
internal sealed class KafkaBroker : IMessageBroker, IDisposable {
	private readonly KafkaOptions _options;
	private readonly ILogger<KafkaBroker> _logger;
	private readonly IProducer<string, string> _producer;
	private IConsumer<string, string>? _consumer;
	private CancellationTokenSource? _cts;
	private Task? _consumerTask;

	public KafkaBroker( KafkaOptions options, ILogger<KafkaBroker> logger ) {
		_options = options;
		_logger = logger;

		var producerConfig = new ProducerConfig {
			BootstrapServers = options.BootstrapServers,
			ClientId = options.ClientId ?? $"flow-actions-producer-{Guid.NewGuid( )}",
			LingerMs = options.ProducerLingerMs,
			BatchSize = options.ProducerBatchSize,
			CompressionType = Enum.Parse<CompressionType>( options.CompressionType, true ),
			EnableIdempotence = true,
			MaxInFlight = 5,
			Acks = Acks.All
		};

		foreach ( var kvp in options.AdditionalConfig )
			producerConfig.Set( kvp.Key, kvp.Value );

		_producer = new ProducerBuilder<string, string>( producerConfig ).Build( );
	}

	public async Task PublishAsync<TEvent>( TEvent @event, CancellationToken cancellationToken = default )
		where TEvent : IEvent {
		try {
			var topic = GetTopicName<TEvent>( );
			var key = @event.EventId;
			var value = JsonSerializer.Serialize( @event );

			var message = new Message<string, string> {
				Key = key,
				Value = value,
				Headers = new Headers
				{
				{ "event-type", System.Text.Encoding.UTF8.GetBytes(typeof(TEvent).FullName ?? typeof(TEvent).Name) },
				{ "event-id", System.Text.Encoding.UTF8.GetBytes(@event.EventId) },
				{ "occurred-at", System.Text.Encoding.UTF8.GetBytes(@event.OccurredAt.ToString("O")) }
			}
			};

			var result = await _producer.ProduceAsync( topic, message, cancellationToken );

			_logger.LogInformation( "Published event {EventType} to Kafka topic {Topic} at offset {Offset}",
				typeof( TEvent ).Name, topic, result.Offset );
		} catch ( ProduceException<string, string> ex ) {
			_logger.LogError( ex, "Error publishing event {EventType} to Kafka", typeof( TEvent ).Name );
			throw new MessageBrokerException( $"Failed to publish event to Kafka: {ex.Error.Reason}", ex );
		}
	}

	public Task StartAsync( CancellationToken cancellationToken = default ) {
		_logger.LogInformation( "Starting Kafka message broker consumer" );

		var consumerConfig = new ConsumerConfig {
			BootstrapServers = _options.BootstrapServers,
			GroupId = _options.GroupId,
			ClientId = _options.ClientId ?? $"flow-actions-consumer-{Guid.NewGuid( )}",
			EnableAutoCommit = _options.EnableAutoCommit,
			SessionTimeoutMs = _options.SessionTimeoutMs,
			MaxPollIntervalMs = _options.MaxPollIntervalMs,
			AutoOffsetReset = Enum.Parse<AutoOffsetReset>( _options.AutoOffsetReset, true ),
			EnablePartitionEof = false
		};

		foreach ( var kvp in _options.AdditionalConfig )
			consumerConfig.Set( kvp.Key, kvp.Value );

		_consumer = new ConsumerBuilder<string, string>( consumerConfig ).Build( );
		_cts = new CancellationTokenSource( );
		_consumerTask = ConsumeMessagesAsync( _cts.Token );

		return Task.CompletedTask;
	}

	public async Task StopAsync( CancellationToken cancellationToken = default ) {
		_logger.LogInformation( "Stopping Kafka message broker consumer" );

		_cts?.Cancel( );

		if ( _consumerTask != null )
			await _consumerTask;

		_consumer?.Close( );
	}

	private async Task ConsumeMessagesAsync( CancellationToken cancellationToken ) {
		if ( _consumer == null )
			return;

		try {
			while ( !cancellationToken.IsCancellationRequested ) {
				try {
					var consumeResult = _consumer.Consume( cancellationToken );

					if ( consumeResult?.Message != null ) {
						_logger.LogDebug( "Consumed message from topic {Topic} at offset {Offset}",
							consumeResult.Topic, consumeResult.Offset );

						if ( !_options.EnableAutoCommit )
							_consumer.Commit( consumeResult );
					}
				} catch ( ConsumeException ex ) {
					_logger.LogError( ex, "Error consuming message from Kafka" );
				}
			}
		} catch ( OperationCanceledException ) {
			_logger.LogInformation( "Kafka consumer cancelled" );
		} finally {
			await Task.CompletedTask;
		}
	}

	private static string GetTopicName<TEvent>( ) where TEvent : IEvent {
		var eventType = typeof( TEvent );
		var topicName = eventType.Name.ToLowerInvariant( ).Replace( "event", "" );
		return $"flow.actions.{topicName}";
	}

	public void Dispose( ) {
		_producer?.Dispose( );
		_consumer?.Dispose( );
		_cts?.Dispose( );
	}
}