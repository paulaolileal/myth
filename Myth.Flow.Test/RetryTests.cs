using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.ServiceProvider;
using Xunit;

namespace Myth.Flow.Test;

public class RetryTests {

	[Fact]
	public async Task WithRetry_ShouldRetryOnFailure( ) {
		// Arrange
		var dto = new TestDto { Value = 1 };
		var attempts = 0;
		var mockService = new Mock<ITestService>( );
		mockService.Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
			.Returns<TestDto>( d => {
				attempts++;
				if ( attempts < 3 )
					throw new InvalidOperationException( "Transient error" );
				return Task.FromResult( new TestDto { Value = d.Value + 1 } );
			} );

		var services = new ServiceCollection( );
		services.AddSingleton( mockService.Object );
		var provider = services.BuildServiceProvider( );
		MythServiceProvider.Initialize( provider );

		var service = mockService.Object;

		// Act
		var result = await Pipeline.Start( dto )
			.WithRetry( maxAttempts: 3, backoffMs: 10 )
			.StepAsync( d => service.ProcessAsync( d ) )
			.ExecuteAsync( );

		// Assert
		Assert.True( result.IsSuccess );
		Assert.Equal( 3, attempts );
		Assert.Equal( 2, result.Value!.Value );
	}

	[Fact]
	public async Task WithRetry_ExceedingMaxAttempts_ShouldFail( ) {
		// Arrange
		var dto = new TestDto { Value = 1 };
		var mockService = new Mock<ITestService>( );
		mockService.Setup( s => s.ProcessAsync( It.IsAny<TestDto>( ) ) )
			.ThrowsAsync( new InvalidOperationException( "Persistent error" ) );

		var services = new ServiceCollection( );
		services.AddSingleton( mockService.Object );
		var provider = services.BuildServiceProvider( );
		MythServiceProvider.Initialize( provider );

		var service = mockService.Object;

		// Act
		var result = await Pipeline.Start( dto )
			.WithRetry( maxAttempts: 2, backoffMs: 10 )
			.StepAsync( d => service.ProcessAsync( d ) )
			.ExecuteAsync( );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.Contains( "Persistent error", result.ErrorMessage );
	}
}
