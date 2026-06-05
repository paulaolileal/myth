using FluentAssertions;
using Microsoft.Extensions.Logging;
using Myth.Interfaces;
using Myth.Models;
using NSubstitute;

namespace Myth.Flow.Actions.Test;

public class CacheManagerTests {
	private readonly ICacheProvider _cacheProvider;
	private readonly ILogger<CacheManager> _logger;
	private readonly ICacheManager _sut;

	public CacheManagerTests( ) {
		_cacheProvider = Substitute.For<ICacheProvider>( );
		_logger = Substitute.For<ILogger<CacheManager>>( );
		_sut = new CacheManager( _cacheProvider, _logger );
	}

	[Fact]
	public async Task GetAsync_WhenValueExists_ShouldReturnValue( ) {
		// Arrange
		var expectedValue = "test-value";
		_cacheProvider.GetAsync<string>( "test-key", Arg.Any<CancellationToken>( ) )
			.Returns( CacheValue<string>.Hit( expectedValue ) );

		// Act
		var result = await _sut.GetAsync<string>( "test-key" );

		// Assert
		result.Should( ).Be( expectedValue );
	}

	[Fact]
	public async Task GetAsync_WhenValueDoesNotExist_ShouldReturnNull( ) {
		// Arrange
		_cacheProvider.GetAsync<string>( "nonexistent", Arg.Any<CancellationToken>( ) )
			.Returns( CacheValue<string>.Miss( ) );

		// Act
		var result = await _sut.GetAsync<string>( "nonexistent" );

		// Assert
		result.Should( ).BeNull( );
	}

	[Fact]
	public async Task SetAsync_ShouldDelegateToProvider( ) {
		// Arrange
		var key = "test-key";
		var value = "test-value";
		var ttl = TimeSpan.FromMinutes( 5 );

		// Act
		await _sut.SetAsync( key, value, ttl );

		// Assert
		await _cacheProvider.Received( 1 ).SetAsync(
			key,
			value,
			ttl,
			false,
			Arg.Any<CancellationToken>( ) );
	}

	[Fact]
	public async Task SetAsync_WithSlidingExpiration_ShouldPassSlidingFlag( ) {
		// Arrange
		var key = "test-key";
		var value = "test-value";
		var ttl = TimeSpan.FromMinutes( 5 );

		// Act
		await _sut.SetAsync( key, value, ttl, slidingExpiration: true );

		// Assert
		await _cacheProvider.Received( 1 ).SetAsync(
			key,
			value,
			ttl,
			true,
			Arg.Any<CancellationToken>( ) );
	}

	[Fact]
	public async Task InvalidateAsync_ShouldDelegateToProvider( ) {
		// Arrange
		var key = "test-key";

		// Act
		await _sut.InvalidateAsync( key );

		// Assert
		await _cacheProvider.Received( 1 ).RemoveAsync( key, Arg.Any<CancellationToken>( ) );
	}

	[Fact]
	public async Task InvalidateByPatternAsync_ShouldDelegateToProvider( ) {
		// Arrange
		var pattern = "User:*";

		// Act
		await _sut.InvalidateByPatternAsync( pattern );

		// Assert
		await _cacheProvider.Received( 1 ).RemoveByPatternAsync( pattern, Arg.Any<CancellationToken>( ) );
	}

	[Fact]
	public async Task InvalidateByTypeAsync_WithQueryInstance_ShouldInvalidateSpecificQuery( ) {
		// Arrange
		var query = new TestQuery { Id = 123, Name = "Test" };

		// Act
		await _sut.InvalidateByTypeAsync( query );

		// Assert
		await _cacheProvider.Received( 1 ).RemoveAsync(
			Arg.Is<string>( key => key.StartsWith( "TestQuery:" ) ),
			Arg.Any<CancellationToken>( ) );
	}

	[Fact]
	public async Task InvalidateByTypeAsync_WithoutQueryInstance_ShouldInvalidateAllQueriesOfType( ) {
		// Arrange & Act
		await _sut.InvalidateByTypeAsync<TestQuery>( );

		// Assert
		await _cacheProvider.Received( 1 ).RemoveByPatternAsync(
			"TestQuery:*",
			Arg.Any<CancellationToken>( ) );
	}

	[Fact]
	public void GenerateKey_ShouldGenerateConsistentKey( ) {
		// Arrange
		var query1 = new TestQuery { Id = 123, Name = "Test" };
		var query2 = new TestQuery { Id = 123, Name = "Test" };

		// Act
		var key1 = _sut.GenerateKey( query1 );
		var key2 = _sut.GenerateKey( query2 );

		// Assert
		key1.Should( ).Be( key2 );
		key1.Should( ).StartWith( "TestQuery:" );
	}

	[Fact]
	public void GenerateKey_WithDifferentValues_ShouldGenerateDifferentKeys( ) {
		// Arrange
		var query1 = new TestQuery { Id = 123, Name = "Test1" };
		var query2 = new TestQuery { Id = 123, Name = "Test2" };

		// Act
		var key1 = _sut.GenerateKey( query1 );
		var key2 = _sut.GenerateKey( query2 );

		// Assert
		key1.Should( ).NotBe( key2 );
	}

	[Fact]
	public void GenerateKey_ShouldIncludeTypeName( ) {
		// Arrange
		var query = new TestQuery { Id = 123, Name = "Test" };

		// Act
		var key = _sut.GenerateKey( query );

		// Assert
		key.Should( ).StartWith( "TestQuery:" );
	}

	// Helper class for testing
	private class TestQuery {
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}
