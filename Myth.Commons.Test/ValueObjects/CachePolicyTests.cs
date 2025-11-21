using FluentAssertions;
using Myth.ValueObjects;
using Xunit;

namespace Myth.ValueObjects;

public class CachePolicyTests {

	[Fact]
	public void CachePolicy_Add_Should_Include_Directive( ) {
		// Arrange
		var policy = new CachePolicy( );

		// Act
		policy.Add( CacheDirective.Public );

		// Assert
		policy.Directives.Should( ).Contain( CacheDirective.Public );
		policy.IsPublic.Should( ).BeTrue( );
	}

	[Fact]
	public void CachePolicy_WithMaxAge_Should_Set_MaxAge_And_Add_Directive( ) {
		// Arrange
		var policy = new CachePolicy( );
		var duration = TimeSpan.FromMinutes( 15 );

		// Act
		policy.WithMaxAge( duration );

		// Assert
		policy.MaxAge.Should( ).Be( duration );
		policy.Directives.Should( ).Contain( d => d.Value == "max-age=900" );
	}

	[Fact]
	public void CachePolicy_ToHeaderValue_Should_Generate_Correct_String( ) {
		// Arrange
		var policy = new CachePolicy( )
			.Add( CacheDirective.Public )
			.WithMaxAge( TimeSpan.FromMinutes( 30 ) )
			.Add( CacheDirective.MustRevalidate );

		// Act
		var headerValue = policy.ToHeaderValue( );

		// Assert
		headerValue.Should( ).Be( "public, max-age=1800, must-revalidate" );
	}

	[Fact]
	public void CachePolicy_NoCache_Should_Create_NoCache_Policy( ) {
		// Arrange & Act
		var policy = CachePolicy.NoCache;

		// Assert
		policy.IsNoCache.Should( ).BeTrue( );
		policy.ToHeaderValue( ).Should( ).Be( "no-cache" );
	}

	[Fact]
	public void CachePolicy_NoStore_Should_Create_NoStore_Policy( ) {
		// Arrange & Act
		var policy = CachePolicy.NoStore;

		// Assert
		policy.ToHeaderValue( ).Should( ).Be( "no-store" );
	}

	[Fact]
	public void CachePolicy_Public_Should_Create_Public_Policy_With_MaxAge( ) {
		// Arrange
		var duration = TimeSpan.FromHours( 1 );

		// Act
		var policy = CachePolicy.Public( duration );

		// Assert
		policy.IsPublic.Should( ).BeTrue( );
		policy.MaxAge.Should( ).Be( duration );
		policy.ToHeaderValue( ).Should( ).Be( "public, max-age=3600" );
	}

	[Fact]
	public void CachePolicy_Private_Should_Create_Private_Policy_With_MaxAge( ) {
		// Arrange
		var duration = TimeSpan.FromMinutes( 10 );

		// Act
		var policy = CachePolicy.Private( duration );

		// Assert
		policy.IsPrivate.Should( ).BeTrue( );
		policy.MaxAge.Should( ).Be( duration );
		policy.ToHeaderValue( ).Should( ).Be( "private, max-age=600" );
	}

	[Fact]
	public void CachePolicy_Immutable_Should_Create_Immutable_Policy( ) {
		// Arrange
		var duration = TimeSpan.FromDays( 365 );

		// Act
		var policy = CachePolicy.Immutable( duration );

		// Assert
		policy.IsPublic.Should( ).BeTrue( );
		policy.MaxAge.Should( ).Be( duration );
		policy.ToHeaderValue( ).Should( ).Be( "public, immutable, max-age=31536000" );
	}

	[Fact]
	public void CachePolicy_Should_Support_Method_Chaining( ) {
		// Arrange & Act
		var policy = new CachePolicy( )
			.Add( CacheDirective.Public )
			.Add( CacheDirective.MustRevalidate )
			.WithMaxAge( TimeSpan.FromMinutes( 5 ) );

		// Assert
		policy.Directives.Should( ).HaveCount( 3 ); // public, must-revalidate, max-age
		policy.IsPublic.Should( ).BeTrue( );
		policy.MaxAge.Should( ).Be( TimeSpan.FromMinutes( 5 ) );
	}

	[Fact]
	public void CachePolicy_Add_Should_Track_MaxAge_From_MaxAge_Directive( ) {
		// Arrange
		var policy = new CachePolicy( );

		// Act
		policy.Add( CacheDirective.MaxAge( 1800 ) );

		// Assert
		policy.MaxAge.Should( ).Be( TimeSpan.FromSeconds( 1800 ) );
	}

	[Fact]
	public void CachePolicy_ToString_Should_Return_HeaderValue( ) {
		// Arrange
		var policy = CachePolicy.Public( TimeSpan.FromMinutes( 15 ) );

		// Act
		var result = policy.ToString( );

		// Assert
		result.Should( ).Be( "public, max-age=900" );
	}
}
