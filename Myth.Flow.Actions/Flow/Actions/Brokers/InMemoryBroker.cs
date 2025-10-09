using Microsoft.Extensions.Logging;
using Myth.Interfaces;
using System.Threading.Channels;

namespace Myth.Flow.Actions.Brokers;

/// <summary>
/// In-memory message broker for testing and development
/// </summary>
internal sealed class InMemoryBroker : IMessageBroker, IDisposable {
	private readonly Channel<IEvent> _channel;
	private readonly ILogger<InMemoryBroker> _logger;
	private readonly CancellationTokenSource _cts = new( );
	private Task? _processingTask;

	public InMemoryBroker( ILogger<InMemoryBroker> logger, int channelCapacity = 1000 ) {
		_logger = logger;
		_channel = Channel.CreateBounded<IEvent>( new BoundedChannelOptions( channelCapacity ) {
			FullMode = BoundedChannelFullMode.Wait
		} );
	}

	public async Task PublishAsync<TEvent>( TEvent @event, CancellationToken cancellationToken = default )
		where TEvent : IEvent {
		try {
			await _channel.Writer.WriteAsync( @event, cancellationToken );
			_logger.LogDebug( "Published event {EventType} to in-memory channel", typeof( TEvent ).Name );
		} catch ( Exception ex ) {
			_logger.LogError( ex, "Error publishing event {EventType} to in-memory channel", typeof( TEvent ).Name );
			throw;
		}
	}

	public Task StartAsync( CancellationToken cancellationToken = default ) {
		_logger.LogInformation( "Starting in-memory message broker" );
		_processingTask = ProcessMessagesAsync( _cts.Token );
		return Task.CompletedTask;
	}

	public async Task StopAsync( CancellationToken cancellationToken = default ) {
		_logger.LogInformation( "Stopping in-memory message broker" );
		_channel.Writer.Complete( );
		_cts.Cancel( );

		if ( _processingTask != null )
			await _processingTask;
	}

	private async Task ProcessMessagesAsync( CancellationToken cancellationToken ) {
		await foreach ( var @event in _channel.Reader.ReadAllAsync( cancellationToken ) ) {
			_logger.LogDebug( "Processing event {EventType} from in-memory channel", @event.GetType( ).Name );
		}
	}

	public void Dispose( ) {
		_cts.Dispose( );
	}
}