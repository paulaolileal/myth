using Myth.Builder;
using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Guard.Test;

public partial class ConstantValidationRulesTests {
	#region Helper Classes

	/// <summary>
	/// DTO for testing nullable values
	/// </summary>
	public class SimpleNullableDto : IValidatable<SimpleNullableDto> {
		public string? NullableStatusCode { get; set; }
		public string? NullableStatusName { get; set; }

		public void Validate( ValidationBuilder<SimpleNullableDto> builder, ValidationContextKey? context = null ) {
			builder.For( NullableStatusCode, x => x.ExistsInConstant<TestStatus, string>( ) );
			builder.For( NullableStatusName, x => x.NameExistsInConstant<TestStatus, string>( ) );
		}
	}

	#endregion
}
