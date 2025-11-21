using FluentAssertions;
using Myth.ValueObjects;
using Xunit;

namespace Myth.ValueObjects;

public class CacheDirectiveTests {

	[Fact]
	public void CacheDirective_Constants_Should_Have_Correct_Values( ) {
		// Arrange & Act & Assert
		CacheDirective.Public.Value.Should( ).Be( "public" );
		CacheDirective.Private.Value.Should( ).Be( "private" );
		CacheDirective.NoCache.Value.Should( ).Be( "no-cache" );
		CacheDirective.NoStore.Value.Should( ).Be( "no-store" );
		CacheDirective.MustRevalidate.Value.Should( ).Be( "must-revalidate" );
		CacheDirective.Immutable.Value.Should( ).Be( "immutable" );
	}

	[Fact]
	public void CacheDirective_MaxAge_Should_Generate_Correct_Value_From_Seconds( ) {
		// Arrange
		var maxAge = CacheDirective.MaxAge( 3600 );

		// Act & Assert
		maxAge.Value.Should( ).Be( "max-age=3600" );
	}

	[Fact]
	public void CacheDirective_MaxAge_Should_Generate_Correct_Value_From_TimeSpan( ) {
		// Arrange
		var duration = TimeSpan.FromMinutes( 30 );
		var maxAge = CacheDirective.MaxAge( duration );

		// Act & Assert
		maxAge.Value.Should( ).Be( "max-age=1800" );
	}

	[Fact]
	public void CacheDirective_SMaxAge_Should_Generate_Correct_Value( ) {
		// Arrange
		var sMaxAge = CacheDirective.SMaxAge( 7200 );

		// Act & Assert
		sMaxAge.Value.Should( ).Be( "s-maxage=7200" );
	}

	[Fact]
	public void CacheDirective_Should_Support_Equality_Comparison( ) {
		// Arrange
		var directive1 = CacheDirective.Public;
		var directive2 = CacheDirective.Public;
		var directive3 = CacheDirective.Private;

		// Act & Assert
		directive1.Should( ).Be( directive2 );
		directive1.Should( ).NotBe( directive3 );
	}

	[Fact]
	public void CacheDirective_Should_Support_Implicit_Conversion_To_String( ) {
		// Arrange
		var directive = CacheDirective.Public;

		// Act
		string value = directive;

		// Assert
		value.Should( ).Be( "public" );
	}

	[Fact]
	public void CacheDirective_GetAll_Should_Return_All_Predefined_Constants( ) {
		// Arrange & Act
		var allDirectives = CacheDirective.GetAll( );

		// Assert
		allDirectives.Should( ).Contain( CacheDirective.Public );
		allDirectives.Should( ).Contain( CacheDirective.Private );
		allDirectives.Should( ).Contain( CacheDirective.NoCache );
		allDirectives.Should( ).Contain( CacheDirective.NoStore );
		allDirectives.Should( ).Contain( CacheDirective.MustRevalidate );
		allDirectives.Should( ).Contain( CacheDirective.Immutable );
	}

	[Fact]
	public void CacheDirective_FromValue_Should_Find_Correct_Directive( ) {
		// Arrange & Act
		var directive = CacheDirective.FromValue( "public" );

		// Assert
		directive.Should( ).Be( CacheDirective.Public );
	}

	[Fact]
	public void CacheDirective_FromName_Should_Find_Correct_Directive( ) {
		// Arrange & Act
		var directive = CacheDirective.FromName( "Public" );

		// Assert
		directive.Should( ).Be( CacheDirective.Public );
	}
}
