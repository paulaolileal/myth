using Microsoft.Extensions.DependencyInjection;
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

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );

			// Act
			var result = await Pipeline.Start( dto, provider )
				.Step<ITestService>( ( svc, d ) => svc.Process( d ) )
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

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );

			// Act
			var result = await Pipeline.Start( dto, provider )
				.Step<ITestService>(
					( svc, d ) => svc.Process( d ),
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

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );

			// Act
			var result = await Pipeline.Start( dto, provider )
				.Step<ITestService>(
					( svc, d ) => svc.Process( d ),
					onError: ex => capturedException = ex )
				.ExecuteAsync( );

			// Assert
			Assert.False( result.IsSuccess );
			Assert.NotNull( capturedException );
			Assert.IsType<InvalidOperationException>( capturedException );
		}
	}
}