using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow.Test.Models;
using Myth.Models;
using System.Diagnostics;
using Xunit;

namespace Myth.Flow.Test {

	public class DependencyInjectionTests {

		[Fact]
		public void AddFlow_ShouldRegisterServices( ) {
			// Arrange
			var services = new ServiceCollection( );

			// Act
			services.AddFlow( );
			var provider = services.BuildServiceProvider( );

			// Assert
			Assert.NotNull( provider.GetService<PipelineConfiguration>( ) );
			Assert.NotNull( provider.GetService<ActivitySource>( ) );
		}

		[Fact]
		public void AddFlow_WithConfiguration_ShouldApplyConfiguration( ) {
			// Arrange
			var services = new ServiceCollection( );

			// Act
			services.AddFlow( config => {
				config.EnableTelemetry = false;
				config.DefaultRetryAttempts = 5;
			} );
			var provider = services.BuildServiceProvider( );
			var config = provider.GetRequiredService<PipelineConfiguration>( );

			// Assert
			Assert.False( config.EnableTelemetry );
			Assert.Equal( 5, config.DefaultRetryAttempts );
		}

		[Fact]
		public void AddFlow_ShouldSetGlobalServiceProvider( ) {
			// Arrange
			var services = new ServiceCollection( );
			services.AddFlow( );
			var provider = services.BuildServiceProvider( );

			// Service provider auto-initializes now - no manual initialization needed

			// Act
			var dto = new TestDto { Value = 1 };
			var pipeline = Pipeline.Start( dto );

			// Assert - pipeline should be created without exception
			Assert.NotNull( pipeline );
		}
	}
}