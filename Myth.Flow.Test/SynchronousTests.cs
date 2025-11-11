using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.ServiceProvider;
using Xunit;

namespace Myth.Flow.Test;

public class SynchronousStepTests {
	private readonly Mock<ITestService> _mockService = new( );
	private ITestService _service => _mockService.Object;

	public SynchronousStepTests( ) {
		var services = new ServiceCollection( );
		var serviceProvider = services.BuildServiceProvider( );

		MythServiceProvider.Initialize( serviceProvider );
	}

	[Fact]
	public async Task Step_ShouldExecuteSuccessfully( ) {
		// Arrange
		var dto = new TestDto { Value = 1 };

		_mockService
			.Setup( s => s.Process( It.IsAny<TestDto>( ) ) )
			.Returns( ( TestDto d ) => new TestDto { Value = d.Value + 1 } );

		// Act
		var result = await Pipeline
			.Start( dto )
			.Step( d => _service.Process( d ) )
			.ExecuteAsync( );

		// Assert
		Assert.True( result.IsSuccess );
		Assert.Equal( 2, result.Value!.Value );
		_mockService.Verify( s => s.Process( It.IsAny<TestDto>( ) ), Times.Once );
	}

	[Fact]
	public async Task Step_WithOnSuccess_ShouldInvokeCallback( ) {
		// Arrange
		var dto = new TestDto { Value = 1 };
		var callbackInvoked = false;

		_mockService
			.Setup( s => s.Process( It.IsAny<TestDto>( ) ) )
			.Returns( ( TestDto d ) => new TestDto { Value = d.Value + 1 } );

		// Act
		var result = await Pipeline.Start( dto )
			.Step(
				d => _service.Process( d ),
				onSuccess: _ => callbackInvoked = true )
			.ExecuteAsync( );

		// Assert
		Assert.True( result.IsSuccess );
		Assert.True( callbackInvoked );
	}

	[Fact]
	public async Task Step_WithOnError_ShouldInvokeCallbackOnFailure( ) {
		// Arrange
		var dto = new TestDto { Value = 1 };
		Exception? capturedException = null;

		_mockService
			.Setup( s => s.Process( It.IsAny<TestDto>( ) ) )
			.Throws( new InvalidOperationException( "Test error" ) );

		// Act
		var result = await Pipeline.Start( dto )
			.Step(
				d => _service.Process( d ),
				onError: ex => capturedException = ex )
			.ExecuteAsync( );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.NotNull( capturedException );
		Assert.IsType<InvalidOperationException>( capturedException );
	}
}
