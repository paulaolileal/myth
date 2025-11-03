namespace Myth.Morph.Test.Models.Dtos {

	public class DtoWithNested {
		public int Id { get; set; }
		public List<NestedItem> Items { get; set; } = [ ];
	}
}