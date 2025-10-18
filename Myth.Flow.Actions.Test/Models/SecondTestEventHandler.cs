using Myth.Interfaces;

namespace Myth.Flow.Actions.Test.Models {

	public class SecondTestEventHandler : IEventHandler<TestEvent> {
		public int CallCount { get; private set; }

		public Task HandleAsync( TestEvent @event, CancellationToken cancellationToken = default ) {
			CallCount++;
			return Task.CompletedTask;
		}
	}
}