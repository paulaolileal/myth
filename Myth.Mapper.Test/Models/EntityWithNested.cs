using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	public class EntityWithNested : IMorphable<DtoWithNested> {
		public int Id { get; set; }
		public List<NestedItem> Items { get; set; } = new( );

		public void MorphTo( Schema<DtoWithNested> builder ) {
		}
	}
}