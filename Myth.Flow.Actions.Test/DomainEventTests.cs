using FluentAssertions;
using Myth.Flow.Actions.Test.Models;

namespace Myth.Flow.Actions.Test;

public class DomainEventTests {

	[Fact]
	public void DomainEvent_ShouldHaveEventId( ) {
		// Arrange & Act
		var @event = new TestEvent { Message = "test" };

		// Assert
		@event.EventId.Should( ).NotBeNullOrEmpty( );
	}

	[Fact]
	public void DomainEvent_ShouldHaveOccurredAt( ) {
		// Arrange & Act
		var @event = new TestEvent { Message = "test" };

		// Assert
		@event.OccurredAt.Should( ).BeCloseTo( DateTimeOffset.UtcNow, TimeSpan.FromSeconds( 1 ) );
	}

	[Fact]
	public void DomainEvent_EventIdsShouldBeUnique( ) {
		// Arrange & Act
		var event1 = new TestEvent { Message = "test1" };
		var event2 = new TestEvent { Message = "test2" };

		// Assert
		event1.EventId.Should( ).NotBe( event2.EventId );
	}
}
