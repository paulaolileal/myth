using FluentAssertions;
using Myth.Interfaces;
using Myth.Models;
using Xunit;

namespace Myth.Flow.Actions.Test;

public class CacheDataCachingTests {

	[Fact]
	public void CacheConfigBuilder_UseCache_Should_Enable_Cache( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );

		// Act
		builder.UseCache( );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Enabled.Should( ).BeTrue( );
		options.Ttl.Should( ).Be( TimeSpan.FromMinutes( 5 ) ); // Default TTL
	}

	[Fact]
	public void CacheConfigBuilder_UseCache_WithKey_Should_Set_Key( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		var key = "custom-cache-key";

		// Act
		builder.UseCache( key );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Enabled.Should( ).BeTrue( );
		options.CacheKey.Should( ).Be( key );
	}

	[Fact]
	public void CacheConfigBuilder_UseCache_WithKeyAndTtl_Should_Set_Both( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		var key = "custom-key";
		var ttl = TimeSpan.FromMinutes( 10 );

		// Act
		builder.UseCache( key, ttl );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Enabled.Should( ).BeTrue( );
		options.CacheKey.Should( ).Be( key );
		options.Ttl.Should( ).Be( ttl );
	}

	[Fact]
	public void CacheConfigBuilder_WithTtl_Should_Set_TimeToLive( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		var ttl = TimeSpan.FromHours( 2 );

		// Act
		builder.UseCache( ).WithTtl( ttl );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Enabled.Should( ).BeTrue( );
		options.Ttl.Should( ).Be( ttl );
	}

	[Fact]
	public void CacheConfigBuilder_WithKey_Should_Set_Cache_Key( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		var key = "my-custom-key";

		// Act
		builder.UseCache( ).WithKey( key );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Enabled.Should( ).BeTrue( );
		options.CacheKey.Should( ).Be( key );
	}

	[Fact]
	public void CacheConfigBuilder_WithSlidingExpiration_Should_Enable_Sliding( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );

		// Act
		builder.UseCache( ).WithSlidingExpiration( true );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Enabled.Should( ).BeTrue( );
		options.SlidingExpiration.Should( ).BeTrue( );
	}

	[Fact]
	public void CacheConfigBuilder_UseCache_With_Configure_Should_Apply_Configuration( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );

		// Act
		builder.UseCache( cache => cache
			.WithKey( "configured-key" )
			.WithTtl( TimeSpan.FromMinutes( 30 ) )
			.WithSlidingExpiration( true )
		);

		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Enabled.Should( ).BeTrue( );
		options.CacheKey.Should( ).Be( "configured-key" );
		options.Ttl.Should( ).Be( TimeSpan.FromMinutes( 30 ) );
		options.SlidingExpiration.Should( ).BeTrue( );
	}

	[Fact]
	public void CacheConfigBuilder_NotEnabled_Should_Return_Null( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );

		// Act
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).BeNull( );
	}

	[Fact]
	public void CacheOptions_DefaultTtl_Should_Be_Five_Minutes( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );

		// Act
		builder.UseCache( );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Ttl.Should( ).Be( TimeSpan.FromMinutes( 5 ) );
	}

	[Fact]
	public void CacheConfigBuilder_KeyGenerator_Should_Be_Set( ) {
		// Arrange
		var builder = new CacheConfigBuilder( );
		Func<object, string> keyGen = obj => $"key-{obj.GetHashCode( )}";

		// Act
		builder.UseCache( keyGen );
		var options = builder.ToCacheOptions( );

		// Assert
		options.Should( ).NotBeNull( );
		options!.Enabled.Should( ).BeTrue( );
		options.KeyGenerator.Should( ).NotBeNull( );

		// Test that the key generator works correctly
		var testObj = new { Id = 123, Name = "Test" };
		var expectedKey = keyGen( testObj );
		var actualKey = options.KeyGenerator!( testObj );
		actualKey.Should( ).Be( expectedKey );
	}
}
