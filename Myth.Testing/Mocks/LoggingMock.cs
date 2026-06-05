using Microsoft.Extensions.Logging;
using Moq;

namespace Myth.Mocks;

internal class LoggingMock {
	private static readonly Mock<ILogger> _logger = new( );

	public static ILogger Logger => _logger.Object;
}
