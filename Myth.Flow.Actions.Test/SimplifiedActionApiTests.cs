using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Test.Models;
using Myth.Models;

namespace Myth.Flow.Actions.Test {

	/// <summary>
	/// Tests for the simplified Action API that allows using objects directly without complex state management.
	/// </summary>
	public class SimplifiedActionApiTests {

		public class SimplifiedValidationService {

			public async Task<TestCommand> ValidateAsync( TestCommand command, CancellationToken cancellationToken ) {
				await Task.Delay( 10, cancellationToken );

				if ( string.IsNullOrEmpty( command.Value ) ) {
					throw new InvalidOperationException( "Value is required" );
				}

				return new TestCommand {
					Value = $"Validated: {command.Value}"
				};
			}

			public async Task<Result<TestCommand>> ValidateWithResultAsync( TestCommand command, CancellationToken cancellationToken ) {
				await Task.Delay( 10, cancellationToken );

				if ( string.IsNullOrEmpty( command.Value ) ) {
					return Result<TestCommand>.Failure( "Value is required" );
				}

				return Result<TestCommand>.Success( new TestCommand {
					Value = $"Validated: {command.Value}"
				} );
			}
		}

		[Fact]
		public async Task SimplifiedStepAsync_WithCancellationToken_ShouldPassObjectDirectly( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddFlow( );
			services.AddFlowActions( options => options.UseInMemory( ) );
			services.AddTransient<SimplifiedValidationService>( );
			var serviceProvider = services.BuildServiceProvider( );

			var input = new TestCommand { Value = "test" };

			// Act
			var result = await Pipeline.Start( input, serviceProvider )
				.StepAsync<SimplifiedValidationService>( ( service, command, ct ) =>
					service.ValidateAsync( command, ct ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.NotNull( result.Value );
			Assert.Equal( "Validated: test", result.Value!.Value );
		}

		[Fact]
		public async Task SimplifiedStepResultAsync_WithCancellationToken_ShouldHandleResultPattern( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddFlow( );
			services.AddFlowActions( options => options.UseInMemory( ) );
			services.AddTransient<SimplifiedValidationService>( );
			var serviceProvider = services.BuildServiceProvider( );

			var input = new TestCommand { Value = "test" };

			// Act
			var result = await Pipeline.Start( input, serviceProvider )
				.StepResultAsync<SimplifiedValidationService>( ( service, command, ct ) =>
					service.ValidateWithResultAsync( command, ct ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.NotNull( result.Value );
			Assert.Equal( "Validated: test", result.Value!.Value );
		}

		[Fact]
		public async Task SimplifiedStepResultAsync_WithFailure_ShouldThrowPipelineException( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddFlow( );
			services.AddFlowActions( options => options.UseInMemory( ) );
			services.AddTransient<SimplifiedValidationService>( );
			var serviceProvider = services.BuildServiceProvider( );

			var input = new TestCommand { Value = "" }; // Invalid input

			// Act
			var result = await Pipeline.Start( input, serviceProvider )
				.StepResultAsync<SimplifiedValidationService>( ( service, command, ct ) =>
					service.ValidateWithResultAsync( command, ct ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsFailure );
			Assert.Contains( "Value is required", result.ErrorMessage ?? "" );
		}

		[Fact]
		public async Task ChainedSimplifiedSteps_ShouldWorkCorrectly( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddFlow( );
			services.AddFlowActions( options => options.UseInMemory( ) );
			services.AddTransient<SimplifiedValidationService>( );
			var serviceProvider = services.BuildServiceProvider( );

			var input = new TestCommand { Value = "test" };

			// Act
			var result = await Pipeline.Start( input, serviceProvider )
				.StepAsync<SimplifiedValidationService>( ( service, command, ct ) =>
					service.ValidateAsync( command, ct ) )
				.StepAsync<SimplifiedValidationService>( ( service, command, ct ) =>
					service.ValidateAsync( command, ct ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.NotNull( result.Value );
			Assert.StartsWith( "Validated: Validated:", result.Value!.Value );
		}

		[Fact]
		public async Task SimplifiedApi_WithCancellation_ShouldRespectCancellationToken( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddFlow( );
			services.AddFlowActions( options => options.UseInMemory( ) );
			services.AddTransient<SimplifiedValidationService>( );
			var serviceProvider = services.BuildServiceProvider( );

			var input = new TestCommand { Value = "test" };
			var cts = new CancellationTokenSource( );

			// Cancel immediately
			cts.Cancel( );

			// Act
			var result = await Pipeline.Start( input, serviceProvider )
				.StepAsync<SimplifiedValidationService>( ( service, command, ct ) =>
					service.ValidateAsync( command, ct ) )
				.ExecuteAsync( cts.Token );

			// Assert
			Assert.True( result.IsFailure );
			Assert.Contains( "cancelled", result.ErrorMessage ?? "" );
		}
	}
}