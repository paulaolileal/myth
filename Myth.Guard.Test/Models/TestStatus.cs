using Myth.ValueObjects;

namespace Myth.Guard.Test;

public partial class ConstantValidationRulesTests {
	/// <summary>
	/// Test Status constant for string values
	/// </summary>
	public class TestStatus : Constant<TestStatus, string> {
		public static readonly TestStatus Active = new( "Active", "A" );
		public static readonly TestStatus Inactive = new( "Inactive", "I" );
		public static readonly TestStatus Pending = new( "Pending", "P" );

		public TestStatus( string name, string value ) : base( name, value ) { }
	}
}
