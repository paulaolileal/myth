using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Testing.Mocks {
	internal class LoggingMock {
		private static readonly Mock<ILogger> _logger = new( );

		public static ILogger Logger => _logger.Object;
	}
}
