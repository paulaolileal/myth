using Myth.Exceptions;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class ServiceResolutionTests : BaseTestFixture {

		[Fact]
		public async Task Step_WithoutServiceProvider_ShouldThrowException( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };

			// Act & Assert
			await Assert.ThrowsAsync<PipelineConfigurationException>( async ( ) =>
				await Pipeline.Start( dto )
					.Step<ITestService>( ( svc, d ) => svc.Process( d ) )
					.ExecuteAsync( ) );
		}

		[Fact]
		public async Task Step_WithUnregisteredService_ShouldThrowException( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };

			// Act & Assert
			await Assert.ThrowsAsync<PipelineConfigurationException>( async ( ) =>
				await Pipeline.Start( dto )
					.Step<ITestService>( ( svc, d ) => svc.Process( d ) )
					.ExecuteAsync( ) );
		}
	}
}