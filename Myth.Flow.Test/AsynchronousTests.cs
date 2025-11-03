using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.ServiceProvider;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class AsynchronousStepTests {

		[Fact]
		public async Task StepAsync_ShouldExecuteSuccessfully( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var mockService = new Mock<ITestService>( );
			mockService
				.Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
				.ReturnsAsync( ( TestDto d ) => new TestDto { Value = d.Value + 1 } );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			var service = mockService.Object;

			// Act
			var result = await Pipeline.Start( dto )
				.StepAsync( d => service.ProcessAsync( d ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( 2, result.Value!.Value );
		}

		[Fact]
		public async Task StepAsync_WithOnSuccess_ShouldInvokeCallback( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var callbackInvoked = false;
			var mockService = new Mock<ITestService>( );
			mockService
				.Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
				.ReturnsAsync( ( TestDto d ) => new TestDto { Value = d.Value + 1 } );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			var service = mockService.Object;

			// Act
			var result = await Pipeline.Start( dto )
				.StepAsync(
					d => service.ProcessAsync( d ),
					onSuccess: _ => callbackInvoked = true )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.True( callbackInvoked );
		}

		[Fact]
		public async Task StepAsync_WithError_ShouldReturnFailure( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var mockService = new Mock<ITestService>( );
			mockService.Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
				.ThrowsAsync( new InvalidOperationException( "Async error" ) );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			var service = mockService.Object;

			// Act
			var result = await Pipeline.Start( dto )
				.StepAsync( d => service.ProcessAsync( d ) )
				.ExecuteAsync( );

			// Assert
			Assert.False( result.IsSuccess );
			Assert.Contains( "Async error", result.ErrorMessage );
		}
	}
}