using System.Collections.Generic;
using System.Threading.Tasks;
using Myth.Flow.Test.Interfaces;

public class MockEventPublisher : IEventPublisher {
	public List<object> PublishedEvents { get; } = new( );

	public Task PublishAsync<TEvent>( TEvent @event ) {
		PublishedEvents.Add( @event! );
		return Task.CompletedTask;
	}
}
