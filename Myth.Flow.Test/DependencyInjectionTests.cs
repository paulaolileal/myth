using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow.Test.Models;
using Myth.Models;
using Xunit;

namespace Myth.Flow.Test;

public class DependencyInjectionTests : BaseTestFixture {

	[Fact]
	public void AddFlow_ShouldRegisterServices( ) {
		// Assert
		Assert.NotNull( ServiceProvider.GetService<PipelineConfiguration>( ) );
		Assert.NotNull( ServiceProvider.GetService<ActivitySource>( ) );
	}

	[Fact]
	public void AddFlow_WithConfiguration_ShouldApplyConfiguration( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddFlow( builder => builder
			.DisableTelemetry( )
			.UseRetry( 5 )
		);
		var provider = services.BuildServiceProvider( );
		var config = provider.GetRequiredService<PipelineConfiguration>( );

		// Assert
		Assert.False( config.EnableTelemetry );
		Assert.Equal( 5, config.DefaultRetryAttempts );
	}

	[Fact]
	public void AddFlow_ShouldSetGlobalServiceProvider( ) {
		// Act
		var dto = new TestDto { Value = 1 };
		var pipeline = Pipeline.Start( dto );

		// Assert - pipeline should be created without exception
		Assert.NotNull( pipeline );
	}

	[Fact]
	public void AddFlow_WithFluentConfiguration_ShouldRegisterServices( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddFlow( builder => builder
			.UseTelemetry( )
			.UseLogging( )
			.UseRetry( 3, 200 )
		);
		var provider = services.BuildServiceProvider( );

		// Assert
		Assert.NotNull( provider.GetService<PipelineConfiguration>( ) );
		Assert.NotNull( provider.GetService<ActivitySource>( ) );
	}

	[Fact]
	public void AddFlow_WithFluentConfiguration_ShouldApplyConfiguration( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddFlow( builder => builder
			.DisableTelemetry( )
			.DisableLogging( )
			.UseRetry( 5, 300 )
		);
		var provider = services.BuildServiceProvider( );
		var config = provider.GetRequiredService<PipelineConfiguration>( );

		// Assert
		Assert.False( config.EnableTelemetry );
		Assert.False( config.EnableLogging );
		Assert.Equal( 5, config.DefaultRetryAttempts );
		Assert.Equal( 300, config.DefaultBackoffMs );
	}

	[Fact]
	public void AddFlow_WithFluentConfiguration_CustomActivitySource_ShouldApplyConfiguration( ) {
		// Arrange
		var services = new ServiceCollection( );
		var customActivitySource = new ActivitySource( "CustomSource", "1.0.0" );
		services.AddFlow( builder => builder
			.UseTelemetry( )
			.UseActivitySource( customActivitySource )
		);
		var provider = services.BuildServiceProvider( );
		var config = provider.GetRequiredService<PipelineConfiguration>( );
		var activitySource = provider.GetRequiredService<ActivitySource>( );

		// Assert
		Assert.True( config.EnableTelemetry );
		Assert.Equal( customActivitySource, config.ActivitySource );
		Assert.Equal( customActivitySource, activitySource );
	}

	[Fact]
	public void AddFlow_WithFluentConfiguration_CustomActivitySourceByName_ShouldApplyConfiguration( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddFlow( builder => builder
			.UseTelemetry( )
			.UseActivitySource( "MyCustomSource", "2.0.0" )
		);
		var provider = services.BuildServiceProvider( );
		var config = provider.GetRequiredService<PipelineConfiguration>( );
		var activitySource = provider.GetRequiredService<ActivitySource>( );

		// Assert
		Assert.True( config.EnableTelemetry );
		Assert.NotNull( config.ActivitySource );
		Assert.Equal( "MyCustomSource", config.ActivitySource.Name );
		Assert.Equal( "2.0.0", config.ActivitySource.Version );
		Assert.Equal( config.ActivitySource, activitySource );
	}

	[Fact]
	public void AddFlow_WithFluentConfiguration_DisableRetry_ShouldSetRetryToZero( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddFlow( builder => builder
			.UseRetry( 5, 100 )  // Set some retry first
			.DisableRetry( )     // Then disable it
		);
		var provider = services.BuildServiceProvider( );
		var config = provider.GetRequiredService<PipelineConfiguration>( );

		// Assert
		Assert.Equal( 0, config.DefaultRetryAttempts );
	}
}
