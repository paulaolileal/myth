using Myth.Interfaces;

namespace Myth.Flow.Actions.Test.Models;

public class TestEventHandler : IEventHandler<TestEvent> {
	public int CallCount { get; private set; }
	public TestEvent? LastEvent { get; private set; }

	public Task HandleAsync( TestEvent @event, CancellationToken cancellationToken = default ) {
		CallCount++;
		LastEvent = @event;
		return Task.CompletedTask;
	}
}
