using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Flow.Actions.Test.Models;
using Myth.Interfaces;
using NSubstitute;

namespace Myth.Flow.Actions.Test;

public class EventBusTests : BaseTestFixture {
	private readonly IMessageBroker _messageBroker;
	private readonly ILogger<EventBus> _logger;
	private readonly ActivitySource _activitySource;
	private readonly EventBus _sut;

	public EventBusTests( ) {
		_messageBroker = Substitute.For<IMessageBroker>( );
		_logger = Substitute.For<ILogger<EventBus>>( );
		_activitySource = new ActivitySource( "Test" );
		_sut = new EventBus( _messageBroker, _logger, _activitySource );
	}

	protected override void ConfigureServices( IServiceCollection services ) {
		var handler = new TestEventHandler( );
		services.AddSingleton<TestEventHandler>( handler );
	}

	[Fact]
	public async Task PublishAsync_ShouldCallMessageBroker( ) {
		// Arrange
		var @event = new TestEvent { Message = "test" };

		// Act
		await _sut.PublishAsync( @event );

		// Assert
		await _messageBroker.Received( 1 ).PublishAsync( @event, Arg.Any<CancellationToken>( ) );
	}

	[Fact]
	public async Task PublishAsync_WithSubscribedHandlers_ShouldOnlyPublishToMessageBroker( ) {
		// Arrange
		var @event = new TestEvent { Message = "test" };
		_sut.Subscribe<TestEvent, TestEventHandler>( );

		// Act
		await _sut.PublishAsync( @event );

		// Assert
		// EventBus should only publish to message broker, not invoke handlers directly
		await _messageBroker.Received( 1 ).PublishAsync( @event, Arg.Any<CancellationToken>( ) );

		// Handlers should NOT be invoked directly by EventBus - they will be handled by the message broker
		var handler = ServiceProvider.GetRequiredService<TestEventHandler>( );
		handler.CallCount.Should( ).Be( 0 );
	}

	[Fact]
	public void Subscribe_ShouldRegisterHandler( ) {
		// Arrange & Act
		_sut.Subscribe<TestEvent, TestEventHandler>( );
		_sut.Subscribe<TestEvent, SecondTestEventHandler>( );

		// Assert - No exception thrown
	}

	[Fact]
	public void Unsubscribe_ShouldRemoveHandler( ) {
		// Arrange
		_sut.Subscribe<TestEvent, TestEventHandler>( );

		// Act
		_sut.Unsubscribe<TestEvent, TestEventHandler>( );

		// Assert - No exception thrown
	}
}
