using FluentAssertions;
using Myth.Flow.Actions.Test.Models;
using Myth.Interfaces;

namespace Myth.Flow.Actions.Test;

public class EventSubscriptionManagerTests {
	private readonly EventSubscriptionManager _sut;

	public EventSubscriptionManagerTests( ) {
		_sut = new EventSubscriptionManager( );
	}

	[Fact]
	public void RegisterHandler_ShouldAddHandler( ) {
		// Arrange & Act
		_sut.RegisterHandler<TestEvent, TestEventHandler>( );

		// Assert
		var handlers = _sut.GetHandlersForEvent<TestEvent>( );
		handlers.Should( ).Contain( typeof( TestEventHandler ) );
	}

	[Fact]
	public void RegisterHandler_MultipleTimes_ShouldNotDuplicate( ) {
		// Arrange & Act
		_sut.RegisterHandler<TestEvent, TestEventHandler>( );
		_sut.RegisterHandler<TestEvent, TestEventHandler>( );

		// Assert
		var handlers = _sut.GetHandlersForEvent<TestEvent>( );
		handlers.Count( ).Should( ).Be( 1 );
	}

	[Fact]
	public void GetHandlersForEvent_WhenNoHandlers_ShouldReturnEmpty( ) {
		// Arrange & Act
		var handlers = _sut.GetHandlersForEvent<TestEvent>( );

		// Assert
		handlers.Should( ).BeEmpty( );
	}

	[Fact]
	public void HasHandlers_WhenHandlerRegistered_ShouldReturnTrue( ) {
		// Arrange
		_sut.RegisterHandler<TestEvent, TestEventHandler>( );

		// Act
		var hasHandlers = _sut.HasHandlers<TestEvent>( );

		// Assert
		hasHandlers.Should( ).BeTrue( );
	}

	[Fact]
	public void HasHandlers_WhenNoHandlers_ShouldReturnFalse( ) {
		// Arrange & Act
		var hasHandlers = _sut.HasHandlers<TestEvent>( );

		// Assert
		hasHandlers.Should( ).BeFalse( );
	}
}
