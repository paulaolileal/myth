using Myth.ValueObjects;

namespace Myth.Guard.Test.Models;

/// <summary>
/// Test priority levels constant for testing WithOptions functionality
/// </summary>
public class TestPriorityLevels : Constant<TestPriorityLevels, int> {
	public static readonly TestPriorityLevels Low = new( nameof( Low ), 1 );
	public static readonly TestPriorityLevels Medium = new( nameof( Medium ), 3 );
	public static readonly TestPriorityLevels High = new( nameof( High ), 5 );
	public static readonly TestPriorityLevels Critical = new( nameof( Critical ), 10 );

	private TestPriorityLevels( string name, int value ) : base( name, value ) { }
}
