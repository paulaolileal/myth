namespace Myth.Commons.Test.Models {

	internal class TestModel : ITestModel {
		public int Id { get; set; }
		public string Name { get; set; } = null!;

		public string Test( ) => $"{Id} - {Name}";
	}
}