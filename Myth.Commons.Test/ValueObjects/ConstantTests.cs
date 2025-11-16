using FluentAssertions;
using Myth.Commons.Test.Models;
using Myth.ValueObjects;

namespace Myth.Commons.Test.ValueObjects;

public class ConstantTests {

	[Fact]
	public void GetAll_ShouldReturnAllInstances( ) {
		// Act
		var instances = ConstantMock.GetAll( );

		// Assert
		instances.Should( ).HaveCount( 2 );
		instances.Should( ).Contain( ConstantMock.One );
		instances.Should( ).Contain( ConstantMock.Two );
	}

	[Fact]
	public void GetOptions_ShouldReturnFormattedString( ) {
		// Act
		var options = ConstantMock.GetOptions( );

		// Assert
		options.Should( ).Contain( $"{ConstantMock.One.Value}: {ConstantMock.One.Name}" );
		options.Should( ).Contain( $"{ConstantMock.Two.Value}: {ConstantMock.Two.Name}" );
	}

	[Fact]
	public void FromValue_WithValidValue_ShouldReturnCorrectInstance( ) {
		// Act
		var result = ConstantMock.FromValue( 1 );

		// Assert
		result.Should( ).Be( ConstantMock.One );
		result.Name.Should( ).Be( "One" );
		result.Value.Should( ).Be( 1 );
	}

	[Fact]
	public void FromValue_WithInvalidValue_ShouldThrowException( ) {
		// Act
		Action act = ( ) => ConstantMock.FromValue( 999 );

		// Assert
		act.Should( ).Throw<ConstantNotFoundException>( )
			.WithMessage( "No ConstantMock with value '999' found" );
	}

	[Fact]
	public void FromName_WithValidName_ShouldReturnCorrectInstance( ) {
		// Act
		var result = ConstantMock.FromName( "Two" );

		// Assert
		result.Should( ).Be( ConstantMock.Two );
		result.Name.Should( ).Be( "Two" );
		result.Value.Should( ).Be( 2 );
	}

	[Fact]
	public void FromName_WithInvalidName_ShouldThrowException( ) {
		// Act
		Action act = ( ) => ConstantMock.FromName( "Invalid" );

		// Assert
		act.Should( ).Throw<ConstantNotFoundException>( )
			.WithMessage( "No ConstantMock with name 'Invalid' found" );
	}

	[Fact]
	public void TryFromValue_WithValidValue_ShouldReturnTrueAndCorrectInstance( ) {
		// Act
		var success = ConstantMock.TryFromValue( 1, out var result );

		// Assert
		success.Should( ).BeTrue( );
		result.Should( ).Be( ConstantMock.One );
	}

	[Fact]
	public void TryFromValue_WithInvalidValue_ShouldReturnFalse( ) {
		// Act
		var success = ConstantMock.TryFromValue( 999, out var result );

		// Assert
		success.Should( ).BeFalse( );
		result.Should( ).BeNull( );
	}

	[Fact]
	public void TryFromName_WithValidName_ShouldReturnTrueAndCorrectInstance( ) {
		// Act
		var success = ConstantMock.TryFromName( "Two", out var result );

		// Assert
		success.Should( ).BeTrue( );
		result.Should( ).Be( ConstantMock.Two );
	}

	[Fact]
	public void TryFromName_WithInvalidName_ShouldReturnFalse( ) {
		// Act
		var success = ConstantMock.TryFromName( "Invalid", out var result );

		// Assert
		success.Should( ).BeFalse( );
		result.Should( ).BeNull( );
	}

	[Fact]
	public void ImplicitConversion_ToValue_ShouldWork( ) {
		// Act
		int value = ConstantMock.One;

		// Assert
		value.Should( ).Be( 1 );
	}

	[Fact]
	public void Name_Property_ShouldReturnCorrectValue( ) {
		// Act
		string name = ConstantMock.One.Name;

		// Assert
		name.Should( ).Be( "One" );
	}

	[Fact]
	public void All_Property_ShouldAllowEasyIteration( ) {
		// Act
		var names = new List<string>( );
		foreach ( var constant in ConstantMock.All ) {
			names.Add( constant.Name );
		}

		// Assert
		names.Should( ).Contain( "One" );
		names.Should( ).Contain( "Two" );
		names.Should( ).HaveCount( 2 );
	}

	[Fact]
	public void Values_Property_ShouldReturnAllValues( ) {
		// Act
		var values = ConstantMock.Values.All.ToList( );

		// Assert
		values.Should( ).Contain( 1 );
		values.Should( ).Contain( 2 );
		values.Should( ).HaveCount( 2 );
	}

	[Fact]
	public void Equality_ShouldWorkCorrectly( ) {
		// Arrange
		var constant1 = ConstantMock.One;
		var constant2 = ConstantMock.FromValue( 1 );

		// Act & Assert
		constant1.Should( ).Be( constant2 );
		( constant1 == constant2 ).Should( ).BeTrue( );
		( constant1 != ConstantMock.Two ).Should( ).BeTrue( );
	}

	[Fact]
	public void Comparison_ShouldWorkCorrectly( ) {
		// Act & Assert
		( ConstantMock.One < ConstantMock.Two ).Should( ).BeTrue( );
		( ConstantMock.Two > ConstantMock.One ).Should( ).BeTrue( );
		ConstantMock.One.CompareTo( ConstantMock.Two ).Should( ).BeLessThan( 0 );
	}

	[Fact]
	public void ToString_ShouldReturnName( ) {
		// Act
		var stringValue = ConstantMock.One.ToString( );

		// Assert
		stringValue.Should( ).Be( "One" );
	}

	[Fact]
	public void GetHashCode_ShouldBeConsistent( ) {
		// Act
		var hash1 = ConstantMock.One.GetHashCode( );
		var hash2 = ConstantMock.FromValue( 1 ).GetHashCode( );

		// Assert
		hash1.Should( ).Be( hash2 );
	}
}
