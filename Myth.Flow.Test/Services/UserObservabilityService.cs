using Microsoft.Extensions.Logging;
using Myth.Flow.Test.Contexts;
using System.Threading.Tasks;

public class UserObservabilityService {
	private readonly ILogger<UserObservabilityService> _logger;
	private readonly IUserMetrics _metrics;

	public UserObservabilityService(
		ILogger<UserObservabilityService> logger,
		IUserMetrics metrics ) {
		_logger = logger;
		_metrics = metrics;
	}

	public Task ObserveUserCreationAsync( CreateUserContext context ) {
		if ( context.CreatedUser != null ) {
			_logger.LogInformation(
				"User {Email} created with success at {CreatedAt}",
				context.CreatedUser.Email,
				context.CreatedUser.CreatedAt );

			_metrics.IncrementUserCreated( );
		}

		return Task.CompletedTask;
	}
}