using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Test.Models;
using Myth.Models;

namespace Myth.Flow.Actions.Test {

	/// <summary>
	/// Tests for the simplified Action API that allows using objects directly without complex state management.
	/// </summary>
	public class SimplifiedActionApiTests : BaseTestFixture {

		protected override void ConfigureServices( IServiceCollection services ) {
			services.AddFlow( config => config.UseActions( actions => actions.UseInMemory( ) ) );
		}

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
			// Arrange - using inherited service configuration

			var service = new SimplifiedValidationService( );
			var input = new TestCommand { Value = "test" };

			// Act
			var result = await Pipeline.Start( input )
				.StepAsync( async ( state, ct ) => {
					var result = await service.ValidateAsync( state.CurrentRequest!, ct );
					state.CurrentRequest = result;
					return state;
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.NotNull( result.Value );
			Assert.Equal( "Validated: test", result.Value!.Value );
		}

		[Fact]
		public async Task SimplifiedStepResultAsync_WithCancellationToken_ShouldHandleResultPattern( ) {
			// Arrange - using inherited service configuration

			var service = new SimplifiedValidationService( );
			var input = new TestCommand { Value = "test" };

			// Act
			var result = await Pipeline.Start( input )
				.StepResultAsync( async ( state, ct ) => {
					var validationResult = await service.ValidateWithResultAsync( state.CurrentRequest!, ct );

					if ( validationResult.IsSuccess ) {
						state.CurrentRequest = validationResult.Value;
						return Result<ActionPipelineState<TestCommand>>.Success( state );
					}

					return Result<ActionPipelineState<TestCommand>>.Failure( validationResult.ErrorMessage ?? "Validation failed" );
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.NotNull( result.Value );
			Assert.Equal( "Validated: test", result.Value!.Value );
		}

		[Fact]
		public async Task SimplifiedStepResultAsync_WithFailure_ShouldThrowPipelineException( ) {
			// Arrange - using inherited service configuration

			var service = new SimplifiedValidationService( );
			var input = new TestCommand { Value = "" }; // Invalid input

			// Act
			var result = await Pipeline.Start( input )
				.StepResultAsync( async ( state, ct ) => {
					var validationResult = await service.ValidateWithResultAsync( state.CurrentRequest!, ct );

					if ( validationResult.IsSuccess ) {
						state.CurrentRequest = validationResult.Value;
						return Result<ActionPipelineState<TestCommand>>.Success( state );
					}

					return Result<ActionPipelineState<TestCommand>>.Failure( validationResult.ErrorMessage ?? "Validation failed" );
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsFailure );
			Assert.Contains( "Value is required", result.ErrorMessage ?? "" );
		}

		[Fact]
		public async Task ChainedSimplifiedSteps_ShouldWorkCorrectly( ) {
			// Arrange - using inherited service configuration

			var service = new SimplifiedValidationService( );
			var input = new TestCommand { Value = "test" };

			// Act
			var result = await Pipeline.Start( input )
				.StepAsync( async ( state, ct ) => {
					var result = await service.ValidateAsync( state.CurrentRequest!, ct );
					state.CurrentRequest = result;
					return state;
				} )
				.StepAsync( async ( state, ct ) => {
					var result = await service.ValidateAsync( state.CurrentRequest!, ct );
					state.CurrentRequest = result;
					return state;
				} )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.NotNull( result.Value );
			Assert.StartsWith( "Validated: Validated:", result.Value!.Value );
		}

		[Fact]
		public async Task SimplifiedApi_WithCancellation_ShouldRespectCancellationToken( ) {
			// Arrange - using inherited service configuration

			var service = new SimplifiedValidationService( );
			var input = new TestCommand { Value = "test" };
			var cts = new CancellationTokenSource( );

			// Cancel immediately
			cts.Cancel( );

			// Act
			var result = await Pipeline.Start( input )
				.StepAsync( async ( state, ct ) => {
					var result = await service.ValidateAsync( state.CurrentRequest!, ct );
					state.CurrentRequest = result;
					return state;
				} )
				.ExecuteAsync( cts.Token );

			// Assert
			Assert.True( result.IsFailure );
			Assert.Contains( "cancelled", result.ErrorMessage ?? "" );
		}
	}
}