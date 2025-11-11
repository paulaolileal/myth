using FluentAssertions;
using Myth.Flow.Actions.Test.Models;

namespace Myth.Flow.Actions.Test;

public class TypeRegistryTests {
	private readonly TypeRegistry _sut;

	public TypeRegistryTests( ) {
		_sut = new TypeRegistry( );
	}

	[Fact]
	public void Register_ShouldAddHandler( ) {
		// Arrange & Act
		_sut.Register( typeof( TestCommand ), typeof( TestCommandHandler ) );

		// Assert
		var handlers = _sut.GetHandlers( typeof( TestCommand ) );
		handlers.Should( ).Contain( typeof( TestCommandHandler ) );
	}

	[Fact]
	public void Register_MultipleTimes_ShouldNotDuplicate( ) {
		// Arrange & Act
		_sut.Register( typeof( TestCommand ), typeof( TestCommandHandler ) );
		_sut.Register( typeof( TestCommand ), typeof( TestCommandHandler ) );

		// Assert
		var handlers = _sut.GetHandlers( typeof( TestCommand ) );
		handlers.Count( ).Should( ).Be( 1 );
	}

	[Fact]
	public void HasHandler_WhenRegistered_ShouldReturnTrue( ) {
		// Arrange
		_sut.Register( typeof( TestCommand ), typeof( TestCommandHandler ) );

		// Act
		var hasHandler = _sut.HasHandler( typeof( TestCommand ) );

		// Assert
		hasHandler.Should( ).BeTrue( );
	}

	[Fact]
	public void GetAllRegistrations_ShouldReturnAllPairs( ) {
		// Arrange
		_sut.Register( typeof( TestCommand ), typeof( TestCommandHandler ) );
		_sut.Register( typeof( TestQuery ), typeof( TestQueryHandler ) );

		// Act
		var registrations = _sut.GetAllRegistrations( ).ToList( );

		// Assert
		registrations.Should( ).HaveCount( 2 );
	}
}
