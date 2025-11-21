using FluentAssertions;
using Myth.Flow.Actions.ValueObjects;
using Myth.Models;
using Xunit;

namespace Myth.Flow.Actions.Test;

public class CacheMetadataTests {

	[Fact]
	public void CacheMetadata_ToHeaders_Should_Generate_Cache_Control_Header( ) {
		// Arrange
		var metadata = new CacheMetadata {
			Policy = CachePolicy.Public( TimeSpan.FromMinutes( 15 ) )
		};

		// Act
		var headers = metadata.ToHeaders( );

		// Assert
		headers.Should( ).ContainKey( "Cache-Control" );
		headers[ "Cache-Control" ].Should( ).Be( "public, max-age=900" );
	}

	[Fact]
	public void CacheMetadata_ToHeaders_Should_Generate_ETag_Header( ) {
		// Arrange
		var metadata = new CacheMetadata {
			ETag = "\"12345\""
		};

		// Act
		var headers = metadata.ToHeaders( );

		// Assert
		headers.Should( ).ContainKey( "ETag" );
		headers[ "ETag" ].Should( ).Be( "\"12345\"" );
	}

	[Fact]
	public void CacheMetadata_ToHeaders_Should_Generate_Expires_Header( ) {
		// Arrange
		var expiresAt = new DateTime( 2024, 1, 1, 12, 0, 0, DateTimeKind.Utc );
		var metadata = new CacheMetadata {
			ExpiresAt = expiresAt
		};

		// Act
		var headers = metadata.ToHeaders( );

		// Assert
		headers.Should( ).ContainKey( "Expires" );
		headers[ "Expires" ].Should( ).Be( expiresAt.ToString( "R" ) );
	}

	[Fact]
	public void CacheMetadata_ToHeaders_Should_Generate_Vary_Header( ) {
		// Arrange
		var metadata = new CacheMetadata {
			VaryHeaders = new[ ] { "Accept-Language", "Authorization" }
		};

		// Act
		var headers = metadata.ToHeaders( );

		// Assert
		headers.Should( ).ContainKey( "Vary" );
		headers[ "Vary" ].Should( ).Be( "Accept-Language, Authorization" );
	}

	[Fact]
	public void CacheMetadata_ToHeaders_Should_Generate_Age_Header( ) {
		// Arrange
		var metadata = new CacheMetadata {
			Age = TimeSpan.FromMinutes( 5 )
		};

		// Act
		var headers = metadata.ToHeaders( );

		// Assert
		headers.Should( ).ContainKey( "Age" );
		headers[ "Age" ].Should( ).Be( "300" );
	}

	[Fact]
	public void CacheMetadata_ToHeaders_Should_Generate_Last_Modified_Header( ) {
		// Arrange
		var lastModified = new DateTime( 2024, 1, 1, 10, 0, 0, DateTimeKind.Utc );
		var metadata = new CacheMetadata {
			LastModified = lastModified
		};

		// Act
		var headers = metadata.ToHeaders( );

		// Assert
		headers.Should( ).ContainKey( "Last-Modified" );
		headers[ "Last-Modified" ].Should( ).Be( lastModified.ToString( "R" ) );
	}

	[Fact]
	public void CacheMetadata_ToHeaders_Should_Generate_All_Headers_When_All_Set( ) {
		// Arrange
		var expiresAt = DateTime.UtcNow.AddMinutes( 15 );
		var lastModified = DateTime.UtcNow.AddHours( -1 );
		var metadata = new CacheMetadata {
			Policy = CachePolicy.Public( TimeSpan.FromMinutes( 15 ) ),
			ExpiresAt = expiresAt,
			ETag = "\"abcdef\"",
			LastModified = lastModified,
			VaryHeaders = new[ ] { "Accept-Language" },
			Age = TimeSpan.FromMinutes( 2 )
		};

		// Act
		var headers = metadata.ToHeaders( );

		// Assert
		headers.Should( ).HaveCount( 6 );
		headers.Should( ).ContainKeys( "Cache-Control", "Expires", "ETag", "Last-Modified", "Vary", "Age" );
	}

	[Fact]
	public void CacheMetadata_ShouldGenerateHeaders_Should_Return_True_When_Policy_Set( ) {
		// Arrange
		var metadata = new CacheMetadata {
			Policy = CachePolicy.Public( TimeSpan.FromMinutes( 10 ) )
		};

		// Act & Assert
		metadata.ShouldGenerateHeaders.Should( ).BeTrue( );
	}

	[Fact]
	public void CacheMetadata_ShouldGenerateHeaders_Should_Return_True_When_ETag_Set( ) {
		// Arrange
		var metadata = new CacheMetadata {
			ETag = "\"12345\""
		};

		// Act & Assert
		metadata.ShouldGenerateHeaders.Should( ).BeTrue( );
	}

	[Fact]
	public void CacheMetadata_ShouldGenerateHeaders_Should_Return_False_When_Nothing_Set( ) {
		// Arrange
		var metadata = new CacheMetadata( );

		// Act & Assert
		metadata.ShouldGenerateHeaders.Should( ).BeFalse( );
	}

	[Fact]
	public void CacheMetadata_Public_Factory_Should_Create_Public_Metadata( ) {
		// Arrange
		var duration = TimeSpan.FromMinutes( 30 );
		var etag = "\"test-etag\"";

		// Act
		var metadata = CacheMetadata.Public( duration, etag );

		// Assert
		metadata.Policy!.IsPublic.Should( ).BeTrue( );
		metadata.Policy.MaxAge.Should( ).Be( duration );
		metadata.ETag.Should( ).Be( etag );
		metadata.ExpiresAt.Should( ).BeCloseTo( DateTime.UtcNow.Add( duration ), TimeSpan.FromSeconds( 1 ) );
	}

	[Fact]
	public void CacheMetadata_Private_Factory_Should_Create_Private_Metadata( ) {
		// Arrange
		var duration = TimeSpan.FromMinutes( 10 );

		// Act
		var metadata = CacheMetadata.Private( duration );

		// Assert
		metadata.Policy!.IsPrivate.Should( ).BeTrue( );
		metadata.Policy.MaxAge.Should( ).Be( duration );
		metadata.ExpiresAt.Should( ).BeCloseTo( DateTime.UtcNow.Add( duration ), TimeSpan.FromSeconds( 1 ) );
	}

	[Fact]
	public void CacheMetadata_NoCache_Factory_Should_Create_NoCache_Metadata( ) {
		// Arrange & Act
		var metadata = CacheMetadata.NoCache( );

		// Assert
		metadata.Policy!.IsNoCache.Should( ).BeTrue( );
	}
}
