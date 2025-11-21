using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Myth.Flow.Test.Contexts;

public class UserObservabilityService(
	ILogger<UserObservabilityService> logger,
	IUserMetrics metrics ) {
	private readonly ILogger<UserObservabilityService> _logger = logger;
	private readonly IUserMetrics _metrics = metrics;

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
