using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	public class EntityWithNested : IMorphTo<DtoWithNested> {
		public int Id { get; set; }
		public List<NestedItem> Items { get; set; } = new( );

		public void Binder( BinderBuilder<DtoWithNested> builder ) {
		}
	}
}