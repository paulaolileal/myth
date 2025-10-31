using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Flow.Actions.Brokers;
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Settings;
using Myth.Flow.Actions.Test.Models;
using Myth.Interfaces;

namespace Myth.Flow.Actions.Test {

	public class ServiceCollectionExtensionsTests {

		[Fact]
		public void AddFlowActions_ShouldRegisterCoreServices( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddLogging( );

			// Act
			services.AddFlow( config => config.UseActions( actions => actions.UseInMemory( ) ) );
			var provider = services.BuildServiceProvider( );

			// Assert
			provider.GetService<IDispatcher>( ).Should( ).NotBeNull( );
			provider.GetService<IEventBus>( ).Should( ).NotBeNull( );
			provider.GetService<IEventSubscriptionManager>( ).Should( ).NotBeNull( );
		}

		[Fact]
		public void AddFlowActions_WithInMemory_ShouldRegisterInMemoryBroker( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddLogging( );

			// Act
			services.AddFlow( config => config.UseActions( actions => actions.UseInMemory( ) ) );
			var provider = services.BuildServiceProvider( );

			// Assert
			provider.GetService<IMessageBroker>( ).Should( ).NotBeNull( );
			provider.GetService<IMessageBroker>( ).Should( ).BeOfType<InMemoryBroker>( );
		}

		[Fact]
		public void AddFlowActions_WithCaching_ShouldRegisterCacheProvider( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddLogging( );

			// Act
			services.AddFlow( config => config.UseActions( actions => {
				actions.UseInMemory( )
					   .EnableCaching( cache => {
						   cache.ProviderType = CacheProviderType.Memory;
					   } );
			} ) );
			var provider = services.BuildServiceProvider( );

			// Assert
			provider.GetService<ICacheProvider>( ).Should( ).NotBeNull( );
		}

		[Fact]
		public void AddFlowActions_WithAssemblyScanning_ShouldRegisterHandlers( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddLogging( );

			// Act
			services.AddFlow( config => config.UseActions( actions => actions
				.UseInMemory( )
				.ScanAssemblies( typeof( TestCommandHandler ).Assembly )
			) );
			var provider = services.BuildServiceProvider( );

			// Assert
			provider.GetService<ICommandHandler<TestCommand, string>>( ).Should( ).NotBeNull( );
			provider.GetService<ICommandHandler<TestCommandNoResponse>>( ).Should( ).NotBeNull( );
			provider.GetService<IQueryHandler<TestQuery, string>>( ).Should( ).NotBeNull( );

			var eventHandlers = provider.GetServices<IEventHandler<TestEvent>>( ).ToList( );
			eventHandlers.Should( ).NotBeEmpty( );
		}

		[Fact]
		public void AddFlowActions_WithNullServices_ShouldThrow( ) {
			// Arrange
			IServiceCollection services = null!;

			// Act
			var act = ( ) => services.AddFlow( config => config.UseActions( actions => actions.UseInMemory( ) ) );

			// Assert
			act.Should( ).Throw<ArgumentNullException>( );
		}

		[Fact]
		public void AddFlowActions_WithNullConfiguration_ShouldThrow( ) {
			// Arrange
			var services = new ServiceCollection( );

			// Act
			var act = ( ) => services.AddFlow( null! );

			// Assert
			act.Should( ).Throw<ArgumentNullException>( );
		}
	}
}