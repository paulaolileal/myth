using Myth.ValueObjects;

namespace Myth.Guard.Test;

public partial class ConstantValidationRulesTests {
	/// <summary>
	/// Test Priority constant for integer values
	/// </summary>
	public class TestPriority : Constant<TestPriority, int> {
		public static readonly TestPriority Low = new( "Low", 1 );
		public static readonly TestPriority Medium = new( "Medium", 5 );
		public static readonly TestPriority High = new( "High", 10 );

		public TestPriority( string name, int value ) : base( name, value ) { }
	}
}
