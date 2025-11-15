using Myth.Builder;
using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Guard.Test;

public partial class ConstantValidationRulesTests {
	#region Helper Classes

	/// <summary>
	/// DTO for testing custom messages
	/// </summary>
	public class CustomMessageDto : IValidatable<CustomMessageDto> {
		public string StatusCode { get; set; } = string.Empty;

		public void Validate( ValidationBuilder<CustomMessageDto> builder, ValidationContextKey? context = null ) {
			builder.For( StatusCode, x => x
				.ExistsInConstant<TestStatus, string>( )
				.WithMessage( "Please select a valid status" )
				.WithCode( "INVALID_STATUS" ) );
		}
	}

	#endregion
}
