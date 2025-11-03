using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.ServiceProvider;
using System;
using Xunit;

namespace Myth.Flow.Test {

	public class SynchronousExecuteTests {

		[Fact]
		public void Execute_ShouldRunSynchronously( ) {
			// Arrange
			var dto = new TestDto { Value = 1 };
			var provider = new ServiceCollection( ).BuildServiceProvider( );
			MythServiceProvider.Initialize( provider );

			// Act
			var result = Pipeline.Start( dto )
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
			MythServiceProvider.Initialize( provider );

			// Act - Using direct service reference instead of service locator
			var service = mockService.Object;
			var result = Pipeline.Start( dto )
				.Step( d => service.Process( d ) )
				.Execute( );

			// Assert
			Assert.False( result.IsSuccess );
			Assert.Contains( "Sync error", result.ErrorMessage );
		}
	}
}