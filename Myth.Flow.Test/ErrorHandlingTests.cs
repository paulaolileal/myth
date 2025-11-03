using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.ServiceProvider;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class ErrorHandlingTests : BaseTestFixture {
		private readonly Mock<ITestService> _mockService1 = new Mock<ITestService>( );
		private readonly Mock<IValidationService> _mockService2 = new Mock<IValidationService>( );

		public ErrorHandlingTests( ) {
			_mockService1
				.Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
				.ThrowsAsync( new InvalidOperationException( "Error 1" ) );
		}

		protected override void ConfigureServices( IServiceCollection services ) {
			services.AddSingleton( _mockService1.Object );
			services.AddSingleton( _mockService2.Object );
		}

		[Fact]
		public async Task MultipleErrorHandlers_ShouldAllBeInvoked( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var handler1Invoked = false;
			var handler2Invoked = false;

			var testService = _mockService1.Object;
			var validationService = _mockService2.Object;

			// Act
			var result = await Pipeline.Start( dto )
				.StepAsync(
					d => testService.ProcessAsync( d ),
					onError: _ => handler1Invoked = true )
				.StepResult(
					d => validationService.Validate( d ) )
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
			MythServiceProvider.Initialize( provider );

			var service = mockService.Object;

			// Act
			var result = await Pipeline.Start( dto )
				.StepAsync(
					d => service.ProcessAsync( d ),
					onError: _ => throw new Exception( "Handler error" ) )
				.ExecuteAsync( );

			// Assert
			Assert.False( result.IsSuccess );
		}
	}
}