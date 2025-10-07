using Myth.Models;
using System.Diagnostics;
using Xunit;

namespace Myth.Flow.Test {

	public class PipelineConfigurationTests {

		[Fact]
		public void DefaultConfiguration_ShouldHaveCorrectDefaults( ) {
			// Act
			var config = new PipelineConfiguration( );

			// Assert
			Assert.True( config.EnableTelemetry );
			Assert.True( config.EnableLogging );
			Assert.Equal( 0, config.DefaultRetryAttempts );
			Assert.Equal( 100, config.DefaultBackoffMs );
			Assert.Null( config.ActivitySource );
		}

		[Fact]
		public void Configuration_ShouldAllowCustomization( ) {
			// Arrange
			var activitySource = new ActivitySource( "Test" );

			// Act
			var config = new PipelineConfiguration {
				EnableTelemetry = false,
				EnableLogging = false,
				DefaultRetryAttempts = 3,
				DefaultBackoffMs = 500,
				ActivitySource = activitySource
			};

			// Assert
			Assert.False( config.EnableTelemetry );
			Assert.False( config.EnableLogging );
			Assert.Equal( 3, config.DefaultRetryAttempts );
			Assert.Equal( 500, config.DefaultBackoffMs );
			Assert.Equal( activitySource, config.ActivitySource );
		}
	}
}