using Myth.Builder;
using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Guard.Test.Models;

/// <summary>
/// Test entity for WithOptions functionality
/// </summary>
public class TestEntityWithOptions : IValidatable<TestEntityWithOptions> {
	public TestStatus Status { get; set; }

	public void Validate( ValidationBuilder<TestEntityWithOptions> builder, ValidationContextKey? context = null ) {
		builder.For( Status, r => r.IsValidEnumValue( ).WithOptions( ) );
	}
}
