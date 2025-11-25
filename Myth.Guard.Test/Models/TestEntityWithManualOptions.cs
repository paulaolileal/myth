using Myth.Builder;
using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Guard.Test.Models;

/// <summary>
/// Test entity for manual options
/// </summary>
public class TestEntityWithManualOptions : IValidatable<TestEntityWithManualOptions> {
	public string Category { get; set; } = string.Empty;

	public void Validate( ValidationBuilder<TestEntityWithManualOptions> builder, ValidationContextKey? context = null ) {
		builder.For( Category, r => r
			.BeOneOf( "electronics", "clothing", "books", "home" )
			.WithOptions( "electronics: Electronics", "clothing: Clothing", "books: Books", "home: Home & Garden" ) );
	}
}
