using Myth.Builder;
using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Guard.Test;

public partial class ConstantValidationRulesTests {
	/// <summary>
	/// Test DTO for validation
	/// </summary>
	public class TestDto : IValidatable<TestDto> {
		public string StatusCode { get; set; } = string.Empty;
		public string StatusName { get; set; } = string.Empty;
		public int PriorityLevel { get; set; }
		public string PriorityName { get; set; } = string.Empty;

		public void Validate( ValidationBuilder<TestDto> builder, ValidationContextKey? context = null ) {
			// Validate status code exists in TestStatus constants
			builder.For( StatusCode, x => x
				.NotEmpty( )
				.ExistsInConstant<TestStatus, string>( ) );

			// Validate status name exists in TestStatus constants
			builder.For( StatusName, x => x
				.NotEmpty( )
				.NameExistsInConstant<TestStatus, string>( ) );

			// Validate priority level exists in TestPriority constants
			builder.For( PriorityLevel, x => x
				.ExistsInConstant<TestPriority, int>( ) );

			// Validate priority name exists in TestPriority constants
			builder.For( PriorityName, x => x
				.NotEmpty( )
				.NameExistsInConstant<TestPriority, int>( ) );
		}
	}

}
