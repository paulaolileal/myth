using FluentAssertions;
using Microsoft.Extensions.Logging;
using Myth.Flow.Resilience;
using NSubstitute;

namespace Myth.Flow.Actions.Test {

	public class DeadLetterQueueTests {
		private readonly ILogger<DeadLetterQueue> _logger;
		private readonly DeadLetterQueue _sut;

		public DeadLetterQueueTests( ) {
			_logger = Substitute.For<ILogger<DeadLetterQueue>>( );
			_sut = new DeadLetterQueue( _logger, maxSize: 100 );
		}

		[Fact]
		public void Enqueue_ShouldAddMessage( ) {
			// Arrange
			var message = new { Id = 1 };
			var exception = new InvalidOperationException( "Test" );

			// Act
			_sut.Enqueue( message, exception );

			// Assert
			_sut.Count.Should( ).Be( 1 );
		}

		[Fact]
		public void TryDequeue_WhenMessagesExist_ShouldReturnMessage( ) {
			// Arrange
			var message = new { Id = 1 };
			var exception = new InvalidOperationException( "Test" );
			_sut.Enqueue( message, exception );

			// Act
			var success = _sut.TryDequeue( out var deadLetter );

			// Assert
			success.Should( ).BeTrue( );
			deadLetter.Should( ).NotBeNull( );
			deadLetter!.Exception.Should( ).Be( exception );
		}

		[Fact]
		public void TryDequeue_WhenEmpty_ShouldReturnFalse( ) {
			// Arrange & Act
			var success = _sut.TryDequeue( out var deadLetter );

			// Assert
			success.Should( ).BeFalse( );
			deadLetter.Should( ).BeNull( );
		}
	}
}