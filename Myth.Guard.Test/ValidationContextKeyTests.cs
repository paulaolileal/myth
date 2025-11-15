using FluentAssertions;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for ValidationContextKey record struct
/// </summary>
public class ValidationContextKeyTests {

	[Fact]
	public void Default_ShouldHaveCorrectValue( ) {
		// Act
		var defaultContext = ValidationContextKey.Default;

		// Assert
		defaultContext.Key.Should( ).Be( "Default" );
	}

	[Fact]
	public void Create_ShouldHaveCorrectValue( ) {
		// Act
		var createContext = ValidationContextKey.Create;

		// Assert
		createContext.Key.Should( ).Be( "Create" );
	}

	[Fact]
	public void Update_ShouldHaveCorrectValue( ) {
		// Act
		var updateContext = ValidationContextKey.Update;

		// Assert
		updateContext.Key.Should( ).Be( "Update" );
	}

	[Fact]
	public void Delete_ShouldHaveCorrectValue( ) {
		// Act
		var deleteContext = ValidationContextKey.Delete;

		// Assert
		deleteContext.Key.Should( ).Be( "Delete" );
	}

	[Fact]
	public void GetByField_ShouldHaveCorrectValue( ) {
		// Act
		var getByFieldContext = ValidationContextKey.GetByField;

		// Assert
		getByFieldContext.Key.Should( ).Be( "GetByField" );
	}

	[Fact]
	public void GetAll_ShouldHaveCorrectValue( ) {
		// Act
		var getAllContext = ValidationContextKey.GetAll;

		// Assert
		getAllContext.Key.Should( ).Be( "GetAll" );
	}

	[Fact]
	public void Search_ShouldHaveCorrectValue( ) {
		// Act
		var searchContext = ValidationContextKey.Search;

		// Assert
		searchContext.Key.Should( ).Be( "Search" );
	}

	[Fact]
	public void Activate_ShouldHaveCorrectValue( ) {
		// Act
		var activateContext = ValidationContextKey.Activate;

		// Assert
		activateContext.Key.Should( ).Be( "Activate" );
	}

	[Fact]
	public void Deactivate_ShouldHaveCorrectValue( ) {
		// Act
		var deactivateContext = ValidationContextKey.Deactivate;

		// Assert
		deactivateContext.Key.Should( ).Be( "Deactivate" );
	}

	[Theory]
	[InlineData( "CustomOperation" )]
	[InlineData( "SpecialValidation" )]
	[InlineData( "BulkImport" )]
	[InlineData( "" )]
	public void Custom_ShouldCreateContextWithCustomKey( string customKey ) {
		// Act
		var customContext = ValidationContextKey.Custom( customKey );

		// Assert
		customContext.Key.Should( ).Be( customKey );
	}

	[Fact]
	public void Equality_WithSameKey_ShouldBeEqual( ) {
		// Arrange
		var context1 = ValidationContextKey.Custom( "TestKey" );
		var context2 = ValidationContextKey.Custom( "TestKey" );

		// Assert
		context1.Should( ).Be( context2 );
		context1.Equals( context2 ).Should( ).BeTrue( );
		( context1 == context2 ).Should( ).BeTrue( );
		( context1 != context2 ).Should( ).BeFalse( );
	}

	[Fact]
	public void Equality_WithDifferentKeys_ShouldNotBeEqual( ) {
		// Arrange
		var context1 = ValidationContextKey.Custom( "Key1" );
		var context2 = ValidationContextKey.Custom( "Key2" );

		// Assert
		context1.Should( ).NotBe( context2 );
		context1.Equals( context2 ).Should( ).BeFalse( );
		( context1 == context2 ).Should( ).BeFalse( );
		( context1 != context2 ).Should( ).BeTrue( );
	}

	[Fact]
	public void GetHashCode_WithSameKey_ShouldBeEqual( ) {
		// Arrange
		var context1 = ValidationContextKey.Custom( "TestKey" );
		var context2 = ValidationContextKey.Custom( "TestKey" );

		// Assert
		context1.GetHashCode( ).Should( ).Be( context2.GetHashCode( ) );
	}

	[Fact]
	public void GetHashCode_WithDifferentKeys_ShouldBeDifferent( ) {
		// Arrange
		var context1 = ValidationContextKey.Custom( "Key1" );
		var context2 = ValidationContextKey.Custom( "Key2" );

		// Assert
		context1.GetHashCode( ).Should( ).NotBe( context2.GetHashCode( ) );
	}

	[Fact]
	public void ToString_ShouldReturnKey( ) {
		// Arrange
		var context = ValidationContextKey.Custom( "MyCustomContext" );

		// Act
		var toString = context.ToString( );

		// Assert
		toString.Should( ).Be( "MyCustomContext" );
	}

	[Fact]
	public void ToString_WithNullKey_ShouldReturnEmptyString( ) {
		// Arrange
		Action act = ( ) => ValidationContextKey.Custom( null );

		// Act & Assert
		act.Should( ).Throw<ArgumentNullException>( );
	}

	[Fact]
	public void PredefinedContexts_ShouldBeDistinct( ) {
		// Arrange
		var contexts = new[ ]
		{
			ValidationContextKey.Default,
			ValidationContextKey.Create,
			ValidationContextKey.Update,
			ValidationContextKey.Delete,
			ValidationContextKey.GetByField,
			ValidationContextKey.GetAll,
			ValidationContextKey.Search,
			ValidationContextKey.Activate,
			ValidationContextKey.Deactivate
		};

		// Assert
		contexts.Should( ).OnlyHaveUniqueItems( );
		contexts.Select( c => c.Key ).Should( ).OnlyHaveUniqueItems( );
	}

	[Fact]
	public void ImplicitConversion_FromString_ShouldWork( ) {
		// Arrange
		const string customKey = "ImplicitTest";

		// Act
		ValidationContextKey context = customKey;

		// Assert
		context.Key.Should( ).Be( customKey );
	}

	[Fact]
	public void Deconstruct_ShouldExtractKey( ) {
		// Arrange
		var context = ValidationContextKey.Custom( "DeconstructTest" );

		// Act
		var key = context.Key;

		// Assert
		key.Should( ).Be( "DeconstructTest" );
	}
}
