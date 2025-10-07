using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	public class CancellationTests {

		[Fact]
		public async Task ExecuteAsync_WithCancellation_ShouldReturnFailure( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var cts = new CancellationTokenSource( );
			var mockService = new Mock<ITestService>( );

			mockService.Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
				.Returns<TestDto>( async d => {
					await Task.Delay( 10 ); // Pequeno delay
					cts.Token.ThrowIfCancellationRequested( ); // Força verificação
					return new TestDto { Value = d.Value + 1 };
				} );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );

			cts.Cancel( );

			// Act
			var result = await Pipeline.Start( dto, provider )
				.StepAsync<ITestService>( ( svc, d ) => svc.ProcessAsync( d ) )
				.ExecuteAsync( cts.Token );

			// Assert
			Assert.False( result.IsSuccess );
			Assert.Contains( "cancelled", result.ErrorMessage, StringComparison.OrdinalIgnoreCase );
		}

		[Fact]
		public async Task ExecuteAsync_PreCancelled_ShouldReturnImmediately( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var cts = new CancellationTokenSource( );
			cts.Cancel( );

			// Act
			var result = await Pipeline.Start( dto )
				.Tap( d => d.Value++ )
				.ExecuteAsync( cts.Token );

			// Assert
			Assert.False( result.IsSuccess );
		}
	}
}