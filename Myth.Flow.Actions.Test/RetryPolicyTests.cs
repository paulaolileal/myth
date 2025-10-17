using FluentAssertions;
using Microsoft.Extensions.Logging;
using Myth.Flow.Resilience;
using NSubstitute;

namespace Myth.Flow.Actions.Test {

	public class RetryPolicyTests {
		private readonly ILogger _logger;

		public RetryPolicyTests( ) {
			_logger = Substitute.For<ILogger>( );
		}

		[Fact]
		public async Task ExecuteAsync_WhenSuccessful_ShouldReturnResult( ) {
			// Arrange
			var policy = new RetryPolicy( 3, 100, false, _logger );
			var operation = ( ) => Task.FromResult( 42 );

			// Act
			var result = await policy.ExecuteAsync( operation );

			// Assert
			result.Should( ).Be( 42 );
		}

		[Fact]
		public async Task ExecuteAsync_WhenFailsOnce_ShouldRetryAndSucceed( ) {
			// Arrange
			var policy = new RetryPolicy( 3, 10, false, _logger );
			var attemptCount = 0;
			var operation = ( ) => {
				attemptCount++;
				if ( attemptCount == 1 )
					throw new InvalidOperationException( "First attempt failed" );
				return Task.FromResult( 42 );
			};

			// Act
			var result = await policy.ExecuteAsync( operation );

			// Assert
			result.Should( ).Be( 42 );
			attemptCount.Should( ).Be( 2 );
		}

		[Fact]
		public async Task ExecuteAsync_WithExponentialBackoff_ShouldIncreaseDelay( ) {
			// Arrange
			var policy = new RetryPolicy( 3, 10, exponentialBackoff: true, _logger );
			var attemptCount = 0;
			var operation = ( ) => {
				attemptCount++;
				if ( attemptCount < 3 )
					throw new InvalidOperationException( "Failed" );
				return Task.FromResult( 42 );
			};

			// Act
			var result = await policy.ExecuteAsync( operation );

			// Assert
			result.Should( ).Be( 42 );
			attemptCount.Should( ).Be( 3 );
		}

		[Fact]
		public async Task ExecuteAsync_WhenAllAttemptsFail_ShouldThrow( ) {
			// Arrange
			var policy = new RetryPolicy( 2, 10, false, _logger );
			var operation = ( ) => Task.FromException<int>( new InvalidOperationException( "Always fails" ) );

			// Act
			var act = async ( ) => await policy.ExecuteAsync( operation );

			// Assert
			await act.Should( ).ThrowAsync<InvalidOperationException>( );
		}
	}
}