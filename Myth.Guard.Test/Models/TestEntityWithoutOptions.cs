using Myth.Builder;
using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Guard.Test.Models;

/// <summary>
/// Test entity without options (control test)
/// </summary>
public class TestEntityWithoutOptions : IValidatable<TestEntityWithoutOptions> {
	public TestStatus Status { get; set; }

	public void Validate( ValidationBuilder<TestEntityWithoutOptions> builder, ValidationContextKey? context = null ) {
		builder.For( Status, r => r.IsValidEnumValue( ) ); // No WithOptions()
	}
}
