using FluentAssertions;
using Microsoft.Extensions.Logging;
using Myth.Flow.Actions.Brokers;
using Myth.Flow.Actions.Test.Models;
using NSubstitute;

namespace Myth.Flow.Actions.Test {

	public class InMemoryBrokerTests : IAsyncDisposable {
		private readonly ILogger<InMemoryBroker> _logger;
		private readonly InMemoryBroker _sut;

		public InMemoryBrokerTests( ) {
			_logger = Substitute.For<ILogger<InMemoryBroker>>( );
			_sut = new InMemoryBroker( _logger );
		}

		[Fact]
		public async Task PublishAsync_ShouldNotThrow( ) {
			// Arrange
			var @event = new TestEvent { Message = "test" };

			// Act
			var act = async ( ) => await _sut.PublishAsync( @event );

			// Assert
			await act.Should( ).NotThrowAsync( );
		}

		[Fact]
		public async Task StartAsync_ShouldNotThrow( ) {
			// Arrange & Act
			var act = async ( ) => await _sut.StartAsync( );

			// Assert
			await act.Should( ).NotThrowAsync( );
		}

		[Fact]
		public async Task StopAsync_ShouldCompleteSuccessfully( ) {
			// Arrange
			await _sut.StartAsync( );

			// Act
			await _sut.StopAsync( );

			// Assert
			_logger.Received( ).LogInformation( "Stopping in-memory message broker" );
		}

		[Fact]
		public async Task PublishAsync_MultipleMessages_ShouldHandleAll( ) {
			// Arrange
			await _sut.StartAsync( );

			// Act
			for ( int i = 0; i < 10; i++ ) {
				await _sut.PublishAsync( new TestEvent { Message = $"Message {i}" } );
			}

			// Assert - No exception thrown
			await _sut.StopAsync( );
		}

		[Fact]
		public async Task PublishAsync_BeforeStart_ShouldStillWork( ) {
			// Arrange
			var @event = new TestEvent { Message = "test" };

			// Act & Assert - Should not throw
			await _sut.PublishAsync( @event );
		}

		[Fact]
		public async Task StartAsync_ShouldLogStartup( ) {
			// Arrange & Act
			await _sut.StartAsync( );

			// Assert
			_logger.Received( ).LogInformation( "Starting in-memory message broker" );
		}

		public async ValueTask DisposeAsync( ) {
			await _sut.StopAsync( );
			_sut.Dispose( );
		}
	}
}