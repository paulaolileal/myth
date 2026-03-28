using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Myth.Exceptions;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.ServiceProvider;
using Xunit;

namespace Myth.Flow.Test;

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

		var service = mockService.Object;

		// Act
		var result = await Pipeline.Start( dto )
			.Step( d => service.Process( d ) )
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

	[Fact]
	public async Task Transform_WithFailingInnerStep_ShouldIdentifyInnerStepInErrorMessage( ) {
		// Arrange
		var dto = new TestDto { Value = 1 };
		var provider = new ServiceCollection( ).BuildServiceProvider( );
		MythServiceProvider.Initialize( provider );

		// Act — TapAsync throws; Transform re-executes all previous steps, one of which fails
		var result = await Pipeline.Start( dto )
			.TapAsync( _ => throw new InvalidOperationException( "inner step error" ) )
			.Transform( d => new TestResult { Data = d.Message } )
			.ExecuteAsync( );

		// Assert — pipeline returns Failure; error message must reference the inner step (TapAsync at index 0)
		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).Contain( "inner step [0]" );
		result.ErrorMessage.Should( ).Contain( "TapAsync" );
	}

	[Fact]
	public async Task Transform_WithFailingMapper_ShouldIdentifyMappingPhaseInErrorMessage( ) {
		// Arrange
		var dto = new TestDto { Value = 42 };
		var provider = new ServiceCollection( ).BuildServiceProvider( );
		MythServiceProvider.Initialize( provider );

		// Act — mapper itself throws
		var result = await Pipeline.Start( dto )
			.Transform<TestResult>( _ => throw new InvalidOperationException( "mapper error" ) )
			.ExecuteAsync( );

		// Assert — error message must reference the mapping phase and the types involved
		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).Contain( "mapping phase" );
		result.ErrorMessage.Should( ).Contain( "TestDto" );
		result.ErrorMessage.Should( ).Contain( "TestResult" );
	}

	[Fact]
	public async Task TransformAsync_WithFailingInnerStep_ShouldIdentifyInnerStepInErrorMessage( ) {
		// Arrange
		var dto = new TestDto { Value = 1 };
		var provider = new ServiceCollection( ).BuildServiceProvider( );
		MythServiceProvider.Initialize( provider );

		// Act — Step at index 0 fails; TapAsync at index 1 would not run
		var result = await Pipeline.Start( dto )
			.Step( _ => throw new InvalidOperationException( "step error" ) )
			.TapAsync( _ => Task.CompletedTask )
			.TransformAsync<TestResult>( async _ => {
				await Task.Delay( 0 );
				return new TestResult( );
			} )
			.ExecuteAsync( );

		// Assert — Step at index 0 fails; message must reference it
		result.IsFailure.Should( ).BeTrue( );
		result.ErrorMessage.Should( ).Contain( "inner step [0]" );
		result.ErrorMessage.Should( ).Contain( "Step" );
	}
}
