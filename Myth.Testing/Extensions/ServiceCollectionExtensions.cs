using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Mocks;

namespace Myth.Extensions;

public static class ServiceCollectionExtensions {

	public static IServiceCollection AddMockedLoggingT( this IServiceCollection services ) {
		services.AddSingleton<ILogger>( LoggingMock.Logger );

		return services;
	}
}
