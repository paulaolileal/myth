using Myth.ValueObjects;

namespace Myth.Commons.Test.Models;

internal class ValueObjectMock( string test ) : ValueObject {
	public string Test { get; private set; } = test;

	protected override IEnumerable<object> GetAtomicValues( ) {
		yield return Test;
	}
}