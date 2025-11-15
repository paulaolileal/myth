using System.Threading.Tasks;

namespace Myth.Flow.Test.Interfaces;

public interface IEventPublisher {

	Task PublishAsync<TEvent>( TEvent @event );
}
