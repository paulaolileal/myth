using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.Models;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class IntegrationTests : BaseTestFixture {
		private readonly Mock<IValidationService> _mockValidator = new Mock<IValidationService>( );
		private readonly Mock<ITestService> _mockService = new Mock<ITestService>( );
		private readonly Mock<IEventService> _mockEvent = new Mock<IEventService>( );

		public IntegrationTests( ) {
			_mockValidator.Setup( s => s.Validate( It.IsAny<TestDto>( ) ) )
				.Returns( ( TestDto d ) => Result<TestDto>.Success( d ) );

			_mockService.Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
				.ReturnsAsync( ( TestDto d ) => new TestDto {
					Value = d.Value + 10,
					Message = d.Message + "-Processed"
				} );

			_mockEvent.Setup( s => s.PublishAsync( It.IsAny<TestDto>( ) ) )
				.Returns( Task.CompletedTask );
		}

		protected override void ConfigureServices( IServiceCollection services ) {
			services.AddSingleton( _mockValidator.Object );
			services.AddSingleton( _mockService.Object );
			services.AddSingleton( _mockEvent.Object );
		}

		[Fact]
		public async Task ComplexPipeline_ShouldExecuteAllSteps( ) {
			// Arrange
			var dto = new TestDto { Value = 1, Message = "Start" };
			var sideEffectCount = 0;

			// Act
			var result = await Pipeline.Start( dto )
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
			_mockEvent.Verify( s => s.PublishAsync( It.IsAny<TestDto>( ) ), Times.Once );
		}
	}
}