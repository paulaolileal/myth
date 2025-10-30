using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class SynchronousStepTests {

		[Fact]
		public async Task Step_ShouldExecuteSuccessfully( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var mockService = new Mock<ITestService>( );
			mockService.Setup( s => s.Process( It.IsAny<TestDto>( ) ) )
				.Returns( ( TestDto d ) => new TestDto { Value = d.Value + 1 } );

			var service = mockService.Object;

			// Act - Using direct service reference instead of service locator
			var result = await Pipeline.Start( dto )
				.Step( d => service.Process( d ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( 2, result.Value!.Value );
			mockService.Verify( s => s.Process( It.IsAny<TestDto>( ) ), Times.Once );
		}

		[Fact]
		public async Task Step_WithOnSuccess_ShouldInvokeCallback( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var callbackInvoked = false;
			var mockService = new Mock<ITestService>( );
			mockService.Setup( s => s.Process( It.IsAny<TestDto>( ) ) )
				.Returns( ( TestDto d ) => new TestDto { Value = d.Value + 1 } );

			var service = mockService.Object;

			// Act - Using direct service reference instead of service locator
			var result = await Pipeline.Start( dto )
				.Step(
					d => service.Process( d ),
					onSuccess: _ => callbackInvoked = true )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.True( callbackInvoked );
		}

		[Fact]
		public async Task Step_WithOnError_ShouldInvokeCallbackOnFailure( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			Exception? capturedException = null;
			var mockService = new Mock<ITestService>( );
			mockService.Setup( s => s.Process( It.IsAny<TestDto>( ) ) )
				.Throws( new InvalidOperationException( "Test error" ) );

			var service = mockService.Object;

			// Act - Using direct service reference instead of service locator
			var result = await Pipeline.Start( dto )
				.Step(
					d => service.Process( d ),
					onError: ex => capturedException = ex )
				.ExecuteAsync( );

			// Assert
			Assert.False( result.IsSuccess );
			Assert.NotNull( capturedException );
			Assert.IsType<InvalidOperationException>( capturedException );
		}
	}
}