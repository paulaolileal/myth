using Microsoft.Extensions.Logging;
using Myth.Flow.Interfaces;
using System.Collections.Concurrent;

/// <summary>
/// Dead letter queue for failed messages
/// </summary>
public sealed class DeadLetterQueue {
	private readonly ConcurrentQueue<DeadLetterMessage> _queue = new( );
	private readonly ILogger<DeadLetterQueue> _logger;
	private readonly int _maxSize;

	public DeadLetterQueue( ILogger<DeadLetterQueue> logger, int maxSize = 10000 ) {
		_logger = logger;
		_maxSize = maxSize;
	}

	public void Enqueue( object message, Exception exception, EventMetadata? metadata = null ) {
		if ( _queue.Count >= _maxSize ) {
			_logger.LogWarning( "Dead letter queue is full. Dropping oldest message." );
			_queue.TryDequeue( out _ );
		}

		var deadLetter = new DeadLetterMessage {
			Message = message,
			Exception = exception,
			Metadata = metadata,
			EnqueuedAt = DateTimeOffset.UtcNow
		};

		_queue.Enqueue( deadLetter );

		_logger.LogError( exception,
			"Message added to dead letter queue. Type: {MessageType}, Queue size: {QueueSize}",
			message.GetType( ).Name, _queue.Count );
	}

	public bool TryDequeue( out DeadLetterMessage? message ) {
		return _queue.TryDequeue( out message );
	}

	public IEnumerable<DeadLetterMessage> GetAll( ) {
		return _queue.ToArray( );
	}

	public int Count => _queue.Count;

	public void Clear( ) {
		_queue.Clear( );
		_logger.LogInformation( "Dead letter queue cleared" );
	}
}