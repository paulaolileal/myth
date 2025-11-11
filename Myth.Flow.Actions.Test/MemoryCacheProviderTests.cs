using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Myth.Models;
using NSubstitute;

namespace Myth.Flow.Actions.Test;

public class MemoryCacheProviderTests : IDisposable {
	private readonly IMemoryCache _memoryCache;
	private readonly ILogger<MemoryCacheProvider> _logger;
	private readonly MemoryCacheProvider _sut;

	public MemoryCacheProviderTests( ) {
		_memoryCache = new MemoryCache( new MemoryCacheOptions( ) );
		_logger = Substitute.For<ILogger<MemoryCacheProvider>>( );
		_sut = new MemoryCacheProvider( _memoryCache, _logger );
	}

	[Fact]
	public async Task GetAsync_WhenValueExists_ShouldReturnHit( ) {
		// Arrange
		await _sut.SetAsync( "key1", "value1", TimeSpan.FromMinutes( 1 ) );

		// Act
		var result = await _sut.GetAsync<string>( "key1" );

		// Assert
		result.HasValue.Should( ).BeTrue( );
		result.Value.Should( ).Be( "value1" );
	}

	[Fact]
	public async Task GetAsync_WhenValueDoesNotExist_ShouldReturnMiss( ) {
		// Arrange & Act
		var result = await _sut.GetAsync<string>( "nonexistent" );

		// Assert
		result.HasValue.Should( ).BeFalse( );
	}

	[Fact]
	public async Task SetAsync_ShouldStoreValue( ) {
		// Arrange & Act
		await _sut.SetAsync( "key2", 123, TimeSpan.FromMinutes( 1 ) );
		var result = await _sut.GetAsync<int>( "key2" );

		// Assert
		result.HasValue.Should( ).BeTrue( );
		result.Value.Should( ).Be( 123 );
	}

	[Fact]
	public async Task RemoveAsync_ShouldRemoveValue( ) {
		// Arrange
		await _sut.SetAsync( "key3", "value3", TimeSpan.FromMinutes( 1 ) );

		// Act
		await _sut.RemoveAsync( "key3" );
		var result = await _sut.GetAsync<string>( "key3" );

		// Assert
		result.HasValue.Should( ).BeFalse( );
	}

	[Fact]
	public async Task SetAsync_WithSlidingExpiration_ShouldUseSliding( ) {
		// Arrange & Act
		await _sut.SetAsync( "sliding", "value", TimeSpan.FromSeconds( 1 ), slidingExpiration: true );
		var result = await _sut.GetAsync<string>( "sliding" );

		// Assert
		result.HasValue.Should( ).BeTrue( );
	}

	public void Dispose( ) {
		_memoryCache?.Dispose( );
	}
}
