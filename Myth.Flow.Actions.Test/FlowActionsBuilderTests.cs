using FluentAssertions;
using Myth.Flow.Actions.Settings;

namespace Myth.Flow.Actions.Test {

	public class FlowActionsBuilderTests {

		[Fact]
		public void UseInMemory_ShouldSetBrokerType( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.UseInMemory( );
			var config = builder.Build( );

			// Assert
			config.BrokerType.Should( ).Be( MessageBrokerType.InMemory );
		}

		[Fact]
		public void UseKafka_ShouldSetBrokerTypeAndConfiguration( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.UseKafka( kafka => {
				kafka.BootstrapServers = "localhost:9092";
				kafka.GroupId = "test-group";
			} );
			var config = builder.Build( );

			// Assert
			config.BrokerType.Should( ).Be( MessageBrokerType.Kafka );
			config.BrokerConfigurationFactory.Should( ).NotBeNull( );
		}

		[Fact]
		public void UseRabbitMQ_ShouldSetBrokerTypeAndConfiguration( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.UseRabbitMQ( rabbit => {
				rabbit.HostName = "localhost";
				rabbit.UserName = "guest";
				rabbit.Password = "guest";
			} );
			var config = builder.Build( );

			// Assert
			config.BrokerType.Should( ).Be( MessageBrokerType.RabbitMQ );
			config.BrokerConfigurationFactory.Should( ).NotBeNull( );
		}

		// Telemetry tests removed - telemetry is now controlled by Flow configuration only

		[Fact]
		public void UseCaching_ShouldSetFlag( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.UseCaching( cache => {
				cache.ProviderType = CacheProviderType.Memory;
				cache.DefaultTtl = TimeSpan.FromMinutes( 5 );
			} );
			var config = builder.Build( );

			// Assert
			config.CachingEnabled.Should( ).BeTrue( );
			config.CacheConfiguration.Should( ).NotBeNull( );
		}

		// Retry tests removed - retry policy is now controlled by Flow configuration only

		[Fact]
		public void UseDeadLetterQueue_ShouldSetFlag( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );

			// Act
			builder.UseDeadLetterQueue( );
			var config = builder.Build( );

			// Assert
			config.DeadLetterQueueEnabled.Should( ).BeTrue( );
		}

		[Fact]
		public void ScanAssemblies_ShouldAddAssemblies( ) {
			// Arrange
			var builder = new FlowActionsBuilder( );
			var assembly = typeof( FlowActionsBuilderTests ).Assembly;

			// Act
			builder.ScanAssemblies( assembly );
			var config = builder.Build( );

			// Assert
			config.AssembliesToScan.Should( ).Contain( assembly );
		}

		[Fact]
		public void Build_ShouldChainMethods( ) {
			// Arrange & Act
			var config = new FlowActionsBuilder( )
				.UseInMemory( )
				.UseCaching( )
				.UseDeadLetterQueue( )
				.ScanAssemblies( GetType( ).Assembly )
				.Build( );

			// Assert
			config.BrokerType.Should( ).Be( MessageBrokerType.InMemory );
			config.CachingEnabled.Should( ).BeTrue( );
			config.DeadLetterQueueEnabled.Should( ).BeTrue( );
			config.AssembliesToScan.Should( ).NotBeEmpty( );
		}
	}
}