using Myth.ValueObjects;

namespace Myth.Guard.Test;

public partial class ConstantValidationRulesTests {
	/// <summary>
	/// Test Status constant for string values
	/// </summary>
	public class TestStatus( string name, string value ) : Constant<TestStatus, string>( name, value ) {
		public static readonly TestStatus Active = new( name: "Active", value: "A" );
		public static readonly TestStatus Inactive = new( name: "Inactive", value: "I" );
		public static readonly TestStatus Pending = new( name: "Pending", value: "P" );
	}
}
