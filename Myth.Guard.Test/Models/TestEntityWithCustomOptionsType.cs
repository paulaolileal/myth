using Myth.Builder;
using Myth.Enums;
using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Guard.Test.Models;

/// <summary>
/// Test entity for WithOptions with custom OptionsType
/// </summary>
public class TestEntityWithCustomOptionsType : IValidatable<TestEntityWithCustomOptionsType> {
	public TestStatus Status { get; set; }
	public OptionsType OptionsType { get; set; }

	public void Validate( ValidationBuilder<TestEntityWithCustomOptionsType> builder, ValidationContextKey? context = null ) {
		builder.For( Status, r => r.IsValidEnumValue( ).WithOptions( OptionsType ) );
	}
}
