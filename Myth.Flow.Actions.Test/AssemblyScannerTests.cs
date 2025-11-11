using FluentAssertions;
using Myth.Builders;
using Myth.Flow.Actions.Test.Models;

namespace Myth.Flow.Actions.Test;

public class AssemblyScannerTests {
	private readonly AssemblyScanner _sut;

	public AssemblyScannerTests( ) {
		_sut = new AssemblyScanner( );
	}

	[Fact]
	public void ScanForHandlers_ShouldDiscoverCommandHandlers( ) {
		// Arrange
		var assembly = typeof( TestCommandHandler ).Assembly;

		// Act
		var handlers = _sut.ScanForHandlers( assembly ).ToList( );

		// Assert
		handlers.Should( ).NotBeEmpty( );
		handlers.Should( ).Contain( h => h.ImplementationType == typeof( TestCommandHandler ) );
	}

	[Fact]
	public void ScanForHandlers_ShouldDiscoverQueryHandlers( ) {
		// Arrange
		var assembly = typeof( TestQueryHandler ).Assembly;

		// Act
		var handlers = _sut.ScanForHandlers( assembly ).ToList( );

		// Assert
		handlers.Should( ).Contain( h => h.ImplementationType == typeof( TestQueryHandler ) );
	}

	[Fact]
	public void ScanForEventHandlers_ShouldDiscoverEventHandlers( ) {
		// Arrange
		var assembly = typeof( TestEventHandler ).Assembly;

		// Act
		var handlers = _sut.ScanForEventHandlers( assembly ).ToList( );

		// Assert
		handlers.Should( ).NotBeEmpty( );
		handlers.Should( ).Contain( h => h.HandlerType == typeof( TestEventHandler ) );
	}

	[Fact]
	public void ScanForHandlers_WithEmptyAssembly_ShouldReturnEmpty( ) {
		// Arrange
		var assembly = typeof( object ).Assembly;

		// Act
		var handlers = _sut.ScanForHandlers( assembly ).ToList( );

		// Assert
		handlers.Should( ).BeEmpty( );
	}
}
