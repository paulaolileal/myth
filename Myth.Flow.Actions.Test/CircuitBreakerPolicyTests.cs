using FluentAssertions;
using Microsoft.Extensions.Logging;
using Myth.Flow.Resilience;
using NSubstitute;

namespace Myth.Flow.Actions.Test;

public class CircuitBreakerPolicyTests {
	private readonly ILogger _logger;

	public CircuitBreakerPolicyTests( ) {
		_logger = Substitute.For<ILogger>( );
	}

	[Fact]
	public async Task ExecuteAsync_WhenSuccessful_ShouldStayClosed( ) {
		// Arrange
		var policy = new CircuitBreakerPolicy( 3, TimeSpan.FromSeconds( 1 ), _logger );
		var operation = ( ) => Task.FromResult( 42 );

		// Act
		var result = await policy.ExecuteAsync( operation );

		// Assert
		result.Should( ).Be( 42 );
		policy.State.Should( ).Be( CircuitState.Closed );
	}

	[Fact]
	public async Task ExecuteAsync_WhenFailureThresholdReached_ShouldOpenCircuit( ) {
		// Arrange
		var policy = new CircuitBreakerPolicy( 2, TimeSpan.FromSeconds( 1 ), _logger );
		var operation = ( ) => Task.FromException<int>( new InvalidOperationException( "Error" ) );

		// Act
		for ( int i = 0; i < 2; i++ ) {
			try { await policy.ExecuteAsync( operation ); } catch { /* Expected */ }
		}

		// Assert
		policy.State.Should( ).Be( CircuitState.Open );
	}

	[Fact]
	public async Task ExecuteAsync_WhenCircuitOpen_ShouldThrowImmediately( ) {
		// Arrange
		var policy = new CircuitBreakerPolicy( 1, TimeSpan.FromSeconds( 10 ), _logger );
		var operation = ( ) => Task.FromException<int>( new InvalidOperationException( "Error" ) );

		try { await policy.ExecuteAsync( operation ); } catch { /* Open circuit */ }

		// Act
		var act = async ( ) => await policy.ExecuteAsync( ( ) => Task.FromResult( 42 ) );

		// Assert
		await act.Should( ).ThrowAsync<InvalidOperationException>( )
			.WithMessage( "Circuit breaker is open" );
	}

	[Fact]
	public async Task ExecuteAsync_AfterOpenDuration_ShouldTransitionToHalfOpen( ) {
		// Arrange
		var policy = new CircuitBreakerPolicy( 1, TimeSpan.FromMilliseconds( 100 ), _logger );
		var operation = ( ) => Task.FromException<int>( new InvalidOperationException( "Error" ) );

		try { await policy.ExecuteAsync( operation ); } catch { /* Open circuit */ }

		// Act
		await Task.Delay( 150 );
		var result = await policy.ExecuteAsync( ( ) => Task.FromResult( 42 ) );

		// Assert
		result.Should( ).Be( 42 );
		policy.State.Should( ).Be( CircuitState.Closed );
	}
}
