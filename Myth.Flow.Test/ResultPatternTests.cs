using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.Models;
using Myth.ServiceProvider;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class ResultPatternStepTests {

		[Fact]
		public async Task StepResult_WithSuccess_ShouldContinuePipeline( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var mockService = new Mock<IValidationService>( );
			mockService.Setup( s => s.Validate( It.IsAny<TestDto>( ) ) )
				.Returns( ( TestDto d ) => Result<TestDto>.Success(
					new TestDto { Value = d.Value + 1 } ) );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			var service = mockService.Object;

			// Act
			var result = await Pipeline.Start( dto )
				.StepResult( d => service.Validate( d ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( 2, result.Value!.Value );
		}

		[Fact]
		public async Task StepResult_WithFailure_ShouldStopPipeline( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var mockService = new Mock<IValidationService>( );
			mockService.Setup( s => s.Validate( It.IsAny<TestDto>( ) ) )
				.Returns( Result<TestDto>.Failure( "Validation failed" ) );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			var service = mockService.Object;

			// Act
			var result = await Pipeline.Start( dto )
				.StepResult( d => service.Validate( d ) )
				.ExecuteAsync( );

			// Assert
			Assert.False( result.IsSuccess );
			Assert.Contains( "Validation failed", result.ErrorMessage );
		}

		[Fact]
		public async Task StepResultAsync_WithSuccess_ShouldContinuePipeline( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var mockService = new Mock<ITestService>( );
			mockService.Setup( s => s.ProcessWithResultAsync( It.IsAny<TestDto>( ) ) )
				.ReturnsAsync( ( TestDto d ) => Result<TestDto>.Success(
					new TestDto { Value = d.Value + 1 } ) );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			var service = mockService.Object;

			// Act
			var result = await Pipeline.Start( dto )
				.StepResultAsync( d => service.ProcessWithResultAsync( d ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( 2, result.Value!.Value );
		}

		[Fact]
		public async Task StepResultAsync_WithFailure_ShouldStopPipeline( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var mockService = new Mock<ITestService>( );
			mockService.Setup( s => s.ProcessWithResultAsync( It.IsAny<TestDto>( ) ) )
				.ReturnsAsync( Result<TestDto>.Failure( "Processing failed" ) );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			var service = mockService.Object;

			// Act
			var result = await Pipeline.Start( dto )
				.StepResultAsync( d => service.ProcessWithResultAsync( d ) )
				.ExecuteAsync( );

			// Assert
			Assert.False( result.IsSuccess );
			Assert.Contains( "Processing failed", result.ErrorMessage );
		}
	}
}