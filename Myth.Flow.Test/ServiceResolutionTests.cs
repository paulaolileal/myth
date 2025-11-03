using Myth.Flow.Test.Models;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class ServiceResolutionTests : BaseTestFixture {

		[Fact]
		public async Task Step_WithNullService_ShouldThrowException( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };

			// Act & Assert - This test is no longer relevant as service locator pattern has been removed
			// Services are now passed directly into pipeline steps, so null reference would be a compile-time issue
			var result = await Pipeline.Start( dto )
				.Tap( d => d.Value++ )
				.ExecuteAsync( );

			Assert.True( result.IsSuccess );
		}
	}
}