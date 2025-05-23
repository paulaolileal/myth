namespace Myth.Commons.Test.Models {

	internal interface ITestModel {
		int Id { get; set; }
		string Name { get; set; }

		string Test( );
	}
}