using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.Models;
using Myth.ServiceProvider;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class ConditionalWhenTests {

		[Fact]
		public async Task When_PredicateTrue_ShouldExecuteConditionalPipeline( ) {
			// Arrange
			var dto = new TestDto { Value = 5 };
			var provider = new ServiceCollection( ).BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			// Act
			var result = await Pipeline.Start( dto )
				.When(
					d => d.Value > 3,
					pipeline => pipeline.Tap( d => d.Value *= 2 ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( 10, result.Value!.Value );
		}

		[Fact]
		public async Task When_PredicateFalse_ShouldSkipConditionalPipeline( ) {
			// Arrange
			var dto = new TestDto { Value = 2 };
			var provider = new ServiceCollection( ).BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			// Act
			var result = await Pipeline.Start( dto )
				.When(
					d => d.Value > 3,
					pipeline => pipeline.Tap( d => d.Value *= 2 ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( 2, result.Value!.Value );
		}

		[Fact]
		public async Task When_WithFailingConditionalPipeline_ShouldReturnFailure( ) {
			// Arrange
			var dto = new TestDto { Value = 5 };
			var mockService = new Mock<IValidationService>( );
			mockService.Setup( s => s.Validate( It.IsAny<TestDto>( ) ) )
				.Returns( Result<TestDto>.Failure( "Conditional validation failed" ) );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			var service = mockService.Object;

			// Act
			var result = await Pipeline.Start( dto )
				.When(
					d => d.Value > 3,
					pipeline => pipeline.StepResult(
						d => service.Validate( d ) ) )
				.ExecuteAsync( );

			// Assert
			Assert.False( result.IsSuccess );
			Assert.Contains( "Conditional validation failed", result.ErrorMessage );
		}
	}
}