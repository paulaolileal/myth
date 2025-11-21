using FluentAssertions;
using Microsoft.OpenApi.Models;
using Myth.Flow.Actions.Swagger;
using Myth.Flow.Actions.ValueObjects;
using NSubstitute;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Myth.Flow.Actions.Test.Swagger;

public class CacheControlSchemaFilterTests {

	[Fact]
	public void Apply_WithCacheControlType_ShouldConfigureAsStringEnum( ) {
		// Arrange
		var filter = new CacheControlSchemaFilter( );
		var schema = new OpenApiSchema( );
		var context = CreateMockContext( typeof( CacheControl ) );

		// Act
		filter.Apply( schema, context );

		// Assert
		schema.Type.Should( ).Be( "string" );
		schema.Format.Should( ).BeNull( );
		schema.Description.Should( ).Contain( "HTTP Cache-Control directive" );
		schema.Enum.Should( ).NotBeEmpty( );
		schema.Enum.Should( ).HaveCountGreaterThan( 10 );
		schema.Example.Should( ).NotBeNull( );
		schema.Pattern.Should( ).NotBeNullOrEmpty( );
	}

	[Fact]
	public void Apply_WithOtherType_ShouldNotModifySchema( ) {
		// Arrange
		var filter = new CacheControlSchemaFilter( );
		var originalSchema = new OpenApiSchema {
			Type = "integer",
			Format = "int32"
		};
		var schema = new OpenApiSchema {
			Type = originalSchema.Type,
			Format = originalSchema.Format
		};
		var context = CreateMockContext( typeof( int ) );

		// Act
		filter.Apply( schema, context );

		// Assert
		schema.Type.Should( ).Be( originalSchema.Type );
		schema.Format.Should( ).Be( originalSchema.Format );
		schema.Enum.Should( ).BeNullOrEmpty( );
	}

	[Fact]
	public void Apply_WithCacheControlType_ShouldIncludeCommonDirectives( ) {
		// Arrange
		var filter = new CacheControlSchemaFilter( );
		var schema = new OpenApiSchema( );
		var context = CreateMockContext( typeof( CacheControl ) );

		// Act
		filter.Apply( schema, context );

		// Assert
		var enumValues = schema.Enum.Cast<Microsoft.OpenApi.Any.OpenApiString>( ).Select( e => e.Value ).ToList( );
		enumValues.Should( ).Contain( "no-cache" );
		enumValues.Should( ).Contain( "no-store" );
		enumValues.Should( ).Contain( "public" );
		enumValues.Should( ).Contain( "private" );
		enumValues.Should( ).Contain( "max-age=3600" );
		enumValues.Should( ).Contain( "public, max-age=1800" );
		enumValues.Should( ).Contain( "private, max-age=300" );
		enumValues.Should( ).Contain( "public, immutable, max-age=31536000" );
	}

	private static SchemaFilterContext CreateMockContext( Type type ) {
		var schemaGenerator = Substitute.For<ISchemaGenerator>( );
		var schemaRepository = new SchemaRepository( );
		return new SchemaFilterContext( type, schemaGenerator, schemaRepository );
	}
}
