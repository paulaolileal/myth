using Myth.Builder;
using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Guard.Test.Models;

/// <summary>
/// Test model for enum validation testing
/// </summary>
public class EnumTestEntity : IValidatable<EnumTestEntity> {
	public UserRole Role { get; set; }
	public TestPriority Priority { get; set; }
	public TestStatus? NullableStatus { get; set; }

	public void Validate( ValidationBuilder<EnumTestEntity> builder, ValidationContextKey? context = null ) {
		// Test the new IsValidEnumValue method
		builder.For( Role, x => x.IsValidEnumValue( ) );

		// Test existing BeInEnum method for comparison
		builder.For( Priority, x => x.BeInEnum( ) );

		// Test the new IsValidNullableEnumValue method
		builder.For( NullableStatus, x => x.IsValidNullableEnumValue( ) );
	}
}

/// <summary>
/// Test enum for priority levels
/// </summary>
public enum TestPriority {
	Low = 1,
	Medium = 2,
	High = 3,
	Critical = 4
}

/// <summary>
/// Test enum for status values
/// </summary>
public enum TestStatus {
	Active = 10,
	Inactive = 20,
	Pending = 30,
	Suspended = 40
}