using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using System;
using Xunit;

namespace Myth.Flow.Test {

	public class SynchronousExecuteTests {

		[Fact]
		public void Execute_ShouldRunSynchronously( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };

			// Act
			var result = Pipeline.Start( dto, new ServiceCollection( ).BuildServiceProvider( ) )
				.Tap( d => d.Value++ )
				.Execute( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.Equal( 2, result.Value!.Value );
		}

		[Fact]
		public void Execute_WithError_ShouldReturnFailure( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var mockService = new Mock<ITestService>( );
			mockService.Setup( s => s.Process( It.IsAny<TestDto>( ) ) )
				.Throws( new InvalidOperationException( "Sync error" ) );

			var services = new ServiceCollection( );
			services.AddSingleton( mockService.Object );
			var provider = services.BuildServiceProvider( );

			// Act
			var result = Pipeline.Start( dto, provider )
				.Step<ITestService>( ( svc, d ) => svc.Process( d ) )
				.Execute( );

			// Assert
			Assert.False( result.IsSuccess );
			Assert.Contains( "Sync error", result.ErrorMessage );
		}
	}
}