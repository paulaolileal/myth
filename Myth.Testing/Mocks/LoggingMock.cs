using Microsoft.Extensions.Logging;
using Moq;

namespace Myth.Testing.Mocks;

internal class LoggingMock {
	private static readonly Mock<ILogger> _logger = new( );

	public static ILogger Logger => _logger.Object;
}
