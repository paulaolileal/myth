using Myth.ValueObjects;

namespace Myth.Commons.Test.Models {

	internal class ValueObjectMock : ValueObject {
		public string Test { get; private set; }

		public ValueObjectMock( string test ) {
			Test = test;
		}

		protected override IEnumerable<object> GetAtomicValues( ) {
			yield return Test;
		}
	}
}