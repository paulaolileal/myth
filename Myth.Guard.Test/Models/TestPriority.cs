using Myth.ValueObjects;

namespace Myth.Guard.Test;

public partial class ConstantValidationRulesTests {
	/// <summary>
	/// Test Priority constant for integer values
	/// </summary>
	public class TestPriority( string name, int value ) : Constant<TestPriority, int>( name, value ) {
		public static readonly TestPriority Low = new( "Low", 1 );
		public static readonly TestPriority Medium = new( "Medium", 5 );
		public static readonly TestPriority High = new( "High", 10 );
	}
}
