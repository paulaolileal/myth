using FluentAssertions;
using Myth.Interfaces;
using Myth.Morph;

namespace Myth.Morph.Test.SourceGeneration;

/// <summary>
/// Unit tests for <see cref="GeneratedBootstrapRegistry"/>.
/// Verifies registration, retrieval, and the <see cref="GeneratedBootstrapRegistry.HasBootstraps"/> sentinel.
/// </summary>
public sealed class GeneratedBootstrapRegistryTests : IDisposable {

    public void Dispose( ) => GeneratedBootstrapRegistry.Clear( );

    // ─── HasBootstraps ───────────────────────────────────────────────────────────

    [Fact]
    public void HasBootstraps_ShouldBeFalse_WhenNoBootstrapRegistered( ) {
        GeneratedBootstrapRegistry.HasBootstraps.Should( ).BeFalse( );
    }

    [Fact]
    public void HasBootstraps_ShouldBeTrue_AfterAtLeastOneRegistration( ) {
        GeneratedBootstrapRegistry.Register( new FakeBootstrap( ) );
        GeneratedBootstrapRegistry.HasBootstraps.Should( ).BeTrue( );
    }

    // ─── Register / GetAll ───────────────────────────────────────────────────────

    [Fact]
    public void GetAll_ShouldReturn_AllRegisteredBootstraps( ) {
        var first = new FakeBootstrap( );
        var second = new FakeBootstrap( );

        GeneratedBootstrapRegistry.Register( first );
        GeneratedBootstrapRegistry.Register( second );

        var all = GeneratedBootstrapRegistry.GetAll( );

        all.Should( ).HaveCount( 2 );
        all.Should( ).Contain( first );
        all.Should( ).Contain( second );
    }

    [Fact]
    public void Register_WithNullBootstrap_ShouldThrowArgumentNullException( ) {
        var act = ( ) => GeneratedBootstrapRegistry.Register( null! );
        act.Should( ).Throw<ArgumentNullException>( );
    }

    [Fact]
    public void HasBootstraps_ShouldBeFalse_AfterClear( ) {
        GeneratedBootstrapRegistry.Register( new FakeBootstrap( ) );
        GeneratedBootstrapRegistry.Clear( );

        GeneratedBootstrapRegistry.HasBootstraps.Should( ).BeFalse( );
    }

    [Fact]
    public void GetAll_ShouldReturnReadOnlyCollection_ThatDoesNotReflectLaterChanges( ) {
        GeneratedBootstrapRegistry.Register( new FakeBootstrap( ) );
        var snapshot = GeneratedBootstrapRegistry.GetAll( );

        GeneratedBootstrapRegistry.Register( new FakeBootstrap( ) );

        snapshot.Should( ).HaveCount( 1, "GetAll returns a snapshot, not a live view" );
    }

    // ─── Test helpers ────────────────────────────────────────────────────────────

    private sealed class FakeBootstrap : IMorphRegistrationBootstrap {
        public void Register( SchemaRegistry registry ) { }
    }
}
