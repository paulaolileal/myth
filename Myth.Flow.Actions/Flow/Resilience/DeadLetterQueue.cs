using Microsoft.Extensions.Logging;
using Myth.Models;
using System.Collections.Concurrent;

namespace Myth.Flow.Resilience;

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

	/// <summary>
	/// Adds a failed message to the dead letter queue
	/// </summary>
	/// <param name="message">The message that failed to process</param>
	/// <param name="exception">The exception that caused the failure</param>
	/// <param name="metadata">Optional metadata about the event</param>
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

	/// <summary>
	/// Attempts to remove and return a message from the dead letter queue
	/// </summary>
	/// <param name="message">The dequeued message, or null if the queue is empty</param>
	/// <returns>True if a message was dequeued successfully, otherwise false</returns>
	public bool TryDequeue( out DeadLetterMessage? message ) {
		return _queue.TryDequeue( out message );
	}

	/// <summary>
	/// Gets all messages currently in the dead letter queue
	/// </summary>
	/// <returns>A snapshot of all messages in the queue</returns>
	public IEnumerable<DeadLetterMessage> GetAll( ) {
		return _queue.ToArray( );
	}

	/// <summary>
	/// Gets the current number of messages in the dead letter queue
	/// </summary>
	public int Count => _queue.Count;

	/// <summary>
	/// Removes all messages from the dead letter queue
	/// </summary>
	public void Clear( ) {
		_queue.Clear( );
		_logger.LogInformation( "Dead letter queue cleared" );
	}
}