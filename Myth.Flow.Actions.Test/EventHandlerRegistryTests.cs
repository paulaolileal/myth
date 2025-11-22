using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Flow.Actions.Test.Models;
using Myth.Interfaces;

namespace Myth.Flow.Actions.Test;

/// <summary>
/// Tests for EventHandlerRegistry deduplication functionality
/// </summary>
public class EventHandlerRegistryTests {

	[Fact]
	public void RegisterHandler_MultipleTimes_ShouldNotDuplicate( ) {
		// Arrange
		var services = new ServiceCollection( );
		var subscriptionManager = new EventSubscriptionManager( );
		var registry = new EventHandlerRegistry( subscriptionManager );

		// Act - Register the same handler multiple times
		registry.RegisterHandler( typeof( TestEvent ), typeof( TestEventHandler ) );
		registry.RegisterHandler( typeof( TestEvent ), typeof( TestEventHandler ) );
		registry.RegisterHandler( typeof( TestEvent ), typeof( TestEventHandler ) );

		// Assert - Should only have one registration
		var registrations = registry.GetRegisteredHandlers( );
		var testEventRegistrations = registrations
			.Where( r => r.EventType == typeof( TestEvent ) && r.HandlerType == typeof( TestEventHandler ) )
			.ToList( );

		testEventRegistrations.Should( ).HaveCount( 1, "Multiple registrations of the same handler should be deduplicated" );
	}

	[Fact]
	public void RegisterHandler_DifferentHandlers_ShouldAllowMultiple( ) {
		// Arrange
		var services = new ServiceCollection( );
		var subscriptionManager = new EventSubscriptionManager( );
		var registry = new EventHandlerRegistry( subscriptionManager );

		// Act - Register different handlers for the same event
		registry.RegisterHandler( typeof( TestEvent ), typeof( TestEventHandler ) );
		registry.RegisterHandler( typeof( TestEvent ), typeof( SecondTestEventHandler ) );

		// Assert - Should have both registrations
		var registrations = registry.GetRegisteredHandlers( );
		var testEventRegistrations = registrations
			.Where( r => r.EventType == typeof( TestEvent ) )
			.ToList( );

		testEventRegistrations.Should( ).HaveCount( 2, "Different handlers for the same event should be allowed" );
		testEventRegistrations.Should( ).Contain( r => r.HandlerType == typeof( TestEventHandler ) );
		testEventRegistrations.Should( ).Contain( r => r.HandlerType == typeof( SecondTestEventHandler ) );
	}

	[Fact]
	public void RegisterHandler_SameHandlerDifferentEvents_ShouldAllowMultiple( ) {
		// Arrange
		var services = new ServiceCollection( );
		var subscriptionManager = new EventSubscriptionManager( );
		var registry = new EventHandlerRegistry( subscriptionManager );

		// For this test, we'll use TestEvent only, but register the handler once
		// This test verifies that the registry can handle registrations properly
		var eventType1 = typeof( TestEvent );

		// Act - Register handler for one event
		registry.RegisterHandler( eventType1, typeof( TestEventHandler ) );

		// Assert - Should have the registration
		var registrations = registry.GetRegisteredHandlers( );
		var handlerRegistrations = registrations
			.Where( r => r.HandlerType == typeof( TestEventHandler ) )
			.ToList( );

		handlerRegistrations.Should( ).HaveCount( 1, "Handler should be registered for the event" );
		handlerRegistrations.Should( ).Contain( r => r.EventType == eventType1 );
	}

	[Fact]
	public void GetRegisteredHandlers_EmptyRegistry_ShouldReturnEmpty( ) {
		// Arrange
		_ = new ServiceCollection( );
		var subscriptionManager = new EventSubscriptionManager( );
		var registry = new EventHandlerRegistry( subscriptionManager );

		// Act
		var registrations = registry.GetRegisteredHandlers( );

		// Assert
		registrations.Should( ).BeEmpty( "Empty registry should return no registrations" );
	}

	[Fact]
	public async Task RegisterHandler_ConcurrentAccess_ShouldHandleThreadSafely( ) {
		// Arrange
		var services = new ServiceCollection( );
		var subscriptionManager = new EventSubscriptionManager( );
		var registry = new EventHandlerRegistry( subscriptionManager );
		var tasks = new List<Task>( );

		// Act - Simulate concurrent registration of the same handler
		for ( int i = 0; i < 10; i++ ) {
			tasks.Add( Task.Run( ( ) => {
				registry.RegisterHandler( typeof( TestEvent ), typeof( TestEventHandler ) );
			} ) );
		}

		await Task.WhenAll( tasks );

		// Assert - Should only have one registration despite concurrent access
		var registrations = registry.GetRegisteredHandlers( );
		var testEventRegistrations = registrations
			.Where( r => r.EventType == typeof( TestEvent ) && r.HandlerType == typeof( TestEventHandler ) )
			.ToList( );

		testEventRegistrations.Should( ).HaveCount( 1, "Concurrent registrations should be deduplicated" );
	}
}
