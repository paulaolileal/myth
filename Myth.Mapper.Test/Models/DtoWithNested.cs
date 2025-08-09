namespace Myth.Mapper.Test.Models {

	public class DtoWithNested {
		public int Id { get; set; }
		public List<NestedItem> Items { get; set; } = new( );
	}
}