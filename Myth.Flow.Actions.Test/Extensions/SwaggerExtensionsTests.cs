using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Myth.Flow.Actions.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Myth.Flow.Actions.Test.Extensions;

public class SwaggerExtensionsTests {

	[Fact]
	public void AddCacheControlSchemaFilter_ShouldNotThrowException( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddOptions<SwaggerGenOptions>( );

		// Act & Assert - Should not throw
		services.Configure<SwaggerGenOptions>( options => {
			var action = ( ) => options.AddCacheControlSchemaFilter( );
			action.Should( ).NotThrow( );
		} );
	}

	[Fact]
	public void AddFlowActionsSchemaFilters_ShouldNotThrowException( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddOptions<SwaggerGenOptions>( );

		// Act & Assert - Should not throw
		services.Configure<SwaggerGenOptions>( options => {
			var action = ( ) => options.AddFlowActionsSchemaFilters( );
			action.Should( ).NotThrow( );
		} );
	}

	[Fact]
	public void AddCacheControlSchemaFilter_WithChaining_ShouldReturnSwaggerGenOptions( ) {
		// Arrange
		var services = new ServiceCollection( );
		services.AddOptions<SwaggerGenOptions>( );

		// Act & Assert
		services.Configure<SwaggerGenOptions>( options => {
			var result = options.AddCacheControlSchemaFilter( );
			result.Should( ).BeSameAs( options );
		} );
	}
}
