using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.Models;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class IntegrationTests {

		[Fact]
		public async Task ComplexPipeline_ShouldExecuteAllSteps( ) {
			// Arrange
			var dto = new TestDto { Value = 1, Message = "Start" };
			var sideEffectCount = 0;

			var mockValidator = new Mock<IValidationService>( );
			mockValidator.Setup( s => s.Validate( It.IsAny<TestDto>( ) ) )
				.Returns( ( TestDto d ) => Result<TestDto>.Success( d ) );

			var mockService = new Mock<ITestService>( );
			mockService.Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
				.ReturnsAsync( ( TestDto d ) => new TestDto {
					Value = d.Value + 10,
					Message = d.Message + "-Processed"
				} );

			var mockEvent = new Mock<IEventService>( );
			mockEvent.Setup( s => s.PublishAsync( It.IsAny<TestDto>( ) ) )
				.Returns( Task.CompletedTask );

			var services = new ServiceCollection( );
			services.AddSingleton( mockValidator.Object );
			services.AddSingleton( mockService.Object );
			services.AddSingleton( mockEvent.Object );
			var provider = services.BuildServiceProvider( );

			// Act
			var result = await Pipeline.Start( dto, provider )
				.WithTelemetry( "ComplexOperation" )
				.WithRetry( maxAttempts: 2 )
				.StepResult<IValidationService>( ( svc, d ) => svc.Validate( d ) )
				.StepAsync<ITestService>( ( svc, d ) => svc.ProcessAsync( d ) )
				.Tap( _ => sideEffectCount++ )
				.When(
					dto => dto.Value > 5,
					pipeline => pipeline.StepAsync<IEventService>(
						async ( eventService, dto ) => {
							await eventService.PublishAsync( dto );
							return dto;
						} ) )
				.Transform( d => new TestResult {
					Success = true,
					Data = $"{d.Message}:{d.Value}"
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( "Start-Processed:11", result.Value!.Data );
			Assert.Equal( 1, sideEffectCount );
			mockEvent.Verify( s => s.PublishAsync( It.IsAny<TestDto>( ) ), Times.Once );
		}
	}
}