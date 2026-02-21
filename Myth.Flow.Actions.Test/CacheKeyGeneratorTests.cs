using FluentAssertions;
using Myth.Models;

namespace Myth.Flow.Actions.Test;

public class CacheKeyGeneratorTests {

	[Fact]
	public void Generate_WithSameQueryValues_ShouldGenerateSameKey( ) {
		// Arrange
		var query1 = new TestQuery { Id = 123, Name = "Test", IsActive = true };
		var query2 = new TestQuery { Id = 123, Name = "Test", IsActive = true };

		// Act
		var key1 = CacheKeyGenerator.Generate( query1 );
		var key2 = CacheKeyGenerator.Generate( query2 );

		// Assert
		key1.Should( ).Be( key2 );
	}

	[Fact]
	public void Generate_WithDifferentQueryValues_ShouldGenerateDifferentKeys( ) {
		// Arrange
		var query1 = new TestQuery { Id = 123, Name = "Test1" };
		var query2 = new TestQuery { Id = 123, Name = "Test2" };

		// Act
		var key1 = CacheKeyGenerator.Generate( query1 );
		var key2 = CacheKeyGenerator.Generate( query2 );

		// Assert
		key1.Should( ).NotBe( key2 );
	}

	[Fact]
	public void Generate_ShouldIncludeTypeName( ) {
		// Arrange
		var query = new TestQuery { Id = 1 };

		// Act
		var key = CacheKeyGenerator.Generate( query );

		// Assert
		key.Should( ).StartWith( "TestQuery:" );
	}

	[Fact]
	public void Generate_ShouldIncludeHash( ) {
		// Arrange
		var query = new TestQuery { Id = 1 };

		// Act
		var key = CacheKeyGenerator.Generate( query );

		// Assert
		key.Should( ).MatchRegex( @"^TestQuery:[A-F0-9]{16}$" );
	}

	[Fact]
	public void Generate_WithNullValues_ShouldGenerateKey( ) {
		// Arrange
		var query = new TestQuery { Id = 1, Name = null };

		// Act
		var key = CacheKeyGenerator.Generate( query );

		// Assert
		key.Should( ).NotBeNullOrEmpty( );
		key.Should( ).StartWith( "TestQuery:" );
	}

	[Fact]
	public void Generate_WithComplexObject_ShouldGenerateKey( ) {
		// Arrange
		var query = new ComplexQuery {
			Id = 1,
			Filters = new List<string> { "filter1", "filter2" },
			Metadata = new Dictionary<string, object> {
				{ "key1", "value1" },
				{ "key2", 123 }
			}
		};

		// Act
		var key = CacheKeyGenerator.Generate( query );

		// Assert
		key.Should( ).NotBeNullOrEmpty( );
		key.Should( ).StartWith( "ComplexQuery:" );
	}

	[Fact]
	public void Generate_WithSameComplexObject_ShouldGenerateSameKey( ) {
		// Arrange
		var query1 = new ComplexQuery {
			Id = 1,
			Filters = new List<string> { "filter1", "filter2" }
		};
		var query2 = new ComplexQuery {
			Id = 1,
			Filters = new List<string> { "filter1", "filter2" }
		};

		// Act
		var key1 = CacheKeyGenerator.Generate( query1 );
		var key2 = CacheKeyGenerator.Generate( query2 );

		// Assert
		key1.Should( ).Be( key2 );
	}

	[Fact]
	public void Generate_WithDifferentPropertyOrder_ShouldGenerateSameKey( ) {
		// Arrange
		// JSON serialization with CamelCase policy should produce consistent output
		var query1 = new OrderTestQuery { First = "A", Second = "B" };
		var query2 = new OrderTestQuery { Second = "B", First = "A" };

		// Act
		var key1 = CacheKeyGenerator.Generate( query1 );
		var key2 = CacheKeyGenerator.Generate( query2 );

		// Assert
		// Should be same because property declaration order is consistent
		key1.Should( ).Be( key2 );
	}

	// Helper classes for testing
	private class TestQuery {
		public int Id { get; set; }
		public string? Name { get; set; }
		public bool IsActive { get; set; }
	}

	private class ComplexQuery {
		public int Id { get; set; }
		public List<string>? Filters { get; set; }
		public Dictionary<string, object>? Metadata { get; set; }
	}

	private class OrderTestQuery {
		public string? First { get; set; }
		public string? Second { get; set; }
	}
}
