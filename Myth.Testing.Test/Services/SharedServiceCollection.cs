namespace Myth.Testing.Test.Services;

/// <summary>
/// Example of collection fixture for sharing across multiple test classes
/// </summary>
[CollectionDefinition( "Shared Service Collection" )]
public class SharedServiceCollection : ICollectionFixture<SharedServiceFixture> {
	// This class has no code, and is never created. Its purpose is simply
	// to be the place to apply [CollectionDefinition] and all the
	// ICollectionFixture<> interfaces.
}
