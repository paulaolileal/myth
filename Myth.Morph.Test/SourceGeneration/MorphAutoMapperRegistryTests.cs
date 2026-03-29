using FluentAssertions;
using Myth.Interfaces;
using Myth.Morph;

namespace Myth.Morph.Test.SourceGeneration;

/// <summary>
/// Unit tests for <see cref="MorphAutoMapperRegistry"/>.
/// Verifies registration, retrieval, and the not-found fallback path.
/// </summary>
public sealed class MorphAutoMapperRegistryTests : IDisposable {

	public void Dispose( ) => MorphAutoMapperRegistry.Clear( );

	// ─── Register / TryGet ───────────────────────────────────────────────────────

	[Fact]
	public void Register_ShouldStore_Mapper_And_TryGet_ShouldRetrieveIt( ) {
		var mapper = new FakeAutoMapper( typeof( FakeSource ), typeof( FakeDestination ) );

		MorphAutoMapperRegistry.Register( mapper );

		var found = MorphAutoMapperRegistry.TryGet( typeof( FakeSource ), typeof( FakeDestination ), out var retrieved );

		found.Should( ).BeTrue( );
		retrieved.Should( ).BeSameAs( mapper );
	}

	[Fact]
	public void TryGet_ShouldReturnFalse_WhenNoPairRegistered( ) {
		var found = MorphAutoMapperRegistry.TryGet( typeof( FakeSource ), typeof( FakeDestination ), out var retrieved );

		found.Should( ).BeFalse( );
		retrieved.Should( ).BeNull( );
	}

	[Fact]
	public void Register_WithNullMapper_ShouldThrowArgumentNullException( ) {
		var act = ( ) => MorphAutoMapperRegistry.Register( null! );
		act.Should( ).Throw<ArgumentNullException>( );
	}

	[Fact]
	public void Register_ShouldOverwrite_PreviousMapper_ForSamePair( ) {
		var first = new FakeAutoMapper( typeof( FakeSource ), typeof( FakeDestination ) );
		var second = new FakeAutoMapper( typeof( FakeSource ), typeof( FakeDestination ) );

		MorphAutoMapperRegistry.Register( first );
		MorphAutoMapperRegistry.Register( second );

		MorphAutoMapperRegistry.TryGet( typeof( FakeSource ), typeof( FakeDestination ), out var retrieved );
		retrieved.Should( ).BeSameAs( second );
	}

	[Fact]
	public void TryGet_ShouldReturnFalse_AfterClear( ) {
		var mapper = new FakeAutoMapper( typeof( FakeSource ), typeof( FakeDestination ) );
		MorphAutoMapperRegistry.Register( mapper );
		MorphAutoMapperRegistry.Clear( );

		var found = MorphAutoMapperRegistry.TryGet( typeof( FakeSource ), typeof( FakeDestination ), out _ );
		found.Should( ).BeFalse( );
	}

	[Fact]
	public void TryGet_ShouldReturn_CorrectMapper_ForMultipleRegisteredPairs( ) {
		var mapperAB = new FakeAutoMapper( typeof( FakeSource ), typeof( FakeDestination ) );
		var mapperBA = new FakeAutoMapper( typeof( FakeDestination ), typeof( FakeSource ) );

		MorphAutoMapperRegistry.Register( mapperAB );
		MorphAutoMapperRegistry.Register( mapperBA );

		MorphAutoMapperRegistry.TryGet( typeof( FakeSource ), typeof( FakeDestination ), out var ab );
		MorphAutoMapperRegistry.TryGet( typeof( FakeDestination ), typeof( FakeSource ), out var ba );

		ab.Should( ).BeSameAs( mapperAB );
		ba.Should( ).BeSameAs( mapperBA );
	}

	// ─── Test helpers ────────────────────────────────────────────────────────────

	private sealed class FakeSource { }
	private sealed class FakeDestination { }

	private sealed class FakeAutoMapper( Type sourceType, Type destinationType ) : IMorphAutoMapper {
		public Type SourceType { get; } = sourceType;
		public Type DestinationType { get; } = destinationType;
		public void MapProperties( object source, object destination, HashSet<string> skip ) { }
	}
}
