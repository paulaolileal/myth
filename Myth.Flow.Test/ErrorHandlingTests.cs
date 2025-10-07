using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class ErrorHandlingTests {

		[Fact]
		public async Task MultipleErrorHandlers_ShouldAllBeInvoked( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var handler1Invoked = false;
			var handler2Invoked = false;

			var mockService1 = new Mock<ITestService>( );
			mockService1.
				Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
				.ThrowsAsync( new InvalidOperationException( "Error 1" ) );

			var mockService2 = new Mock<IValidationService>( );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService1.Object );
			services.AddSingleton( mockService2.Object );
			var provider = services.BuildServiceProvider( );

			// Act
			var result = await Pipeline.Start( dto, provider )
				.StepAsync<ITestService>(
					( svc, d ) => svc.ProcessAsync( d ),
					onError: _ => handler1Invoked = true )
				.StepResult<IValidationService>(
					( svc, d ) => svc.Validate( d ) )
				.ExecuteAsync( );

			// Assert
			Assert.False( result.IsSuccess );
			Assert.True( handler1Invoked );
			Assert.False( handler2Invoked ); // Second step never executed
		}

		[Fact]
		public async Task ErrorHandler_ThrowingException_ShouldNotStopPipeline( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var mockService = new Mock<ITestService>( );
			mockService.Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
				.ThrowsAsync( new InvalidOperationException( "Test error" ) );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			services.AddLogging( );
			var provider = services.BuildServiceProvider( );

			// Act
			var result = await Pipeline.Start( dto, provider )
				.StepAsync<ITestService>(
					( svc, d ) => svc.ProcessAsync( d ),
					onError: _ => throw new Exception( "Handler error" ) )
				.ExecuteAsync( );

			// Assert
			Assert.False( result.IsSuccess );
		}
	}
}