using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Myth.Interfaces;

namespace Myth.Flow.Actions.Hosting;

/// <summary>
/// Hosted service that manages message broker lifecycle
/// </summary>
internal sealed class MessageBrokerHostedService(
	IMessageBroker messageBroker,
	ILogger<MessageBrokerHostedService> logger ) : IHostedService {
	private readonly IMessageBroker _messageBroker = messageBroker;
	private readonly ILogger<MessageBrokerHostedService> _logger = logger;

	public async Task StartAsync( CancellationToken cancellationToken ) {
		_logger.LogInformation( "Starting message broker hosted service" );
		await _messageBroker.StartAsync( cancellationToken );
	}

	public async Task StopAsync( CancellationToken cancellationToken ) {
		_logger.LogInformation( "Stopping message broker hosted service" );
		await _messageBroker.StopAsync( cancellationToken );
	}
}
