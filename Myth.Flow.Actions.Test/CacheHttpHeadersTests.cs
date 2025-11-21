using FluentAssertions;
using Myth.Flow.Actions.ValueObjects;
using Myth.Interfaces;
using Myth.Models;
using Myth.ValueObjects;
using Xunit;

namespace Myth.Flow.Actions.Test;

public class CacheHttpHeadersTests {

	[Fact]
	public void CacheConfigBuilder_Public_Should_Set_Public_Policy( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		var duration = TimeSpan.FromMinutes( 15 );

		// Act
		builder.Public( duration );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Enabled.Should( ).BeTrue( );
		options.Policy.Should( ).NotBeNull( );
		options.Policy!.IsPublic.Should( ).BeTrue( );
		options.Policy.MaxAge.Should( ).Be( duration );
		options.Ttl.Should( ).Be( duration );
	}

	[Fact]
	public void CacheConfigBuilder_Private_Should_Set_Private_Policy( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		var duration = TimeSpan.FromMinutes( 5 );

		// Act
		builder.Private( duration );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Policy!.IsPrivate.Should( ).BeTrue( );
		options.Policy.MaxAge.Should( ).Be( duration );
	}

	[Fact]
	public void CacheConfigBuilder_NoCache_Should_Set_NoCache_Policy( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );

		// Act
		builder.NoCache( );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Policy!.IsNoCache.Should( ).BeTrue( );
	}

	[Fact]
	public void CacheConfigBuilder_Immutable_Should_Set_Immutable_Policy( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		var duration = TimeSpan.FromDays( 1 );

		// Act
		builder.Immutable( duration );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Policy!.IsPublic.Should( ).BeTrue( );
		options.Policy.ToHeaderValue( ).Should( ).Contain( "immutable" );
	}

	[Fact]
	public void CacheConfigBuilder_WithPolicy_Should_Set_Custom_Policy( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		var customPolicy = new CachePolicy( )
			.Add( CacheControl.Private )
			.Add( CacheControl.MustRevalidate )
			.WithMaxAge( TimeSpan.FromMinutes( 10 ) );

		// Act
		builder.WithPolicy( customPolicy );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Policy.Should( ).Be( customPolicy );
		options.Ttl.Should( ).Be( TimeSpan.FromMinutes( 10 ) );
	}

	[Fact]
	public void CacheConfigBuilder_WithETag_Should_Set_ETag_Generator( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		Func<object, string> etagGenerator = obj => $"\"{obj.GetHashCode( )}\"";

		// Act
		builder.WithETag( etagGenerator );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.ETagGenerator.Should( ).Be( etagGenerator );
	}

	[Fact]
	public void CacheConfigBuilder_WithVary_Should_Set_Vary_Headers( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		var varyHeaders = new[ ] { "Accept-Language", "Authorization" };

		// Act
		builder.WithVary( varyHeaders );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.VaryHeaders.Should( ).BeEquivalentTo( varyHeaders );
	}

	[Fact]
	public void CacheConfigBuilder_WithoutHeaders_Should_Disable_Header_Generation( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );

		// Act
		builder.Public( TimeSpan.FromMinutes( 10 ) ).WithoutHeaders( );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.GenerateHeaders.Should( ).BeFalse( );
		options.ShouldGenerateHeaders.Should( ).BeFalse( );
	}

	[Fact]
	public void CacheConfigBuilder_UseCache_With_Configure_Should_Apply_Configuration( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );

		// Act
		builder.UseCache( cache => cache
			.Public( TimeSpan.FromMinutes( 30 ) )
			.WithETag( obj => $"\"{obj.GetHashCode( )}\"" )
			.WithVary( "Accept-Language" )
		);

		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Enabled.Should( ).BeTrue( );
		options.Policy!.IsPublic.Should( ).BeTrue( );
		options.Policy.MaxAge.Should( ).Be( TimeSpan.FromMinutes( 30 ) );
		options.ETagGenerator.Should( ).NotBeNull( );
		options.VaryHeaders.Should( ).Contain( "Accept-Language" );
	}

	[Fact]
	public void CacheOptions_ShouldGenerateHeaders_Should_Return_True_When_Policy_Set( ) {
		// Arrange
		var options = new CacheOptions {
			Policy = CachePolicy.Public( TimeSpan.FromMinutes( 10 ) ),
			GenerateHeaders = true
		};

		// Act & Assert
		options.ShouldGenerateHeaders.Should( ).BeTrue( );
	}

	[Fact]
	public void CacheOptions_ShouldGenerateHeaders_Should_Return_False_When_Policy_Null( ) {
		// Arrange
		var options = new CacheOptions {
			Policy = null,
			GenerateHeaders = true
		};

		// Act & Assert
		options.ShouldGenerateHeaders.Should( ).BeFalse( );
	}

	[Fact]
	public void CacheOptions_ShouldGenerateHeaders_Should_Return_False_When_GenerateHeaders_False( ) {
		// Arrange
		var options = new CacheOptions {
			Policy = CachePolicy.Public( TimeSpan.FromMinutes( 10 ) ),
			GenerateHeaders = false
		};

		// Act & Assert
		options.ShouldGenerateHeaders.Should( ).BeFalse( );
	}
}
