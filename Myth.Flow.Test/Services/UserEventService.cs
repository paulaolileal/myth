using System.Threading.Tasks;
using Myth.Flow.Test.Contexts;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;

public class UserEventService {
	private readonly IEventPublisher _eventPublisher;

	public UserEventService( IEventPublisher eventPublisher ) {
		_eventPublisher = eventPublisher;
	}

	public async Task<CreateUserContext> PublishUserCreatedAsync( CreateUserContext context ) {
		if ( context.CreatedUser != null ) {
			var @event = new UserCreatedEvent(
				context.CreatedUser.Id,
				context.CreatedUser.Email,
				context.CreatedUser.Role,
				context.CreatedUser.CreatedAt
			);

			await _eventPublisher.PublishAsync( @event );
		}

		return context;
	}
}
