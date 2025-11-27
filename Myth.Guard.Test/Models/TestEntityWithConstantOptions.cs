using Myth.Builder;
using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Guard.Test.Models;

/// <summary>
/// Test entity for constant options
/// </summary>
public class TestEntityWithConstantOptions : IValidatable<TestEntityWithConstantOptions> {
	public int Priority { get; set; }

	public void Validate( ValidationBuilder<TestEntityWithConstantOptions> builder, ValidationContextKey? context = null ) {
		builder.For( Priority, r => r.ExistsInConstant<TestPriorityLevels, int>( ).WithOptions( ) );
	}
}
