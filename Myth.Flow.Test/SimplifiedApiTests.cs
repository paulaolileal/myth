using Microsoft.Extensions.DependencyInjection;
using Myth.Flow.Test.Models;
using Myth.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Flow.Test {

	/// <summary>
	/// Tests for the simplified API that allows using objects directly without complex context management.
	/// </summary>
	public class SimplifiedApiTests {

		public class ValidationService {

			public async Task<TestDto> ValidateAsync( TestDto dto, CancellationToken cancellationToken ) {
				await Task.Delay( 10, cancellationToken );

				if ( dto.Value <= 0 ) {
					throw new InvalidOperationException( "Value must be greater than 0" );
				}

				return new TestDto {
					Value = dto.Value + 1,
					Message = $"Validated: {dto.Value}"
				};
			}

			public async Task<Result<TestDto>> ValidateWithResultAsync( TestDto dto, CancellationToken cancellationToken ) {
				await Task.Delay( 10, cancellationToken );

				if ( dto.Value <= 0 ) {
					return Result<TestDto>.Failure( "Value must be greater than 0" );
				}

				return Result<TestDto>.Success( new TestDto {
					Value = dto.Value + 1,
					Message = $"Validated: {dto.Value}"
				} );
			}
		}

		[Fact]
		public async Task StepAsync_WithCancellationToken_ShouldPassObjectDirectly( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddTransient<ValidationService>( );
			var serviceProvider = services.BuildServiceProvider( );

			var input = new TestDto { Value = 5 };

			// Act
			var result = await Pipeline.Start( input, serviceProvider )
				.StepAsync<ValidationService>( ( service, dto, ct ) =>
					service.ValidateAsync( dto, ct ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.NotNull( result.Value );
			Assert.NotNull( result.Value );
			Assert.Equal( 6, result.Value!.Value );
			Assert.StartsWith( "Validated:", result.Value.Message ?? "" );
		}

		[Fact]
		public async Task StepResultAsync_WithCancellationToken_ShouldHandleResultPattern( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddTransient<ValidationService>( );
			var serviceProvider = services.BuildServiceProvider( );

			var input = new TestDto { Value = 5 };

			// Act
			var result = await Pipeline.Start( input, serviceProvider )
				.StepResultAsync<ValidationService>( ( service, dto, ct ) =>
					service.ValidateWithResultAsync( dto, ct ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.NotNull( result.Value );
			Assert.NotNull( result.Value );
			Assert.Equal( 6, result.Value!.Value );
			Assert.StartsWith( "Validated:", result.Value.Message ?? "" );
		}

		[Fact]
		public async Task StepResultAsync_WithFailure_ShouldThrowPipelineException( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddTransient<ValidationService>( );
			var serviceProvider = services.BuildServiceProvider( );

			var input = new TestDto { Value = -1 }; // Invalid input

			// Act
			var result = await Pipeline.Start( input, serviceProvider )
				.StepResultAsync<ValidationService>( ( service, dto, ct ) =>
					service.ValidateWithResultAsync( dto, ct ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsFailure );
			Assert.Contains( "Value must be greater than 0", result.ErrorMessage ?? "" );
		}

		[Fact]
		public async Task ChainedSteps_WithSimplifiedApi_ShouldWorkCorrectly( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddTransient<ValidationService>( );
			var serviceProvider = services.BuildServiceProvider( );

			var input = new TestDto { Value = 5 };

			// Act
			var result = await Pipeline.Start( input, serviceProvider )
				.StepAsync<ValidationService>( ( service, dto, ct ) =>
					service.ValidateAsync( dto, ct ) )
				.Transform( testDto => new TestDto { Value = testDto.Value * 2, Message = $"Processed: {testDto.Message}" } )
				.StepAsync<ValidationService>( ( service, dto, ct ) =>
					service.ValidateAsync( dto, ct ) )
				.ExecuteAsync( );

			// Assert
			Assert.True( result.IsSuccess );
			Assert.NotNull( result.Value );
			Assert.Equal( 13, result.Value!.Value ); // 5 -> 6 -> 12 -> 13
			Assert.StartsWith( "Validated:", result.Value.Message ?? "" );
		}

		[Fact]
		public async Task SimplifiedApi_WithCancellation_ShouldRespectCancellationToken( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddTransient<ValidationService>( );
			var serviceProvider = services.BuildServiceProvider( );

			var input = new TestDto { Value = 5 };
			var cts = new CancellationTokenSource( );

			// Cancel immediately
			cts.Cancel( );

			// Act
			var result = await Pipeline.Start( input, serviceProvider )
				.StepAsync<ValidationService>( ( service, dto, ct ) =>
					service.ValidateAsync( dto, ct ) )
				.ExecuteAsync( cts.Token );

			// Assert
			Assert.True( result.IsFailure );
			Assert.Contains( "cancelled", result.ErrorMessage ?? "" );
		}
	}
}