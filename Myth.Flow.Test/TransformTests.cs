using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.ServiceProvider;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class TransformTests {

		[Fact]
		public async Task Transform_ShouldChangeContextType( ) {
			// Arrange
			var dto = new TestDto { Value = 42, Message = "Test" };
			var provider = new ServiceCollection( ).BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			// Act
			var result = await Pipeline.Start( dto )
				.Transform( d => new TestResult {
					Success = true,
					Data = $"{d.Message}:{d.Value}"
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( "Test:42", result.Value!.Data );
			Assert.True( result.Value.Success );
		}

		[Fact]
		public async Task TransformAsync_ShouldChangeContextTypeAsynchronously( ) {
			// Arrange
			var dto = new TestDto { Value = 42, Message = "Test" };
			var provider = new ServiceCollection( ).BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			// Act
			var result = await Pipeline.Start( dto )
				.TransformAsync( async d => {
					await Task.Delay( 10 );
					return new TestResult {
						Success = true,
						Data = $"{d.Message}:{d.Value}"
					};
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( "Test:42", result.Value!.Data );
		}

		[Fact]
		public async Task Transform_AfterStep_ShouldMaintainPipelineState( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var mockService = new Mock<ITestService>( );
			mockService.Setup( s => s.Process( It.IsAny<TestDto>( ) ) )
				.Returns( ( TestDto d ) => new TestDto { Value = d.Value + 1, Message = "Processed" } );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			// Act
			var result = await Pipeline.Start( dto )
				.Step<ITestService>( ( svc, d ) => svc.Process( d ) )
				.Transform( d => new TestResult {
					Success = d.Value > 1,
					Data = d.Message
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.True( result.Value!.Success );
			Assert.Equal( "Processed", result.Value.Data );
		}
	}
}